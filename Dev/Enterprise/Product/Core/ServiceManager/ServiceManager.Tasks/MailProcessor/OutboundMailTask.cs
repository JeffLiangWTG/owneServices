using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Tasks.MailProcessor;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"OMS",
	"Outbound Mail Service",
	"MAI",
	typeof(OutboundMailTask),
	AllowsMultipleInstances = true,
	IsMandatory = true,
	MinimumPeriod = "1minute",
	MaximumPeriod = "1hour",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true,
	TaskSpecificValidationType = typeof(OMSSpecificValidation))
]

[assembly: HostedServiceBusinessObjectBinding("OMS", MailDBItemsSchema.Constants.TableName, new[] { MailDBItemsSchema.Constants.MI_Direction + "=" + MailDirection.Transmit, MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.Queued }, "Outbound Mail")]
[assembly: HostedServiceBusinessObjectBinding("OMS", MailDBItemsSchema.Constants.TableName, new[] { MailDBItemsSchema.Constants.MI_Direction + "=" + MailDirection.Transmit, MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.QueuedWithAck }, "Outbound Mail with acknowledgement")]

namespace Enterprise.ServiceManager.Tasks.MailProcessor
{
	public class OutboundMailTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			if (string.IsNullOrEmpty(Env.Instance.Registry.SMTPServer) && !Env.Instance.Registry.UseGraphApiForOutgoing && !SystemDataRegistry.Instance.RunOMSInSimulationMode.Value)
			{
				var nextRunTimeUtc = ZDateTime.UtcNow.AddDays(7);
				ServiceLogger.Log(LogType.Error, $@"As a result of incomplete mail configurations in the Registry, the Outbound Mail Service Task has been paused and has been rescheduled to run again on {nextRunTimeUtc}.
Please verify the mail configuration settings in the Registry at Registry -> {RawDataRegistry.Instance.SMTPServer.GetLocation()}.
Once the mail configuration has been set, the Service Task will automatically resume running after {RegistryRefresh.FrequencyInSeconds} seconds.");

				MailProcessorHelper.RescheduleMailTask("OMS", nextRunTimeUtc);
				return;
			}
			var sender = new MailSender();

			try
			{
				ServiceLogger.Log(LogType.Information, "Outbound Mail Service task started.");
				var processedData = false;
				do
				{
					processedData = SendEmail(sender, token) > 0;
				}
				while (processedData && !token.IsCancellationRequested);

				ServiceLogger.Log(LogType.Information, "Outbound Mail Service task finished.");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var detailsEx = ex is FailedToSendAllMailItemsException && ex.InnerException != null ? ex.InnerException : ex;

					ServiceLogger.Log(LogType.Debug, "An exception occurred during OMS cycle: " + ex);
					var errorMessage = new ZStringBuilder();
					if (ex is SmtpConfigurationException || ex.InnerException is SmtpConfigurationException)
					{
						errorMessage.Append("There is a configuration error with your SMTP server: ");
						errorMessage.Append(detailsEx.Message);
						errorMessage.Append("\r\n\r\nThis is not a system defect, this is a configuration issue. Please contact your SMTP administrator to check the Security SMTP settings in the System Registry under ");
						errorMessage.Append(sender.ConfigurationRegistryPath);
						errorMessage.Append(".\r\n\r\nPlease refer to our eLearning Portal -> FAQs -> 'We cannot email from our system, what should we do?' for more information before contacting CW1 Support.");
					}
					else if (ex is FailedToConnectException || ex.InnerException is FailedToConnectException)
					{
						errorMessage.Append("Failed to connect to the SMTP server.\r\n\r\n");
						errorMessage.Append(detailsEx.Message);
						errorMessage.Append("\r\n\r\nCheck your SMTP settings in the System Registry under ");
						errorMessage.Append(sender.ConfigurationRegistryPath);
						errorMessage.Append(", and be sure the SMTP server is running properly.");
					}
					else if (ex is FailedToAuthenticateException || ex.InnerException is FailedToAuthenticateException)
					{
						errorMessage.Append("Failed to login to the SMTP server.\r\n\r\n");
						errorMessage.Append(detailsEx.Message);
						errorMessage.Append("\r\n\r\nCheck your login and security SMTP settings in the System Registry under ");
						errorMessage.Append(sender.ConfigurationRegistryPath);
						errorMessage.Append(".");
					}
					errorMessage.Append("\r\n\r\n------------------------------------------------------------------------------------------------------");
					errorMessage.Append("\r\n\r\nError details:");
					errorMessage.Append(detailsEx.ToString());

					ServiceLogger.Log(LogType.Error, errorMessage.ToString());

				throw new HostedServiceException(errorMessage.ToString(), ex);
			}
		}

		#region Implementation

		int SendEmail(MailSender mailSender, CancellationToken token)
		{
			var totalSent = 0;

			var outgoingMailQueue = new DbOnlyBusinessObjectQueue<MailItem>(GetQueuedSendableMailQuery());
			outgoingMailQueue.ProcessBatch((outgoingMail, e) =>
			{
				if (token.IsCancellationRequested)
				{
					Thread.MemoryBarrier();
					e.Cancel = true;
				}
				else
				{
					SendEmail(mailSender, outgoingMail, false, ref totalSent);
				}
			}, DataRegistry.Instance.MaximumNumberOfMailItemsToSendInABatch, token, true);

			if (SupportSendingEmailWithAcknowledgement && !token.IsCancellationRequested)
			{
				var failedUpgrades = GetDatabaseEmailManagement().StopUnsuccessfulUpgradeSending();
				if (failedUpgrades > 0)
				{
					ServiceLogger.Log(LogType.Warning, "Stopped unsuccessful sending of " + failedUpgrades + " upgrade(s).");
				}

				var outgoingMailWithAsknowledgementQueue = new DbOnlyBusinessObjectQueue<MailItem>(GetQueuedWithAcknowledgementSendableMailQuery());
				outgoingMailWithAsknowledgementQueue.ProcessBatch((outgoingMailWithAsknowledgment, e) =>
				{
					if (token.IsCancellationRequested)
					{
						Thread.MemoryBarrier();
						e.Cancel = true;
					}
					else
					{
						SendEmail(mailSender, outgoingMailWithAsknowledgment, true, ref totalSent);
					}
				}, DataRegistry.Instance.MaximumNumberOfMailItemsToSendInABatch, token);
			}
			return totalSent;
		}

		bool ItemHasAlreadyBeenProcessed(MailItem mail, bool withAcknowledgment)
		{
			// If it's been processed by another service task we will need to reload to see changes
			mail.Reload();

			var readyToProcessStatus = withAcknowledgment ? MailStatus.QueuedWithAck : MailStatus.Queued;
			return mail.MI_Status != readyToProcessStatus;
		}

		protected void SendEmail(MailSender mailSender, MailItem[] mailItems, bool withAcknowledgment, ref int totalSent)
		{
			if (mailItems.Length == 0)
			{
				return;
			}

			var emailsSentByServer = new List<MailSender.EmailSentResult>();
			var unhandledFailures = new List<(MailItem mi, Exception ex)>();

			foreach (var mail in mailItems)
			{
				var result = Db.Connection.RunLocked("MailToSend:" + mail.PK,
					isFinished: () => ItemHasAlreadyBeenProcessed(mail, withAcknowledgment),
					process: (isFirstRun) => ProcessSingleMailItem(mailSender, mail, unhandledFailures));

				switch (result)
				{
					case LockedProcessResult.Completed:
						emailsSentByServer.AddRange(mailSender.GetEmailSentResults());
						break;
					case LockedProcessResult.Error:
						ServiceLogger.Log(LogType.Warning, "One mail item was not sent because of repeated connection issues. It has been removed from this list and will be retried in another run.");
						break;
				}
			}

			if (!SystemDataRegistry.Instance.RunOMSInSimulationMode.Value)
			{
				ProcessSendResults(unhandledFailures,mailItems,emailsSentByServer,withAcknowledgment,ref totalSent);
				return;
			}

			ServiceLogger.Log(LogType.Information, $"{mailItems.Length} email(s) marked as sent as per simulation mode.");
		}

		protected virtual void ProcessSendResults(List<(MailItem mi, Exception ex)> unhandledFailures, MailItem[] mailItems, List<MailSender.EmailSentResult> emailsSentByServer, bool withAcknowledgment, ref int totalSent)
		{
			ProcessUnhandledFailures(unhandledFailures, mailItems.Length, withAcknowledgment);

			totalSent += emailsSentByServer.Count(r => r.IsSuccess);
			ProcessSentItems(emailsSentByServer, withAcknowledgment, totalSent, isSuccess: true);

			ProcessSentItems(emailsSentByServer, withAcknowledgment, totalSent, isSuccess: false);
		}

		void ProcessSingleMailItem(MailSender mailSender, MailItem mail, IList<(MailItem mi, Exception ex)> unhandledFailures)
		{
			try
			{
				if (!SystemDataRegistry.Instance.RunOMSInSimulationMode.Value)
				{
					mailSender.SendMail(mail, ServiceLogger);
				}
				else
				{
					mailSender.MarkAsSentWithoutSending(mail);
				}
			}
			catch (MailInterfaceException) { throw; } // Do not add as unhandled - throw further
			catch (SqlLockLostException) { throw; } // Do not add as unhandled - throw further, will be handled by Db.Connection.RunLocked()
			catch (Exception ex) when (!ex.IsCriticalException() || (ex is OutOfMemoryException))
			{
				unhandledFailures.Add((mail, ex));
			}
		}

		void ProcessUnhandledFailures(List<(MailItem, Exception)> unhandledFailures, int mailItemsCount, bool withAcknowledgment)
		{
			if (unhandledFailures.Count == mailItemsCount)
			{
				throw new FailedToSendAllMailItemsException(unhandledFailures.Last().Item2);
			}
			if (unhandledFailures.Count > 0)
			{
				foreach (var failedItem in unhandledFailures.Select<(MailItem mi, Exception ex), MailItem>(f => f.mi))
				{
					if (!ItemHasAlreadyBeenProcessed(failedItem, withAcknowledgment))
					{
						failedItem.MI_Status = MailStatus.Failed;
					}
				}

				ZExceptionReporting.ProcessWithSaveExceptionHandling(() => unhandledFailures.First().Item1.Factory.Save(), null, reportErrorsOnly: true);
			}
		}

		void ProcessSentItems(IEnumerable<MailSender.EmailSentResult> emailsSentByServer, bool withAcknowledgment, int totalSent, bool isSuccess)
		{
			var message = new StringBuilder();
			var results = emailsSentByServer.Where(r => r.IsSuccess == isSuccess);
			if (totalSent > 0 && results.Any())
			{
				foreach (var group in results.GroupBy(r => r.SendingServer))
				{
					var serverInfo = group.Key;
					var count = group.Count();
					var ackInfo = withAcknowledgment ? " with acknowledgment" : "";
					var failInfo = isSuccess ? "sent" : "failed to be sent";

					if (message.Length > 0)
					{
						message.AppendLine();
					}
					message.AppendFormat(CultureInfo.InvariantCulture, $"{count} email(s){ackInfo} {failInfo} {serverInfo}. Total sent in this batch - {totalSent}");

					foreach (var item in group)
					{
						if (!item.Mail.IsDeleted)
						{
							message.AppendLine().AppendFormat(CultureInfo.InvariantCulture, "  To:{0} Subject:{1}", item.Mail.AllRecipients, item.Mail.MI_Subject);
						}
						else
						{
							message.AppendLine().AppendFormat(CultureInfo.InvariantCulture, "  The Email has been deleted");
						}
					}
				}

				var logType = isSuccess ? LogType.Information : LogType.Warning;
				ServiceLogger.Log(logType, message.ToString());
			}
		}

		internal ZQuery GetQueuedSendableMailQuery()
		{
			var query = new ZQuery(MailDBItemsSchema.MI_Direction, MailDirection.Transmit);
			query.MaximumRows = 1000; // further chunking of this data occurs within the mail sender
			query.AddToFilter(MailDBItemsSchema.MI_Status, MailStatus.Queued);
			query.AddToFilter(MailDBItemsSchema.MI_SendDateTime, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
			query.OrderBy = $"{MailDBItemsSchema.Constants.MI_QueueWithLowPriority}, {MailDBItemsSchema.Constants.MI_SendDateTime}";

			return query;
		}

		ZQuery GetQueuedWithAcknowledgementSendableMailQuery()
		{
			var query = new ZDBOnlyQuery(typeof(MailItem));
			query.MaximumRows = 1000; // further chunking of this data occurs within the mail sender
			query.AddToFilter(MailDBItemsSchema.MI_Direction, MailDirection.Transmit);
			query.AddToFilter(MailDBItemsSchema.MI_Status, MailStatus.QueuedWithAck);
			query.AddToFilter(MailDBItemsSchema.MI_SendDateTime, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);

			ZString sqlText = String.Format("({0} IN " +
				"( SELECT MA.{1} FROM {2} MA " +
				"LEFT JOIN (SELECT MR.{3}, SUM(MR.{6}) As SentCount FROM {2} MR " +
				"WHERE MR.{4} IS NULL AND MR.{5} IS NOT NULL AND MR.{5} > @CountingStart " +
				"GROUP BY MR.{3}) MC " +
				"ON (MC.{3} = MA.{3}) " +
				"WHERE {4} IS NULL AND {6} < @MaxAttempts AND ({5} IS NULL OR {5} < @AcknowledgementTime) AND " +
				"(MC.SentCount IS NULL OR MC.SentCount < @SendingLimit)))",

				MailDBItemsSchema.PK.Name,                           /*0*/
				MailDBRecipientsSchema.MR_MI.Name,                   /*1*/
				MailDBRecipientsSchema.Constants.TableName,          /*2*/
				MailDBRecipientsSchema.MR_RecipientMailAddress.Name, /*3*/
				MailDBRecipientsSchema.MR_DeliveredTime.Name,        /*4*/
				MailDBRecipientsSchema.MR_LastAttempt.Name,          /*5*/
				MailDBRecipientsSchema.MR_AckAttempt.Name);          /*6*/

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@CountingStart", ZDateTime.UtcNow.AddMinutes(-MailSendingLimitation.LimitationPeriod), MailDBRecipientsSchema.MR_LastAttempt);
			parameters.Add("@MaxAttempts", MailAcknowledgement.MaxAttempts, MailDBRecipientsSchema.MR_AckAttempt);
			parameters.Add("@AcknowledgementTime", ZDateTime.UtcNow.AddMinutes(-MailAcknowledgement.AcknowledgmentTimeout), MailDBRecipientsSchema.MR_LastAttempt);
			parameters.Add("@SendingLimit", MailSendingLimitation.MaxToSend, MailDBRecipientsSchema.MR_AckAttempt);
			query.AddFilterAndZSQLParameterCollection(sqlText, parameters);

			query.OrderBy = $"{MailDBItemsSchema.Constants.MI_QueueWithLowPriority}, {MailDBItemsSchema.Constants.MI_SendDateTime}";

			return query;
		}

		DatabaseEmailManagement GetDatabaseEmailManagement()
		{
			return DatabaseEmailManagement.Create();
		}

		protected virtual bool SupportSendingEmailWithAcknowledgement
		{
			get { return ClientHookLoader.Instance.Client == Clients.EDI; }
		}

		#endregion
	}
}
