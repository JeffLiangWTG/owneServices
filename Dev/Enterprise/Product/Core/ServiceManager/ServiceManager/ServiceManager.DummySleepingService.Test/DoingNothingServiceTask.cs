using Enterprise.Integration;
using ServiceManager.DummyServiceTasks.Test;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	DoingNothingServiceTask.ServiceConfig.Code,
	DoingNothingServiceTask.ServiceConfig.Description,
	DoingNothingServiceTask.ServiceConfig.Category,
	typeof(DoingNothingServiceTask),
	AllowsMultipleInstances = DoingNothingServiceTask.ServiceConfig.AllowsMultipleInstances,
	CanRunInAnyBranch = true,
	MinimumPeriod = DoingNothingServiceTask.ServiceConfig.MinimumPeriod,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)]

namespace ServiceManager.DummyServiceTasks.Test;

public class DoingNothingServiceTask : ServiceProviderImpl
{
	public override void RunTask(CancellationToken cancellationToken)
	{
		const string indentation = "- ";

		ServiceLogger?.Log(LogType.Information, $"RunTask {nameof(DoingNothingServiceTask)}.");

		Thread.Sleep(TimeSpan.FromSeconds(2));

		ServiceLogger?.Log(LogType.Information, $"{indentation}CompletedTask {nameof(DoingNothingServiceTask)}.");
	}

	public static class ServiceConfig
	{
		public const string Code = "~DN";
		public const string Description = "Doing Nothing Service Task";
		public const string Category = "TST";
		public const bool AllowsMultipleInstances = false;
		public const string MinimumPeriod = "15seconds";
	}
}
