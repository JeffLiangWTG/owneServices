using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	public class WorkflowTriggerEventLogBatcher : LogBatcher<UserContextSwitcher>
	{
		public WorkflowTriggerEventLogBatcher(INotifications notifications)
		{
			this.notifications = notifications;
			userContextCache = new MRUCache<UserContextTriggerCacheKey, UserContextSwitcher>(10);
		}

		readonly INotifications notifications;
		readonly MRUCache<UserContextTriggerCacheKey, UserContextSwitcher> userContextCache;

		protected override void AddGroupingFetchHints(IEnumerable<IQueuedLog> queuedLogs)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("WorkflowEventTriggerProcessor.AddGroupingFetchHints.ProcessTask"))
			{
				WorkflowEventTriggerProcessor.AddFetchHints(queuedLogs);
			}

			using (PerformanceStatisticsCollector.StartMonitoring("WorkflowEventTriggerProcessor.AddGroupingFetchHints.Trigger"))
			{
				foreach (IQueuedLog log in queuedLogs)
				{
					var trigger = log.LoadTrigger() as ProcessTask;

					if (trigger != null)
					{
						trigger.AddParentFetchHint();
					}
				}
			}

			using (PerformanceStatisticsCollector.StartMonitoring("WorkflowEventTriggerProcessor.AddGroupingFetchHints.JobHeader"))
			{
				foreach (var log in queuedLogs)
				{
					var trigger = log.LoadTrigger();
					if (trigger != null && trigger.GetParentBusinessObject(log) is IJobHeaderParent)
					{
						log.Factory.AddFetchHint(typeof(JobHeader), WorkflowEventTriggerProcessor.GetJobHeaderQuery(trigger));
					}
				}
			}
		}

		protected override UserContextSwitcher GetGroupLogKey(IQueuedLog log)
		{
			var trigger = log.LoadTrigger();

			if (trigger != null) // Trigger may have been deleted since WTE log was created. This is handled intelligently elsewhere but the user context doesn't matter for this case.
			{
				var eventData = new WorkflowTriggerEventData(log);
				var parentID = eventData.TriggeringLogParentPK;
				parentID = parentID.IsValid ? parentID : trigger.ParentID;
				var key = new UserContextTriggerCacheKey(parentID, trigger, eventData);
				if (!userContextCache.TryGetValue(key, out var value))
				{
					var job = trigger.GetParentBusinessObject(log);
					if (job != null) // Job may have been deleted since the WTE log was created. As above.
					{
						value = WorkflowUserContextSwitchCreator.GetTemporaryUserContext(trigger, job, log, notifications);
						userContextCache.SetValue(key, value);
					}
					else
					{
						value = UserContextSwitcher.NullSwitcher;
					}
				}
				return value;
			}

			return UserContextSwitcher.NullSwitcher;
		}

		protected override LogsGroupContext SetContextForLogsGroup(UserContextSwitcher userContextSwitcher, IEnumerable<IQueuedLog> queuedLogs)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("WorkflowEventTriggerProcessor.SetContextForLogsGroup"))
			{
				try
				{
					return new LogsGroupContext(false, userContextSwitcher.Set(notifications));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("WorkflowTriggerEventLogBatcherUserContextError", "Exception thrown whilst trying to set user context.", ex);
					throw;
				}
			}
		}
	}
}
