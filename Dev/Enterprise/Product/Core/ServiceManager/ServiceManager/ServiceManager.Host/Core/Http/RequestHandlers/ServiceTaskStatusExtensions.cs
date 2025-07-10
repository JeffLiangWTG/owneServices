using System;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;

namespace Enterprise.ServiceManager.Host
{
	static class ServiceTaskStatusExtensions
	{
		public static TaskStatusDTO CreateTaskStatusDTO(this ServiceTaskStatus taskStatus)
		{
			var timeInQueue = taskStatus.PlaceInQueue >= 0 ? taskStatus.TimeSinceLastEnqueued.Elapsed : TimeSpan.Zero;
			var timeRunning = taskStatus.RunningCount > 0 ? taskStatus.TimeRunning.Elapsed : TimeSpan.Zero;
			return new TaskStatusDTO(
				taskStatus.Code,
				taskStatus.Branch,
				taskStatus.BindingTypes,
				taskStatus.Description,
				taskStatus.Category,
				taskStatus.IsActive,
				taskStatus.Status,
				taskStatus.RunnerPids,
				taskStatus.RunningCount,
				taskStatus.NextRunTime,
				taskStatus.SchedulePeriod,
				taskStatus.PlaceInQueue,
				timeInQueue,
				timeRunning,
				taskStatus.LastRunTime,
				taskStatus.LastErrorTime,
				taskStatus.ErrorCountLast24Hours);
		}
	}
}
