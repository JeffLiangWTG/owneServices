using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Integration
{
	public interface ILogger
	{
		void Log(LogType type, string message);
		void Log(LogType type, string message, Exception ex);
	}

	public interface ICategoryLogger<T> : ILogger where T : Enum
	{
		string GetContext();
		ILogger GetLogger();
		string GetCategoryName(T category);
		T GetDefaultCategory();
		bool ShouldLog(T category);
	}

	public class DummyLogger : ILogger
	{
		public void Log(LogType type, string message)
			=> Log(type, message, null);

		public void Log(LogType type, string message, Exception ex)
			=> OnLog?.Invoke(this, new DummyLogEventArgs(type, message, ex));

		public event EventHandler<DummyLogEventArgs> OnLog;
	}

	public class DummyLogEventArgs : EventArgs
	{
		public LogType Type { get; }
		public string Message { get; }
		public Exception Ex { get; }

		public DummyLogEventArgs(LogType type, string message, Exception ex)
		{
			Type = type;
			Message = message;
			Ex = ex;
		}
	}

	public static class LoggerExtension
	{
		public static void Log<T>(this ICategoryLogger<T> logger, T category, LogType logType, string message, Exception ex = null) where T : Enum
		{
			if (logger.ShouldLog(category))
			{
				var context = logger.GetContext();
				string categoryName = logger.GetCategoryName(category);
				logger.GetLogger().Log(logType, $"{(string.IsNullOrEmpty(context) ? "" : $"[{context}] ")}{(string.IsNullOrEmpty(categoryName) ? "" : $"[{categoryName}] ")}{message}", ex);
			}
		}

		public static void Log<T>(this ICategoryLogger<T> logger, LogType logType, string message, Exception ex = null) where T : Enum
		{
			Log(logger, logger.GetDefaultCategory(), logType, message, ex);
		}

		public static void Log<T>(this ICategoryLogger<T> logger, T category, string message, Exception ex = null) where T : Enum
		{
			Log(logger, category, LogType.Information, message, ex);
		}

		public static void Log<T>(this ICategoryLogger<T> logger, string message, Exception ex = null) where T : Enum
		{
			Log(logger, logger.GetDefaultCategory(), LogType.Information, message, ex);
		}

		public static void Debug(this ILogger logger, string message)
		{
			Argument.NotNull(logger, nameof(logger));
			logger.Log(LogType.Debug, message);
		}

		public static void Information(this ILogger logger, string message)
		{
			Argument.NotNull(logger, nameof(logger));
			logger.Log(LogType.Information, message);
		}

		public static void Informations(this ILogger logger, IEnumerable<string> messages)
		{
			Argument.NotNull(logger, nameof(logger));
			Argument.NotNull(messages, nameof(messages));
			foreach (var message in messages)
			{
				logger.Information(message);
			}
		}

		public static void Warning(this ILogger logger, string message)
		{
			Argument.NotNull(logger, nameof(logger));
			logger.Log(LogType.Warning, message);
		}

		public static void Error(this ILogger logger, string message)
		{
			Argument.NotNull(logger, nameof(logger));
			logger.Log(LogType.Error, message);
		}

		public static void Error(this ILogger logger, string message, Exception ex)
		{
			Argument.NotNull(logger, nameof(logger));
			logger.Log(LogType.Error, message, ex);
		}

		public static void ErrorAndReportException(this ILogger logger, string message, Exception ex)
		{
			Error(logger, message, ex);
			if (ex != null)
			{
				ErrorReporter.ReportOnce(message, ex);
			}
		}
	}

	public enum LogType
	{
		Error = 0,
		Warning = 1,
		Information = 2,
		Debug = 3,
	}
}

#region Test
#if DEBUG

namespace Enterprise.Integration
{
	[Serializable]
	public class LoggerForTest : ILogger
	{
		public void Log(LogType type, string message)
		{
			logEntries.Add(message);
		}

		public void Log(LogType type, string message, Exception ex)
		{
			logEntries.Add(message);
		}

		public IEnumerable<string> LogEntries
		{
			get { return logEntries; }
		}

		public void ClearLog()
		{
			logEntries.Clear();
		}

		readonly List<string> logEntries = new List<string>();

		public override string ToString()
		{
			return string.Join("\r\n", LogEntries);
		}
	}

	public class DetailedLoggerForTest : ILogger
	{
		public List<Tuple<LogType, string, Exception>> Logs { get; private set; } = new List<Tuple<LogType, string, Exception>>();

		public void Log(LogType type, string message)
		{
			Logs.Add(Tuple.Create(type, message, (Exception)null));
		}

		public void Log(LogType type, string message, Exception ex)
		{
			Logs.Add(Tuple.Create(type, message, ex));
		}
	}
}

#endif
#endregion
