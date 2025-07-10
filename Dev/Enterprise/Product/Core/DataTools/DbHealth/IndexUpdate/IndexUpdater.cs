using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbHealth.Shared;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static System.FormattableString;

namespace Enterprise.DbHealth.IndexUpdate
{
	abstract class IndexUpdater : RunnerWithRegistryWorker
	{
		public IndexUpdater(ILogger logger)
		{
			Logger = logger;
			registrySettings = new IndexUpdateSettings();
		}

		protected ILogger Logger { get; }
		protected readonly IndexUpdateSettings registrySettings;

		internal const string LastRebuildPtyName = "LastRebuild";
		internal const string LastReorganisePtyName = "LastReorganise";
		internal const string PendingFillFactorReductionPtyName = "PendingFillFactorReduction";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected List<IndexUpdateInfo> QueryIndexUpdateInfo(DbConnection connection, string targetDb)
		{
			var currentIndex = RegistryWorker.StartTableForDb(connection.CurrentDatabase);
			var curIndexFilter = (string.IsNullOrEmpty(currentIndex))
				? string.Empty
				: Invariant($"AND sch.name + '.' + tab.name + '.' + ind.name >= '{currentIndex}'");

			var sql = Invariant($@"
SELECT
	schema_name                   = sch.name,
	table_name                    = tab.name,
	index_name                    = ind.name,
	current_fill_factor           = CASE ind.fill_factor WHEN 0 THEN 100 ELSE ind.fill_factor END,
	last_rebuild                  = indinfo.LastRebuild,
	last_reorganise               = indinfo.LastReorganise,
	pending_fill_factor_reduction = indinfo.PendingFillFactorReduction,
	fragmentation                 = stats.avg_fragmentation_in_percent,
	pages                         = stats.page_count,
	allow_page_locks              = ind.allow_page_locks
FROM
	[{targetDb}].sys.tables       AS tab
	JOIN [{targetDb}].sys.schemas AS sch ON sch.schema_id = tab.schema_id
	JOIN [{targetDb}].sys.indexes AS ind ON ind.object_id = tab.object_id
	CROSS APPLY [{targetDb}].sys.dm_db_index_physical_stats(DB_ID('{targetDb}'), tab.object_id, ind.index_id, 1, 'LIMITED') AS stats
	LEFT JOIN
	(
		SELECT
			SEP_SchemaName, SEP_MajorObjectName, SEP_MinorObjectName
			, [LastRebuild], [LastReorganise], [PendingFillFactorReduction]
		FROM
			(SELECT 
				SEP_Class,
				SEP_DatabaseNameSuffix,
				SEP_Name,
				SEP_Value,
				SEP_SchemaName,
				SEP_MajorObjectName,
				SEP_MinorObjectName
			FROM
				[{Db.DatabaseName}].dbo.StmExtendedProperty) AS stmProperty  -- Main database
			PIVOT
			(
				MIN(SEP_Value) FOR SEP_Name in ([LastRebuild], [LastReorganise], [PendingFillFactorReduction])
			) AS p
		WHERE 1=1
			AND SEP_Class              = @SEP_Class
			AND SEP_DatabaseNameSuffix = @SEP_DatabaseNameSuffix
	) AS indinfo ON indinfo.SEP_SchemaName = sch.name AND indinfo.SEP_MajorObjectName = tab.name AND indinfo.SEP_MinorObjectName = ind.name
WHERE 1=1
	AND tab.is_ms_shipped = 0
	AND stats.alloc_unit_type_desc = 'IN_ROW_DATA'
	AND ind.[type] in (1, 2)
	-- exclude huge tables
	AND tab.name NOT in ('StmALog')
	-- exclude client tables
	AND tab.name NOT LIKE 'Client%'
	{curIndexFilter}
ORDER BY
	schema_name, table_name, index_name
;");

			var sep_Class = nameof(ExtProperty.Class.Index);
			var sep_DatabaseNameSuffix = connection.DbSuffixByName(targetDb);

			var stats = new List<IndexUpdateInfo>();
			using (var cmd = connection.Command(sql, DbCommand.Timeout.Infinite))
			{
				cmd.AddParameter("@SEP_Class", SqlDbType.VarChar, 60, sep_Class);
				cmd.AddParameterBasedOnDbColumn("@SEP_DatabaseNameSuffix", sep_DatabaseNameSuffix, StmExtendedPropertySchema.SEP_DatabaseNameSuffix);

				Logger.Log(LogType.Information, "Starting ExecuteReader");
				using (var reader = cmd.ExecuteReader())
				{
					Logger.Log(LogType.Information, "Reader executed to retrieve index details");
					while (reader.Read())
					{
						var lastRebuild = GetDateExtendedPtyValueSafe(reader["last_rebuild"]);
						var lastReorganise = GetDateExtendedPtyValueSafe(reader["last_reorganise"]);
						var pendingFillFactorReduction = GetBoolExtendedPtyValueSafe(reader["pending_fill_factor_reduction"]);

						var schemaName = (string)reader["schema_name"];
						var tableName = (string)reader["table_name"];
						var indexName = (string)reader["index_name"];

						Logger.Log(LogType.Information, "Retrieved data for " + schemaName + "." + tableName + " (" + indexName + ")");

						stats.Add(new IndexUpdateInfo(
							registrySettings,
							dbName: targetDb,
							schemaName: schemaName,
							tableName: tableName,
							indexName: indexName,
							currentFillFactor: (int)reader["current_fill_factor"],
							lastRebuild: lastRebuild,
							lastReorganise: lastReorganise,
							pendingFillFactorReduction: pendingFillFactorReduction,
							fragmentation: (double)reader["fragmentation"],
							pages: (long)reader["pages"],
							allowPageLocks: (bool)reader["allow_page_locks"]
						));
					}
				}
			}

			return stats;
		}

		protected DateTime GetDateExtendedPtyValueSafe(object dateDbValue)
		{
			var result = DateTime.MinValue;

			if (dateDbValue != DBNull.Value)
			{
				try
				{
					result = SqlFormatInfo.FromSqlDate(dateDbValue.ToString());
				}
				catch (FormatException)
				{
					result = DateTime.MinValue;
				}
			}

			return result;
		}

		protected bool GetBoolExtendedPtyValueSafe(object boolDbValue)
		{
			return (boolDbValue is bool)
				? (bool)boolDbValue
				: (boolDbValue.ToString() == "1");
		}

		protected void LogInformation(ILogger logger, string message, IndexUpdateInfo indexToUpdate, string fillFactor = null, string additionalLogInfo = null)
		{
			logger.Information(string.Format(CultureInfo.InvariantCulture,
				"{0}: [{1}].[{2}].[{3}]; {4}Pages: {5}; Fragmentation: {6:F1}%{7}"
				, message                        // 0
				, indexToUpdate.DatabaseName     // 1
				, indexToUpdate.TableName        // 2
				, indexToUpdate.IndexName        // 3
				, (string.IsNullOrWhiteSpace(fillFactor))
					? string.Empty
					: Invariant($"Fill factor: {fillFactor}; ") // 4
				, indexToUpdate.PageCount        // 5
				, indexToUpdate.AvgFragmentation // 6
				, (string.IsNullOrWhiteSpace(additionalLogInfo))
					? string.Empty
					: System.Environment.NewLine + additionalLogInfo // 7
				));
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected static void SetIndexRebuildInfoPropertiesTransactionally(DbConnection connection, IndexUpdateInfo indexToUpdate, params KeyValuePair<string, object>[] properties)
		{
			using (var manager = connection.BeginTransactionWithManager())
			{
				foreach (var ptyPair in properties)
				{
					SetIndexRebuildInfoProperty(connection, indexToUpdate, ptyPair.Key, ptyPair.Value);
				}

				manager.CommitTransaction();
			}
		}

		protected static void SetIndexRebuildInfoProperty(DbConnection connection, IndexUpdateInfo indexInfo, string ptyName, object ptyValue)
		{
			var value = (ptyValue is DateTime)
				? SqlFormatInfo.ToSqlDateString((DateTime)ptyValue)
				: (ptyValue is bool)
					? ((bool)ptyValue)
						? "1"
						: "0"
					: ptyValue.ToString()
				;

			ExtProperty.Index.Update(connection, indexInfo.DatabaseName, indexInfo.SchemaName, indexInfo.TableName, indexInfo.IndexName, ptyName, value);
		}

		#region RunnerWithRegistryWorker

		public override IDbRegistryWorker GetNewWorker()
		{
			return new IndexRebuilderRegistryWorker();
		}

		protected override void HandleRegistryWorkerIsStuck()
		{
			Logger.Log(LogType.Warning, RegistryWorker.IsStuckMessage);
		}

		protected override int InitialTimeoutMs
		{
			get { return 1200000; } // not used anymore
		}

		#endregion
	}

	#region Helper classes

	internal enum IndexAction
	{
		None,
		Rebuild,
		Reorganize,
	}

	[ThreadSafe]
	internal class IndexUpdateInfo
	{
		public IndexUpdateInfo(IndexUpdateSettings registrySettings,
			string dbName, string schemaName, string tableName, string indexName, int currentFillFactor,
			DateTime lastRebuild, DateTime lastReorganise, bool pendingFillFactorReduction,
			double fragmentation, long pages, bool allowPageLocks)
		{
			RegistrySettings = registrySettings;
			this.DatabaseName = dbName;
			this.SchemaName = schemaName;
			this.TableName = tableName;
			this.IndexName = indexName;
			this.CurrentFillFactor = currentFillFactor;
			this.LastRebuild = lastRebuild;
			this.LastReorganise = lastReorganise;
			this.PendingFillFactorReduction = pendingFillFactorReduction;
			this.AvgFragmentation = fragmentation;
			this.PageCount = pages;
			this.AllowPageLocks = allowPageLocks;

			this.TargetAction = (IsOverRebuildThreshold())
				? IndexAction.Rebuild
				: (IsOverReorganiseThreshold())
					? IndexAction.Reorganize
					: IndexAction.None;
		}

		public readonly IndexUpdateSettings RegistrySettings;

		public readonly string DatabaseName;
		public readonly string SchemaName;
		public readonly string TableName;
		public readonly string IndexName;
		public readonly int CurrentFillFactor;
		public readonly DateTime LastRebuild;
		public readonly DateTime LastReorganise;
		public readonly bool PendingFillFactorReduction;
		public readonly double AvgFragmentation;
		public readonly long PageCount;
		public readonly bool AllowPageLocks;
		public readonly IndexAction TargetAction;
		public bool Retry { get; set; }

		public bool IsOverRebuildThreshold()
		{
			return AvgFragmentation > RegistrySettings.Rebuild_ThresholdPercentage;
		}

		public bool IsOverReorganiseThreshold()
		{
			return AvgFragmentation > RegistrySettings.Reorganize_ThresholdPercentage;
		}

		public bool IsRebuildAllowed(DateTime runDate)
		{
			return LastRebuild < runDate.AddDays(-IndexRebuilder.RebuildPeriodInDays);
		}

		public bool IsReorganiseAllowed(DateTime runDate)
		{
			return AllowPageLocks && LastReorganise < runDate.AddDays(-RegistrySettings.Reorganize_PeriodInDays);
		}

		public bool IsOverPageCountThreshold()
		{
			return PageCount > RegistrySettings.MinimumIndexPageCount;
		}
	}

	[Immutable]
	internal class IndexUpdateSettings
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public IndexUpdateSettings()
		{
			MinimumIndexPageCount = Env.Registry.ISU_MinimumIndexPageCount;

			Rebuild_ThresholdPercentage = Env.Registry.ISU_Rebuild_ThresholdPercentage;
			Rebuild_MaxWaitInMinutes = Env.Registry.ISU_Rebuild_MaxWaitInMinutes;
			Rebuild_AbortAfterWait = Env.Registry.ISU_Rebuild_AbortAfterWait;
			Rebuild_Online = Env.Registry.ISU_Rebuild_Online;
			Rebuild_UseObserver = Env.Registry.ISU_Rebuild_UseObserver;

			Reorganize_ThresholdPercentage = Env.Registry.ISU_Reorganize_Threshold;
			Reorganize_PeriodInDays = Env.Registry.ISU_Reorganize_PeriodInDays;

			MaxBacklogWaitTime = TimeSpan.FromMinutes(Env.Registry.ISU_MaxBacklogWaitTime_InMinutes);

			var current_cpu_available = 0;
			//var current_maxdop = 0;
			var sql = @"
SELECT
	current_cpu_available =
		CASE
			WHEN affinity_type = 2 THEN cpu_count
			ELSE (SELECT COUNT(DISTINCT cpu_id) FROM sys.dm_os_schedulers WHERE status = 'VISIBLE ONLINE')
		END
	--, current_maxdop = (SELECT value FROM sys.configurations WHERE name = 'max degree of parallelism')
FROM
	sys.dm_os_sys_info
";

			using (var connection = Db.NewAdminConnection())
			using (var cmd = connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					current_cpu_available = Convert.ToInt32(reader["current_cpu_available"], CultureInfo.InvariantCulture);
					//current_maxdop = Convert.ToInt32(reader["current_maxdop"], CultureInfo.InvariantCulture);
				}
			}

			var maxDopValue = Env.Registry.ISU_Rebuild_MaxDopPercentage * current_cpu_available / 100;
			Rebuild_MaxDop = (maxDopValue == 0) ? 1 : maxDopValue;

			var reorgValue = Env.Registry.ISU_Reorganize_MaxConcurrentProcessesPercentage * current_cpu_available / 100;
			Reorganize_MaxConcurrentProcesses = (reorgValue == 0) ? 1 : reorgValue;
		}

		internal IDisposable TemporarySetAbortAfterWait(string value)
		{
			var old = Rebuild_AbortAfterWait;
			Rebuild_AbortAfterWait = value;
			return new DisposableAction(() => Rebuild_AbortAfterWait = old);
		}

		public readonly int MinimumIndexPageCount;

		public readonly int Rebuild_ThresholdPercentage;
		public readonly int Rebuild_MaxDop;
		public readonly bool Rebuild_Online;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public readonly int Rebuild_MaxWaitInMinutes;
		[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "We can only set it via the temp setter. It's not great but the alternative would be a worse hacky piece of garbage.")]
		public string Rebuild_AbortAfterWait { get; private set; }
		public readonly bool Rebuild_UseObserver;

		public readonly int Reorganize_ThresholdPercentage;
		public readonly int Reorganize_MaxConcurrentProcesses;
		public readonly int Reorganize_PeriodInDays;
		public readonly TimeSpan MaxBacklogWaitTime;
	}

	#endregion // Helper classes
}
