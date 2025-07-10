using System.Threading;
using Enterprise.BufferManagement.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	SchematicTransferLoopMonitorServiceTask.Code,
	SchematicTransferLoopMonitorServiceTask.Description,
	BMSServiceTaskBase.Category,
	typeof(SchematicTransferLoopMonitorServiceTask),
	MinimumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.BufferManagement.Business
{
	public class SchematicTransferLoopMonitorServiceTask : BMSServiceTaskBase
	{
		public const string Code = "BML";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Schematic Transfer Loop Monitor";

		protected override string TaskDescription
		{
			get { return Description; }
		}

		protected override void RunTaskCore(CancellationToken token)
		{
			RunCore(token);
		}

		void RunCore(CancellationToken token)
		{
			var transferMonitor = new SchematicTransferLoopMonitor(ServiceLogger);
			transferMonitor.Process(token);
		}

		[HostedServiceRequirement]
		public static string CheckSufficientWorkflowModeEnabled()
		{
			return CheckEnhancedWorkflowModeOrBetterEnabledCore();
		}
	}
}
