using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class SchedulerServiceTaskLoader : IServiceTaskLoader
	{
		public SchedulerServiceTaskLoader()
			: this(new Lazy<BusinessObjectFactory>(() => new BusinessObjectFactory()))
		{ }

		public SchedulerServiceTaskLoader(Lazy<BusinessObjectFactory> factory)
		{
			this.factory = factory;
		}

		public IServiceTask Load(string serviceTaskCode)
		{
			var query = new ZQuery();
			query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, StmServiceHostSchema.Constants.Prefix);
			query.AddToFilter(StmScheduleTaskSchema.S5_ScheduleType, serviceTaskCode);
			var schedule = factory.Value.LoadTop1<ServiceTaskSchedule>(query);

			return schedule is null
				? null
				: new SchedulerServiceTask(schedule);
		}

		public IEnumerable<IServiceTask> LoadAll()
		{
			return factory.Value.Load<ServiceTaskSchedule>(new ZQuery(StmScheduleTaskSchema.S5_ParentTableCode, StmServiceHostSchema.Constants.Prefix))
				.Select(bo => new SchedulerServiceTask(bo));
		}

		readonly Lazy<BusinessObjectFactory> factory;
	}
}
