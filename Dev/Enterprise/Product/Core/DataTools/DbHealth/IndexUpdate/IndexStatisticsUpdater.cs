using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbHealth.Shared;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.DbHealth.IndexUpdate
{
	class IndexStatisticsUpdater : RunnerWithRegistryWorker
	{
		public IndexStatisticsUpdater(ILogger logger)
		{
			this.logger = logger;
		}

		internal virtual int EmptyStatisticsRowsThreshold => Env.Registry.ISU_EmptyStatisticsRowsThreshold;

#if DEBUG
		internal int DaysOfLastUpdateToAcceptAsValid_Exposed
		{
			get { return daysOfLastUpdateToAcceptAsValid; }
			set { daysOfLastUpdateToAcceptAsValid = value; }
		}
		internal void SetAutoStatsForFilteredIndexes_Exposed(DbConnection connection)
		{
			SetAutoStatsForFilteredIndexes(connection);
		}
		internal Action UserActionGetData_ForTest { get; set; }
		internal Action UserActionUpdate_ForTest { get; set; }
		internal Action UserActionPreUpdate_ForTest { get; set; }
		internal Action<DbCommand, Queue<string>> UserActionSetAutoStatsGetData_ForTest { get; set; }
		internal Action<DbCommand> UserActionSetAutoStatsUpdate_ForTest { get; set; }
		internal Action<string, DbCommand> UserActionStatisticsUpdate_ForTest { get; set; }
		void RunUserActionGetData_ForTest()
		{
			UserActionGetData_ForTest?.Invoke();
		}
		void RunUserActionUpdate_ForTest()
		{
			UserActionUpdate_ForTest?.Invoke();
		}
		void RunUserActionPreUpdate_ForTest()
		{
			UserActionPreUpdate_ForTest?.Invoke();
		}
		void RunUserActionSetAutoStatsGetData_ForTest(DbCommand cmd, Queue<string> stmts)
		{
			UserActionSetAutoStatsGetData_ForTest?.Invoke(cmd, stmts);
		}
		void RunUserActionSetAutoStatsUpdate_ForTest(DbCommand cmd)
		{
			UserActionSetAutoStatsUpdate_ForTest?.Invoke(cmd);
		}
		void RunUserActionStatisticsUpdate_ForsTest(string statsName, DbCommand cmd)
		{
			UserActionStatisticsUpdate_ForTest?.Invoke(statsName, cmd);
		}
#endif

		#region Overrides

		public override IDbRegistryWorker GetNewWorker()
		{
			return new IndexStatisticsUpdateRegistryWorker();
		}

		protected override string CustomExceptionMessage
		{
			get
			{
				return "Index Statistics Upgrade timed out";
			}
		}

		protected override int InitialTimeoutMs
		{
			get { return initialTimeoutInMilliseconds; }
		}

		protected override void HandleRegistryWorkerIsStuck()
		{
			logger.Log(LogType.Warning, RegistryWorker.IsStuckMessage);
		}

		#endregion // Overrides

		#region Update

		int daysOfLastUpdateToAcceptAsValid = 7;

		protected override void RunOnGivenDB(DbConnection connection, string dbName)
		{
			if (connection.IsDbWriteable(dbName))
			{
				logger.Log(LogType.Information, Invariant($"Updating Index Statistics: DB [{dbName}]"));

				if (RefDbTableNameResolver.IsSharedDatabase(dbName))
				{
					RunWithAppLock(dbName, () =>
					{
						RunUpdates(dbName, connection);
					});
				}
				else
				{
					RunUpdates(dbName, connection);
				}
			}
		}

		void RunUpdates(string dbName, DbConnection connection)
		{
			// requires changing DB because the sql function STATS_DATE() always returns NULL on remote DB.
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
#if DEBUG
				RunUserActionPreUpdate_ForTest();
#endif
				RunCommonTasks(connection);

				var currentObject = RegistryWorker.StartTableForDb(dbName);
				var tableFilter = string.Empty;
				if (!string.IsNullOrWhiteSpace(currentObject) && DataUtils.ObjectExists(connection, currentObject))
				{
					tableFilter = Invariant($"AND '[' + sch.name + '].[' + obj.name + ']' >= '{currentObject}'");
				}

				var objectsToUpdate = GetObjectsToUpdate(connection, daysOfLastUpdateToAcceptAsValid, tableFilter);
				UpdateStatistics(connection, objectsToUpdate);
			}
		}

		void RunCommonTasks(DbConnection connection)
		{
			logger.Information("Switching off autostats for LOB...");
			SwitchOffAutoStatsForLOB(connection);

			logger.Information("Creating common stats for filtered indexes...");
			CreateCommonStatsForFilteredIndexes(connection);

			logger.Information("Setting autostats for filtered indexes...");
			SetAutoStatsForFilteredIndexes(connection);
		}

		void SwitchOffAutoStatsForLOB(DbConnection connection)
		{
			var sql = @"
SELECT
	stmt = N'EXEC sys.sp_autostats N' + QUOTENAME(QUOTENAME(sch.name) + N'.' + QUOTENAME(obj.name), '''') + N', ''OFF'', ' + QUOTENAME(s.name) + N';'
FROM
	sys.schemas            AS sch
	JOIN sys.objects       AS obj ON obj.schema_id = sch.schema_id
	JOIN sys.stats         AS s   ON s.object_id = obj.object_id
	JOIN sys.stats_columns AS sc  ON sc.object_id = s.object_id AND sc.stats_id = s.stats_id
	JOIN sys.columns       AS c   ON c.object_id = sc.object_id AND c.column_id = sc.column_id
WHERE 1=1
	AND obj.is_ms_shipped = 0
	AND obj.type in ('U', 'V')
	AND s.no_recompute = 0
	AND s.auto_created = 1
	AND c.max_length = -1

";

			var stmts = new List<string>();
			connection.ExecuteReader(sql, (reader) => stmts.Add((string)reader["stmt"]));

			foreach (var stmt in stmts)
			{
				try
				{
					logger.Information(stmt);
					_ = connection.ExecuteNonQuery(stmt);
				}
				catch (SqlException ex) when (IsTimeoutOrLockTimeoutSqlException(ex))
				{
					logger.Warning($"Cannot apply the following changes: \"{stmt}\" due to blocking issue {ex}. Consider to run ISU at the most quiet time");
				}
			}
		}

		void CreateCommonStatsForFilteredIndexes(DbConnection connection)
		{
			var sql = Invariant($@"
;WITH
	filtered AS (
			SELECT
				sch_name = sch.name,
				obj_name = obj.name,
				col_name = c.name
				, s.object_id, s.stats_id
				, sc.column_id
			FROM
				sys.schemas            AS sch
				JOIN sys.objects       AS obj ON obj.schema_id = sch.schema_id
				JOIN sys.stats         AS s   ON s.object_id = obj.object_id
				JOIN sys.index_columns AS sc  ON sc.object_id = s.object_id AND sc.index_id = s.stats_id
					AND sc.is_included_column = 0
					AND sc.key_ordinal = 1
				JOIN sys.columns       AS c   ON c.object_id = sc.object_id AND c.column_id = sc.column_id
			WHERE 1=1
				AND obj.is_ms_shipped = 0
				AND obj.type in ('U', 'V')
				AND (s.auto_created = 0 AND s.user_created = 0)
				AND s.has_filter = 1
		)
	, covering AS (
			SELECT
				s.object_id, s.stats_id
				, column_id = ISNULL(ic.column_id, sc.column_id)
			FROM
				sys.stats                   AS s
				LEFT JOIN sys.stats_columns AS sc ON sc.object_id = s.object_id AND sc.stats_id = s.stats_id
					AND (s.auto_created = 1 OR s.user_created = 1)
					AND sc.stats_column_id = 1
				LEFT JOIN sys.index_columns AS ic ON ic.object_id = s.object_id AND ic.index_id = s.stats_id
					AND (s.auto_created = 0 AND s.user_created = 0)
					AND ic.is_included_column = 0
					AND ic.key_ordinal = 1
			WHERE 1=1
				AND s.has_filter = 0
		)
SELECT
	stmt = N'if (NOT EXISTS (SELECT NULL FROM sys.stats WHERE name = N' + QUOTENAME(N'{MetaData.StatisticsInfo.WTG_STATS_PREFIX}' + col_name, '''') + N'))'
		+ N' CREATE STATISTICS ' + QUOTENAME(N'{MetaData.StatisticsInfo.WTG_STATS_PREFIX}' + col_name) + N' ON ' + QUOTENAME(sch_name) + N'.' + QUOTENAME(obj_name) + N' (' + QUOTENAME(col_name) + N');'
FROM
	filtered
WHERE
	NOT EXISTS
	(
		SELECT NULL
		FROM
			covering
		WHERE 1=1
			AND covering.object_id = filtered.object_id
			AND covering.column_id = filtered.column_id
			AND covering.stats_id <> filtered.stats_id
	)

");

			var stmts = new List<string>();
			connection.ExecuteReader(sql, (reader) => stmts.Add((string)reader["stmt"]));

			foreach (var stmt in stmts)
			{
				try
				{
					logger.Information(stmt);
					_ = connection.ExecuteNonQuery(stmt);
				}
				catch (SqlException ex) when (IsTimeoutOrLockTimeoutSqlException(ex))
				{
					logger.Warning($"Cannot apply the following changes: \"{stmt}\" due to blocking issue {ex}. Consider to run ISU at the most quiet time");
				}
			}
		}

		void SetAutoStatsForFilteredIndexes(DbConnection connection)
		{
			var sql = Invariant($@"-- GetData for SetAutoStatsForFilteredIndexes
SELECT
	stmt = CONCAT(N'EXEC sys.sp_autostats N''', QUOTENAME(sch.name), N'.', QUOTENAME(obj.name), '''', N', ', IIF(par.rows < {EmptyStatisticsRowsThreshold}, N'''OFF''', N'''ON'''), ', ', QUOTENAME(s.name), N';')
FROM
	sys.schemas     AS sch WITH (NOLOCK) 
	JOIN sys.tables AS obj WITH (NOLOCK) ON obj.schema_id = sch.schema_id
	JOIN sys.stats  AS s   WITH (NOLOCK) ON s.object_id = obj.object_id

	LEFT JOIN
	(
		SELECT
			object_id, index_id
			, rows = SUM(rows)
		FROM
			sys.partitions WITH (NOLOCK)
		WHERE 1=1
			AND index_id > 1
		GROUP BY
			object_id, index_id
	) AS par ON par.object_id = s.object_id AND par.index_id = s.stats_id

WHERE 1=1
	AND obj.is_ms_shipped = 0
	AND (s.auto_created = 0 AND s.user_created = 0)
	AND s.has_filter = 1
	AND
	(1=2
		OR par.rows <  {EmptyStatisticsRowsThreshold} AND s.no_recompute = 0 -- currently ON  => switch to OFF
		OR par.rows >= {EmptyStatisticsRowsThreshold} AND s.no_recompute = 1 -- currently OFF => switch to ON
	)

");

			logger.Information(Invariant($"Checking autostats for filtered indexes (threshold: {EmptyStatisticsRowsThreshold:N0} rows)"));

			var stmts = new Queue<string>();
			connection.ExecuteReader(sql
				, cmd =>
				{
#if DEBUG
					RunUserActionSetAutoStatsGetData_ForTest(cmd, stmts);
#endif
				}
				, reader =>
				{
					stmts.Enqueue((string)reader["stmt"]);
				});

			using (connection.TemporarySetDeadlockPriority(DeadlockPriority.Max))
			// to avoid too many blocking events, we set lock timeout of 4s here, which is less than "blocked process threshold" (5s) set on sql servers
			using (connection.TemporarySetLockTimeout(TimeSpan.FromSeconds(4d)))
			{
				while (stmts.Count > 0)
				{
					var stmt = stmts.Dequeue();

					logger.Information(stmt);

					try
					{
						_ = connection.ExecuteNonQuery(stmt
							, cmd =>
							{
#if DEBUG
								RunUserActionSetAutoStatsUpdate_ForTest(cmd);
#endif
							});
					}
					catch (SqlException sqlException) when (IsTimeoutOrLockTimeoutSqlException(sqlException))
					{
						logger.Warning($"Cannot apply the following changes: \"{stmt}\" due to blocking issue. Consider to run ISU at the most quiet time");
					}
					catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.DeadlockError)
					{
						stmts.Enqueue(stmt);
						logger.Information("Re-queued due to deadlock: " + stmt);
					}
				}
			}
		}

		static bool IsTimeoutOrLockTimeoutSqlException(SqlException sqlException)
		{
			var exceptionType = new DbErrorMatch(sqlException).ExceptionType;
			return (exceptionType == DbErrorType.TimeoutExpired) || (exceptionType == DbErrorType.LockTimeoutExpired);
		}

		IEnumerable<IGrouping<string, DataRow>> GetObjectsToUpdate(DbConnection connection, int lastUpdateThreshold, string tableFilter)
		{
			// Note: the sql function STATS_DATE(...) returns NULL for statistics that never been created.
			// Therefore, we use ISNULL(STATS_DATE(...), 0) to treat that statistics as old one and need to be updated.
			var sql = Invariant($@"
SELECT
	obj_schema = sch.name,
	obj_name   = obj.name,
	obj_stats  = s.name
	, s.has_filter
	, s.no_recompute
FROM
	sys.schemas      AS sch
	JOIN sys.objects AS obj ON obj.schema_id = sch.schema_id
	JOIN sys.stats   AS s   ON s.object_id = obj.object_id
	CROSS APPLY sys.dm_db_stats_properties(s.object_id, s.stats_id) AS sp

	LEFT JOIN
	(
		SELECT
			object_id, index_id
			, rows = SUM(rows)
		FROM
			sys.partitions
		GROUP BY
			object_id, index_id
	) AS par ON par.object_id = s.object_id AND par.index_id = s.stats_id

	LEFT JOIN sys.indexes AS i ON i.object_id = s.object_id AND i.index_id = s.stats_id
		AND i.type > 2

WHERE 1=1
	AND obj.is_ms_shipped = 0
	AND obj.type in ('U', 'V')
	AND
	(
		s.no_recompute = 0
		OR s.no_recompute = 1 AND s.has_filter = 1 AND par.rows < {EmptyStatisticsRowsThreshold}
	)

	-- exclude columnstore index stats
	AND i.object_id is NULL 

	AND (sp.last_updated is NULL OR sp.last_updated < DATEADD(DAY, -{lastUpdateThreshold}, GETDATE()))
	AND (sp.modification_counter is NULL OR sp.modification_counter > 0)
	{tableFilter}

");

			try
			{
#if DEBUG
				RunUserActionGetData_ForTest();
#endif
				return Utilities.GetDataTableFromQuery(connection, sql)
					.Rows.Cast<DataRow>()
					.GroupBy(row => Invariant($"{row["obj_schema"].ToString().QuoteName()}.{row["obj_name"].ToString().QuoteName()}"));
			}
			catch (SqlException e) when (new DbErrorMatch(e).ExceptionType == DbErrorType.LockTimeoutExpired)
			{
				return null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void UpdateStatistics(DbConnection connection, IEnumerable<IGrouping<string, DataRow>> objectsToUpdate)
		{
			if (objectsToUpdate != null)
			{
				foreach (var obj in objectsToUpdate.OrderBy(o => o.Key))
				{
					try
					{
						var obj_name = obj.Key;

						logger.Log(LogType.Information, Invariant($"Updating Index Statistics: DB [{connection.CurrentDatabase}] - Table/View {obj_name}"));
						RegistryWorker.CurrentTableOrView = obj_name;

						foreach (var obj_stats in obj)
						{
							var remainingSeconds = CheckAndReturnRemainingTimeoutInSeconds();

							var stats_name = obj_stats["obj_stats"].ToString();
							var has_filter = (bool)obj_stats["has_filter"];
							var no_recompute = (bool)(obj_stats["no_recompute"]);
#if DEBUG
							RunUserActionUpdate_ForTest();
#endif

							var sql = SQL_UpdateStats(obj_name, stats_name, forceFullScan: has_filter, no_recompute);

							try
							{
								using (var cmd = connection.Command(sql, remainingSeconds))
								{
									_ = cmd.ExecuteNonQuery();
#if DEBUG
									RunUserActionStatisticsUpdate_ForsTest(stats_name, cmd);
#endif
								}
							}
							catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.TimeoutExpired)
							{
								if (has_filter)
								{
									sql = SQL_UpdateStats(obj_name, stats_name, false, no_recompute);
									_ = connection.ExecuteNonQuery(sql, remainingSeconds);
								}
							}
						}
					}
					catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.LockTimeoutExpired)
					{
						logger.Log(LogType.Information, ex.Message);
					}
					catch (SqlException ex)
					{
						logger.Log(LogType.Error, ex.Message);
					}
				}
			}

			RegistryWorker.CurrentTableOrView = string.Empty;
		}

		static string SQL_UpdateStats(string obj_name, string stats_name, bool forceFullScan, bool noRecompute)
		{
			var options = "";

			if (forceFullScan && !noRecompute)
			{
				options = " WITH FULLSCAN";
			}
			else
			if (forceFullScan && noRecompute)
			{
				options = " WITH FULLSCAN, NORECOMPUTE";
			}
			else
			if (!forceFullScan && noRecompute)
			{
				options = " WITH NORECOMPUTE";
			}

			return Invariant($"UPDATE STATISTICS {obj_name} ({stats_name.QuoteName()}){options};");
		}

		#endregion // Update

		#region Implementation

		readonly ILogger logger;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int initialTimeoutInMilliseconds = 1200000;

		#endregion // Implementation
	}
}
