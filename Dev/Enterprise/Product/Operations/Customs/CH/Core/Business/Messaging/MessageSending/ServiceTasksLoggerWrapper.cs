using System;
using System.Text;
using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.CH.Business
{
	class ServiceTasksLoggerWrapper : ILogger
	{
		readonly LoggingInformationBase logger;

		public ServiceTasksLoggerWrapper(LoggingInformationBase log)
		{
			logger = log;
		}

		public void Log(LogType type, string message)
		{
			logger.Log(type, message);
		}

		public void Log(LogType type, string message, Exception ex)
		{
			var content = new StringBuilder();
			content.Append(message);
			if (ex != null)
			{
				content.Append($" - {ex.ToString()}");
			}
			logger.Log(type, content.ToString());
		}
	}
}
