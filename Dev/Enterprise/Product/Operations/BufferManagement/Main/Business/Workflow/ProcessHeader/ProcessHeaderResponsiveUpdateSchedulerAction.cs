using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Schema;

[assembly: SchedulerAction(
		ProcessHeaderResponsiveUpdateSchedulerAction.Code,
		ProcessHeaderResponsiveUpdateSchedulerAction.Description,
		typeof(ProcessHeaderResponsiveUpdateSchedulerAction))]
namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderResponsiveUpdateSchedulerAction : ISchedulerAction
	{
		public const string Code = ProcessHeaderResponsiveActionConstants.ProcessHeaderResponsiveUpdateSchedulerActionCode;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "used for service task logging")]
		public const string Description = "Workflow Responsive Update";

		public string Execute(CancellationToken token, BusinessObjectFactory factory, ILogger logger, ZGuid targetPk, string targetCode, string parameter, ZDateTime systemCreateTimeUtc)
		{
			if (BMSRegistry.Instance.WorkflowManagementMode.Value == WorkflowManagementModes.Codes.BasicWorkflow
				|| !BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.Value
				|| !BMSRegistry.Instance.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges.Value)
			{
				logger.Log(LogType.Debug, "Cancelled processing as disabled in the registry");
				return SchedulerActionCancelledResult;
			}

			var component = factory.Load<BMComponent>(targetPk);

			if (component == null)
			{
				logger.Log(LogType.Debug, $"Cancelled processing as component (PK = {targetPk}) does not exist");
				return SchedulerActionCancelledResult;
			}

			logger.Log(LogType.Information, $"Performing {parameter} responsive action for component {component.FC_Name} (PK = {targetPk}):");

			var query = new ZQuery(ProcessHeaderSchema.FH_FC_CurrentComponent, targetPk);

			if (parameter == ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer)
			{
				// when we update solely dedicated buffers, we can ignore inactive workflows - their dedicated buffers will stay reset anyway
				query.AddToFilter(ProcessHeaderSchema.FH_IsActive, true);
			}

			// we don't need to recalculated dedicated buffers on closed workflows
			// we also don't need to recalculate effective branches and departments on closed workflows:
			// we assume that closed workflows cannot be normally situated in a buffer and therefore we don't need their buffer penetration (which are based on effective buffer and department)
			query.AddToFilter(ProcessHeaderSchema.FH_Status, SQLComparisonOperator.NotEqual, WorkflowStatusList.Codes.Closed);
			query.AddToFilter(ProcessHeaderSchema.FH_Status, SQLComparisonOperator.NotEqual, WorkflowStatusList.Codes.ClosedWithOpenPrerequisites);

			var performer = new ProcessHeaderResponsiveActionPerformer(logger, query, parameter);
			performer.PerformActionOnMultipleWorkflows(token);
			return Constants.SchedulerActionSuccessResult;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "used for service task logging")]
		const string SchedulerActionCancelledResult = "Cancelled";
	}
}
