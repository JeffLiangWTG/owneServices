using System.Collections.Generic;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Common.Abstractions
{
	public interface IServiceTaskScheduleManager
	{
		IEnumerable<IServiceTask> ConfigureSchedules(IEnumerable<IHostedServiceAttribute> serviceTasks);
	}
}
