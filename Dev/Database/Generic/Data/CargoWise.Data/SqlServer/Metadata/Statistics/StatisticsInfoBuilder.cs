using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Database.Shared;

namespace CargoWise.Data
{
	public static partial class MetaData
	{
		public sealed partial class StatisticsInfo
		{
			public sealed class Builder
			{
				Builder(string schemaName, string tableName, string statisticsName
					, bool isAutoCreated, bool isUserCreated, bool isTemporary, bool isIncremental
					)
				{
					Argument.NotNullOrEmpty(schemaName, nameof(schemaName));
					Argument.NotNullOrEmpty(tableName, nameof(tableName));
					Argument.NotNullOrEmpty(statisticsName, nameof(statisticsName));

					SchemaName = schemaName;
					TableName = tableName;
					StatisticsName = statisticsName;

					keyColumns = new List<ColumnInfo>();
					options = new Dictionary<StatisticsOptions, int>();

					IsAutoCreated = isAutoCreated;
					IsUserCreated = isUserCreated;
					IsTemporary = isTemporary;
					IsIncremental = isIncremental;
				}

				public static Builder New(string schemaName, string tableName, string statisticsName
					, bool isAutoCreated = false, bool isUserCreated = true, bool isTemporary = false, bool isIncremental = false
					)
				{
					Argument.NotNullOrEmpty(schemaName, nameof(schemaName));
					Argument.NotNullOrEmpty(tableName, nameof(tableName));
					Argument.NotNullOrEmpty(statisticsName, nameof(statisticsName));

					return new Builder(schemaName, tableName, statisticsName, isAutoCreated, isUserCreated, isTemporary, isIncremental);
				}

				public readonly string SchemaName;
				public readonly string TableName;
				public readonly string StatisticsName;

				#region Columns

				#region Key

				readonly List<ColumnInfo> keyColumns;

				public Builder Key(params string[] columnNames)
				{
					Argument.NotNull(columnNames, nameof(columnNames));

					foreach (var name in columnNames)
					{
						if (!string.IsNullOrWhiteSpace(name))
						{
							keyColumns.Add(ColumnInfo.New(name));
						}
					}

					return this;
				}

				#endregion // Key

				#endregion // Columns

				#region Filter

				public string Filter;

				public bool HasFilter => !string.IsNullOrWhiteSpace(Filter);

				public Builder Where(string filterPredicate)
				{
					Filter = SqlUtils.RemoveEncasingParentheses(filterPredicate);

					return this;
				}

				#endregion // Filter

				#region Options

				readonly Dictionary<StatisticsOptions, int> options;

				public bool HasOptions => options.Count > 0;

				public Builder Option(StatisticsOptions option, int sampleValue = 0)
				{
					if (option == StatisticsOptions.FULLSCAN || option == StatisticsOptions.SAMPLE_PERCENT || option == StatisticsOptions.SAMPLE_ROWS)
					{
						if (options.ContainsKey(StatisticsOptions.FULLSCAN) || options.ContainsKey(StatisticsOptions.SAMPLE_PERCENT) || options.ContainsKey(StatisticsOptions.SAMPLE_ROWS))
						{
							throw new ArgumentException("These options are mutually exclusive and you have attempted to add two: FULLSCAN, SAMPLE_PERCENT and SAMPLE_ROWS", nameof(option));
						}
					}

					options.Add(option, sampleValue);

					return this;
				}

				#endregion // Options

				#region Flags

				public readonly bool IsAutoCreated;
				public readonly bool IsUserCreated;
				public readonly bool IsTemporary;
				public readonly bool IsIncremental; // Applies to: SQL Server 2014 through SQL Server 2016.

				#endregion // Flags

				#region MISC

				public string Definition
				{
					get
					{
						var keys = string.Join(", ", keyColumns);

						var where = "";
						if (HasFilter)
						{
							where = " WHERE (" + Filter + ")"; // Direct SQL query
						}

						var statsOptions = "";
						if (HasOptions)
						{
							statsOptions = " WITH " + string.Join(", ", options.Select(option => StatisticsInfo.GetOptionDefinition(option.Key, option.Value))); // Direct SQL query
						}

						var result = string.Format(CultureInfo.InvariantCulture,
							"STATISTICS {2} ON {0}.{1} ({3}){4}{5}" // Direct SQL query
							, SchemaName.QuoteName()     // 0
							, TableName.QuoteName()      // 1
							, StatisticsName.QuoteName() // 2
							, keys                       // 3
							, where                      // 4
							, statsOptions               // 5
							);

						return result;
					}
				}

				public StatisticsInfo Info
				{
					get
					{
						return new StatisticsInfo(SchemaName, TableName, StatisticsName
							, keyColumns, Filter, options
							, Definition
							, IsAutoCreated, IsUserCreated, IsTemporary, IsIncremental
							);
					}
				}

				#endregion // MISC
			}
		}
	}
}
