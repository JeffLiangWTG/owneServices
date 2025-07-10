using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.LogWalker
{
	sealed class RecurringLogHandler : IAfterOnSavingBOProcessingService, IAfterSaveInTransactionService, IDisposable
	{
		#region Constructor

		internal RecurringLogHandler(ProcessableLogGroup currentLogGroup, ILogger logger, LogSubscriber[] subscribers, int maximumDepth)
		{
			this.currentLogGroup = currentLogGroup;
			this.logger = logger;
			this.subscribers = subscribers;
			this.maximumDepth = maximumDepth;
			allowLogRecursion = maximumDepth > 0;
			recurringGroups = new List<SubscribersToConvertIntoProcessableLogGroup>();
			currentLogGroup.Factory.ServiceContainer.AddAfterOnSavingService(this);
			currentLogGroup.Factory.ServiceContainer.AddAfterSaveInTransactionService(this);
		}

		bool succeeded;
		readonly bool allowLogRecursion;
		readonly ProcessableLogGroup currentLogGroup;
		readonly LogSubscriber[] subscribers;
		readonly ILogger logger;
		RecurringLogTracker.RecurringLogsResult recurringLogs;
		RecurringLogTracker.RecurringLogsResult delayedRecurringLogs;
		readonly List<SubscribersToConvertIntoProcessableLogGroup> recurringGroups;
		readonly int maximumDepth;

		#endregion

		#region Public API

		public void MarkAsSucceeded()
		{
			succeeded = true;
		}

		public IEnumerable<SubscribersToConvertIntoProcessableLogGroup> GetNewGroups()
		{
			if (delayedRecurringLogs != null)
			{
				recurringGroups.AddRange(GetRecurringLogGroups(delayedRecurringLogs));
				currentLogGroup.Factory.Save();
			}
			return recurringGroups;
		}

		#endregion

		#region Interfaces

		void IDisposable.Dispose()
		{
			currentLogGroup.Factory.ServiceContainer.RemoveAfterOnSavingService<RecurringLogHandler>();
			currentLogGroup.Factory.ServiceContainer.RemoveAfterSaveInTransactionService<RecurringLogHandler>();

			if (!succeeded)
			{
				foreach (var group in recurringGroups)
				{
					group.Dispose();
				}
			}
		}

		void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			if (allowLogRecursion)
			{
				Db.Connection.ExecuteNonQuery("EXEC dbo.SuspendTrigger 'TG_CP_INS_StmALogQueue'");
			}

			recurringLogs = GetRecurringLogs();
			recurringGroups.AddRange(GetRecurringLogGroups(recurringLogs));
		}

		void IAfterSaveInTransactionService.DoFinalCheckBeforeCommit(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			if (allowLogRecursion)
			{
				Db.Connection.ExecuteNonQuery("EXEC dbo.ResumeTrigger 'TG_CP_INS_StmALogQueue'");
			}

			delayedRecurringLogs = GetDelayedRecurringLogs();
		}

		#endregion

		#region Impl

		RecurringLogTracker.RecurringLogsResult GetRecurringLogs()
		{
			RecurringLogTracker.RecurringLogsResult recurringLogs;
			var newLogs = new HashSet<StmALog>();
			do
			{
				recurringLogs = currentLogGroup.RecursionTracker.GetRecurringLogs(currentLogGroup.Factory);
				recurringLogs.NewLogs.ForEach(l => newLogs.Add(l));

				if (recurringLogs.DuplicateLogs.Count > 0)
				{
					logger.Log(LogType.Warning, new DuplicateLogDeleter().DeleteAndGetMessage(recurringLogs.DuplicateLogs));
				}
			} while (recurringLogs.DuplicateLogs.Count > 0);

			return recurringLogs;
		}

		RecurringLogTracker.RecurringLogsResult GetDelayedRecurringLogs()
		{
			var allLogs = currentLogGroup.RecursionTracker.GetRecurringLogs(currentLogGroup.Factory);
			if (allLogs.NewLogs != null)
			{
				var delayed = allLogs.NewLogs.Except(recurringLogs.NewLogs);
				if (delayed.Any() && maximumDepth > currentLogGroup.Depth)
				{
					return new RecurringLogTracker.RecurringLogsResult(delayed.ToList().AsReadOnly(), null);
				}
			}
			return null;
		}

		IEnumerable<SubscribersToConvertIntoProcessableLogGroup> GetRecurringLogGroups(RecurringLogTracker.RecurringLogsResult recurringLogs)
		{
			if (recurringLogs.NewLogs.Count > 0)
			{
				if (maximumDepth > currentLogGroup.Depth)
				{
					return CreateRecurringProcessableLogs(recurringLogs);
				}
				else if (maximumDepth > 0)
				{
					logger.Log(LogType.Warning, new LogRecursionLimitExceededDeleter().DeleteAndGetMessage(recurringLogs.NewLogs));
				}
			}

			return Enumerable.Empty<SubscribersToConvertIntoProcessableLogGroup>();
		}

		IEnumerable<SubscribersToConvertIntoProcessableLogGroup> CreateRecurringProcessableLogs(RecurringLogTracker.RecurringLogsResult recurringLogs)
		{
			var result = new List<SubscribersToConvertIntoProcessableLogGroup>();
			foreach (var recurringLogGroup in recurringLogs.NewLogs.Where(l => !l.SL_FireWorkflow).GroupBy(l => (EventCode: l.SL_SE_NKEvent.ToString(), TableName: l.SL_Table.ToString())))
			{
				foreach (var logGroupSubscriber in subscribers)
				{
					if (logGroupSubscriber.EventTypes.Contains(recurringLogGroup.Key.EventCode)
						&& (logGroupSubscriber.TableNames.Contains(recurringLogGroup.Key.TableName)))
					{
						var lockedQueues = NewsTransmitter.CreatLogQueueItems(recurringLogGroup.First().Factory, recurringLogGroup, logGroupSubscriber).ApplyAppLocks(NewsTransmitter.GetApplockKey(logGroupSubscriber), NewsTransmitter.GetApplockColumns());
						result.Add(new SubscribersToConvertIntoProcessableLogGroup(recurringLogGroup, lockedQueues, logGroupSubscriber));
					}
				}
			}

			var asyncLogSubscriber = subscribers.FirstOrDefault(l => l.Name == "TasksAndMilestonesLoader");
			if (asyncLogSubscriber != null)
			{
				var delayedLogs = recurringLogs.NewLogs.Where(l => l.SL_FireWorkflow).ToArray();
				if (delayedLogs.Any())
				{
					var lockedQueues = NewsTransmitter.CreatLogQueueItems(delayedLogs.First().Factory, delayedLogs, asyncLogSubscriber).ApplyAppLocks(NewsTransmitter.GetApplockKey(asyncLogSubscriber), NewsTransmitter.GetApplockColumns());
					result.Add(new SubscribersToConvertIntoProcessableLogGroup(delayedLogs, lockedQueues, asyncLogSubscriber));
				}
			}

			return result;
		}

		#endregion
	}

	#region Aux Classes

	public sealed class SubscribersToConvertIntoProcessableLogGroup : IDisposable
	{
		public SubscribersToConvertIntoProcessableLogGroup(IEnumerable<StmALog> stmaLogs, LoadWithAppLockResult<IQueuedLog> lockedQueues, LogSubscriber subscriber)
		{
			StmALogs = stmaLogs;
			LockedQueues = lockedQueues;
			Subscriber = subscriber;
		}
		internal IEnumerable<StmALog> StmALogs { get; }
		internal LoadWithAppLockResult<IQueuedLog> LockedQueues { get; }
		internal LogSubscriber Subscriber { get; }

		public void Dispose() => LockedQueues.Dispose();
	}

	abstract class LogDeleter
	{
		internal string DeleteAndGetMessage(IEnumerable<StmALog> duplicateLogs)
		{
			var addedFeedbackLogMessage = new ZStringBuilder();
			foreach (StmALog log in duplicateLogs.OrderBy(l => l.SL_SE_NKEvent))
			{
				if (!log.IsDeleted)
				{
					addedFeedbackLogMessage.Append(GetMessageForLog(log));
					log.Delete();
				}
			}

			return AggregateMessage(addedFeedbackLogMessage);
		}

		protected abstract string GetMessageForLog(StmALog log);
		protected abstract string AggregateMessage(ZStringBuilder stringBuilder);
	}

	class DuplicateLogDeleter : LogDeleter
	{
		protected override string AggregateMessage(ZStringBuilder stringBuilder)
		{
			return Res.GetString("663f8325-2bd3-46ed-bb88-2bce78744f45", "attempted to add the following logs which are duplicates:\r\n{0}", stringBuilder.ToStringWithNewLineBetweenAppends());
		}

		protected override string GetMessageForLog(StmALog log)
		{
			return Res.GetString("04e65eec-60ff-4b94-8a18-10f99176bf67", "Table: {0}, Event: {1}, Parent PK: {2}, Reference: {3}", log.SL_Table, log.SL_SE_NKEvent, log.SL_Parent, log.SL_Reference);
		}
	}

	class LogRecursionLimitExceededDeleter : LogDeleter
	{
		protected override string AggregateMessage(ZStringBuilder stringBuilder)
		{
			return Res.GetString("b6abd829-2790-41f9-b38c-f814330a3c78", "attempted to add the following logs which could cause infinite feedback:\r\n{0}", stringBuilder.ToStringWithNewLineBetweenAppends());
		}

		protected override string GetMessageForLog(StmALog log)
		{
			return Res.GetString("c246cbca-e774-4f3d-9719-ef50709ac80f", "Table: {0}, Event: {1}, Parent PK: {2}", log.SL_Table, log.SL_SE_NKEvent, log.SL_Parent);
		}
	}

	#endregion
}
