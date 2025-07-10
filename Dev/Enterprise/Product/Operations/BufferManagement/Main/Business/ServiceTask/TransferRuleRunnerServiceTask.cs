using System;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	TransferRuleRunnerServiceTask.Code,
	TransferRuleRunnerServiceTask.Description,
	BMSServiceTaskBase.Category,
	typeof(TransferRuleRunnerServiceTask),
	MinimumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.BufferManagement.Business
{
	public class TransferRuleRunnerServiceTask : SystemSchematicServiceTaskBase
	{
		public const string Code = "BMS";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Buffer Management Schematic Transfer Runner";

		protected readonly TransferRuleRunnerParams transferRuleRunnerParams;

		public TransferRuleRunnerServiceTask()
		{
			transferRuleRunnerParams = new TransferRuleRunnerParams();
		}

		protected override string TaskDescription
		{
			get { return Description; }
		}

		bool processAllTransferRulesLinksOnNextBMSRun;

		protected override void OnSystemsLoaded()
		{
			processAllTransferRulesLinksOnNextBMSRun = BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.Value;
			base.OnSystemsLoaded();
		}

		protected override IPAVEProcessor GetProcessor(BMSystem system, ReleaseGateLogger releaseGateLogger)
		{
			var logger = new TransferRuleRunnerLogger(ServiceLogger, system);
			var dataAccessor = new TransferRuleRunnerDataAccessor(logger);

			return new TransferRuleRunner(system, dataAccessor, logger, transferRuleRunnerParams);
		}

		protected override void OnSystemsProcessed()
		{
			if (processAllTransferRulesLinksOnNextBMSRun)
			{
				BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}
		}

		[HostedServiceRequirement]
		public static string CheckSufficientWorkflowModeEnabled()
		{
			return CheckEnhancedWorkflowModeOrBetterEnabledCore();
		}

		protected override bool IsSufficientWorkflowManagementModeEnabled => BMSRegistryProvider.IsEnhancedWorkflowManagementOrBetterEnabled;
	}
}
