using System;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	abstract class ServiceProviderBase
	{
		public T GetService<T>() where T : class, IService
		{
			IService result;
			Services.TryGetValue(typeof(T), out result);
			return (T)result;
		}

		public void AddService<T>(T serviceInstance) where T : class, IService
		{
			if (services.ContainsKey(typeof(T)))
			{
				throw new ArgumentException("The service already exists in the container.");
			}

			services.Add(typeof(T), serviceInstance);
		}

		public void RemoveService<T>() where T : class, IService
		{
			services.Remove(typeof(T));
		}

		public bool IsServiceCollectionEmpty
		{
			get { return Services.Count == 0; }
		}

		readonly Dictionary<Type, IService> services = new Dictionary<Type, IService>();
		protected IReadOnlyDictionary<Type, IService> Services => services;
	}
}
