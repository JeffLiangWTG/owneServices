using System.Diagnostics;
using Enterprise.Integration;
using ServiceManager.DummySleepingService;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	DummySleepingTask.ServiceConfig.Code,
	DummySleepingTask.ServiceConfig.Description,
	DummySleepingTask.ServiceConfig.Category,
	typeof(DummySleepingTask),
	IsMandatory = DummySleepingTask.ServiceConfig.IsMandatory,
	CanRunInAnyBranch = true,
	MinimumPeriod = DummySleepingTask.ServiceConfig.MinimumPeriod,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)]

namespace ServiceManager.DummySleepingService
{
	public class DummySleepingTask : ServiceProviderImpl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		public override void RunTask(CancellationToken cancellationToken)
		{
			const string indentation = "- ";

			ServiceLogger?.Log(LogType.Information, $"RunTask {nameof(DummySleepingTask)}.");

			using var mutex = new Mutex(true, MutexLock, out var createdNew);

			if (!createdNew)
			{
				ServiceLogger?.Log(LogType.Information, $"{indentation}Failed to create mutex lock: '{MutexLock}', completing the task and exit...");
				return;
			}

			try
			{
				mutex.WaitOne();
				ServiceLogger?.Log(LogType.Information, $"{indentation}Acquired a mutex lock: '{MutexLock}' at {DateTime.Now:s}.");

				var logTimeout = TimeSpan.FromSeconds(10);
				var stopwatch = Stopwatch.StartNew();

				while (!cancellationToken.IsCancellationRequested)
				{
					Thread.Sleep(100);
					if (stopwatch.Elapsed > logTimeout)
					{
						ServiceLogger?.Log(LogType.Debug, $"{indentation}Waiting for cancellation to task {nameof(DummySleepingTask)} at {DateTime.Now:s}.");
						stopwatch.Restart();
					}
				}

				ServiceLogger?.Log(LogType.Information, $"{indentation}Cancellation has been requested to task {nameof(DummySleepingTask)} at {DateTime.Now:s}.");
			}
			finally
			{
				mutex.ReleaseMutex();
				ServiceLogger?.Log(LogType.Information, $"{indentation}Released the mutex lock: '{MutexLock}' at {DateTime.Now:s}.");
			}

			ServiceLogger?.Log(LogType.Information, $"{indentation}CompletedTask {nameof(DummySleepingTask)}.");
		}

		public const string MutexLock = nameof(DummySleepingTask);

		public static class ServiceConfig
		{
			public const string Code = "~01";
			public const string Description = "Dummy Sleeping Task";
			public const string Category = "TST";
			public const bool IsMandatory = true;
			public const string MinimumPeriod = "15seconds";
		}
	}
}
