using System;
using System.ComponentModel.Design;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public interface IService
	{
	}

	public class ZServiceContainer
	{
		public T GetService<T>() where T : IService
		{
			T result = (T)serviceContainer.GetService(typeof(T));
			AssertNotDisposable(result);
			return result;
		}

		/// <summary>
		/// Adds <paramref name="serviceInstance"/> to this service container, keyed by type <typeparamref name="T"/>, if not already present.
		/// </summary>
		/// <typeparam name="T">Service generic type parameter.</typeparam>
		/// <param name="serviceInstance">Service to add if not already present.</param>
		/// <returns>The <paramref name="serviceInstance"/> if not already present in the service container, otherwise the existing service of type <typeparamref name="T"/>.</returns>
		public T AddService<T>(T serviceInstance) where T : IService
		{
			return AddService(serviceInstance, typeof(T));
		}

		/// <summary>
		/// Adds <paramref name="serviceInstance"/> to this service container, keyed by type <paramref name="serviceType"/>, if not already present. Use this overload if the generic type is not known at compile time.
		/// </summary>
		/// <typeparam name="T">Service generic type parameter.</typeparam>
		/// <param name="serviceInstance">Service to add if not already present.</param>
		/// <param name="serviceType">The type to use for checking existence of service in this service container.</param>
		/// <returns>The <paramref name="serviceInstance"/> if not already present in the service container, otherwise the existing service of type <paramref name="serviceType"/>.</returns>
		public T AddService<T>(T serviceInstance, Type serviceType) where T : IService
		{
			lock (this)
			{
				AssertNotDisposable(serviceInstance);
				T result = (T)serviceContainer.GetService(serviceType);
				if (result == null)
				{
					serviceContainer.AddService(serviceType, serviceInstance);
					return serviceInstance;
				}
				else
				{
					return result;
				}
			}
		}

		public void RemoveService<T>()
		{
			serviceContainer.RemoveService(typeof(T));
		}

		void AssertNotDisposable(object obj)
		{
			if (obj is IDisposable)
			{
				ErrorReporter.ReportOnce("CCE99B3F-9CD8-4C47-BE6D-3B900796790A", "Do not use IDisposable instances with the Service Container as it is not disposed.");
			}
		}

		readonly ServiceContainer serviceContainer = new ServiceContainer();
	}
}
