using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Res = MailManager.Res;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public class MailSender
	{
		internal void SendMail(IEnumerable<MailItem> mailItems, ILogger logger = null)
		{
			Helper.ResetEmailResults();
			foreach (var item in mailItems)
			{
				SendOneMail(item, logger);
			}
		}

		public void SendMail(MailItem mailItem, ILogger logger)
		{
			Helper.ResetEmailResults();
			SendOneMail(mailItem, logger);
		}

		public void MarkAsSentWithoutSending(MailItem mailItem)
		{
			mailItem.MI_Status = MailStatus.Sent;
			mailItem.Factory.Save();
		}

		void SendOneMail(MailItem mailItem, ILogger logger)
		{
			try
			{
				SendOneMailCore(mailItem, logger);
			}
			catch (ZSaveConcurrencyException)
			{
				//do nothing - will be handled on the next loop
				//but do reload it so we don't keep running into concurrency error
				mailItem.ReloadSafe();
			}
			catch (ZDataConcurrencyException)
			{
				mailItem.ReloadSafe();
			}
			catch (FailedToAuthenticateException ex)
			{
				logger.Log(LogType.Error, $"Error sending email with subject '{mailItem.MI_Subject}' : {ex.GetFullMessage()}");
				throw;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging. Should not be translated.")]
		void SendOneMailCore(MailItem mailItem, ILogger logger)
		{
			using (var sender = ObjectFactory.Get<IMailSenderProvider>().GetSender(logger, mailItem.MI_From))
			{
				if (mailItem.MailRecipients.Count == 0)
				{
					MarkAsFailedAndSave(sender, mailItem);
				}
				else if (mailItem.HasActiveRecipients())
				{
					try
					{
						CheckRecipients(mailItem);
						CheckAttachmentSize(mailItem);
						sender.Send(mailItem);
						mailItem.UpdateMailItemStatus();
						Helper.AddEmailResult(sender, mailItem, isSuccess: true);

						if (sender is ISmtpSender smtpSender)
						{
							var rejectedRecipients = smtpSender.GetRejectedRecipients();
							if (rejectedRecipients != null && rejectedRecipients.Length > 0 && logger != null)
							{
								var warningMessageBuilder = new ZStringBuilder("The email was not delivered to the following recipients due to partial rejection by the mail server:");
								foreach (var recipient in rejectedRecipients)
								{
									warningMessageBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0} ([{1}]{2})", recipient.Address, recipient.ErrorCode, recipient.ErrorMessage));
								}
								logger.Log(LogType.Warning, warningMessageBuilder.ToStringWithNewLineBetweenAppends());
							}
						}

						mailItem.Factory.Save();
					}
					catch (FailedToSendMessageException ex)
					{
						LogSendError(logger, ex);
						if (ex.Message.Contains("4.4.2")) //4.4.2 Message submission rate for this client has exceeded the configured limit
						{
							//wait until next minute before continuing to try and send emails
							// https://learn.microsoft.com/en-us/exchange/mail-flow/message-rate-limits?view=exchserver-2019
							//(except in debug mode, because then it'll be awful to unit test)
#if !DEBUG
							System.Threading.Thread.Sleep(1000 * 60);
#endif
						}
						//If contaions "4.3.2", not mark as failed.
						// https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages
						else if (!ex.Message.Contains("4.3.2")) //4.3.2 Concurrent connections limit exceeded
						{							
							MarkAsFailedAndSave(sender, mailItem);
						}
					}
					catch (FailedToConnectException ex)
					{
						LogSendError(logger, ex);
						MarkAsFailedAndSave(sender, mailItem);
						if (!ex.Message.Contains("A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider."))
						{
							throw;
						}
					}
					catch (MailInterfaceException) { throw; }
					catch (SqlLockLostException) { throw; }
					catch (ArgumentException ex) when (HasNonAsciiRecipient(mailItem))
					{
						LogSendError(logger, ex);
						MarkAsFailedAndSave(sender, mailItem);
					}
					catch (FormatException ex)
					{
						LogSendError(logger, ex);
						MarkAsFailedAndSave(sender, mailItem);
					}
					catch (Exception ex) when (!ex.IsCriticalException() || (ex is OutOfMemoryException))
					{
						LogSendError(logger, ex);
						Helper.AddEmailResult(sender, mailItem, isSuccess: false);
						throw;
					}
				}
			}
		}

		bool HasNonAsciiRecipient(MailItem mailItem)
		{
			var recipients = mailItem.AllRecipients;
			return Encoding.UTF8.GetByteCount(recipients) != recipients.Length;
		}

		void MarkAsFailedAndSave(IMailSender sender, MailItem mailItem)
		{
			mailItem.MI_Status = MailStatus.Failed;
			Helper.AddEmailResult(sender, mailItem, isSuccess: false);
			mailItem.Factory.Save();
		}

		void LogSendError(ILogger logger, Exception ex)
		{
			if (logger != null)
			{
				logger.Log(LogType.Error, ex.ToString());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging. Should not be translated.")]

#if DEBUG
		public
#endif
		void CheckRecipients(MailItem mailItem)
		{
			var testSystemEmailOverride = Environment.Env.Registry.EmailDestinationOverride;

			foreach (var recipient in mailItem.MailRecipients.Cast<MailRecipient>())
			{
				var emailsRecipient = recipient.EmailAddress.Split(',');
				foreach (var email in emailsRecipient)
				{
					if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(email.TrimStart()))
					{
						var message =
							string.Format(CultureInfo.InvariantCulture,
								"Failed to send email with subject {0} because one or more recipients have empty or invalid email address",
								mailItem.MI_Subject
							);
						throw new FailedToSendMessageException(message);
					}
					else if (email == testSystemEmailOverride)
					{
						var glbEmailDestinationOverrideAddress = GlbEmailAddress.Load(mailItem.Factory, testSystemEmailOverride);
						if (glbEmailDestinationOverrideAddress != null &&
							glbEmailDestinationOverrideAddress.GI_DeliveryStatus == EmailDeliveryReportStatus.Codes.NonDeliveryReport)
						{
							var message =
							string.Format(CultureInfo.InvariantCulture,
								"Failed to send email with subject {0}. The email destination override has received NDR, please check the address in Registry->System->Testing->Email Destination Override",
								mailItem.MI_Subject
							);
							throw new FailedToSendMessageException(message);
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logging. Should not be translated.")]
		void CheckAttachmentSize(MailItem mailItem)
		{
			var registrySizeLimitInBytes = (long)SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.Value * Megabyte;
			var mailAttachmentTotalSize = mailItem.MailAttachments.OfType<MailAttachment>().Sum(a => a.MA_Data.Length);
			if (mailAttachmentTotalSize > registrySizeLimitInBytes)
			{
				var attachmentNames = mailItem.MailAttachments.OfType<MailAttachment>().Select(a => a.MA_FileName);
				var message =
					string.Format(CultureInfo.InvariantCulture,
						"Failed to send email with Subject: {0} as the attachment: {1} file size ({2}) exceeds the size limit ({3}MB) set in the Registry at {4}.  Please verify with your email administrator that larger attachments are supported by the email server before adjusting this registry setting.",
						mailItem.MI_Subject, ListFormatter.GetCommaSeparatedText(attachmentNames),
						MailAttachment.AttachmentSize(mailAttachmentTotalSize), SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.Value,
						SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.Location());
				throw new FailedToSendMessageException(message);
			}
		}

		public List<EmailSentResult> GetEmailSentResults() => Helper.SentEmailResults;

		public string ConfigurationRegistryPath => Res.GetString("11CBD47E-3269-4221-BF5F-1D7E4CC65860", "Physical Server -> Mail -> Outgoing -> SMTP");

		MailSenderHelper Helper { get; } = new MailSenderHelper();
		internal const int Megabyte = 1024 * 1024;

		class MailSenderHelper
		{
			public MailSenderHelper()
			{
				SentEmailResults = new List<EmailSentResult>();
			}

			public void AddEmailResult(IMailSender sender, MailItem mail, bool isSuccess)
			{
				var sendingServer = sender.GetSendingInfo();
				SentEmailResults.Add(new EmailSentResult(mail, sendingServer, isSuccess));
			}

			public void ResetEmailResults()
			{
				SentEmailResults.Clear();
			}

			public List<EmailSentResult> SentEmailResults { get; internal set; }
		}
		public class EmailSentResult
		{
			public EmailSentResult(MailItem mail, string sendingServer, bool isSuccess)
			{
				Mail = mail;
				IsSuccess = isSuccess;
				SendingServer = sendingServer;
			}
			public MailItem Mail { get; }
			public bool IsSuccess { get; }
			public string SendingServer { get; }
		}
	}

	[Serializable]
	public class FailedToSendAllMailItemsException : Exception
	{
#if NETFRAMEWORK
		public FailedToSendAllMailItemsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception messages should not be translated")]
		public FailedToSendAllMailItemsException(Exception lastUnhandledException)
			: base("Failed to send all emails", lastUnhandledException)
		{ }
	}
}
