using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Xabe.FFmpeg;
using System.IO;

namespace YeetTonTuyeau
{
    public partial class YeetTonTuyeau : Form
    {
        public int pceTimeValue = 0;
        public int fe = 0;
        public YeetTonTuyeau()
        {
            InitializeComponent();
            memeImg.Visible = false;
            stopBtn.Enabled = false;

            ffmpegCheck();
            ffmpegDCheck();
            ffmpegCheckTmr.Start();

            if (fe == 4)
            {
                string message = "ffmpeg est manquant, voulez-vous le télécharger ?";
                string caption = "sérieux?";

                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult resultat;

                resultat = MessageBox.Show(message, caption, buttons);

                if (resultat == System.Windows.Forms.DialogResult.Yes)
                {
                    FFmpeg.GetLatestVersion();
                }

            }
            else if (fe == 2 || fe == 3)
            {
                string message = "ffmpeg ou ffprobe est manquant, voulez-vous le télécharger ?";
                string caption = "sérieux?";

                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult resultat;

                resultat = MessageBox.Show(message, caption, buttons);

                if (resultat == System.Windows.Forms.DialogResult.Yes)
                {
                    FFmpeg.GetLatestVersion();
                }
            }
        }

        public string AddQuotesIfRequired(string path)
        {
            return !string.IsNullOrWhiteSpace(path) ?
                path.Contains(" ") && (!path.StartsWith("\"") && !path.EndsWith("\"")) ?
                    "\"" + path + "\"" : path :
                    string.Empty;
        }

        private void YeetTonTuyeau_FormClosing(Object sender, FormClosingEventArgs e)
        {
            foreach (var process in Process.GetProcessesByName("ffmpeg"))
            {
                string message = "la conversion est toujours en cours";
                string caption = "sérieux?";

                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult resultat;


                resultat = MessageBox.Show(message, caption, buttons);

                if (resultat == System.Windows.Forms.DialogResult.Yes)
                {
                    TueLeBebe();

                }
                else
                {
                    e.Cancel = true;
                }

            }
        }

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Tout les fichiers (*.*)|*.*|Fichier podcast (*.mp3)|*.mp3";
            dlg.FilterIndex = 2;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                mp3txt.Text = dlg.FileName;

            }
        }

        private void ImageBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Tout les fichiers (*.*)|*.*|Fichier images (*.png;*.bmp;*.png;*.jpg)|*.png;*.bmp;*.png;*.jpg";
            dlg.FilterIndex = 2;
            dlg.RestoreDirectory = true;


            if (dlg.ShowDialog() == DialogResult.OK)
            {
                imgtxt.Text = dlg.FileName;
            }
        }

        private void OutputBtn_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                outputtxt.Text = fbd.SelectedPath;
            }
        }

        private void ConvertBtn_Click(object sender, EventArgs e)
        {

            string filename = Path.GetFileNameWithoutExtension(mp3txt.Text);
            string message = "un fichier du même nom est existant, voulez vous le yeet?";
            string caption = "fichier existant";

            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult resultat;

            if (File.Exists(outputtxt.Text + "\\" + filename + "_(video).flv"))
            {
                resultat = MessageBox.Show(message, caption, buttons);

                if (resultat == System.Windows.Forms.DialogResult.Yes)
                {
                    File.Delete(outputtxt.Text + "\\" + filename + "_(video).flv");
                    convertProcess();
                }
                
            }
            else
            {
                convertProcess();
            }

        }

        private void convertProcess()
        {
            char comm = '"';
            string filename = Path.GetFileNameWithoutExtension(mp3txt.Text);
            convertBtn.Enabled = false;
            stopBtn.Enabled = true;
            Conversion convert = new Conversion();
            convert.AddParameter(@"-r 1 -loop 1 -i " + comm + imgtxt.Text + comm + " -i " + comm + mp3txt.Text + comm + " -acodec copy -r 1 -shortest -vf scale=1280:-1 " + comm + outputtxt.Text + "\\" + filename + "_(video).flv" + comm);
            convert.OnProgress += Convert_OnProgress;
            convert.Start();

        }

        private void TueLeBebe()
        {
            foreach (var process in Process.GetProcessesByName("ffmpeg"))
            {
                process.Kill();
            }
        }


        private void Convert_OnProgress(object sender, Xabe.FFmpeg.Events.ConversionProgressEventArgs args)
        {
            var percent = (int)(Math.Round(args.Duration.TotalSeconds / args.TotalLength.TotalSeconds, 2) * 100);
            if (percent > 100)
            {
                percent = 100;
                this.Invoke(new Action(() => progressBar1.Value = percent));
            }
            else
                this.Invoke(new Action(() => progressBar1.Value = percent));

                this.Invoke(new Action(() => infoProgress.Text = "[" + args.Duration + " / " + args.TotalLength + "] " + percent + " %"));

            if (percent == 100)
            {
                string message = "conversion terminé avec succès";
                string caption = "letttsss go!";

                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult resultat;

                resultat = MessageBox.Show(message, caption, buttons);

                this.Invoke(new Action(() => progressBar1.Value = percent));
                this.Invoke(new Action(() => convertBtn.Enabled = true));
                this.Invoke(new Action(() => stopBtn.Enabled = false));

                Process.Start(@outputtxt.Text);
            }

        }

        private void ffmpegCheck()
        {
            if (File.Exists("ffmpeg.exe") && File.Exists("ffprobe.exe"))
            {
                fe = 1;
            }
            else if (File.Exists("ffmpeg.exe"))
            {
                fe = 2;
            }
            else if (File.Exists("ffprobe.exe"))
            {
                fe = 3;
            }
            else
                fe = 4;
        }


        private void ffmpegDCheck()
        {
            switch (fe)
            {
                case 1:
                    ffm_box.Checked = true;
                    ffp_box.Checked = true;
                    ffmpeg_label.ForeColor = Color.Green;
                    ffprobe_label.ForeColor = Color.Green;
                    break;
                case 2:
                    ffm_box.Checked = true;
                    ffp_box.Checked = false;
                    ffmpeg_label.ForeColor = Color.Green;
                    ffprobe_label.ForeColor = Color.Red;
                    break;
                case 3:
                    ffm_box.Checked = false;
                    ffp_box.Checked = true;
                    ffmpeg_label.ForeColor = Color.Red;
                    ffprobe_label.ForeColor = Color.Green;
                    break;
                case 4:
                    ffm_box.Checked = false;
                    ffp_box.Checked = false;
                    ffmpeg_label.ForeColor = Color.Red;
                    ffprobe_label.ForeColor = Color.Red;
                    break;
            }
        }

        private void iightImma()
        {
            pceTimer.Start();
            memeImg.Visible = true;
            TueLeBebe();
        }

        private void StopBtn_Click(object sender, EventArgs e)
        {

            TueLeBebe();
            progressBar1.Value = 0;
            infoProgress.Text = "conversion yeet the fuck out";
            convertBtn.Enabled = true;
            stopBtn.Enabled = false;

        }

        private void QuitApp_Click(object sender, EventArgs e)
        {
            int i = 0;

            foreach (var process in Process.GetProcessesByName("ffmpeg"))
            {
            i++;
            }

            if (i==1)

            {
                string message = "la conversion est toujours en cours";
                string caption = "sérieux?";

                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult resultat;


                resultat = MessageBox.Show(message, caption, buttons);

                if (resultat == System.Windows.Forms.DialogResult.Yes)
                {
                    iightImma();
                 }
               
            }
            else
            {
                iightImma();
            }
        }

        private void PceTimer_Tick(object sender, EventArgs e)
        {
            if (pceTimeValue == 10)
            {
                this.Close();
            }
            else
            {
                pceTimeValue++;
            }
        }
        private void PictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.facebook.com/Le-journal-dun-podcast-570941473324268/");
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutApp about = new aboutApp();
            about.Show();
        }

        private void Mp3txt_TextChanged(object sender, EventArgs e)
        {
            infoProgress.Text = string.Empty;
            progressBar1.Value = 0;
            string filename = Path.GetFileNameWithoutExtension(mp3txt.Text);
            infoProgress.Text = filename;
        }

        private void FfmpegCheckTmr_Tick(object sender, EventArgs e)
        {
            ffmpegCheck();
            ffmpegDCheck();
        }
    }


}
