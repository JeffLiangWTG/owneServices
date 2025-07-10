using System;
using System.Collections.Concurrent;

namespace CargoWise.Common
{
	public class FactoryService : IFactoryService
	{
		readonly ConcurrentDictionary<Type, object> factories = new ConcurrentDictionary<Type, object>();

		public void RegisterFactory<T>(T factory)
		{
			factories.TryAdd(typeof(T), factory);
		}

		public T GetFactory<T>()
		{
			if (!factories.TryGetValue(typeof(T), out var factory))
			{
				throw new InvalidOperationException($"Factory of type {typeof(T).ToString()} has not been registered");
			}

			return (T)factory;
		}
	}
}
