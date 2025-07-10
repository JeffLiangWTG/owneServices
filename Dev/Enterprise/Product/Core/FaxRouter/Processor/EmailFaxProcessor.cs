using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
#if NETFRAMEWORK
using System.Web;
#else
using System.Net.Http;
#endif
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.FaxRouter.EventLogging;
using Enterprise.FaxRouter.MailSecurity;
using Enterprise.FaxRouter.TypeDefinitions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.FaxRouter.Processor
{
	public class EmailFaxProcessor : MailDataModule
	{
		readonly ICryptographicProvider cryptographicProvider;

		public EmailFaxProcessor()
		{
			TemporaryFilesDirectory = FAX_GATEWAY_TEMP_FILE_DIRECTORY;

			if (!Directory.Exists(TemporaryFilesDirectory))
			{
				_ = Directory.CreateDirectory(TemporaryFilesDirectory);
			}

			cryptographicProvider = new CryptProvider();
		}

		protected FaxAcknowledgement AFaxAcknowledgement = new();

		protected int maxEmailBatchSize = 10;

		public void TiffFileDataTransform()
		{
			fFaxDataModule.TiffFileDataTransform();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public void HandleEmailToFax()
		{
			var startCount = GetTotalNewReceivedMailItemsBySubjectSubstring("EDI Fax");
			var currentPosition = 0;
			FireFaxProgress(" Checking for new faxes", "FaxJob");

			foreach (var mailItemBatch in GetNewReceivedMailItemsBySubjectSubstringInBatches("EDI Fax", maxEmailBatchSize))
			{
				HandleEmailBatchToFax(mailItemBatch, startCount, currentPosition);
				currentPosition += mailItemBatch.Count;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		protected virtual void HandleEmailBatchToFax(IEnumerable<MailDBItemDataLine> newMailItems, int startCount, int currentPosition)
		{
			foreach (var aMailDBItem in newMailItems)
			{
				try
				{
					currentPosition++;
					var tiffFilename = ExtractEmailTIFFAttachment(aMailDBItem);
					if (SaveFaxJobWithFaxKey(aMailDBItem, tiffFilename))
					{
						var blacklistResult = BlacklistChecker.Check(aMailDBItem);
						if (blacklistResult.IsBlacklisted)
						{
							var blacklistMessage = string.Format(" FAX JOB [{0} of {1}] is blacklisted (Sender: {2}, Destination: {3}, MI_PK {4})", currentPosition, startCount, blacklistResult.Sender, blacklistResult.Destination, aMailDBItem.PrimaryKey);
							FireFaxProgress(blacklistMessage, "Blacklist");
							AFaxAcknowledgement.SendEnterpriseFaxDeliveryNotification(aMailDBItem.SysFaxJobId, DeliveryNotificationType.FAILED, aMailDBItem.From);
						}
						else
						{
							var systemName = "EDI Enterprise";
							FireFaxProgress(" FAX JOB [" + currentPosition + " of " + startCount + "] Processing " + systemName + " Fax (recipient id = " + aMailDBItem.FaxRecipientId + ")", "EDIEnterpriseFaxJob");
							var aFaxForwarder = GetNewFaxForwarder();
							aFaxForwarder.ProcessFaxJobToFaxHandler(tiffFilename, aMailDBItem);
						}
					}
					MarkMailItemAsProcessed(aMailDBItem);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					FireFaxProgress($" FAX JOB [{currentPosition} of {startCount}] Process Failed {ex}", "EDIEnterpriseFaxJob");
					HandleExceptionHandleEmailToFax(aMailDBItem, ex);
				}
			}
		}

		protected virtual FaxForwarder GetNewFaxForwarder()
		{
			return new FaxForwarder();
		}

		void HandleExceptionHandleEmailToFax(MailDBItemDataLine mailDbItem, Exception ex)
		{
			var now = ZDateTime.Now;
			if (mailDbItem.LastAttemptDateTime != default && mailDbItem.LastAttemptDateTime != DateTime.MinValue && mailDbItem.LastAttemptDateTime.Date != new DateTime(1900, 1, 1).Date)
			{
				var diff = now - mailDbItem.LastAttemptDateTime;
				if (diff.TotalMinutes >= 60)
				{
					SendNotificationOnErrorProcessingEmailToFax(ex, mailDbItem);
				}
			}
			else
			{
				mailDbItem.LastAttemptDateTime = now.ToDateTime();
				UpdateLastAttemptDateTime(mailDbItem);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void SendNotificationOnErrorProcessingEmailToFax(Exception ex, MailDBItemDataLine mailDbItem)
		{
			if (!ExceptionIsCausedByEDIProdBeingOffline(ex))
			{
				EventLog.AddErrorEntry("FaxProcessor unknown exception", ex);
				var details = new ExceptionDetails(ex);
				EventLog.ForwardCopyOfProblemEmailToFaxAdministrator(mailDbItem.PrimaryKey, "HandleEmailToFax:\n\rLast Attempt: " + mailDbItem.LastAttemptDateTime.ToString(CultureInfo.CurrentCulture) + "\n\rNow:" + DateTime.Now.ToString(CultureInfo.CurrentCulture) + "\n\r" + details.GetFullReport());
			}
		}

		#region Fax Progress Event

		public delegate void FaxProgressDelegate(string statusMessage, string eventStatus);
		public FaxProgressDelegate OnFaxProgress;

		void FireFaxProgress(string statusMessage, string eventStatus)
		{
			OnFaxProgress?.Invoke(statusMessage, eventStatus);
		}

		#endregion

		bool ExceptionIsCausedByEDIProdBeingOffline(Exception ex)
		{
			var sqlEx = ex as SqlException;
			if (sqlEx != null)
			{
				var errorHandler = new DbErrorHandler(sqlEx, null);
				try
				{
					var errorType = errorHandler.ExceptionType;
					return Db.IsUpgradeLockoutError(sqlEx)
						|| errorType == DbErrorType.CouldNotFindDatabaseId
						|| errorType == DbErrorType.GeneralNetworkError
						|| errorType == DbErrorType.LoginFailedForUser
						|| errorType == DbErrorType.NotInitialised
						|| errorType == DbErrorType.ServerDoesNotExist;
				}
				catch (Exception ex1) when (!ex1.IsCriticalException())
				{
					// If there is an exception determining the error type, then it's not one of errors that we're interested in
					return false;
				}
			}
			else if (ex is DatabaseUpgradeException)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public void HandleFaxAcknowledgements()
		{
			var startCount = GetTotalNewReceivedMailItemsBySender(FAX_ACK_SENDER);
			var currentPosition = 0;
			FireFaxProgress(" Checking for fax acknowledgements", "Ack");

			foreach (var mailItemsBatch in GetNewReceivedMailItemsBySenderInBatches(FAX_ACK_SENDER, maxEmailBatchSize))
			{
				HandleFaxAcknowledgementsBatch(mailItemsBatch, startCount, currentPosition);
				currentPosition += mailItemsBatch.Count;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		protected virtual void HandleFaxAcknowledgementsBatch(IEnumerable<MailDBItemDataLine> newMailItems, int startCount, int currentPosition)
		{
			foreach (var aMailDBItem in newMailItems)
			{
				try
				{
					currentPosition++;
					var returnCode = AFaxAcknowledgement.ProcessFaxJobConfirmation(aMailDBItem);
					FireFaxProgress(" ACK [" + currentPosition + " of " + startCount + "]" + returnCode, "Ack");
					MarkMailItemAsProcessed(aMailDBItem);
				}
#if NETFRAMEWORK
				catch (HttpException webEx)
				{
					FireFaxProgress($" ACK [{currentPosition} of {startCount}] Failed {webEx}", "Ack");
					HandleExceptionOnProcessingAck(aMailDBItem, webEx);
				}
#else
				catch (HttpRequestException webEx)
				{
					FireFaxProgress($"ACK [{currentPosition} of {startCount}] Failed {webEx}", "Ack");
					HandleExceptionOnProcessingAck(aMailDBItem, webEx);
				}
#endif
				catch (SqlException sqlEx)
				{
					FireFaxProgress($" ACK [{currentPosition} of {startCount}] Failed {sqlEx}", "Ack");
					HandleExceptionOnProcessingAck(aMailDBItem, sqlEx);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					FireFaxProgress($" ACK [{currentPosition} of {startCount}] Failed {ex}", "Ack");
					SendNotificationOnErrorProcessingAck(ex, aMailDBItem.PrimaryKey);
				}
			}

			ProcessOverDueAcknowledgements();

			return;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		void HandleExceptionOnProcessingAck(MailDBItemDataLine aMailDBItem, Exception ex)
		{
			var now = DateTime.Now;
			if (aMailDBItem.LastAttemptDateTime != default && aMailDBItem.LastAttemptDateTime != DateTime.MinValue && aMailDBItem.LastAttemptDateTime.Date != new DateTime(1900, 1, 1).Date)
			{
				var diff = now - aMailDBItem.LastAttemptDateTime;
				if (diff.TotalMinutes >= 60)
				{
					SendNotificationOnErrorProcessingAck(ex, aMailDBItem.PrimaryKey, aMailDBItem.LastAttemptDateTime);
				}
			}
			else
			{
				aMailDBItem.LastAttemptDateTime = now;
				UpdateLastAttemptDateTime(aMailDBItem);
			}
		}

		void SendNotificationOnErrorProcessingAck(Exception ex, Guid mailItemPK)
		{
			SendNotificationOnErrorProcessingAck(ex, mailItemPK, new DateTime(1900, 1, 1));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void SendNotificationOnErrorProcessingAck(Exception ex, Guid mailItemPK, DateTime time)
		{
			if (!ExceptionIsCausedByEDIProdBeingOffline(ex))
			{
				EventLog.AddErrorEntry("FaxProcessorAck unknown exception", ex);
				var details = new ExceptionDetails(ex);
				EventLog.ForwardCopyOfProblemEmailToFaxAdministrator(mailItemPK, "HandleFaxAcknowledgements:\n\rLast Attempt: " + time.ToString(CultureInfo.CurrentCulture) + "\n\rNow:" + DateTime.Now.ToString(CultureInfo.CurrentCulture) + "\n\r" + details.GetFullReport());
			}
		}

		bool SaveFaxJobWithFaxKey(MailDBItemDataLine aMailDBItem, string aTiffFileName)
		{
			var aFaxJobRecord = new EDIFaxDBJobDataLine();
			string faxCommandString = null;
			foreach (MailDBAttachmentDataLine attachment in aMailDBItem.Attachments)
			{
				if (attachment.FileName.ToUpper() == "FAXCOMMAND.BASE64")
				{
					var faxCommandBase64Encoded = Encoding.ASCII.GetString(attachment.Data);
					var faxCommandUtf8Bytes = Convert.FromBase64String(faxCommandBase64Encoded);
					faxCommandString = Encoding.UTF8.GetString(faxCommandUtf8Bytes);
					break;
				}
			}

			faxCommandString ??= aMailDBItem.Body;

			var faxCommand = new FaxCommand(faxCommandString);

			var sysID = faxCommand.SysID;
			if (sysID.ToLower().IndexOf("modem") >= 0)
			{
				// Modem usage used to be logged to ediProd. Now it is ignored.
				return false;
			}
			else if (sysID.StartsWith("DailyCount"))
			{
				// Note, the DailyCount report code was removed on 19 Mar 2010 (see WI00022830)
				// Old clients will still be sending this report though.
				return false;
			}
			else if (aTiffFileName != null && aTiffFileName.Length > 0 && File.Exists(aTiffFileName))
			{
				var pageCount = TIFFSDKWrapper.GetNumberOfImagesInTiffFile(aTiffFileName);

				aFaxJobRecord.FaxJobId = Guid.NewGuid();
				aFaxJobRecord.ReceivedDateTime = aMailDBItem.ReceivedDateTime;
				aFaxJobRecord.Sender = aMailDBItem.From;

				aFaxJobRecord.EmailBody = faxCommandString;
				aFaxJobRecord.FaxNumber = faxCommand.FaxNumber;
				aFaxJobRecord.SysFaxJobId = faxCommand.SysFaxJobID;
				aFaxJobRecord.SysId = faxCommand.SysID;
				aFaxJobRecord.PageCount = pageCount;
				aFaxJobRecord.EnterpriseCode = faxCommand.EnterpriseCode;
				aFaxJobRecord.CompanyCode = faxCommand.CompanyCode;
				aFaxJobRecord.ServerCode = faxCommand.PhysicalServerID;

				aMailDBItem.ChargeCode = aFaxJobRecord.ChargeCode;
				aMailDBItem.FaxKeyDateTime = faxCommand.SentDateTimeAsString;
				aMailDBItem.FaxKey = faxCommand.FaxKey;

				var aFaxRecipientRecord = new EDIFaxDBRecipientDataLine();
				aFaxRecipientRecord.FaxRecipientId = Guid.NewGuid();
				aMailDBItem.FaxRecipientId = aFaxRecipientRecord.FaxRecipientId;
				aFaxRecipientRecord.FaxNumber = aFaxJobRecord.FaxNumber;
				aFaxRecipientRecord.AttentionName = faxCommand.FaxAttention;
				aFaxRecipientRecord.Company = faxCommand.FaxAttentionCompany;
				aMailDBItem.FaxRecipientAttentionName = aFaxRecipientRecord.AttentionName.Trim();
				aMailDBItem.FaxRecipientCompany = aFaxRecipientRecord.Company.Trim();
				aMailDBItem.FaxRecipientNumber = aFaxRecipientRecord.FaxNumber.Trim();

				var aMailKey = cryptographicProvider.GenerateKey(FileReader.ReadFile(aTiffFileName), aMailDBItem.FaxKeyDateTime, CryptKeyType.FAX);

				if (aMailKey.Equals(aMailDBItem.FaxKey))
				{
					fFaxDataModule.InsertFaxJob(aFaxJobRecord);
					fFaxDataModule.InsertFaxRecipient(aFaxJobRecord.FaxJobId, aFaxRecipientRecord);
					fFaxDataModule.SetTiffFile(aFaxJobRecord.FaxJobId, aTiffFileName);
					return true;
				}
				return false;
			}
			else
			{
				return false;
			}
		}

		readonly FaxDataModule fFaxDataModule = new();

		string SaveTIFFEmailAttachmentsToTempDirectory(MailDBItemDataLine aMailDBItem)
		{
			foreach (MailDBAttachmentDataLine attachment in aMailDBItem.Attachments)
			{
				if (attachment.FileName.ToUpper().EndsWith(".TIF"))
				{
					var filename = TemporaryFilesDirectory + attachment.FileName;
					if (File.Exists(filename))
					{
						File.Delete(filename);
					}

					var fs = File.Create(filename, 1024);
					fs.Write(attachment.Data, 0, attachment.Data.Length);
					fs.Close();
					return filename;
				}
			}
			return null;
		}

		readonly string TemporaryFilesDirectory = string.Empty;

		string ExtractEmailTIFFAttachment(MailDBItemDataLine aMailDBItem)
		{
			GetMailDBItemTIFFAttachment(aMailDBItem);
			var hasTifAttachment = false;
			foreach (MailDBAttachmentDataLine attachment in aMailDBItem.Attachments)
			{
				if (attachment.FileName.ToUpper().EndsWith(".TIF"))
				{
					hasTifAttachment = true;
					break;
				}
			}

			if (!hasTifAttachment)
			{
				return null;
			}

			return SaveTIFFEmailAttachmentsToTempDirectory(aMailDBItem);
		}

		void ProcessOverDueAcknowledgements()
		{
			try
			{
				var isPriorAckWarningSent = false;
				var numberOfDaysBeforeForgotten = 7;
				var numberOfHoursBeforeOverDue = 2;

				var overDueAckRecords = FaxDataModule.GetOverDueAckFaxes(isPriorAckWarningSent, numberOfDaysBeforeForgotten, numberOfHoursBeforeOverDue);

				if (overDueAckRecords.Count > 0)
				{
					foreach (EDIFaxDBRecipientDataLine recipientDataLine in overDueAckRecords)
					{
						var item = FaxDataModule.GetFaxRecord(recipientDataLine.FaxRecipientId.ToString());
						AFaxAcknowledgement.SendEnterpriseFaxDeliveryNotification(item.SysFaxJobId, DeliveryNotificationType.FAILED, item.From);
						FaxDataModule.OverDueAckFaxWarningSent(recipientDataLine.FaxRecipientId);
					}
				}
			}
			catch (Exception catchAll) when (!catchAll.IsCriticalException())
			{
				EventLog.AddErrorEntry("OverDueAcks", catchAll);
			}
		}

		public static string FormatGatewayTimer(TimeSpan aTimeSpan)
		{
			var formattedTime = new StringBuilder();
			if (aTimeSpan.Hours < 10)
			{
				_ = formattedTime.Append("0");
			}

			_ = formattedTime.Append(aTimeSpan.Hours.ToString());
			_ = formattedTime.Append(":");
			if (aTimeSpan.Minutes < 10)
			{
				_ = formattedTime.Append("0");
			}

			_ = formattedTime.Append(aTimeSpan.Minutes.ToString());
			_ = formattedTime.Append(":");
			if (aTimeSpan.Seconds < 10)
			{
				_ = formattedTime.Append("0");
			}

			_ = formattedTime.Append(aTimeSpan.Seconds.ToString());
			return formattedTime.ToString();
		}
	}
}
