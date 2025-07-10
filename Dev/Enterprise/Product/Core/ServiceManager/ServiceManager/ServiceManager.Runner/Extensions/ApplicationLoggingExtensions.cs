using WTG.ApplicationLogging.Abstractions;

namespace Enterprise.ServiceManager.Runner.Extensions
{
	public static class ApplicationLoggingExtensions
	{
		public static IApplicationLogger CreateRunnerLogger(this IApplicationLoggerFactory loggerFactory)
		{
			return loggerFactory.CreateApplicationLogger("ServiceManagerRunner");
		}
	}
}
