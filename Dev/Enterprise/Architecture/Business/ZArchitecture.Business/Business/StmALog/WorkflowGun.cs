using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class WorkflowGun
	{
		public WorkflowGun(IStmALog log, FireWorkflowMode mode)
		{
			this.log = log;
			this.mode = mode;
		}

		readonly IStmALog log;
		readonly FireWorkflowMode mode;

		public void TriggerWorkflow(IStmALogParent master)
		{
			if (mode == FireWorkflowMode.Suppress || IsAnythingImportantDeleted(master, log))
			{
				return;
			}

			using (ErrorReporter.GatherAdditionalInformation())
			using (PerformanceStatisticsCollector.StartMonitoring("FireWorkflow", log.SL_Table))
			{
				ErrorReporter.CreateInfoFlag(StmALog.ErrorReportKeys.Flags.TrackDeletedStmALogs);

				var categoryKey = StmALog.ErrorReportKeys.Category.WorkflowGun;

				ErrorReporter.SetAdditionalInfo(categoryKey, "log.PK", log.PK.ToString()); // This is a developer error
				ErrorReporter.SetAdditionalInfo(categoryKey, "log.SL_EventTime", log.SL_EventTime.ToBestReadableDateTimeString()); // This is a developer error
				ErrorReporter.SetAdditionalInfo(categoryKey, "log.SL_SE_NKEvent", log.SL_SE_NKEvent.ToString()); // This is a developer error
				ErrorReporter.SetAdditionalInfo(categoryKey, "log.SL_Parent", log.SL_Parent.ToString()); // This is a developer error
				ErrorReporter.SetAdditionalInfo(categoryKey, "log.SL_Reference", log.SL_Reference.ToString()); // This is a developer error
				ErrorReporter.SetAdditionalInfo(categoryKey, "log.SL_IsCancelled", log.SL_IsCancelled.ToString()); // This is a developer error
				ErrorReporter.SetAdditionalInfo(categoryKey, "log.GetType()", log.GetType().ToString()); // This is a developer error

				if (master != null)
				{
					ErrorReporter.SetAdditionalInfo(categoryKey, "master.LogsParentTableName", master.LogsParentTableName); // This is a developer error
					ErrorReporter.SetAdditionalInfo(categoryKey, "master.LogsParentPK", master.LogsParentPK.ToString()); // This is a developer error
					ErrorReporter.SetAdditionalInfo(categoryKey, "master.TableName", master.TableName); // This is a developer error
				}

				if (AreAllKeyFieldsValid(master))
				{
					using (EventRecursionHandler.WithEventRecursionDetection())
					{
						var eventDateUpdater = new UpdateEventDateMediator(log, master);

						if (log.SL_IsCancelled)
						{
							var shouldUpdateRelatedFields = mode != FireWorkflowMode.WithoutUpdatingRelatedFields && !ExistsActiveLogOfSameEventType();
							if (shouldUpdateRelatedFields)
							{
								eventDateUpdater.CancelUpdateEventDateProperty();
							}

							var shouldWithdrawWorkflow = !log.SL_IsEstimate || shouldUpdateRelatedFields; // TODO: Cancelling estimate events should work the same as cancelling actuals.
							if (mode != FireWorkflowMode.UpdateRelatedFields && shouldWithdrawWorkflow)
							{
								GetProcessTaskHandler(log, master).Withdraw();
							}
						}
						else
						{
							if (mode != FireWorkflowMode.WithoutUpdatingRelatedFields)
							{
								eventDateUpdater.PerformUpdateEventDateProperty();
							}
							if (mode != FireWorkflowMode.UpdateRelatedFields)
							{
								GetProcessTaskHandler(log, master).Fire();
							}
							if (master != null && !IsAnythingImportantDeleted(master, log))
							{
								master.ProcessLog(log);
							}
						}
					}
				}
			}
		}

		bool IsAnythingImportantDeleted(IStmALogParent master, IStmALog stmALog)
		{
			return (stmALog is BusinessObject bizoLog && (bizoLog.IsDeleted || bizoLog.IsDeleting)) || (master != null && master.IsDeleted);
		}

		bool ExistsActiveLogOfSameEventType()
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_Parent, log.SL_Parent);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, log.SL_SE_NKEvent);
			query.AddToFilter(StmALogSchema.SL_IsEstimate, log.SL_IsEstimate);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);

			var factory = log.Factory;
			var parent = factory.GetBizOsForPK(log.SL_Parent.ToGuid()).FirstOrDefault();
			query.FetchOnlyFromLocalCache = parent != null && !parent.IsInDatabase;

			var activeLog = factory.LoadTop1<StmALog>(query);
			return (activeLog != null);
		}

		bool AreAllKeyFieldsValid(IStmALogParent master)
		{
			return (master != null || ((log.SL_Table.IsValid && !log.SL_Table.IsEmpty) && (log.SL_Parent.IsValid && !log.SL_Parent.IsEmpty)))
				&& (log.SL_EventTime.IsValid && !log.SL_EventTime.IsEmpty)
				&& (log.SL_SE_NKEvent.IsValid && !log.SL_SE_NKEvent.IsEmpty);
		}

		public static IProcessTaskHandler GetProcessTaskHandler(IStmALog log, IStmALogParent master)
		{
			var handlerProvider = master as ICustomProcessTaskHandlerProvider;
			if (handlerProvider != null)
			{
				return handlerProvider.GetHandler(log);
			}
			else
			{
				var handlerInfoProvider = master as IProcessHandlingInfoProvider;
				if (handlerInfoProvider == null)
				{
					if (master == null || master is IWorkflowProviderCore || master is IWorkflowTriggerEventSource)
					{
						return new ProcessTaskHandler(null, log, master);
					}
					else
					{
						return new NullProcessTaskHandler();
					}
				}
				else
				{
					return new ProcessTaskHandler(handlerInfoProvider.ProcessHandlingInfo, log, master);
				}
			}
		}
	}

	/// <summary>
	/// Suppress should be used when modifying a log should not fire workflow.
	/// UpdateRelatedFields should only be used when cancelling an event should not clear Milestones.
	/// WithoutUpdatingRelatedFields unfires milestones and triggers but does not update related fields.
	/// UpdateAll is the general case; i.e. An event has been created that should fire.
	/// </summary>
	public enum FireWorkflowMode
	{
		Suppress,
		UpdateRelatedFields,
		WithoutUpdatingRelatedFields,
		UpdateAll,
	}
}
