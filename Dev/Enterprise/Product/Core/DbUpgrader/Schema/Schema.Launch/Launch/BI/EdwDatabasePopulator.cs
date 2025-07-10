using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Bi.Deployment.AnalysisServices;
using CargoWise.Data;
using CargoWise.Integration;
using Enterprise.ChangeDataCapture.Common;
using static CargoWise.Bi.Configuration.DataSets.BiAutomationConfigDataSet;

namespace Enterprise.DbUpgrader.Schema
{
	public class EdwDatabasePopulator
	{
		public EdwDatabasePopulator(DbConnection biConnection)
		{
			this.biConnection = biConnection;
		}
		readonly DbConnection biConnection;

		protected List<string> tablesForInitialLoad = new List<string>();
		protected List<string> customTablesForInitialLoad = new List<string>();

		public void Run()
		{
			GetTablesForPartialInitialLoad();
			GetCustomTablesForPartialInitialLoad();
			PopulateTableConfiguration();
			PopulateTableState();
			DetermineViewChangesForDenormalizedTables();
			DetermineViewChangesForCustomTables();
			InitiateInitialLoadIfRequired();
			PopulateSsasInfo();
			FlagCdcReEnabledTablesAsRequiringInitialLoad();
		}

		#region Partial Initial Load Settings

		public void FlagCdcReEnabledTablesAsRequiringInitialLoad()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(Db.EdwDatabaseName))
			{
				if (CdcDatabase.IsEnabled(Db.Connection, Db.DatabaseName))
				{
					var sqlText = "SELECT capture_instance FROM cdc.change_tables WHERE start_lsn > @startLsn AND capture_instance IN (SELECT * FROM @captureInstances)";

					using (((ICurrentDbControl)Db.Connection).UseDatabase(Db.DatabaseName))
#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
					using (var cmd = Db.Connection.Command(sqlText))
					{
						cmd.AddParameter("@startLsn", SqlDbType.Binary, GetEdwMaxProcessedLsn());
						cmd.AddTableValuedParameter("@captureInstances", "dbo.TVP_varchar_250", ConfigData.EdwTableConfig.Select(t => string.Format(CultureInfo.InvariantCulture, "{0}_{1}", t.SourceSchema, t.StagingTable)).Distinct());

						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								var instance = reader.GetString(0);
								if (!string.IsNullOrEmpty(instance))
								{
									AddTableForPartialInitialLoad(instance.Replace("_", "."));
								}
							}
						}
					}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
					InitiateInitialLoadIfRequired();
					if (tablesForInitialLoad.Any())
					{
						biConnection.ExecuteNonQuery($"TRUNCATE TABLE [{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[MasterState]");
					}
				}
				else
				{
					var sqlText = string.Format(CultureInfo.InvariantCulture,
@"UPDATE [{0}].[{1}].[StagingTableState]
SET
	CurrentMaxLsn = @lsn,
	CurrentState = @state,
	InitialLoadRequired = @initialLoadRequired

TRUNCATE TABLE [{0}].[{1}].[MasterState]",
						Db.EdwDatabaseName,
						BiConstants.BiAdminSchemaName);

#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
					using (var cmd = biConnection.Command(sqlText))
					{
						cmd.AddParameter("@lsn", SqlDbType.Binary, new byte[] { 0 });
						cmd.AddParameter("@state", SqlDbType.VarChar, "New");
						cmd.AddParameter("@initialLoadRequired", SqlDbType.Bit, true);

						cmd.ExecuteNonQuery();
					}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
				}
			}
		}

		byte[] GetEdwMaxProcessedLsn()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(Db.EdwDatabaseName))
			{
				return BiMasterState.GetParameterAsByteArray(biConnection, BiConstants.MaxLsnToBeProcessed);
			}
		}

		void GetTablesForPartialInitialLoad()
		{
			var stagingTableList = GetStagingTableList();
			AddNewTablesForPartialInitialLoad(stagingTableList);

			var regex = new Regex(@"\/\*(?<modelColumnName>[A-Z|a-z|\s]*)\*\/\s+(?<expression>.+?)(,|\s*FROM)", RegexOptions.IgnoreCase);
			foreach (var stagingTable in stagingTableList)
			{
				foreach (var edwTable in ConfigData.EdwTableConfig.Where(t => t.Schema + "." + t.Name == stagingTable.ModelName && t.StagingTable == stagingTable.Name && t.TransformId == stagingTable.TransformId))
				{
					var deployedColumnList = new List<ModelColumnObject>();
					foreach (Match match in regex.Matches(stagingTable.InitialLoadQuery))
					{
						var modelColumnName = (match.Groups["modelColumnName"].Value).Trim('\r', '\n', ' ');
						var expression = (match.Groups["expression"].Value).Trim('\r', '\n', ' ');
						deployedColumnList.Add(new ModelColumnObject(modelColumnName, expression));
					}

					var configuredColumnList = new List<ModelColumnObject>();
					var initialLoadQuery = ConfigData.GetInitialLoadQueryForEdwTable(edwTable);
					foreach (Match match in regex.Matches(initialLoadQuery))
					{
						var modelColumnName = (match.Groups["modelColumnName"].Value).Trim('\r', '\n', ' ');
						var expression = (match.Groups["expression"].Value).Trim('\r', '\n', ' ');
						configuredColumnList.Add(new ModelColumnObject(modelColumnName, expression));
					}

					#region New Model Columns

					if (configuredColumnList.Any(cc => !deployedColumnList.Select(dc => dc.Name).Contains(cc.Name)))
					{
						AddTableForPartialInitialLoad(stagingTable);
						continue;
					}

					#endregion

					#region Changed Column Expression

					foreach (var deployedColumn in deployedColumnList)
					{
						var configuredColumn = configuredColumnList.FirstOrDefault(cc => cc.Name == deployedColumn.Name);
						if (configuredColumn != null && deployedColumn.Expression != configuredColumn.Expression)
						{
							AddTableForPartialInitialLoad(stagingTable);
							break;
						}
					}

					#endregion
				}
			}
		}

		void GetCustomTablesForPartialInitialLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"SELECT ModelSchemaName, ModelTableName, ViewName, InitialLoadQuery, IncrementalLoadQuery FROM [{0}].[CustomTableConfiguration]", BiConstants.BiAdminSchemaName);
			using (var cmd = biConnection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var schemaName = reader["ModelSchemaName"].ToString();
					var tableName = reader["ModelTableName"].ToString();
					var viewName = reader["ViewName"].ToString();
					var initialLoadQuery = reader["InitialLoadQuery"].ToString();
					var incrementalLoadQuery = reader["IncrementalLoadQuery"].ToString();

					var customTable = ConfigData.EdwCustomTableConfig.FirstOrDefault(t => t.Schema == schemaName && t.Name == tableName);
					if (customTable != null)
					{
						if (customTable.ViewName != viewName ||
							customTable.InitialLoadQuery != initialLoadQuery ||
							customTable.IncrementalLoadQuery != incrementalLoadQuery)
						{
							customTablesForInitialLoad.Add(schemaName + "." + tableName);
						}
					}
				}
			}
		}

		List<StagingTableObject> GetStagingTableList()
		{
			List<StagingTableObject> stagingTableList = new List<StagingTableObject>();
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"SELECT SourceTableName, ModelSchemaName + '.' + ModelTableName AS ModelName, TransformId, InitialLoadQuery FROM [{0}].[{1}].[TransformTableConfiguration]", Db.EdwDatabaseName, BiConstants.BiAdminSchemaName);

			using (var reader = biConnection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					var sourceTableName = reader["SourceTableName"].ToString();
					var modelName = reader["ModelName"].ToString();
					var transformId = Convert.ToInt32(reader["TransformId"], CultureInfo.InvariantCulture);
					var query = reader["InitialLoadQuery"].ToString();
					stagingTableList.Add(new StagingTableObject(sourceTableName, modelName, transformId, query));
				}
		}

			return stagingTableList;
		}

		void AddNewTablesForPartialInitialLoad(List<StagingTableObject> stagingTableList)
		{
			var clientHookLoader = ObjectFactory.Get<IClientHookLoader>();
			var includeEdiTables = clientHookLoader?.ClientHook?.UniqueId == "EDI";

			var edwTablesForInitialLoad = ConfigData.EdwTableConfig.Where(t => !stagingTableList.Any(st => st.Name == t.StagingTable && st.TransformId == t.TransformId)).Select(et => et.StagingTable);
			var cdcTablesForInitialLoad = ConfigData.CdcTableConfig.Where(t => edwTablesForInitialLoad.Contains(t.SourceTable) && (!t.IsEdiClient || includeEdiTables));

			foreach (var table in cdcTablesForInitialLoad)
			{
				AddTableForPartialInitialLoad(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", table.SourceSchema, table.SourceTable));
			}
		}

		void AddTableForPartialInitialLoad(StagingTableObject table)
		{
			AddTableForPartialInitialLoad(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", table.Schema, table.Name));
		}

		void AddTableForPartialInitialLoad(string tableName)
		{
			if (!tablesForInitialLoad.Contains(tableName))
			{
				tablesForInitialLoad.Add(tableName);
			}
		}

		class StagingTableObject
		{
			public StagingTableObject(string sourceTableName, string modelName, int transformId, string query)
			{
				Schema = sourceTableName.Split('.')[0];
				Name = sourceTableName.Split('.')[1];
				ModelName = modelName;
				TransformId = transformId;
				InitialLoadQuery = query;
			}
			public string Schema { get; set; }
			public string Name { get; set; }
			public string ModelName { get; set; }
			public int TransformId { get; set; }
			public string InitialLoadQuery { get; set; }
		}

		class ModelColumnObject
		{
			public ModelColumnObject(string name, string expression)
			{
				Name = name;
				Expression = expression;
			}
			public string Name { get; set; }
			public string Expression { get; set; }
		}

		#endregion

		#region Populate Table Configuration

		void TruncateTableConfiguration()
		{
			biConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, @"
TRUNCATE TABLE [{0}].[StagingTableConfiguration]
TRUNCATE TABLE [{0}].[TransformTableConfiguration]
TRUNCATE TABLE [{0}].[CustomTableConfiguration]
TRUNCATE TABLE [{0}].[ModelTableConfiguration]",
				BiConstants.BiAdminSchemaName));
		}

		void PopulateTableConfiguration()
		{
			TruncateTableConfiguration();
			PopulateStagingTableConfiguration();
			PopulateTransformTableConfiguration();
			PopulateCustomTableConfiguration();
			PopulateModelTableConfiguration();
		}

		void PopulateStagingTableConfiguration()
		{
			var tableValueList = new List<string>();
			var clientHookLoader = ObjectFactory.Get<IClientHookLoader>();
			var includeEdiTables = clientHookLoader?.ClientHook?.UniqueId == "EDI";
			foreach (var table in ConfigData.CdcTableConfig.Where(t => t.TableInEdw && (!t.IsEdiClient || includeEdiTables)))
			{
				var columnList = "'@SourceSchemaName', '@StagingSchemaName', '@StagingTableName', '@EdwFilter', N'@PkName', N'@IndexedColumnName', N'@StagingTableDefinition', N'@StagingTableColumnListInsert', N'@StagingTableColumnListSelect', N'@ColumnEnumeratedList', N'@ColumnEnumeratedListWithDefinition'";

				var cdcColumnConfig = ConfigData.CdcColumnConfig.Where(c => c.SourceTable == table.SourceTable && c.ColumnInEdw);

				columnList = columnList.Replace("@SourceSchemaName", table.SourceSchema);
				columnList = columnList.Replace("@StagingSchemaName", "Staging");
				columnList = columnList.Replace("@StagingTableName", table.SourceTable);
				columnList = columnList.Replace("@EdwFilter", table.EdwFilter?.Replace("'", "''"));
				columnList = columnList.Replace("@PkName", cdcColumnConfig.Where(c => c.IsPrimaryKey).FirstOrDefault().SourceColumn);
				columnList = columnList.Replace("@IndexedColumnName", table.IndexedColumn);
				columnList = columnList.Replace("@StagingTableDefinition", string.Join(",", cdcColumnConfig.Select(c => string.Format(CultureInfo.InvariantCulture, "{0} {1}", c.SourceColumn, GetDataTypeDefinition(c.DataType, c.MaxLength, c.Precision, c.Scale)))));
				columnList = columnList.Replace("@StagingTableColumnListInsert", string.Join(",", cdcColumnConfig.Select(c => c.SourceColumn)));
				columnList = columnList.Replace("@StagingTableColumnListSelect", string.Join(",", cdcColumnConfig.Select(c => c.DataType.Equals("xml", StringComparison.OrdinalIgnoreCase) ? $"CAST({c.SourceColumn} AS NVARCHAR(MAX)) AS {c.SourceColumn}" : c.SourceColumn)));

				int counter = 1;
				columnList = columnList.Replace("@ColumnEnumeratedListWithDefinition", string.Join(",", cdcColumnConfig.Select(c => string.Format(CultureInfo.InvariantCulture, "c{0} {1}", counter++, c.DataType.Equals("xml", StringComparison.OrdinalIgnoreCase) ? "NVARCHAR(MAX)" : GetDataTypeDefinition(c.DataType, c.MaxLength, c.Precision, c.Scale)))));
				counter = 1;
				columnList = columnList.Replace("@ColumnEnumeratedList", string.Join(",", cdcColumnConfig.Select(c => string.Format(CultureInfo.InvariantCulture, "c{0}", counter++))));

				tableValueList.Add(columnList);
			}

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
INSERT INTO [{1}].[StagingTableConfiguration]
	(SourceSchemaName,
	StagingSchemaName,
	StagingTableName,
	EdwFilter,
	PkName,
	IndexedColumnName,
	StagingTableDefinition,
	StagingTableColumnListInsert,
	StagingTableColumnListSelect,
	ColumnEnumeratedList,
	ColumnEnumeratedListWithDefinition)
VALUES
({0})",
				string.Join("),\r\n(", tableValueList),
				BiConstants.BiAdminSchemaName);

			biConnection.ExecuteNonQuery(sqlText);
		}

		void PopulateTransformTableConfiguration()
		{
			var tableValueList = new List<string>();
			foreach (var edwTable in ConfigData.EdwTableConfig.OrderBy(t => t.DependencyOrder))
			{
				var columnList = "'@SourceTableName', '@ModelSchemaName', '@ModelTableName', @DependencyOrder, @TransformId, @IsLastTransform, N'@InitialLoadQuery', N'@IncrementalInsertQuery', N'@IncrementalDeleteQuery', @HasChildTables, @IsSelfReferenced, N'@WhereCondition', N'@OltpPartitionColumnExpression', N'@EdwPartitionColumnName', N'@CustomIndexScript'";

				var clientHookLoader = ObjectFactory.Get<IClientHookLoader>();
				var includeEdiTables = clientHookLoader?.ClientHook?.UniqueId == "EDI";
				var sourceTable = ConfigData.CdcTableConfig.FirstOrDefault(t => t.SourceSchema == edwTable.SourceSchema && t.SourceTable == edwTable.StagingTable);
				if (sourceTable != null)
				{
					if (!sourceTable.IsEdiClient || includeEdiTables)
					{
						columnList = columnList.Replace("@SourceTableName", edwTable.SourceSchema + "." + edwTable.StagingTable);
						columnList = columnList.Replace("@ModelSchemaName", edwTable.Schema);
						columnList = columnList.Replace("@ModelTableName", edwTable.Name);
						columnList = columnList.Replace("@DependencyOrder", edwTable.DependencyOrder.ToString(CultureInfo.InvariantCulture));
						columnList = columnList.Replace("@TransformId", edwTable.TransformId.ToString(CultureInfo.InvariantCulture));
						var isLastTransform = ConfigData.EdwTableConfig.IsLastTransform(edwTable);
						columnList = columnList.Replace("@IsLastTransform", isLastTransform ? "1" : "0");

						columnList = columnList.Replace("@InitialLoadQuery", FormatQuery(BiAutomationConfigLoader.Instance.ConfigData.GetInitialLoadQueryForEdwTable(edwTable)));
						columnList = columnList.Replace("@IncrementalInsertQuery", FormatQuery(BiAutomationConfigLoader.Instance.ConfigData.GetIncrementalInsertQueryForEdwTable(edwTable)));
						columnList = columnList.Replace("@IncrementalDeleteQuery", FormatQuery(BiAutomationConfigLoader.Instance.ConfigData.GetIncrementalDeleteQueryForEdwTable(edwTable)));

						columnList = columnList.Replace("@HasChildTables", ConfigData.EdwTableConfig.HasChildTables(edwTable) ? "1" : "0");
						columnList = columnList.Replace("@IsSelfReferenced", ConfigData.EdwTableConfig.IsSelfReferenced(edwTable) ? "1" : "0");
						columnList = columnList.Replace("@WhereCondition", FormatQuery(edwTable.WhereClause ?? ""));
						columnList = columnList.Replace("@OltpPartitionColumnExpression", FormatQuery(edwTable.GetEdwColumnConfigRows().Where(c => c.IsPartitionKey).Select(c => c.Expression).FirstOrDefault() ?? ""));
						columnList = columnList.Replace("@EdwPartitionColumnName", FormatQuery(edwTable.GetEdwColumnConfigRows().Where(c => c.IsPartitionKey).Select(c => c.Name).FirstOrDefault() ?? ""));

						columnList = columnList.Replace("@CustomIndexScript", FormatQuery(GetCustomIndexScript(edwTable)));

						tableValueList.Add(columnList);
					}
				}
				else
				{
					throw new OdysseyDataException(string.Format(CultureInfo.InvariantCulture, "Could not find staging table [{0}]. Make sure that CDC configuration is updated and try again.", edwTable.StagingTable));
				}
			}

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
INSERT INTO [{1}].[TransformTableConfiguration]
	(SourceTableName,
	ModelSchemaName,
	ModelTableName,
	DependencyOrder,
	TransformId,
	IsLastTransform,
	InitialLoadQuery,
	IncrementalInsertQuery,
	IncrementalDeleteQuery,
	HasChildTables,
	IsSelfReferenced,
	WhereCondition,
	OltpPartitionColumnExpression,
	EdwPartitionColumnName,
	CustomIndexScript)
VALUES
({0})",
				string.Join("),\r\n(", tableValueList),
				BiConstants.BiAdminSchemaName);

			biConnection.ExecuteNonQuery(sqlText);
		}

		internal string GetCustomIndexScript(EdwTableConfigRow edwTable)
		{
			var script = string.Join("", BiAutomationConfigLoader.Instance.ConfigData.GetCustomIndexScriptQueryForEdwTable(edwTable), BiAutomationConfigLoader.Instance.ConfigData.GetEditableCustomIndexScriptQueryForEdwTable(edwTable));
			return InsertTrackingCommentsInIndexScript(script, edwTable);
		}

		internal string InsertTrackingCommentsInIndexScript(string customIndexScript, EdwTableConfigRow edwTable)
		{
			var indexRegex = "(CREATE\\s+(?:UNIQUE\\s+)?(?:NONCLUSTERED|CLUSTERED)?\\s*INDEX)";
			var indexReplaceRegex = $"$0 /* Table:Ini_{edwTable.Schema}.{edwTable.Name} */";
			return Regex.Replace(customIndexScript, indexRegex, indexReplaceRegex);
		}

		protected void PopulateCustomTableConfiguration()
		{
			foreach (var customTable in ConfigData.EdwCustomTableConfig)
			{
				var columnList = "'@ModelSchemaName', '@ModelTableName', @RunBeforeTransform, @DependencyOrder, '@TableDependencyList', '@CustomTableDependencyList', '@ViewName', N'@InitialLoadQuery', N'@IncrementalLoadQuery'";

				columnList = columnList.Replace("@ModelSchemaName", customTable.Schema);
				columnList = columnList.Replace("@ModelTableName", customTable.Name);
				columnList = columnList.Replace("@RunBeforeTransform", customTable.RunBeforeTransform ? "1" : "0");
				columnList = columnList.Replace("@DependencyOrder", customTable.DependencyOrder.ToString(CultureInfo.InvariantCulture));
				columnList = columnList.Replace("@TableDependencyList", customTable.GetTableDependencyList());
				columnList = columnList.Replace("@CustomTableDependencyList", string.Join(",", customTable.ParseCustomTablesFromQueries().Select(t => "[" + t.Schema + "].[" + t.Name + "]")));
				columnList = columnList.Replace("@ViewName", customTable.ViewName);
				columnList = columnList.Replace("@InitialLoadQuery", FormatQuery(customTable.InitialLoadQuery));
				columnList = columnList.Replace("@IncrementalLoadQuery", FormatQuery(customTable.IncrementalLoadQuery));

				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
INSERT INTO [{1}].[CustomTableConfiguration]
	(ModelSchemaName,
	ModelTableName,
	RunBeforeTransform,
	DependencyOrder,
	TableDependencyList,
	CustomTableDependencyList,
	ViewName,
	InitialLoadQuery,
	IncrementalLoadQuery)
VALUES
({0})", columnList,
	BiConstants.BiAdminSchemaName);

				biConnection.ExecuteNonQuery(sqlText);
			}
		}

		void PopulateModelTableConfiguration()
		{
			var tableValueList = new List<string>();
			foreach (var aggTable in ConfigData.EdwDenormalizedTableConfig)
			{
				var columnList = "'@ModelSchemaName', '@ModelTableName', @DependencyOrder, '@SourceTables', N'@EdwPartitionColumnName'";

				columnList = columnList.Replace("@ModelSchemaName", aggTable.Schema);
				columnList = columnList.Replace("@ModelTableName", aggTable.Name);
				columnList = columnList.Replace("@DependencyOrder", aggTable.DependencyOrder.ToString(CultureInfo.InvariantCulture));
				columnList = columnList.Replace("@SourceTables", string.Join(",", BiAutomationConfigDataSet.ParseSourceTables(aggTable.Expression).Select(st => string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", st.Schema, st.Table)).OrderBy(st => st)));
				columnList = columnList.Replace("@EdwPartitionColumnName", FormatQuery(aggTable.GetEdwDenormalizedColumnConfigRows().Where(c => c.IsPartitionKey).Select(c => c.Name).FirstOrDefault() ?? ""));

				columnList = columnList.Replace("@CustomIndexScript", FormatQuery(string.Join("", BiAutomationConfigLoader.Instance.ConfigData.GetCustomIndexScriptQueryForDenormalizedTable(aggTable), BiAutomationConfigLoader.Instance.ConfigData.GetEditableCustomIndexScriptQueryForDenormalizedTable(aggTable))));

				tableValueList.Add(columnList);
			}

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
INSERT INTO [{1}].[ModelTableConfiguration]
	(ModelSchemaName,
	ModelTableName,
	DependencyOrder,
	SourceTables,
	EdwPartitionColumnName)
VALUES
({0})",
				string.Join("),\r\n(", tableValueList),
				BiConstants.BiAdminSchemaName);

			biConnection.ExecuteNonQuery(sqlText);
		}

		public static string FormatQuery(string query)
		{
			return !string.IsNullOrEmpty(query) ? "' + CAST(N'' AS nvarchar(max)) + N'" + query.Replace("'", "''").Replace("\r\n", "' + CHAR(13) + CHAR(10) + N'").Replace("\t", "' + CHAR(9) + N'") : query;
		}

		protected string GetDataTypeDefinitionFromStagingTable(string tableName, string columnName)
		{
			string sqlText = @"
				SELECT
					typ.name,
					col.max_length,
					col.precision,
					col.scale
				FROM
					sys.schemas sch
					INNER JOIN sys.tables tab ON tab.schema_id = sch.schema_id
					INNER JOIN sys.columns col ON col.object_id = tab.object_id
					INNER JOIN sys.types typ ON typ.user_type_id = col.user_type_id
				WHERE
					sch.name = @SchemaName
					AND tab.name = @TableName
					AND col.name = @ColumnName";

			using (var cmd = biConnection.Command(sqlText))
			{
				cmd.AddParameter("@SchemaName", SqlDbType.VarChar, 128, "Staging");
				cmd.AddParameter("@TableName", SqlDbType.VarChar, 128, tableName);
				cmd.AddParameter("@ColumnName", SqlDbType.VarChar, 128, columnName);

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						string dataType = reader[0].ToString();
						int maxLength = Convert.ToInt32(reader[1], CultureInfo.InvariantCulture);
						int precision = Convert.ToInt32(reader[2], CultureInfo.InvariantCulture);
						int scale = Convert.ToInt32(reader[3], CultureInfo.InvariantCulture);
						return GetDataTypeDefinition(dataType, maxLength, precision, scale);
					}
					else
					{
						throw new OdysseyDataException(string.Format(CultureInfo.InvariantCulture, "Could not find staging column [{0}.{1}]. Make sure that CDC configuration is updated and try again.", tableName, columnName));
					}
				}
			}
		}

		protected string GetDataTypeDefinition(string dataType, int maxLength, int precision, int scale)
		{
			string dataTypeDefinition = dataType;

			switch (dataType.ToUpperInvariant())
			{
				case "DECIMAL":
					dataTypeDefinition += "(" + precision.ToString(CultureInfo.InvariantCulture) + "," + scale.ToString(CultureInfo.InvariantCulture) + ")";
					break;
				case "CHAR":
				case "VARCHAR":
				case "NCHAR":
				case "NVARCHAR":
				case "VARBINARY":
					dataTypeDefinition += "(" + (maxLength == -1 ? "max" : maxLength.ToString()) + ")";
					break;
			}

			return dataTypeDefinition;
		}

		#endregion

		#region Populate Table State

		void PopulateTableState()
		{
			PopulateStagingTableState();
			PopulateTransformTableState();
			PopulateCustomTableState();
			PopulateModelTableState();
		}

		void PopulateStagingTableState()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"INSERT INTO [{0}].[{1}].[StagingTableState] (SourceTableName, CurrentMaxLsn, CurrentState)
SELECT SourceSchemaName + '.' + StagingTableName, 0x0, 'New'
FROM [{0}].[{1}].[StagingTableConfiguration] tc WHERE NOT EXISTS
	(SELECT NULL FROM [{0}].[{1}].[StagingTableState] ts  WHERE ts.SourceTableName = tc.SourceSchemaName + '.' + tc.StagingTableName)

DELETE ts FROM [{0}].[{1}].[StagingTableState] ts
WHERE NOT EXISTS
	(SELECT 1 FROM [{0}].[{1}].[StagingTableConfiguration] tc
	 WHERE ts.SourceTableName = tc.SourceSchemaName + '.' + tc.StagingTableName)",
			Db.EdwDatabaseName,
			BiConstants.BiAdminSchemaName);

			biConnection.ExecuteNonQuery(sqlText);
		}

		void PopulateTransformTableState()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"INSERT INTO [{0}].[TransformTableState] (ModelSchemaName, ModelTableName, TransformId, CurrentState)
SELECT ModelSchemaName, ModelTableName, TransformId, 'New'
FROM [{0}].[TransformTableConfiguration] tc WHERE NOT EXISTS
	(SELECT NULL FROM [{0}].[TransformTableState] ts  WHERE ts.ModelSchemaName = tc.ModelSchemaName AND  ts.ModelTableName = tc.ModelTableName AND ts.TransformId = tc.TransformId)

DELETE ts FROM [{0}].[TransformTableState] ts
WHERE NOT EXISTS
	(SELECT 1 FROM [{0}].[TransformTableConfiguration] tc
	 WHERE tc.ModelSchemaName = ts.ModelSchemaName
		AND tc.ModelTableName = ts.ModelTableName
		AND tc.TransformId = ts.TransformId)",
			BiConstants.BiAdminSchemaName);

			biConnection.ExecuteNonQuery(sqlText);
		}

		void PopulateCustomTableState()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"INSERT INTO [{0}].[CustomTableState] (ModelSchemaName, ModelTableName, CurrentState)
SELECT ModelSchemaName, ModelTableName, 'New'
FROM [{0}].[CustomTableConfiguration] tc WHERE NOT EXISTS
	(SELECT NULL FROM [{0}].[CustomTableState] ts  WHERE ts.ModelSchemaName = tc.ModelSchemaName AND  ts.ModelTableName = tc.ModelTableName)

DELETE ts FROM [{0}].[CustomTableState] ts
WHERE NOT EXISTS
	(SELECT 1 FROM [{0}].[CustomTableConfiguration] tc
	 WHERE tc.ModelSchemaName = ts.ModelSchemaName
		AND tc.ModelTableName = ts.ModelTableName)",
			BiConstants.BiAdminSchemaName);

			biConnection.ExecuteNonQuery(sqlText);
		}

		void PopulateModelTableState()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"DELETE mts
FROM [{0}].[ModelTableState] mts
WHERE NOT EXISTS
	(SELECT 1 
	FROM [{0}].[ModelTableConfiguration] mtc
	WHERE mts.ModelSchemaName = mtc.ModelSchemaName
		AND mts.ModelTableName = mtc.ModelTableName)

INSERT INTO [{0}].[ModelTableState] (ModelSchemaName, ModelTableName, CurrentState, InitialLoadRequired)
SELECT t.ModelSchemaName, t.ModelTableName, 'New', 1
FROM [{0}].[ModelTableConfiguration] t
LEFT JOIN [{0}].[ModelTableState] mt on 
		mt.ModelSchemaName = t.ModelSchemaName AND 
		mt.ModelTableName = t.ModelTableName
WHERE mt.ModelSchemaName IS NULL",
			BiConstants.BiAdminSchemaName);

			biConnection.ExecuteNonQuery(sqlText);
		}

		#endregion

		#region Determine Changes

		void DetermineViewChangesForDenormalizedTables()
		{
			var denormTableList = new List<string>();
			string sqlText;
			foreach (var denormTable in ConfigData.EdwDenormalizedTableConfig)
			{
				sqlText = string.Format(CultureInfo.InvariantCulture, @"
DECLARE @actual_text NVARCHAR(MAX) = N'', @expected_text NVARCHAR(MAX) = N'{0}'

select @actual_text = sm.definition
from sys.objects o
	inner join sys.sql_modules sm on sm.object_id = o.object_id
where 
	o.name = 'vw_{1}'
	and schema_name(o.schema_id) = '{2}'

SET @actual_text = LTRIM(RTRIM(REPLACE(REPLACE(@actual_text, CHAR(10) ,N''), CHAR(13), N'')))
SET @expected_text = LTRIM(RTRIM(REPLACE(REPLACE(@expected_text, CHAR(10) ,N''), CHAR(13), N'')))

IF @actual_text = @expected_text
	SELECT 0
ELSE
	SELECT 1",
				ConfigData.GetCreateDenormalizedTableViewQuery(denormTable).Replace("'", "''"),
				denormTable.Name,
				denormTable.Schema);

				if (Convert.ToBoolean(biConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture))
				{
					denormTableList.Add(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", denormTable.Schema, denormTable.Name));
				}
			}

			if (denormTableList.Any())
			{
				sqlText = $@"
UPDATE [{BiConstants.BiAdminSchemaName}].[ModelTableState]
SET InitialLoadRequired = @initialLoadRequired, CurrentState = @state
WHERE ModelSchemaName + '.' + ModelTableName IN (SELECT value FROM STRING_SPLIT(@tableList, ',') WHERE RTRIM(value) <> '')";

				using (var cmd = biConnection.Command(sqlText))
				{
					cmd.AddParameter("@state", SqlDbType.VarChar, "New");
					cmd.AddParameter("@initialLoadRequired", SqlDbType.Bit, true);
					cmd.AddParameter("@tableList", SqlDbType.NVarChar, string.Join(",", denormTableList));

					cmd.ExecuteNonQuery();
				}
			}
		}

		void DetermineViewChangesForCustomTables()
		{
			string sqlText;
			foreach (var customTable in ConfigData.EdwCustomTableConfig)
			{
				if (!string.IsNullOrEmpty(customTable.ViewName))
				{
					sqlText = string.Format(CultureInfo.InvariantCulture, @"
DECLARE @actual_text NVARCHAR(MAX) = N'', @expected_text NVARCHAR(MAX) = N'{0}'

select @actual_text = sm.definition
from sys.objects o
	inner join sys.sql_modules sm on sm.object_id = o.object_id
where 
	o.name = '{1}'
	and schema_name(o.schema_id) = '{2}'

SET @actual_text = LTRIM(RTRIM(REPLACE(REPLACE(@actual_text, CHAR(10) ,N''), CHAR(13), N'')))
SET @expected_text = LTRIM(RTRIM(REPLACE(REPLACE(@expected_text, CHAR(10) ,N''), CHAR(13), N'')))

IF @actual_text = @expected_text
	SELECT 0
ELSE
	SELECT 1",
					customTable.CreateViewQuery.Replace("'", "''"),
					customTable.ViewName,
					customTable.Schema);

					if (Convert.ToBoolean(biConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture))
					{
						var tableName = customTable.Schema + "." + customTable.Name;
						if (!customTablesForInitialLoad.Contains(tableName))
						{
							customTablesForInitialLoad.Add(tableName);
						}
					}
				}
			}
		}

		void InitiateInitialLoadIfRequired()
		{
			var stringBuilder = new StringBuilder();

			if (tablesForInitialLoad.Any())
			{
				stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, @"
UPDATE [{1}].[{2}].[StagingTableState]
SET
	CurrentMaxLsn = @lsn,
	CurrentState = @state,
	InitialLoadRequired = @initialLoadRequired
WHERE SourceTableName IN ('{0}')",
					string.Join("', '", tablesForInitialLoad),
					Db.EdwDatabaseName,
					BiConstants.BiAdminSchemaName));
			}

			if (customTablesForInitialLoad.Any())
			{
				stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, @"
UPDATE [{1}].[CustomTableState]
SET
	CurrentState = @state,
	InitialLoadRequired = @initialLoadRequired
WHERE ModelSchemaName + '.' + ModelTableName IN ('{0}')",
					string.Join("', '", customTablesForInitialLoad),
					BiConstants.BiAdminSchemaName));
			}

			if (stringBuilder.Length > 0)
			{
				using (var cmd = biConnection.Command(stringBuilder.ToString()))
				{
					cmd.AddParameter("@lsn", SqlDbType.Binary, new byte[] { 0 });
					cmd.AddParameter("@state", SqlDbType.VarChar, "New");
					cmd.AddParameter("@initialLoadRequired", SqlDbType.Bit, true);

					cmd.ExecuteNonQuery();
				}
			}
		}

		#endregion

		#region SSAS Tables

		void PopulateSsasInfo()
		{
			var populator = new SsasInfoPopulator(biConnection);
			populator.PopulateSsasInfo();
		}

		#endregion

		BiConfigurationData ConfigData
		{
			get
			{
				return configData ?? (configData = BiAutomationConfigLoader.Instance.ConfigData);
			}
		}
		BiConfigurationData configData;
	}
}
