using System;
using System.Collections.Generic;
using System.Linq;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class TaskStatusProvider : ITaskStatusProvider
	{
		readonly IAllTasksConsumer tasks;
		readonly ITaskQueue taskQueue;
		readonly IProcessRunnerPool runnerPool;
		readonly IProductRegistrationPeriodicChecker regChecker;
		readonly IEnumerable<IHostedServiceBusinessObjectBinding> serviceTaskBizOBindings;

		public TaskStatusProvider(IAllTasksConsumer tasks, ITaskQueue taskQueue, IProcessRunnerPool runnerPool, IProductRegistrationPeriodicChecker regChecker, IEnumerable<IHostedServiceBusinessObjectBinding> serviceTaskBizOBindings)
		{
			this.tasks = tasks;
			this.taskQueue = taskQueue;
			this.runnerPool = runnerPool;
			this.regChecker = regChecker;
			this.serviceTaskBizOBindings = serviceTaskBizOBindings;
		}

		public IEnumerable<ServiceTaskStatus> GetTasksStatus()
		{
			return tasks.GetAll().Select(t => new ServiceTaskStatus(t, taskQueue.GetQueueSnapshot(), runnerPool.GetRunnersSnapshot(), serviceTaskBizOBindings, !regChecker.IsProductRegisteredAsNonTrialSystemOrUnknown()));
		}

		public ServiceTaskStatus GetTaskStatus(string taskCode)
		{
			return tasks.TryGetByCode(taskCode, out var taskRunner) ? new ServiceTaskStatus(taskRunner, taskQueue.GetQueueSnapshot(), runnerPool.GetRunnersSnapshot(), serviceTaskBizOBindings, !regChecker.IsProductRegisteredAsNonTrialSystemOrUnknown()) : null;
		}
	}
}

