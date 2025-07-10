using System;
using System.Linq;
using CargoWise.EntityFramework;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class SchedulerServiceTasksLoader : IServiceTasksLoader
	{
		public SchedulerServiceTasksLoader(BusinessObjectFactory factory)
		{
			this.factory = new Lazy<BusinessObjectFactory>(() => factory);
		}

		public IServiceTaskCollectionGovernor Load()
		{
			return new SchedulerServiceTaskCollectionGovernor(
				serviceTaskScheduleCollectionProvider
					.Load(factory.Value, ServiceTaskScheduleCollection.RemoteStatus.WithoutStatus)
					.Tasks
					.Cast<ServiceTaskSchedule>());
		}

		readonly Lazy<BusinessObjectFactory> factory;
		readonly IServiceTaskScheduleCollectionProvider serviceTaskScheduleCollectionProvider = new ServiceTaskScheduleCollectionProvider();
	}
}
