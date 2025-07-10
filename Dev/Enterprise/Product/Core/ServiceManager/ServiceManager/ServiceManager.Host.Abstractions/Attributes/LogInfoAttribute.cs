using System;
using Microsoft.Extensions.Logging;

namespace ServiceManager.Host.Abstractions
{
	public class LogInfoAttribute : Attribute
	{
		public LogInfoAttribute(LogLevel logLevel, string message)
		{
			LogLevel = logLevel;
			Message = message;
		}

		public string Message { get; }
		public LogLevel LogLevel { get; }
	}
}
