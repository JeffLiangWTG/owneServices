using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.MemoryManagement;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public class SecondaryServerConnectionDetailsProvider
	{
		public SecondaryServerConnectionDetailsProvider(Action<Exception, string> showException = null, Action<string> addLogs = null)
		{
			cache = new ServerDelayCache();
			random = new Random();
			this.showException = showException;
			this.addLogs = addLogs;
		}
		readonly MemoryCacheWrapper cache;
		readonly Random random;
		readonly Action<Exception, string> showException;
		readonly Action<string> addLogs;

		internal const int CannotOpenDatabaseErrorNumber = 4060;
		internal const int LoginFailedForUserErrorNumber = 18456;

		protected virtual string DatabaseName
		{
			get { return Db.DatabaseName; }
		}

		internal bool IsReportingDbEnabled
		{
			get { return ObjectFactory.Get<ISystemDataRegistry>().UseReportingDbServerNames && SuitableDelay != 0 && (HasReportingDbServerNamesInRegistry || IsPartOfAlwaysOn) && GetAllReportServerNames().Length > 0; }
		}

#if DEBUG
		internal void ClearCache()
		{
			// I am not proud.
			cache.Clear();
		}
#endif

		internal DbConnectionForReportingWrapper GetNewConnectionCore(string dbUserName, string applicationNameSuffix = null)
		{
			var serverDelay = GetCachedValue();
			return GetNewConnectionCore(serverDelay.ServerName, dbUserName, applicationNameSuffix);
		}

#if DEBUG
		protected virtual
#endif
 DbConnectionForReportingWrapper GetNewConnectionCore(string reportServerName, string dbUserName, string applicationNameSuffix = null)
		{
			var reportDbConnection = NewSecondaryConnection(reportServerName, dbUserName, applicationNameSuffix);
			try
			{
				reportDbConnection.Connection.EnsureIsOpen();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				reportDbConnection.Connection.Dispose();
				throw;
			}
			return reportDbConnection;
		}

		/// <summary>
		/// Returns the first server that is less that noticed in registry [suitableDelay]
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Just for DB Server Info logs")]
		internal ServerDelay GetAnyUpToDateReportServer()
		{
			if (!IsReportingDbEnabled)
			{
				throw new InvalidOperationException("Invalid attempt to access the report DB when report server is not defined.");
			}

			string mostSuitableReportServerName = null;
			var delayFromPrimary = long.MaxValue;
			var maxSystemIdle = -1;

			var allReportServerNames = GetAllReportServerNames();
			var okServerWithoutSystemIdle = new List<(string serverName, long delay)>();
			for (var i = 0; i < allReportServerNames.Length; i++)
			{
				var serverName = allReportServerNames[i].Trim();
				var currentDelay = GetDelayBetweenPrimaryAndReportDatabaseInTicks(serverName);
				if (currentDelay <= SuitableDelay)
				{
					if (!TryGetSystemIdle(serverName, out var currentSystemIdle))
					{
						okServerWithoutSystemIdle.Add((serverName, currentDelay));
					}
					else if (currentSystemIdle > maxSystemIdle)
					{
						mostSuitableReportServerName = serverName;
						delayFromPrimary = currentDelay;
						maxSystemIdle = currentSystemIdle;
					}
				}
			}

			if (mostSuitableReportServerName == null)
			{
				if (okServerWithoutSystemIdle.Count > 0)
				{
					(mostSuitableReportServerName, delayFromPrimary) = okServerWithoutSystemIdle[random.Next(okServerWithoutSystemIdle.Count)];
				}
				else
				{
					AddLogs("Unable to connect to the reporting database servers, report will be run on primary server.");
					mostSuitableReportServerName = Db.ServerName;
				}
			}

			return new ServerDelay(mostSuitableReportServerName, delayFromPrimary);
		}

		ServerDelay GetCachedValue()
		{
			lock (cache)
			{
				var serverDelay = cache.GetCachedOrCalculateValue(ServerDelayCache.ServerDelayKey, () => GetAnyUpToDateReportServer());
				var delay = serverDelay.Delay;
				if (delay > SuitableDelay && serverDelay.InitialDelay < SuitableDelay && serverDelay.InitialDelay != long.MaxValue)
				{
					cache.Clear();
					serverDelay = cache.GetCachedOrCalculateValue(ServerDelayCache.ServerDelayKey, () => GetAnyUpToDateReportServer());
				}
				return serverDelay;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int GetDelayBetweenPrimaryAndReportDatabaseInMinutes()
		{
			var serverDelay = GetCachedValue();
			var diffInMinutes = TimeSpan.FromTicks(serverDelay.Delay).TotalMinutes;
			return (diffInMinutes > Int32.MaxValue) ? Int32.MaxValue : Convert.ToInt32(diffInMinutes);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "error message for developers")]

#if DEBUG
		protected virtual
#endif
 long GetDelayBetweenPrimaryAndReportDatabaseInTicks(string reportServerName)
		{
			try
			{
				return GetDelayInTicks(reportServerName);
			}
			catch (DbException sqlExceptionRaw)
			{
				var sqlException = new SqlExceptionWrapper(sqlExceptionRaw);
				if (sqlException != null &&
						(sqlException.Number == LoginFailedForUserErrorNumber && sqlException.Message.StartsWith("Login failed for user", StringComparison.OrdinalIgnoreCase) ||
							sqlException.Number == CannotOpenDatabaseErrorNumber && sqlException.Message.StartsWith("Cannot open database", StringComparison.OrdinalIgnoreCase)))
				{
					try
					{
						Db.FixReaderLogin(reportServerName);
						return GetDelayInTicks(reportServerName);
					}
					catch (DbException sqlExceptionTwice)
					{
						AddExceptionLogs(sqlExceptionTwice, reportServerName);
						ShowException(sqlExceptionTwice, reportServerName);
					}
				}
				else
				{
					AddExceptionLogs(sqlExceptionRaw, reportServerName);
				}

				return long.MaxValue;
			}
		}

#if DEBUG
		protected virtual
#endif
		bool TryGetSystemIdle(string reportServerName, out int systemIdle)
		{
			try
			{
				using (var connection = Db.NewAdminConnection(reportServerName, DatabaseName))
				using (connection.UseMasterDb())
				{
					var sqlText = $@"
SELECT
	TOP 1 recordx.value('(./Record/SchedulerMonitorEvent/SystemHealth/SystemIdle)[1]', 'int') as SystemIdle
FROM (
	SELECT CONVERT(xml, record) as recordx, timestamp
	FROM {TableName} with (nolock)
	WHERE ring_buffer_type = N'RING_BUFFER_SCHEDULER_MONITOR') as x
ORDER BY timestamp DESC
";
					systemIdle = connection.ExecuteScalar<int>(sqlText); // This is an SQL data provider for reports}
					return true;
				}
			}
			catch (Exception exception)
			{
				if (exception is DbException sqlException)
				{
					AddExceptionLogs(sqlException, reportServerName);
				}
				ShowException(exception, reportServerName);
				systemIdle = -1;
				return false;
			}
		}

		protected virtual string TableName => "sys.dm_os_ring_buffers";

		void AddExceptionLogs(DbException exception, string reportServerName) => AddLogs($"Unable to connect to the reporting database server {reportServerName}, error message: {exception.Message}"); // Just for DB Server Info logs

#if DEBUG
		protected
#endif
		void ShowException(Exception exception, string reportServerName)
		{
			if (showException != null)
			{
				showException(exception, reportServerName);
			}
		}

		void AddLogs(string message) => addLogs?.Invoke(message);

		long GetDelayInTicks(string reportServerName)
		{
			var sqlText = "SELECT Max(SL_PostedTimeUtc) FROM dbo.StmALog ";
			var primaryServerLastPost = new ZDateTime(Db.Connection.ExecuteScalar(sqlText)); // This is an SQL data provider for reports

			using (var reportDbConnection = GetNewConnectionCore(reportServerName, null, null))
			{
				var reportServerLastPost = new ZDateTime(reportDbConnection.Connection.ExecuteScalar(sqlText));

				return (primaryServerLastPost - reportServerLastPost).Ticks;
			}
		}

		protected virtual DbConnectionForReportingWrapper NewSecondaryConnection(string reportServerName, string dbUserName, string applicationNameSuffix = null)
		{
			if (string.IsNullOrWhiteSpace(dbUserName))
			{
				return new DbConnectionForReportingWrapper(Db.NewExtraRestrictedReaderConnection(reportServerName, DatabaseName, applicationNameSuffix));
			}
			else
			{
				return new DbConnectionForReportingWrapper(Db.NewExtraUnrestrictedWriterConnection(reportServerName, DatabaseName, applicationNameSuffix))
					.ImpersonateDbUser(dbUserName);
			}
		}

		internal
#if DEBUG
		protected virtual
#endif
 long SuitableDelay
		{
			get { return suitableDelay; }
		}
		readonly long suitableDelay = ObjectFactory.Get<ISystemDataRegistry>().ReportingDbServerThreshold.Ticks;

		readonly string[] reportServerNamesInRegistry = ObjectFactory.Get<ISystemDataRegistry>().GetReportingDbServerNames();

		bool HasReportingDbServerNamesInRegistry
		{
			get { return reportServerNamesInRegistry != null && reportServerNamesInRegistry.Length > 0; }
		}

#if DEBUG
		protected virtual
#endif
 string[] GetAllReportServerNames()
		{
			if (allReportServerNames == null)
			{
				if (HasReportingDbServerNamesInRegistry)
				{
					allReportServerNames = reportServerNamesInRegistry;
				}
				else
				{
					allReportServerNames = AlwaysOnHelper.AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(Db.DatabaseName, useCache: false) ?? Array.Empty<string>();
				}
			}

			return (string[])allReportServerNames.Clone();
		}
		string[] allReportServerNames;

#if DEBUG
		protected virtual
#endif
 bool IsPartOfAlwaysOn
		{
			get
			{
				if (!isPartOfAlwaysOn.HasValue)
				{
					isPartOfAlwaysOn = AlwaysOn.IsDbPartOfAlwaysOn(Db.Connection, Db.DatabaseName);
				}
				return isPartOfAlwaysOn.Value;
			}
		}
		bool? isPartOfAlwaysOn;

		class ServerDelayCache : MemoryCacheWrapper
		{
			protected override TimeSpan GetAbsoluteExpiry(string key)
			{
				return TimeSpan.FromMinutes(5);
			}

			internal const string ServerDelayKey = "MostUpToDateServer";
		}

		public class ServerDelay
		{
			public ServerDelay(string serverName, long delay)
			{
				this.serverName = serverName;
				this.delay = delay;
				sw = Stopwatch.StartNew();
			}

			readonly string serverName;
			readonly long delay;
			readonly Stopwatch sw;

			public string ServerName
			{
				get { return serverName; }
			}

			public long InitialDelay
			{
				get { return delay; }
			}

			public long Delay
			{
				get { return delay == long.MaxValue ? delay : delay + sw.ElapsedTicks; }
			}
		}
	}
}
