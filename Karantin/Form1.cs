using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using HtmlAgilityPack;

namespace Karantin
{
    public partial class Form1 : Form
    {


        private BackgroundWorker backgroundWorker1;
        private Button downloadButton;
        private string htmlCode;
        private string time;
        private string date;
        private string price;
        private bool found;
        private bool findFreeTime;
        private System.Timers.Timer timer;


        public Form1()
        {
            InitializeComponent();

            price = textBox2.Text + ".00";
    

            // Instantiate BackgroundWorker and attach handlers to its
            // DowWork and RunWorkerCompleted events.
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            // Start the download operation in the background.
            this.backgroundWorker1.RunWorkerAsync();

            // Disable the button for the duration of the download.
            //  this.downloadButton.Enabled = false;

            // Once you have started the background thread you 
            // can exit the handler and the application will 
            // wait until the RunWorkerCompleted event is raised.

            // Or if you want to do something else in the main thread,
            // such as update a progress bar, you can do so in a loop 
            // while checking IsBusy to see if the background task is
            // still running.

            while (this.backgroundWorker1.IsBusy)
            {
                //  progressBar1.Increment(1);
                // Keep UI messages moving, so the form remains 
                // responsive during the asynchronous operation.
                Application.DoEvents();
            }

            SetTextSafe("Last scan: " + DateTime.Now
                + " - Date: " + (string.IsNullOrEmpty(date) ? "none" : date).ToString()
                + " - Time: " + (string.IsNullOrEmpty(time) ? "none" : time).ToString());
        }


        private void SetTextSafe(string text)
        {
            if (InvokeRequired)
                this.BeginInvoke(new Action<string>((s) =>
                {
                    SetText(s);
                }), text);
            else SetText(text);
        }

        private void SetText(string text)
        {
            this.textBox1.Text = text;
            if (found)
            {
                timer.Enabled = false;
                MessageBox.Show(this.textBox1.Text, "МЫ НАШЛИ!",
MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
         
     
        }


        private void downloadButton_Click(object sender, EventArgs e)
        {

            // Get fin type
            findFreeTime = this.checkBox1.Checked;

            // Create a timer 
            timer = new System.Timers.Timer(int.Parse(this.textBox3.Text) * 1000);

            // Hook up the Elapsed event for the timer.
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            timer.Enabled = true;


        }

        private void backgroundWorker1_DoWork(
            object sender,
            DoWorkEventArgs e)
        {
            using (var client = new WebClient())
            {
                client.Encoding = UTF8Encoding.UTF8;
                htmlCode = client.DownloadString("http://karantinum.ru/quests/yantar/");

                HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();

                document.LoadHtml(htmlCode);
                HtmlNodeCollection collection = document.DocumentNode.SelectNodes("//div");
                if (findFreeTime)
                {
                    var findclasses = document.DocumentNode.Descendants("div")
                             .Where(d =>
             d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("booking_hour") && d.Attributes["class"].Value != "booking_hour busy"
             && d.Attributes.Contains("price") && d.Attributes["price"].Value.Contains(price)
         );
                    if (findclasses.Count() > 0)
                    {
                        time = findclasses.FirstOrDefault().InnerText;

                        var dateDiv = findclasses.FirstOrDefault().ParentNode.ParentNode.Descendants("div")
                            .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("booking_date_f"));

                        byte[] bytes = Encoding.Convert(Encoding.GetEncoding("windows-1251"), Encoding.UTF8, Encoding.GetEncoding("windows-1251").GetBytes(dateDiv.FirstOrDefault().InnerText));

                        date = Encoding.UTF8.GetString(bytes).Trim();
                        found = true;
                    }
                    else
                    {
                        date = null;
                        time = null;
                        found = false;
                    } 
                }
                else
                {
                    var findclasses = document.DocumentNode.Descendants("div")
                             .Where(d =>
             d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("booking_hour") 
             && d.Attributes.Contains("price") && d.Attributes["price"].Value.Contains(price)
         );
                    if (findclasses.FirstOrDefault().InnerText != null)
                    {
                        time = findclasses.FirstOrDefault().InnerText;

                        var dateDiv = findclasses.FirstOrDefault().ParentNode.ParentNode.Descendants("div")
                            .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("booking_date_f"));

                        byte[] bytes = Encoding.Convert(Encoding.GetEncoding("windows-1251"), Encoding.UTF8, Encoding.GetEncoding("windows-1251").GetBytes(dateDiv.FirstOrDefault().InnerText));

                        date = Encoding.UTF8.GetString(bytes).Trim();
                        found = true;
                    }
                    else
                    {
                        date = null;
                        time = null;
                        found = false;
                    }
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            // Set progress bar to 100% in case it's not already there.
            //   progressBar1.Value = 100;

            if (e.Error == null)
            {

            }
            else
            {
                MessageBox.Show(
                    "Failed to download file",
                    "Download failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                timer.Enabled = false;
            }

            // Enable the download button and reset the progress bar.
            this.downloadButton.Enabled = true;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
        }
    }
}
