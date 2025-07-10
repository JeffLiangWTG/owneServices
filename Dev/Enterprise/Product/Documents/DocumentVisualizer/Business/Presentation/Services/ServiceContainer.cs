using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Enterprise.DocumentVisualizer.Presentation
{
	[DebuggerDisplay("{serviceRegister.Count} services")]
	public sealed class ServiceContainer : IServiceContainer
	{
		public ServiceContainer(bool allowProxyGeneration = true)
		{
			this.allowProxyGeneration = allowProxyGeneration;
		}

		readonly bool allowProxyGeneration;
		readonly Dictionary<Type, Func<object>> serviceRegister = new Dictionary<Type, Func<object>>();

		public void Register<T>(object instance) where T : class
		{
			if (instance != null)
			{
				serviceRegister[typeof(T)] = () => instance;
			}
		}

		public void Register<T>(Func<T> serviceProvider) where T : class
		{
			if (serviceProvider != null)
			{
				serviceRegister[typeof(T)] = serviceProvider;
			}
		}

		public T Resolve<T>() where T : class
		{
			var key = typeof(T);

			if (serviceRegister.ContainsKey(key))
			{
				return (T)serviceRegister[key]();
			}

			T proxy = null;

			if (allowProxyGeneration)
			{
				proxy = ProxyBuilder.Instance.CreateProxy<T>();
				Register<T>(() => proxy);
			}

			return proxy;
		}
	}
}