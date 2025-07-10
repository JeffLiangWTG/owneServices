using System;
using System.Collections.Generic;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;

namespace ServiceManager.Host.Abstractions
{
	public interface ITaskScheduler : IDisposable
	{
		void InitialiseTasks();
		ITaskRunRequest ReconstructRequest(Func<IRunnableServiceTask, ITaskRunRequest> createRequest, string taskCode);

		TasksActionResultDTO ScheduleTasks(IEnumerable<TaskCodeDTO> taskCodes, bool echoes = true, TimeSpan? delay = null, string? userCode = null);

		TasksActionResultDTO RequestReloadOfTaskConfiguration(IEnumerable<TaskCodeDTO> taskCodes);

		TasksActionResultDTO SetNextRuntime(IEnumerable<TaskCodeDTO> taskCodes, DateTimeOffset? value, bool? revertingAfterFailedRun = null);

		void TrackServiceTaskError(string taskCode);

		void Save();
	}
}
