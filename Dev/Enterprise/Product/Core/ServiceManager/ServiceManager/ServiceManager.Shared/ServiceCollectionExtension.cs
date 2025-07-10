using System;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.ServiceManager.Shared
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddTransientLazy<TService>(this IServiceCollection services)
			where TService : notnull =>
				services.AddTransient(typeof(Lazy<TService>), provider => new Lazy<TService>(() =>
					provider.GetRequiredService<TService>()));

		public static IServiceCollection AddTransientWithLazy<TService, TImpl>(this IServiceCollection services)
			where TService : notnull
			where TImpl : TService =>
				services
					.AddTransient(typeof(TService), typeof(TImpl))
					.AddTransientLazy<TService>();

		public static IServiceCollection AddSingleton<TService1, TService2, TImpl>(this IServiceCollection services)
			where TImpl : notnull, TService1, TService2 =>
				services
					.AddSingleton(typeof(TImpl))
					.AddSingleton(typeof(TService1), provider => provider.GetRequiredService<TImpl>())
					.AddSingleton(typeof(TService2), provider => provider.GetRequiredService<TImpl>());

		public static IServiceCollection AddSingleton<TService1, TService2, TService3, TImpl>(this IServiceCollection services)
			where TImpl : notnull, TService1, TService2, TService3 =>
				services
					.AddSingleton(typeof(TImpl))
					.AddSingleton(typeof(TService1), provider => provider.GetRequiredService<TImpl>())
					.AddSingleton(typeof(TService2), provider => provider.GetRequiredService<TImpl>())
					.AddSingleton(typeof(TService3), provider => provider.GetRequiredService<TImpl>());

		public static IServiceCollection AddCustom(this IServiceCollection services, Func<IServiceCollection, IServiceCollection> add) =>
			add(services);
	}
}
