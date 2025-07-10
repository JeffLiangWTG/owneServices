using System;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Data.Services
{
	public class DbServiceLocator
	{
		public DbServiceLocator(Action<IServiceCollection> builder)
		{
			var services = new ServiceCollection();

			builder(services);

			CurrentServiceCollection = services;
			CurrentServiceProvider = services.BuildServiceProvider();
		}

		internal IServiceCollection CurrentServiceCollection { get; private set; }
		IServiceProvider CurrentServiceProvider { get; set; }

		public T GetService<T>()
		{
			return CurrentServiceProvider.GetService<T>();
		}
	}
}
