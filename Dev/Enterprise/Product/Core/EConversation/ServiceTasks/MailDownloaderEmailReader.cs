using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Enterprise.Integration;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;

namespace Enterprise.EConversation.ServiceTasks
{
	public sealed class MailDownloaderEmailReader : IEmailReader
	{
		public event IEmailReader.EmailDownloadedDelegate EmailDownloaded;

		readonly IMailDownloader downloader;
		readonly ILogger logger;

		public MailDownloaderEmailReader(string server, string mailbox, IMailDownloader downloader, ILogger logger)
		{
			Server = server;
			Mailbox = mailbox;

			this.logger = logger;
			this.downloader = downloader;
			this.downloader.LogMessage += Downloader_LogMessage;
			this.downloader.EmailDownloaded += Downloader_EmailDownloaded;
		}

		public string Server { get; }

		public string Mailbox { get; }

		public void DeleteProcessedMails(IEnumerable<string> processedIds)
		{
			foreach (var id in processedIds)
			{
				downloader.DeleteMessage(id);
			}
		}

		public Email GetRawEmailFromString(string emailText)
		{
			return new Email(Encoding.UTF8.GetBytes(emailText));
		}

		public void Start()
		{
			downloader.DownloadFromServer();
		}

		public void Dispose()
		{
			downloader.LogMessage -= Downloader_LogMessage;
			downloader.EmailDownloaded -= Downloader_EmailDownloaded;
			downloader.Dispose();
		}

		void Downloader_LogMessage(TraceEventType eventType, string message)
		{
			logger.Log(GetLogTypeFromTraceEventType(eventType), message);
		}

		void Downloader_EmailDownloaded(string uniqueId, ref string email, ref bool continueDownloading)
		{
			EmailDownloaded?.Invoke(uniqueId, email, ref continueDownloading);
		}

		static LogType GetLogTypeFromTraceEventType(TraceEventType eventType)
		{
			LogType logType;
			switch (eventType)
			{
				case TraceEventType.Critical:
				case TraceEventType.Error:
					logType = LogType.Error;
					break;

				case TraceEventType.Warning:
					logType = LogType.Warning;
					break;

				case TraceEventType.Verbose:
					logType = LogType.Debug;
					break;

				default:
					logType = LogType.Information;
					break;
			}

			return logType;
		}
	}
}
