using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Async;
using CargoWise.Data;
using Enterprise.DbHealth.IndexUpdate;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ServiceManager.Tasks.DbMaintenance;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(DbMaintenanceTasks.GhostRecordCleanupCode, "Ghost Record Cleaner", "DBM", typeof(GhostRecordCleanerServiceTask),
	MinimumPeriod = "5minutes",
	MaximumPeriod = "60minutes",
	IsReadOnlyForWiseCloudClient = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "5minutes",
	ActiveByDefault = true)
]

namespace Enterprise.ServiceManager.Tasks.DbMaintenance
{
	class GhostRecordCleanerServiceTask : ServiceProviderImpl
	{
		public GhostRecordCleanerServiceTask()
			: this(Env.Registry.GhostRecordCleanUpThreshold, Env.Registry.GhostRecordCleanUpRebuildWaitMinutes, Env.Registry.GhostRecordProcessingThreadInterval, Env.Registry.GhostRecordMaxProcessingThreadCount, Env.Registry.GhostRecordCleanupKillBlockersThreshold, Env.Registry.GhostRecordCleanupCommandTimeOutThreshold, Env.Registry.GhostRecordCleanupOnlineRebuild, Env.Registry.LockTimeout)
		{
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "We require number of minutes for REBUILD INDEX statement")]
		internal GhostRecordCleanerServiceTask(int ghostCountThreshold, int rebuildLowPriorityWaitMinutes = 1, int processingThreadInterval = 5, int maxProcessingThreadCount = 10, int killBlockersThreshold = -1, int commandTimeOutThreshold = DbCommand.Timeout.Infinite, bool onlineRebuild = true, int connectionLockTimeout = DbConnection.LockTimeout.Default)
		{
			GhostCountThreshold = ghostCountThreshold;
			RebuildLowPriorityWaitMinutes = rebuildLowPriorityWaitMinutes;
			ProcessingThreadInterval = processingThreadInterval;
			MaxProcessingThreadCount = maxProcessingThreadCount;
			OnlineRebuild = onlineRebuild;
			ConnectionLockTimeout = connectionLockTimeout;

			if (ConnectionLockTimeout == DbConnection.LockTimeout.Infinite)
			{
				KillBlockersThreshold = -1;
			}
			else
			{
				KillBlockersThreshold = killBlockersThreshold;
			}

			KillBlockersWaitTime = GetKillBlockersWaitTime(KillBlockersThreshold, ConnectionLockTimeout);
			CommandTimeOut = GetCommandTimeout(commandTimeOutThreshold, connectionLockTimeout);
			OnlineOption = GetOnlineOption(OnlineRebuild, RebuildLowPriorityWaitMinutes);
		}

		internal int GhostCountThreshold { get; }

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "We require number of minutes for REBUILD INDEX statement")]
		internal int RebuildLowPriorityWaitMinutes { get; }

		internal int ProcessingThreadInterval { get; }
		internal int MaxProcessingThreadCount { get; }
		int KillBlockersThreshold { get; }
		TimeSpan KillBlockersWaitTime { get; }
		int CommandTimeOut { get; }
		bool OnlineRebuild { get; }
		int ConnectionLockTimeout { get; }
		string OnlineOption { get; }

		const int percentageDivisor = 100;

		TimeSpan LockTimeout => TimeSpan.FromSeconds(Env.Registry.GhostRecordCleanerLockTimeoutSeconds);

		internal virtual IEnumerable<(string schemaName, string tableName, string indexName)> GhostRecordCleanerIndexes
			=> GhostRecordCleanerIndexGroup.GhostRecordCleanerIndexes;

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			ServiceLogger.Log(LogType.Information, "Ghost record cleaner started.");
			var indexesToRebuild = GetIndexesToRebuild(youMustReactToThisToken).ToList();
			if (indexesToRebuild.Count == 0)
			{
				ServiceLogger.Log(LogType.Information, "Ghost record cleaner completed - no indexes to rebuild.");
				return;
			}

			CleanGhostRecords(indexesToRebuild, youMustReactToThisToken);
			ServiceLogger.Log(LogType.Information, "Ghost record cleaner completed.");
		}

		internal IEnumerable<(string schemaName, string tableName, string indexName)> GetIndexesToRebuild(CancellationToken cancellationToken)
		{
			var foundIndexes = new List<(string schemaName, string tableName, string indexName)>();
			var lockTimeout = LockTimeout;
			using (Db.Connection.TemporarySetLockTimeout(lockTimeout))
			{
				foreach (var ghostRecordIndex in GhostRecordCleanerIndexes)
				{
					cancellationToken.ThrowIfCancellationRequested();
					try
					{
						ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"Checking index {ghostRecordIndex.schemaName}.{ghostRecordIndex.tableName}.{ghostRecordIndex.indexName} for rebuild."));
						var ghostRecordCount = GetGhostRecordCountForIndex(ghostRecordIndex);
						if (ghostRecordCount > GhostCountThreshold)
						{
							ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"Adding index {ghostRecordIndex.schemaName}.{ghostRecordIndex.tableName}.{ghostRecordIndex.indexName} with {ghostRecordCount} ghost records to rebuild list."));
							foundIndexes.Add(ghostRecordIndex);
						}
					}
					catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.LockTimeoutExpired)
					{
						ServiceLogger.Log(LogType.Warning, FormattableString.Invariant($"Unable to check ghost records on {ghostRecordIndex.schemaName}.{ghostRecordIndex.tableName}.{ghostRecordIndex.indexName} due to a lock timeout of {lockTimeout.Seconds} seconds"));
					}
				}
			}

			return foundIndexes;
		}

		string GhostCountQuery => @"
SELECT
	SUM(s.ghost_record_count + s.version_ghost_record_count)
FROM
	sys.indexes AS i
	CROSS APPLY sys.dm_db_index_physical_stats(DB_ID(), i.object_id, i.index_id, NULL , 'SAMPLED') AS s
WHERE 1=1
	AND i.object_id = OBJECT_ID(@schemaName + N'.' + @tableName, 'U')
	AND i.name = @indexName
";

		[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "<Pending>")]
		internal virtual long GetGhostRecordCountForIndex((string schemaName, string tableName, string indexName) ghostRecordIndex)
		{
			using var command = Db.Connection.Command(GhostCountQuery, DbCommand.Timeout.Infinite);
			command.AddParameter("@schemaName", SqlDbType.NVarChar, 128, ghostRecordIndex.schemaName);
			command.AddParameter("@tableName", SqlDbType.NVarChar, 128, ghostRecordIndex.tableName);
			command.AddParameter("@indexName", SqlDbType.NVarChar, 128, ghostRecordIndex.indexName);

			var result = command.ExecuteScalar();
			return result != DBNull.Value ? (long)result : 0;
		}

		void CleanGhostRecords(IEnumerable<(string schemaName, string tableName, string indexName)> indexesToProcess, CancellationToken cancellationToken)
		{
			var mainThread = System.Environment.CurrentManagedThreadId;
			indexesToProcess
				.OrderBy(x => (x.schemaName, x.tableName, x.indexName))
				.GroupBy(index => (index.schemaName, index.tableName))
				.ParallelForEachDynamic(
					(x) =>
					{
						var currentThread = System.Environment.CurrentManagedThreadId;
						if (currentThread != mainThread)
						{
							using (Db.DisposableActionForDbConnection())
							{
								ProcessGhostRecordsForTable(x.Key.schemaName, x.Key.tableName, x.Select(y => y.indexName), logPrefix: $"Thread {currentThread}: ");
							}
						}
						else
						{
							ProcessGhostRecordsForTable(x.Key.schemaName, x.Key.tableName, x.Select(y => y.indexName), logPrefix: "Thread main: ");
						}
					},
					TimeSpan.FromSeconds(ProcessingThreadInterval),
					cancellationToken,
					MaxProcessingThreadCount);
		}

		void ProcessGhostRecordsForTable(string schemaName, string tableName, IEnumerable<string> indexes, string logPrefix)
		{
			foreach (var indexName in indexes)
			{
				try
				{
					ServiceLogger.Log(LogType.Debug,
						FormattableString.Invariant($"{logPrefix}Rebuilding index {schemaName}.{tableName}.{indexName}"));
					RebuildIndividualIndex(schemaName, tableName, indexName);
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.LockTimeoutExpired)
				{
					ServiceLogger.Log(LogType.Warning, FormattableString.Invariant($"{logPrefix}Unable to rebuild index {schemaName}.{tableName}.{indexName} for {RebuildLowPriorityWaitMinutes} due to it being locked"));
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.TimeoutExpired)
				{
					ServiceLogger.Log(LogType.Warning, FormattableString.Invariant($"{logPrefix}Unable to rebuild index {schemaName}.{tableName}.{indexName} because it was timeout after {CommandTimeOut} secs waiting"));
				}
				catch (AggregateException ex) when (ex.InnerException is SqlException sqlEx && new DbErrorMatch(sqlEx).ExceptionType == DbErrorType.TimeoutExpired)
				{
					ServiceLogger.Log(LogType.Warning, FormattableString.Invariant($"{logPrefix}Unable to rebuild index {schemaName}.{tableName}.{indexName} with observer because it was timeout after {CommandTimeOut} secs waiting"));
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.IndexOperationAlreadyInProgress)
				{
					ServiceLogger.Log(LogType.Warning, FormattableString.Invariant($"{logPrefix}Unable to rebuild index {schemaName}.{tableName}.{indexName} due to another process working on the index."));
				}
			}
		}

		internal virtual void RebuildIndividualIndex(string schemaName, string tableName, string indexName)
		{
			var rebuildSql = $@"
ALTER INDEX {indexName.QuoteName()} ON {schemaName.QuoteName()}.{tableName.QuoteName()} 
REBUILD WITH (ONLINE = {OnlineOption});";

			if (KillBlockersThreshold == -1
				|| ConnectionLockTimeout == DbConnection.LockTimeout.Infinite)
			{
				Db.Connection.ExecuteNonQuery(rebuildSql, CommandTimeOut);
			}
			else
			{
				var initialWait = KillBlockersWaitTime;
				new IndexRebuilderObserver(OnlineRebuild, initialWait, ObserverAction.KillBlockers, ObserverAction.KillBlockers)
					.Run(Db.Connection, rebuildSql, message => ServiceLogger.Log(LogType.Debug, message), CommandTimeOut);
			}
		}

		internal static TimeSpan GetKillBlockersWaitTime(int killBlockersThreshold, int connectionLockTimeout)
		{
			if (killBlockersThreshold == -1
				|| connectionLockTimeout == DbConnection.LockTimeout.Infinite)
			{
				return TimeSpan.MaxValue;
			}
			else
			{
				return TimeSpan.FromMilliseconds(connectionLockTimeout * killBlockersThreshold / percentageDivisor);
			}
		}

		internal static string GetOnlineOption(bool onlineRebuild, int rebuildLowPriorityWait)
		{
			return onlineRebuild
				? $"ON (WAIT_AT_LOW_PRIORITY (MAX_DURATION = {rebuildLowPriorityWait} MINUTES, ABORT_AFTER_WAIT = SELF))"
				: "OFF";
		}

		internal static int GetCommandTimeout(int commandTimeOutThreshold, int connectionLockTimeout)
		{
			if (connectionLockTimeout == DbConnection.LockTimeout.Infinite)
			{
				return DbCommand.Timeout.Infinite;
			}
			else
			{
				const int millisecDivisor = 1000;
				return connectionLockTimeout * commandTimeOutThreshold / percentageDivisor / millisecDivisor;
			}
		}
	}
}
