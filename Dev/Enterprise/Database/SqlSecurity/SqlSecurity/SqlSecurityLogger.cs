using System;
using System.Text;
using CargoWise.Common;
using Enterprise.Integration;
using SynchroniserLogging = WTG.Data.SqlDbSecuritySynchroniser;

namespace Enterprise.SqlSecurity
{
	class SqlSecurityLogger : SynchroniserLogging.ILogger
	{
		readonly ILogger integrationLogger;
		readonly StringBuilder logBuilder = new StringBuilder();
		public SqlSecurityLogger(ILogger integrationLogger)
		{
			_ = integrationLogger ?? throw new ArgumentNullException(nameof(integrationLogger));
			this.integrationLogger = integrationLogger;
		}

		public void Log(SynchroniserLogging.LogType logType, string message)
		{
			LogToIntegrationLoger(logType.ToIntegrationLoggerLogType(), message);
		}

		public void Log(SynchroniserLogging.LogType logType, string message, Exception ex)
		{
			LogToIntegrationLoger(logType.ToIntegrationLoggerLogType(), message, ex);
		}

		internal void LogToIntegrationLoger(LogType logType, string message)
		{
			logBuilder.AppendLine(message);
			integrationLogger.Log(logType, message);
		}

		internal void LogToIntegrationLoger(LogType logType, string message, Exception ex)
		{
			logBuilder.AppendLine(message);
			logBuilder.AppendLine(ex.Message);
			logBuilder.AppendLine(ex.StackTrace);
			integrationLogger.Log(logType, message, ex);
		}

		internal void ReportDeveloperExceptionOnce(string message, Exception ex)
		{
			ErrorReporter.ReportDeveloperExceptionOnce(message, logBuilder.ToString(), ex);
		}
	}
}
