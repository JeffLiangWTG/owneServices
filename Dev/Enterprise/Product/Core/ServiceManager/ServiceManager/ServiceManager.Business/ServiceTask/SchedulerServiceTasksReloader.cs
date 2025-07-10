using System;
using System.Linq;
using CargoWise.EntityFramework;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class SchedulerServiceTasksReloader : IServiceTasksReloader
	{
		public IServiceTaskCollectionGovernor Reload(DateTimeOffset lastUpdateTime)
		{
			return new SchedulerServiceTaskCollectionGovernor(
				serviceTaskScheduleCollectionProvider
					.LoadUpdatedFromTime(factory.Value, lastUpdateTime.UtcDateTime)
					.Tasks
					.Cast<ServiceTaskSchedule>());
		}

		readonly Lazy<BusinessObjectFactory> factory = new(() => new BusinessObjectFactory());
		readonly IServiceTaskScheduleCollectionProvider serviceTaskScheduleCollectionProvider = new ServiceTaskScheduleCollectionProvider();
	}
}
