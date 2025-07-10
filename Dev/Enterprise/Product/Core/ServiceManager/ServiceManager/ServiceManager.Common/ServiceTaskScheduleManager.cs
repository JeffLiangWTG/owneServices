using System;
using System.Collections.Generic;
using System.Linq;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Common
{
	public class ServiceTaskScheduleManager : IServiceTaskScheduleManager
	{
		public ServiceTaskScheduleManager(
			IServiceTaskLoaderFactory serviceTaskLoaderFactory,
			ITransactionAdapterFactory transactionAdapterFactory,
			IDefaultScheduleConfigurer defaultScheduleConfigurer,
			IServiceTaskRequirementsChecker serviceTaskRequirementsChecker)
		{
			this.serviceTaskLoaderFactory = serviceTaskLoaderFactory ?? throw new ArgumentNullException(nameof(serviceTaskLoaderFactory));
			this.transactionAdapterFactory = transactionAdapterFactory ?? throw new ArgumentNullException(nameof(transactionAdapterFactory));
			this.defaultScheduleConfigurer = defaultScheduleConfigurer ?? throw new ArgumentNullException(nameof(defaultScheduleConfigurer));
			this.serviceTaskRequirementsChecker = serviceTaskRequirementsChecker ?? throw new ArgumentNullException(nameof(serviceTaskRequirementsChecker));
		}

		public IEnumerable<IServiceTask> ConfigureSchedules(IEnumerable<IHostedServiceAttribute> serviceTaskAttributes)
		{
			var serviceTaskLoader = serviceTaskLoaderFactory.CreateServiceTaskLoader();
			using var transactionAdapter = transactionAdapterFactory.CreateTransactionAdapter();
			var tasksAlreadyInitialized = serviceTaskLoader.LoadAll()
				.ToDictionary(t => t.Code, t => t);
			var hostedServiceAttributeDictionary = serviceTaskAttributes
				.ToDictionary(h => h.Code, h => h);
			var taskCodesRequiringInitialization = hostedServiceAttributeDictionary.Keys.Except(tasksAlreadyInitialized.Keys);

			var governorsForNewTasks = new List<IServiceTaskGovernor>();
			foreach (var taskCode in taskCodesRequiringInitialization)
			{
				var hostedServiceAttribute = hostedServiceAttributeDictionary[taskCode];
				var newTaskGovernor = transactionAdapter.GetNewServiceTaskGovernor(hostedServiceAttribute);
				defaultScheduleConfigurer.SetDefaultScheduleForTask(newTaskGovernor, hostedServiceAttribute);

				if (serviceTaskRequirementsChecker.AttributeSatisfiesRequirements(hostedServiceAttribute, out var branchCode) && !string.IsNullOrEmpty(branchCode))
				{
					newTaskGovernor.SetBranchFromCode(branchCode);
				}

				governorsForNewTasks.Add(newTaskGovernor);
			}

			var governorsForModifiedTasks = new List<IServiceTaskGovernor>();
			foreach (var serviceTask in tasksAlreadyInitialized
						.Where(t => !t.Value.IsActive)
						.Where(t => hostedServiceAttributeDictionary.TryGetValue(t.Key, out var attribute) && attribute.IsMandatory))
			{
				var taskGovernor = transactionAdapter.GetServiceTaskGovernor(serviceTask.Value.Pk);
				taskGovernor?.SetActive(true);
				if (taskGovernor != null)
				{
					governorsForModifiedTasks.Add(taskGovernor);
				}
			}

			transactionAdapter.Commit();

			var result = governorsForNewTasks.Select(g => g.GovernedTask).ToList();
			var modifiedTasks = governorsForModifiedTasks.Select(g => g.GovernedTask).ToList();
			var initializedNotModifiedTasks = tasksAlreadyInitialized
				.Where(t => !modifiedTasks.Any(m => m.Pk == t.Value.Pk))
				.Select(t => t.Value)
				.ToList();
			result.AddRange(modifiedTasks);
			result.AddRange(initializedNotModifiedTasks);

			return result;
		}

		readonly IServiceTaskLoaderFactory serviceTaskLoaderFactory;
		readonly ITransactionAdapterFactory transactionAdapterFactory;
		readonly IDefaultScheduleConfigurer defaultScheduleConfigurer;
		readonly IServiceTaskRequirementsChecker serviceTaskRequirementsChecker;
	}
}
