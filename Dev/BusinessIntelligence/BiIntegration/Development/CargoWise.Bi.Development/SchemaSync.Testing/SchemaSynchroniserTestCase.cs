using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Bi.Common;
using CargoWise.Bi.Development.Common;
using CargoWise.Bi.Development.SchemaSync.DataSets;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.IO;
using Enterprise.Client.EDI;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static CargoWise.Bi.Configuration.DataSets.BiAutomationConfigDataSet;

namespace CargoWise.Bi.Development.SchemaSync.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	partial class SchemaSynchroniserTestCase : TestCase
	{
		/// <summary>
		/// Setup TFS Control Mock
		/// BI Configuration XML files are in:
		/// ..\Dev\Database\BusinessIntelligence\ConfigLoader\ConfigLoader\Config
		/// T-SQl query used to load main DB schema is:
		/// ..\Dev\BusinessIntelligence\CargoWiseBusinessIntelligence\Integration\CargoWise.Bi.Automation\SQL\MainDbSchemaQuery.sql
		/// </summary>
		protected override void SetUp()
		{
			base.SetUp();

			BiFiles.ResetPaths();
			BiAutomationConfigLoaderForDevelopment.Instance.ResetConfiguration();

			BiDatabase.ServerName = Db.ServerName;
		}

		CdcColumnConfigDataTable cdcColumnConfig;
		public CdcColumnConfigDataTable CdcColumnConfig
		{
			get
			{
				if (cdcColumnConfig == null)
				{
					cdcColumnConfig = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig;
				}
				return cdcColumnConfig;
			}
			set
			{
				cdcColumnConfig = value;
			}
		}

		CdcTableConfigDataTable cdcTableConfig;
		public CdcTableConfigDataTable CdcTableConfig
		{
			get
			{
				if (cdcTableConfig == null)
				{
					cdcTableConfig = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig;
				}
				return cdcTableConfig;
			}
		}

		EdwTableConfigDataTable edwTableConfig;
		public EdwTableConfigDataTable EdwTableConfig
		{
			get
			{
				if (edwTableConfig == null)
				{
					edwTableConfig = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig;
				}
				return edwTableConfig;
			}
			set
			{
				edwTableConfig = value;
			}
		}

		EdwColumnConfigDataTable edwColumnConfig;
		public EdwColumnConfigDataTable EdwColumnConfig
		{
			get
			{
				if (edwColumnConfig == null)
				{
					edwColumnConfig = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig;
				}
				return edwColumnConfig;
			}
			set
			{
				edwColumnConfig = value;
			}
		}

		/// <summary>
		/// Assert both the previous version of BI Configuration XML file and the current main DB schema
		/// are loaded correctly in BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.
		/// Assert all table additions have Action="Added".
		/// Assert all table deletions have Action="Deleted".
		/// Assert all tables that have column changes have Action="ChangedColumnOnly".
		/// Assert all column additions have Action="Added".
		/// Assert all column deleted have Action="Deleted".
		/// </summary>
		[UseSnapshotProtection]
		public void TestSync()
		{
			using (var temp = new TempDirectory())
			using (DatabaseScriptTestCase.InitializePaths(temp.DirectoryName))
			{
				var droppedColumn = (from column in CdcColumnConfig
									 where column.DataType == "bit"
									 select column.SourceColumn).First();
				var tableWithDroppedColumn = (from table in CdcTableConfig
											  join column in CdcColumnConfig
												  on table.SourceTable equals column.SourceTable
											  where column.SourceColumn == droppedColumn
											  select table.SourceTable).First();
				var changedColumn = (from column in CdcColumnConfig
									 join table in CdcTableConfig
										on column.SourceTable equals table.SourceTable
									 where  column.SourceColumn != droppedColumn && column.DataType == "bit"
									 select column.SourceColumn).First();
				var tableWithChangedColumn = (from table in CdcTableConfig
											  join column in CdcColumnConfig
												  on table.SourceTable equals column.SourceTable
											  where column.SourceColumn == changedColumn
											  select table.SourceTable).First();

				try
				{
					SchemaSynchroniser.BeforeBiConfigurationLoad = (connection, dbName) =>
					{
						AddTable(connection, dbName, "NewTable");
						AddTable(connection, dbName, "NewSchema", "IncludedSchemaTable");
						AddColumn(connection, dbName, "NewTable", "NewColumn");
						ChangeColumn(connection, dbName, tableWithChangedColumn, changedColumn);
						DropColumn(connection, dbName, tableWithDroppedColumn, droppedColumn);
					};

					SchemaSynchroniser.AfterBiConfigurationLoad = (connection, dbName) =>
					{
						AssertTableAdded(connection, dbName, "NewTable");
						AssertTableAdded(connection, dbName, "IncludedSchemaTable");
						AssertColumnAdded(connection, dbName, "NewTable", "NewColumn");
						AssertColumnChanged(connection, dbName, tableWithChangedColumn, changedColumn);
						AssertColumnDropped(connection, dbName, tableWithDroppedColumn, droppedColumn);
					};

					SchemaSynchroniser.LoadBiConfiguration();
					SchemaSynchroniser.SyncSchemaConfiguration();
				}
				finally
				{
					SchemaSynchroniser.BeforeBiConfigurationLoad = null;
					SchemaSynchroniser.AfterBiConfigurationLoad = null;
				}

				var tableAdded = from table in CdcTableConfig
								 where table.Action == "Added" && table.SourceTable == "NewTable"
								 select true;
				Assert("Added table does not reflect in data set.", tableAdded.Any());

				var columnAdded = from column in CdcColumnConfig
								  where column.Action == "Added" && column.SourceColumn == "NewColumn"
								  select true;
				Assert("Added column does not reflect in data set.", columnAdded.Any());

				var tableWithChangedColumnOnly = from table in CdcTableConfig
												 where table.Action == "ChangedColumnOnly" && table.SourceTable == tableWithChangedColumn
												 select true;
				Assert("Table with changed column only does not reflect in data set.", tableWithChangedColumnOnly.Any());

				var columnChanged = from column in CdcColumnConfig
									where column.Action == "Changed" && column.SourceColumn == changedColumn
									select true;
				Assert("Changed column does not reflect in data set.", columnChanged.Any());

				var columnDeleted = from column in CdcColumnConfig
									where column.Action == "Deleted" && column.SourceColumn == droppedColumn
									select column;
				Assert("Deleted column does not reflect in data set.", columnDeleted.Any());
			}
		}

		public void TestOnlyAllowedSchemasInAuditDb()
		{
			var auditTables = from table in CdcTableConfig
							  where SchemaIsBanned(table.SourceSchema) && table.TableInAudit
							  select $"{table.SourceSchema}.{table.SourceTable}";

			var auditTablesErrorMessage = string.Format("The following tables are from a forbidden schema and should thus not be in the Audit DB.\n{0}", string.Join("\n", auditTables.ToList()));
			AssertEquals(auditTablesErrorMessage, 0, auditTables.Count());
		}

		bool SchemaIsBanned(string sourceSchema)
		{
			var bannedSchemas = new List<string>() { "sys", "cdc", "guest", "INFORMATION_SCHEMA" };
			return bannedSchemas.Contains(sourceSchema) || sourceSchema.StartsWith("db_");
		}

		public void TestCdcDisabledTablesDontHaveCdcFlaggedColumns()
		{
			var nonCdcTables = from table in CdcTableConfig
							  where !table.TableInEdw && !table.TableInAudit
							  select table;

			CombineAssertions("The following columns are CDC enabled, but their source table is CDC disabled", () =>
			{
				foreach (var table in nonCdcTables)
				{
					var cdcEnabledColumns = table.GetCdcColumnConfigRows().Where(col => col.CdcEnabled);
					var message = string.Format($" > [{table.SourceSchema}].[{table.SourceTable}]\r\n" + string.Join("\r\n    ", cdcEnabledColumns.Select(c => c.SourceColumn).ToList().ToString()));
					AssertEquals(message, 0, cdcEnabledColumns.Count());
				}
			});
		}

		public void TestAutoversionColumnsAreDisabledInTables()
		{
			var allTables = from table in CdcTableConfig
							select table;
			CombineAssertions("The following tables have cdc enabled autoversion columns", () =>
			{
				foreach (var table in allTables)
				{
					var autoversionColumnsCdcEnabled = table.GetCdcColumnConfigRows().Where(col => col.SourceColumn.EndsWith("_AutoVersion") && col.CdcEnabled);
					var message = string.Format($" > [{table.SourceSchema}].[{table.SourceTable}]\r\n" + string.Join("\r\n    ", autoversionColumnsCdcEnabled.Select(c => c.SourceColumn).ToList().ToString()));
					AssertEquals(message, 0, autoversionColumnsCdcEnabled.Count());
				}
			});
		}

		public void TestRowVersionColumnsAreDisabledInTables()
		{
			var allTables = from table in CdcTableConfig
							select table;
			CombineAssertions("The following tables have cdc enabled RowVersion columns", () =>
			{
				foreach (var table in allTables)
				{
					var rowVersionColumnsCdcEnabled = table.GetCdcColumnConfigRows().Where(col => col.SourceColumn.EndsWith("_RowVersion") && col.CdcEnabled);
					var message = string.Format($" > [{table.SourceSchema}].[{table.SourceTable}]\r\n" + string.Join("\r\n    ", rowVersionColumnsCdcEnabled.Select(c => c.SourceColumn).ToList().ToString()));
					AssertEquals(message, 0, rowVersionColumnsCdcEnabled.Count());
				}
			});
		}

		public void TestClusterKeyColumnsAreAvailableInSchemaForCdc()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var dataTable = DataUtils.GetDataTableFromQuery(connection, BiFiles.MainDbSchemaQueryContent);
				var clusterKeyColumns = dataTable.Select("SourceColumn LIKE '%_ClusterKey'");

				Assert("MainDbSchemaQuery should support ClusterKey columns.", clusterKeyColumns.Any());
			}
		}

		public void TestBiConfigurationTablesAreUpdated()
		{
			var allTables = CdcTableConfig;
			var invalidTables = allTables.Where(t => t.Action != "Unchanged");

			var changedTableErrorMessage = $"The following tables have recorded changes and BI config files need to be updated. Regen on an updated workspace or run 'BiRegenSynchroniser.exe -sync' in a console.\n{string.Join("\n", invalidTables)}";
			Assert(changedTableErrorMessage, !invalidTables.Any());
		}

		public void TestBiConfigurationColumnsAreUpdated()
		{
			var changedColumns =
				from column in CdcColumnConfig
				join table in CdcTableConfig
				on column.SourceTable equals table.SourceTable
				where column.Action != "Unchanged"
				select string.Format("{0}.{1}", table.SourceTable, column.SourceColumn);

			var changedColumnsErrorMessage = string.Format("The following columns have recorded changes and BI config files need to be updated. Regen on an updated workspace or run 'BiRegenSynchroniser.exe -sync' in a console.\n{0}", string.Join("\n", changedColumns.ToList()));
			AssertEquals(changedColumnsErrorMessage, 0, changedColumns.Count());
		}

		[UseSnapshotProtection]
		public void TestAssertDeletedColumnSetsEdwColumnExpressionToNull()
		{
			using (var temp = new TempDirectory())
			using (DatabaseScriptTestCase.InitializePaths(temp.DirectoryName))
			{
				var droppedColumn = (from column in CdcColumnConfig
									 where column.DataType == "bit" && !column.GetCdcTable().TableInEdw
									 select column).First();
				var cdcTableElement = droppedColumn.GetCdcTable();
				var cdcSchemaName = cdcTableElement.SourceSchema;
				var cdcTableName = cdcTableElement.SourceTable;

				var edwTable = EdwTableConfig.AddEdwTableConfigRow("Test", "TestEdwTable", "", cdcSchemaName, cdcTableName, "", 1, false, 1, "");
				EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "bit", 0, 0, 0, "TestEdwColumn", droppedColumn.SourceColumn, false, false, "", null, null, null, null, null, false);

				var edwColumn = EdwColumnConfig.Where(c => c.Name == "TestEdwColumn").First();
				AssertEquals(droppedColumn.SourceColumn, edwColumn.Expression);

				using (var connection = Db.NewAdminConnection(BiDatabase.ServerName, Db.SqlMasterDb))
				{
					DropColumn(connection, BiDatabase.TemplateDatabaseName, cdcTableName, droppedColumn.SourceColumn);
					SchemaSynchroniser.LoadDatabaseSchema(connection);
				}

				SchemaSynchroniser.MarkRemovedColumnAsDeleted();
				AssertEquals("NULL", edwColumn.Expression);
			}
		}

		public void TestNoEdwColumnExpressionIsNull()
		{
			var edwColumnsWithNullExpression = EdwColumnConfig.Where(c => c.Expression == "NULL");
			Assert(
				string.Format("The following EDW columns have NULL expressions.\r\n{0}\r\nPlease create a task in your current WI and assign it to the BI team for action.",
					string.Join("\r\n", edwColumnsWithNullExpression.Select(c => string.Format("[{0}].[{1}]", c.GetEdwTable().Name, c.Name)))),
				!edwColumnsWithNullExpression.Any());
		}

		public void TestBaseTableHaveKeyAndIdIndexForOptimization()
		{
			var edwColumnsShouldBeEnableIndex = EdwColumnConfig.Where(c => !c.EnableIndex
			&& (c.Name == c.TableName + "ID" || c.Name == c.TableName + "Key"));

			Assert(
				string.Format("The following EDW columns should have indexes enabled for table optimization.\r\n{0}",
					string.Join("\r\n", edwColumnsShouldBeEnableIndex.Select(c => string.Format("[{0}].[{1}].[{2}]", c.GetEdwTable().Schema, c.GetEdwTable().Name, c.Name)))),
				!edwColumnsShouldBeEnableIndex.Any());
		}

		public void TestBiConfigValidation()
		{
			try
			{
				BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.Validate();
				Assert(true);
			}
			catch (Exception ex)
			{
				Fail(string.Format("BI configuration validation failed.\r\n{0}", ex.Message));
			}
		}

		public void TestBaseTableDependencyOrder()
		{
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SortModelDependencyOrder();

			var baseTableDataView = EdwTableConfig.DefaultView;
			baseTableDataView.Sort = "DependencyOrder";
			var baseTables = (EdwTableConfigDataTable)baseTableDataView.Table;

			CombineAssertions(() =>
			{
				foreach (var baseTable in baseTables)
				{
					var columnsWithDependencies = baseTable.GetEdwColumnConfigRows().Where(c => !string.IsNullOrEmpty(c.ParentTable) && c.ParentTable != baseTable.Name);

					foreach (var column in columnsWithDependencies)
					{
						var tableDependency = (from table in EdwTableConfig
											   where table.Name == column.ParentTable && table.DependencyOrder > baseTable.DependencyOrder && GetTableDependencies(table).Contains(baseTable.Name)
											   select table.Name).FirstOrDefault();
						if (!string.IsNullOrEmpty(tableDependency))
						{
							Fail(string.Format("Column {0}.{1} has dependency on table {2} that has a higher Dependency Order ID", baseTable.Name, column.Name, tableDependency));
						}
					}
				}
			});

			Assert(true);
		}

		public void TestBiAdminTablesExistForAuditDb()
		{
			var auditBiAdminTables = DataUtils.GetListOfValuesFromQuery(
				Db.Connection,
				string.Format(CultureInfo.InvariantCulture,
					"select t.name from [{0}].sys.tables t inner join [{0}].sys.schemas s ON s.schema_id = t.schema_id where s.name = '{1}'",
					Db.AuditDatabaseName,
					BiConstants.BiAdminSchemaName
				)).ToArray();

			CombineAssertions(
				() => Array.ForEach(
					new[] { "MasterState", "TableState", "TableConfiguration", "SubscriberControl", "CdcHistorySummary", "DataLossLog", "LsnTimeMapping", "SchemaMappingSummary", "CdcHistorySummaryStaging" },
					(expectedTable) => AssertCollectionContains(
						string.Format(CultureInfo.InvariantCulture, "Expected BI admin table [{0}].[{1}] does not exist.", BiConstants.BiAdminSchemaName, expectedTable),
						expectedTable, auditBiAdminTables
					)
				)
			);
		}

		public void TestAuditTablesHaveOneColumnstoreAndOneRowstoreIndex()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"select i.name
from [{0}].sys.indexes i
inner join [{0}].sys.tables t on t.object_id = i.object_id
inner join [{0}].sys.schemas s on s.schema_id = t.schema_id
where s.name = @SchemaName and t.name = @TableName",
				Db.AuditDatabaseName);

			CombineAssertions(() =>
			{
				foreach (var cdcTable in CdcTableConfig.Where(t => t.TableInAudit))
				{
					var expectedIndexList = new List<string> { string.Format(CultureInfo.InvariantCulture, "cci_{0}_{1}", cdcTable.SourceSchema, cdcTable.SourceTable) };
					expectedIndexList.Add(string.Format(CultureInfo.InvariantCulture, "IX_{0}_StartLsn", cdcTable.SourceTable));

					using (var cmd = Db.Connection.Command(sqlText))
					{
						cmd.AddParameter("@SchemaName", SqlDbType.NVarChar, 128, cdcTable.SourceSchema);
						cmd.AddParameter("@TableName", SqlDbType.NVarChar, 128, cdcTable.SourceTable);

						var actualIndexList = DataUtils.GetListOfValuesFromCommand(cmd);

						AssertContainsExactElementsInAnyOrder($"[{cdcTable.SourceSchema}].[{cdcTable.SourceTable}]", expectedIndexList, actualIndexList);
					}
				}
			});
		}

		public void TestStartLsnIndexesExistForAuditDb()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"select DISTINCT t.name AS TableName, i.name AS IndexName, p.data_compression_desc AS DataCompression
from [{0}].sys.indexes i
inner join [{0}].sys.tables t
	on i.object_id = t.object_id
inner join [{0}].sys.partitions p
	on t.object_id = p.object_id and p.index_id = i.index_id
where i.name like '%_StartLsn' and i.type_desc = 'NONCLUSTERED'",
				Db.AuditDatabaseName);

			var auditStartLsnIndexes = DataUtils.GetDataTableFromQuery(Db.Connection, sqlText).AsEnumerable();

			CombineAssertions(() =>
			{
				foreach (var cdcTable in CdcTableConfig.Where(t => t.TableInAudit))
				{
					var indexObj = auditStartLsnIndexes.FirstOrDefault(i => i["TableName"].ToString() == cdcTable.SourceTable);

					string actualIndexName = null;
					string actualDataCompression = null;

					if (indexObj != null)
					{
						actualIndexName = indexObj["IndexName"].ToString();
						actualDataCompression = indexObj["DataCompression"].ToString();
					}

					var expectedIndexName = string.Format(CultureInfo.InvariantCulture, "IX_{0}_StartLsn", cdcTable.SourceTable);

					AssertEquals("Index for " + cdcTable.SourceTable, expectedIndexName, actualIndexName);
					if (actualIndexName == expectedIndexName)
					{
						AssertEquals("Data Compression for " + actualIndexName, "PAGE", actualDataCompression);
					}
				}
			});
		}

		public void TestBiAdminTablesExistForEdwDb()
		{
			var edwBiAdminTables = DataUtils.GetListOfValuesFromQuery(
				Db.Connection,
				string.Format(CultureInfo.InvariantCulture,
					"select t.name from [{0}].sys.tables t inner join [{0}].sys.schemas s ON s.schema_id = t.schema_id where s.name = '{1}'",
					Db.EdwDatabaseName,
					BiConstants.BiAdminSchemaName
				)).ToArray();
			var expectedAdminTables = new[] {
				"MasterState",
				"StagingTableState", "TransformTableState", "CustomTableState",
				"StagingTableConfiguration", "TransformTableConfiguration", "CustomTableConfiguration",
				"EdwModelTableToSsasTableMapping", "ModelTableConfiguration", "ModelTableState",
				"SsasCube", "SsasTable", "SsasPartition",
				"TransformedRow", "SsasPartitionUnprocessedDate", "ReportParameterConfiguration", "ReportConfiguration" };
			AssertContainsExactElementsInAnyOrder(string.Format("Unexpected table in [{0}] schema for EDW database.", BiConstants.BiAdminSchemaName), expectedAdminTables, edwBiAdminTables);
		}

		public void TestNoCalculatedColumnsInCdcConfiguration()
		{
			var sqlText = "SELECT t.name AS TableName, c.name AS ColumnName FROM sys.columns c INNER JOIN sys.tables t ON t.object_id = c.object_id WHERE c.is_computed = 1";

			CombineAssertions("The following calculated columns should be excluded from configuration.", () =>
			{
				Assert(true);

				using (var connection = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
				using (var cmd = connection.Command(sqlText))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var tableName = reader["TableName"].ToString();
						var columnName = reader["ColumnName"].ToString();

						var cdcTable = CdcTableConfig.FirstOrDefault(t => t.SourceTable.Equals(tableName, StringComparison.OrdinalIgnoreCase));
						if (cdcTable != null)
						{
							var cdcColumn = cdcTable.GetCdcColumnConfigRows().FirstOrDefault(c => c.SourceColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase));
							Assert($"[{tableName}].[{columnName}]", cdcColumn == null);
						}
					}
				}
			});
		}

		public void TestIsEdiClient()
		{
			var allTables = CdcTableConfig.OrderBy(x => x.SourceTable);
			var ediScripts = EDIClientDbSchemaUpgradeInfo.TableCreationScripts.Value;
			var ediTableScripts = ediScripts.Where(x => Regex.IsMatch(x.CreateScript, "Create Table", RegexOptions.IgnoreCase));
			var ediTableScriptNames = ediTableScripts.Select(x => x.ObjectName);
			CombineAssertions("EDI tables not found or not set to IsEdiClient", () =>
			{
				foreach (var scriptName in ediTableScriptNames)
				{
					var foundTable = allTables.FirstOrDefault(x => x.SourceTable.Equals(scriptName, StringComparison.OrdinalIgnoreCase));
					AssertNotNull($"EDI table '{scriptName}' not found in Configuration: ", foundTable);
					if (foundTable != null)
					{
						Assert($"EDI table '{scriptName}' not set as IsEdiClient: ", foundTable.IsEdiClient);
					}
				}
			});
		}

		public void TestIsNotEdiClient()
		{
			var ediScripts = EDIClientDbSchemaUpgradeInfo.TableCreationScripts.Value;
			var ediTableScriptNames = ediScripts.Where(x => Regex.IsMatch(x.CreateScript, "Create Table", RegexOptions.IgnoreCase)).Select(x => x.ObjectName);

			var allTables = CdcTableConfig;
			var invalidTables = allTables.Where(t => t.IsEdiClient && !ediTableScriptNames.Contains(t.SourceTable));
			var invalidTablesString = string.Join("\r\n > ", invalidTables);

			Assert($"The following tables have 'IsEdiClient' set to true, but do not exist in the create table scripts:\r\n{invalidTablesString}", !invalidTables.Any());
		}

		public void TestConfigLoaderProjectHasBiTags()
		{
			var fileContent = File.ReadAllText(BiConfigurationFileHandler.ConfigLoaderProjectFilePath);

			var biConfigTag = @"<!--BI config files-->";
			var regex = new Regex(biConfigTag);

			var matches = regex.Matches(fileContent);

			AssertEquals($"ConfigLoader.csproj should contain BI config files tags.", 2, matches.Count);
		}

		public void TestCustomTablesContainTPTTComments()
		{
			var sqlText = $"SELECT InitialLoadQuery, IncrementalLoadQuery, ModelSchemaName, ModelTableName FROM {Db.EdwDatabaseName}.[biAdmin].[CustomTableConfiguration]";

			CombineAssertions("Custom tables should contain TPTT comments in correct format.", () =>
			{
				using (var connection = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
				using (var cmd = connection.Command(sqlText))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var initialLoadQuery = reader["InitialLoadQuery"].ToString();
						var incrementalLoadQuery = reader["IncrementalLoadQuery"].ToString();
						var modelSchemaName = reader["ModelSchemaName"].ToString();
						var modelTableName = reader["ModelTableName"].ToString();

						if (!string.IsNullOrWhiteSpace(initialLoadQuery))
						{
							var initialLoadComments = ExtractCommentsFromScript(initialLoadQuery);
							var hasValidInitialLoadComment = VerifyCommentsInScript(initialLoadComments, "Ini", modelSchemaName, modelTableName);
							Assert($"No valid TPTT comment found in InitialLoadQuery for {modelSchemaName}.{modelTableName}.", hasValidInitialLoadComment);
						}
						if (!string.IsNullOrWhiteSpace(incrementalLoadQuery))
						{
							var incrementalLoadComments = ExtractCommentsFromScript(incrementalLoadQuery);
							var hasValidIncrementalLoadComment = VerifyCommentsInScript(incrementalLoadComments, "Inc", modelSchemaName, modelTableName);
							Assert($"No valid TPTT comment found in IncrementalLoadQuery for {modelSchemaName}.{modelTableName}.", hasValidIncrementalLoadComment);
						}
					}
				}
			});
		}

		bool VerifyCommentsInScript(List<string> comments, string commentType, string schemaName, string tableName)
		{
			var expectedPrefix = $"--Table:{commentType}_{schemaName}_{tableName}";

			foreach (var comment in comments)
			{
				if (comment.StartsWith("--Table:"))
				{
					if (comment.StartsWith(expectedPrefix))
					{
						return true;
					}
				}
			}
			return false;
		}

		List<string> ExtractCommentsFromScript(string script)
		{
			var comments = new List<string>();
			var commentPattern = @"--.*?$";
			var matches = Regex.Matches(script, commentPattern, RegexOptions.Multiline);

			foreach (Match match in matches)
			{
				comments.Add(match.Value.Trim());
			}

			return comments;
		}

		#region Implementation

		IEnumerable<string> GetTableDependencies(EdwTableConfigRow edwTable)
		{
			return edwTable.GetEdwColumnConfigRows().Where(c => !string.IsNullOrEmpty(c.ParentTable)).Select(c => c.ParentTable);
		}

		void AddTable(DbConnection connection, string dbName, string schemaName, string tableName)
		{
			var sqlText = $"IF EXISTS (SELECT * FROM [{dbName}].sys.schemas WHERE name = '{schemaName}') SELECT 1 ELSE SELECT 0";
			var schemaExists = Convert.ToBoolean(connection.Command(sqlText).ExecuteScalar(), CultureInfo.InvariantCulture);
			if (!schemaExists)
			{
				using (var adminConnection = Db.NewAdminConnection(dbName))
				{
					sqlText = $"CREATE SCHEMA [{schemaName}]";
					adminConnection.Command(sqlText).ExecuteNonQuery();
				}
			}
			sqlText = $"CREATE TABLE [{dbName}].[{schemaName}].[{tableName}] (PK uniqueidentifier PRIMARY KEY)";
			connection.Command(sqlText).ExecuteNonQuery();
		}

		void AddTable(DbConnection connection, string dbName, string tableName)
		{
			var sqlText = $"CREATE TABLE [{dbName}]..[{tableName}] (PK uniqueidentifier PRIMARY KEY)";
			connection.ExecuteNonQuery(sqlText);
		}

		void AddColumn(DbConnection connection, string dbName, string tableName, string columnName)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"ALTER TABLE [{0}]..[{1}] ADD [{2}] int",
				dbName, tableName, columnName);
			connection.ExecuteNonQuery(sqlText);
		}

		void DropColumn(DbConnection connection, string dbName, string tableName, string columnName)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "ALTER TABLE [{0}] DROP COLUMN [{1}]", tableName, columnName);
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, tableName, columnName).DropRelateObjects(connection);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void ChangeColumn(DbConnection connection, string dbName, string tableName, string columnName)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "ALTER TABLE [{0}] ALTER COLUMN [{1}] int", tableName, columnName);
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, tableName, columnName).DropRelateObjects(connection);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		CdcConfigDataSet LoadDataSetFromFile(string filePath)
		{
			var result = new CdcConfigDataSet();
			using (XmlReader reader = XmlReader.Create(filePath))
			{
				using (DataSet tempDataSet = new DataSet())
				{
					tempDataSet.ReadXml(reader, XmlReadMode.ReadSchema);
					tempDataSet.AcceptChanges();
					result.Merge(tempDataSet, false);
				}
			}
			return result;
		}

		void AssertTableAdded(DbConnection connection, string dbName, string tableName)
		{
			var query = string.Format(CultureInfo.InvariantCulture,
				"SELECT COUNT(*) FROM [{0}].information_schema.tables WHERE table_name = '{1}'",
				dbName, tableName);
			Assert(string.Format("Table {0} should be added.", tableName), Convert.ToInt32(connection.ExecuteScalar(query)) > 0);
		}

		void AssertColumnAdded(DbConnection connection, string dbName, string tableName, string columnName)
		{
			var query = string.Format(CultureInfo.InvariantCulture,
				"SELECT COUNT(*) FROM [{0}].information_schema.columns WHERE table_name = '{1}' and column_name = '{2}'",
				dbName, tableName, columnName);
			Assert(string.Format("Column {0} should be added.", columnName), Convert.ToInt32(connection.ExecuteScalar(query)) > 0);
		}

		void AssertColumnDropped(DbConnection connection, string dbName, string tableName, string columnName)
		{
			var query = string.Format(CultureInfo.InvariantCulture,
				"SELECT COUNT(*) FROM [{0}].information_schema.columns WHERE table_name = '{1}' and column_name = '{2}'",
				dbName, tableName, columnName);
			AssertEquals(string.Format("Column {0} should be deleted.", columnName), 0, Convert.ToInt32(connection.ExecuteScalar(query)));
		}

		void AssertColumnChanged(DbConnection connection, string dbName, string tableName, string columnName)
		{
			var query = string.Format(CultureInfo.InvariantCulture,
				"SELECT COUNT(*) FROM [{0}].information_schema.columns WHERE table_name = '{1}' and column_name = '{2}' and data_type = 'int'",
				dbName, tableName, columnName);
			Assert(string.Format("Column {0} was not changed.", columnName), Convert.ToInt32(connection.ExecuteScalar(query)) > 0);
		}

		#endregion
	}
}
