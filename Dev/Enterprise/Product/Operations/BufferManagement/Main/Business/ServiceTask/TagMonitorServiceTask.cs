using System.Threading;
using Enterprise.BufferManagement.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	TagMonitorServiceTask.Code,
	TagMonitorServiceTask.Description,
	BMSServiceTaskBase.Category,
	typeof(TagMonitorServiceTask),
	MinimumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.BufferManagement.Business
{
	public class TagMonitorServiceTask : BMSServiceTaskBase
	{
		public const string Code = "BMM";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Tag Rule Monitor";

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
			var ruleMonitor = new TagRuleMonitor(ServiceLogger);
			ruleMonitor.Process(token);
		}

		[HostedServiceRequirement]
		public static string CheckSufficientWorkflowModeEnabled()
		{
			return CheckBufferManagementWorkflowModeOrBetterEnabledCore();
		}
	}
}
