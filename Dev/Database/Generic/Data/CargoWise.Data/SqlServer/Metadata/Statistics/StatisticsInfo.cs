using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Database.Shared;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	public static partial class MetaData
	{
		#region StatisticsOptions

		public enum StatisticsOptions
		{
			FULLSCAN
			, SAMPLE_PERCENT // SAMPLE number { PERCENT | ROWS }
			, SAMPLE_ROWS    // SAMPLE number { PERCENT | ROWS }
			, NORECOMPUTE
			, INCREMENTAL    // = { ON | OFF }  // Applies to: SQL Server 2014 through SQL Server 2016.
		}

		#endregion // StatisticsOptions

		[Immutable]
		public sealed partial class StatisticsInfo
		{
			public const string WTG_STATS_PREFIX = "WTG_";

			StatisticsInfo(string schemaName, string tableName, string statisticsName
				, List<ColumnInfo> keyColumns, string filter, Dictionary<StatisticsOptions, int> options
				, string definition
				, bool isAutoCreated, bool isUserCreated, bool isTemporary, bool isIncremental
				)
			{
				Argument.NotNullOrEmpty(schemaName, nameof(schemaName));
				Argument.NotNullOrEmpty(tableName, nameof(tableName));
				Argument.NotNullOrEmpty(statisticsName, nameof(statisticsName));
				Argument.NotNull(keyColumns, nameof(keyColumns));
				Argument.NotNull(options, nameof(options));
				Argument.NotNullOrEmpty(definition, nameof(definition));

				SchemaName = schemaName;
				TableName = tableName;
				StatisticsName = statisticsName;

				var newKeyColumns = keyColumns.ToImmutableList();
				this.keyColumns = newKeyColumns;

				Filter = filter;

				var newOptions = options.ToImmutableDictionary();
				this.options = newOptions;

				Definition = definition;

				IsAutoCreated = isAutoCreated;
				IsUserCreated = isUserCreated;
				IsTemporary = isTemporary;
				IsIncremental = isIncremental;
			}

			public readonly string SchemaName;
			public readonly string TableName;
			public readonly string StatisticsName;
			public readonly string Definition;

			#region Columns

			#region Key

			readonly ImmutableList<ColumnInfo> keyColumns;

			public List<ColumnInfo> KeyColumns => new List<ColumnInfo>(keyColumns);

			#endregion // Key

			#endregion // Columns

			#region Filter

			public readonly string Filter;

			public bool HasFilter => !string.IsNullOrWhiteSpace(Filter);

			#endregion // Filter

			#region Options

			readonly ImmutableDictionary<StatisticsOptions, int> options;

			public bool HasOptions => options.Count > 0;

			public Dictionary<StatisticsOptions, int> Options => new Dictionary<StatisticsOptions, int>(options);

			static string GetOptionDefinition(StatisticsOptions option, int sampleValue)
			{
				switch (option)
				{
					case StatisticsOptions.FULLSCAN:
						return "FULLSCAN"; // Direct SQL query
					case StatisticsOptions.SAMPLE_PERCENT:
						return string.Format(CultureInfo.InvariantCulture, "SAMPLE {0} PERCENT", sampleValue); // Direct SQL query
					case StatisticsOptions.SAMPLE_ROWS:
						return string.Format(CultureInfo.InvariantCulture, "SAMPLE {0} ROWS", sampleValue); // Direct SQL query
					case StatisticsOptions.NORECOMPUTE:
						return "NORECOMPUTE"; // Direct SQL query
					case StatisticsOptions.INCREMENTAL: // Applies to: SQL Server 2014 through SQL Server 2016.
						return "INCREMENTAL = ON"; // Direct SQL query
					default:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The option '{0}' is not supported", option.ToString()));
				}
			}

			#endregion // Options

			#region Flags

			public readonly bool IsAutoCreated;
			public readonly bool IsUserCreated;
			public readonly bool IsTemporary;
			public readonly bool IsIncremental; // Applies to: SQL Server 2014 through SQL Server 2016.

			#endregion // Flags

			public StatisticsInfo Create(DbConnection connection)
			{
				Argument.NotNull(connection, nameof(connection));

				if (DataUtils.ObjectExistsNolock(connection, SchemaName, TableName))
				{
					if (!StatisticsInfo.Exists(connection, SchemaName, TableName, StatisticsName))
					{
						connection.ExecuteNonQuery(SQL_Create);
					}
				}

				return this;
			}

			public void Drop(DbConnection connection)
			{
				Argument.NotNull(connection, nameof(connection));

				if (StatisticsInfo.Exists(connection, SchemaName, TableName, StatisticsName))
				{
					connection.ExecuteNonQuery(SQL_Drop);
				}
			}

			public static StatisticsInfo LoadTop1(DbConnection connection, string schemaName, string tableName, string statisticsName)
			{
				Argument.NotNull(connection, nameof(connection));
				if (!(!string.IsNullOrWhiteSpace(schemaName) || !string.IsNullOrWhiteSpace(tableName) || !string.IsNullOrWhiteSpace(statisticsName)))
				{
					throw new ArgumentException("Invalid argument.", nameof(schemaName));
				}

				var result = LoadInfo(connection, true, schemaName, tableName, statisticsName);

				return (result.Count > 0) ? result[0] : null;
			}

			public static List<StatisticsInfo> Load(DbConnection connection, string schemaName, string tableName, string statisticsName)
			{
				Argument.NotNull(connection, nameof(connection));
				if (!(!string.IsNullOrWhiteSpace(schemaName) || !string.IsNullOrWhiteSpace(tableName) || !string.IsNullOrWhiteSpace(statisticsName)))
				{
					throw new ArgumentException("Invalid argument.", nameof(schemaName));
				}

				return LoadInfo(connection, false, schemaName, tableName, statisticsName);
			}

			public static bool Exists(DbConnection connection, string schemaName, string tableName, string statisticsName)
			{
				Argument.NotNull(connection, nameof(connection));
				Argument.NotNullOrEmpty(statisticsName, nameof(statisticsName));

				var schemaPredicate = (string.IsNullOrWhiteSpace(schemaName)) ? "" : "AND sch.name = @sch_name"; // Direct SQL query
				var tablePredicate = (string.IsNullOrWhiteSpace(tableName)) ? "" : "AND tab.name = @tab_name"; // Direct SQL query

				var sql = string.Format(CultureInfo.InvariantCulture, @"-- MetaData.StatisticsInfo.Exists
SELECT
	StatsExists = CONVERT(bit, CASE WHEN EXISTS (
			SELECT
				NULL
			FROM
				sys.schemas      AS sch WITH (NOLOCK)
				JOIN sys.objects AS tab WITH (NOLOCK) ON tab.schema_id = sch.schema_id
				JOIN sys.stats   AS sts WITH (NOLOCK) ON sts.object_id = tab.object_id 
			WHERE
				sts.name = @sts_name
				{0}
				{1}
		) THEN 1 ELSE 0 END)
"
					, schemaPredicate // 0
					, tablePredicate  // 1
					);

				using (var cmd = connection.Command(sql))
				{
					if (!string.IsNullOrWhiteSpace(schemaName))
					{
						cmd.AddParameter("@sch_name", SqlDbType.NVarChar, 128, schemaName);
					}

					if (!string.IsNullOrWhiteSpace(tableName))
					{
						cmd.AddParameter("@tab_name", SqlDbType.NVarChar, 128, tableName);
					}

					cmd.AddParameter("@sts_name", SqlDbType.NVarChar, 128, statisticsName);

					return Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
				}
			}

			#region Implementation

			string SQL_Create => "CREATE " + Definition + ";"; // Direct SQL query

			string SQL_Drop => "DROP STATISTICS " + SchemaName.QuoteName() + "." + TableName.QuoteName() + "." + StatisticsName.QuoteName() + ";"; // Direct SQL query

			static List<StatisticsInfo> LoadInfo(DbConnection connection, bool useTop1, string schemaName, string tableName, string statsName)
			{
				Argument.NotNull(connection, nameof(connection));

				var result = new List<StatisticsInfo>();

				var top_1 = (useTop1) ? "TOP (1)" : ""; // Direct SQL query
				var top_1_order_by = (useTop1) ? "ORDER BY Sch.name, Tab.name, Sts.name" : ""; // Direct SQL query

				var schPredicate = (string.IsNullOrWhiteSpace(schemaName)) ? "" : "AND Sch.name = @sch_name"; // Direct SQL query
				var tabPredicate = (string.IsNullOrWhiteSpace(tableName)) ? "" : "AND Tab.name = @tab_name"; // Direct SQL query
				var stsPredicate = (string.IsNullOrWhiteSpace(statsName)) ? "" : "AND Sts.name = @sts_name"; // Direct SQL query

				var sql = string.Format(CultureInfo.InvariantCulture, @"-- MetaData.StatisticsInfo.LoadInfo
;WITH
	data AS (
			SELECT {0}
				sch_name       = Sch.name,
				tab_id         = Tab.object_id,
				tab_name       = Tab.name,
				sts_id         = Sts.stats_id,
				sts_name       = Sts.name,
				sts_filter     = ISNULL(Sts.filter_definition, N'')
				, sts_is_auto_created = Sts.auto_created
				, sts_is_user_created = Sts.user_created
				, sts_is_temporary    = Sts.is_temporary
				, sts_is_incremental  = Sts.is_incremental
			FROM
				sys.schemas      AS Sch
				JOIN sys.objects AS Tab ON Tab.schema_id = Sch.schema_id
				JOIN sys.stats   AS Sts ON Sts.object_id = Tab.object_id
			WHERE
				Tab.is_ms_shipped = 0
				{2}
				{3}
				{4}
			{1}
		)
SELECT
	sch_name            = Sts.sch_name,
	tab_name            = Sts.tab_name,
	sts_name            = Sts.sts_name,
	sts_filter          = Sts.sts_filter,
	sts_is_auto_created = Sts.sts_is_auto_created,
	sts_is_user_created = Sts.sts_is_user_created,
	sts_is_temporary    = Sts.sts_is_temporary,
	sts_is_incremental  = Sts.sts_is_incremental,
	col_name            = Col.name
FROM
	data                        AS Sts
	JOIN sys.stats_columns      AS StsCol ON StsCol.object_id = Sts.tab_id AND StsCol.stats_id = Sts.sts_id
	LEFT JOIN sys.index_columns AS IndCol ON IndCol.object_id = StsCol.object_id AND IndCol.index_id = StsCol.stats_id
		AND IndCol.index_column_id = StsCol.stats_column_id
	JOIN sys.columns            AS Col    ON Col.object_id = StsCol.object_id AND Col.column_id = ISNULL(IndCol.column_id, StsCol.column_id)
ORDER BY
	Sts.sch_name, Sts.tab_name, Sts.sts_name,
	ISNULL(IndCol.key_ordinal, StsCol.stats_column_id),
	Col.name
"
					, top_1          // 0
					, top_1_order_by // 1
					, schPredicate   // 2
					, tabPredicate   // 3
					, stsPredicate   // 4
					);

				var prev_sch_name = "";
				var prev_tab_name = "";
				var prev_sts_name = "";
				Builder builder = null;
				using (var cmd = connection.Command(sql))
				{
					if (!string.IsNullOrWhiteSpace(schemaName))
					{
						cmd.AddParameter("@sch_name", SqlDbType.NVarChar, 128, schemaName);
					}

					if (!string.IsNullOrWhiteSpace(tableName))
					{
						cmd.AddParameter("@tab_name", SqlDbType.NVarChar, 128, tableName);
					}

					if (!string.IsNullOrWhiteSpace(statsName))
					{
						cmd.AddParameter("@sts_name", SqlDbType.NVarChar, 128, statsName);
					}

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var sch_name = (string)reader["sch_name"];
							var tab_name = (string)reader["tab_name"];
							var sts_name = (string)reader["sts_name"];

							var sts_filter = (string)reader["sts_filter"];
							var sts_is_auto_created = Convert.ToBoolean(reader["sts_is_auto_created"], CultureInfo.InvariantCulture);
							var sts_is_user_created = Convert.ToBoolean(reader["sts_is_user_created"], CultureInfo.InvariantCulture);
							var sts_is_temporary = Convert.ToBoolean(reader["sts_is_temporary"], CultureInfo.InvariantCulture);
							var sts_is_incremental = Convert.ToBoolean(reader["sts_is_incremental"], CultureInfo.InvariantCulture);

							var col_name = (string)reader["col_name"];

							if (prev_sch_name != sch_name || prev_tab_name != tab_name || prev_sts_name != sts_name)
							{
								if (builder != null)
								{
									result.Add(builder.Info);
								}

								prev_sch_name = sch_name;
								prev_tab_name = tab_name;
								prev_sts_name = sts_name;

								builder = StatisticsInfo.Builder.New(sch_name, tab_name, sts_name, sts_is_auto_created, sts_is_user_created, sts_is_temporary, sts_is_incremental)
									.Where(sts_filter);
							}

							builder?.Key(col_name);
						}

						if (builder != null)
						{
							result.Add(builder.Info);
						}
					}
				}

				return result;
			}

			public override string ToString()
			{
				return Definition;
			}

			#endregion // Implementation
		}
	}
}
