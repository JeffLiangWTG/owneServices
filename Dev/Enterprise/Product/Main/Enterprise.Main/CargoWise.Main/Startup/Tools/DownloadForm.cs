using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup.Tools
{
	public partial class DownloadForm : ZArchitecture.GUI.ZChildForm
	{
		public DownloadForm(Uri sourceUrl)
		{
			this.sourceUrl = sourceUrl;
		}

		public string DownloadedFilePath { get; private set; }

		protected override void OnShown(EventArgs e)
		{
			StartDownloading();
		}

		void StartDownloading()
		{
			if (webClient == null)
			{
#pragma warning disable SYSLIB0014 // WebClient.WebClient()' is obsolete: 'WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.'
				webClient = new WebClient();
#pragma warning restore SYSLIB0014
				webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
				webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
			}

			var fileName = Path.GetFileName(sourceUrl.LocalPath);
			if (string.IsNullOrEmpty(fileName))
			{
				fileName = Path.GetRandomFileName();
			}

			var filePath = Path.Combine(Environment.Env.TempPath, fileName);
			webClient.DownloadFileAsync(sourceUrl, filePath, filePath);
		}

		void WebClient_DownloadProgressChanged(object sender, ProgressChangedEventArgs eventArgs)
		{
			progressBar.Value = eventArgs.ProgressPercentage;
		}

		void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs eventArgs)
		{
			if (eventArgs.Error != null && !eventArgs.Cancelled)
			{
				Globals.Message.ShowError(string.Concat(
					Res.GetString("89608b62-bc34-4350-a22c-61fcbb6f49aa", "Error downloading the package."),
					System.Environment.NewLine,
					eventArgs.Error.Message));

				Close(DialogResult.Abort);
				return;
			}

			if (eventArgs.Cancelled)
			{
				Close(DialogResult.Cancel);
				return;
			}

			progressBar.Value = 100;
			DownloadedFilePath = (string)eventArgs.UserState;
			Close(DialogResult.OK);
		}

		void CancelDownload_Click(object sender, EventArgs e)
		{
			webClient?.CancelAsync();
			Close(DialogResult.Cancel);
		}

		void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if (webClient != null && webClient.IsBusy)
			{
				webClient.CancelAsync();
			}
		}

		void Close(DialogResult dialogResult)
		{
			DialogResult = dialogResult;
			Close();
		}

		WebClient webClient;
		readonly Uri sourceUrl;
	}
}
