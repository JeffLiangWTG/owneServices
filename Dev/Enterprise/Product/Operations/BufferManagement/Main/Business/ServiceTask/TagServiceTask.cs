using System;
using System.Threading;
using Enterprise.BufferManagement.Business;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	TagServiceTask.Code,
	TagServiceTask.Description,
	BMSServiceTaskBase.Category,
	typeof(TagServiceTask),
	MinimumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	DefaultScheduleRunEvery = "15minutes",
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.BufferManagement.Business
{
	public class TagServiceTask : BMSServiceTaskBase, IServiceTaskConfigurationUser
	{
		public const string Code = "TAG";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Tag Service Task";

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
			var shouldRunAllRules = ShouldRunAllRules();
			var ruleRunner = new TagRuleRunner(ServiceLogger) { ShouldRunAllRules = shouldRunAllRules };
			ruleRunner.Process(token);
		}

		bool ShouldRunAllRules()
		{
			var configString = ((IServiceTaskConfigurationUser)this).ConfigString;

			if (string.IsNullOrEmpty(configString))
			{
				return false;
			}

			var stringToCompare = configString.Replace(":", "").Trim();

			return string.Compare(stringToCompare, "ALL", StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		string IServiceTaskConfigurationUser.ConfigString { get; set; }

		[HostedServiceRequirement]
		public static string CheckSufficientWorkflowModeEnabled()
		{
			return CheckBufferManagementWorkflowModeOrBetterEnabledCore();
		}
	}
}
