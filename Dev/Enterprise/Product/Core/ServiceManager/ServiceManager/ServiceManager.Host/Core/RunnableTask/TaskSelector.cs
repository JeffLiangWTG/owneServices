using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class TaskSelector : ITaskSelector
	{
		readonly IProcessRunnerPool runnerPool;
		readonly IHostLogger hostLogger;

		public TaskSelector(IProcessRunnerPool runnerPool, IHostLogger hostLogger)
		{
			this.runnerPool = runnerPool ?? throw new ArgumentNullException(nameof(runnerPool));
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
		}

		public IEnumerable<ITaskRunRequest> SelectTasksToRun(IEnumerable<IRunnableServiceTask> tasks)
		{
			var tasksToRun = new List<ITaskRunRequest>();

			var tasksEligibleForRunning = GetTasksPotentiallyEligibleForRunning(tasks).ToList();
			tasksToRun.AddRange(GetTasksToRunOnSchedule(tasksEligibleForRunning, hostLogger));

			return tasksToRun.Distinct().ToList();
		}

		public IEnumerable<IRunnableServiceTask> GetTasksPotentiallyEligibleForRunning(IEnumerable<IRunnableServiceTask> tasks)
		{
			return tasks.Where(IsEligibleForRunning);
		}

		public bool ReachedMaxSecondary(IRunnableServiceTask task)
		{
			return runnerPool.RunningCount(task) > task.MaxSecondaryRunningCount;
		}

		bool IsEligibleForRunning(IRunnableServiceTask task)
		{
			var isEligible = task.IsActive && !ReachedMaxSecondary(task);
			return isEligible;
		}

		static IEnumerable<ITaskRunRequest> GetTasksToRunOnSchedule(IEnumerable<IRunnableServiceTask> tasksEligibleForRunning, IHostLogger hostLogger)
		{
			var scheduledRequestCount = 0;
			using (hostLogger.LogSection(LogLevel.Debug,
				$"Creating Scheduled run requests for startable tasks.",
				() => $"{scheduledRequestCount} Scheduled run requests for startable tasks are created and enqueued."))
			{
				var scheduledRequests = tasksEligibleForRunning
					.Where(task => task.NextRunTimeAllowingForLocalSchedulingFailures <= ZDateTimeOffset.UtcNow.ToDateTimeOffset())
					.Select(rt =>
					{
						var newRequest = new ScheduledTaskRunRequest(rt);
						hostLogger.Log(LogLevel.Debug, newRequest.FormatRequestToLogMessage(LogMessageStage.RequestIsCreated));
						return newRequest;
					})
					.OrderBy(tr => tr.Task.NextRunTimeAllowingForLocalSchedulingFailures)
					.ToList();
				scheduledRequestCount = scheduledRequests.Count;
				return scheduledRequests;
			}
		}
	}
}

