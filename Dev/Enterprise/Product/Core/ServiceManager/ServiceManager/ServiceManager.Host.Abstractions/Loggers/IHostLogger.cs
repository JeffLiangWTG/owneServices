using System;
using Microsoft.Extensions.Logging;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Host.Abstractions
{
	public interface IHostLogger : ILogger
	{
		void Log(LogLevel logLevel, string message);
		void Log(LogLevel logLevel, string message, Exception ex);
		void Log(LogLevel logLevel, IHostedServiceAttribute hostedServiceAttribute, string message, Exception ex);

		void Log(LogLevel logLevel, IHostedServiceAttribute hostedServiceAttribute, string message);

		void Log(LogLevel logLevel, IHostedServiceAttribute hostedServiceAttribute, int processId, string message);

		IDisposable LogSection(LogLevel logLevel, string startMessage, string endMessage);

		IDisposable LogSection(LogLevel logLevel, string startMessage, Func<string> endMessageCallBack);
	}
}
