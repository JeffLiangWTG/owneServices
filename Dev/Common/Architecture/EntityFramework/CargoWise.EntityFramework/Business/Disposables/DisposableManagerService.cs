using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public class DisposableManagerService : IService
	{
		internal DisposableManagerService(DisposableManager manager)
		{
			managers = new Stack<DisposableManager>();
			managers.Push(manager);
		}

		readonly Stack<DisposableManager> managers;

		internal void Push(DisposableManager manager) => managers.Push(manager);

		internal DisposableManager Pop() => managers.Pop();

		public T Subscribe<T>(T arg)
			where T : IDisposable
		{
			return CurrentManager.Subscribe(arg);
		}

		internal DisposableManager CurrentManager => managers.Peek();
	}

	public static class DisposableManagerService_Extensions
	{
		public static DisposableManager GetDisposableManager(this BusinessObjectFactory factory)
		{
			var manager = factory.ServiceContainer.GetService<DisposableManagerService>()?.CurrentManager;
			if (manager == null)
			{
				ReportServiceNotInitialized(factory);
				return null;
			}
			else
			{
				return manager;
			}
		}

		public static bool TryGetDisposableManager(this BusinessObjectFactory factory, out DisposableManager manager)
		{
			manager = factory?.ServiceContainer.GetService<DisposableManagerService>()?.CurrentManager;
			return manager != null;
		}

		public static T SubscribeForDispose<T>(this BusinessObjectFactory factory, T disposable)
			where T : IDisposable
		{
			var service = factory.ServiceContainer.GetService<DisposableManagerService>();
			if (service == null)
			{
				ReportServiceNotInitialized(factory);
				return disposable;
			}
			else
			{
				return service.Subscribe(disposable);
			}
		}

		public static IDisposable AddDisposableService(this BusinessObjectFactory factory, DisposableManager manager)
		{
			var service = factory.ServiceContainer.GetService<DisposableManagerService>();
			if (service == null)
			{
				service = new DisposableManagerService(manager);
				factory.ServiceContainer.AddService(service);
				return new DisposableAction(() => factory.ServiceContainer.RemoveService<DisposableManagerService>());
			}
			else
			{
				service.Push(manager);
				return new DisposableAction(() => service.Pop());
			}
		}

		/// <summary>
		/// Add a disposable service to the factory.
		/// Disposable objects can be subscribed to this service to be disposed when this service is disposed.
		/// Multiple services can be added, disposable objects will subscribe to the innermost service.
		/// </summary>
		/// <param name="factory"></param>
		public static IDisposable AddDisposableService(this BusinessObjectFactory factory)
		{
			return AddDisposableService(factory, true);
		}

		/// <summary>
		/// Add a disposable service to the factory if one does not already exist.
		/// </summary>
		/// <param name="factory"></param>
		public static IDisposable AddDisposableServiceIfRequired(this BusinessObjectFactory factory)
		{
			return AddDisposableService(factory, false);
		}

		static IDisposable AddDisposableService(this BusinessObjectFactory factory, bool addNestedService)
		{
			var service = factory.ServiceContainer.GetService<DisposableManagerService>();
			if (service == null)
			{
				var manager = new DisposableManager();
				service = new DisposableManagerService(manager);
				factory.ServiceContainer.AddService(service);
				return new DisposableAction(() =>
				{
					factory.ServiceContainer.RemoveService<DisposableManagerService>();
					service.Pop().Dispose();
				});
			}
			else if (addNestedService)
			{
				var manager = new DisposableManager();
				service.Push(manager);
				return new DisposableAction(() => service.Pop().Dispose());
			}
			else
			{
				return null;
			}
		}

		static void ReportServiceNotInitialized(BusinessObjectFactory factory)
		{
			var msg = $"Disposable manager service is not initialized. Factory: {factory.NameForDebugging}";
			ErrorReporter.ReportOnce(msg);
		}
	}
}
