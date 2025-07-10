using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.EConversation.ServiceTasks
{
	public abstract class EmailProcessorServiceProvider : ServiceProviderImpl, IEmailProcessorLogger
	{
		public const string MinimumPeriod = "30seconds";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		public override void RunTask(CancellationToken token)
		{
			// The registry item is not cached, so we're assigning this to a field for each run for performance
			SetIsVerboseModeFromRegistry();

			var processedIds = new List<string>();
			try
			{
				if (!HasValidMailboxSettings())
				{
					return;
				}

				using var emailReader = GetNewEmailReader();
				Log(LogType.Information, false, "Attempting to connect to Server:{0} | Mailbox:{1}", emailReader.Server, emailReader.Mailbox);

				ReadAndProcessEmail(emailReader, processedIds, token);
				Log(LogType.Information, false, "Processed {0} emails", processedIds.Count);

				emailReader.DeleteProcessedMails(processedIds);
				Log(LogType.Information, false, "{0} emails read, processed and deleted", processedIds.Count);

				processedIds.Clear();
			}
			catch (Exception ex) when (ex.Source.StartsWith(nameof(MailKit)) || ex.GetBaseException() is SocketException)
			{
				var socketEx = ex.GetBaseException() as SocketException;
				const int WSAETIMEDOUT = 10060; // connection timed out - see Windows Sockets Error Codes
				const int WSAECONNABORTED = 10053; // connection lost
				if (socketEx is { ErrorCode: WSAETIMEDOUT })
				{
					// Treat timeouts as just a warning. We don't want the service to decide
					// it is faulty just because the mail server is not responding.
					Log(LogType.Warning, false, "Connection timed out");
				}
				else if (socketEx is { ErrorCode: WSAECONNABORTED })
				{
					// Mail server is either blocking the defined SMTP port, or is not configured properly
					// Or a firewall is stopping the connection.
					Log(LogType.Warning, false, "Connection lost");
				}
				else
				{
					Log(LogType.Error, ex.Message, ex);
				}
			}
			catch (MailInterfaceException ex)
			{
				Log(LogType.Error, ex.Message, ex);
			}
			finally
			{
				if (processedIds.Count > 0)
				{
					try
					{
						using var emailReader = GetNewEmailReader();
						emailReader.DeleteProcessedMails(processedIds);
						Log(LogType.Information, false, "{0} emails read, processed and deleted", processedIds.Count);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce("Unhandled Exception while deleting processed emails", ex.Message, ex);
					}
				}
			}
		}

#if DEBUG
		public Email processingErrorSimulationEmail;
		public Exception processingErrorSimulationException;
#endif
		protected abstract Guid RecipientGroupPk { get; }

		protected abstract IGroupSourceLocator GroupLocator { get; }

		protected abstract bool HasValidMailboxSettings();

		protected abstract IEmailReader GetNewEmailReader();

		protected abstract bool ProcessEmailCore(Email email);

		protected void Log(string message, Exception ex)
		{
			ServiceLogger.Log(LogType.Error, message, ex);
		}

		protected void Log(LogType logType, string format, params object[] args)
		{
			Log(logType, false, format, args);
		}

		protected void Log(LogType logType, bool verboseModeOnly, string format, params object[] args)
		{
			if (!verboseModeOnly || isVerboseMode)
			{
				ServiceLogger.Log(logType, string.Format(format, args));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		void ReadAndProcessEmail(IEmailReader emailReader, ICollection<string> processedIds, CancellationToken token)
		{
			try
			{
				emailReader.EmailDownloaded += EmailDownloaded;
				emailReader.Start();
			}
			finally
			{
				emailReader.EmailDownloaded -= EmailDownloaded;
			}

			return;

			void EmailDownloaded(string id, string emailText, ref bool continueDownloading)
			{
				if (string.IsNullOrEmpty(emailText))
				{
					return;
				}

				Email email = null;

				try
				{
					email = emailReader.GetRawEmailFromString(emailText);
					Log(
						LogType.Information,
						false,
						"Email:{0} Attachments:{1} From:{2} Subject:{3}",
						id, email.NonVisualCount + email.VisualCount, email.SenderNameAddress, email.Subject);
				}
				catch (Exception ex) when (ex.Source.StartsWith(nameof(MailKit)))
				{
					throw;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce(GetType().Name + ".ProcessEmail." + ex.GetType().Name, ex.Message, ex);

					var message = string.Format(
						CultureInfo.InvariantCulture,
						"Exception processing email from {0}: {1}",
						!string.IsNullOrWhiteSpace(email?.From) ? email.From : "NULL",
						ex.Message);
					Log(message, ex);

					ForwardUnprocessedEmail(ex, Encoding.UTF8.GetBytes(emailText));

					Log(LogType.Information, true, "Marking email no. {0} as processed", id);
					processedIds.Add(id);
					return;
				}

				var processedSuccessfully = ProcessEmail(email);
				if (processedSuccessfully || email.IsEmailProcessingSkipped)
				{
					if (email.IsEmailProcessingSkipped)
					{
						Log(LogType.Information, false,
							"Out-of-office Email found:{0} Attachments:{1} From:{2} Subject:{3} and would be deleted",
							id, email.NonVisualCount + email.VisualCount, email.SenderNameAddress, email.Subject);
					}

					Log(LogType.Information, true, "Marking email no. {0} as processed", id);
					processedIds.Add(id);
				}

				if (token.IsCancellationRequested)
				{
					continueDownloading = false;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		bool ProcessEmail(Email email)
		{
			try
			{
#if DEBUG
				if (processingErrorSimulationEmail != null)
				{
					email = processingErrorSimulationEmail;
					throw processingErrorSimulationException;
				}
#endif
				return ProcessEmailCore(email);
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				var key = GetType().Name + ".ProcessEmail." + ex.GetType().Name;
				var message = $"Exception processing email from {(!string.IsNullOrWhiteSpace(email?.From) ? email.From : "NULL")}: {ex.Message}";
				Log(message, ex);
				HandleException(email, key, ex);

				return true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		protected void ForwardUnprocessedEmail(Email badEmail, Exception ex, string subjectPostfix)
		{
			var mail = new EmailDef();
			if (ex != null)
			{
				mail.Subject = this.GetType().Name + ' ' + subjectPostfix;
				mail.Body = ex.ToString();
			}
			else
			{
				mail.Subject = this.GetType().Name + ' ' + subjectPostfix;
				mail.Body = "From: " + badEmail.SenderNameAddress + "\r\nSubject:" + badEmail.Subject;
			}

			mail.Attachments.Add(new AttachmentDef("Unprocessed.eml", Encoding.UTF8.GetBytes(badEmail.GetEml())));

			try
			{
				var attachmentText = badEmail.GetFirstAttachmentOrVisualText();
				if (!string.IsNullOrEmpty(attachmentText))
				{
					mail.Attachments.Add(new AttachmentDef("Decoded.txt", Encoding.UTF8.GetBytes(attachmentText)));
				}
			}
			catch (Exception ex1) when (!ex1.IsCriticalException())
			{
			}

			try
			{
				Env.OutgoingMailManager.CreateAndSave(mail, RecipientGroupPk, GroupLocator);
			}
			catch (EmailHasNoRecipientsException)
			{
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		protected void ForwardUnprocessedEmail(Exception ex, byte[] rawEmail)
		{
			var mail = new EmailDef { Subject = GetType().Name + " error " };
			if (ex != null)
			{
				mail.Body = ex.ToString();
			}
			else
			{
				mail.Body = "Subject:" + mail.Subject;
			}

			mail.Attachments.Add(new AttachmentDef("Unprocessed.eml", rawEmail));

			try
			{
				Env.OutgoingMailManager.CreateAndSave(mail, RecipientGroupPk, GroupLocator);
			}
			catch (EmailHasNoRecipientsException)
			{
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		protected virtual void HandleException(Email badEmail, string key, Exception ex)
		{
			ErrorReporter.ReportOnce(key, ex.Message, ex);
			ForwardUnprocessedEmail(badEmail, ex, "error (" + badEmail.SenderNameAddress + ')');
		}

		bool isVerboseMode;

		#region IEmailProcessorLogger Members

		void IEmailProcessorLogger.Log(LogType logType, bool verboseModeOnly, string format, params object[] args)
		{
			Log(logType, verboseModeOnly, format, args);
		}

		public void SetIsVerboseModeFromRegistry()
		{
			isVerboseMode = IsVerboseModeFromRegistry;
		}

		protected abstract bool IsVerboseModeFromRegistry { get; }

		#endregion
	}
}
