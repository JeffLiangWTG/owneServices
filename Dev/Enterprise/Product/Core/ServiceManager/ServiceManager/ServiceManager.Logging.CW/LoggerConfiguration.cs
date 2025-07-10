using NLog;

namespace ServiceManager.Logging.CW
{
	public static class LoggerConfiguration
	{
		public static void InitializeLoggingConsole()
		{
			LoggerHelper.ConfigureTargetAndRule(new NLogColoredConsoleTargetFactory(), "*", LogLevel.Debug);

			LogManager.ReconfigExistingLoggers();
		}
	}
}
