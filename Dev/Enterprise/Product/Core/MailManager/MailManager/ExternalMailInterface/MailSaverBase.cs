using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Res = MailManager.Res;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public class MailSaverBase
	{
		public MailSaverBase(IMailDownloader downloader)
		{
			this.downloader = downloader;
			this.downloader.EmailDownloaded += Downloader_EmailDownloaded;
			this.downloader.DownloaderClosing += delegate(long messageCount)
			{
				if (downloadedIndexes.Count > 0)
				{
					DoEndOfDownload();
				}
			};
		}

		public event PersistEmailHandler PersistingEmail;
		public event EndOfDownloadHandler EndOfDownload;
		internal const int DownloadSizeThreshold = 400 * 1000; // Seems to be about the right size based on performance testing

		public void Retrieve()
		{
			downloader.DownloadFromServer();
		}

		void Downloader_EmailDownloaded(string uniqueId, ref string email, ref bool continueDownloading)
		{
			downloadedIndexes.Add(uniqueId);
			bytesDownloaded += email.Length;

			try
			{
				PersistingEmail?.Invoke(uniqueId, email, ref continueDownloading);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ex is InvalidMailFormatException)
				{
					SendErrorReport(email, ex);
					return;
				}

				if (ex.Source.StartsWith(nameof(MimeKit)) || ex is FormatException || ex is ArgumentException)
				{
					SendErrorReport(email, ex);
				}
				else
				{
					downloadedIndexes.Remove(uniqueId);
				}

				throw;
			}

			if (bytesDownloaded > DownloadSizeThreshold)
			{
				DoEndOfDownload();
			}
		}

		void DoEndOfDownload()
		{
			try
			{
				EndOfDownload?.Invoke();
				downloadedIndexes.ForEach(index => DeleteMessage(index));
			}
			finally
			{
				bytesDownloaded = 0;
				downloadedIndexes.Clear();
			}
		}

		void DeleteMessage(string index)
		{
			try
			{
				downloader.DeleteMessage(index);
			}
			catch (NullReferenceException ex)
			{
				ErrorReporter.ReportOnce("Could not delete message from MailSaverBase", $"Could not delete message with index {index} : {ex.Message}");
			}
		}

		void SendErrorReport(string email, Exception ex)
		{
			var notificationSubject = Res.GetString("ea7b98af-a728-4e86-8252-d6c5ac913aa3", "Error while processing an incoming email addressed to {0}", Env.Registry.MailboxEmailAddress);
			var notificationBody = Res.GetString("f1fd8c91-c83f-4961-bb00-33d54afa77a3", "At {0} Email batch processor reported an error while processing an incoming email.\r\n\r\nPOP3 Server {1} Port: {2} Mailbox: {3}\r\n\r\nError Message: {4}\r\n\r\nPlease, see the raw email message text below.\r\n\r\n{5}",
					ZDateTime.Now, Env.Registry.MailServer, Env.Registry.MailServerPort, Env.Registry.MailboxEmailAddress, ex.Message, email);
			Env.OutgoingMailManager.CreateAndSaveToCompanyNotificationGroup(notificationSubject, notificationBody);
		}

		protected readonly IMailDownloader downloader;
		int bytesDownloaded;
		readonly List<string> downloadedIndexes = new List<string>();
	}
}
