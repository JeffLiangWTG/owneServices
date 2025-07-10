using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema
{
	sealed class EdwDatabasePopulatorTest : TestCase
	{
		protected override void TearDown()
		{
			BiAutomationConfigLoader.Instance.ResetConfiguration();
			base.TearDown();
		}

		public void TestIsStagingTableConfigurationPopulated()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var populator = new EdwDatabasePopulatorForTest(connection);

				var stagingTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => t.TableInEdw && !t.IsEdiClient);

				AssertEquals("Table Configuration count", stagingTables.Count(), Convert.ToInt32(connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM [{0}].[StagingTableConfiguration]", BiConstants.BiAdminSchemaName))));

				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
IF EXISTS(SELECT NULL
		FROM [{0}].[StagingTableConfiguration]
		WHERE SourceSchemaName = '@SourceSchemaName' AND
			StagingSchemaName = '@StagingSchemaName' AND
			StagingTableName = '@StagingTableName' AND
			EdwFilter = '@EdwFilter' AND
			PkName = '@PkName' AND
			IndexedColumnName = '@IndexedColumnName' AND
			StagingTableDefinition = N'@StagingTableDefinition' AND
			StagingTableColumnListInsert = N'@StagingTableColumnListInsert' AND
			StagingTableColumnListSelect = N'@StagingTableColumnListSelect' AND
			ColumnEnumeratedList = N'@ColumnEnumeratedList' AND
			ColumnEnumeratedListWithDefinition = N'@ColumnEnumeratedListWithDefinition')
	SELECT 1
ELSE
	SELECT 0",
					BiConstants.BiAdminSchemaName);

				CombineAssertions(() =>
				{
					foreach (var table in stagingTables)
					{
						var cmdText = sqlText;
						var cdcColumnConfig = BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.Where(c => c.SourceTable == table.SourceTable && c.ColumnInEdw);
						var regex = new Regex(string.Format(CultureInfo.InvariantCulture, @"\b{0}\b", cdcColumnConfig.Where(c => c.IsPrimaryKey).FirstOrDefault().SourceColumn), RegexOptions.IgnoreCase);
						var edwTable = BiAutomationConfigLoader.Instance.ConfigData.EdwColumnConfig
							.Where(ec => regex.Match(ec.Expression).Success)
							.FirstOrDefault()
							.GetEdwTable();

						cmdText = cmdText.Replace("@SourceSchemaName", table.SourceSchema);
						cmdText = cmdText.Replace("@StagingSchemaName", "Staging");
						cmdText = cmdText.Replace("@StagingTableName", table.SourceTable);
						cmdText = cmdText.Replace("@EdwFilter", table.EdwFilter?.Replace("'", "''"));
						cmdText = cmdText.Replace("@PkName", cdcColumnConfig.Where(c => c.IsPrimaryKey).FirstOrDefault().SourceColumn);
						cmdText = cmdText.Replace("@IndexedColumnName", table.IndexedColumn);
						cmdText = cmdText.Replace("@DependencyOrder", edwTable.DependencyOrder.ToString());
						cmdText = cmdText.Replace("@StagingTableDefinition", string.Join(",", cdcColumnConfig.Select(c => string.Format(CultureInfo.InvariantCulture, "{0} {1}", c.SourceColumn, populator.GetDataTypeDefinition_Exposed(c.DataType, c.MaxLength, c.Precision, c.Scale)))));
						cmdText = cmdText.Replace("@StagingTableColumnListInsert", string.Join(",", cdcColumnConfig.Select(c => c.SourceColumn)));
						cmdText = cmdText.Replace("@StagingTableColumnListSelect", string.Join(",", cdcColumnConfig.Select(c => c.DataType.Equals("xml", StringComparison.OrdinalIgnoreCase) ? $"CAST({c.SourceColumn} AS NVARCHAR(MAX)) AS {c.SourceColumn}" : c.SourceColumn)));

						int counter = 1;
						cmdText = cmdText.Replace("@ColumnEnumeratedListWithDefinition", string.Join(",", cdcColumnConfig.Select(c => string.Format(CultureInfo.InvariantCulture, "c{0} {1}", counter++, c.DataType.Equals("xml", StringComparison.OrdinalIgnoreCase) ? "NVARCHAR(MAX)" : populator.GetDataTypeDefinition_Exposed(c.DataType, c.MaxLength, c.Precision, c.Scale)))));
						counter = 1;
						cmdText = cmdText.Replace("@ColumnEnumeratedList", string.Join(",", cdcColumnConfig.Select(c => string.Format(CultureInfo.InvariantCulture, "c{0}", counter++))));

						Assert(string.Format("Table {0} not in StagingTableConfiguration", table.SourceTable), Convert.ToBoolean(connection.ExecuteScalar(cmdText)));
					}
				});
			}
		}

		public void TestIsTransformTableConfigurationPopulated()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				connection.IgnoreCommitTracker = true;

				var populator = new EdwDatabasePopulatorForTest(connection);

				var stagingTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => t.TableInEdw && !t.IsEdiClient).Select(t => t.SourceTable);
				var edwTables = BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.Where(t => stagingTables.Contains(t.StagingTable));

				AssertEquals("Table Configuration count", edwTables.Count(), Convert.ToInt32(connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM [{0}].[TransformTableConfiguration]", BiConstants.BiAdminSchemaName))));

				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
IF EXISTS(SELECT NULL
		FROM [{0}].[TransformTableConfiguration]
		WHERE SourceTableName = '@SourceTableName' AND 
			ModelSchemaName = '@ModelSchemaName' AND 
			ModelTableName = '@ModelTableName' AND 
			DependencyOrder = @DependencyOrder AND 
			TransformId = @TransformId AND 
			IsLastTransform = @IsLastTransform AND 
			HasChildTables = @HasChildTables AND 
			IsSelfReferenced = @IsSelfReferenced AND
			InitialLoadQuery = N'@InitialLoadQuery' AND
			IncrementalInsertQuery = N'@IncrementalInsertQuery' AND
			IncrementalDeleteQuery = N'@IncrementalDeleteQuery' AND
			WhereCondition = N'@WhereCondition' AND
			CustomIndexScript = N'@CustomIndexScript')
	SELECT 1
ELSE
	SELECT 0",
					BiConstants.BiAdminSchemaName);

				CombineAssertions(() =>
				{
					foreach (var edwTable in edwTables)
					{
						var cmdText = sqlText;

						var sourceTable = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.FirstOrDefault(t => t.SourceSchema == edwTable.SourceSchema && t.SourceTable == edwTable.StagingTable);
						if (sourceTable != null)
						{
							cmdText = cmdText.Replace("@SourceTableName", edwTable.SourceSchema + "." + edwTable.StagingTable);
							cmdText = cmdText.Replace("@ModelSchemaName", edwTable.Schema);
							cmdText = cmdText.Replace("@ModelTableName", edwTable.Name);
							cmdText = cmdText.Replace("@DependencyOrder", edwTable.DependencyOrder.ToString());
							cmdText = cmdText.Replace("@TransformId", edwTable.TransformId.ToString());
							cmdText = cmdText.Replace("@IsLastTransform", BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.IsLastTransform(edwTable) ? "1" : "0");
							cmdText = cmdText.Replace("@HasChildTables", BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.HasChildTables(edwTable) ? "1" : "0");
							cmdText = cmdText.Replace("@IsSelfReferenced", BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.IsSelfReferenced(edwTable) ? "1" : "0");

							cmdText = cmdText.Replace("@InitialLoadQuery", FormatQuery(BiAutomationConfigLoader.Instance.ConfigData.GetInitialLoadQueryForEdwTable(edwTable)));
							cmdText = cmdText.Replace("@IncrementalInsertQuery", FormatQuery(BiAutomationConfigLoader.Instance.ConfigData.GetIncrementalInsertQueryForEdwTable(edwTable)));
							cmdText = cmdText.Replace("@IncrementalDeleteQuery", FormatQuery(BiAutomationConfigLoader.Instance.ConfigData.GetIncrementalDeleteQueryForEdwTable(edwTable)));
							cmdText = cmdText.Replace("@WhereCondition", FormatQuery(edwTable.WhereClause ?? ""));
							cmdText = cmdText.Replace("@CustomIndexScript", FormatQuery(populator.GetCustomIndexScript(edwTable)));

							Assert(string.Format("Table [{0}].[{1}] not in TransformTableConfiguration", edwTable.Schema, edwTable.Name), Convert.ToBoolean(connection.ExecuteScalar(cmdText)));
						}
						else
						{
							Fail(string.Format("Could not find staging table [{0}].", edwTable.StagingTable));
						}
					}
				});
			}
		}

		public void TestGetCustomIndexScript()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var stagingTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => t.TableInEdw && !t.IsEdiClient).Select(t => t.SourceTable);
				var edwTable = BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.Where(t => stagingTables.Contains(t.StagingTable)).First();

				var populator = new EdwDatabasePopulatorForTest(connection);
				var customIndexScriptBefore = "CREATE NONCLUSTERED INDEX [IX_Finance_BAS__AlternateChart_AlternateChartID] ON [Finance].[BAS__AlternateChart] (AlternateChartID, [__$transform_id]);"
					+ "CREATE INDEX [IX_Finance_BAS__AlternateChart_AlternateChartKey] ON [Finance].[BAS__AlternateChart] (AlternateChartKey);";
				var expectedCustomIndexScriptAfter = $"CREATE NONCLUSTERED INDEX /* Table:Ini_{edwTable.Schema}.{edwTable.Name} */ [IX_Finance_BAS__AlternateChart_AlternateChartID] ON [Finance].[BAS__AlternateChart] (AlternateChartID, [__$transform_id]);"
					+ $"CREATE INDEX /* Table:Ini_{edwTable.Schema}.{edwTable.Name} */ [IX_Finance_BAS__AlternateChart_AlternateChartKey] ON [Finance].[BAS__AlternateChart] (AlternateChartKey);";

				var actualCustomIndexScriptAfter = populator.InsertTrackingCommentsInIndexScript(customIndexScriptBefore, edwTable);

				AssertEquals(expectedCustomIndexScriptAfter, actualCustomIndexScriptAfter);
			}
		}

		public void TestIsCustomTableConfigurationPopulated()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				connection.IgnoreCommitTracker = true;

				var populator = new EdwDatabasePopulatorForTest(connection);

				var customTables = BiAutomationConfigLoader.Instance.ConfigData.EdwCustomTableConfig;

				AssertEquals("Table Configuration count", customTables.Count, Convert.ToInt32(connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM [{0}].[CustomTableConfiguration]", BiConstants.BiAdminSchemaName))));

				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
IF EXISTS(SELECT NULL
		FROM [{0}].[CustomTableConfiguration]
		WHERE
			ModelSchemaName = '@ModelSchemaName' AND 
			ModelTableName = '@ModelTableName' AND 
			RunBeforeTransform = @RunBeforeTransform AND 
			DependencyOrder = @DependencyOrder AND 
			TableDependencyList = '@TableDependencyList' AND
			CustomTableDependencyList = '@CustomTableDependencyList' AND
			ViewName = '@ViewName' AND
			InitialLoadQuery = N'@InitialLoadQuery' AND
			IncrementalLoadQuery = N'@IncrementalLoadQuery')
	SELECT 1
ELSE
	SELECT 0",
					BiConstants.BiAdminSchemaName);

				CombineAssertions(() =>
				{
					foreach (var customTable in customTables)
					{
						var cmdText = sqlText;

						cmdText = cmdText.Replace("@ModelSchemaName", customTable.Schema);
						cmdText = cmdText.Replace("@ModelTableName", customTable.Name);
						cmdText = cmdText.Replace("@RunBeforeTransform", customTable.RunBeforeTransform ? "1" : "0");
						cmdText = cmdText.Replace("@DependencyOrder", customTable.DependencyOrder.ToString());
						cmdText = cmdText.Replace("@TableDependencyList", customTable.GetTableDependencyList());
						cmdText = cmdText.Replace("@CustomTableDependencyList", string.Join(",", customTable.ParseCustomTablesFromQueries().Select(t => "[" + t.Schema + "].[" + t.Name + "]")));
						cmdText = cmdText.Replace("@ViewName", customTable.ViewName);

						cmdText = cmdText.Replace("@InitialLoadQuery", FormatQuery(customTable.InitialLoadQuery));
						cmdText = cmdText.Replace("@IncrementalLoadQuery", FormatQuery(customTable.IncrementalLoadQuery));

						Assert(string.Format("Table [{0}].[{1}] not in CustomTableConfiguration", customTable.Schema, customTable.Name), Convert.ToBoolean(connection.ExecuteScalar(cmdText)));
					}
				});
			}
		}

		public void TestIsModelTableConfigurationPopulated()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var populator = new EdwDatabasePopulatorForTest(connection);

				var aggTables = BiAutomationConfigLoader.Instance.ConfigData.EdwDenormalizedTableConfig;

				AssertEquals("Table Configuration count", aggTables.Count, Convert.ToInt32(connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM [{0}].[ModelTableConfiguration]", BiConstants.BiAdminSchemaName))));

				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
IF EXISTS(SELECT NULL
		FROM [{0}].[ModelTableConfiguration]
		WHERE ModelSchemaName = '@ModelSchemaName' AND 
			ModelTableName = '@ModelTableName' AND 
			DependencyOrder = @DependencyOrder AND 
			SourceTables = '@SourceTables')
	SELECT 1
ELSE
	SELECT 0",
					BiConstants.BiAdminSchemaName);

				CombineAssertions(() =>
				{
					foreach (var aggTable in aggTables)
					{
						var cmdText = sqlText;

						cmdText = cmdText.Replace("@ModelSchemaName", aggTable.Schema);
						cmdText = cmdText.Replace("@ModelTableName", aggTable.Name);
						cmdText = cmdText.Replace("@DependencyOrder", aggTable.DependencyOrder.ToString());
						cmdText = cmdText.Replace("@SourceTables", string.Join(",", BiAutomationConfigDataSet.ParseSourceTables(aggTable.Expression).Select(st => string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", st.Schema, st.Table)).OrderBy(st => st)));

						Assert(string.Format("Table [{0}].[{1}] not in TransformTableConfiguration", aggTable.Schema, aggTable.Name), Convert.ToBoolean(connection.ExecuteScalar(cmdText)));
					}
				});
			}
		}

		string FormatQuery(string query)
		{
			return EdwDatabasePopulator.FormatQuery(query);
		}

		public void TestIsStagingTableStatePopulated()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var stagingTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => t.TableInEdw && !t.IsEdiClient);
				AssertEquals("Staging Table State count", stagingTables.Count(), Convert.ToInt32(connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM [{0}].[StagingTableState]", BiConstants.BiAdminSchemaName))));

				var sqlText = @"IF EXISTS(SELECT NULL FROM [{0}].[StagingTableState] WHERE SourceTableName = '{1}.{2}') SELECT 1 ELSE SELECT 0";

				foreach (var table in stagingTables)
				{
					Assert(string.Format("Table [{0}].[{1}] not in StagingTableState.", table.SourceSchema, table.SourceTable),
						Convert.ToBoolean(connection.ExecuteScalar(string.Format(sqlText, BiConstants.BiAdminSchemaName, table.SourceSchema, table.SourceTable))));
				}
			}
		}

		public void TestIsTransformTableStatePopulated()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var stagingTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => t.TableInEdw && !t.IsEdiClient).Select(t => t.SourceTable);
				var edwTables = BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.Where(t => stagingTables.Contains(t.StagingTable));

				AssertEquals("Transform Table State count", edwTables.Count(), Convert.ToInt32(connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM [{0}].[TransformTableState]", BiConstants.BiAdminSchemaName))));

				var sqlText = @"IF EXISTS(SELECT NULL FROM [{0}].[TransformTableState] WHERE ModelSchemaName = '{1}' AND ModelTableName = '{2}' AND TransformId = {3}) SELECT 1 ELSE SELECT 0";

				foreach (var table in edwTables)
				{
					Assert(string.Format("Table [{0}].[{1}] not in TransformTableState.", table.Schema, table.Name), Convert.ToBoolean(connection.ExecuteScalar(string.Format(sqlText, BiConstants.BiAdminSchemaName, table.Schema, table.Name, table.TransformId))));
				}
			}
		}

		public void TestIsCustomTableStatePopulated()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var configDataSet = BiAutomationConfigLoader.Instance.ConfigData.EdwCustomTableConfig;

				AssertEquals("Custom Table State count", configDataSet.Count, Convert.ToInt32(connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM [{0}].[CustomTableState]", BiConstants.BiAdminSchemaName))));

				var sqlText = @"IF EXISTS(SELECT NULL FROM [{0}].[CustomTableState] WHERE ModelSchemaName = '{1}' AND ModelTableName = '{2}') SELECT 1 ELSE SELECT 0";

				foreach (var table in configDataSet)
				{
					Assert(string.Format("Table [{0}].[{1}] not in CustomTableState.", table.Schema, table.Name), Convert.ToBoolean(connection.ExecuteScalar(string.Format(sqlText, BiConstants.BiAdminSchemaName, table.Schema, table.Name))));
				}
			}
		}

		public void TestIsModelTableStatePopulated()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var expectedModelTableList = BiAutomationConfigLoader.Instance.ConfigData.EdwDenormalizedTableConfig.Select(t => string.Format("[{0}].[{1}]", t.Schema, t.Name));

				var sqlText = string.Format(CultureInfo.InvariantCulture, @"SELECT '[' + ModelSchemaName + '].[' + ModelTableName + ']' FROM [{0}].[ModelTableState]", BiConstants.BiAdminSchemaName);
				var actualModelTableList = DataUtils.GetListOfValuesFromQuery(connection, sqlText);

				foreach (var expectedModel in expectedModelTableList)
				{
					Assert(string.Format("Table {0} not in ModelTableState.", expectedModel), actualModelTableList.Contains(expectedModel));
				}

				foreach (var actualModel in actualModelTableList)
				{
					Assert(string.Format("Table {0} in ModelTableState is not in BI configuration file.", actualModel), expectedModelTableList.Contains(actualModel));
				}
			}
		}

		public void TestIsSsasCubePopulated()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var populator = new EdwDatabasePopulatorForTest(connection);

				var ssasCubes = BiAutomationConfigLoader.Instance.ConfigData.SsasCubes;

				AssertEquals("SSAS Cube count", ssasCubes.Count, Convert.ToInt32(connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM [{0}].[SsasCube]", BiConstants.BiAdminSchemaName))));

				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
IF EXISTS(SELECT NULL
		FROM [{0}].[SsasCube]
		WHERE
			SsasModelFileName = '@SsasModelFileName' AND
			SsasModelLogicalName = '@SsasModelLogicalName')
	SELECT 1
ELSE
	SELECT 0",
					BiConstants.BiAdminSchemaName);

				CombineAssertions(() =>
				{
					foreach (var ssasCube in ssasCubes)
					{
						var cmdText = sqlText;

						cmdText = cmdText.Replace("@SsasModelFileName", ssasCube.SsasModelFileName);
						cmdText = cmdText.Replace("@SsasModelLogicalName", ssasCube.SsasModelLogicalName);

						Assert(string.Format("Tabular Model [{0}] not in CustomTableConfiguration", ssasCube.SsasModelFileName), Convert.ToBoolean(connection.ExecuteScalar(cmdText)));
					}
				});
			}
		}
		[UseSnapshotProtection(new[] { DatabaseType.EDW })]
		public void TestPopulateCustomTableConfigurationCanHandleLongQueries()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var populator = new EdwDatabasePopulatorForTest(connection);
				BiAutomationConfigLoader.Instance.ConfigData.EdwCustomColumnConfig.Clear();
				BiAutomationConfigLoader.Instance.ConfigData.EdwCustomTableConfig.Clear();

				string RepeatPatternToGetLongQuery(string p, int reps) => "-- This is a normal length query.\r\n" + string.Concat(Enumerable.Repeat(p, reps)) + "\r\n-- This is a normal length query.";
				var pattern = "SELECT * FROM Table WHERE Column = 'Some Value';"; // Repeat the pattern >83.33 times to exceed 4000 chars
				var longInitialLoadQuery = RepeatPatternToGetLongQuery(pattern, 100);
				var longIncrementalLoadQuery = RepeatPatternToGetLongQuery(pattern, 1000);

				BiAutomationConfigLoader.Instance.ConfigData.EdwCustomTableConfig.AddEdwCustomTableConfigRow(
					 "Test",
					 "CUS__UnitTestTable1",
					 -1,
					 "",
					 "",
					 longInitialLoadQuery,
					 longIncrementalLoadQuery,
					 RunBeforeTransform: false,
					 ""
				);

				populator.PopulateCustomTableConfiguration_Exposed();

				var result = DataUtils.GetDataTableFromQuery(connection, @"
					SELECT TOP (1) InitialLoadQuery, IncrementalLoadQuery 
					FROM [biadmin].[CustomTableConfiguration] 
					WHERE ModelSchemaName = 'Test' AND ModelTableName = 'CUS__UnitTestTable1'
			  ");

				var actualInitialLoadQuery = result.Rows[0]["InitialLoadQuery"].ToString();
				var actualIncrementalLoadQuery = result.Rows[0]["IncrementalLoadQuery"].ToString();

				string GetEndOfString(string s) => s.Length > 50 ? s.Substring(s.Length - 50) : s;

				AssertEquals(
					 "Should not truncate the long Initial Load Query after inserting in DB",
					 GetEndOfString(longInitialLoadQuery),
					 GetEndOfString(actualInitialLoadQuery)
				);
				AssertEquals(
					 "Should not truncate the long Incremental Load Query after inserting in DB",
					 GetEndOfString(longIncrementalLoadQuery),
					 GetEndOfString(actualIncrementalLoadQuery)
				);

				AssertEquals(
					 "The length of the Initial Load Query should match after inserting in DB",
					 longInitialLoadQuery.Length,
					 actualInitialLoadQuery.Length
				);
				AssertEquals(
					 "The length of the Incremental Load Query should match after inserting in DB",
					 longIncrementalLoadQuery.Length,
					 actualIncrementalLoadQuery.Length
				);
			}
		}
	}
}
