using System;
using CargoWise.Common;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class DependentServiceContainer : IServiceContainer
	{
		public DependentServiceContainer(IServiceContainer parent)
		{
			Argument.NotNull(parent, nameof(parent));
			this.parent = parent;
			this.inner = new ServiceContainer(false);
		}

		readonly IServiceContainer parent;
		readonly ServiceContainer inner;

		public void Register<T>(object instance) where T : class
		{
			inner.Register<T>(instance);
		}

		public void Register<T>(Func<T> serviceProvider) where T : class
		{
			inner.Register(serviceProvider);
		}

		public T Resolve<T>() where T : class
		{
			return inner.Resolve<T>() ?? parent.Resolve<T>();
		}
	}
}