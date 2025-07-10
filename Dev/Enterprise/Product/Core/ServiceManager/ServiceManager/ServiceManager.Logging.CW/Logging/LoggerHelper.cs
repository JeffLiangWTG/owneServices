using System;
using System.Linq;
using CargoWise.Data;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace ServiceManager.Logging.CW
{
	class LoggerHelper
	{
		public static void ConfigureTargetAndRule(INLogTargetFactory targetFactory, string loggerPattern, LogLevel minLevel)
		{
			var target = TryGetOrCreateTarget(targetFactory);
			if (target == null)
			{
				return;
			}

			lock (LogManager.Configuration.LoggingRules)
			{
				LogManager.Configuration.AddTarget(target);

				var loggingRule = LogManager.Configuration.FindRuleByName(loggerPattern);
				if (loggingRule == null)
				{
					LogManager.Configuration.LoggingRules.Add(new LoggingRule(loggerPattern, minLevel, target) { RuleName = loggerPattern });
				}
				else if (!loggingRule.Targets.Any(o => string.Equals(o.Name, target.Name, StringComparison.OrdinalIgnoreCase)))
				{
					loggingRule.Targets.Add(target);
				}
			}
		}

		internal static Target? TryGetOrCreateTarget(INLogTargetFactory targetFactory)
		{
			try
			{
				return targetFactory.GetOrCreateTarget();
			}
			catch (Exception ex)
			{
				if (ex is DatabaseUpgradeException)
				{
					Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
				}

				InternalLogger.Log(ex, LogLevel.Error, "Creating log target " + targetFactory.GetType().Name);
				return null;
			}
		}

		internal static bool IsEnabled(ILogger logger, Microsoft.Extensions.Logging.LogLevel logLevel) =>
			logLevel switch
			{
				Microsoft.Extensions.Logging.LogLevel.Trace => logger.IsTraceEnabled,
				Microsoft.Extensions.Logging.LogLevel.Debug => logger.IsDebugEnabled,
				Microsoft.Extensions.Logging.LogLevel.Information => logger.IsInfoEnabled,
				Microsoft.Extensions.Logging.LogLevel.Warning => logger.IsWarnEnabled,
				Microsoft.Extensions.Logging.LogLevel.Error => logger.IsErrorEnabled,
				Microsoft.Extensions.Logging.LogLevel.Critical => logger.IsFatalEnabled,
				_ => false,
			};

		internal static LogLevel ConvertLogLevel(Microsoft.Extensions.Logging.LogLevel logLevel) =>
			logLevel switch
			{
				Microsoft.Extensions.Logging.LogLevel.None => LogLevel.Off,
				Microsoft.Extensions.Logging.LogLevel.Debug => LogLevel.Debug,
				Microsoft.Extensions.Logging.LogLevel.Information => LogLevel.Info,
				Microsoft.Extensions.Logging.LogLevel.Warning => LogLevel.Warn,
				Microsoft.Extensions.Logging.LogLevel.Error => LogLevel.Error,
				Microsoft.Extensions.Logging.LogLevel.Critical => LogLevel.Fatal,
				_ => LogLevel.Trace,
			};
	}
}
