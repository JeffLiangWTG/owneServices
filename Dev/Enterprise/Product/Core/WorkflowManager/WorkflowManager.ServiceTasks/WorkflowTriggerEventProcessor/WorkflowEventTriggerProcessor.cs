using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	[Serializable]
	public class WorkflowEventTriggerProcessor : LogSubscriber
	{
		#region Subscriber properties

		public override string Name => ProcessTask.WorkflowEventTriggerJobQueueName;

		public override string[] EventTypes => new string[] { Events.WorkflowTriggerEvent.Code };

		public override string[] TableNames => new[] { ProcessTasksSchema.Constants.TableName, ProcessJobTriggerLinkSchema.Constants.TableName };

		public override PrettyPrinter GetPrettyPrinter() => new WorkflowTriggerEventPrettyPrinter(Name);

		#endregion

		#region Logs Grouping

		protected override ILogBatcher GetLogBatcher() => new WorkflowTriggerEventLogBatcher(GetINotificationsWrapperAroundILogger());

		#endregion

		#region Process Logs

		IWorkflowEventTriggerConfig Config
		{
			get
			{
				if (config == null)
				{
					config = WorkflowEventTriggerConfig.Create();
				}
				return config;
			}
		}
		IWorkflowEventTriggerConfig config;

		protected override ILogger SetDefaultLoggerCore(ILogger loggerToSet)
		{
			return new WorkflowEventTriggerCategoryLogger(loggerToSet);
		}

		protected virtual BusinessObjectFactory GetFactory(IQueuedLog queuedLog)
		{
			return queuedLog.Factory;
		}

		protected override LogSubscriberResult ProcessBatch(IQueuedLog[] queuedLogs)
		{
			var logger = DefaultLogger as ICategoryLogger<WorkflowEventTriggerCategories>;

			Argument.NotNull(logger, nameof(logger));
			Argument.NotNull(queuedLogs, nameof(queuedLogs));

			var visitedWTELogs = new HashSet<(ZGuid, ZGuid)>();
			var parents = AddFetchHintsAndGetAllParentPKs(queuedLogs);
			var processedCount = 0;

			if (queuedLogs.Length > 0)
			{
				var factory = queuedLogs[0].Factory;
				using (ServiceContainerSuspenderHelper.GetDocManagerInfoNewFactorySuspender(factory))
				{
					foreach (var queuedLog in queuedLogs)
					{
						processedCount++;
						if (logger.ShouldLog(WorkflowEventTriggerCategories.DelayedLog))
						{
							var lagTime = ZDateTime.UtcNow - queuedLog.SJ_PostedTimeUtc;
							if (lagTime > Config.LogAllowedLag)
							{
								logger.Log(WorkflowEventTriggerCategories.DelayedLog, LogType.Information, FormattableString.Invariant($"Log has been delayed by {lagTime.TotalSeconds} seconds. {queuedLog.PrintBasic()}"));
							}
						}
						var trigger = GetTrigger(queuedLog);
						var eventData = new WorkflowTriggerEventData(queuedLog);
						// And now we handle the million reasons why the event should not fire.
						if (trigger == null)
						{
							var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Did not fire event [{0}] as trigger could not be found.", queuedLog.PrintBasic());
							logger.Log(LogType.Warning, message);
						}
						else if (eventData.TriggeringLogPK != ZGuid.Empty && !visitedWTELogs.Add((queuedLog.SJ_ParentID, eventData.TriggeringLogPK)))
						{
							// In theory this should never happen. It was observed that a single event caused the same trigger to fire twice, and this is supposed to protect us from that.
							var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Did not fire event [{0}] as there was a duplicate.", trigger.HumanReadableName);
							logger.Log(LogType.Warning, message);
						}
						else if (trigger.LastFiredTime.IsEmpty)
						{
							// The log was probably cancelled.
							var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Did not fire event as [{0}] has empty event date.", trigger.HumanReadableName);
							logger.Log(LogType.Warning, message);
						}
						else if (!(parents.TryGetValue(trigger.Identifier, out var parentCount) && parentCount > 0))
						{
							// This should only happen if a trigger's parent was deleted after we loaded the trigger.
							var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Did not fire event as there was a wacky race condition for [{0}].", trigger.HumanReadableName);
							logger.Log(LogType.Warning, message);
						}
						else
						{
							var notifications = new NotificationProxy(logger);

							LogReferenceNumber(trigger, notifications);
							var parent = trigger.GetParentBusinessObject(queuedLog);

							if (trigger.IsLineTriggerEvent(queuedLog))
							{
								logger.Log(LogType.Information, FormattableString.Invariant($"Line trigger with line: [{parent?.HumanReadableName}]"));
							}

							RestrictTemplateApplicationScope(parent as IWorkflowProvider);

							var saves = BusinessObjectFactory.GlobalSaveCount;
							RunTriggerActions(parent, trigger, queuedLog, notifications);
							parents[trigger.Identifier]--;
							if (saves < BusinessObjectFactory.GlobalSaveCount && processedCount < queuedLogs.Length)
							{
								// We yield to escape from concurrency related issues.
								logger.Log(LogType.Information, FormattableString.Invariant($"yielding batch due to save. Logs remaining in batch: [{queuedLogs.Length - processedCount}]"));
								return new LogSubscriberResult(queuedLogs.Take(processedCount).ToList(), queuedLogs.Skip(processedCount).ToList());
							}
						}
					}
				}
			}

			return new LogSubscriberResult(queuedLogs, Array.Empty<IQueuedLog>());
		}

		void RestrictTemplateApplicationScope(IWorkflowProvider parent)
		{
			parent?.SetWorkflowTemplateScopeToMessage();
		}

		void LogReferenceNumber(IBaseTrigger trigger, NotificationProxy notifications)
		{
			notifications.Add(
				CargoWise.EntityFramework.NotificationType.Information,
				Res.GetString("97A54424-53E9-4D29-ACE7-9F3113694E73", "Processing: {0}", trigger.GetDiagnosticLogInfo()));
		}

		IWorkflowTrigger GetTrigger(IQueuedLog log)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("WorkflowEventTriggerProcessor.GetTrigger"))
			{
				return log.LoadTrigger();
			}
		}

		void RunTriggerActions(BusinessObject job, IWorkflowTrigger trigger, IQueuedLog queuedLog, INotifications notifications)
		{
			try
			{
				using (PerformanceStatisticsCollector.StartMonitoring("WorkflowEventTriggerProcessor.RunTriggerAction"))
				{
					new WorkflowTriggerActionManager(notifications).Run(job, trigger, queuedLog);
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException() || ExceptionVisibilityAttribute.Evaluate(ex) != ExceptionVisibility.User)
				{
					throw;
				}

				WorkflowFailureHandler.HandleTriggerFailure(trigger, ex);
			}
		}

		#region Fetch Hints, Triggers, and Related Parties

		Dictionary<ZGuid, int> AddFetchHintsAndGetAllParentPKs(IQueuedLog[] queuedLogs)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("WorkflowEventTriggerProcessor.AddFetchHintsAndGetAllParentPKs"))
			{
				var parents = new Dictionary<ZGuid, int>();

				foreach (var queuedLog in queuedLogs)
				{
					Argument.NotNull(queuedLog, nameof(queuedLog));
					int count;
					parents.TryGetValue(queuedLog.SJ_ParentID, out count);
					parents[queuedLog.SJ_ParentID] = count + 1;
				}

				AddFetchHints(queuedLogs);

				foreach (var queuedLog in queuedLogs)
				{
					var trigger = queuedLog.LoadTrigger();
					if (trigger != null)
					{
						(trigger as ProcessTask)?.AddParentFetchHint();
					}
				}

				foreach (IQueuedLog queuedLog in queuedLogs)
				{
					var trigger = queuedLog.LoadTrigger();
					var parent = trigger?.GetParentBusinessObject(queuedLog);
					if (trigger != null && parent is IJobHeaderParent)
					{
						GetFactory(queuedLog).AddFetchHint(typeof(JobHeader), GetJobHeaderQuery(trigger));
					}
				}

				return parents;
			}
		}

		public static ZQuery GetJobHeaderQuery(IBaseTrigger trigger)
		{
			return new ZQuery(JobHeaderSchema.JH_ParentID, trigger.ParentID)
				.AddToFilter(JobHeaderSchema.JH_GC, trigger.CompanyPK);
		}

		internal static void AddFetchHints(IEnumerable<IQueuedLog> queuedLogs)
		{
			AddFetchHints(queuedLogs, ProcessTasksSchema.Constants.Prefix, typeof(ProcessTask));
			AddFetchHints(queuedLogs, ProcessJobTriggerLinkSchema.Constants.Prefix, ObjectFactory.GetType<IProcessJobTriggerLink>());
		}

		static void AddFetchHints(IEnumerable<IQueuedLog> queuedLogs, string tablePrefix, Type fetchHintBizoType)
		{
			foreach (var log in queuedLogs.Where(l => l.SJ_ParentTableCode == tablePrefix))
			{
				log.Factory.AddFetchHint(fetchHintBizoType, log.SJ_ParentID);
			}
		}

		#endregion

		#endregion

		#region NotificationProxy

		class NotificationProxy : INotifications
		{
			public NotificationProxy(ICategoryLogger<WorkflowEventTriggerCategories> logger)
			{
				this.logger = logger;
			}

			#region INotifications Members

			public void Add(INotification notification)
			{
				logger.Log(notification.Type.ToLogType(), notification.Message);
			}

			#endregion

			readonly ICategoryLogger<WorkflowEventTriggerCategories> logger;
		}

		#endregion

		protected override ZQuery GetFeedbackLogFilter(IQueuedLog[] queuedLogs)
		{
			var result = base.GetFeedbackLogFilter(queuedLogs);
			result.AddToFilter(StmALogSchema.SL_Parent, queuedLogs.Select(o => o.SJ_ParentID).Distinct());
			return result;
		}
	}
}
