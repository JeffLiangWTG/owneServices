using NLog;

namespace CargoWiseOne.WebInfrastructure.Integration.Test.Helpers
{
	class LoggerHelper
	{
		public LoggerHelper(string name)
		{
			logger = LogManager.GetLogger(name);
		}

		public void Info(string message)
		{
			var logMessage = message?.Trim();
			if (string.IsNullOrEmpty(logMessage))
			{
				return;
			}

			logger.Log(LogLevel.Info, logMessage);
		}

		readonly Logger logger;
	}
}
