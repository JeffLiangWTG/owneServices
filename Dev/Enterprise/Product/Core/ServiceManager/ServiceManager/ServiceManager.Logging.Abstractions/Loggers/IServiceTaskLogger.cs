using System;
using Microsoft.Extensions.Logging;

namespace ServiceManager.Logging.Abstractions
{
	public interface IServiceTaskLogger : ILogger
	{
		bool TaskLoggerIsActive { get; }
		IDisposable SetTaskLoggerCode(string code);
		void Log(LogLevel logLevel, string message);
		void Log(LogLevel logLevel, string message, Exception ex);
	}
}
