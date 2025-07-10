using System;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.Integration;

namespace Enterprise.DbUpgrader.Startup
{
	class LoggerWrapper : ILogger
	{
		public LoggerWrapper(IUpgradeTaskWorkflowLogger logger)
		{
			this.logger = logger;
		}
		readonly IUpgradeTaskWorkflowLogger logger;

		public void Log(LogType logType, string message)
		{
			logger.ShowInfoMessage(message);
		}

		public void Log(LogType logType, string message, Exception ex)
		{
			Log(logType, message + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
		}
	}
}
