using System;
using IntegrationLogging = Enterprise.Integration;
using SynchroniserLogging = WTG.Data.SqlDbSecuritySynchroniser;

namespace Enterprise.SqlSecurity
{
	static class Extensions
	{
		internal static IntegrationLogging.LogType ToIntegrationLoggerLogType(this SynchroniserLogging.LogType logType)
		{
			switch (logType)
			{
				case SynchroniserLogging.LogType.TimerStats:
				case SynchroniserLogging.LogType.NothingToDo:
				case SynchroniserLogging.LogType.Statements:
					return IntegrationLogging.LogType.Information;
				case SynchroniserLogging.LogType.BatchRunError:
				case SynchroniserLogging.LogType.StatementsAfterSync:
					return IntegrationLogging.LogType.Warning;
				case SynchroniserLogging.LogType.SyncError:
					return IntegrationLogging.LogType.Error;
				default:
					throw new NotSupportedException($"Unsupported log type '{logType}'");
			}
		}
	}
}
