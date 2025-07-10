using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class SchedulerServiceTaskCollectionGovernor : IServiceTaskCollectionGovernor
	{
		public SchedulerServiceTaskCollectionGovernor(IEnumerable<ServiceTaskSchedule> taskCollection)
		{
			cachedTasks = taskCollection;

			governedTasks = new Lazy<IEnumerable<IServiceTask>>(ConvertTasks);
		}

		public void SetActive(bool active, IEnumerable<Guid> taskPkFilter)
		{
			cachedTasks
				.Where(t => taskPkFilter.IsNullOrEmpty() || taskPkFilter.Contains(t.PK.ToGuid()))
				.ForEach(t => t.S5_IsActive = active);
		}

		public void SetBranchPk(Guid pk, IEnumerable<Guid> taskPkFilter)
		{
			cachedTasks
				.Where(t => taskPkFilter.IsNullOrEmpty() || taskPkFilter.Contains(t.PK.ToGuid()))
				.ForEach(t => t.S5_GB = pk);
		}

		public void Reload()
		{
			cachedTasks
				.Where(t => t.IsInDatabase)
				.ForEach(t => t.Reload());

			governedTasks = new Lazy<IEnumerable<IServiceTask>>(ConvertTasks);
		}

		public IEnumerable<IServiceTask> GovernedTasks => governedTasks.Value;

		IEnumerable<IServiceTask> ConvertTasks()
		{
			foreach (var task in cachedTasks)
			{
				yield return new SchedulerServiceTask(task);
			}
		}

		readonly IEnumerable<ServiceTaskSchedule> cachedTasks;
		Lazy<IEnumerable<IServiceTask>> governedTasks;
	}
}
