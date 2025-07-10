using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Tasks.MailProcessor;
using Enterprise.ZArchitecture.Modules;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("MMS", "Mail Maintenance Service", "MAI", typeof(MailMaintananceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "15minutes",
	MaximumPeriod = "1week",
	DefaultScheduleRunEvery = "30minutes",
	DefaultScheduleStartAtLocal = "23hours",
	ActiveByDefault = true)
]

namespace Enterprise.ServiceManager.Tasks.MailProcessor
{
	public class MailMaintananceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			LogProcess("Start");

			int messagesProcessed = 0;
			do
			{
				token.ThrowIfCancellationRequested();
				try
				{
					messagesProcessed = PerformMaintenanceActions();
				}
				catch (SqlException ex) when (ex.Number == DeadlockSqlExceptionNumber)
				{
					ServiceLogger.Log(LogType.Information, "Deadlock occurred whilst performing maintenance. Retrying action.");
					Thread.Sleep(TimeSpan.FromSeconds(20));
				}
			} while (messagesProcessed > 0);

			LogProcess("End");
		}
		protected const int DeadlockSqlExceptionNumber = 1205;

		#region Implementation

		int PerformMaintenanceActions()
		{
			int messagesProcessed = 0;
			DatabaseEmailManagement databaseEmailManager = GetDatabaseEmailManagement();

			ServiceLogger.Log(LogType.Information, "Checking for old received and processed messages to purge...");
			int messagesDeleted = databaseEmailManager.PurgeProcessedIncomingEmailOlderThan(SystemDataRegistry.Instance.PurgeIncomingProcessedEmailsOlderThan.Value);
			if (messagesDeleted > 0)
			{
				messagesProcessed += messagesDeleted;
				ServiceLogger.Log(LogType.Information, messagesDeleted + " processed message(s) purged.");
			}
			else
			{
				ServiceLogger.Log(LogType.Information, "None found.");
			}

			ServiceLogger.Log(LogType.Information, "Checking for old unprocessed messages to purge...");
			messagesDeleted = databaseEmailManager.PurgeIncomingEmailOlderThan(SystemDataRegistry.Instance.PurgeIncomingUnProcessedEmailsOlderThan.Value);
			if (messagesDeleted > 0)
			{
				messagesProcessed += messagesDeleted;
				ServiceLogger.Log(LogType.Information, messagesDeleted + " unprocessed message(s) purged.");
			}
			else
			{
				ServiceLogger.Log(LogType.Information, "None found.");
			}

			ServiceLogger.Log(LogType.Information, "Checking for old sent messages to purge...");
			messagesDeleted = databaseEmailManager.PurgeProcessedOutgoingEmailOlderThan(SystemDataRegistry.Instance.PurgeOutgoingEmailsOlderThan.Value);
			if (messagesDeleted > 0)
			{
				messagesProcessed += messagesDeleted;
				ServiceLogger.Log(LogType.Information, messagesDeleted + " sent message(s) purged.");
			}
			else
			{
				ServiceLogger.Log(LogType.Information, "None found.");
			}

			ServiceLogger.Log(LogType.Information, "Checking for old unsent messages to purge...");
			messagesDeleted = databaseEmailManager.PurgeUnsentOutgoingEmailOlderThan(SystemDataRegistry.Instance.PurgeUnsentOutgoingEmailsOlderThan.Value);
			if (messagesDeleted > 0)
			{
				messagesProcessed += messagesDeleted;
				ServiceLogger.Log(LogType.Information, messagesDeleted + " unsent message(s) purged.");
			}
			else
			{
				ServiceLogger.Log(LogType.Information, "None found.");
			}

			if (SupportSendingEmailWithAcknowledgement)
			{
				ServiceLogger.Log(LogType.Information, "Checking for old upgrade messages to truncate message body...");
				int messagesTruncated = databaseEmailManager.TruncateBodyOutgoingUpgradeEmailOlderThan(3);
				if (messagesTruncated > 0)
				{
					messagesProcessed += messagesTruncated;
					ServiceLogger.Log(LogType.Information, messagesTruncated + " message(s) truncated.");
				}
				else
				{
					ServiceLogger.Log(LogType.Information, "None found.");
				}

				ServiceLogger.Log(LogType.Information, "Checking for old queued with acknowledgement messages to mark as failed...");
				int messagesApplied = databaseEmailManager.SetToFailedQueuedWithAckOutgoingEmailOlderThan(2);
				if (messagesApplied > 0)
				{
					messagesProcessed += messagesApplied;
					ServiceLogger.Log(LogType.Information, messagesApplied + " message(s) marked as failed.");
				}
				else
				{
					ServiceLogger.Log(LogType.Information, "None found.");
				}
			}
			return messagesProcessed;
		}

#if DEBUG
		internal
#endif
		DatabaseEmailManagement GetDatabaseEmailManagement()
		{
			var result = DatabaseEmailManagement.Create();
			result.MaximumBatchSize = 200;
			result.MaximumBatchCount = 5;
			return result;
		}

		protected virtual bool SupportSendingEmailWithAcknowledgement
		{
			get { return ClientHookLoader.Instance.Client == Clients.EDI; }
		}

		#endregion

		void LogProcess(string message)
		{
			try
			{
				ServiceLogger.Information(message + " process " + Process.GetCurrentProcess().Id);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Information(ex.Message + ": " + message);
			}
		}
	}
}
