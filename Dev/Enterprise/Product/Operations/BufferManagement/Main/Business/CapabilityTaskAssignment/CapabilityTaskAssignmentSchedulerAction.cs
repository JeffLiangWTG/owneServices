using System;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.Integration;

[assembly: SchedulerAction(
		CapabilityTaskAssignmentSchedulerAction.Code,
		CapabilityTaskAssignmentSchedulerAction.Description,
		typeof(CapabilityTaskAssignmentSchedulerAction))]
namespace Enterprise.BufferManagement.Business
{
	public class CapabilityTaskAssignmentSchedulerAction : ISchedulerAction
	{
		public const string Code = "ACT";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "used for service task logging")]
		public const string Description = "Capability Task Auto-Assignment";

		public const string CapacityCalculationsDisabledResult = "CapacityCalculationsDisabled"; // Execution code

		public string Execute(CancellationToken token, BusinessObjectFactory factory, ILogger logger, ZGuid targetPk, string targetCode, string parameter, ZDateTime systemCreateTimeUtc)
		{
			if (!BMSRegistry.Instance.DisableCapacityCalculations.Value)
			{
				new WorkflowCapabilityAssigner().AutoAssignWorkflowsImmediatelyOrDelayed(factory, new Guid[] { targetPk.ToGuid() }, logger);
				return Constants.SchedulerActionSuccessResult;
			}
			else
			{
				logger.Log(LogType.Information, $"{Description} not run because the [Disable Capacity Calculations] registry item is enabled.");
				return CapacityCalculationsDisabledResult;
			}
		}
	}
}
