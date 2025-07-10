using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	internal interface IInitializationTaskRunner
	{
		void EnqueueNudgeableTasks(IEnumerable<IRunnableServiceTask> allTasks, IEnumerable<IHostedServiceBusinessObjectBinding> nudgingAttributes);
		void EnqueueTasksThatAlwaysRunOnStartup(IEnumerable<IRunnableServiceTask> allTasks);
	}

	class InitializationTaskRunner : IInitializationTaskRunner
	{
		readonly IHostLogger hostLogger;
		readonly ITaskQueue taskQueue;

		public InitializationTaskRunner(IHostLogger hostLogger, ITaskQueue taskQueue)
		{
			this.hostLogger = hostLogger;
			this.taskQueue = taskQueue;
		}

		public void EnqueueNudgeableTasks(IEnumerable<IRunnableServiceTask> allTasks, IEnumerable<IHostedServiceBusinessObjectBinding> nudgingAttributes)
		{
			// run tasks with business object bindings
			using (hostLogger.LogSection(LogLevel.Information,
				"Creating Nudge run requests for service tasks with business object binding.",
				"Nudge run requests for service tasks are created and enqueued."))
			{
				var tasksWithBizOBindingAttributes = nudgingAttributes
					.Select(taskBindingAttribute => allTasks.FirstOrDefault(t => t.HasSchedule && t.Code == taskBindingAttribute.ServiceTaskCode))
					.Where(task => task != null && task.IsActive)
					.Distinct();
				foreach (var task in tasksWithBizOBindingAttributes)
				{
					var newRequest = new DirectTaskRunRequest(task, echoes: false);
					hostLogger.Log(LogLevel.Debug, newRequest.FormatRequestToLogMessage(LogMessageStage.RequestIsCreated));
					taskQueue.EnqueueTask(newRequest);
				}
			}
		}

		public void EnqueueTasksThatAlwaysRunOnStartup(IEnumerable<IRunnableServiceTask> allTasks)
		{
			using (hostLogger.LogSection(LogLevel.Information,
				"Creating Nudge run requests for tasks requiring to be launched on startup.",
				"Nudge run requests for tasks requiring to be launched on startup are created and enqueued."))
			{
				var tasksToRun = allTasks
					.Where(t => t != null && t.HasSchedule && t.IsActive && t.Info.HostedServiceAttribute.AlwaysRunAtStartup);
				foreach (var task in tasksToRun)
				{
					var newRequest = new DirectTaskRunRequest(task, echoes: false);
					hostLogger.Log(LogLevel.Debug, newRequest.FormatRequestToLogMessage(LogMessageStage.RequestIsCreated));
					taskQueue.EnqueueTask(newRequest);
				}
			}
		}
	}
}
