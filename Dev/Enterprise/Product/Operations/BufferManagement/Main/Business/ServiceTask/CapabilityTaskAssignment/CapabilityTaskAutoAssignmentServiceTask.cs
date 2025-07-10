using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CapabilityTaskAutoAssignmentServiceTask.Code,
	CapabilityTaskAutoAssignmentServiceTask.Description,
	BMSServiceTaskBase.Category,
	typeof(CapabilityTaskAutoAssignmentServiceTask),
	MinimumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "30minutes",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.BufferManagement.Business
{
	public class CapabilityTaskAutoAssignmentServiceTask : BMSServiceTaskBase
	{
		public const string Code = "BMT";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Capability Task Auto-Assignment";

		protected override string TaskDescription => Description;

		[HostedServiceRequirement]
		public static string CheckCapacityCalculationsNotDisabled()
		{
			return CheckCapacityCalculationsNotDisabledCore();
		}

		[HostedServiceRequirement]
		public static string CheckSufficientWorkflowModeEnabled()
		{
			return CheckBufferManagementWorkflowModeOrBetterEnabledCore();
		}

		protected override bool ShouldNotRunIfCapacityCalculationsDisabled => true;

		protected override bool IsSufficientWorkflowManagementModeEnabled => BMSRegistryProvider.IsBufferManagementWorkflowModeOrBetterEnabled;

		protected override void RunTaskCore(CancellationToken token)
		{
			var componentQuery = new ZQuery(BMComponentSchema.FC_AutoAssignTasksAge, SQLComparisonOperator.NotEqual, null);
			componentQuery.AddToFilter(BMComponentSchema.FC_IsActive, true);
			componentQuery.AddToFilter(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.Buffer);

			var buffers = new BusinessObjectFactory { NameForDebugging = "CapabilityTaskAutoAssignmentServiceTask.Components" }.Load<BMComponent>(componentQuery);
			var buffersByBranch = buffers.GroupBy(x => x.FC_GB_AgingBranch);

			foreach (var buffersForBranch in buffersByBranch)
			{
				using (Environment.DisposableEnvironment.ForBranch(buffersForBranch.Key.ToGuid()))
				{
					foreach (var buffer in buffersForBranch)
					{
						token.ThrowIfCancellationRequested();
						ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Auto-assigning tasks in component [{0}]: [{1}].", buffer.System.FS_Name, buffer.FC_Name)); // Service task logging

						var dataAccessor = GetDataAccessor();
						var assigner = GetWorkflowCapabilityAssigner();
						assigner.AutoAssignWorkflowsInBatchesImmediately(buffer, ServiceLogger, dataAccessor, Description);
					}
				}
			}
		}

		protected virtual WorkflowCapabilityAssigner GetWorkflowCapabilityAssigner() => new WorkflowCapabilityAssigner();

		protected virtual IWorkflowCapabilityAssignerDataAccessor GetDataAccessor() => new WorkflowCapabilityAssignerDataAccessor(ServiceLogger);
	}
}
