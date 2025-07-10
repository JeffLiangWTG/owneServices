using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Logging.CW
{
	class HostLogger : IHostLogger
	{
		public HostLogger(IServiceManagerHostOptions hostOptions)
			: this(new Lazy<ILogger>(() => new LoggerNLogWrapper(hostOptions.ServerName, hostOptions.DatabaseName, ServiceManagerHelper.HostLoggerCode)))
		{
		}

		internal HostLogger(Lazy<ILogger> logger)
		{
			loggerLazy = logger;
		}

		public void Log(LogLevel logLevel, string message)
		{
			LogWithProperties(() => Logger.Log(logLevel, message));
		}

		public void Log(LogLevel logLevel, string message, Exception? ex)
		{
			LogWithProperties(() => Logger.Log(logLevel, ex, message));
		}

		public void Log(LogLevel logLevel, IHostedServiceAttribute hostedServiceAttribute, string message, Exception ex)
		{
			Log(logLevel, FormatMessage(hostedServiceAttribute, message), ex);
		}

		public void Log(LogLevel logLevel, IHostedServiceAttribute hostedServiceAttribute, string message)
		{
			Log(logLevel, FormatMessage(hostedServiceAttribute, message));
		}

		public void Log(LogLevel logLevel, IHostedServiceAttribute hostedServiceAttribute, int processId, string message)
		{
			Log(logLevel, FormatMessage(hostedServiceAttribute, FormattableString.Invariant($"PID={processId}: {message}")));
		}

		public IDisposable LogSection(LogLevel logLevel, string startMessage, string endMessage)
		{
			Log(logLevel, startMessage);

			return new DisposableAction(() => Log(logLevel, endMessage));
		}

		public IDisposable LogSection(LogLevel logLevel, string startMessage, Func<string> endMessageCallback)
		{
			Log(logLevel, startMessage);

			return new DisposableAction(() => Log(logLevel, endMessageCallback.Invoke()));
		}

		string FormatMessage(IHostedServiceAttribute hostedServiceAttribute, string message)
		{
			return hostedServiceAttribute == null ? message : FormattableString.Invariant($"{hostedServiceAttribute.Code}: {message} ({hostedServiceAttribute.Description})");
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			LogWithProperties(() => Log(logLevel, formatter(state, exception), exception));
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return Logger.IsEnabled(logLevel);
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			return Logger.BeginScope(state);
		}

		void LogWithProperties(Action logAction)
		{
			using (Logger.BeginScope(new[] { new KeyValuePair<string, object>("exe", "Host") }))
			{
				logAction();
			}
		}

		readonly Lazy<ILogger> loggerLazy;
		ILogger Logger => loggerLazy.Value;
	}
}
