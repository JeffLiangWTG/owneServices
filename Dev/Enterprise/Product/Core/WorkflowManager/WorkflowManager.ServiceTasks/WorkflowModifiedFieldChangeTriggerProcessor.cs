using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	[Serializable]
	class WorkflowModifiedFieldChangeTriggerProcessor : IProcessor
	{
		#region SuppressResourceStringsCheckRegion

		#region Process

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			Notifications = notifications ?? new NotificationCollection();
			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				if (SystemDataRegistry.Instance.WorkflowFieldChangeTriggersEnabled.Value)
				{
					var from = HighWaterMark;
					var to = ZDateTime.UtcNow.AddMinutes(-1);
					to = new ZDateTime(to.Year, to.Month, to.Day, to.Hour, to.Minute, to.Second);

					ProcessCore(Notifications, from, to, token);
				}
				var stmChangeLogPurger = new StmChangeLogPurger(Db.Connection);
				stmChangeLogPurger.Purge(token);
			}
		}

		protected virtual TriggeringBusinessObjectFactoryProvider GetFactoryProvider(INotifications notifications) => new TriggeringBusinessObjectFactoryProvider(this, notifications);

		void ProcessCore(INotifications notifications, ZDateTime from, ZDateTime to, CancellationToken token)
		{
			var query = NewChangeLogQuery(from, to);
			var factoryProvider = GetFactoryProvider(notifications);
			factoryProvider.Current.RefreshEnabled = false;
			var reader = new FilteredBusinessObjectReader(factoryProvider, query, typeof(StmChangeLog));
			notifications.Add(new InfoNotification(string.Format(CultureInfo.InvariantCulture, "Processing {0} change log(s)", reader.ApproximateCount)));
			var batch = reader.LoadNextBatchInANewFactory(ZGuid.Empty);

			while (batch.Any())
			{
				try
				{
					foreach (StmChangeLog changeLog in batch)
					{
						ProcessLog(notifications, factoryProvider, changeLog);
					}

					factoryProvider.SaveCurrentAndUpdateRecordCounts();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					notifications.Add(new WarningNotification(string.Format(CultureInfo.InvariantCulture,
						 "Error happened during saving changes to database. Retrying change logs one by one.\r\n\r\n{0}", ex.Message)));

					ReprocessCore(notifications, batch, token);
				}

				batch = reader.LoadNextBatchInANewFactory(batch[batch.Length - 1]);
			}

			notifications.Add(new InfoNotification("Finishing cycle"));
		}

		void ReprocessCore(INotifications notifications, BusinessObject[] changeLogs, CancellationToken token)
		{
			notifications.Add(new InfoNotification(string.Format(CultureInfo.InvariantCulture, "Reprocessing {0} change log(s)", changeLogs.Length)));

			var factoryProvider = GetFactoryProvider(notifications);
			foreach (StmChangeLog changeLogToLoad in changeLogs)
			{
				token.ThrowIfCancellationRequested();
				try
				{
					ZExceptionReporting.ProcessWithConcurrencyHandling(
						action: () =>
						{
							factoryProvider.CreateNewWithoutSave();
							var changeLog = factoryProvider.Current.Load<StmChangeLog>(changeLogToLoad.PK);
							if (changeLog != null)
							{
								ProcessLog(notifications, factoryProvider, changeLog);
								factoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
							}
							factoryProvider.SaveCurrentAndUpdateRecordCounts();
						},
						onRetry: () => notifications.Add(new WarningNotification(FormattableString.Invariant($"Retrying for error while processing log {changeLogToLoad.PK}."))),
						retries: 3);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					notifications.Add(new WarningNotification(FormattableString.Invariant($"Unhandled error occurred while processing log {changeLogToLoad.PK}.\r\n{ex}")));
					// Breaking the loop means logs go unprocessed which would be pretty bad.
					ErrorReporter.ReportOnce("WorkflowModifiedFieldChangeTriggerProcessor", FormattableString.Invariant($"Unhandlable exception while processing log {changeLogToLoad.PK}"), ex);
				}
			}
		}

		protected virtual void ProcessLog(INotifications notifications, TriggeringBusinessObjectFactoryProvider factoryProvider, StmChangeLog changeLog)
		{
			var tasks = GetTriggersToRun(changeLog);
			AddTriggersCountNotification(notifications, tasks, changeLog);

			factoryProvider.AddTriggersToRun(changeLog, tasks);
			factoryProvider.LastChangeLogProcessed = changeLog;
		}

		void AddTriggersCountNotification(INotifications notifications, IBaseTrigger[] triggers, StmChangeLog changeLog)
		{
			var sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendLine();
			int countTriggers = 0;
			int countMilestones = 0;
			foreach (var task in triggers)
			{
				sb.AppendLine(task.GetRecordID());
				if (task.IsTrigger())
				{
					countTriggers++;
				}
				else if (task.IsMilestone())
				{
					countMilestones++;
				}
			}
			var triggersPart = countTriggers > 0 ? string.Format(CultureInfo.InvariantCulture, "{0} trigger(s)", countTriggers) : "";
			var milestonesPart = countMilestones > 0 ? string.Format(CultureInfo.InvariantCulture, "{0} milestone(s)", countMilestones) : "";
			if (countTriggers > 0 && countMilestones > 0)
			{
				milestonesPart = ", " + milestonesPart;
			}
			sb.Insert(0, string.Format(CultureInfo.InvariantCulture, "Processing {0}{1} for change log {2}", triggersPart, milestonesPart, changeLog.PK.ToString()));
			notifications.Add(new InfoNotification(sb.ToString()));
		}

		ZQuery NewChangeLogQuery(ZDateTime from, ZDateTime to)
		{
			var query = new ZQuery();
			if (from.IsValid)
			{
				query.AddToFilter(StmChangeLogSchema.SY_PostedTimeUtc, SQLComparisonOperator.GreaterThan, from);
			}
			query.AddToFilter(StmChangeLogSchema.SY_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, to);
			query.OrderBy = StmChangeLogSchema.SY_PostedTimeUtc.Name;
			query.IsNoLock = false;
			return query;
		}

		#endregion

		#region GetTriggersToRun

		IWorkflowTrigger[] GetTriggersToRun(StmChangeLog changeLog)
		{
			var result = Array.Empty<IWorkflowTrigger>();

			string[] changedPropertyNames = new List<string>(GetChangedPropertyNames(changeLog)).ToArray();
			if (changedPropertyNames.Length > 0)
			{
				var job = changeLog.Parent;
				IWorkflowModifiedFieldChangeTriggerProcessorExtender extender;
				ExtendersDictionary.TryGetValue(changeLog.SY_ParentTableCode, out extender);
				result = (extender != null)
					? extender.GetTriggersToRun(changeLog, changedPropertyNames)
					: GetTriggersToRun(job, changedPropertyNames);

				if (result.Length > 0)
				{
					// Do not update already actioned milestones
					result = result.Where(trigger =>
					{
						var triggerParent = trigger.GetJob();
						return triggerParent != null && TriggerConditionEvaluator.AreTriggerConditionsMet(triggerParent, trigger, changeLog)
								&& (!trigger.IsMilestone() || !((ProcessTask)trigger).P9_ActualDate.IsValid);
					}).ToArray();
				}
			}

			return result;
		}

		Dictionary<string, IWorkflowModifiedFieldChangeTriggerProcessorExtender> ExtendersDictionary
		{
			get
			{
				if (extendersDictionary == null)
				{
					Hashtable objectHandleHashtable = (Hashtable)ObjectFactory.Get("WorkflowModifiedFieldChangeTriggerProcessorExtenders");
					extendersDictionary = new Dictionary<string, IWorkflowModifiedFieldChangeTriggerProcessorExtender>(objectHandleHashtable.Count);
					foreach (DictionaryEntry entry in objectHandleHashtable)
					{
						ObjectHandle handle = (ObjectHandle)entry.Value;
						extendersDictionary.Add((string)entry.Key, (IWorkflowModifiedFieldChangeTriggerProcessorExtender)handle.GetObject());
					}
				}
				return extendersDictionary;
			}
		}

		Dictionary<string, IWorkflowModifiedFieldChangeTriggerProcessorExtender> extendersDictionary;

		#endregion

		#region TriggeringBusinessObjectFactoryProvider class

		protected class TriggeringBusinessObjectFactoryProvider : BusinessObjectFactoryProvider
		{
			public TriggeringBusinessObjectFactoryProvider(WorkflowModifiedFieldChangeTriggerProcessor owner, INotifications notifications)
			{
				this.owner = owner;
				this.notifications = notifications;
			}

			public void AddTriggersToRun(StmChangeLog changeLog, IWorkflowTrigger[] newTriggersToRun)
			{
				TriggerGroupings.Add(changeLog, newTriggersToRun);
			}

			public readonly WorkflowFieldChangeTriggerList TriggerGroupings = new WorkflowFieldChangeTriggerList();
			public StmChangeLog LastChangeLogProcessed;
			readonly WorkflowModifiedFieldChangeTriggerProcessor owner;
			readonly INotifications notifications;

			protected override void SaveCurrent()
			{
				if (LastChangeLogProcessed != null)
				{
					foreach (var group in TriggerGroupings)
					{
						var log = group.ChangeLog;
						foreach (var trigger in group.Triggers)
						{
							var job = trigger.GetJob();
							using (WorkflowUserContextSwitchCreator.GetTemporaryUserContext(trigger, job, log, notifications).Set(notifications))
							{
								var logDate = GetDateForTrigger(log, trigger, job);
								trigger.TrySetActualDateForEvent(log, job, logDate);
							}
						}
					}
					owner.HighWaterMark = LastChangeLogProcessed.SY_PostedTimeUtc.AddMilliseconds(-1);
					TriggerGroupings.Clear();
					LastChangeLogProcessed = null;
				}

				base.SaveCurrent();
			}

			static ZDateTimeOffset GetDateForTrigger(StmChangeLog log, IWorkflowTrigger trigger, BusinessObject job)
			{
				var homePort = TriggerProvider.GetBranchForTemporaryUserContext(trigger, job, Lazy.Create(() => trigger.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, log.SY_GS_NKUser)))?.HomePort;
				return homePort != null ? log.SY_PostedTimeUtc.ToLocationTime(homePort) : log.SY_PostedTimeUtc.ToLocalBranchTimeOffset(trigger.Factory);
			}
		}

		#endregion

		#region GetTriggersToRun

		static IWorkflowTrigger[] GetTriggersToRun(BusinessObject businessObject, string[] changedPropertyNames)
		{
			var workflowProviders = GetWorkflowProviders(businessObject);

			if (workflowProviders.Length > 0)
			{
				var factory = ((BusinessObject)workflowProviders[0]).Factory;
				var parentPKs = workflowProviders.Select(w => w.Identifier).ToArray();

				var processTaskQuery = new ZQuery();
				processTaskQuery.AddToFilter(ProcessTasksSchema.P9_TriggerField, changedPropertyNames);
				processTaskQuery.AddToFilter(ProcessTasksSchema.P9_ParentID, parentPKs);
				var processTasks = factory.Load<ProcessTask>(processTaskQuery);

				// TODO in WI00127805: add support for Universal Triggers.

				return processTasks;
			}
			else
			{
				return Array.Empty<IWorkflowTrigger>();
			}
		}

		#endregion

		#region Implementation

		static IWorkflowProvider[] GetWorkflowProviders(BusinessObject businessObject)
		{
			IWorkflowTriggerFieldChangeSource fieldChangeSource = businessObject as IWorkflowTriggerFieldChangeSource;
			IWorkflowProvider workflowProvider = businessObject as IWorkflowProvider;
			List<IWorkflowProvider> result = new List<IWorkflowProvider>();
			if (workflowProvider != null)
			{
				result.Add(workflowProvider);
			}

			if (fieldChangeSource != null)
			{
				result.AddRange(fieldChangeSource.ParentWorkflowProviders);
			}

			return result.ToArray();
		}

		IEnumerable<string> GetChangedPropertyNames(StmChangeLog changeLog)
		{
			foreach (StmFieldChangeLog fieldChangeLog in changeLog.FieldChanges)
			{
				yield return fieldChangeLog.PropertyName;
				if (TriggerFieldDependencies.ContainsKey(fieldChangeLog.PropertyName))
				{
					yield return TriggerFieldDependencies[fieldChangeLog.PropertyName];
				}
			}
		}

		protected Dictionary<string, string> TriggerFieldDependencies
		{
			get
			{
				if (propertyDependencies == null)
				{
					propertyDependencies = new Dictionary<string, string>();
					propertyDependencies.Add(JobVoyOriginSchema.Constants.JA_E_DEP, JobConsolTransportSchema.JW_ETD.Name);
					propertyDependencies.Add(JobVoyOriginSchema.Constants.JA_A_DEP, JobConsolTransportSchema.JW_ATD.Name);
					propertyDependencies.Add(JobVoyOriginSchema.Constants.JA_RL_NKPortOfLoading, JobConsolTransportSchema.JW_RL_NKLoadPort.Name);
					propertyDependencies.Add(JobVoyDestinationSchema.Constants.JB_E_ARV, JobConsolTransportSchema.JW_ETA.Name);
					propertyDependencies.Add(JobVoyDestinationSchema.Constants.JB_A_ARV, JobConsolTransportSchema.JW_ATA.Name);
					propertyDependencies.Add(JobVoyDestinationSchema.Constants.JB_RL_NKPortOfDischarge, JobConsolTransportSchema.JW_RL_NKDiscPort.Name);
					propertyDependencies.Add(JobVoyageSchema.Constants.JV_RV_NKVessel, JobConsolTransportSchema.JW_Vessel.Name);
					propertyDependencies.Add(JobVoyageSchema.Constants.JV_VoyageFlight, JobConsolTransportSchema.JW_VoyageFlight.Name);
				}

				return propertyDependencies;
			}
		}
		Dictionary<string, string> propertyDependencies;

		ZDateTime HighWaterMark
		{
			get
			{
				if (!highWaterMark.IsValid)
				{
					DateTime value = SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
					highWaterMark = (value == DateTime.MinValue) ? ZDateTime.Empty : new ZDateTime(value);
				}
				return highWaterMark;
			}
			set
			{
				SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToDateTime());
				highWaterMark = value;
			}
		}

		[NonSerialized]
		ZDateTime highWaterMark;

		#endregion

		INotifications Notifications;

		#endregion
	}
}
