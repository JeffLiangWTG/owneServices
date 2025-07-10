using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Common.Abstractions;

namespace ServiceManager.Common.CW
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection RegisterCommonServices(this IServiceCollection services)
		{
			services
				.AddSingleton<IErrorReporterProxy, ErrorReporterProxy>()
				.AddSingleton<ISqlMutexLockProvider, SqlMutexLockProvider>();

			return services;
		}
	}
}
