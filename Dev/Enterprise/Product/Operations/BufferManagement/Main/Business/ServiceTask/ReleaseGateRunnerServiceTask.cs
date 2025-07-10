using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ReleaseGateRunnerServiceTask.Code,
	ReleaseGateRunnerServiceTask.Description,
	BMSServiceTaskBase.Category,
	typeof(ReleaseGateRunnerServiceTask),
	MinimumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.BufferManagement.Business
{
	public class ReleaseGateRunnerServiceTask : SystemSchematicServiceTaskBase
	{
		public const string Code = "BMG";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Release Gate Runner";

		protected override string TaskDescription
		{
			get { return Description; }
		}

		protected override IPAVEProcessor GetProcessor(BMSystem system, ReleaseGateLogger releaseGateLogger)
		{
			return new ReleaseGateDirector(system, ServiceLogger, releaseGateLogger: releaseGateLogger);
		}

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
	}
}
