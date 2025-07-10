using System;
using System.Collections.Generic;

namespace ServiceManager.Shared.Abstractions
{
	public interface IServiceTaskScheduleStatusProvider : IDisposable
	{
		IDictionary<string, TaskInstanceStatus> GetServiceStatus();

		TaskInstanceStatus GetServiceTaskStatus(string taskCode);

		void SetServiceTaskNextRuntime(string taskCode, DateTime? nextRunTime, bool? revertingAfterFailedRun = null);
		void RequestTaskConfigurationReload(string taskCode);
	}
}
