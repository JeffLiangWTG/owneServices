using System;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class TaskStatusRequestHandler : RequestHandler
	{
		public TaskStatusRequestHandler(ITaskStatusProvider taskStatusProvider, IJsonConverter jsonConverter, string hostName)
		{
			this.taskStatusProvider = taskStatusProvider;
			this.jsonConverter = jsonConverter;
			Uri = ServiceManagerHelper.GetTaskStatusUri(hostName);
		}

		public override Uri Uri { get; }

		protected override string HandleCore(IHttpRequestInfo request)
		{
			var task = request.QueryString["task"];
			var taskStatus = taskStatusProvider.GetTaskStatus(task);
			return taskStatus != null
				? jsonConverter.Serialize(taskStatus.CreateTaskStatusDTO())
				: @"null";
		}

		readonly ITaskStatusProvider taskStatusProvider;
		readonly IJsonConverter jsonConverter;
	}
}
