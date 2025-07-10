using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.ServiceTask
{
	class IssueManagerEmailDownloader
	{
		public IssueManagerEmailDownloader(ILogger logger, string errorLogDirectory)
		{
			ServiceLogger = logger;
			this.errorLogDirectory = errorLogDirectory;
		}

		readonly ILogger ServiceLogger;
		readonly string errorLogDirectory;

		public void RunTask()
		{
			var mailBoxSettings = EDIDataRegistry.Instance.IssueReportMailBox;
			var configuration = new MailServerConfiguration(mailBoxSettings);
			using var downLoader = new MailDownloader(configuration);

			ZString userName = mailBoxSettings.UserName;

			if (!userName.IsEmpty)
			{
				var mailRetriever = new MailSaverBase(downLoader);
				mailRetriever.PersistingEmail += mailRetriever_PersistingEmail;

				downLoader.LogMessage += downLoader_LogMessage;
				downLoader.DownloaderClosing += downLoader_Pop3Closing;
				try
				{
					mailRetriever.Retrieve();
				}
				finally
				{
					downLoader.LogMessage -= downLoader_LogMessage;
					downLoader.DownloaderClosing -= downLoader_Pop3Closing;
					mailRetriever.PersistingEmail -= mailRetriever_PersistingEmail;
				}
			}
			else
			{
				ServiceLogger.Log(LogType.Error, "Issue Report Mailbox not set in Registry");
			}
		}

		void mailRetriever_PersistingEmail(string uniqueId, string email, ref bool continueDownloading)
		{
			var message = MimeMessageExtensions.CreateMessageFromEml(email);
			var attachments = message.GetFullAttachments();
			foreach (var attachment in attachments)
			{
				ProcessAttachment(attachment.GetName(), attachment.GetData());
			}
		}

		void downLoader_Pop3Closing(long messageCount)
		{
			ServiceLogger.Log(LogType.Information, messageCount.ToString() + " issue manager emails downloaded");
		}

		void downLoader_LogMessage(TraceEventType eventType, string message)
		{
			ServiceLogger.Log(GetLogTypeFromTraceEventType(eventType), message);
		}

		void ProcessAttachment(string attachmentFilename, byte[] attachmentData)
		{
			if (!string.IsNullOrEmpty(attachmentFilename))
			{
				string extension = Path.GetExtension(attachmentFilename);

				if (extension.EndsWith("zip", StringComparison.OrdinalIgnoreCase))
				{
					StringBuilder sb = new StringBuilder();
					new AttachmentTextAppender().Append(attachmentFilename, attachmentData, sb);
					string data = sb.ToString();
					Process(data);
				}
				else
				{
					string data = Encoding.ASCII.GetString(attachmentData);
					Process(data);
				}
			}
		}

		void Process(string xmlData)
		{
			if (xmlData != null)
			{
				ZGuid guid = ZGuid.NewZGuid();
				string fileName = Path.Combine(errorLogDirectory, guid.ToString() + ".tmp");
				File.WriteAllText(fileName, xmlData);
				File.Move(fileName, Path.ChangeExtension(fileName, ".xml"));
			}
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
