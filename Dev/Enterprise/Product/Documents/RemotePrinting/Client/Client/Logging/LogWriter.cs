using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NLog;
using NLog.Targets;

namespace Enterprise.RemotePrinting.Client
{
	public static class LogWriter
	{
		readonly static object RegisterStaticLock = new object();
		const string SystemInfoLoggerName = "SystemInfo";

		public static void Append(WebPrintEventLogEntryType logEntryType, string message, string specificLoggerName = DefaultNamePattern)
		{
			lock (RegisterStaticLock)
			{
				Logger logger;
				if (logEntryType == WebPrintEventLogEntryType.SystemInformation)
				{
					logger = LogManager.GetLogger(SystemInfoLoggerName);
				}
				else
				{
					logger = LogManager.GetLogger(specificLoggerName);
				}
				logger.Log(GetLogLevel(logEntryType), message);

				if (logEntryType == WebPrintEventLogEntryType.SystemInformation &&
					specificLoggerName != DefaultNamePattern)
				{
					Append(WebPrintEventLogEntryType.Information, message, specificLoggerName);
				}
			}
		}

		public static LogLevel GetLogLevel(WebPrintEventLogEntryType logEntryType)
		{
			switch (logEntryType)
			{
				case WebPrintEventLogEntryType.VerboseInformation:
					return LogLevel.Trace;
				case WebPrintEventLogEntryType.SystemInformation:
				case WebPrintEventLogEntryType.Information:
					return LogLevel.Info;
				case WebPrintEventLogEntryType.Warning:
					return LogLevel.Warn;
				case WebPrintEventLogEntryType.Error:
					return LogLevel.Error;
				default:
					return LogLevel.Info;
			}
		}

		public static void RegisterLogTarget(Target logTarget, WebPrintEventLogEntryType minEntryType, WebPrintEventLogEntryType maxEntryType, string namePattern = DefaultNamePattern)
		{
			lock (RegisterStaticLock)
			{
				var minLevel = GetLogLevel(minEntryType);
				var maxLevel = GetLogLevel(maxEntryType);

				if (minEntryType == WebPrintEventLogEntryType.SystemInformation)
				{
					// Add separate rule for system info
					RegisterLogTarget(logTarget, LogLevel.Info, LogLevel.Info, SystemInfoLoggerName);

					// Bring min level to next step up
					minLevel = LogLevel.Warn;
				}

				if (maxLevel >= minLevel)
				{
					RegisterLogTarget(logTarget, minLevel, maxLevel, namePattern);
				}
			}
		}

		static void RegisterLogTarget(Target logTarget, LogLevel minLevel, LogLevel maxLevel, string namePattern = DefaultNamePattern)
		{
			if (string.IsNullOrEmpty(logTarget.Name))
			{
				logTarget.Name = Guid.NewGuid().ToString();
			}

			var config = LogManager.Configuration ?? new NLog.Config.LoggingConfiguration();
			var rule = new NLog.Config.LoggingRule(logTarget.Name);
			rule.LoggerNamePattern = namePattern;
			rule.Targets.Add(logTarget);
			rule.EnableLoggingForLevels(minLevel, maxLevel);

			if (!config.LoggingRules.Any(x => x.RuleName == rule.RuleName))
			{
				config.LoggingRules.Add(rule);
			}
			LogManager.Configuration = config;
		}

		public static void UnregisterTargetAndDispose(Target logTarget)
		{
			lock (RegisterStaticLock)
			{
				var config = LogManager.Configuration;
				if (config != null && logTarget != null)
				{
					foreach (var loggingRule in config.LoggingRules.ToArray())
					{
						if (loggingRule.Targets.Contains(logTarget))
						{
							loggingRule.Targets.Remove(logTarget);
							if (loggingRule.Targets.Count == 0)
							{
								config.LoggingRules.Remove(loggingRule);
							}
						}
					}

					config.RemoveTarget(logTarget.Name);

					logTarget.Dispose();
				}
				LogManager.Configuration = config;
			}
		}

		public static void UnregisterAllLogTargets()
		{
			lock (RegisterStaticLock)
			{
				var config = LogManager.Configuration;
				if (config != null)
				{
					LogManager.Configuration = null;

					foreach (var logTarget in config.AllTargets.ToArray())
					{
						logTarget.Dispose();
					}
				}
			}
		}

		#region FileTarget
		public static FileTarget RegisterFileTarget(string filePath, string filePrefix, WebPrintEventLogEntryType minEntryType, WebPrintEventLogEntryType maxEntryType)
		{
			return RegisterFileTarget(filePath, filePrefix, minEntryType, maxEntryType, DefaultNamePattern, DefaultMaxLogFileSize);
		}

		public static FileTarget RegisterFileTarget(string filePath, string filePrefix, WebPrintEventLogEntryType minEntryType, WebPrintEventLogEntryType maxEntryType, string namePattern)
		{
			return RegisterFileTarget(filePath, filePrefix, minEntryType, maxEntryType, namePattern, DefaultMaxLogFileSize);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "RegisterFileTarget")]
		public static FileTarget RegisterFileTarget(string filePath, string filePrefix, WebPrintEventLogEntryType minEntryType, WebPrintEventLogEntryType maxEntryType, string namePattern, long filesize)
		{
			var name = filePrefix + "_" + minEntryType.ToString() + "_" + Process.GetCurrentProcess().Id;
			var fileName = filePrefix + "_" + "${date:format=yyyyMMdd}" + ".txt";
			var fullFileName = Path.Combine(filePath, fileName);
			var fileTarget = new FileTarget
			{
				Name = name,
				FileName = fullFileName,
				ArchiveAboveSize = filesize,
				ArchiveOldFileOnStartup = true,
				ArchiveNumbering = ArchiveNumberingMode.Sequence,
				ConcurrentWrites = true,
				MaxArchiveFiles = 1000,
				Layout = "[${longdate}] ${message}",
				Encoding = Encoding.UTF8,
				KeepFileOpen = false
			};

			RegisterLogTarget(fileTarget, minEntryType, maxEntryType, namePattern);

			return fileTarget;
		}

		public static DirectoryInfo GetOutputDirectory()
		{
			var directoryPath = Constants.LogsFolder;

			var result = new DirectoryInfo(directoryPath);

			if (!result.Exists)
			{
				result.Create();
			}

			return result;
		}

		const int DefaultMaxLogFileSize = 1024 * 1024;
		const string DefaultNamePattern = "*";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Static fields in this class do not need to be thread-static")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Static string")]
		public static string FileNamePrefix = "Log"; // Next Checkin move this to a class, Enum maybe with all the others

		#endregion

		#region TextBoxTarget

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Layout")]
		public static TextBoxBaseTarget RegisterTextBoxBaseTarget(TextBoxBase textBox, WebPrintEventLogEntryType minEntryType, WebPrintEventLogEntryType maxEntryType)
		{
			var outputBox = new TextBoxBaseTarget(textBox)
			{
				Name = textBox.Name,
				Layout = "[${longdate}] ${message}",
			};
			RegisterLogTarget(outputBox, minEntryType, maxEntryType);

			return outputBox;
		}

		#endregion

		#region EventLogTarget

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "eventLogTarget")]
		public static EventLogTarget RegisterEventLogTarget(string source, WebPrintEventLogEntryType minEntryType, WebPrintEventLogEntryType maxEntryType)
		{
			var eventLogTarget = new EventLogTarget(WebPrintEventLog);
			eventLogTarget.Source = source;
			eventLogTarget.Layout = "${message}";
			eventLogTarget.Log = "Application";

			RegisterLogTarget(eventLogTarget, minEntryType, maxEntryType);

			return eventLogTarget;
		}

		const string WebPrintEventLog = "WebPrintEventLog";

		#endregion
	}
}
