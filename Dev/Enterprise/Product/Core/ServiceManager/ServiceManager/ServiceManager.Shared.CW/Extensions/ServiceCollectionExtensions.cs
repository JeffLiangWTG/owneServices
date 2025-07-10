using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Shared.CW
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection RegisterSharedServices(this IServiceCollection services)
		{
			services
				.AddSingleton<IHostedServiceBusinessObjectBindingsProvider, HostedServiceBusinessObjectBindingsProvider>();

			return services;
		}
	}
}
