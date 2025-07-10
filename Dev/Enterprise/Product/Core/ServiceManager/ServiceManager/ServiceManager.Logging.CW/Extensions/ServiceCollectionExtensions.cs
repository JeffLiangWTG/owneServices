using System;
using System.Collections.Concurrent;
using CargoWise.Data;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Runner.Abstractions;
using MEL = Microsoft.Extensions.Logging;

namespace ServiceManager.Logging.CW
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection RegisterNextLoggerServices(this IServiceCollection services)
		{
			services.AddTransient(sp =>
			{
				var loggerFactory = sp.GetRequiredService<MEL.ILoggerFactory>();
				return loggerFactory.CreateLogger(typeof(LoggerNLogWrapper).FullName ?? throw new InvalidOperationException($"Unable to find name of {typeof(LoggerNLogWrapper)}"));
			});
			services.TryAddEnumerable(ServiceDescriptor.Singleton<MEL.ILoggerProvider, LoggerNLogWrapperWrapperProvider>());
			return services;
		}

		public static IServiceCollection RegisterRunnerLoggerServices(this IServiceCollection services)
		{
			services
				.AddTransient<ILoggerFactory, LoggerFactory>()
				.AddTransient<ILoggerFinalizer, LoggerFactory>()
				.AddTransient<IRunnerLogger, RunnerLogger>()
				.AddSingleton<IServiceTaskLogger, ServiceTaskLogger>();

			return services;
		}

		public static IServiceCollection RegisterHostLoggerServices(this IServiceCollection services)
		{
			services
				.AddTransient<ILoggerFactory, LoggerFactory>()
				.AddSingleton<IEventLogger, EventLogger>()
				.AddSingleton<IQueueMonitorLogger, QueueMonitorLogger>()
				.AddTransient<IHostLogger, HostLogger>();

			return services;
		}
	}

	class LoggerNLogWrapperWrapper : MEL.ILogger
	{
		public LoggerNLogWrapperWrapper(string categoryName)
		{
			CategoryName = categoryName;
		}

		public string CategoryName { get; }

		readonly Lazy<MEL.ILogger> lazyLogger = new(() => new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, ServiceManagerHelper.WebTaskLoggerCode));

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => lazyLogger.Value.BeginScope(state);

		public bool IsEnabled(MEL.LogLevel logLevel) => Db.DatabaseNameIsInitialized && lazyLogger.Value.IsEnabled(logLevel);

		public void Log<TState>(MEL.LogLevel logLevel, MEL.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			lazyLogger.Value.Log(logLevel, eventId, state, exception, formatter);
		}
	}

	sealed class LoggerNLogWrapperWrapperProvider : MEL.ILoggerProvider
	{
		readonly ConcurrentDictionary<string, LoggerNLogWrapperWrapper> loggers = new(StringComparer.OrdinalIgnoreCase);

		public void Dispose()
		{
			loggers.Clear();
		}

		public MEL.ILogger CreateLogger(string categoryName)
		{
			return loggers.GetOrAdd(categoryName, name => new LoggerNLogWrapperWrapper(categoryName));
		}
	}
}
