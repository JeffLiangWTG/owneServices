using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class TaskQueue : ITaskQueue
	{
		readonly IProcessRunnerPool runnerPool;
		readonly IHostLogger hostLogger;
		readonly List<ITaskRunRequest> queue;
		readonly object queueLockObj;

		public TaskQueue(IProcessRunnerPool runnerPool, IHostLogger hostLogger)
		{
			this.runnerPool = runnerPool ?? throw new ArgumentNullException(nameof(runnerPool));
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.queue = new List<ITaskRunRequest>();
			queueLockObj = new object();
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Second access is guarded against race conditions by the TryDeque")]
		public bool TryDequeueTask(out ITaskRunRequest dequeued)
		{
			lock (queueLockObj)
			{
				dequeued = null;

				foreach (var request in queue)
				{
					if (runnerPool.RunningCount(request.Task) <= request.Task.MaxSecondaryRunningCount)
					{
						queue.Remove(request);
						dequeued = request;
						break;
					}
				}
			}

			if (dequeued != null)
			{
				dequeued.Task.TimeSinceLastDequeued.Restart();
				hostLogger.Log(LogLevel.Debug, dequeued.FormatRequestToLogMessage(LogMessageStage.DequeuedRequest));
				return true;
			}

			return false;
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
		public bool EnqueueTask(ITaskRunRequest request)
		{
			lock (queueLockObj)
			{
				var directRequest = request as IDirectTaskRunRequest;
				var index = queue
					.FindIndex(taskRunRequest => taskRunRequest.Task.Equals(request.Task)
												&& taskRunRequest.GetType() == request.GetType());

				if (index >= 0)
				{
					var existingRequest = queue[index];
					if (directRequest != null
						&& directRequest.HasMoreRetriesRemained((IDirectTaskRunRequest)existingRequest))
					{
						queue[index] = request;
						hostLogger.Log(LogLevel.Debug, request.FormatRequestToLogMessage(LogMessageStage.ReplacesRequest, existingRequest));
					}
					else
					{
						hostLogger.Log(LogLevel.Debug, request.FormatRequestToLogMessage(LogMessageStage.AbsorbedByAnotherRequest, existingRequest));
					}

					return false;
				}

				if (directRequest != null
					&& !directRequest.HasRunsRemaining)
				{
					hostLogger.Log(LogLevel.Information, request.FormatRequestToLogMessage(LogMessageStage.NoRunsAttemptsRemaining));
					return false;
				}

				queue.Add(request);
				request.Task.TimeSinceLastEnqueued.Restart();
				hostLogger.Log(LogLevel.Debug, request.FormatRequestToLogMessage(LogMessageStage.EnqueuedRequest));
				return true;
			}
		}

		public void EmptyQueue()
		{
			lock (queueLockObj)
			{
				queue.Clear();
			}

			hostLogger.Log(LogLevel.Debug, "Emptied task queue");
		}

		public IEnumerable<IRunnableServiceTask> GetQueueSnapshot()
		{
			lock (queueLockObj)
			{
				return queue.Select(r => r.Task).ToArray();
			}
		}
	}
}

