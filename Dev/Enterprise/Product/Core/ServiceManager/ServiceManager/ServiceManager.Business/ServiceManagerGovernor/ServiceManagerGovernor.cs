using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public class ServiceManagerGovernor : IServiceManagerGovernor
	{
		readonly IServiceTaskScheduleStatusProvider statusProvider;
		readonly BusinessObjectFactory factory;

		public ServiceManagerGovernor() : this(ObjectFactory.Get<IServiceTaskScheduleStatusProvider>(), new BusinessObjectFactory())
		{
		}

		public ServiceManagerGovernor(IServiceTaskScheduleStatusProvider statusProvider, BusinessObjectFactory factory)
		{
			this.statusProvider = statusProvider;
			this.factory = factory;
		}

		public void SetServiceTaskNextRuntime(string code, DateTimeOffset? nextRunTime)
		{
			using var transactionAdapter = GetTransactionAdapter();
			var taskGovernor = transactionAdapter.GetServiceTaskGovernor(code);

			if (taskGovernor is null)
			{
				return;
			}

			taskGovernor.SetNextRunTime(nextRunTime ?? DateTimeOffset.MinValue);
			transactionAdapter.Commit();

			statusProvider.SetServiceTaskNextRuntime(code, nextRunTime?.UtcDateTime);
		}

		public void SetServiceTaskIsActive(string code, bool isActive)
		{
			using var transactionAdapter = GetTransactionAdapter();
			var taskGovernor = transactionAdapter.GetServiceTaskGovernor(code);

			if (taskGovernor is null)
			{
				return;
			}

			taskGovernor.SetActive(isActive);
			transactionAdapter.Commit();
		}

		ITransactionAdapter GetTransactionAdapter()
		{
			if (!SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
			{
				return new SchedulerServiceTaskTransactionAdapter(new Lazy<BusinessObjectFactory>(() => factory));
			}

			return ObjectFactory.Get<ITransactionAdapter>();
		}
	}
}

