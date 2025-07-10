using System.Collections.Generic;

namespace Enterprise.ServiceManager.Host
{
	public interface ITaskStatusProvider
	{
		IEnumerable<ServiceTaskStatus> GetTasksStatus();
		ServiceTaskStatus GetTaskStatus(string taskCode);
	}
}
