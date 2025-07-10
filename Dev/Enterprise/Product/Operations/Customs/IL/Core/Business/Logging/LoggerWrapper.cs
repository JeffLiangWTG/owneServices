using System;
using CargoWise.Common;
using Enterprise.Customs.Business.Logging;
using Enterprise.Integration;
using Enterprise.Integration.BatchProcessor;

namespace Enterprise.Customs.IL.Business
{
	public class LoggerWrapper : ICommonLogger
	{
		readonly ILogger logger;
		readonly ILoggingInformation simpleLogger;

		public LoggerWrapper(ILogger logger)
		{
			this.logger = Argument.NotNull(logger, "logger");
		}

		public LoggerWrapper(ILoggingInformation simpleLogger)
		{
			this.simpleLogger = Argument.NotNull(simpleLogger, "simpleLogger");
		}

		public void Log(LogType logLevel, string message)
		{
			logger?.Log(logLevel, message);
			simpleLogger?.Log(message);
		}

		public void Log(LogType logLevel, string message, Exception ex)
		{
			logger.Log(logLevel, message, ex);
			simpleLogger?.Log(message);
		}

		public void LogFormat(LogType logLevel, string format, params object[] args)
		{
			logger.Log(logLevel, string.Format(format, args));
			simpleLogger?.Log(string.Format(format, args));
		}

		public void BumpSectionProgress()
		{
		}

		public void SetSectionProgressMax(int max)
		{
		}
	}
}
