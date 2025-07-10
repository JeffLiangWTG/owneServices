using System;
using System.Collections.Generic;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;

namespace Enterprise.ServiceManager.Host
{
	class TasksStatusRequestHandler : RequestHandler
	{
		public TasksStatusRequestHandler(ITaskStatusProvider taskStatusProvider, IJsonConverter jsonConverter, string hostName)
		{
			this.taskStatusProvider = taskStatusProvider;
			this.jsonConverter = jsonConverter;
			Uri = ServiceManagerHelper.GetStatusUri(hostName);
		}

		public override Uri Uri { get; }

		string GetTasksStatus()
		{
			var taskStatuses = taskStatusProvider.GetTasksStatus();
			var taskStatusList = new List<TaskStatusDTO>();

			if (taskStatuses != null)
			{
				foreach (var taskStatus in taskStatuses)
				{
					taskStatusList.Add(taskStatus.CreateTaskStatusDTO());
				}
			}

			var result = new TaskStatusListDTO(taskStatusList);

			return jsonConverter.Serialize(result);
		}

		protected override string HandleCore(IHttpRequestInfo request)
		{
			return GetTasksStatus();
		}

		readonly ITaskStatusProvider taskStatusProvider;
		readonly IJsonConverter jsonConverter;
	}
}
