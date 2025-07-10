using System;
using Microsoft.Extensions.Logging;

namespace ServiceManager.Runner.Abstractions
{
	public interface IRunnerLogger : ILogger
	{
		void Log(LogLevel logLevel, string message, ICommandInfo commandInfo);
		void Log(LogLevel logLevel, string message);
		void Log(LogLevel logLevel, string message, Exception ex);
	}
}
