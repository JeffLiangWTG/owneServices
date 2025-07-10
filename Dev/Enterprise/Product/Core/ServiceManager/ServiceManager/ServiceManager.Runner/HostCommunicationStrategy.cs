using System;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManagerProto;

namespace Enterprise.ServiceManager.Runner
{
	class HostCommunicationStrategy : IHostCommunicationStrategy
	{
		public HostCommunicationStrategy(IServiceHostMessageDispatcher serviceHostMessageDispatcher)
		{
			this.serviceHostMessageDispatcher = serviceHostMessageDispatcher ?? throw new ArgumentNullException(nameof(serviceHostMessageDispatcher));
		}

		public void Completed(IServiceTaskRunnerQueue queueService)
		{
			queueService.EnqueueResponse(new ServiceTaskRunResponse
			{
				Status = Status.ProcessFinished,
				Command = ResponseCommandType.Idle,
			});
		}

		public void ServiceTaskLockNotAcquired(IServiceTaskRunnerQueue queueService, ICommandInfo commandInfo)
		{
			var responseCommandType = commandInfo is IDirectRunCommandInfo
				? ResponseCommandType.Reenqueue
				: ResponseCommandType.Idle;
			SendRequeueMessage(queueService, commandInfo, FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock, responseCommandType);
		}

		public void GroupLockNotAcquired(IServiceTaskRunnerQueue queueService, ICommandInfo commandInfo)
		{
			var responseCommandType = commandInfo is IDirectRunCommandInfo
				? ResponseCommandType.Reenqueue
				: ResponseCommandType.ScheduleNextRunTime;
			SendRequeueMessage(queueService, commandInfo, FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock, responseCommandType);
		}

		void SendRequeueMessage(IServiceTaskRunnerQueue queueService, ICommandInfo commandInfo, FailureReasonType failureReason, ResponseCommandType responseCommandType)
		{
			switch (commandInfo)
			{
				case IDirectRunCommandInfo directRunCommandInfo:
					queueService.EnqueueResponse(new ServiceTaskRunResponse
					{
						Status = Status.ProcessFinished,
						Command = responseCommandType,
						TaskCode = directRunCommandInfo.Code,
						TaskId = directRunCommandInfo.Id.ToString(),
						FailureReason = failureReason,
					});
					break;
				case IScheduledRunCommandInfo scheduledRunCommandInfo:
					queueService.EnqueueResponse(new ServiceTaskRunResponse
					{
						Status = Status.ProcessFinished,
						Command = responseCommandType,
						TaskCode = scheduledRunCommandInfo.Code,
						TaskId = scheduledRunCommandInfo.Id.ToString(),
						FailureReason = failureReason,
					});
					break;
				case null:
					throw new ArgumentNullException(nameof(commandInfo));
				default:
					throw new ArgumentOutOfRangeException(nameof(commandInfo));
			}
		}

		public void GrpcPortOpened(int port)
		{
			serviceHostMessageDispatcher.SendGrpcPortLockAcquired(port);
		}

		readonly IServiceHostMessageDispatcher serviceHostMessageDispatcher;
	}
}
