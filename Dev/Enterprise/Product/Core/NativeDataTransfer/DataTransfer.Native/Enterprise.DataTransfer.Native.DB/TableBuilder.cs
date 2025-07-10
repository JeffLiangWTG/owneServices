using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.Schema;
using Enterprise.DataTransfer.Native.DB.Helpers;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.DB
{
	/// <summary>
	/// Table Builder, build Table Definition from Database Metadata
	/// It will first create column definitions 
	/// Then base on column definitions and Database Metadata,
	/// It will create constraint definitions/indexs
	/// </summary>
	public class TableBuilder
	{
		internal Table Construct(string tableName)
		{
			var table = new Table();
			BuildTableName(table, tableName);
			BuildColumns(table);
			BuildConstraints(table);
			return table;
		}
		readonly DbConnection dbConnection = DataSetContext.Connection;

		void BuildTableName(Table table, string tableName)
		{
			CheckTableName(tableName);
			table.Name = tableName;
		}

		void CheckTableName(string tableName)
		{
			if (tableName.IsEmpty())
			{
#if DEBUG
				throw new InvalidOperationException("Table Name should not be empty. <!-- Wasn't that useless? If you are making changes to the Odyssey schema, check that you have included information in ZArch.Schema to map your table's column prefix to a table name. If you are making changes to the reference databases, e.g. RefZZ for customs, hack your way out of this mess using method GetSynonymNameHackForTablesInOtherDatabases(). You're welcome. ");
#else
				throw new InvalidOperationException("Table Name should not be empty.");
#endif
			}
		}

		void BuildColumns(Table table)
		{
			IEnumerable<DataRow> columnInfos;

			lock (ColumnsDictionary)
			{
				if (!ColumnsDictionary.TryGetValue(table.Name, out columnInfos))
				{
					columnInfos = BuildColumnsForTable(table, Db.DatabaseName, table.Name);
					if (!columnInfos.Any())
					{
						// Try synonyms
						var baseObjectName = GetBaseObjectNameFromSynonym(table);
						if (baseObjectName != null && !string.IsNullOrEmpty(baseObjectName.ToString()))
						{
							// synonym name: RefDbEntUS_USCCountry
							// All possible scenario for the synonym object:
							// synonym object: [Test_RefDb_Ent_US].[dbo].[USCCountry]
							// synonym object: [CW-RefDb-Ent-US-000130].[dbo].[USCCountry]
							// synonym object: [CW-AG-RefDb-ORDWP4-CP1AS1-Ent-US-000130].[dbo].[USCCountry]
							// synonym object: [CW-AG-RefDb-EUAWSDBAAG003.cloud.corp-Ent-US-000130].[dbo].[USCCountry]

							var pattern = @"\[(.*?)\]\.\[(.*?)\]\.\[(.*?)\]";
							var match = Regex.Match(baseObjectName.ToString(), pattern);

							if (match.Success)
							{
								var databaseName = "[" + match.Groups[1].Value + "]";
								var tableName = "[" + match.Groups[3].Value + "]";

								columnInfos = BuildColumnsForTable(table, databaseName, tableName);
							}
						}
					}

					if (columnInfos.Any())
					{
						ColumnsDictionary.Add(table.Name, columnInfos);
					}
				}

				if (!columnInfos.Any())
				{
					throw new InvalidOperationException("Column names for object " + table.Name + " could not be obtained.");
				}
			}

			var columns = columnInfos.Select(x => BuildColumn(table, x)).ToArray();
			table.Columns = new ColumnCollection(table.Name, columns);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		IEnumerable<DataRow> BuildColumnsForTable(Table table, string databaseName, string tableName)
		{
			using (var cmd = dbConnection.Command(
				$@"SELECT
	cols.*
FROM
	{databaseName}.INFORMATION_SCHEMA.COLUMNS AS cols
	CROSS APPLY
	(
			SELECT TOP(1)
				Value = SUBSTRING(name, 1, CHARINDEX('_', name))
			FROM
				{databaseName}.sys.columns
			WHERE
				object_id = OBJECT_ID('{databaseName}..' + @TableName)
				AND NAME LIKE '%[_]PK'
	) AS Prefix
WHERE
	cols.table_name = @CleanTableName
	AND cols.column_name NOT like '%[_]TMP[_]HasMoveColumn%'
	AND cols.column_name like Value + '%'"))
			{
				cmd.AddParameter("@TableName", SqlDbType.NVarChar, 128, tableName);
				cmd.AddParameter("@CleanTableName", SqlDbType.NVarChar, 128, tableName.Replace("]", "").Replace("[", ""));
				var columns = cmd.ExecuteDataTable();
				return columns.Where(column => IsColumnInApplicationSchema(column.Field<string>("COLUMN_NAME"), table.Name));
			}
		}

		bool IsColumnInApplicationSchema(string columnName, string tableName)
		{
#if DEBUG
			if (IsTableColumnsAllowedForTest(tableName))
			{
				return true;
			}
#endif
			var schemaColumn = SchemaResolver.GetSchemaColumnSafe(columnName, tableName);
			return !schemaColumn?.IsComputed ?? false;
		}
		IApplicationSchemaResolver SchemaResolver => schemaResolver = schemaResolver ?? ObjectFactory.Get<IApplicationSchemaResolver>();
		IApplicationSchemaResolver schemaResolver;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		object GetBaseObjectNameFromSynonym(Table table)
		{
			using (var cmd = dbConnection.Command("select base_object_name from sys.synonyms where name = @TableName"))
			{
				cmd.AddParameter("@TableName", SqlDbType.NVarChar, 128, table.Name);
				var baseObjectName = cmd.ExecuteScalar();
				return baseObjectName;
			}
		}

		[WTG.StaticAnalysis.Annotation.ThreadSafe(WTG.StaticAnalysis.Annotation.ThreadSafeAttribute.Mechanism.Lock)]
		static readonly Dictionary<string, IEnumerable<DataRow>> ColumnsDictionary = new Dictionary<string, IEnumerable<DataRow>>();

		ColumnDef BuildColumn(Table table, DataRow row)
		{
			var builder = new ColumnBuilder(table);

			var columnSchema = new ColumnSchema
			{
				Name = row.Field<string>("COLUMN_NAME"),
				DataType = row.Field<string>("DATA_TYPE"),
				DefaultValue = row.Field<object>("COLUMN_DEFAULT"),
				IsNullable = row.Field<string>("IS_NULLABLE"),
				Length = row.Field<object>("CHARACTER_MAXIMUM_LENGTH"),
				Scale = row.Field<object>("NUMERIC_SCALE"),
				Precision = row.Field<object>("NUMERIC_PRECISION"),
			};
			return builder.Construct(columnSchema);
		}

		void BuildConstraints(Table table)
		{
			IEnumerable<DataRow> indexRows;

			lock (IndexesDictionary)
			{
				if (!IndexesDictionary.TryGetValue(table.Name, out indexRows))
				{
					indexRows = GetIndexRows(table);
					IndexesDictionary.Add(table.Name, indexRows);
				}
			}

			var indexInfos =
				from info in indexRows
				group (string)info["column_name"] by
					new Constraint
					{
						Name = info.Field<string>("index_name"),
						IsUnique = info.Field<bool>("is_unique"),
						Table = table
					}
					into g
				select g;

			table.CandidateKeyConstraints = indexInfos.Select(
				result =>
				{
					foreach (var columnName in result)
					{
						var column = table.Columns[columnName];
						result.Key.AddColumn(column);
					}
					return result.Key;
				}).Where(index => index.IsUnique && !index.Name.Contains("PK")).ToArray();
		}

		IEnumerable<DataRow> GetIndexRows(Table table)
		{
			IEnumerable<DataRow> indexRows;

			using (var cmd = BuildIndexesSqlCommand(table.Name)) {
				indexRows = cmd.ExecuteDataTable();
			}
			if (!indexRows.Any())
			{
				// Sigh.  Try synonyms. 
				var baseObjectName = GetBaseObjectNameFromSynonym(table);
				if (baseObjectName != null && !string.IsNullOrEmpty(baseObjectName.ToString()))
				{
					// e.g. "[odysseyDjc_RefDb_Ent_US].[dbo].[USCCountry]"
					var elements = baseObjectName.ToString().Split('.');
					if (elements.Length == 3)
					{
						var cleanTableName = elements[2].Replace("]", "").Replace("[", "");

						using (var cmd = BuildIndexesSqlCommand(cleanTableName, elements[0]))
						{
							indexRows = cmd.ExecuteDataTable();
						}
						if (!indexRows.Any() && RefDatabaseVersionMapHelper.TableViewMappings.ContainsValue(cleanTableName))
						{
							using (var cmd = BuildIndexesSqlCommand(RefDatabaseVersionMapHelper.TableViewMappings.FirstOrDefault(x => x.Value == cleanTableName).Key, elements[0]))
							{
								indexRows = cmd.ExecuteDataTable();
							}
						}
					}
				}
			}

			return indexRows.Where(row => IsColumnInApplicationSchema(row.Field<string>("COLUMN_NAME"), table.Name));
		}

		[WTG.StaticAnalysis.Annotation.ThreadSafe(WTG.StaticAnalysis.Annotation.ThreadSafeAttribute.Mechanism.Lock)]
		static readonly Dictionary<string, IEnumerable<DataRow>> IndexesDictionary = new Dictionary<string, IEnumerable<DataRow>>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DbCommand BuildIndexesSqlCommand(string tableName, string databaseNamePrefix = "")
		{
			var databaseNamePrefixWithDot = string.IsNullOrEmpty(databaseNamePrefix) ? "" : databaseNamePrefix + ".";
			var sql = string.Format(
				@"SELECT
					I.name AS {0}, 
					AC.name AS {1},
					CASE WHEN (I.is_unique = 1 AND I.filter_definition IS NOT NULL) THEN CAST(0 as bit) ELSE I.is_unique END as is_unique
				FROM			{2}sys.tables AS T  
					INNER JOIN	{2}sys.indexes I ON T.object_id = I.object_id   
					INNER JOIN	{2}sys.index_columns IC ON I.object_id = IC.object_id AND I.index_id = IC.index_id
					INNER JOIN	{2}sys.all_columns AC ON T.object_id = AC.object_id AND IC.column_id = AC.column_id
				WHERE T.is_ms_shipped = 0 
					AND I.type_desc <> @p0
					AND I.name NOT LIKE @p1
					AND IC.is_included_column = @p2
					AND T.name = @p3",
				"index_name",
				"column_name",
				databaseNamePrefixWithDot);

			var cmd = dbConnection.Command(sql);
			cmd.AddParameter("@p0", SqlDbType.VarChar, "HEAP");
			cmd.AddParameter("@p1", SqlDbType.VarChar, $"{IndexInfo.ONLINE_INDEX_PREFIX}%");
			cmd.AddParameter("@p2", SqlDbType.Bit, 0);
			cmd.AddParameter("@p3", SqlDbType.VarChar, tableName);

			return cmd;
		}

		#region Build Index Strings(Obsolete)

		// TODO: I don't like these two methods, remove it!!
		internal static IEnumerable<string> BuildIndexStrings(Table table)
		{
			var rawIndexes = GetRawIndexes(table.Name).Select(rawIndex => rawIndex.Replace(table.Prefix + "_", string.Empty));

			return rawIndexes.Select(BuildIndexString);
		}

		internal static string BuildIndexString(string rawIndex)
		{
			var formattedIndexList = new List<string>();
			var individualColumns = rawIndex.Split('_');
			foreach (var column in individualColumns)
			{
				if (column.Length > 2)
				{
					formattedIndexList.Add(column);
				}
				else
				{
					var relatedTableName = TableNameHelper.GetTableNameFromPrefix(column);
					var relatedTable = Table.Get(relatedTableName);
					// There is a recursive here!!
					// Might have problem when have Circular Dependencies
					var relatedTableIndexes = relatedTable.IndexStrings.Select(item => relatedTableName + "." + item);
					formattedIndexList.AddRange(relatedTableIndexes);
				}
			}
			return string.Join(",", formattedIndexList.ToArray());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		static IEnumerable<string> GetRawIndexes(string tableName)
		{
			var schema = EnterpriseSchema.GetTableSchema(tableName);
			if (schema == null)
			{
				return new List<string>();
			}

			var constantsType = schema.GetType().GetNestedType("Constants", BindingFlags.Static | BindingFlags.Public);
			var indexesType = constantsType.GetNestedType("Indexes", BindingFlags.Static | BindingFlags.Public);
			if (indexesType == null)
			{
				return new List<string>();
			}

			var fields = indexesType.GetFields(BindingFlags.Static | BindingFlags.Public);

			var rawIndexes = fields.Select(field =>
											{
												var indexName = (string)field.GetValue(null);
												return indexName.Replace("NR_UX__", string.Empty).Replace("FK_UX__", string.Empty);
											}).Where(x => !x.Contains("NR_RX__") && !x.Contains("FK_RX__"));

			return rawIndexes;
		}
		#endregion

#if DEBUG

		public static void ResetStaticCacheForTesting()
		{
			lock (IndexesDictionary)
			{
				IndexesDictionary.Clear();
			}
			lock (ColumnsDictionary)
			{
				ColumnsDictionary.Clear();
			}
		}

		public static Overridable<bool> AllowDummyColumnsForTest { get; } = new Overridable<bool>(false);

		bool IsTableColumnsAllowedForTest(string tableName) => AllowDummyColumnsForTest.Value && tableName.Contains("Dummy");

#endif
	}
}
