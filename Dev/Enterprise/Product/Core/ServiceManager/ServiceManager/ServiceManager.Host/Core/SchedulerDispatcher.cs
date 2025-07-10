using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public class SchedulerDispatcher : ISchedulerDispatcher
	{
		public SchedulerDispatcher(
			IAllTasksConsumer allTasks,
			ITaskSelector taskSelector,
			ITaskRunRequestProcessor runner,
			IHostLogger logger,
			IServiceHostsCache serviceHostsCache,
			IProcessRunnerPool processRunnerPool,
			ITaskScheduler taskScheduler,
			IBackgroundThreadActionQueue actionQueue,
			CancellationToken cancellationToken,
			IErrorReporterProxy errorReporterProxy,
			IHostRegistrySettings hostRegistry,
			IResourceThrottler resourceThrottler)
		{
			this.allTasks = allTasks ?? throw new ArgumentNullException(nameof(allTasks));
			this.taskSelector = taskSelector ?? throw new ArgumentNullException(nameof(taskSelector));
			this.taskRunner = runner ?? throw new ArgumentNullException(nameof(runner));
			this.cancellationToken = cancellationToken;
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.serviceHostsCache = serviceHostsCache ?? throw new ArgumentNullException(nameof(serviceHostsCache));
			this.resourceThrottler = resourceThrottler ?? throw new ArgumentNullException(nameof(resourceThrottler));
			this.processRunnerPool = processRunnerPool ?? throw new ArgumentNullException(nameof(processRunnerPool));
			this.taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(taskScheduler));
			this.actionQueue = actionQueue ?? throw new ArgumentNullException(nameof(actionQueue));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
			this.hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
		}

		public void Schedule(ITaskQueue taskQueue)
		{
			// enqueue all eligible tasks
			var tasksToEnqueue = taskSelector.SelectTasksToRun(allTasks.GetAll());
			foreach (var task in tasksToEnqueue)
			{
				taskQueue.EnqueueTask(task);
			}
		}

		public void Dispatch(ITaskQueue taskQueue)
		{
			var scalingFactor = hostRegistry.ServiceTaskProcessingBatchSizeScalingFactor;
			var maximumBatchSize = hostRegistry.ServiceTaskProcessingMaximumBatchSize;
			var dispatchingSize = taskQueue.GetQueueSnapshot().Count();
			var taskRunRequestLogMessages = new List<string>();
			var batchSize = 1.0d;
			while (dispatchingSize > 0 && !cancellationToken.IsCancellationRequested)
			{
				var waitResult = resourceThrottler.WaitForResource();
				if (waitResult.TimedOut)
				{
					logger.Log(LogLevel.Warning, $"Starting dispatch round delayed due to resource contention. {waitResult}");
					return;
				}

				var batchSizeForIteration = (int)Math.Min(Math.Floor(batchSize), maximumBatchSize);
				DispatchTaskBatch(taskQueue, ref dispatchingSize, taskRunRequestLogMessages, batchSizeForIteration);
				if (batchSize < maximumBatchSize)
				{
					batchSize *= (double)scalingFactor;
				}
			}

			if (taskRunRequestLogMessages.Count > 0)
			{
				logger.Log(LogLevel.Information, $"Requests dispatched to runners: {string.Join(", ", taskRunRequestLogMessages)}.");
			}
			else
			{
				logger.Log(LogLevel.Debug, "No requests dispatched to runners.");
			}
		}

		void DispatchTaskBatch(ITaskQueue taskQueue, ref int dispatchingSize, List<string> taskRunRequestLogMessages, int batchSize)
		{
			var (dispatchingQueue, tasksToAwait) = GetDispatchQueue(taskQueue, batchSize, ref dispatchingSize);
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					var (requestToRunTask, runner) = InvokeActionsWhileWaitingForRunner(dispatchingQueue, tasksToAwait);
					if (runner == null)
					{
						break;
					}

					if (requestToRunTask is IDirectTaskRunRequest)
					{
						ProcessRunRequest(requestToRunTask, runner);
						taskRunRequestLogMessages.Add($"{requestToRunTask.Task.Code}/{requestToRunTask.Id}");
						continue;
					}

					if (requestToRunTask.Task.IsOverdue)
					{
						logger.Log(LogLevel.Information, requestToRunTask.FormatRequestToLogMessage(LogMessageStage.RequestIsOverDue));
						requestToRunTask.Task.SetNextRunTimeBasedOnRecurrence();
						continue;
					}

					if (serviceHostsCache.ConfiguredServiceHosts.Count() <= 1)
					{
						requestToRunTask.Task.SetNextRunTimeBasedOnRecurrence();
						ProcessRunRequest(requestToRunTask, runner);
						taskRunRequestLogMessages.Add($"{requestToRunTask.Task.Code}/{requestToRunTask.Id}");
					}
					else
					{
						var nextRunTimeIsInFuture = true;
						var result = Db.Connection.RunLocked(string.Format(CultureInfo.InvariantCulture, "SchedulerDispatcher:Dispatch:UpdateNextRunTime({0})", requestToRunTask.Task.Code), (isFirstAttempt) =>
						{
							requestToRunTask.Task.UpdateStatus();
							nextRunTimeIsInFuture = requestToRunTask.Task.NextRunTimeIsInFuture;
							if (!nextRunTimeIsInFuture)
							{
								requestToRunTask.Task.SetNextRunTimeBasedOnRecurrence();
								ProcessRunRequest(requestToRunTask, runner);
							}
						}, max_tries: 1);

						switch (result)
						{
							case LockedProcessResult.Error:
								requestToRunTask.OnUnableToRun(UnableToRunReason.LockToUpdateNextRunTimeFailed, retry: true, failedPostScheduleUpdate: false);
								break;

							case LockedProcessResult.AlreadyBeingProcessed:
								requestToRunTask.OnUnableToRun(UnableToRunReason.OtherHostIsSchedulingTheTask, retry: true, failedPostScheduleUpdate: false);
								break;

							case LockedProcessResult.Completed:
								if (nextRunTimeIsInFuture)
								{
									//The other host already successfully scheduled this task, clear any local scheduling error.
									requestToRunTask.Task.ClearFailedRunAttempt();
									taskRunRequestLogMessages.Add($"{requestToRunTask.Task.Code}/{requestToRunTask.Id}");
								}

								break;

							default:
								throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Handling for result {0} from Db.Connection.RunLocked has not been implemented", result));
						}
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					errorReporterProxy.ReportOnce("Task dispatch failures", e);
				}
			}

			try
			{
				Task.WaitAll(tasksToAwait, cancellationToken);
			}
			catch (Exception e) when (!e.FlattenInnerExceptions().Any(e => e is ICriticalException))
			{
				errorReporterProxy.ReportOnce("Failure to retrieve runner", e);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:ConcurrentQueueAccess", Justification = "Retrieval is performed in synchronous code, injection in asynchronous, only this loop can modify the dictionary at this point")]
			(ITaskRunRequest requestToRunTask, IServiceRunner runner) InvokeActionsWhileWaitingForRunner(
				ConcurrentQueue<(ITaskRunRequest request, IServiceRunner runner)> runnerQueue,
				Task[] runnerCreationTasks)
			{
				(ITaskRunRequest task, IServiceRunner runner) taskAndRunner;
				while (!runnerQueue.TryDequeue(out taskAndRunner))
				{
					if (cancellationToken.IsCancellationRequested
						|| runnerCreationTasks.All(task => task.IsCompleted))
					{
						if (runnerQueue.TryDequeue(out taskAndRunner)) // Just in case the last task finished after the first TryDequeue
						{
							break;
						}
						return (null, null);
					}
					actionQueue.InvokeActionsWhileWaiting(hostRegistry.ServiceTaskProcessingBatchDelay, () => !runnerQueue.IsEmpty);
				}

				return taskAndRunner;
			}
		}

		(ConcurrentQueue<(ITaskRunRequest request, IServiceRunner runner)>, Task[] tasksToAwait) GetDispatchQueue(ITaskQueue taskQueue, int batchSize, ref int dispatchingSize)
		{
			var tasksToAwait = new List<Task>();
			var dispatchingThisRound = Math.Min(
				dispatchingSize,
				batchSize);
			var readyRunners = new ConcurrentQueue<(ITaskRunRequest request, IServiceRunner runner)>();
			var originalDispatchingSize = dispatchingThisRound;
			var dispatchedTasks = new List<string>();
			while (--dispatchingThisRound >= 0 && !cancellationToken.IsCancellationRequested)
			{
				--dispatchingSize;
				if (taskQueue.TryDequeueTask(out var request))
				{
					dispatchedTasks.Add($"[{request.Task.Code}/{request.Id}]");
					tasksToAwait.Add(Task.Run(async () =>
						{
							try
							{
								var runner = await processRunnerPool.GetOrCreateRunnerAsync(request, taskScheduler, cancellationToken);
								readyRunners.Enqueue((request, runner));
							}
							catch
							{
								logger.Log(LogLevel.Information, request.Task?.Info?.HostedServiceAttribute, "Task failed to allocate to runner, re-enqueueing request");
								taskQueue.EnqueueTask(request);
								throw;
							}
						},
						cancellationToken));
				}
				else
				{
					break;
				}
			}

			logger.Log(LogLevel.Debug, $"Dispatching {originalDispatchingSize} tasks in batch: ({string.Join(", ", dispatchedTasks)})");
			return (readyRunners, tasksToAwait.ToArray());
		}

		void ProcessRunRequest(ITaskRunRequest requestToRunTask, IServiceRunner runner)
		{
			(requestToRunTask as IScheduledTaskRunRequest)?.TakeNextRunTimeFromTask();

			var result = taskRunner.ProcessRunRequest(requestToRunTask, runner);
			switch (result)
			{
				case TaskRunRequestResult.Success:
				case TaskRunRequestResult.TaskIsDisabled:
					requestToRunTask.OnSuccessfulRunAttempt();
					break;
				case TaskRunRequestResult.ConfigurationError:
					requestToRunTask.OnUnableToRun(UnableToRunReason.ConfigurationError, retry: false, failedPostScheduleUpdate: true);
					break;
				case TaskRunRequestResult.Inactive:
					requestToRunTask.OnUnableToRun(UnableToRunReason.TaskIsInactive, retry: false, failedPostScheduleUpdate: true);
					break;
				case TaskRunRequestResult.ProcessDidNotStart:
					requestToRunTask.OnUnableToRun(UnableToRunReason.RunnerProcessDidNotStart, retry: true, failedPostScheduleUpdate: true);
					break;
				case TaskRunRequestResult.TaskAlreadyCompleted:
					requestToRunTask.OnUnableToRun(UnableToRunReason.TaskAlreadyCompleted, retry: false, failedPostScheduleUpdate: true);
					break;
				default:
					throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Handling for result {0} from ProcessRunRequest has not been implemented", result));
			}
		}

		readonly IAllTasksConsumer allTasks;
		readonly ITaskSelector taskSelector;
		readonly ITaskRunRequestProcessor taskRunner;
		readonly CancellationToken cancellationToken;
		readonly IHostLogger logger;
		readonly IServiceHostsCache serviceHostsCache;
		readonly IResourceThrottler resourceThrottler;
		readonly IProcessRunnerPool processRunnerPool;
		readonly ITaskScheduler taskScheduler;
		readonly IBackgroundThreadActionQueue actionQueue;
		readonly IErrorReporterProxy errorReporterProxy;
		readonly IHostRegistrySettings hostRegistry;
	}
}
