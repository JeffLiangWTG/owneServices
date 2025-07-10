using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	[Serializable]
	public class TasksAndMilestonesLoader : LogSubscriber
	{
		#region LogSubscriber Overrides

		public override string[] EventTypes
		{
			get { return new[] { Events.TasksAndMilestonesLoaderEvent.Code }; }
		}

		public override string Name => SubscriberName;

		const string SubscriberName = "TasksAndMilestonesLoader";

		public override string[] TableNames => tableNames ?? (tableNames = WorkflowSupportableTableNames.Instance.GetTableNames().Concat(AdditionalWorkflowSupportableTables).Distinct().ToArray());

		string[] tableNames;

		string[] AdditionalWorkflowSupportableTables => new[]
		{
			DtbConsignmentRunSheetInstructionSchema.Constants.TableName,
			DtbConsignmentVariationSchema.Constants.TableName,
			HVLVItemSchema.Constants.TableName,
			PkgPackageSchema.Constants.TableName,
		};

		HashSet<string> SuppressTemplateApplicationOnSaveForEventsFromTables { get; } = new HashSet<string>
		{
			ProcessHeaderSchema.Constants.Prefix
		};

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var factory = queuedLogs.First().Factory;
			using (Logs.FiringDelayedWorkflow())
			{
				foreach (var log in queuedLogs)
				{
					var bizo = factory.Load(log.SJ_ParentTableCode, log.SJ_ParentID);
					if (bizo is IWorkflowProvider workflowProvider && !workflowProvider.WorkflowType.IsEmpty)
					{
						workflowProvider.ApplyWorkflowTemplates();
					}

					if (log is IStmALog trigger && bizo is IStmALogParent parent)
					{
						new WorkflowGun(trigger, FireWorkflowMode.UpdateAll).TriggerWorkflow(parent);
					}
				}
			}

			if (queuedLogs.Any(l => SuppressTemplateApplicationOnSaveForEventsFromTables.Contains(l.SJ_ParentTableCode)))
			{
				// We're preventing template application from the weird company contexts that tend to be assosciated with these events.
				factory.SubscribeForDispose(ProcessTask.Loader.SuppressTemplateApplication());
			}
		}

		protected override ILogBatcher GetLogBatcher() => new UserContextSwitchingLogBatcher(GetINotificationsWrapperAroundILogger(), SubscriberName);

		#endregion
	}
}

