using System;
using Microsoft.Extensions.Logging;

namespace ServiceManager.Logging.Abstractions
{
	public interface IEventLogger : ILogger
	{
		void Log(LogLevel logLevel, string message);
		void Log(LogLevel logLevel, string message, Exception ex);
	}
}
