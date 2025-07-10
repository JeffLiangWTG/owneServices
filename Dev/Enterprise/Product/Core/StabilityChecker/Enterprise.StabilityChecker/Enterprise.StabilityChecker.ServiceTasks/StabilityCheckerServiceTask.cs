using System.Globalization;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"STB",
	"System Stability Checker",
	"SYS",
	typeof(Enterprise.StabilityChecker.ServiceTasks.StabilityCheckerServiceTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	MaximumPeriod = "1day",
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true)
]
namespace Enterprise.StabilityChecker.ServiceTasks
{
	sealed class StabilityCheckerServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			StabilityResults results = StabilityChecker.CalculateStabilityResults();
			StabilityChecker.StoreStabilityResults(results);
			StabilityChecker.NotifyUsersOfStabilityResultsIfRequired(results);
			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Stability check completed, {0} issues detected.", results.Results.Count));
		}
	}
}
