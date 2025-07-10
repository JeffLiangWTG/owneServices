using Enterprise.LogWalker;
using Enterprise.ServiceManager.Tasks.LogWalker;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(LogWalkerPurgeServiceTask.CODE, "Event Log Walker Purge Service", "SYS", typeof(LogWalkerPurgeServiceTask),
	AllowsMultipleInstances = false,
	IsMandatory = true,
	MinimumPeriod = "1minute",
	MaximumPeriod = "1day",
	DefaultScheduleRunEvery = "5minutes",
	ActiveByDefault = true)]
namespace Enterprise.ServiceManager.Tasks.LogWalker
{
	public class LogWalkerPurgeServiceTask : LogWalkerServiceTaskBase
	{
		public const string CODE = "LWP";

		protected override LogWalkerRunner Runner => runner ?? (runner = LogWalkerRunner.Purge());
		LogWalkerRunner runner;
	}
}
