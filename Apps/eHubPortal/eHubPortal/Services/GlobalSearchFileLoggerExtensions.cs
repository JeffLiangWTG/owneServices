using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;

namespace eServices.eHubPortal.Services;

public static class GlobalSearchFileLoggerExtensions
{
	public static ILoggingBuilder AddGlobalSearchFileLogger(this ILoggingBuilder builder)
	{
		builder.AddConfiguration();
		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, GlobalSearchFileLoggerProvider>());
		LoggerProviderOptions.RegisterProviderOptions<GlobalSearchFileLoggerConfiguration, GlobalSearchFileLoggerProvider>(builder.Services);
		return builder;
	}

	public static ILoggingBuilder AddGlobalSearchFileLogger(this ILoggingBuilder builder, Action<GlobalSearchFileLoggerConfiguration> configure)
	{
		builder.AddGlobalSearchFileLogger();
		builder.Services.Configure(configure);
		return builder;
	}
}
