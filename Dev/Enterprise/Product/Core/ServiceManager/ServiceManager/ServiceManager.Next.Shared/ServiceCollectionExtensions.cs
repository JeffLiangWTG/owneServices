using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Logging.CW;
using ServiceManager.Shared.Abstractions;

namespace CargoWise.ServiceManager.Next.Shared;

public static class ServiceCollectionExtension
{
	public static void AddWebTaskServices(this IServiceCollection services)
	{
		services.AddTransient<IExceptionHandler, WebTaskExceptionHandler>();
		services.AddTransient<BaseExceptionReporter, WebTaskServiceErrorReporter>();
		services.AddTransient<IServiceManagerInitializer, WebTaskServiceManagerInitializer>();
		services.RegisterNextLoggerServices();
	}

	public static void InitializeWebTaskService(this IServiceProvider services)
	{
		using var scope = services.CreateScope();
		var initializer = scope.ServiceProvider.GetRequiredService<IServiceManagerInitializer>();

		initializer.InitializeApplication();
		initializer.InitializeDatabase();
	}
}
