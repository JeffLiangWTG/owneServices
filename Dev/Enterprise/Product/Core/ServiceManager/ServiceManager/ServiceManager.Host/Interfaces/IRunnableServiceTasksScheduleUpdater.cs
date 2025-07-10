using System.Collections.Generic;
using ServiceManager.Host.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	interface IRunnableServiceTasksScheduleUpdater
	{
		void ReloadUpdatedFromDatabase(IEnumerable<IRunnableServiceTask> allTasks, IServiceTasksReloader updatedTasksReloader, ITaskScheduler taskScheduler);
	}
}
