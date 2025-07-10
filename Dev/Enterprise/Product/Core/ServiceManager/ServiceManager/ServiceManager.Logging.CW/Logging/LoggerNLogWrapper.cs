using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using NLog.Time;
using ServiceManager.Logging.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ServiceManager.Logging.CW
{
	class LoggerNLogWrapper : ILogger
	{
		public LoggerNLogWrapper(string dbServer, string dbName, string programCode, string? directoryPath = null, ILoggerNLogConfigurationFactory? configFactory = null, bool archiveLogFiles = true, bool trackServiceTaskErrors = true)
		{
			this.programCode = programCode;
			this.directoryPath = directoryPath ?? ServiceManagerHelper.GetLogFilesDirectory(dbServer, dbName);
			currentProcessId = Process.GetCurrentProcess().Id;
			hostname = ServiceManagerHelper.GetHostName().ToLowerInvariant();
			processControllerVersion = ReleaseInfo.Instance.VersionNumber.ToString();
			nLogConfigurationLazy = new Lazy<ILoggerNLogConfiguration>(() => (configFactory ?? new LoggerNLogConfigurationFactory()).GetConfiguration(Db.DatabaseName));
			serviceTaskErrorTracker = !trackServiceTaskErrors
				|| programCode.Equals(ServiceManagerHelper.HostLoggerCode, StringComparison.OrdinalIgnoreCase)
					? null
					: serviceTaskErrorTracker = ObjectFactory.Get<IServiceTaskErrorTracker>();

			var productRegistrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			installationCode = $"{productRegistrationKey.EnterpriseCode}{productRegistrationKey.ServerCode}";

			if (LogManager.Configuration == null)
			{
				LogManager.Configuration = new LoggingConfiguration();
			}

			InitializeLoggingConfiguration(archiveLogFiles);

			var baseProperties = new Dictionary<string, object>()
			{
				{ "database", dbName.ToLowerInvariant() },
				{ "database_host", dbServer.ToLowerInvariant() },
				{ "installation", new { code = installationCode } },
				{ "host", new { name = hostname } },
				{ "processcontroller", new { hostname, version = processControllerVersion, } },
				{ "servicetask", new { code = programCode.ToUpperInvariant(), } },
				{ "process", new { pid = currentProcessId, } },
			};

			LogManager.ReconfigExistingLoggers();
			logger = LogManager.GetLogger(programCode)
				.WithProperties(baseProperties)
				;
		}

		readonly NLog.ILogger logger;
		readonly string programCode;
		readonly string directoryPath;
		readonly int currentProcessId;
		readonly string hostname;
		readonly string installationCode;
		readonly Lazy<ILoggerNLogConfiguration> nLogConfigurationLazy;
		readonly IServiceTaskErrorTracker? serviceTaskErrorTracker;
		readonly string processControllerVersion;
		TimeSpan localTimeOffset;

		void Log(LogLevel logLevel, string message, Exception? ex)
		{
			var currentTime = UtcNowSafe(out var timeException);

			if (timeException != null)
			{
				Log(LogLevel.Error, currentTime, "Time retrieving error.", timeException);
			}

			Log(logLevel, currentTime, message, ex);

			if (timeException?.IsCriticalException() ?? false)
			{
				ExceptionDispatchInfo
					.Capture(timeException)
					.Throw();
			}
		}

		void Log(LogLevel logLevel, DateTime currentTime, string message, Exception? ex)
		{
			RefreshConfigurationSafe();

			var logEventInfo = CreateLogEventInfo(logLevel, currentTime, message, ex);
			logger.Log(logEventInfo);

			if (logLevel == LogLevel.Error
				&& serviceTaskErrorTracker != null)
			{
				serviceTaskErrorTracker.TrackServiceTaskError(programCode);
			}

			LogEventInfo CreateLogEventInfo(LogLevel logLevel, DateTime currentTime, string message, Exception? ex)
			{
				var logEventInfo = LogEventInfo.Create(LoggerHelper.ConvertLogLevel(logLevel), logger.Name, Logger.FormatMessage(message, ex));
				logEventInfo.TimeStamp = currentTime;
				logEventInfo.Properties.Add("severity", logLevel.ToString());
				logEventInfo.Properties.Add("sequenceId", new { sequenceId = logEventInfo.SequenceID });

				if (ex != null)
				{
					logEventInfo.Exception = ex;
				}

				return logEventInfo;
			}

			void RefreshConfigurationSafe()
			{
				try
				{
					using (SuppressDbUpgradeExceptionHasBeenThrownInConnection())
					{
						nLogConfigurationLazy.Value.Refresh();
					}
				}
				catch (DatabaseUpgradeException)
				{
					// Ignoring DatabaseUpgradeException so logging still works after upgrades
				}
				catch (Exception e)
				{
					logger.Log(CreateLogEventInfo(LogLevel.Error, currentTime, "Refresh configuration error.", e));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		DateTime UtcNowSafe(out Exception? exception)
		{
			try
			{
				using (SuppressDbUpgradeExceptionHasBeenThrownInConnection())
				{
					exception = null;
					var result = GetCurrentUtcDateTime();
					localTimeOffset = result - DateTime.UtcNow;
					return result;
				}
			}
			catch (Exception e)
			{
				// Ignoring DatabaseUpgradeException so logging still works after upgrades
				exception = e is DatabaseUpgradeException ? null : e;
				return DateTime.UtcNow + localTimeOffset;
			}
		}

		protected virtual DateTime GetCurrentUtcDateTime()
			=> Env.Time.CurrentUtcDateTime;

#if DEBUG
		public FileInfo? CurrentOutputFileForTest(string taskCode)
		{
			var fileName = GetLogFileName(NLogFileTargetFactory.GetLogfileTargetName(taskCode));
			return !string.IsNullOrWhiteSpace(fileName) ? new FileInfo(fileName) : null;

			string GetLogFileName(string targetName)
			{
				var rtnVal = string.Empty;

				if (LogManager.Configuration != null && LogManager.Configuration.ConfiguredNamedTargets.Count != 0)
				{
					var t = LogManager.Configuration.FindTargetByName<FileTarget>(targetName);
					if (t != null)
					{
						var layout = t.FileName;
						var logEventInfo = new LogEventInfo();
						logEventInfo.LoggerName = logger.Name;
						rtnVal = layout.Render(logEventInfo);
					}
				}

				return rtnVal;
			}
		}
#endif

		void InitializeLoggingConfiguration(bool archiveLogFiles)
		{
			LogManager.ThrowExceptions = false;
			TimeSource.Current = new FastUtcTimeSource();
			var nLogConfiguration = nLogConfigurationLazy.Value;

			if (LogManager.Configuration.AllTargets.Count == 0 || ConsoleAndEventLogAreInitialized())
			{
				InitializeNLogInternalLogging(nLogConfiguration);
			}

			var minLevel = nLogConfiguration.VerboseLogging.VerboseLoggingByCode(programCode) ? NLog.LogLevel.Debug : NLog.LogLevel.Info;

			LoggerHelper.ConfigureTargetAndRule(new NLogFileTargetFactory(programCode, directoryPath, archiveLogFiles), programCode, minLevel);
			LoggerHelper.ConfigureTargetAndRule(new NLogCombinedFileTargetFactory(installationCode), programCode, minLevel);
			LoggerHelper.ConfigureTargetAndRule(new NLogKafkaTargetFactory(), programCode, minLevel);
			LoggerHelper.ConfigureTargetAndRule(new NLogElasticsearchTargetFactory(), programCode, minLevel);
			LoggerHelper.ConfigureTargetAndRule(new NLogSyslogTargetFactory(), programCode, minLevel);

			static bool ConsoleAndEventLogAreInitialized()
			{
				var targets = LogManager.Configuration.AllTargets;
				return targets.Count <= 2 && targets
					.Select(target => target.Name)
					.Intersect(
						new[] { NLogColoredConsoleTargetFactory.TargetName, NLogEventLogTargetFactory.TargetName },
						StringComparer.OrdinalIgnoreCase)
					.Count() == 2;
			}
		}

		void InitializeNLogInternalLogging(ILoggerNLogConfiguration configuration)
		{
			InternalLogger.LogLevel = NLog.LogLevel.Error;
			InternalLogger.LogToConsoleError = true;
			InternalLogger.LogWriter = new StringWriter();
			_ = Trace.Listeners.Add(new TextWriterTraceListener(InternalLogger.LogWriter));

			if (configuration.InternalNLogLoggingEnabled)
			{
				var currentDate = UtcNowSafe(out var timeException);
				InternalLogger.LogFile = Path.Combine(directoryPath, FormattableString.Invariant($"NLog_{currentDate:yyyyMMdd}.txt"));
				InternalLogger.LogToConsoleError = false;
				InternalLogger.LogLevel = NLog.LogLevel.Debug;
				if (timeException != null)
				{
					InternalLogger.Log(timeException, NLog.LogLevel.Error, "Time retrieving error.");
				}
			}
		}

		DisposableAction SuppressDbUpgradeExceptionHasBeenThrownInConnection()
		{
			return new DisposableAction(Suppress, Suppress);

			void Suppress()
			{
				if (Db.DatabaseUpgradedExceptionHasBeenThrownInConnection)
				{
					// Suppress DatabaseUpgradedException thrown twice issue report
					Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
				}
			}
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
			Log(logLevel, formatter(state, exception), exception);

		public bool IsEnabled(LogLevel logLevel) =>
			logLevel switch
			{
				LogLevel.Trace => logger.IsTraceEnabled,
				LogLevel.Debug => logger.IsDebugEnabled,
				LogLevel.Information => logger.IsInfoEnabled,
				LogLevel.Warning => logger.IsWarnEnabled,
				LogLevel.Error => logger.IsErrorEnabled,
				LogLevel.Critical => logger.IsFatalEnabled,
				_ => false
			};

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			if (state is IReadOnlyCollection<KeyValuePair<string, object>> properties)
			{
				return ScopeContext.PushProperties(properties);
			}
			return LoggerNullScope.Instance;
		}
	}
}
