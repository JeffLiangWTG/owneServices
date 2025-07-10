using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class SchedulerServiceTaskTransactionAdapter : ITransactionAdapter
	{
		public SchedulerServiceTaskTransactionAdapter()
			: this(new Lazy<BusinessObjectFactory>(() => new BusinessObjectFactory()))
		{
		}

		public SchedulerServiceTaskTransactionAdapter(Lazy<BusinessObjectFactory> factory)
		{
			this.factory = factory;
			loader = new Lazy<SchedulerServiceTasksLoader>(() => new SchedulerServiceTasksLoader(Factory));
		}

		public IServiceTaskGovernor GetServiceTaskGovernor(Guid pk)
		{
			var query = new ZQuery();
			query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, StmServiceHostSchema.Constants.Prefix);
			query.AddToFilter(StmScheduleTaskSchema.PK, pk);
			var serviceTask = Factory.LoadTop1<ServiceTaskSchedule>(query);

			return serviceTask is null
				? null
				: new SchedulerServiceTaskGovernor(Factory, serviceTask);
		}

		public IServiceTaskGovernor GetServiceTaskGovernor(string code)
		{
			var query = new ZQuery();
			query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, StmServiceHostSchema.Constants.Prefix);
			query.AddToFilter(StmScheduleTaskSchema.S5_ScheduleType, code);
			var serviceTask = Factory.LoadTop1<ServiceTaskSchedule>(query);

			return serviceTask is null
				? null
				: new SchedulerServiceTaskGovernor(Factory, serviceTask);
		}

		public IServiceTaskGovernor GetNewServiceTaskGovernor(IHostedServiceAttribute newTaskAttribute)
		{
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = newTaskAttribute.Code;
			schedule.S5_ScheduleDescription = newTaskAttribute.Description;
			schedule.S5_TypeOfDocument = newTaskAttribute.Category;
			if (newTaskAttribute.CanRunInAnyBranch)
			{
				schedule.S5_GB = ZGuid.Empty;
			}

			return new SchedulerServiceTaskGovernor(Factory, schedule);
		}

		public IServiceTaskCollectionGovernor GetCollectionGovernorForAllTasks()
		{
			return Loader.Load();
		}

		public void Commit() => Factory.Save();

		public void Dispose()
		{
			factory = null;
			loader = null;
			GC.SuppressFinalize(this);
		}

		BusinessObjectFactory Factory => factory.Value;
		SchedulerServiceTasksLoader Loader => loader.Value;

		Lazy<BusinessObjectFactory> factory;
		Lazy<SchedulerServiceTasksLoader> loader;

		public event EventHandler Committing { add { } remove { } }
	}
}
