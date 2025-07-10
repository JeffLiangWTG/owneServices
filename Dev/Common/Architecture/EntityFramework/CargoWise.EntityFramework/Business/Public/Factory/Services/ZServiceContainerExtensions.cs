using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;

namespace CargoWise.EntityFramework.Extensions
{
	public static class ZServiceContainerExtensions
	{
		#region OnSavingService

		public static T GetOnSavingService<T>(this ZServiceContainer serviceContainer) where T : class, IOnSavingServiceBuilder
		{
			return GetOnSavingContainer(serviceContainer).GetService<T>();
		}

		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This is a false positive")]
		public static void AddOnSavingService<T>(this ZServiceContainer serviceContainer, T serviceInstance) where T : class, IOnSavingServiceBuilder
		{
			GetOnSavingContainer(serviceContainer).AddService(serviceInstance);
		}

		internal static OnSavingService GetOnSavingContainer(this ZServiceContainer serviceContainer)
		{
			var serviceProvider = serviceContainer.GetService<OnSavingService>();
			if (serviceProvider == null)
			{
				serviceProvider = InitOnSavingService();
				serviceContainer.AddService(serviceProvider);
			}

			return serviceProvider;
		}

		static OnSavingService InitOnSavingService()
		{
			var service = new OnSavingService();
			service.AddService(ObjectFactory.Get<IOnSavingServiceBuilder>("NumberFountainBatchServiceBuilder"));
			return service;
		}

		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This is a false positive")]
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Instance unknown, only type.")]
		public static void RemoveOnSavingService<T>(this ZServiceContainer serviceContainer) where T : class, IOnSavingServiceBuilder
		{
			var serviceProvider = serviceContainer.GetService<OnSavingService>();
			if (serviceProvider != null)
			{
				serviceProvider.RemoveService<T>();
				if (serviceProvider.IsServiceCollectionEmpty)
				{
					serviceContainer.RemoveService<OnSavingService>();
				}
			}
		}

		#endregion

		#region IAfterOnSavingBOProcessingService

		public static T GetAfterOnSavingService<T>(this ZServiceContainer serviceContainer) where T : class, IAfterOnSavingBOProcessingService
		{
			return serviceContainer.GetServiceFromProvider<AfterOnSavingBOProcessingServiceProvider, T>();
		}

		public static void AddAfterOnSavingService<T>(this ZServiceContainer serviceContainer, T serviceInstance) where T : class, IAfterOnSavingBOProcessingService
		{
			serviceContainer.AddServiceToProvider<AfterOnSavingBOProcessingServiceProvider, T>(serviceInstance);
		}

		public static void RemoveAfterOnSavingService<T>(this ZServiceContainer serviceContainer) where T : class, IAfterOnSavingBOProcessingService
		{
			var serviceProvider = serviceContainer.GetService<AfterOnSavingBOProcessingServiceProvider>();
			if (serviceProvider != null)
			{
				serviceProvider.RemoveService<T>();
				if (serviceProvider.IsServiceCollectionEmpty)
				{
					serviceContainer.RemoveService<AfterOnSavingBOProcessingServiceProvider>();
				}
			}
		}

		#endregion

		#region ICriticalValidationService

		public static T GetCriticalValidationService<T>(this ZServiceContainer serviceContainer) where T : class,
			ICriticalValidationService
		{
			return serviceContainer.GetServiceFromProvider<CriticalValidationServiceProvider, T>();
		}

		public static void AddCriticalValidationService<T>(this ZServiceContainer serviceContainer, T serviceInstance) where T : class, ICriticalValidationService
		{
			serviceContainer.AddServiceToProvider<CriticalValidationServiceProvider, T>(serviceInstance);
		}

		#endregion

		#region IAfterSaveInTransactionService

		public static T GetAfterSaveInTransactionService<T>(this ZServiceContainer serviceContainer) where T : class, IAfterSaveInTransactionService
		{
			return serviceContainer.GetServiceFromProvider<AfterSaveInTransactionService, T>();
		}

		public static void AddAfterSaveInTransactionService<T>(this ZServiceContainer serviceContainer, T serviceInstance) where T : class, IAfterSaveInTransactionService
		{
			serviceContainer.AddServiceToProvider<AfterSaveInTransactionService, T>(serviceInstance);
		}

		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Do not want to refactor all upstream calls of IService.RemoveService yet")]
		public static void RemoveAfterSaveInTransactionService<T>(this ZServiceContainer serviceContainer) where T : class, IAfterSaveInTransactionService
		{
			serviceContainer.RemoveServiceFromProvider<AfterSaveInTransactionService, T>();
		}

		#endregion

		#region IAfterCommittedService

		public static T GetAfterCommittedService<T>(this ZServiceContainer serviceContainer) where T : class, IAfterCommittedService
		{
			return serviceContainer.GetServiceFromProvider<AfterCommittedServiceProvider, T>();
		}

		public static void AddAfterCommittedService<T>(this ZServiceContainer serviceContainer, T serviceInstance) where T : class, IAfterCommittedService
		{
			serviceContainer.AddServiceToProvider<AfterCommittedServiceProvider, T>(serviceInstance);
		}

		#endregion

		#region GetServiceFromProvider & AddServiceToProvider

		static TService GetServiceFromProvider<TServiceProvider, TService>(this ZServiceContainer serviceContainer)
			where TServiceProvider : ServiceProviderBase, IService
			where TService : class, IService
		{
			var wrappingService = serviceContainer.GetService<TServiceProvider>();
			return wrappingService?.GetService<TService>();
		}

		static void AddServiceToProvider<TServiceProvider, TService>(this ZServiceContainer serviceContainer, TService serviceInstance)
			where TServiceProvider : ServiceProviderBase, IService, new()
			where TService : class, IService
		{
			var serviceProvider = serviceContainer.GetService<TServiceProvider>();
			if (serviceProvider == null)
			{
				serviceProvider = new TServiceProvider();
				serviceContainer.AddService(serviceProvider);
			}

			serviceProvider.AddService(serviceInstance);
		}

		static void RemoveServiceFromProvider<TServiceProvider, TService>(this ZServiceContainer serviceContainer)
			where TServiceProvider : ServiceProviderBase, IService
			where TService : class, IService
		{
			serviceContainer.GetService<TServiceProvider>().RemoveService<TService>();
		}

		#endregion
	}
}
