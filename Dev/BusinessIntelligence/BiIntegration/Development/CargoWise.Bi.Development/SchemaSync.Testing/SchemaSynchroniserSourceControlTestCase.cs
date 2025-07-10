using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Bi.Development.Common;
using CargoWise.BuildTools.Testing;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.IO;
using Enterprise.DbUpgrader.Resource;
using Enterprise.DbUpgrader.Shared;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CargoWise.Bi.Development.SchemaSync.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	class SchemaSynchroniserSourceControlTestCase : TestCase
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

			MockSourceControl.Setup();
			BiFiles.ResetPaths();
			BiAutomationConfigLoaderForDevelopment.Instance.ResetConfiguration();

			BiDatabase.ServerName = Db.ServerName;
			BiDatabase.AuditDatabaseName = Db.AuditDatabaseName;
			BiDatabase.EdwDatabaseName = Db.EdwDatabaseName;
		}

		/// <summary>
		/// Undo all MasterSetUp changes.
		/// </summary>
		protected override void TearDown()
		{
			MockSourceControl.TearDown();
			base.TearDown();
		}

		[UseSnapshotProtection]
		public void TestAssertEdwTableConfigFilesAreUpdated()
		{
			var biConfigFileType = BiConfigFileType.EdwTableConfig;
			TestBiConfigurationFiles(biConfigFileType);
		}

		[UseSnapshotProtection]
		public void TestAssertCdcTableConfigFilesAreUpdated()
		{
			var biConfigFileType = BiConfigFileType.CdcTableConfig;
			TestBiConfigurationFiles(biConfigFileType);
		}

		[UseSnapshotProtection]
		public void TestAssertEdwDenormalizedTableConfigFilesAreUpdated()
		{
			var biConfigFileType = BiConfigFileType.EdwDenormalizedTableConfig;
			TestBiConfigurationFiles(biConfigFileType);
		}

		[UseSnapshotProtection]
		public void TestAssertEdwCustomTableConfigFilesAreUpdated()
		{
			var biConfigFileType = BiConfigFileType.EdwCustomTableConfig;
			TestBiConfigurationFiles(biConfigFileType);
		}

		[UseSnapshotProtection]
		public void TestAssertEdwModelViewTableConfigFilesAreUpdated()
		{
			var biConfigFileType = BiConfigFileType.EdwModelViewTableConfig;
			TestBiConfigurationFiles(biConfigFileType);
		}

		[UseSnapshotProtection]
		public void TestAssertSsasCubesConfigFilesAreUpdated()
		{
			var biConfigFileType = BiConfigFileType.SsasCubes;
			TestBiConfigurationFiles(biConfigFileType);
		}

		[UseSnapshotProtection]
		public void TestAssertTabularModelConfigFilesAreUpdated()
		{
			var biConfigFileType = BiConfigFileType.TabularModel;
			TestBiConfigurationFiles(biConfigFileType);
		}

		[UseSnapshotProtection]
		public void TestAssertReportMappingConfigFilesAreUpdated()
		{
			var biConfigFileType = BiConfigFileType.ReportMappingConfig;
			TestBiConfigurationFiles(biConfigFileType);
		}

		void TestBiConfigurationFiles(BiConfigFileType biConfigFileType)
		{
			using (var temp = new TempDirectory())
			using (DatabaseScriptTestCase.InitializePaths(temp.DirectoryName))
			{
				SchemaSynchroniser.SyncConfiguration();

				var sourceConfigurationFilePath = Path.Combine(TestCase.BaseSourcePath, @"Database\BusinessIntelligence\ConfigLoader\ConfigLoader\Config", biConfigFileType.ToString());
				var outputConfigurationFilePath = Path.Combine(temp.DirectoryName, biConfigFileType.ToString());

				var sourceFileList = Directory.EnumerateFiles(sourceConfigurationFilePath, "*.xml", SearchOption.AllDirectories);
				var outputFileList = Directory.EnumerateFiles(outputConfigurationFilePath, "*.xml", SearchOption.AllDirectories);

				AssertContainsExactElementsInAnyOrder($"{biConfigFileType} file list should be the same. Run 'BiRegenSynchroniser.exe -sync' in a console.", outputFileList.Select(f => new FileInfo(f).Name), sourceFileList.Select(f => new FileInfo(f).Name));

				CombineAssertions("Run 'BiRegenSynchroniser.exe -sync' in a console.", () =>
				{
					foreach (var actualFile in sourceFileList)
					{
						var expectedFile = Path.Combine(temp.DirectoryName, biConfigFileType.ToString(), Path.GetFileName(actualFile));
						AssertFilesAreEqual(Path.GetFileName(expectedFile), expectedFile, actualFile);
					}
				});
			}
		}

		void AssertFilesAreEqual(string message, string expectedFile, string actualFile)
		{
			var expectedFileContent = File.ReadAllText(expectedFile);
			var actualFileContent = File.ReadAllText(actualFile);

			if (expectedFileContent.Length == actualFileContent.Length)
			{
				var batchSize = expectedFileContent.Length / 5 + 1;
				for (int i = 0; i < expectedFileContent.Length; i += batchSize)
				{
					int lengthToCompare = Math.Min(batchSize, expectedFileContent.Length - i);

					var batch1 = expectedFileContent.Substring(i, lengthToCompare);
					var batch2 = actualFileContent.Substring(i, lengthToCompare);

					AssertMultilineASCIIEquals(message, batch1, batch2);
				}
			}
			else
			{
				Fail($"File sizes are not equal [{actualFile}]");
			}
		}

		[UseSnapshotProtection]
		public void TestAssertAuditSchemaIsUpdated()
		{
			using (var sourceTemp = new TempDirectory())
			using (var temp = new TempDirectory())
			using (DatabaseScriptTestCase.InitializePaths(temp.DirectoryName))
			{
				SchemaSynchroniser.SyncConfiguration();
				var manager = new ScriptManager();

				var sourceAuditSchemaFile = Path.Combine(sourceTemp.DirectoryName, "Schema_Audit.sql");
				File.WriteAllText(sourceAuditSchemaFile, manager.AuditDbSchemaScript);

				var outputAuditSchemaFile = Path.Combine(temp.DirectoryName, @"Schema_Audit.sql");

				Assert("Schema Audit file was not saved to correct directory.", File.Exists(outputAuditSchemaFile));
				AssertFilesAreEqual("Schema Audit file is out of date.", outputAuditSchemaFile, sourceAuditSchemaFile);
			}
		}

		[UseSnapshotProtection]
		public void TestAssertEdwSchemaIsUpdated()
		{
			using (var sourceTemp = new TempDirectory())
			using (var temp = new TempDirectory())
			using (DatabaseScriptTestCase.InitializePaths(temp.DirectoryName))
			{
				SchemaSynchroniser.SyncConfiguration();
				var manager = new ScriptManager();

				var sourceEdwSchemaFile = Path.Combine(sourceTemp.DirectoryName, @"Schema_EDW.sql");
				File.WriteAllText(sourceEdwSchemaFile, manager.BiEdwDbSchemaScript);

				var outputEdwSchemaFile = Path.Combine(temp.DirectoryName, @"Schema_EDW.sql");

				Assert("Schema EDW file was not saved to correct directory.", File.Exists(outputEdwSchemaFile));
				AssertFilesAreEqual("Schema EDW file is out of date.", outputEdwSchemaFile, sourceEdwSchemaFile);
			}
		}

		public void TestAggregateTableDependencyOrder()
		{
			BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.SortModelDependencyOrder();

			var aggTableDataView = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedTableConfig.DefaultView;
			aggTableDataView.Sort = "DependencyOrder";
			var aggTables = (BiAutomationConfigDataSet.EdwDenormalizedTableConfigDataTable)aggTableDataView.Table;

			CombineAssertions(() =>
			{
				var refAggTables = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwDenormalizedTableConfig;

				foreach (var aggTable in aggTables)
				{
					var sourceTables = BiAutomationConfigDataSet.ParseSourceTables(aggTable.Expression);
					foreach (var sourceTable in sourceTables)
					{
						var filteredRefAggTables = refAggTables.AsParallel().Where(t => t.Schema == sourceTable.Schema && t.Name == sourceTable.Table).FirstOrDefault();
						if (filteredRefAggTables != null)
						{
							if (filteredRefAggTables.DependencyOrder > aggTable.DependencyOrder)
							{
								Fail(string.Format("Aggregate Table [{0}].[{1}] has dependency on table [{2}].[{3}] that has a higher Dependency Order ID", aggTable.Schema, aggTable.Name, filteredRefAggTables.Schema, filteredRefAggTables.Name));
							}
						}
					}
				}
			});

			Assert(true);
		}

		/// <summary>
		/// Assert BI Configuration XML file is a valid subset of main DB database schema without the desirable exclusions.
		/// from the previous version of the same file.
		/// </summary>
		[UseSnapshotProtection]
		public void TestAssertBiConfigurationContainsUpdatedMainDbSchema()
		{
			var initialBiConfigDataSet = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData;

			using (var temp = new TempDirectory())
			using (DatabaseScriptTestCase.InitializePaths(temp.DirectoryName))
			{
				SchemaSynchroniser.SyncConfiguration();
				var testBiConfigDataSet = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData;

				CombineAssertions(() =>
				{
					var tablesNotAdded = from table in initialBiConfigDataSet.CdcTableConfig
										 where table.Action == "Added" &&
										 !(from otable in testBiConfigDataSet.CdcTableConfig
										   select otable.SourceTable).Contains(table.SourceTable)
										 select table.SourceTable;
					Assert("There are added tables in database schema that had not been added to BI configuration file", !tablesNotAdded.Any());

					var tablesNotDeleted = from table in initialBiConfigDataSet.CdcTableConfig
										   where table.Action == "Deleted" &&
										   (from otable in testBiConfigDataSet.CdcTableConfig
											select otable.SourceTable).Contains(table.SourceTable)
										   select table.SourceTable;
					Assert("There are deleted tables in database schema that had not been deleted in BI configuration file", !tablesNotDeleted.Any());

					var columnsNotAdded = from column in initialBiConfigDataSet.CdcColumnConfig
										  where column.Action == "Added" &&
										  !(from ocolumn in testBiConfigDataSet.CdcColumnConfig
											select ocolumn.SourceColumn).Contains(column.SourceColumn)
										  select column.SourceColumn;
					Assert("There are added columns in database schema that had not been added to BI configuration file", !columnsNotAdded.Any());

					var columnsNotDeleted = from column in initialBiConfigDataSet.CdcColumnConfig
											where column.Action == "Deleted" &&
											!(from ocolumn in testBiConfigDataSet.CdcColumnConfig
											  select ocolumn.SourceColumn).Contains(column.SourceColumn)
											select column.SourceColumn;
					Assert("There are deleted columns in database schema that had not been deleted in BI configuration file", !columnsNotDeleted.Any());

					var columnsNotChanged = from column in initialBiConfigDataSet.CdcColumnConfig
											join ocolumn in testBiConfigDataSet.CdcColumnConfig on new { column.SourceColumn, column.SourceTable } equals new { ocolumn.SourceColumn, ocolumn.SourceTable }
											where column.Action == "Changed" &&
											((column.DataType != ocolumn.DataType) || (column.MaxLength != ocolumn.MaxLength) || (column.Precision != ocolumn.Precision) ||
											(column.Scale != ocolumn.Scale) || (column.Nullable != ocolumn.Nullable) || (column.IsPrimaryKey != ocolumn.IsPrimaryKey))
											select column.SourceColumn;
					Assert("There are updated columns in database schema that had not been changed in BI configuration file", !columnsNotChanged.Any());
				});
			}
		}

		[UseSnapshotProtection]
		public void TestCdcColumnDataTypeChanges()
		{
			using (var temp = new TempDirectory())
			using (DatabaseScriptTestCase.InitializePaths(temp.DirectoryName))
			{
				var edwColumn = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwColumnConfig.First(ec =>
					BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig.Any(cc => cc.DataType == "nvarchar" && cc.MaxLength < 100 && string.Equals(cc.SourceColumn, ec.Expression, StringComparison.OrdinalIgnoreCase)));
				var cdcColumn = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcColumnConfig.First(cc => string.Equals(cc.SourceColumn, edwColumn.Expression, StringComparison.OrdinalIgnoreCase));

				using (var conn = Db.NewAdminConnection())
				{
					new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, cdcColumn.SourceTable, cdcColumn.SourceColumn).DropRelateObjects(conn);

					conn.ExecuteNonQuery($"ALTER TABLE [{cdcColumn.SourceTable}] ALTER COLUMN [{cdcColumn.SourceColumn}] varchar(100)");
				}

				SchemaSynchroniser.SyncConfiguration();

				CombineAssertions(() =>
				{
					AssertEquals("EDW column data type", "varchar", edwColumn.DataType);
					AssertEquals("EDW column max length", 100, edwColumn.MaxLength);
				});
			}
		}

		[UseSnapshotProtection]
		public void TestCdcTableAndColumnChangedCasing()
		{
			using (var temp = new TempDirectory())
			using (DatabaseScriptTestCase.InitializePaths(temp.DirectoryName))
			{
				var cdcTable = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.CdcTableConfig.First(t => t.SourceTable == "AccBankAccount");
				var cdcColumn = cdcTable.GetCdcColumnConfigRows().First(c => c.SourceColumn != c.SourceColumn.ToUpperInvariant() && c.ColumnInEdw);

				var tableNameRegex = new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", cdcTable.SourceTable), RegexOptions.IgnoreCase);
				var columnNameRegex = new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", cdcColumn.SourceColumn), RegexOptions.IgnoreCase);

				var edwTable = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData.EdwTableConfig.First(t => t.SourceSchema == cdcTable.SourceSchema && t.StagingTable == cdcTable.SourceTable);
				var edwColumn = edwTable.GetEdwColumnConfigRows().First(c => columnNameRegex.IsMatch(c.Expression));

				var cdcSchemaName = cdcTable.SourceSchema;
				var newCdcTableName = cdcTable.SourceTable.ToUpperInvariant();
				var newCdcColumnName = cdcColumn.SourceColumn.ToUpperInvariant();

				var expectedEdwColumnExpression = columnNameRegex.Replace(edwColumn.Expression, newCdcColumnName);

				using (var conn = Db.NewAdminConnection())
				{
					RenameTable(conn, cdcTable.SourceTable, newCdcTableName);
					RenameColumn(conn, newCdcTableName, cdcColumn.SourceColumn, newCdcColumnName);
				}
				SchemaSynchroniser.LoadBiConfiguration();
				SchemaSynchroniser.SyncSchemaConfiguration();

				CombineAssertions(() =>
				{
					AssertEquals("Source Table Name", newCdcTableName, cdcTable.SourceTable);
					AssertEquals("Source Column Name", newCdcColumnName, cdcColumn.SourceColumn);
					AssertEquals("Column SourceTable", newCdcTableName, cdcColumn.SourceTable);

					AssertEquals("Source Schema", cdcSchemaName, edwTable.SourceSchema);
					AssertEquals("Staging Table", newCdcTableName, edwTable.StagingTable);
					AssertEquals("EDW Column Expression", expectedEdwColumnExpression, edwColumn.Expression);
				});
			}
		}

		[UseSnapshotProtection]
		public void TestRunTestQueryForAllEdwTablesMultipleTimes_DoesNotThrow()
		{
			using (var temp = new TempDirectory())
			using (DatabaseScriptTestCase.InitializePaths(temp.DirectoryName))
			{
				try
				{
					var scriptManager = new ScriptManager();
					File.WriteAllText(BiFiles.GeneratedEDWTableSchemaPath, scriptManager.BiEdwDbSchemaScript);

					BiDatabase.UseTestDatabaseName = true;
					BiDatabase.EdwDatabaseName = "Test_EDW";
					SchemaSynchroniser.TestQueryForAllEdwTables();
					AssertNoExceptionThrown(SchemaSynchroniser.TestQueryForAllEdwTables);
				}
				finally
				{
					BiDatabase.UseTestDatabaseName = false;
				}
			}
		}

		void RenameTable(DbConnection conn, string oldTableName, string newTableName)
		{
			var oldTableColumns = DbObjectCreator.GetTableColumns(conn, oldTableName);
			foreach (var oldColumn in oldTableColumns)
			{
				new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, oldTableName, oldColumn).DropRelateObjects(conn);
			}
			DbObjectCreator.RenameTable(conn, Db.DatabaseName, Db.SqlDbOwnerSchema, oldTableName, newTableName);
		}

		void RenameColumn(AdminConnection conn, string tableName, string oldColumnName, string newColumnName)
		{
			var sqlText = $"EXEC sp_rename '{tableName}.{oldColumnName}', '{newColumnName}', 'COLUMN'";
			conn.ExecuteNonQuery(sqlText);
		}

		public void TestRunQuery()
		{
			using (var connection = Db.NewAdminConnection(BiDatabase.ServerName, Db.SqlMasterDb))
			{
				var query = @"SELECT 1";
				var outputMsg = SchemaSynchroniser.RunQuery(query, connection);
				AssertNull("Query should execute successfully.", outputMsg);

				query = @"RAISERROR('Test error', 16, 1)";
				outputMsg = SchemaSynchroniser.RunQuery(query, connection);
				Assert("Query should execute with error.", outputMsg.Contains("Test error"));

				query = "SELECT 1 GO SELECT 2";
				outputMsg = SchemaSynchroniser.RunQuery(query, connection);
				AssertNull("Query should execute successfully.", outputMsg);

				query = "SELECT 'WGO' GO SELECT 'WGO1'";
				outputMsg = SchemaSynchroniser.RunQuery(query, connection);
				AssertNull("Query should execute successfully.", outputMsg);
			}
		}

		public void TestAssertBiModelQuery()
		{
			var biModelPath = Path.Combine(TestCase.BaseSourcePath, @"BusinessIntelligence");

			var sourceFileList = Directory.EnumerateFiles(biModelPath, "*.bim", SearchOption.AllDirectories);

			CombineAssertions("Check BI Model if there is 3-part-query.", () =>
			{
				foreach (var file in sourceFileList)
				{
					var jObject = JObject.Parse(File.ReadAllText(file));
					var tables = JObject.Parse(File.ReadAllText(file))["model"]["tables"];
					if (tables != null)
					{
						foreach (var table in tables)
						{
							var partitions = table["partitions"];
							var annotations = table["annotations"];
							foreach (var partition in partitions)
							{
								if (partition["source"]["type"] == null || partition["source"]["type"].ToString().Equals("query"))
								{
									Assert($"BI Model has invalid query, name: {partition["name"]}", QueryValidation(GetQueryFromJson(partition["source"]["query"])));
									if (partition["annotations"] != null)
									{
										foreach (var annotation in partition["annotations"])
										{
											if (annotation["name"] != null && annotation["name"].ToString().Equals("QueryEditorSerialization") && annotation["value"] != null)
											{
												Assert($"BI Model has invalid QueryEditorSerialization, name: {partition["name"]}", QueryValidation(GetQueryFromJson(annotation["value"])));
											}
										}
									}
									foreach (var annotation in annotations)
									{
										var annotationName = annotation["name"].ToString();
										if ((annotationName.Equals("_TM_ExtProp_QueryDefinition") || annotationName.Equals("QueryEditorSerialization")) && annotation["value"] != null)
										{
											Assert($"BI Model has invalid {annotationName} in annotation, name: {partition["name"]}", QueryValidation(GetQueryFromJson(annotation["value"])));
										}
									}
								}
							}
						}
					}
				}
			});
		}

		string GetQueryFromJson(IEnumerable<JToken> jTokens)
		{
			var result = string.Join(" ", jTokens);
			return result.IsNullOrEmpty() ? jTokens.ToString() : result;
		}
		
		bool QueryValidation(string query)
		{
			query = query.Replace("\n", " ").Replace("\t", " ").Replace("\r", " ");
			var fieldIndex = new Stack<int>();
			var index = 0;

			while (index < query.Length)
			{
				var selectIndex = query.IndexOf("SELECT", index, StringComparison.CurrentCultureIgnoreCase);
				if (selectIndex == -1)
				{
					break;
				}
				fieldIndex.Push(selectIndex + 6);
				index = selectIndex + 6;

				if ((index + 8) < query.Length && query.Substring(index).TrimStart().Substring(0, 8).ToUpper().Equals("DISTINCT"))
				{
					var distinctIndex = query.IndexOf("DISTINCT", index, StringComparison.CurrentCultureIgnoreCase);
					fieldIndex.Pop();
					fieldIndex.Push(distinctIndex + 8);
					index = distinctIndex + 8;
				}

				var fromIndex = query.IndexOf("FROM", index, StringComparison.CurrentCultureIgnoreCase);
				if (fromIndex == -1)
				{
					break;
				}

				var startIndexOfFields = fieldIndex.Pop();

				var fields = query.Substring(startIndexOfFields, fromIndex - startIndexOfFields).Split(',');

				index = fromIndex + 4;
				var fromTable = GetTable(query, ref index);
				string joinTable = null;
				if ((index + 4) < query.Length && query.Substring(index).TrimStart().Substring(0, 4).ToUpper().Equals("JOIN"))
				{
					var joinIndex = query.IndexOf("JOIN", index, StringComparison.CurrentCultureIgnoreCase);
					index = joinIndex + 4;
					joinTable = GetTable(query, ref index);
				}

				for (var i = 0; i < fields.Length; i++)
				{
					fields[i] = fields[i].Trim();

					if (fields[i].StartsWith(fromTable))
					{
						return false;
					}

					if (joinTable != null && fields[i].StartsWith(joinTable) && !IsFieldNameDuplicated(fields[i], fromTable, joinTable))
					{
						return false;
					}
				}
			}

			return true;
		}

		string GetTable(string query, ref int index)
		{
			var hasExtraFlag = false;

			while (query[index] == ' ')
			{
				index++;
			}

			var startIndex = index;

			if (query[index] == '[')
			{
				while (index < query.Length)
				{
					if (query[index] == '[' && !hasExtraFlag)
					{
						hasExtraFlag = true;
					}

					if (query[index] == ']' && hasExtraFlag)
					{
						hasExtraFlag = false;
						if ((index + 1) >= query.Length || query[index + 1] != '.')
						{
							index++;
							return query.Substring(startIndex, index - startIndex);
						}
					}

					if (query[index + 1] == ' ' && !hasExtraFlag)
					{
						index++;
						return query.Substring(startIndex, index - startIndex);
					}

					index++;
				}
			}

			else
			{
				return query.Substring(startIndex, (query.IndexOf(' ', startIndex) == -1 ? query.Length : query.IndexOf(' ', startIndex)) - startIndex);
			}

			return string.Empty;
		}

		bool IsFieldNameDuplicated(string fullFieldName, string fromTableName, string joinTableName)
		{
			if (fullFieldName.StartsWith(fromTableName) || fullFieldName.StartsWith(joinTableName))
			{
				var tableName = fullFieldName.StartsWith(fromTableName) ? joinTableName : fromTableName;
				var fieldName = fullFieldName.Split('.').Last().Replace("[", "").Replace("]", "");
				var sqlText = $@"SELECT NULL FROM sys.columns WHERE object_id = OBJECT_ID('{tableName}') AND name = '{fieldName}'";

				using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
				{
					return connection.ExecuteScalar(sqlText) == DBNull.Value;
				}
			}

			return false;
		}
	}
}
