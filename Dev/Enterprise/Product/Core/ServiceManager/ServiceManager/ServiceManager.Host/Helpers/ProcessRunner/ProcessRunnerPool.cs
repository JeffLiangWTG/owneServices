using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using static System.FormattableString;

namespace Enterprise.ServiceManager.Host
{
	public class ProcessRunnerPool : IProcessRunnerPool
	{
		readonly IHostLogger hostLogger;
		readonly ConcurrentDictionary<IServiceRunner, IRunnableServiceTask> runnerToTaskMapping = new ConcurrentDictionary<IServiceRunner, IRunnableServiceTask>();
		readonly Dictionary<string, List<IServiceRunner>> pooledRunnersDictionary = new Dictionary<string, List<IServiceRunner>>(StringComparer.OrdinalIgnoreCase) { { string.Empty, new List<IServiceRunner>() } };
		readonly IBackgroundThreadActionQueue actionQueue;
		readonly INudgingController nudgingController;
		readonly IDisposable checkIdleTimer;
		readonly IDisposable checkBacklogHelpTimer;
		readonly IServiceRunnerFactory serviceRunnerFactory;
		readonly IJobObject jobObject;
		readonly SemaphoreSlim runnerMappingSemaphore;
		static readonly TimeSpan WaitForRunnerDelay = TimeSpan.FromMilliseconds(250);
		readonly IHostRegistrySettings hostRegistry;
		readonly IErrorReporterProxy errorReporter;

		public ProcessRunnerPool(
			IHostLogger hostLogger,
			IBackgroundThreadActionQueueFactory actionQueueFactory,
			IServiceRunnerFactory serviceRunnerFactory,
			IJobObject jobObject,
			IHostRegistrySettings hostRegistry,
			IErrorReporterProxy errorReporter,
			INudgingController nudgingController)
		{
			BusyRunnerWaitTime = TimeSpan.FromSeconds(hostRegistry.BusyRunnerWaitTimeInSeconds);
			IdleProcessCheckSpan = TimeSpan.FromSeconds((double)hostRegistry.ServiceTaskUnloadTimeoutInSeconds / 2);
			IdleProcessExpiry = TimeSpan.FromSeconds(hostRegistry.ServiceTaskUnloadTimeoutInSeconds);
			SecondaryProcessSpinUpDelayTime = TimeSpan.FromSeconds(hostRegistry.SecondaryProcessSpinUpDelayInSeconds);

			this.hostRegistry = hostRegistry;
			this.hostLogger = hostLogger;
			actionQueue = actionQueueFactory.BackgroundThreadActionQueue;
			this.nudgingController = nudgingController;
			checkIdleTimer = actionQueue.CreateTimer(CheckIdleRunners, IdleProcessCheckSpan, IdleProcessCheckSpan);
			var backlogHelpCheckTime = TimeSpan.FromMilliseconds(SecondaryProcessSpinUpDelayTime.TotalMilliseconds / 2);
			checkBacklogHelpTimer = actionQueue.CreateTimer(CheckRunnersRequiringBacklogHelp, backlogHelpCheckTime, backlogHelpCheckTime);
			this.serviceRunnerFactory = serviceRunnerFactory ?? throw new ArgumentNullException(nameof(serviceRunnerFactory));
			this.jobObject = jobObject ?? throw new ArgumentNullException(nameof(jobObject));
			this.errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
			this.runnerMappingSemaphore = new SemaphoreSlim(1, 1);
		}

		public IEnumerable<ServiceTaskCodeWithRunnerProcessId> GetRunnersSnapshot()
		{
			// two ToList enumerations are required for multithreading and results caching
			return runnerToTaskMapping
				.ToArray()
				.Where(x => x.Value.HasSchedule && x.Key.TaskRunning)
				.Select(x => new ServiceTaskCodeWithRunnerProcessId(x.Value.Code, x.Key.ProcessId))
				.ToList();
		}

		public int RunningCount(IRunnableServiceTask task = null)
		{
			// first ToList enumeration is required for multithreaded access
			return runnerToTaskMapping
				.ToArray()
				.Count(pair =>
					(task == null || pair.Value == task)
					&& pair.Key.TaskRunning);
		}

		async Task<IServiceRunner> GetRunnerAsync(CancellationToken cancellationToken, List<IServiceRunner> pooledRunners, SemaphoreSlim semaphore = null)
		{
			await runnerMappingSemaphore.WaitAsync(cancellationToken);
			try
			{
				var matchingRunners = pooledRunners
					.Where(x => !x.IsAllocatingTask)
					.ToList();

				if (matchingRunners.Count == 0)
				{
					hostLogger.Log(LogLevel.Debug, Invariant($"No processes exist"));
					return null;
				}

				// get 'earliest' runner (created first will have more assemblies loaded and be more costly to discard)
				var idleRunner = matchingRunners
					.FirstOrDefault(x => x.IsIdle);
				if (idleRunner != null)
				{
					hostLogger.Log(LogLevel.Debug, Invariant($"Reusing runner [{idleRunner}]"));
					idleRunner.IsAllocatingTask = true;
					return idleRunner;
				}

				var busyRunners = pooledRunners
					.Where(x => x.TaskRunning)
					.Where(x => x.ElapsedFromLastRun < BusyRunnerWaitTime)
					.ToList();
				if (busyRunners.Count == 0)
				{
					hostLogger.Log(LogLevel.Debug, Invariant($"Could not find idle runner"));
					return null;
				}
			}
			finally
			{
				runnerMappingSemaphore.Release(1);
			}

			hostLogger.Log(LogLevel.Debug, "Waiting for runner to become idle");
			var sw = Stopwatch.StartNew();

			while (sw.Elapsed < BusyRunnerWaitTime)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					hostLogger.Log(LogLevel.Debug, Invariant($"Cancellation requested before idle runner could be found"));
					return null;
				}

				await runnerMappingSemaphore.WaitAsync(cancellationToken);
				try
				{
					var idleRunner = pooledRunners
						.Where(x => !x.IsAllocatingTask)
						.FirstOrDefault(x => x.IsIdle);
					if (idleRunner != null)
					{
						hostLogger.Log(LogLevel.Debug, Invariant($"Found idle runner [{idleRunner}]. [Wait time: {sw.Elapsed:hh\\:mm\\:ss\\.fff}]"));
						idleRunner.IsAllocatingTask = true;
						return idleRunner;
					}
				}
				finally
				{
					runnerMappingSemaphore.Release(1);
				}

				await Task.Delay(WaitForRunnerDelay, cancellationToken);
			}

			hostLogger.Log(LogLevel.Debug, $"Waited for idle runner but could not find one after {sw.Elapsed}");
			return null;
		}

		public async Task<IServiceRunner> GetOrCreateRunnerAsync(ITaskRunRequest request, ITaskScheduler taskScheduler, CancellationToken cancellationToken)
		{
			hostLogger.Log(LogLevel.Debug, request.FormatRequestToLogMessage(LogMessageStage.ObtainingRunner));

			var runner = (IServiceRunner)null;
			var (taskGroup, pooledRunners) = GetRunnerPool(request.Task.Code);
			if (pooledRunners != null)
			{
				runner = await GetRunnerAsync(cancellationToken, pooledRunners);
			}

			if (runner == null)
			{
				var newRunner = serviceRunnerFactory.Create(taskScheduler, taskGroup);
				newRunner.ProcessStarted += ProcessStarted;
				runner = newRunner;
				runner.IsAllocatingTask = true;
				await AddRunnerThreadSafeAsync(runner, cancellationToken);
				hostLogger.Log(LogLevel.Debug, request.FormatRequestToLogMessage(LogMessageStage.RunnerIsCreated, runner));
			}
			else
			{
				hostLogger.Log(LogLevel.Debug, request.FormatRequestToLogMessage(LogMessageStage.RunnerIsFound, runner));
			}

			Associate(runner, request.Task);
			runner.TaskRunRequestCompleted += TaskRunRequestCompleted;

			return runner;

			void ProcessStarted(object sender, ProcessStartedEventArgs e)
			{
				var process = e.Process;
				if (process != null)
				{
					jobObject.AddProcess(process.Handle);
				}
			}
		}

		(string taskGroup, List<IServiceRunner>) GetRunnerPool(string taskCode)
		{
			var taskGroup = string.Empty;
			if (hostRegistry.ServiceTaskRunnerSpecificGroup.TryGetValue("ERROR", out var errorMessage))
			{
				hostLogger.Log(LogLevel.Debug, $"Task specific group could not be deserialized from registry, defaulting to no grouping: [{errorMessage}]");
			}
			else
			{
				taskGroup = hostRegistry.ServiceTaskRunnerSpecificGroup.TryGetValue(taskCode, out var group)
					? group
					: string.Empty;
			}

			if (!pooledRunnersDictionary.TryGetValue(taskGroup, out var pooledRunners))
			{
				return (taskGroup, null);
			}

			return (taskGroup, pooledRunners);
		}

		TimeSpan BusyRunnerWaitTime { get; }
		TimeSpan IdleProcessCheckSpan { get; }
		TimeSpan IdleProcessExpiry { get; }
		TimeSpan SecondaryProcessSpinUpDelayTime { get; }

		protected void CheckIdleRunners()
		{
			IEnumerable<IServiceRunner> runnersCache;
			runnerMappingSemaphore.Wait();
			try
			{
				runnersCache = pooledRunnersDictionary.SelectMany(kvp => kvp.Value).ToArray();
			}
			finally
			{
				runnerMappingSemaphore.Release(1);
			}
			foreach (var runner in runnersCache)
			{
				if (!runner.TaskRunning)
				{
					runnerToTaskMapping.TryRemove(runner, out var task);
				}
				runner.CheckIdleStatus(hostLogger, IdleProcessExpiry);
			}
		}

		void CheckRunnersRequiringBacklogHelp()
		{
			var tasksRunning = runnerToTaskMapping
				.Where(rm => rm.Key.TaskRunning)
				.Select(rm => rm.Value)
				.Distinct()
				.ToArray();

			var tasksNeedingHelp = tasksRunning
				.Where(t =>
					t != null
					&& t.MaxSecondaryRunningCount > 0
					&& t.TimeSinceLastStarted.Elapsed >= SecondaryProcessSpinUpDelayTime)
				.ToArray();
			if (tasksNeedingHelp.Length > 0)
			{
				tasksNeedingHelp.ForEach(rt => rt.TimeSinceLastStarted.Restart());
				nudgingController.ReportNudgeStarted(tasksNeedingHelp.Select(t => t.Code), new StackTrace());
				nudgingController.ScheduleTasks(tasksNeedingHelp.Select(t => t.Code), echoes: false);
			}
		}

		internal async Task AddRunnerThreadSafeAsync(IServiceRunner runner, CancellationToken cancellationToken)
		{
			await runnerMappingSemaphore.WaitAsync(cancellationToken);
			try
			{
				Add(runner);
			}
			finally
			{
				runnerMappingSemaphore.Release(1);
			}
		}

		internal void Associate(IServiceRunner runner, IRunnableServiceTask task)
		{
			if (runner.TaskRunning)
			{
				throw new InvalidOperationException($"Attempted to associate task [{task.Code}] with running runner");
			}
			runnerToTaskMapping[runner] = task;
		}

		void TaskRunRequestCompleted(object sender, EventArgs e)
		{
			var runner = (IServiceRunner)sender;
			Dissociate(runner);
		}

		void Dissociate(IServiceRunner runner)
		{
			if (!runner.TaskRunning)
			{
				runnerToTaskMapping.TryRemove(runner, out var task);
			}
		}

		internal void Add(IServiceRunner runner)
		{
			var pooledRunners = pooledRunnersDictionary.GetOrAdd(runner.TaskGroup, () => new List<IServiceRunner>());
			pooledRunners.Add(runner);
			hostLogger.Log(LogLevel.Debug, $"Adding runner to runner pool group [{(runner.TaskGroup.IsNullOrEmpty() ? "default" : runner.TaskGroup)}]. Pool count [{pooledRunners.Count}]. Pooled Runners [{string.Join(",", pooledRunners.Select(r => $"PID={(r.ProcessId == 0 ? "Starting" : r.ProcessId.ToString())}").ToArray())}]");
			runner.Exited += RunnerExited;
		}

		void RunnerExited(object sender, EventArgs e)
		{
			var runner = (IServiceRunner)sender;
			runnerMappingSemaphore.Wait();
			try
			{
				runnerToTaskMapping.TryRemove(runner, out var task);
				if (!pooledRunnersDictionary.TryGetValue(runner.TaskGroup, out var pooledRunners))
				{
					errorReporter.ReportOnce($"Could not find runner for removal from task group pool [{runner.TaskGroup}]");
				}
				else
				{
					pooledRunners.Remove(runner);
					if (pooledRunners.Count == 0)
					{
						pooledRunnersDictionary.Remove(runner.TaskGroup);
						hostLogger.Log(LogLevel.Debug, $"Removing last runner from task Group pool [{runner.TaskGroup}]");
					}
				}
			}
			finally
			{
				runnerMappingSemaphore.Release(1);
			}
			runner.Exited -= RunnerExited;
			runner.Dispose();
		}

		public void StopAllRunners(IRunnableServiceTask task)
		{
			var matchingRunners = runnerToTaskMapping
				.Where(x => x.Value == task)
				.Select(x => x.Key)
				.ToArray();
			foreach (var runner in matchingRunners)
			{
				runner.Stop();
			}
		}

		public void WaitForRunningTasksToComplete(TimeSpan timeout)
		{
			var sw = Stopwatch.StartNew();
			while (pooledRunnersDictionary.SelectMany(kvp => kvp.Value).Any(pr => pr.TaskRunning) && sw.Elapsed < timeout)
			{
				actionQueue.InvokeActionsWhileWaiting(hostRegistry.ServiceTaskProcessingBatchDelay, () => !pooledRunnersDictionary.SelectMany(kvp => kvp.Value).Any(pr => pr.TaskRunning));
			}
		}

		public void Stop(TimeSpan timeout)
		{
			pooledRunnersDictionary
				.SelectMany(kvp => kvp.Value)
				.ToList()
				.ForEach(r => r.Stop());
			checkIdleTimer.Dispose();
			checkBacklogHelpTimer.Dispose();

			var timeoutSW = Stopwatch.StartNew();
			var logSW = Stopwatch.StartNew();
			var logFrequency = TimeSpan.FromSeconds(2);
			while (pooledRunnersDictionary.SelectMany(kvp => kvp.Value).Any() && timeoutSW.Elapsed < timeout)
			{
				if (logSW.Elapsed > logFrequency)
				{
					foreach (var runner in pooledRunnersDictionary.SelectMany(kvp => kvp.Value))
					{
						hostLogger.Log(LogLevel.Debug, Invariant($"Waiting for completion of Runner [{runner}]"));
					}
					logSW.Restart();
				}
				actionQueue.InvokeActionsWhileWaiting(hostRegistry.ServiceTaskProcessingBatchDelay, () => pooledRunnersDictionary.SelectMany(kvp => kvp.Value).Any());
			}

			foreach (var runner in pooledRunnersDictionary.SelectMany(kvp => kvp.Value))
			{
				runner.Kill();
			}
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (disposing)
			{
				checkIdleTimer.Dispose();
				checkBacklogHelpTimer.Dispose();

				foreach (var runner in pooledRunnersDictionary.SelectMany(kvp => kvp.Value))
				{
					runner.Exited -= RunnerExited;
					runner.TaskRunRequestCompleted -= TaskRunRequestCompleted;
					runner.Dispose();
				}
				runnerToTaskMapping.Clear();
				pooledRunnersDictionary.Clear();
			}
		}

		#endregion
	}
}
