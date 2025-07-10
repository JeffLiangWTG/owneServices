using System;
using Enterprise.Integration;
using NLog;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class BaseLogger : Integration.ILogger
	{
		public BaseLogger(Type type)
		{
			nLogger = LogManager.GetLogger(type.Name);
		}

		readonly Logger nLogger;

		public void Log(LogType type, string message)
		{
			switch (type)
			{
				case LogType.Debug:
					nLogger.Trace(message);
					break;

				case LogType.Information:
					nLogger.Info(message);
					break;

				case LogType.Warning:
					nLogger.Warn(message);
					break;

				case LogType.Error:
					nLogger.Error(message);
					break;

				default:
					return;
			}
		}

		public void Log(LogType type, string message, Exception ex)
		{
			switch (type)
			{
				case LogType.Debug:
					nLogger.Trace(ex, message);
					break;

				case LogType.Information:
					nLogger.Info(ex, message);
					break;

				case LogType.Warning:
					nLogger.Warn(ex, message);
					break;

				case LogType.Error:
					nLogger.Error(ex, message);
					break;

				default:
					return;
			}
		}
	}
}
