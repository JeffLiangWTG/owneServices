using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;

namespace Enterprise.Messaging.Business
{
	public class FailedMessagesManager
	{
		readonly Dictionary<ZGuid, (bool, int)> failedMessages = new Dictionary<ZGuid, (bool, int)>();
		readonly RetryType retryType;
		readonly ShouldRetryOnExceptionFunc shouldRetryOnException;

		public FailedMessagesManager(RetryType retryType, ShouldRetryOnExceptionFunc shouldRetryOnException)
		{
			this.retryType = retryType;
			this.shouldRetryOnException = shouldRetryOnException;
		}

		public delegate bool ShouldRetryOnExceptionFunc(int currentExceptionsCount, EDIMessage message, Exception exception, int retryAttempts, string additionalErrorReportMessage = null);

		public delegate void PostProcessMessageOnExceptionFunc(EDIMessage message);

		public bool ShouldRetry(int currentExceptionsCount, EDIMessage message, Exception exception, int retryAttempts, string additionalErrorReportMessage = null)
		{
			return shouldRetryOnException(currentExceptionsCount, message, exception, retryAttempts, additionalErrorReportMessage);
		}

		public void AddFailedMessage(ZGuid guid, bool shouldRetry, int maxRetryCount)
		{
			var failedCount = 1;
			if (failedMessages.TryGetValue(guid, out (bool, int) value))
			{
				shouldRetry = shouldRetry && value.Item2 < maxRetryCount; //this can only happen if we've failed to update the failure count in the DB
				failedCount = ++value.Item2;
			}
			failedMessages[guid] = (shouldRetry, failedCount);
		}

		public IEnumerable<ZGuid> GetMessagesWithNoRetry()
		{
			if (retryType == RetryType.CurrentExecution)
			{
				return failedMessages.Where(m => !m.Value.Item1).Select(x => x.Key);
			}
			else
			{
				return failedMessages.Keys;
			}
		}

		public static string GetMessageIdentification(EDIMessage message) => $"{message.EM_ApplicationCode}-{message.EM_MessageType}-{message.EM_MessageNum}";

		public void MarkMessageAsHavingException(BusinessObjectFactory newFactory, EDIMessage message, Exception ex, string failedMessageStatus, Func<EDIMessage, bool> isQueuedMessage, LoggingInformation logger, bool hasIncrementedCount, PostProcessMessageOnExceptionFunc postProcessOnException = null, string additionalErrorReportMessage = null)
		{
			using (newFactory.AddDisposableService())
			{
				var shouldRetry = false;
				var maxRetryCount = 0;
				try
				{
					string errorMessage = null;
					newFactory.RefreshEnabled = false;
					var messageFromNewFactory = newFactory.Load<EDIMessage>(message.PK);
					if (messageFromNewFactory != null)
					{
						if (!hasIncrementedCount)
						{
							messageFromNewFactory.EM_RetryCount++;
						}

						if (ex is MessageProcessingBusinessFailureException messageProcessingBusinessFailureEx)
						{
							const int maxRetryTimes = 3;
							shouldRetry = messageFromNewFactory.EM_RetryCount < maxRetryTimes && messageProcessingBusinessFailureEx.ShouldRetry;
							errorMessage = GetErrorMesssage(messageProcessingBusinessFailureEx);
							maxRetryCount = maxRetryTimes;
						}
						else
						{
							maxRetryCount = eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value;
							shouldRetry = ShouldRetry(messageFromNewFactory.EM_RetryCount, message, ex, maxRetryCount, additionalErrorReportMessage);
							errorMessage = ex.Message;
						}

						RequeueOrFailMessage(messageFromNewFactory, shouldRetry, failedMessageStatus, isQueuedMessage(messageFromNewFactory));

						if (messageFromNewFactory.EM_Status == failedMessageStatus)
						{
							var failureSubject = Res.GetString("ea75cf49-e18f-46d9-b863-04c07c6eacdc", "Error Processing Incoming EDI Message: {0}", GetMessageIdentification(messageFromNewFactory));
							CreateNotificationAndNotifyCompanyNotificationGroup(messageFromNewFactory.EM_RetryCount, newFactory, messageFromNewFactory, ex, failureSubject, failedMessageStatus, logger);
							postProcessOnException?.Invoke(messageFromNewFactory);
						}
						else
						{
							AddDataImportLogNote(newFactory, messageFromNewFactory, errorMessage);
						}

						newFactory.Save();
					}
				}
				finally
				{
					AddFailedMessage(message.PK, shouldRetry, maxRetryCount);
				}
			}
		}

		public void AddExceptionLogNote(BusinessObjectFactory factory, EDIMessage message, MessageProcessingBusinessFailureException exception)
		{
			AddDataImportLogNote(factory, message, GetErrorMesssage(exception));
		}

		string GetErrorMesssage(MessageProcessingBusinessFailureException exception)
		{
			var exCaption = string.IsNullOrEmpty(exception.Caption) ? string.Empty : exception.Caption + ": ";
			return string.Join("", exception.LogNote,"\r\n", exCaption, exception.Message);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Error Report Only")]
		void CreateNotificationAndNotifyCompanyNotificationGroup(int count, BusinessObjectFactory factory, EDIMessage message, Exception ex, ZString failureSubject, ZString failedMessageStatus, LoggingInformation logger)
		{
			var failureMessage = GetFailureMessage(count, failedMessageStatus);
			AddDataImportLogNote(factory, message, failureMessage + "\r\n\r\n" + ex.ToString());
			logger.LogError(failureSubject + "\r\n" + failureMessage + "\r\n" + ex.ToString());

			var isReportedToUserOnlyException = ex is System.Data.Common.DbException
				|| ex is ConcurrencyConflictException
				|| ex is SqlLockLostException
				|| ex is ZCannotSaveException
				|| ex is ZSaveConcurrencyException
				|| ex is MessageProcessingBusinessFailureException
				|| (ex is ZSaveException zSaveException
					&& (zSaveException.InnerException.DbErrorType == DbErrorType.InsertConflictedWithCheckConstraint
						|| zSaveException.InnerException.DbErrorType == DbErrorType.UpdateConflictedWithCheckConstraint
					));

			if (!isReportedToUserOnlyException)
			{
				var errorReport = failureSubject;

				if (ex is ApplicationException
					&& ex.InnerException is ApplicationException
					&& ex.InnerException.Message.Contains("Attempted to return a Enterprise.MasterFiles.Business.ProcessTask when a Enterprise.Freight.Forwarding.Business.ForwardingConsolProcessTask was requested."))
				{
					errorReport = GetForwardingConsolProcessTaskReport(message, failureSubject);
				}

				ErrorReporter.ReportOnce(errorReport, ex); // A key will be a calculated hash key of the call stack.
			}

			var emailBody = failureMessage + "\r\n\r\n"
				+ message.EM_MessageText.Left(32384) + "\r\n\r\n"
				+ ex.ToString();
			try
			{
				Env.Instance.OutgoingMailManager.CreateAndSaveToCompanyNotificationGroup(failureSubject, emailBody);
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
			}
		}

		void AddDataImportLogNote(BusinessObjectFactory factory, EDIMessage messageFromNewFactory, string message)
		{
			DataImportNoteCreater.AddNew(messageFromNewFactory, (noteStream) =>
			{
				try
				{
					using (StreamWriter writer = new StreamWriter(noteStream, new UTF8Encoding(false, true), 1024, leaveOpen: true))
					{
						writer.WriteLine(message);
						writer.Flush();
						noteStream.Position = 0;
					}
				} catch (EncoderFallbackException)
				{
					using (StreamWriter writer = new StreamWriter(noteStream, new UTF8Encoding(false, true), 1024, leaveOpen: true))
					{
						string utf8Message = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(message));
						writer.WriteLine(utf8Message);
						writer.Flush();
						noteStream.Position = 0;
					}
				}
			});
		}

		void RequeueOrFailMessage(EDIMessage message, bool shouldRetry, ZString failedMessageStatus, bool isQueuedMessage)
		{
			if (!shouldRetry)
			{
				message.EM_Status = failedMessageStatus;
			}
			else if (!isQueuedMessage)
			{
				message.EM_Status = EDIMessage.Status.Queued;
			}
		}

		string GetFailureMessage(int exceptionCount, ZString failedMessageStatus)
		{
			return Res.GetString("d83824f5-3a91-408e-b08d-cf3ae9d0946a", "Exception occurred {0} times whilst processing a message individually. The message's status has been set to '{1}'.", exceptionCount, new EDIMessageStatusList().GetDescriptionFromCode(failedMessageStatus));
		}

		#region SuppressResourceStringsCheckRegion

		string GetForwardingConsolProcessTaskReport(EDIMessage message, string failureSubject)
		{
			var builder = new ZStringBuilder();
			builder.AppendLine(failureSubject);
			builder.AppendFormat(@"Message EM_MessageType = {0},
EM_MessageSubType = {1},
EM_ReceiveTransmit = {2},
EM_MessageNum = {3},
EM_Status = {4},
EM_LinkTable = {5},
EM_MessageText = {6}.",
				message.EM_MessageType,
				message.EM_MessageSubType,
				message.EM_ReceiveTransmit,
				message.EM_MessageNum,
				message.EM_Status,
				message.EM_LinkTable,
				message.EM_MessageText);

			return builder.ToString();
		}

		#endregion
	}

	public enum RetryType
	{
		///<summary>A message that has thrown and is requeued will be retried in the next batch of messages</summary>
		CurrentExecution,
		///<summary>A message that has thrown and is requeued will be retried in the next service task run after the current batch has finished</summary>
		NextExecution
	}
}
