using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class ServiceManagerQuerier : IServiceManagerQuerier, IServiceTaskScheduleStateQuerier
	{
		readonly IServiceHostsCache serviceHostsCache;
		readonly ReadOnlyBusinessObjectFactory businessObjectFactory;

		public ServiceManagerQuerier() : this(ObjectFactory.Get<IServiceHostsCache>(), new ReadOnlyBusinessObjectFactory())
		{
		}

		public ServiceManagerQuerier(IServiceHostsCache serviceHostsCache, ReadOnlyBusinessObjectFactory bizoF)
		{
			this.serviceHostsCache = serviceHostsCache;
			businessObjectFactory = bizoF;
		}

		public ServiceTaskStatus CheckStateOfNamedServiceTask(string codeOfServiceTaskToCheck)
		{
			var task = GetTask(codeOfServiceTaskToCheck);
			if (task == null)
			{
				return ServiceTaskStatus.NoSuchTaskIsInstalledInThisDb;
			}

			if (!task.IsActive)
			{
				return ServiceTaskStatus.ServiceTaskIsInactive;
			}

			var result = serviceHostsCache
				.CallAllConfiguredHostsUntilFirstSuccess(client => client.GetTaskStatus(codeOfServiceTaskToCheck))
				.Results
				.SingleOrDefault(entry => entry.IsSuccessful);

			if (result == null)
			{
				return ServiceTaskStatus.NoAvailableHosts;
			}

			return result.Result is { IsActive: true }
				? ServiceTaskStatus.AtLeastOneHostIsRunningHealthily
				: ServiceTaskStatus.ServiceTaskIsInactive;
		}

		public IEnumerable<string> GetServiceTasksByCategory(string category)
		{
			if (!SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
			{
				return new ServiceTaskSchedule.Loader(businessObjectFactory)
					.GetInstancesOfServiceTaskByCategory(category)
					.Select(t => t.S5_ScheduleType.ToString());
			}

			return ObjectFactory
				.Get<IServiceTasksLoader>()
				.Load()
				.GovernedTasks
				.Where(t => t.Category == category)
				.Select(t => t.Code);
		}

		public bool TryGetServiceTaskNextRunTime(string serviceTaskCode, out DateTimeOffset? nextRunTime)
		{
			var task = GetTask(serviceTaskCode);
			nextRunTime = task?.NextRunTime;

			return task is not null;
		}

		public bool TryGetServiceTaskScheduleState(string serviceTaskCode, out string scheduleState)
		{
			var task = GetTask(serviceTaskCode);
			scheduleState = SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value
				? task?.ConfigString
				: task?.SettingsXml;

			return task is not null;
		}

		public bool TryGetServiceTaskBranchPK(string serviceTaskCode, out Guid branch)
		{
			var task = GetTask(serviceTaskCode);
			branch = task?.BranchPk ?? Guid.Empty;

			return branch != Guid.Empty;
		}

		IServiceTask GetTask(string serviceTaskCode)
		{
			if (!SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
			{
				var loadedTask = new ServiceTaskSchedule.Loader(businessObjectFactory).GetInstancesOfNameService(serviceTaskCode);
				return loadedTask is not null
					? new SchedulerServiceTask(loadedTask)
					: null;
			}

			return ObjectFactory
				.Get<IServiceTasksLoader>()
				.Load()
				.GovernedTasks
				.FirstOrDefault(t => t.Code == serviceTaskCode);
		}
	}
}

