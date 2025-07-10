using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Bi.Configuration;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Bi.Maintenance;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace CargoWise.Bi.Product.DataLoad.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
	[DatCapabilityRequirement("SOURCE_CODE")]
	class EdwTsqlScriptRunnerTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			BiAutomationConfigLoader.Instance.ResetConfiguration();
		}

		protected override void TearDown()
		{
			BiAutomationConfigLoader.Instance.ResetConfiguration();
			base.TearDown();
		}

		BiConfigurationData configData;

		class BiConfigurationDataForTest : BiConfigurationData
		{
			public BiConfigurationDataForTest()
			{
			}

			public BiConfigurationDataForTest(string schema, string table, params string[] columns)
			{
			}

			protected override void LoadCdcConfigurationFromSharedAssemblies()
			{
			}
		}

		#region ETL run

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRunOrganization()
		{
			ETLRunTransformation("Organization");
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRunCommon()
		{
			ETLRunTransformation("Common");
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRunCustoms()
		{
			ETLRunTransformation("Customs");
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRunDomesticLogistics()
		{
			ETLRunTransformation("DomesticLogistics");
		}

		[UseSnapshotProtection]
		[RequiresLargeDatabase(databaseNameSuffix: Db.EdwDatabaseSuffix)]
		public void TestRunFinance()
		{
			ETLRunTransformation("Finance");
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRunGeography()
		{
			ETLRunTransformation("Geography");
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRunInternationalLogistics()
		{
			ETLRunTransformation("InternationalLogistics");
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRunWarehouse()
		{
			ETLRunTransformation("Warehouse");
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRunWorkflow()
		{
			ETLRunTransformation("Workflow");
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRunIncrementalDoesNotBlockSysTables()
		{
			var logger = new LoggerForTest();
			using (var connection = Db.NewAdminConnection())
			using (((ICurrentDbControl)connection).UseDatabase(Db.EdwDatabaseName))
			{
				// Create fake ETL script which just waits for ages and add it as the last custom table script to run
				var fakeIncLoad = "IF NOT EXISTS (SELECT NULL FROM biadmin.MasterState WHERE ParamName = 'HAS_HIT_WAIT') INSERT INTO biadmin.MasterState (ParamName, ParamValue) VALUES ('HAS_HIT_WAIT', '1');";
				connection.ExecuteNonQuery(@$"
					UPDATE biadmin.CustomTableConfiguration SET IncrementalLoadQuery = 'SELECT NULL FROM biadmin.MasterState' WHERE ModelSchemaName = 'Customs'
					;WITH cte AS (
						SELECT TOP 1 * FROM biadmin.CustomTableConfiguration WHERE ModelSchemaName = 'Customs' ORDER BY DependencyOrder DESC
					) UPDATE cte SET IncrementalLoadQuery = '{fakeIncLoad.Replace("'", "''")}';
				");

				// Run ETL in another thread with a new connection
				var tokenSource = new CancellationTokenSource();
				var task = Task.Run(() =>
				{
					// There is a check when using Db from other threads that enforces using this
					using (Db.DisposableActionForDbConnection())
					{
						TestRunCustoms();
					}
				}, tokenSource.Token);

				try
				{
					// Wait until ETL is finished doing stuff and is stalled on our fake script
					while (!connection.Exists("FROM biadmin.MasterState WITH(nolock) WHERE ParamName = 'HAS_HIT_WAIT'"))
					{
						if (task.IsFaulted)
						{
							Fail($"{task.Exception.Message}\r\n{task.Exception.StackTrace}");
						}
						Thread.Sleep(200);
					}
					// Check locks
					CombineAssertions(() =>
					{
						AssertEquals("sys.schemas is blocked", false, CheckIfBlocked("select * from sys.schemas;", connection));
						AssertEquals("sys.objects is blocked", false, CheckIfBlocked("select * from sys.objects;", connection));
						AssertEquals("sys.indexes is blocked", false, CheckIfBlocked("select * from sys.indexes;", connection));
						AssertEquals("sys.partitions is blocked", false, CheckIfBlocked("select * from sys.partitions", connection));
						AssertEquals("sys.dm_db_missing_index is blocked", false, CheckIfBlocked("select * from sys.dm_db_missing_index_group_stats", connection));
					});
				}
				finally
				{
					tokenSource.Cancel();
					task.Wait();
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.EDW })]
		public void TestPartitionTable()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				CreateTestTable(connection);
				RunPartitionTableStoredProcedure(connection);
				AssertPartitions(connection);
			}
		}

		void AssertPartitions(AdminConnection connection)
		{
			var minCreateDate = new DateTime(2000, 1, 1); // Example: your @MinCreateDate
			var today = DateTime.Today;
			var monthsBetween = (today.Year - minCreateDate.Year) * 12 + (today.Month - minCreateDate.Month);
			// In [usp_PartitionTable] we
				//	DECLARE @NumberOfMonths INT = DATEDIFF(month, @MinDate, GETDATE()) + 1 + @NumberOfFuturePeriods + 1
			// Hence...
			var expectedPartitions = monthsBetween + 8;

			var sqlText = @"
				 SELECT COUNT(*) 
				 FROM sys.partition_range_values prv
				 INNER JOIN sys.partition_functions pf ON pf.function_id = prv.function_id
				 WHERE pf.name = 'PF_TestPartitionTable'
			";

			var actualPartitions = connection.ExecuteScalar<int>(sqlText);
			AssertEquals("There should be the same number of partitions as there are months since 1 Jan 2000 + 6", expectedPartitions, actualPartitions);
		}

		void RunPartitionTableStoredProcedure(AdminConnection connection)
		{
			var sqlText = @"
				DECLARE @ResultCode INT,
						  @ErrorMessage NVARCHAR(MAX),
						  @ErrorCode INT;

				EXEC Transform.usp_PartitionTable 
					 @LoadType = 1,
					 @SchemaName = 'Transform',
					 @TableName = 'TestPartitionTable',
					 @PartitionColumnName = 'CreateDate',
					 @NumberOfFuturePeriods = 6,
					 @ResultCode = @ResultCode OUTPUT,
					 @ErrorMessage = @ErrorMessage OUTPUT,
					 @ErrorCode = @ErrorCode OUTPUT;
			";
			connection.ExecuteNonQuery(sqlText);
			
			Assert("Partition function PF_TestPartitionTable was not created.", connection.Exists("FROM sys.partition_functions WHERE name = 'PF_TestPartitionTable'"));
			Assert("Partition scheme PS_TestPartitionTable was not created.", connection.Exists("FROM sys.partition_schemes WHERE name = 'PS_TestPartitionTable'"));
		}

		void CreateTestTable(AdminConnection connection)
		{
			var sqlText = @"
					IF OBJECT_ID('Transform.TestPartitionTable', 'U') IS NOT NULL
						 DROP TABLE Transform.TestPartitionTable;

					CREATE TABLE Transform.TestPartitionTable
					(
						 Id INT IDENTITY(1,1) PRIMARY KEY,
						 CreateDate DATE NOT NULL
					);

					INSERT INTO Transform.TestPartitionTable (CreateDate)
					VALUES 
					('2000-01-01'),
					('2010-01-01'),
					('2015-01-01'),
					('2020-01-01');
				";
			connection.ExecuteNonQuery(sqlText);
		}

		bool CheckIfBlocked(string statement, DbConnection connection)
		{
			using (connection.TemporarySetLockTimeout(1000))
			{
				try
				{
					connection.ExecuteNonQuery(statement);
				}
				catch (SqlException ex) when (ex.Number == 1222)
				{
					return true;
				}
				return false;
			}
		}

		void DeleteEDWTablesNotInMainTableList(AdminConnection connection, string modelTableNames, string schema)
		{
			List<string> transformTableList = new List<string> { "TransformTableState", "TransformTableConfiguration" };
			List<string> edwTableConfigs = new List<string> { "CustomTableConfiguration", "CustomTableState", "ModelTableConfiguration", "ModelTableState" };

			foreach (string table in transformTableList)
			{
				connection.ExecuteNonQuery($@"
DELETE FROM [{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[{table}]
WHERE  ModelSchemaName + '.' + ModelTableName NOT IN ({modelTableNames})
");
			}

			foreach (string table in edwTableConfigs)
			{
				connection.ExecuteNonQuery($@"
DELETE FROM[{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[{table}]
WHERE ModelSchemaName != '{schema}'
");
			}
		}

		void AddEdwReferenceTables(string schema, List<BiAutomationConfigDataSet.EdwTableConfigRow> mainDbTableList)
		{
			foreach (var baseTable in EdwBaseTableList.Where(t => t.Schema.Equals(schema)))
			{
				foreach (var row in baseTable.GetEdwColumnConfigRows())
				{
					var parentTable = EdwBaseTableList.FirstOrDefault(t => t.Name.Equals(row.ParentTable));
					if (parentTable != null && !mainDbTableList.Contains(parentTable))
					{
						mainDbTableList.Add(parentTable);
					}
				}
			}
			foreach (var aggTable in EdwAggregateTableList.Where(t => t.Schema.Equals(schema)))
			{
				var columnRegex = new Regex($"\\[(.*?)\\].\\[(.*?)\\](?= as)", RegexOptions.IgnoreCase);
				foreach (System.Text.RegularExpressions.Match match in columnRegex.Matches(aggTable.Expression))
				{
					string refSchema = match.Value.Split('.')[0].Trim('[', ']');
					string refTable = match.Value.Split('.')[1].Trim('[', ']');
					var refEdwTable = EdwBaseTableList.FirstOrDefault(t => t.Name.Equals(refTable));
					if (refEdwTable != null && !mainDbTableList.Contains(refEdwTable))
					{
						mainDbTableList.Add(EdwBaseTableList.FirstOrDefault(t => t.Name.Equals(refTable)));
					}
				}
			}
		}

		void ETLRunTransformation(string schema)
		{
			using (var connection = Db.NewAdminConnection())
			{
				var testHelper = new EdwTsqlScriptTestHelper();
				var mainDbTableList = EdwBaseTableList.Where(t => t.Schema.Equals(schema)).ToList();
				AddEdwReferenceTables(schema, mainDbTableList);
				var mainDbStagingTableList = mainDbTableList.Select(t => (t.SourceSchema, t.StagingTable)).Distinct().ToList();
				string modelTableList = string.Join(", ", mainDbTableList.Select(t => string.Format("'{0}'", t.Schema + "." + t.Name)));
				testHelper.TruncateTables(connection);
				DeleteEDWTablesNotInMainTableList(connection, modelTableList, schema);
				DisableCdc(connection);
				testHelper.EnableCdcForMainDBTables(connection, mainDbStagingTableList);

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				scanner.ScanUntilNoTransactionsToProcessSafe();

				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);
				edwTsqlScriptRunner.Run();

				InsertRowsInMainDbTables(connection, initialLoad: true, mainDbStagingTableList);
				scanner.ScanUntilNoTransactionsToProcessSafe();

				edwTsqlScriptRunner.Run();
				AssertModelTables(connection, schema);
				CheckIndex(connection, schema);

				#endregion

				#region Incremental Load

				InsertRowsInMainDbTables(connection, initialLoad: false, mainDbStagingTableList);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();
				AssertModelTables(connection, schema);

				UpdateRowsInMainDbTables(connection, mainDbStagingTableList);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();
				AssertModelTables(connection, schema);

				DeleteRowsInMainDbTables(connection, mainDbStagingTableList);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();
				AssertModelTables(connection, schema);

				#endregion
			}
		}

		public void InsertRowsInMainDbTables(AdminConnection connection, bool initialLoad, IEnumerable<(string, string)> mainDbTableList)
		{
				foreach (var (schema, table) in mainDbTableList)
				{
					InsertRow(connection, initialLoad, schema, table);
				}
			}

		public void UpdateRowsInMainDbTables(AdminConnection connection, IEnumerable<(string, string)> mainDbTableList)
		{
			foreach (var (schema, table) in mainDbTableList)
			{
				UpdateRow(connection, schema, table);
			}
		}

		public void DeleteRowsInMainDbTables(AdminConnection connection, IEnumerable<(string, string)> mainDbTableList)
		{
			foreach (var (schema, table) in mainDbTableList)
			{
				var primaryKey = ColumnsList.FirstOrDefault(c => c.TableName == table && PrimaryKeys.Contains(c.ColumnName));

				if (primaryKey != null)
				{
					var query = string.Format(@"
ALTER TABLE {0}.{1}
NOCHECK CONSTRAINT ALL
ALTER TABLE {0}.{1}
DISABLE TRIGGER ALL
DELETE {0}.{1} WHERE {2} = @PrimaryKey",
						schema,
						table,
						primaryKey.ColumnName);

					using (var cmd = connection.Command(query))
					{
						cmd.AddParameter("@PrimaryKey", primaryKey.DataType, insertedId);
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestEnableEtlFlag()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var testHelper = new EdwTsqlScriptTestHelper();
				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				testHelper.TruncateTables(connection);
				DisableCdc(connection);
				testHelper.EnableCdcForMainDBTables(connection, new List<(string, string)> {
					("dbo", "GlbStaff"),		// Will begin with EnableEtl = 0
					("dbo", "OrgContact")	// Will begin with EnableEtl = 1
				});
				SetEnableEtlForAllTables(connection, enableEtl: 0);
				SetEnableEtl(connection, enableEtl: 1, qualifiedTableNames: ["dbo.OrgContact"]);

				// --- TEST CASE 1: Initial Load ---
				MakeChangesAndRunEtl(connection, scanner, edwTsqlScriptRunner);

				AssertDataWasStaged(connection, expectDataToBeStaged: false, "GlbStaff");
				AssertDataWasStaged(connection, expectDataToBeStaged: true, "OrgContact");
				AssertStagingTableState(connection, expectedState: "Loaded", qualifiedTableNames: "dbo.GlbStaff");
				AssertStagingTableState(connection, expectedState: "Loaded", qualifiedTableNames: "dbo.OrgContact");

				// --- TEST CASE 2: Initial Load on GlbStaff, Incremental Load on OrgContact ---
				SetEnableEtl(connection, enableEtl: 1, qualifiedTableNames: ["dbo.GlbStaff"]);
				MakeChangesAndRunEtl(connection, scanner, edwTsqlScriptRunner);

				AssertDataWasStaged(connection, expectDataToBeStaged: true, "GlbStaff");
				AssertDataWasStaged(connection, expectDataToBeStaged: true, "OrgContact");
				AssertStagingTableState(connection, expectedState: "Loaded", qualifiedTableNames: "dbo.GlbStaff");
				AssertStagingTableState(connection, expectedState: "Loaded", qualifiedTableNames: "dbo.OrgContact");

				// --- TEST CASE 3: Incremental Load with EnableEtl = 0 ---
				SetEnableEtl(connection, enableEtl: 0, qualifiedTableNames: ["dbo.GlbStaff", "dbo.OrgContact"]);
				MakeChangesAndRunEtl(connection, scanner, edwTsqlScriptRunner);

				AssertDataWasStaged(connection, expectDataToBeStaged: false, "GlbStaff");
				AssertDataWasStaged(connection, expectDataToBeStaged: false, "OrgContact");
				AssertStagingTableState(connection, expectedState: "Idle", qualifiedTableNames: "dbo.GlbStaff");
				AssertStagingTableState(connection, expectedState: "Idle", qualifiedTableNames: "dbo.OrgContact");

				// --- TEST CASE 4: Incremental Load with EnableEtl = 1 ---
				SetEnableEtlAndResetInitialLoad(connection, enableEtl: 1, qualifiedTableNames: ["dbo.GlbStaff", "dbo.OrgContact"]);
				MakeChangesAndRunEtl(connection, scanner, edwTsqlScriptRunner);

				AssertDataWasStaged(connection, expectDataToBeStaged: true, "GlbStaff");
				AssertDataWasStaged(connection, expectDataToBeStaged: true, "OrgContact");
				AssertStagingTableState(connection, expectedState: "Loaded", qualifiedTableNames: "dbo.GlbStaff");
				AssertStagingTableState(connection, expectedState: "Loaded", qualifiedTableNames: "dbo.OrgContact");
			}
		}

		void SetEnableEtlForAllTables(AdminConnection connection, int enableEtl)
		{
			var sqlText = $@"
				UPDATE [{Db.EdwDatabaseName}].[biadmin].[StagingTableState] 
				SET EnableEtl = {enableEtl}
			";
			connection.ExecuteNonQuery(sqlText);

			if (enableEtl == 1)
			{
				ResetMasterState(connection);
			}
		}

		void SetEnableEtl(AdminConnection connection, int enableEtl, string[] qualifiedTableNames)
		{
			var tableNamesString = string.Join("','", qualifiedTableNames);
			var sqlText = $@"
				UPDATE [{Db.EdwDatabaseName}].[biadmin].[StagingTableState] 
				SET EnableEtl = {enableEtl}
				WHERE SourceTableName IN ('{tableNamesString}')
			";
			connection.ExecuteNonQuery(sqlText);

			if (enableEtl == 1)
			{
				ResetMasterState(connection);
			}
		}

		void SetEnableEtlAndResetInitialLoad(AdminConnection connection, int enableEtl, string[] qualifiedTableNames)
		{
			var tableNamesString = string.Join("','", qualifiedTableNames);
			var sqlText = $@"
				UPDATE [{Db.EdwDatabaseName}].[biadmin].[StagingTableState] 
				SET EnableEtl = {enableEtl}, InitialLoadRequired = 1, CurrentState = 'New'
				WHERE SourceTableName IN ('{tableNamesString}')
			";

			connection.ExecuteNonQuery(sqlText);

			if (enableEtl == 1)
			{
				ResetMasterState(connection);
			}
		}

		void ResetMasterState(AdminConnection connection)
		{
			connection.ExecuteNonQuery($@"TRUNCATE TABLE [{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[MasterState]");
			connection.ExecuteNonQuery($"EXEC  [{Db.EdwDatabaseName}].[biadmin].usp_SetMasterStateParameter 'INITIAL_LOAD_REQUESTED', '0'");
		}

		void MakeChangesAndRunEtl(AdminConnection connection, CdcScannerForEdwTest scanner, EdwTsqlScriptRunner edwTsqlScriptRunner)
		{
			connection.ExecuteNonQuery($@"TRUNCATE TABLE [{Db.EdwDatabaseName}].[Staging].[GlbStaff]");
			connection.ExecuteNonQuery($@"TRUNCATE TABLE [{Db.EdwDatabaseName}].[Staging].[OrgContact]");

			connection.ExecuteNonQuery($@"
				UPDATE [dbo].[GlbStaff] 
				SET gs_isactive = ~gs_isactive, GS_SystemLastEditTimeUtc = GETUTCDATE(), GS_SystemLastEditUser = '~BP' 
				WHERE GS_PK = 'CC3C77A1-969E-4E03-B5B0-CFD20B163E4B'");
			connection.ExecuteNonQuery($@"
				Update [dbo].OrgContact
				SET oc_isactive = ~oc_isactive
				where OC_PK = 'CA198EF8-B83E-4C62-9F1E-9C317C427595'
			");

			scanner.ScanUntilNoTransactionsToProcessSafe();
			edwTsqlScriptRunner.Run();
		}

		void AssertDataWasStaged(DbConnection connection, bool expectDataToBeStaged, string tableName)
		{
			var result = Convert.ToInt32(connection.ExecuteScalar($"SELECT COUNT(*) FROM [{Db.EdwDatabaseName}].[Staging].[{tableName}]"));
			var message = expectDataToBeStaged ? $"{tableName} - Change should be staged when EnableEtl = 1" : $"{tableName} - Change should NOT be staged when EnableEtl = 0";
			var condition = expectDataToBeStaged ? (result > 0) : (result == 0);
			Assert(message, condition);
		}

		void AssertStagingTableState(AdminConnection connection, string expectedState, string qualifiedTableNames)
		{
			string state = (string)connection.ExecuteScalar($@"
					SELECT CurrentState 
					FROM [{Db.EdwDatabaseName}].[biadmin].[StagingTableState] 
					WHERE SourceTableName = '{qualifiedTableNames}'
				");
			AssertEquals(expectedState, state);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRunHandlesLinkedServerError()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var logger = new LoggerForTest();
				var creator = new LinkedServerCreator(connection, connection.ServerName, logger);

				try
				{
					LinkedServerCreatorTest.SetLocalServerDataAccessOption(connection, value: false);
					LinkedServerCreatorTest.AssertLocalServerDataAccessEnabled(connection, expected: false);

					var testHelper = new EdwTsqlScriptTestHelper();
					testHelper.TruncateTables(connection);
					DisableCdc(connection);
					EnableCdc(connection);

					var scanner = new CdcScannerForEdwTest();
					scanner.ScanUntilNoTransactionsToProcessSafe();

					var keyMock = new Mock<IProductRegistrationKey>();
					var productRegistrationMock = new Mock<IProductRegistration>();
					productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
					productRegistrationMock.SetupGet(r => r.Key).Returns(keyMock.Object);
					using (ObjectFactory.Substitute(productRegistrationMock.Object))
					{
						keyMock.Setup(k => k.HostedLocation).Returns("SYD");
						var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);
						edwTsqlScriptRunner.Run();
					}

					LinkedServerCreatorTest.AssertLocalServerDataAccessEnabled(connection, expected: true);
				}
				finally
				{
					creator.CreateLinkedServer();
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestUnprocessedPartitionDates()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					var testHelper = new EdwTsqlScriptTestHelper();
					testHelper.TruncateTables(connection);
					TruncateEDWTables(connection);
					DisableCdc(connection);
					EnableCdc(connection);
					CreateTestTableInEdw(connection, "TestTable");

					#region Initial Load

					var scanner = new CdcScannerForEdwTest();
					scanner.ScanUntilNoTransactionsToProcessSafe();

					var logger = new LoggerForTest();
					var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);
					edwTsqlScriptRunner.Run();

					var partitionDate0 = new DateTime(2017, 2, 20);
					InsertRowToTestTable(connection, "TestTable", 1, partitionDate0);
					scanner.ScanUntilNoTransactionsToProcessSafe();

					edwTsqlScriptRunner.Run();
					AssertUnprocessedPartitionDate(connection, "Initial Load", "Test", "BAS__TestTable", null);

					#endregion

					#region Incremental Load

					TruncateUnprocessedPartitionDates(connection);
					AssertUnprocessedPartitionDate(connection, "Initial Load", "Test", "BAS__TestTable", null, false);

					var partitionDate1 = new DateTime(2017, 2, 21);
					InsertRowToTestTable(connection, "TestTable", 2, partitionDate1);
					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();
					AssertUnprocessedPartitionDate(connection, "Partition Date 1", "Test", "BAS__TestTable", partitionDate1);

					InsertRowToTestTable(connection, "TestTable", 3, partitionDate1);
					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();
					AssertUnprocessedPartitionDate(connection, "Partition Date 1 Repeated", "Test", "BAS__TestTable", partitionDate1);

					var partitionDate2 = new DateTime(2017, 2, 22);
					InsertRowToTestTable(connection, "TestTable", 4, partitionDate2);
					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();
					AssertUnprocessedPartitionDate(connection, "Partition Date 1", "Test", "BAS__TestTable", partitionDate1);
					AssertUnprocessedPartitionDate(connection, "Partition Date 2", "Test", "BAS__TestTable", partitionDate2);

					InsertRowToTestTable(connection, "TestTable", 5, null);
					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();
					AssertUnprocessedPartitionDate(connection, "NULL Partition Date", "Test", "BAS__TestTable", new DateTime(1900, 1, 1));
					AssertUnprocessedPartitionDate(connection, "Partition Date 1", "Test", "BAS__TestTable", partitionDate1);
					AssertUnprocessedPartitionDate(connection, "Partition Date 2", "Test", "BAS__TestTable", partitionDate2);

					#endregion

					#region Reinitiate Initial Load

					InitiateInitialLoad(connection);

					edwTsqlScriptRunner.Run();

					var partitionDate3 = new DateTime(2017, 2, 22);
					InsertRowToTestTable(connection, "TestTable", 6, partitionDate3);
					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					AssertUnprocessedPartitionDate(connection, "Initial Load", "Test", "BAS__TestTable", null);
					AssertUnprocessedPartitionDate(connection, "NULL Partition Date", "Test", "BAS__TestTable", new DateTime(1900, 1, 1), false);
					AssertUnprocessedPartitionDate(connection, "Partition Date 1", "Test", "BAS__TestTable", partitionDate1, false);
					AssertUnprocessedPartitionDate(connection, "Partition Date 2", "Test", "BAS__TestTable", partitionDate2, false);
					AssertUnprocessedPartitionDate(connection, "Partition Date 3", "Test", "BAS__TestTable", partitionDate3, false);

					#endregion
				}
				catch (SqlException ex) when (ex.Message.Contains("Invalid object name 'cdc.lsn_time_mapping'."))
				{
					var errorMessages = new StringBuilder();
					for (int i = 0; i < ex.Errors.Count; i++)
					{
						errorMessages.Append("Index #" + i + "\n" +
							"Message: " + ex.Errors[i].Message + "\n" +
							"Error Number: " + ex.Errors[i].Number + "\n" +
							"LineNumber: " + ex.Errors[i].LineNumber + "\n" +
							"Source: " + ex.Errors[i].Source + "\n" +
							"Procedure: " + ex.Errors[i].Procedure + "\n");
					}
					Fail(errorMessages.ToString() + "\n\nStack Trace: " + ex.StackTrace);
				}
			}
		}

		#endregion

		#region Logs

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestEtlDates()
		{
			using (var connection = Db.NewAdminConnection())
			{
				configData = new BiConfigurationDataForTest();

				DisableCdc(connection);
				EnableCdc(connection);
				TruncateEDWTables(connection);
				CreateTestTableInEdw_New(connection, "TestTable", configData);

				#region Initial Load

				var initialMaxLsnTime = GetParamValue(connection, "MAX_LSN_TIME_UTC");
				var initialLastTransformRunDate = GetParamValue(connection, "LAST_DATE_TRANSFORM_RUN_UTC");
				var initialLastLsnTransformedDateTime = GetParamValue(connection, "LAST_LSN_TRANSFORMED_UTC");

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();
				InsertRowToTestTable(connection, "TestTable", 1, null);
				Thread.Sleep(100);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				var newMaxLsnTime = GetParamValue(connection, "MAX_LSN_TIME_UTC");
				var newLastTransformRunDate = GetParamValue(connection, "LAST_DATE_TRANSFORM_RUN_UTC");
				var newLastLsnTransformedDateTime = GetParamValue(connection, "LAST_LSN_TRANSFORMED_UTC");

				CombineAssertions(() =>
				{
					AssertEquals("EDW ETL should be in incremental master load stage.", EdwInitialMasterLoadResult.IncrementalMasterLoad, edwTsqlScriptRunner.GetInitialLoadStage());

					Assert("MAX_LSN_TIME_UTC should be updated.", newMaxLsnTime > initialMaxLsnTime);
					Assert("LAST_DATE_TRANSFORM_RUN_UTC should be updated.", newLastTransformRunDate > initialLastTransformRunDate);
					Assert("LAST_LSN_TRANSFORMED_UTC should be updated.", newLastLsnTransformedDateTime > initialLastLsnTransformedDateTime);

					AssertEquals("LAST_LSN_TRANSFORMED_UTC should be equal to MAX_LSN_DATE_UTC.", newMaxLsnTime, newLastLsnTransformedDateTime);
					Assert("LAST_DATE_TRANSFORM_RUN_UTC should be greater than LAST_LSN_TRANSFORMED_UTC.", newLastTransformRunDate > newLastLsnTransformedDateTime);
				});

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestEtlDates_StagingFailed()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				EnableCdc(connection);
				CreateTestTableInEdw(connection, "TestTable");

				#region Initial Load

				var initialMaxLsnDate = GetParamValue(connection, "MAX_LSN_DATE_UTC");

				var scanner = new CdcScannerForEdwTest();
				scanner.ScanUntilNoTransactionsToProcessSafe();

				SetIncorrectTableConfiguration(connection);

				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);
				edwTsqlScriptRunner.Run();

				InsertRowToTestTable(connection, "TestTable", 1, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				var newMaxLsnDate = GetParamValue(connection, "MAX_LSN_DATE_UTC");
				var newLastTransformRunDate = GetParamValue(connection, "LAST_DATE_TRANSFORM_RUN_UTC");
				var newLastLsnTransformedDateTime = GetParamValue(connection, "LAST_LSN_TRANSFORMED_UTC");

				CombineAssertions(string.Join("\r\n", logger.LogEntries), () =>
				{
					AssertEquals("EDW ETL should be in incremental load stage.", EdwInitialMasterLoadResult.IncrementalLoad, edwTsqlScriptRunner.GetInitialLoadStage());
					AssertEquals("MAX_LSN_DATE_UTC should not be updated if Staging fails.", initialMaxLsnDate, newMaxLsnDate);
					AssertEquals("LAST_DATE_TRANSFORM_RUN_UTC should be NULL.", DateTime.MinValue, newLastTransformRunDate);
					AssertEquals("LAST_LSN_TRANSFORMED_UTC should be NULL.", DateTime.MinValue, newLastLsnTransformedDateTime);
				});

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestEtlInitialSummaryLog()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);
				TruncateEDWTables(connection);
				CreateTestTableInEdw(connection, "TestTable");

				#region Initial Load
				var scanner = new CdcScannerForEdwTest();
				scanner.ScanUntilNoTransactionsToProcessSafe();

				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				edwTsqlScriptRunner.Run();

				InsertRowToTestTable(connection, "TestTable", 1, null);

				scanner.ScanUntilNoTransactionsToProcessSafe();

				edwTsqlScriptRunner.Run();

				#endregion

				#region Restart Initial Load

				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					var sqlText = string.Format(CultureInfo.InvariantCulture,
	@"UPDATE [{0}].[StagingTableState]
SET InitialLoadRequired = 1, CurrentState = 'New'
WHERE SourceTableName = 'Test.TestTable'", BiConstants.BiAdminSchemaName);
					connection.ExecuteNonQuery(sqlText);
				}

				edwTsqlScriptRunner.Run();
				InsertRowToTestTable(connection, "TestTable", 1, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				var expectedTotalCount = Convert.ToInt32(connection.ExecuteScalar("SELECT COUNT(*) FROM Test.TestTable"));

				Assert(string.Format(CultureInfo.InvariantCulture, "Expected log: '[Test].[BAS__TestTable] TransformId: 1 Insert({0})'", expectedTotalCount),
					logger.LogEntries.Any(l => l.Contains(string.Format(CultureInfo.InvariantCulture, "[Test].[BAS__TestTable] TransformId: 1 Insert({0})", expectedTotalCount))));

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestEtlIncrementalSummaryLog()
		{
			using (var connection = Db.NewAdminConnection())
			{
				configData = new BiConfigurationDataForTest();

				try
				{
					EnsureCdcIsEnabled(connection, Db.DatabaseName);
					TruncateEDWTables(connection);
					CreateTestTableInEdw_New(connection, "TestTable", configData);
					CreateTestTableInEdw_New(connection, "TestTable2", configData);

					#region Initial Load

					var scanner = new CdcScannerForEdwTest();
					scanner.ScanUntilNoTransactionsToProcessSafe();

					var logger = new LoggerForTest();
					var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);
					edwTsqlScriptRunner.Run();

					InsertRowToTestTable(connection, "TestTable2", 1, null);

					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					#endregion

					#region Restart Initial Load

					using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
					{
						var sqlText = string.Format(CultureInfo.InvariantCulture,
	@"UPDATE [{0}].[StagingTableState]
SET InitialLoadRequired = 1, CurrentState = 'New'", BiConstants.BiAdminSchemaName);
						connection.ExecuteNonQuery(sqlText);
					}

					edwTsqlScriptRunner.Run();
					InsertRowToTestTable(connection, "TestTable", 1, null);
					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Actual Logs:\r\n{0}", string.Join("\r\n", logger.LogEntries)), () =>
					{
						Assert("[Test].[BAS__TestTable] : (deletes: 0, inserts: 1, net: 1)",
							!logger.LogEntries.Any(l => l.Contains("[Test].[BAS__TestTable] : (deletes: 0, inserts: 1, net: 1)")));
					});

					#endregion

					#region Incremental Load
					InsertRowToTestTable(connection, "TestTable2", 1, null);

					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Actual Logs:\r\n{0}", string.Join("\r\n", logger.LogEntries)), () =>
					{
						var scriptLogs = new string[]
						{
							"Updating staging table state.", // Staging Initial load
							"Starting initial load to staging tables.",
							"First custom table transform starting.", // Master Transform
							"Base table transform starting.",
							"Aggregate table transform starting.",
							"Second custom table transform starting.",
							"Cleanup staging starting.",
							"Setting up for incremental load.", // Staging incremental load
							"Starting incremental load to staging tables.",
						};

						foreach (var expectedLog in scriptLogs)
						{
							AssertCollectionContains($"Expected log: '{expectedLog}'", expectedLog, logger.LogEntries);
						}

						Assert("Expected log: 'Total number of processed tables with incremental changes: 1'",
							logger.LogEntries.Any(l => l.Contains("Total number of processed tables with incremental changes: 1")));
						Assert("Expected log: '[Test].[BAS__TestTable2] TransformId: 1 (deletes: 0, inserts: 1, net: 1)'",
							logger.LogEntries.Any(l => l.Contains("[Test].[BAS__TestTable2] TransformId: 1 (deletes: 0, inserts: 1, net: 1)")));
					});

					#endregion
				}
				catch (SqlException ex) when (ex.Message.Contains("Invalid object name 'cdc.lsn_time_mapping'."))
				{
					var errorMessages = new StringBuilder();
					for (int i = 0; i < ex.Errors.Count; i++)
					{
						errorMessages.Append("Index #" + i + "\n" +
							"Message: " + ex.Errors[i].Message + "\n" +
							"Error Number: " + ex.Errors[i].Number + "\n" +
							"LineNumber: " + ex.Errors[i].LineNumber + "\n" +
							"Source: " + ex.Errors[i].Source + "\n" +
							"Procedure: " + ex.Errors[i].Procedure + "\n");
					}
					Fail(errorMessages.ToString() + "\n\nStack Trace: " + ex.StackTrace);
				}
				finally
				{
					configData = null;
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestEtlLogs_StagingFailed()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInEdw(connection, "TestTable");

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				scanner.ScanUntilNoTransactionsToProcessSafe();

				SetIncorrectTableConfiguration(connection);

				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);
				edwTsqlScriptRunner.Run();

				InsertRowToTestTable(connection, "TestTable", 1, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(string.Join("\r\n", logger.LogEntries), () =>
				{
					Assert("Missing log for '[Staging].[TestTable] - Invalid column name 'PK'.'.", logger.LogEntries.Any(l => l.Contains(@"[Staging].[TestTable] - Invalid column name 'PK'.")));
				});

				#endregion
			}
		}

		#endregion

		#region EDW Schema

		public void TestBiTempSchemaTables()
		{
			using (var biConnection = Db.NewAdminConnection(EdwDbName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT null FROM sys.schemas WHERE name = '{0}') SELECT 1 ELSE SELECT 0", BiConstants.BiTempSchemaName);
				Assert(string.Format(CultureInfo.InvariantCulture, "Schema [{0}] does not exist in EDW database.", BiConstants.BiTempSchemaName), Convert.ToBoolean(biConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));

				sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = '{0}'", BiConstants.BiTempSchemaName);
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "Schema [{0}] contains tables in EDW database.", BiConstants.BiTempSchemaName), 0, Convert.ToInt32(biConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		public void TestTestSchemaTables()
		{
			using (var biConnection = Db.NewAdminConnection(EdwDbName))
			{
				var sqlText = "IF EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test') SELECT 1 ELSE SELECT 0";
				Assert("Schema [Test] should not exist in EDW database.", !Convert.ToBoolean(biConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		public void TestTransformedRowPkValueIsNullable()
		{
			using (var biConnection = Db.NewAdminConnection(EdwDbName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"select is_nullable from sys.columns where object_id = object_id('{0}.TransformedRow') and name = 'PKValue'", BiConstants.BiAdminSchemaName);
				Assert("[TransformedRow].[PKName] should be set as NULLABLE.", Convert.ToBoolean(biConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		public void TestTransformedRowRefValueColumnNames()
		{
			using (var biConnection = Db.NewAdminConnection(EdwDbName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
@"select name
from sys.columns
where object_id = object_id('{0}.TransformedRow') and name like 'RefValue%'", BiConstants.BiAdminSchemaName);
				var actualColumnNames = DataUtils.GetListOfValuesFromQuery(biConnection, sqlText);

				AssertContainsExactElementsInAnyOrder("TransformedRow RefValue column names", new[] { "RefValue1", "RefValue2", "RefValue3", "RefValue4", "RefValue5" }, actualColumnNames);
			}
		}

		public void TestTransformedRowRefValueDataTypes()
		{
			using (var biConnection = Db.NewAdminConnection(EdwDbName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
@"select c.name
from sys.columns c
inner join sys.types t
	on c.system_type_id = t.system_type_id
where c.object_id = object_id('{0}.TransformedRow') and c.name like 'RefValue%' and t.name <> 'sql_variant'", BiConstants.BiAdminSchemaName);
				var invalidColumnDataTypes = DataUtils.GetListOfValuesFromQuery(biConnection, sqlText);

				Assert("TransformedRow RefValues should have data type of sql_variant.", !invalidColumnDataTypes.Any());

				sqlText = string.Format(CultureInfo.InvariantCulture, @"select name from sys.columns where object_id = object_id('{0}.TransformedRow') and name like 'RefValue%' and is_nullable = 0", BiConstants.BiAdminSchemaName);
				var invalidNullableValues = DataUtils.GetListOfValuesFromQuery(biConnection, sqlText);

				Assert("TransformedRow RefValues should be set as NULLABLE.", !invalidNullableValues.Any());
			}
		}

		#endregion

		#region Key values

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestBaseTableDoesNotReuseKeys()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInEdw(connection, "TestTable");

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();
				InsertRowToTestTable(connection, "TestTable", 1, null);
				InsertRowToTestTable(connection, "TestTable", 2, null);
				InsertRowToTestTable(connection, "TestTable", 3, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Actual logs:\r\n{0}", string.Join("\r\n", logger.LogEntries)), () =>
				{
					AssertTestTableKeys(connection, "BAS__TestTable", 1);
					AssertTestTableKeys(connection, "BAS__TestTable", 2);
					AssertTestTableKeys(connection, "BAS__TestTable", 3);
				});

				#endregion

				#region Incremental Load

				DeleteRowTestTable(connection, "TestTable", 3);
				InsertRowToTestTable(connection, "TestTable", 4, null);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Actual logs:\r\n{0}", string.Join("\r\n", logger.LogEntries)), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 3);
					AssertTestTableKeys(connection, "BAS__TestTable", 4);
				});

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestAggregateTableDoesNotReuseKeys()
		{
			configData = new BiConfigurationDataForTest();

			try
			{
				using (var connection = Db.NewAdminConnection())
				{
					EnsureCdcIsEnabled(connection, Db.DatabaseName);

					ClearConfiguration(connection);

					CreateTestTableInEdw_New(connection, "TestTable", configData);
					CreateAggregateTestTable_New(connection, "BAS__TestTable", "AGG__AggTestTable", configData);

					#region Initial Load

					var scanner = new CdcScannerForEdwTest();
					var logger = new LoggerForTest();

					var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();
					InsertRowToTestTable(connection, "TestTable", 1, null);
					InsertRowToTestTable(connection, "TestTable", 2, null);
					InsertRowToTestTable(connection, "TestTable", 3, null);
					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Actual logs:\r\n{0}", string.Join("\r\n", logger.LogEntries)), () =>
					{
						AssertAggregateTestTableKeys(connection, "AGG__AggTestTable", 1);
						AssertAggregateTestTableKeys(connection, "AGG__AggTestTable", 2);
						AssertAggregateTestTableKeys(connection, "AGG__AggTestTable", 3);
					});

					#endregion

					#region Incremental Load

					DeleteRowTestTable(connection, "TestTable", 3);
					InsertRowToTestTable(connection, "TestTable", 4, null);

					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Actual logs:\r\n{0}", string.Join("\r\n", logger.LogEntries)), () =>
					{
						AssertTestTableCount(connection, "AGG__AggTestTable", 3);
						AssertAggregateTestTableKeys(connection, "AGG__AggTestTable", 4);
					});

					#endregion
				}
			}
			finally
			{
				configData = null;
			}
		}

		[UseSnapshotProtection]
		public void TestHandleDuplicateKeyOnSelfHosted()
		{
			using (var connection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(connection, Db.Connection.CloseConnection, Db.EdwDatabaseName, Db.EdwDatabaseName + "-SS"))
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				connection.ExecuteNonQuery($@"
UPDATE {Db.EdwDatabaseName}.{BiConstants.BiAdminSchemaName}.TransformTableState
SET SqlErrorMessage = 'Cannot insert duplicate key in object dbo.#Keys'
WHERE ModelSchemaName = 'InternationalLogistics' AND  ModelTableName = 'BAS__Shipment'

IF @@ROWCOUNT = 0
INSERT INTO {Db.EdwDatabaseName}.{BiConstants.BiAdminSchemaName}.TransformTableState
(ModelSchemaName, ModelTableName, SqlErrorMessage)
VALUES
('InternationalLogistics', 'BAS__Shipment', 'Cannot insert duplicate key in object dbo.#Keys')
");

				var etlRunner1 = new EdwTsqlScriptRunnerForDuplicateKeyTest(connection, new LoggerForTest());
				AssertExceptionThrown<EtlExecutionException>(etlRunner1.TransformMasterLoadExposed);

				EnvProxy.SetHostedLocationForTest("NCW");
				connection.ExecuteNonQuery($@"
UPDATE {Db.EdwDatabaseName}.{BiConstants.BiAdminSchemaName}.TransformTableState
SET SqlErrorMessage = 'Cannot insert duplicate key in object dbo.#Keys'
WHERE ModelSchemaName = 'InternationalLogistics' AND  ModelTableName = 'BAS__Shipment'

IF @@ROWCOUNT = 0
INSERT INTO {Db.EdwDatabaseName}.{BiConstants.BiAdminSchemaName}.TransformTableState
(ModelSchemaName, ModelTableName, SqlErrorMessage)
VALUES
('InternationalLogistics', 'BAS__Shipment', 'Cannot insert duplicate key in object dbo.#Keys')
");

				var etlRunner2 = new EdwTsqlScriptRunnerForDuplicateKeyTest(connection, new LoggerForTest());
				AssertExceptionThrown<HostedServiceException>(etlRunner2.TransformMasterLoadExposed);
			}
		}

		#endregion

		#region Parent and Child Relation

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestInitialLoadForParentEdwTables()
		{
			using (var connection = Db.NewAdminConnection())
			{
				configData = new BiConfigurationDataForTest();

				try
				{
					EnsureCdcIsEnabled(connection, Db.DatabaseName);
					ClearConfiguration(connection);
					CreateTestParentAndChildTable_New(connection, configData);

					#region Initial Load

					var scanner = new CdcScannerForEdwTest();
					var logger = new LoggerForTest();

					var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					Guid pk1 = Guid.NewGuid();
					Guid parentPk1 = Guid.NewGuid();
					InsertRowToTestTable(connection, pk1, 1);
					InsertRowToTestTable(connection, parentPk1, 2);

					Guid childPk1 = Guid.NewGuid();
					InsertRowToTestChildTable(connection, childPk1, parentPk1);

					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					CombineAssertions(() =>
					{
						AssertTestTableValues(connection, pk1, 1);
						AssertTestTableValues(connection, parentPk1, 2);
						AssertTestChildTableValues(connection, childPk1, parentPk1);
					});

					DeleteRowTestTable(connection, "TestTable", 1);
					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					CombineAssertions(() =>
					{
						AssertTestTableValues(connection, parentPk1, 2);
						AssertTestChildTableValues(connection, childPk1, parentPk1);
					});

					TriggerInitialLoadForTestTable(connection);
					edwTsqlScriptRunner.Run();

					Guid pk3 = Guid.NewGuid();
					InsertRowToTestTable(connection, pk3, 3);

					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					CombineAssertions("Partial initial load should re-use key for parent table.", () =>
					{
						AssertTestTableValues(connection, parentPk1, 2);
						AssertTestTableValues(connection, pk3, 3);
						AssertTestChildTableValues(connection, childPk1, parentPk1);
					});

					#endregion
				}
				finally
				{
					configData = null;
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestFullInitialLoadForParentEdwTables()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestParentAndChildTable(connection);

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				Guid pk1 = Guid.NewGuid();
				Guid parentPk1 = Guid.NewGuid();
				InsertRowToTestTable(connection, pk1, 1);
				InsertRowToTestTable(connection, parentPk1, 2);

				Guid childPk1 = Guid.NewGuid();
				InsertRowToTestChildTable(connection, childPk1, parentPk1);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertTestTableValues(connection, pk1, 1);
					AssertTestTableValues(connection, parentPk1, 2);
					AssertTestChildTableValues(connection, childPk1, parentPk1);
				});

				DeleteRowTestTable(connection, "TestTable", 1);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertTestTableValues(connection, parentPk1, 2);
					AssertTestChildTableValues(connection, childPk1, parentPk1);
				});

				TriggerInitialLoad(connection);
				edwTsqlScriptRunner.Run();

				Guid pk3 = Guid.NewGuid();
				InsertRowToTestTable(connection, pk3, 3);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions("Full initial load should reset key for parent table.", () =>
				{
					AssertTestTableValues(connection, parentPk1, 2);
					AssertTestTableValues(connection, pk3, 3);
					AssertTestChildTableValues(connection, childPk1, parentPk1);
				});

				#endregion
			}
		}

		#endregion

		#region Multiple Transforms

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestEtlForMultipleTransformEdwTable_SameSourceSameTarget()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInEdw(connection, "TestTable");
				CreateTransformTestTable(connection, "TestTable", "BAS__TestTable");

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();
				InsertRowToTestTable(connection, "TestTable", 1, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertTestTableValues(connection, "BAS__TestTable", 1, 1);
					AssertTestTableValues(connection, "BAS__TestTable", 2, 1);
				});

				#endregion

				#region Incremental Load

				UpdateRowInTestTable(connection, "TestTable", 1, 2);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertTestTableValues(connection, "BAS__TestTable", 1, 2);
					AssertTestTableValues(connection, "BAS__TestTable", 2, 2);
				});

				InsertRowToTestTable(connection, "TestTable", 3, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertTestTableValues(connection, "BAS__TestTable", 1, 2);
					AssertTestTableValues(connection, "BAS__TestTable", 2, 2);
					AssertTestTableValues(connection, "BAS__TestTable", 3, 3);
					AssertTestTableValues(connection, "BAS__TestTable", 4, 3);
				});

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestEtlForMultipleTransformEdwTable_DifferentSourcesSameTarget()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInEdw(connection, "TestTable");
				CreateTestTableInMainDb(connection, "TestTable2");
				CreateTransformTestTable(connection, "TestTable2", "BAS__TestTable");

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();
				InsertRowToTestTable(connection, "TestTable", 1, null);
				InsertRowToTestTable(connection, "TestTable2", 2, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertTestTableValues(connection, "BAS__TestTable", 1, 1);
					AssertTestTableValues(connection, "BAS__TestTable", 2, 2);
				});

				#endregion

				#region Incremental Load

				UpdateRowInTestTable(connection, "TestTable", 1, 3);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertTestTableValues(connection, "BAS__TestTable", 1, 3);
					AssertTestTableValues(connection, "BAS__TestTable", 2, 2);
				});

				InsertRowToTestTable(connection, "TestTable2", 4, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertTestTableValues(connection, "BAS__TestTable", 1, 3);
					AssertTestTableValues(connection, "BAS__TestTable", 2, 2);
					AssertTestTableValues(connection, "BAS__TestTable", 3, 4);
				});

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestEtlForMultipleTransformEdwTable_SameSourceDifferentTargets()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInEdw(connection, "TestTable");
				CreateTransformTestTable(connection, "TestTable", "BAS__TestTable2");

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();
				InsertRowToTestTable(connection, "TestTable", 1, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertTestTableValues(connection, "BAS__TestTable", 1, 1);
					AssertTestTableValues(connection, "BAS__TestTable2", 1, 1);
				});

				#endregion

				#region Incremental Load

				UpdateRowInTestTable(connection, "TestTable", 1, 2);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertTestTableValues(connection, "BAS__TestTable", 1, 2);
					AssertTestTableValues(connection, "BAS__TestTable2", 1, 2);
				});

				InsertRowToTestTable(connection, "TestTable", 3, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertTestTableValues(connection, "BAS__TestTable", 1, 2);
					AssertTestTableValues(connection, "BAS__TestTable2", 1, 2);
					AssertTestTableValues(connection, "BAS__TestTable", 2, 3);
					AssertTestTableValues(connection, "BAS__TestTable2", 2, 3);
				});

				#endregion
			}
		}

		#endregion

		#region Self Referenced Tables

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestSelfReferencedTable_SingleTransform_NoChildTables()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateSelfReferencedTestTableInMainDb(connection, "TestTable");
				CreateSelfReferencedTransformTestTable(connection, "TestTable", "BAS__TestTable", hasChildTables: false);

				#region Initial Load

				var parentPk1 = Guid.NewGuid();
				var childPk1 = Guid.NewGuid();

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				InsertRowToSelfReferencedTestTable(connection, "TestTable", parentPk1, null);
				InsertRowToSelfReferencedTestTable(connection, "TestTable", childPk1, parentPk1);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 2);
					AssertSelfReferencedTestTableValues(connection, parentPk1, null);
					AssertSelfReferencedTestTableValues(connection, childPk1, parentPk1);
				});

				#endregion

				#region Incremental Load

				var pk1 = Guid.NewGuid();
				var pk2 = Guid.NewGuid();

				InsertRowToSelfReferencedTestTable(connection, "TestTable", pk1, null);
				InsertRowToSelfReferencedTestTable(connection, "TestTable", pk2, pk1);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 4);
					AssertSelfReferencedTestTableValues(connection, parentPk1, null);
					AssertSelfReferencedTestTableValues(connection, childPk1, parentPk1);
					AssertSelfReferencedTestTableValues(connection, pk1, null);
					AssertSelfReferencedTestTableValues(connection, pk2, pk1);
				});

				var pk3 = Guid.NewGuid();
				InsertRowToSelfReferencedTestTable(connection, "TestTable", pk3, pk2);
				UpdateRowInSelfReferencedTestTable(connection, "TestTable", pk2, null);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 5);
					AssertSelfReferencedTestTableValues(connection, parentPk1, null);
					AssertSelfReferencedTestTableValues(connection, childPk1, parentPk1);
					AssertSelfReferencedTestTableValues(connection, pk1, null);
					AssertSelfReferencedTestTableValues(connection, pk2, null);
					AssertSelfReferencedTestTableValues(connection, pk3, pk2);
				});

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestSelfReferencedTable_SingleTransform_HasChildTables()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateSelfReferencedTestTableInMainDb(connection, "TestTable");
				CreateSelfReferencedTransformTestTable(connection, "TestTable", "BAS__TestTable", hasChildTables: true);

				#region Initial Load

				var parentPk1 = Guid.NewGuid();
				var childPk1 = Guid.NewGuid();
				var childPk2 = Guid.NewGuid();

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				InsertRowToSelfReferencedTestTable(connection, "TestTable", parentPk1, null);
				InsertRowToSelfReferencedTestTable(connection, "TestTable", childPk1, parentPk1);
				InsertRowToSelfReferencedTestTable(connection, "TestTable", childPk2, parentPk1);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 3);
					AssertSelfReferencedTestTableValues(connection, parentPk1, null);
					AssertSelfReferencedTestTableValues(connection, childPk1, parentPk1);
					AssertSelfReferencedTestTableValues(connection, childPk2, parentPk1);
				});

				#endregion

				#region Incremental Load

				var childPk3 = Guid.NewGuid();
				var childPk4 = Guid.NewGuid();

				InsertRowToSelfReferencedTestTable(connection, "TestTable", childPk3, parentPk1);
				InsertRowToSelfReferencedTestTable(connection, "TestTable", childPk4, parentPk1);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 5);
					AssertSelfReferencedTestTableValues(connection, parentPk1, null);
					AssertSelfReferencedTestTableValues(connection, childPk1, parentPk1);
					AssertSelfReferencedTestTableValues(connection, childPk2, parentPk1);
					AssertSelfReferencedTestTableValues(connection, childPk3, parentPk1);
					AssertSelfReferencedTestTableValues(connection, childPk4, parentPk1);
				});

				var pk1 = Guid.NewGuid();
				var pk2 = Guid.NewGuid();

				InsertRowToSelfReferencedTestTable(connection, "TestTable", pk1, null);
				InsertRowToSelfReferencedTestTable(connection, "TestTable", pk2, pk1);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 7);
					AssertSelfReferencedTestTableValues(connection, parentPk1, null);
					AssertSelfReferencedTestTableValues(connection, childPk1, parentPk1);
					AssertSelfReferencedTestTableValues(connection, childPk2, parentPk1);
					AssertSelfReferencedTestTableValues(connection, childPk3, parentPk1);
					AssertSelfReferencedTestTableValues(connection, childPk4, parentPk1);
					AssertSelfReferencedTestTableValues(connection, pk1, null);
					AssertSelfReferencedTestTableValues(connection, pk2, pk1);
				});

				var pk3 = Guid.NewGuid();
				InsertRowToSelfReferencedTestTable(connection, "TestTable", pk3, pk2);
				UpdateRowInSelfReferencedTestTable(connection, "TestTable", pk2, null);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 8);
					AssertSelfReferencedTestTableValues(connection, parentPk1, null);
					AssertSelfReferencedTestTableValues(connection, childPk1, parentPk1);
					AssertSelfReferencedTestTableValues(connection, childPk2, parentPk1);
					AssertSelfReferencedTestTableValues(connection, childPk3, parentPk1);
					AssertSelfReferencedTestTableValues(connection, childPk4, parentPk1);
					AssertSelfReferencedTestTableValues(connection, pk1, null);
					AssertSelfReferencedTestTableValues(connection, pk2, null);
					AssertSelfReferencedTestTableValues(connection, pk3, pk2);
				});

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestSelfReferencedTable_MultipleTransform_NoChildTables()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateSelfReferencedTestTableInMainDb(connection, "TestTable");
				CreateSelfReferencedMultiTransformTestTable(connection, "TestTable", "BAS__TestTable", hasChildTables: false);

				#region Initial Load

				var dummyPk = Guid.NewGuid();

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				InsertRowToSelfReferencedTestTable(connection, "TestTable", dummyPk, null);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 3);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 3);
				});

				#endregion

				#region Incremental Load

				var pk1 = Guid.NewGuid();
				var pk2 = Guid.NewGuid();
				var pk3 = Guid.NewGuid();

				InsertRowToSelfReferencedTestTable(connection, "TestTable", pk1, null, null, null);
				InsertRowToSelfReferencedTestTable(connection, "TestTable", pk2, null, null, null);
				InsertRowToSelfReferencedTestTable(connection, "TestTable", pk3, pk1, pk2, null);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 12);

					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 3);

					AssertSelfReferencedTestTableValues(connection, pk1, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, pk1, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, pk1, null, transformId: 3);

					AssertSelfReferencedTestTableValues(connection, pk2, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, pk2, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, pk2, null, transformId: 3);

					AssertSelfReferencedTestTableValues(connection, pk3, pk1, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, pk3, pk2, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, pk3, null, transformId: 3);
				});

				UpdateRowInSelfReferencedTestTable(connection, "TestTable", pk3, pk2, pk2, pk1);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 12);

					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 3);

					AssertSelfReferencedTestTableValues(connection, pk1, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, pk1, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, pk1, null, transformId: 3);

					AssertSelfReferencedTestTableValues(connection, pk2, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, pk2, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, pk2, null, transformId: 3);

					AssertSelfReferencedTestTableValues(connection, pk3, pk2, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, pk3, pk2, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, pk3, pk1, transformId: 3);
				});

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestSelfReferencedTable_MultipleTransform_HasChildTables()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateSelfReferencedTestTableInMainDb(connection, "TestTable");
				CreateSelfReferencedMultiTransformTestTable(connection, "TestTable", "BAS__TestTable", hasChildTables: true);

				#region Initial Load

				var dummyPk = Guid.NewGuid();

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				InsertRowToSelfReferencedTestTable(connection, "TestTable", dummyPk, null);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 3);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 3);
				});

				#endregion

				#region Incremental Load

				var pk1 = Guid.NewGuid();
				var pk2 = Guid.NewGuid();
				var pk3 = Guid.NewGuid();

				InsertRowToSelfReferencedTestTable(connection, "TestTable", pk1, null, null, null);
				InsertRowToSelfReferencedTestTable(connection, "TestTable", pk2, null, null, null);
				InsertRowToSelfReferencedTestTable(connection, "TestTable", pk3, pk1, pk2, null);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 12);

					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 3);

					AssertSelfReferencedTestTableValues(connection, pk1, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, pk1, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, pk1, null, transformId: 3);

					AssertSelfReferencedTestTableValues(connection, pk2, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, pk2, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, pk2, null, transformId: 3);

					AssertSelfReferencedTestTableValues(connection, pk3, pk1, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, pk3, pk2, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, pk3, null, transformId: 3);
				});

				UpdateRowInSelfReferencedTestTable(connection, "TestTable", pk3, pk2, pk2, pk1);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 12);

					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, dummyPk, null, transformId: 3);

					AssertSelfReferencedTestTableValues(connection, pk1, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, pk1, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, pk1, null, transformId: 3);

					AssertSelfReferencedTestTableValues(connection, pk2, null, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, pk2, null, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, pk2, null, transformId: 3);

					AssertSelfReferencedTestTableValues(connection, pk3, pk2, transformId: 1);
					AssertSelfReferencedTestTableValues(connection, pk3, pk2, transformId: 2);
					AssertSelfReferencedTestTableValues(connection, pk3, pk1, transformId: 3);
				});

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestSelfReferencedTable_MultipleSelfReferencedKeys()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateSelfReferencedTestTableInMainDb(connection, "TestTable");
				CreateMultiSelfReferencedTransformTestTable(connection, "TestTable", "BAS__TestTable");

				#region Initial Load

				var parentPk1 = Guid.NewGuid();
				var parentPk2 = Guid.NewGuid();
				var childPk1 = Guid.NewGuid();

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				InsertRowToMultiSelfReferencedTestTable(connection, "TestTable", parentPk1, null, null, "Include");
				InsertRowToMultiSelfReferencedTestTable(connection, "TestTable", parentPk2, parentPk1, null, "Include");
				InsertRowToMultiSelfReferencedTestTable(connection, "TestTable", childPk1, parentPk1, parentPk2, "Include");

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 3);
					AssertMultiSelfReferencedTestTableValues(connection, parentPk1, null, null);
					AssertMultiSelfReferencedTestTableValues(connection, parentPk2, parentPk1, null);
					AssertMultiSelfReferencedTestTableValues(connection, childPk1, parentPk1, parentPk2);
				});

				#endregion

				#region Incremental Load

				var pk1 = Guid.NewGuid();
				var pk2 = Guid.NewGuid();

				// ParentKey2 should be null for StringValue == "Exclude"
				InsertRowToMultiSelfReferencedTestTable(connection, "TestTable", pk1, parentPk1, parentPk2, "Include");
				InsertRowToMultiSelfReferencedTestTable(connection, "TestTable", pk2, parentPk1, parentPk2, "Exclude");

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				edwTsqlScriptRunner.Run();

				CombineAssertions(GetTestTableContents(connection, "BAS__TestTable"), () =>
				{
					AssertTestTableCount(connection, "BAS__TestTable", 4);
					AssertMultiSelfReferencedTestTableValues(connection, parentPk1, null, null);
					AssertMultiSelfReferencedTestTableValues(connection, parentPk2, parentPk1, null);
					AssertMultiSelfReferencedTestTableValues(connection, childPk1, parentPk1, parentPk2);

					AssertMultiSelfReferencedTestTableValues(connection, pk1, parentPk1, parentPk2);
					AssertMultiSelfReferencedTestTableValues(connection, pk2, parentPk1, parentPk2, exists: false);
				});

				#endregion
			}
		}

		string GetTestTableContents(DbConnection connection, string tableName)
		{
			var result = new StringBuilder();

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			using (var reader = connection.Command(string.Format("SELECT * FROM [Test].[{0}]", tableName)).ExecuteReader())
			{
				var schemaTable = reader.GetSchemaTable();
				var columnNames = new List<string>();
				foreach (DataRow row in reader.GetSchemaTable().Rows)
				{
					columnNames.Add(row["ColumnName"].ToString());
				}
				result.AppendLine(string.Join("    ", columnNames));

				while (reader.Read())
				{
					var values = new List<string>();
					foreach (var columnName in columnNames)
					{
						var obj = reader[columnName];
						if (obj != DBNull.Value)
						{
							values.Add(reader[columnName].ToString());
						}
						else
						{
							values.Add("NULL");
						}
					}
					result.AppendLine(string.Join("    ", values));
				}
			}

			return result.ToString();
		}

		#endregion

		#region Index Rebuild

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestBaseTableIndex()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInEdw(connection, "TestTable");

				var modelSchema = "Organization";
				var tableName = "BAS__Staff";
				var idName = "StaffID";
				var keyName = "StaffKey";

				CombineAssertions("Before ETL run", () =>
				{
					var idIndexCount = GetIdIndexCount(connection, modelSchema, tableName, idName);
					AssertEquals("Column ID index count", 2, idIndexCount);

					var keyIndexCount = GetKeyIndexCount(connection, modelSchema, tableName, keyName);
					AssertEquals("Column Key index count", 1, keyIndexCount);
				}
				);

				var scanner = new CdcScannerForTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcess();
				edwTsqlScriptRunner.Run();
				InsertRowToTestTable(connection, "TestTable", 1, null);
				scanner.ScanUntilNoTransactionsToProcess();
				edwTsqlScriptRunner.Run();

				CombineAssertions("After ETL run", () =>
				{
					var idIndexCount = GetIdIndexCount(connection, modelSchema, tableName, idName);
					AssertEquals("Column ID index count", 2, idIndexCount);

					var keyIndexCount = GetKeyIndexCount(connection, modelSchema, tableName, keyName);
					AssertEquals("Column Key index count", 1, keyIndexCount);
				}
				);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestAggregateTableIndex()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInEdw(connection, "TestTable");
				CreateAggregateTestTable(connection, "BAS__TestTable", "AGG__AggTestTable");

				var modelSchema = "Test";
				var tableName = "AGG__AggTestTable";
				var keyName = "AggTestTableKey";

				var keyIndexCount = GetKeyIndexCount(connection, modelSchema, tableName, keyName);
				AssertEquals("Before ETL run, Column Key index count should be 0", 0, keyIndexCount);

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();
				InsertRowToTestTable(connection, "TestTable", 1, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				#endregion

				keyIndexCount = GetKeyIndexCount(connection, modelSchema, tableName, keyName);
				AssertEquals("After ETL run, Column Key index count should be 1", 1, keyIndexCount);
			}
		}

		int GetIdIndexCount(AdminConnection connection, string modelSchema, string tableName, string idName)
		{
			int indexCount;
			var pkColumnIndexQuery = @"
SELECT 
	COUNT(*)
FROM sys.indexes i 
	INNER JOIN sys.objects o ON i.object_id = o.object_id 
	INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
	INNER JOIN sys.index_columns ic on ic.object_id = i.object_id and ic.index_id = i.index_id
	INNER join sys.columns c on c.object_id = ic.object_id and ic.column_id = c.column_id	
WHERE o.type <> 'S' AND is_primary_key <> 1 AND i.index_id > 0 and i.is_disabled = 0 and i.type = 2 and i.is_unique = 0
	AND s.name = @model_schema AND o.name = @short_model_table_name 
	AND i.name = 'IX_' + @model_schema + '_' + @short_model_table_name + '_' + @IdName
	AND (c.name = '__$transform_id' or c.name = @IdName)";

			using (((ICurrentDbControl)connection).UseDatabase(Db.EdwDatabaseName))
			using (var cmd = connection.Command(pkColumnIndexQuery))
			{
				cmd.AddParameter("@model_schema", SqlDbType.NVarChar, 128, modelSchema);
				cmd.AddParameter("@short_model_table_name", SqlDbType.NVarChar, 128, tableName);
				cmd.AddParameter("@IdName", SqlDbType.NVarChar, 128, idName);

				indexCount = Convert.ToInt32(cmd.ExecuteScalar());
			}

			return indexCount;
		}

		int GetKeyIndexCount(AdminConnection connection, string modelSchema, string tableName, string keyName)
		{
			int indexCount;
			var pkColumnIndexQuery = @"
SELECT 
	COUNT(*)
FROM sys.indexes i 
	INNER JOIN sys.objects o ON i.object_id = o.object_id 
	INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
	INNER JOIN sys.index_columns ic on ic.object_id = i.object_id and ic.index_id = i.index_id
	INNER join sys.columns c on c.object_id = ic.object_id and ic.column_id = c.column_id
	
WHERE o.type <> 'S' AND is_primary_key <> 1 AND i.index_id > 0 and i.is_disabled = 0 and i.type = 2 and i.is_unique = 0
	AND s.name = @model_schema AND o.name = @short_model_table_name 
	AND i.name = 'IX_' + @model_schema + '_' + @short_model_table_name + '_' + @KeyName
	AND (c.name = @KeyName)";

			using (((ICurrentDbControl)connection).UseDatabase(Db.EdwDatabaseName))
			using (var cmd = connection.Command(pkColumnIndexQuery))
			{
				cmd.AddParameter("@model_schema", SqlDbType.NVarChar, 128, modelSchema);
				cmd.AddParameter("@short_model_table_name", SqlDbType.NVarChar, 128, tableName);
				cmd.AddParameter("@KeyName", SqlDbType.NVarChar, 128, keyName);

				indexCount = Convert.ToInt32(cmd.ExecuteScalar());
			}

			return indexCount;
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		[ExpectNoExceptions]
		public void TestAggregateTableInitialLoadNotCreateExistingIndex()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var modelSchema = "Test";
				var tableName = "AGG__AggTestTable";
				var keyName = "TestTable1Key";

				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInEdw(connection, "TestTable");
				CreateAggregateTestTable(connection, "BAS__TestTable", "AGG__AggTestTable", @$"CREATE NONCLUSTERED INDEX [IX_{modelSchema}_{tableName}_TESTINDEX] ON [{modelSchema}].[{tableName}] ({keyName})");

				RunAggregateTableInitialLoad(connection, modelSchema, tableName);

				int indexCount;
				var pkColumnIndexQuery = @"
SELECT 
	COUNT(*)
FROM sys.indexes i 
	INNER JOIN sys.objects o ON i.object_id = o.object_id 
	INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
	INNER JOIN sys.index_columns ic on ic.object_id = i.object_id and ic.index_id = i.index_id
	INNER join sys.columns c on c.object_id = ic.object_id and ic.column_id = c.column_id
	
WHERE o.type <> 'S' AND is_primary_key <> 1 AND i.index_id > 0 and i.is_disabled = 0 and i.type = 2 and i.is_unique = 0
	AND s.name = @model_schema AND o.name = @short_model_table_name 
	AND i.name = 'IX_' + @model_schema + '_' + @short_model_table_name + '_TESTINDEX'
	AND (c.name = @KeyName)";

				using (((ICurrentDbControl)connection).UseDatabase(Db.EdwDatabaseName))
				using (var cmd = connection.Command(pkColumnIndexQuery))
				{
					cmd.AddParameter("@model_schema", SqlDbType.NVarChar, 128, modelSchema);
					cmd.AddParameter("@short_model_table_name", SqlDbType.NVarChar, 128, tableName);
					cmd.AddParameter("@KeyName", SqlDbType.NVarChar, 128, keyName);

					indexCount = Convert.ToInt32(cmd.ExecuteScalar());
				}

				AssertEquals("New custom index should be created", 1, indexCount);

				// Should not trigger the existing index exception
				RunAggregateTableInitialLoad(connection, modelSchema, tableName);
			}
		}

		void RunAggregateTableInitialLoad(AdminConnection connection, string modelSchema, string tableName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(Db.EdwDatabaseName))
			{
				connection.ExecuteNonQuery(@$"EXEC [{modelSchema}].[usp_IniLoad_{tableName}]");
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRun_CorruptedIndexForBaseTable()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInEdw(connection, "TestTable");

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();
				InsertRowToTestTable(connection, "TestTable", 1, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertTestTableValues(connection, "BAS__TestTable", 1, 1);
					AssertInitialLoadRequiredForBaseTable(connection, "BAS__TestTable", false);
					AssertCorruptedIndexErrorForBaseTable(connection, "BAS__TestTable", false);
				});

				#endregion

				#region Corrupted Index

				UpdateRowInTestTable(connection, "TestTable", 1, 2);

				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					var sqlText = string.Format(CultureInfo.InvariantCulture, @"
DECLARE @Error601Message NVARCHAR(MAX)
SELECT @Error601Message = [text]
FROM sys.messages
WHERE language_id = 1033 and message_id = 601

UPDATE [{0}].TransformTableConfiguration
SET IncrementalInsertQuery = 'DECLARE @inserts BIGINT = 0; RAISERROR (''' + @Error601Message + ''', 12, 1);'
WHERE ModelTableName = 'BAS__TestTable'", BiConstants.BiAdminSchemaName);
					connection.ExecuteNonQuery(sqlText);
				}

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					Assert("Corrupted index error log not found during EET execution.", logger.LogEntries.Contains("Corrupted index found in [BAS__TestTable]. Rebuilding index in the next run."));
					AssertInitialLoadRequiredForBaseTable(connection, "BAS__TestTable", true);
					AssertCorruptedIndexErrorForBaseTable(connection, "BAS__TestTable", true);
				});

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRun_CorruptedIndexForAggregateTable()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInEdw(connection, "TestTable");
				CreateAggregateTestTable(connection, "BAS__TestTable", "AGG__AggTestTable");

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();
				InsertRowToTestTable(connection, "TestTable", 1, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Actual logs:\r\n{0}", string.Join("\r\n", logger.LogEntries)), () =>
				{
					AssertTestTableValues(connection, "BAS__TestTable", 1, 1);
					AssertAggregateTestTableValues(connection, "AGG__AggTestTable", 1, 1);
					AssertInitialLoadRequiredForBaseTable(connection, "BAS__TestTable", false);
					AssertInitialLoadRequiredForAggregateTable(connection, "AGG__AggTestTable", false);
					AssertCorruptedIndexErrorForAggregateTable(connection, "AGG__AggTestTable", false);
				});

				#endregion

				#region Corrupted Index

				UpdateRowInTestTable(connection, "TestTable", 1, 2);

				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					var sqlText = @"
ALTER PROCEDURE [Test].[usp_IncLoad_AGG__AggTestTable]

@number_of_future_periods_for_partitioning INT = 6

AS

BEGIN
	SET NOCOUNT ON

	DECLARE @Error601Message NVARCHAR(MAX)
	SELECT @Error601Message = [text]
	FROM sys.messages
	WHERE language_id = 1033 and message_id = 601

	DECLARE @inserts BIGINT = 0;
	RAISERROR (@Error601Message, 12, 1);

END";
					connection.ExecuteNonQuery(sqlText);
				}

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					Assert("Corrupted index error log not found during EET execution.", logger.LogEntries.Contains("Corrupted index found in [AGG__AggTestTable]. Rebuilding index in the next run."));
					AssertInitialLoadRequiredForBaseTable(connection, "BAS__TestTable", false);
					AssertInitialLoadRequiredForAggregateTable(connection, "AGG__AggTestTable", false);
					AssertCorruptedIndexErrorForAggregateTable(connection, "AGG__AggTestTable", true);
				});

				#endregion

				#region Fix Corrupted Index

				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					connection.ExecuteNonQuery(@"DROP PROCEDURE [Test].[usp_IncLoad_AGG__AggTestTable]");

					var denormTable = BiAutomationConfigLoader.Instance.ConfigData.EdwDenormalizedTableConfig.FirstOrDefault(t => t.Name == "AGG__AggTestTable");
					connection.ExecuteNonQuery(BiAutomationConfigLoader.Instance.ConfigData.GetIncrementalLoadQueryForDenormalizedTable(denormTable));
				}

				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertInitialLoadRequiredForBaseTable(connection, "BAS__TestTable", false);
					AssertInitialLoadRequiredForAggregateTable(connection, "AGG__AggTestTable", false);
					AssertCorruptedIndexErrorForAggregateTable(connection, "AGG__AggTestTable", false);
				});

				#endregion
			}
		}

		public void TestBaseTableCustomIndexesAreInEdwDatabase()
		{
			var configs = BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.Where(x => !string.IsNullOrEmpty(x.CustomIndex));
			var tableIndexMapping = new Dictionary<string, string>();
			foreach (var config in configs)
			{
				tableIndexMapping.Add($"{config.Schema}.{config.Name}", config.CustomIndex);
			}
			TestCustomIndexesInDatabase(tableIndexMapping);
		}

		public void TestDenormalizedTableCustomIndexesAreInEdwDatabase()
		{
			var configs = BiAutomationConfigLoader.Instance.ConfigData.EdwDenormalizedTableConfig.Where(x => !string.IsNullOrEmpty(x.CustomIndex));
			var tableIndexMapping = new Dictionary<string, string>();
			foreach (var config in configs)
			{
				tableIndexMapping.Add($"{config.Schema}.{config.Name}", config.CustomIndex);
			}
			TestCustomIndexesInDatabase(tableIndexMapping);
		}

		void TestCustomIndexesInDatabase(IDictionary<string, string> tableIndexMapping)
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				CombineAssertions(() =>
				{
					foreach (var mapping in tableIndexMapping)
					{
						GetCustomIndexInfo(mapping.Value, out var fullTableName, out var indexName);
						AssertNotNull($"Custom Index in table {mapping.Key} should have a not null name.", indexName);
						AssertEquals($"Index should be created in correct table {mapping.Key}", mapping.Key, fullTableName);
						Assert($"Index {indexName} should exist in EDW database.", CheckIndexExists(connection, indexName));
					}
				});
			}
			Assert("Avoid unit test failure when there is no custom index", true);
		}

		bool CheckIndexExists(AdminConnection adminConnection, string indexName)
		{
			var sqlText = $"IF EXISTS(SELECT NULL FROM sys.indexes WHERE name = '{indexName}') SELECT 1 ELSE SELECT 0";
			var objResult = adminConnection.ExecuteScalar(sqlText);

			return Convert.ToBoolean(objResult, CultureInfo.InvariantCulture);
		}

		void GetCustomIndexInfo(string indexQuery, out string fullTableName, out string indexName)
		{
			indexName = null;
			fullTableName = null;
			var pattern = @"CREATE\s.*\sINDEX\s\[?(?<indexName>.*)\]?\sON\s\[?(?<schemaName>\w+)\]?\.\[?(?<tableName>\w+)\]?\s";
			var match = Regex.Match(indexQuery, pattern, RegexOptions.IgnoreCase);
			if (match.Success)
			{
				indexName = match.Result("${indexName}").Replace("[", "").Replace("]", "");
				fullTableName = match.Result("${schemaName}.${tableName}").Replace("[", "").Replace("]", "");
			}
		}

		#endregion

		#region Staging Table

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestMergeIncrementalChanges_NewAndUpdate()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInMainDb(connection, "TestTable");
				SetInitialLoadRequestedStatus(connection, 3);

				var pk = Guid.NewGuid();
				InsertRowToStagingTable(connection, "TestTable", pk, new byte[] { 1 }, 0);
				InsertRowToStagingTable(connection, "TestTable", pk, new byte[] { 2 }, 0);
				InsertRowToStagingTable(connection, "TestTable", pk, new byte[] { 3 }, 0);

				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);
				edwTsqlScriptRunner.ExecuteStagingMerge_Exposed();

				CombineAssertions(() =>
				{
					AssertStagingTestTableValues(connection, "TestTable", pk, new byte[] { 1 }, 0, 0);
					AssertStagingTestTableValues(connection, "TestTable", pk, new byte[] { 2 }, 0, 0);
					AssertStagingTestTableValues(connection, "TestTable", pk, new byte[] { 3 }, 0, 1);
				});
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestMergeIncrementalChanges_NewAndDelete()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInMainDb(connection, "TestTable");
				SetInitialLoadRequestedStatus(connection, 3);

				var pk = Guid.NewGuid();
				InsertRowToStagingTable(connection, "TestTable", pk, new byte[] { 1 }, 0);
				InsertRowToStagingTable(connection, "TestTable", pk, new byte[] { 2 }, 1);

				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);
				edwTsqlScriptRunner.ExecuteStagingMerge_Exposed();

				CombineAssertions(() =>
				{
					AssertStagingTestTableValues(connection, "TestTable", pk, new byte[] { 1 }, 0, 0);
					AssertStagingTestTableValues(connection, "TestTable", pk, new byte[] { 2 }, 1, 0);
				});
			}
		}

		public void TestInsufficientMemmoryException_WisecloudHosted()
		{
			using (var connection = Db.NewAdminConnection())
			{
				EnvProxy.SetHostedLocationForTest("");
				var logger = new LoggerForTest();
				var tsqlScriptRunner = new TsqlScriptRunnerForTest_InsufficientMemory(connection, logger);
				try
				{
					tsqlScriptRunner.Run();
					Fail("Expected HostedServiceException to be thrown");
				}
				catch (HostedServiceException ex)
				{
					Assert(ex.LogException);
				}
			}
		}

		public void TestInsufficientMemmoryException_SelfHosted()
		{
			using (var connection = Db.NewAdminConnection())
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				var logger = new LoggerForTest();
				var tsqlScriptRunner = new TsqlScriptRunnerForTest_InsufficientMemory(connection, logger);
				try
				{
					tsqlScriptRunner.Run();
					Fail("Expected InsufficientMemory exception.");
				}
				catch (SqlException ex)
				{
					AssertEquals(new DbErrorMatch(ex).ExceptionType, DbErrorType.InsufficientSystemMemoryToRunQuery);
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestMergeIncrementalChanges_NewAndFromIniLoad()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInMainDb(connection, "TestTable");
				SetInitialLoadRequestedStatus(connection, 3);

				var pk = Guid.NewGuid();
				InsertRowToStagingTable(connection, "TestTable", pk, new byte[] { 1 }, 0);
				InsertRowToStagingTable(connection, "TestTable", pk, new byte[] { 0 }, 9);

				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);
				edwTsqlScriptRunner.ExecuteStagingMerge_Exposed();

				CombineAssertions(() =>
				{
					AssertStagingTestTableValues(connection, "TestTable", pk, new byte[] { 1 }, 0, 1);
					AssertStagingTestTableValues(connection, "TestTable", pk, new byte[] { 0 }, 9, 0);
				});
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestStagingTableInitialLoadStartLsnValues()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);
				DisableCdc(connection);
				EnableCdc(connection);

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				scanner.ScanUntilNoTransactionsToProcessSafe();

				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);
				edwTsqlScriptRunner.Run();

				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					var sqlText = @"SELECT COUNT(*) FROM Staging.[{0}] WHERE OP_TYPE = 9 AND __$start_lsn <> 0x0";
					CombineAssertions("All OP_TYPE = 9 rows in Staging tables should have __$start_lsn = 0x0.", () =>
					{
						foreach (var (schema, tableName) in StagingTableList)
						{
							AssertEquals(tableName, 0, Convert.ToInt32(connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, sqlText, tableName)), CultureInfo.InvariantCulture));
						}
					});
				}

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestStagingTableHaveCorrectIndexes()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);
				TruncateEDWTables(connection);
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);
				CreateTestTableInEdw(connection, "TestTable");

				var scanner = new CdcScannerForEdwTest();
				scanner.ScanUntilNoTransactionsToProcessSafe();

				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);
				edwTsqlScriptRunner.Run();

				logger.ClearLog();

				InsertRowToTestTable(connection, "TestTable", 1, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					var sqlText =
@"
IF EXISTS
(SELECT 1
FROM
(SELECT 
	t.name as TableName,
	COUNT(DISTINCT CASE WHEN i.type = 1 THEN i.index_id ELSE NULL END) AS NumberOfClusteredRowstoreIndexes,
	COUNT(DISTINCT i.index_id) AS TotalNumberIndexes,
	COUNT(DISTINCT c.column_id) AS NumberOfColumnsInCRI,
	SUM(CASE WHEN c.name = @OltpPkName THEN 1 ELSE 0 END) AS IndexContainsPK	
FROM sys.tables t
	INNER JOIN sys.schemas s on s.schema_id = t.schema_id
	LEFT JOIN sys.indexes i on i.object_id = t.object_id
	LEFT JOIN sys.index_columns ic on ic.index_id = i.index_id AND ic.object_id = t.object_id AND i.type = 1 AND i.name = @IndexName
	LEFT JOIN sys.columns c on c.column_id = ic.column_id AND c.object_id = t.object_id
WHERE s.name = 'Staging' AND t.name = @TableName
GROUP BY t.name) Q
WHERE Q.NumberOfClusteredRowstoreIndexes = 1 AND Q.TotalNumberIndexes = 1 AND Q.NumberOfColumnsInCRI = 1 AND Q.IndexContainsPK = 1)
SELECT 1
ELSE
SELECT 0";

					var stagingTable = CdcConfigTables.FirstOrDefault(t => t.TableInEdw && !t.IsEdiClient && t.SourceTable.Equals("TestTable"));
					var pkName = stagingTable.GetCdcColumnConfigRows().FirstOrDefault(c => c.IsPrimaryKey).SourceColumn;
					if (!string.IsNullOrEmpty(pkName))
					{
						var indexName = string.Format(CultureInfo.InvariantCulture, "CX_Staging_{0}_{1}", stagingTable.SourceTable, pkName);
						using (var cmd = connection.Command(sqlText))
						{
							cmd.AddParameter("@TableName", SqlDbType.NVarChar, 128, stagingTable.SourceTable);
							cmd.AddParameter("@OltpPkName", SqlDbType.NVarChar, 128, pkName);
							cmd.AddParameter("@IndexName", SqlDbType.NVarChar, 128, indexName);

							Assert(indexName, Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
						}
					}
					else
					{
						Fail(string.Format(CultureInfo.InvariantCulture, "Failed to find primary key for table [{0}]", stagingTable.SourceTable));
					}
				}
			}
		}
		#endregion

		#region Initial Load Batch

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestInitialLoadBatch()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInEdw(connection, "TestTable");

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);

				InsertRowToTestTable(connection, "TestTable", 1, null);
				InsertRowToTestTable(connection, "TestTable", 2, null);
				InsertRowToTestTable(connection, "TestTable", 3, null);
				InsertRowToTestTable(connection, "TestTable", 4, null);
				InsertRowToTestTable(connection, "TestTable", 5, null);
				InsertRowToTestTable(connection, "TestTable", 6, null);
				InsertRowToTestTable(connection, "TestTable", 7, null);
				InsertRowToTestTable(connection, "TestTable", 8, null);
				InsertRowToTestTable(connection, "TestTable", 9, null);
				InsertRowToTestTable(connection, "TestTable", 10, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Actual logs:\r\n{0}", string.Join("\r\n", logger.LogEntries)), () =>
				{
					AssertStagingTableCount(connection, "TestTable", 10);
					AssertInitialLoadCompletedBatches(connection, "TestTable", null);
				});

				#endregion

				#region Batch Mode

				edwTsqlScriptRunner.InitialLoadBatchSizeForTest = 3;

				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);

				DeleteRowTestTable(connection, "TestTable", 1);
				InsertRowToTestTable(connection, "TestTable", 1, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Actual logs:\r\n{0}", string.Join("\r\n", logger.LogEntries)), () =>
				{
					AssertStagingTableCount(connection, "TestTable", 10);
					AssertInitialLoadCompletedBatches(connection, "TestTable", 4);
				});

				testHelper.TruncateTables(connection);
				InsertRowToTestTable(connection, "TestTable", 11, null);
				InsertRowToTestTable(connection, "TestTable", 12, null);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Actual logs:\r\n{0}", string.Join("\r\n", logger.LogEntries)), () =>
				{
					AssertStagingTableCount(connection, "TestTable", 12);
					AssertInitialLoadCompletedBatches(connection, "TestTable", 5);
				});

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestInitialLoadBatch_ResumesAfterFailure()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInEdw(connection, "TestTable");

				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					var sqlText = @"ALTER TABLE [Staging].[TestTable] ALTER COLUMN [Test_String] varchar(5)";
					connection.ExecuteNonQuery(sqlText);
				}

				#region Staging Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);
				edwTsqlScriptRunner.InitialLoadBatchSizeForTest = 3;

				var guidList = (new List<Guid>
					{
						Guid.Parse("D9F327ED-A9DF-4ADB-8D52-313F49C259B7"),
						Guid.Parse("3A080947-2D16-4FDC-B2DD-35CF30CA08F0"),
						Guid.Parse("E7A7C07F-B991-4990-A6EF-54699F6CA1B9"),
						Guid.Parse("BE4C3E83-6719-4216-AB3F-6176DA6CCDEA"),
						Guid.Parse("C1298312-402D-4EF2-8DFC-7CE5F7C8793E"),
						Guid.Parse("6E194D12-6208-4255-B842-7FF392822BEF"),
						Guid.Parse("795ACD58-9019-461B-AD3F-899A63E3259B"),
						Guid.Parse("84385B94-1FB6-4C23-B720-A7F95E3619BC"),
						Guid.Parse("BE8A139B-D239-4ED2-AD10-D32CEF2ABE8C"),
						Guid.Parse("07A34862-384B-408B-8BE8-EE1A270150C5"),
					});

				for (var counter = 0; counter < 5; counter++)
				{
					InsertRowToTestTable(connection, "TestTable", guidList[counter], "Test" + counter.ToString());
				}

				for (var counter = 5; counter < guidList.Count; counter++)
				{
					InsertRowToTestTable(connection, "TestTable", guidList[counter], "TestStr" + counter.ToString());
				}

				scanner.ScanUntilNoTransactionsToProcessSafe();
				try
				{
					edwTsqlScriptRunner.Run();
					Fail("EtlExecutionException should have been thrown.");
				}
				catch (EtlExecutionException ex)
				{
					CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Actual logs:\r\n{0}", string.Join("\r\n", logger.LogEntries)), () =>
					{
						Assert(ex.Message, ex.Message.Contains("String or binary data would be truncated"));

						AssertStagingTableCount(connection, "TestTable", 3);
						AssertStagingTableValue(connection, "TestTable", guidList[0], "Test0");
						AssertStagingTableValue(connection, "TestTable", guidList[1], "Test1");
						AssertStagingTableValue(connection, "TestTable", guidList[2], "Test2");

						AssertInitialLoadCompletedBatches(connection, "TestTable", 1);
					});
				}

				#endregion

				#region Rerun Initial Load

				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					var sqlText = @"ALTER TABLE [Staging].[TestTable] ALTER COLUMN [Test_String] varchar(10)";
					connection.ExecuteNonQuery(sqlText);
				}

				for (var counter = 0; counter < guidList.Count; counter++)
				{
					UpdateRowInTestTable(connection, "TestTable", guidList[counter], "NewString" + counter.ToString());
				}

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Actual logs:\r\n{0}", string.Join("\r\n", logger.LogEntries)), () =>
				{
					AssertStagingTableCount(connection, "TestTable", 10);
					AssertStagingTableValue(connection, "TestTable", guidList[0], "Test0");
					AssertStagingTableValue(connection, "TestTable", guidList[1], "Test1");
					AssertStagingTableValue(connection, "TestTable", guidList[2], "Test2");
					AssertStagingTableValue(connection, "TestTable", guidList[3], "NewString3");
					AssertStagingTableValue(connection, "TestTable", guidList[4], "NewString4");
					AssertStagingTableValue(connection, "TestTable", guidList[5], "NewString5");
					AssertStagingTableValue(connection, "TestTable", guidList[6], "NewString6");
					AssertStagingTableValue(connection, "TestTable", guidList[7], "NewString7");
					AssertStagingTableValue(connection, "TestTable", guidList[8], "NewString8");
					AssertStagingTableValue(connection, "TestTable", guidList[9], "NewString9");

					AssertInitialLoadCompletedBatches(connection, "TestTable", 4);
				});

				#endregion
			}
		}

		#endregion

		#region CDC Update Mask Trigger

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestCdcUpdateMaskTriggersTransform()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);
				TruncateEDWTables(connection);
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateMainDbTableWithMultipleColumns(connection);
				CreateTestTableWithUpdateMask(connection);

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);

				edwTsqlScriptRunner.Run();

				var pk = Guid.NewGuid();
				InsertToTestTable(connection, pk);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				Assert(logger.LogEntries.Any(l => l.Contains("[Test].[BAS__TestTable] TransformId: 1 Insert(1)")));

				#endregion

				#region Incremental Load Single Update

				logger.ClearLog();
				UpdateTestTable(connection, pk, "Column1", 1);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				Assert("EDW Column in first segment of update mask", logger.LogEntries.Any(l => l.Contains("[Test].[BAS__TestTable] TransformId: 1 (deletes: 1, inserts: 1, net: 0)")));

				logger.ClearLog();
				UpdateTestTable(connection, pk, "Column65", 1);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				Assert("EDW Column in second segment of update mask", logger.LogEntries.Any(l => l.Contains("[Test].[BAS__TestTable] TransformId: 1 (deletes: 1, inserts: 1, net: 0)")));

				logger.ClearLog();
				UpdateTestTable(connection, pk, "Column2", 1);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				Assert("Non-EDW Column in first segment of update mask", !logger.LogEntries.Any(l => l.Contains("[Test].[BAS__TestTable] TransformId: 1 (deletes: 1, inserts: 1, net: 0)")));

				logger.ClearLog();
				UpdateTestTable(connection, pk, "Column66", 1);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				Assert("Non-EDW Column in second segment of update mask", !logger.LogEntries.Any(l => l.Contains("[Test].[BAS__TestTable] TransformId: 1 (deletes: 1, inserts: 1, net: 0)")));

				#endregion

				#region Incremental Load Multiple Updates

				logger.ClearLog();
				UpdateTestTable(connection, pk, "Column1", 2);
				UpdateTestTable(connection, pk, "Column1", 3);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				Assert("EDW Column in first segment of update mask - multiple updates", logger.LogEntries.Any(l => l.Contains("[Test].[BAS__TestTable] TransformId: 1 (deletes: 1, inserts: 1, net: 0)")));

				logger.ClearLog();
				UpdateTestTable(connection, pk, "Column65", 2);
				UpdateTestTable(connection, pk, "Column65", 3);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				Assert("EDW Column in second segment of update mask - multiple updates", logger.LogEntries.Any(l => l.Contains("[Test].[BAS__TestTable] TransformId: 1 (deletes: 1, inserts: 1, net: 0)")));

				logger.ClearLog();
				UpdateTestTable(connection, pk, "Column2", 2);
				UpdateTestTable(connection, pk, "Column2", 3);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				Assert("Non-EDW Column in first segment of update mask - multiple updates", !logger.LogEntries.Any(l => l.Contains("[Test].[BAS__TestTable] TransformId: 1 (deletes: 1, inserts: 1, net: 0)")));

				logger.ClearLog();
				UpdateTestTable(connection, pk, "Column66", 2);
				UpdateTestTable(connection, pk, "Column66", 3);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				Assert("Non-EDW Column in second segment of update mask - multiple updates", !logger.LogEntries.Any(l => l.Contains("[Test].[BAS__TestTable] TransformId: 1 (deletes: 1, inserts: 1, net: 0)")));

				#endregion
			}
		}

		void CreateMainDbTableWithMultipleColumns(AdminConnection connection)
		{
			connection.ExecuteNonQuery(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Test].[TestTable] (PK uniqueidentifier NOT NULL)
ALTER TABLE [Test].[TestTable]
ADD CONSTRAINT [PK_UX__TestTable_PK] PRIMARY KEY NONCLUSTERED ([PK] ASC)");

			var table = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.AddCdcTableConfigRow("Test", "TestTable", "Test", "", "", true, true, null, null, null, false);
			BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(table, "PK", "uniqueidentifier", 0, 0, 0, false, true, "", "", true, "", "", true, true, "");

			for (int counter = 1; counter <= 66; counter++)
			{
				connection.ExecuteNonQuery("ALTER TABLE [Test].[TestTable] ADD Column" + counter + " int");

				var columnInEdw = (counter == 1 || counter == 65);

				BiAutomationConfigLoader.Instance.ConfigData.CdcColumnConfig.AddCdcColumnConfigRow(table, "Column" + counter, "int", 0, 0, 0, true, false, "", "", true, "", "", true, columnInEdw, "");
			}

			var cdcTable = new CdcTableForTesting("Test", "TestTable");
			cdcTable.EnableCdc(connection);
		}

		void CreateTestTableWithUpdateMask(DbConnection connection)
		{
			using (((ICurrentDbControl)connection).UseDatabase(Db.EdwDatabaseName))
			{
				var edwTable = BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.AddEdwTableConfigRow("Test", "BAS__TestTable", "", "Test", "TestTable", "", 1, false, 1, "");
				BiAutomationConfigLoader.Instance.ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "uniqueidentifier", 0, 0, 0, "TestTableID", "PK", false, false, "", "", "", "", "", "", true);
				BiAutomationConfigLoader.Instance.ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "bigint", 16, 0, 0, "TestTableKey", "", false, false, "", "", "", "", "", "", true);
				BiAutomationConfigLoader.Instance.ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "int", 0, 0, 0, "Column1", "Column1", false, false, "", "", "", "", "", "", false);
				BiAutomationConfigLoader.Instance.ConfigData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "int", 0, 0, 0, "Column65", "Column65", false, false, "", "", "", "", "", "", false);

				connection.ExecuteNonQuery(string.Format(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Staging].[TestTable] (
	[__$start_lsn] binary(10),
	OP_TYPE tinyint,
	PK uniqueidentifier,
	Column1 int,
	Column65 int)

CREATE TABLE [Test].[BAS__TestTable] (
	[__$transform_id] int,
	TestTableID uniqueidentifier,
	TestTableKey bigint,
	Column1 int,
	Column65 int)

INSERT INTO [{0}].[StagingTableConfiguration](SourceSchemaName, StagingSchemaName, StagingTableName, PkName, IndexedColumnName, StagingTableDefinition, StagingTableColumnListSelect, StagingTableColumnListInsert, ColumnEnumeratedList, ColumnEnumeratedListWithDefinition)
VALUES
(
	N'Test', N'Staging', N'TestTable', N'PK', N'PK', N'PK uniqueidentifier, Column1 int, Column65 int', N'PK, Column1, Column65', N'PK, Column1, Column65',
	N'c1, c2, c3', N'c1 uniqueidentifier, c2 int, c3 int'
)

INSERT INTO [{0}].[StagingTableState](SourceTableName, CurrentMaxLsn, CurrentState, InitialLoadRequired)
VALUES ('Test.TestTable', 0x0, 'New', 1)

INSERT INTO [{0}].[TransformTableConfiguration](SourceTableName, ModelSchemaName, ModelTableName, DependencyOrder, TransformId, IsLastTransform, HasChildTables, InitialLoadQuery, IncrementalInsertQuery, IncrementalDeleteQuery, CustomIndexScript)
VALUES
(
	N'Test.TestTable', N'Test', N'BAS__TestTable', 1, 1, 1, 0, N'{1}', N'{2}', N'{3}', N'{4}'
)

INSERT INTO [{0}].[TransformTableState](ModelSchemaName, ModelTableName, TransformId, CurrentState, InitialLoadRequired)
VALUES ('Test', 'BAS__TestTable', 1, 'New', 1)",
				BiConstants.BiAdminSchemaName,
				BiAutomationConfigLoader.Instance.ConfigData.GetInitialLoadQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
				BiAutomationConfigLoader.Instance.ConfigData.GetIncrementalInsertQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
				BiAutomationConfigLoader.Instance.ConfigData.GetIncrementalDeleteQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
				BiAutomationConfigLoader.Instance.ConfigData.GetCustomIndexScriptQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")));
			}
		}

		void InsertToTestTable(DbConnection connection, Guid pk)
		{
			var sqlText = @"INSERT INTO [Test].[TestTable] (PK) VALUES(@PK)";
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.ExecuteNonQuery();
			}
		}

		void UpdateTestTable(DbConnection connection, Guid pk, string columnName, int value)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"UPDATE [Test].[TestTable] SET [{0}] = @Value WHERE PK = @PK", columnName);
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@Value", SqlDbType.Int, value);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Custom Tables

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestFullInitialLoadTriggersCustomTableInitialLoad()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				EnableCdc(connection);

				var sqlText = string.Format(@"UPDATE [{0}].CustomTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'", BiConstants.BiAdminSchemaName);
				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					connection.ExecuteNonQuery(sqlText);
				}

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				sqlText = string.Format(CultureInfo.InvariantCulture,
@"IF EXISTS (
	SELECT NULL
	FROM [{0}].[CustomTableState] cts
	INNER JOIN [{0}].[CustomTableConfiguration] ctc
		ON cts.ModelTableName = ctc.ModelTableName AND cts.ModelSchemaName = ctc.ModelSchemaName
	WHERE cts.ModelSchemaName = @ModelSchemaName AND cts.ModelTableName = @ModelTableName AND cts.InitialLoadRequired = 1 AND cts.CurrentState = 'New')
		SELECT 1 ELSE SELECT 0", BiConstants.BiAdminSchemaName);

				CombineAssertions("The following custom tables were not triggered for initial load.", () =>
				{
					foreach (var customTable in BiAutomationConfigLoader.Instance.ConfigData.EdwCustomTableConfig.Where(t => !string.IsNullOrEmpty(t.GetTableDependencyList()) && !t.RunBeforeTransform))
					{
						using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
						using (var cmd = connection.Command(sqlText))
						{
							cmd.AddParameter("@ModelSchemaName", SqlDbType.NVarChar, 128, customTable.Schema);
							cmd.AddParameter("@ModelTableName", SqlDbType.NVarChar, 128, customTable.Name);

							Assert(string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", customTable.Schema, customTable.Name), Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
						}
					}
				});

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestPartialInitialLoadRequiredForCustomTables()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				CreateTestTableInEdw(connection, "TestTable");
				CreateCustomTable(connection, "CUS__UnitTestTable", true);

				var sqlText = string.Format(@"
UPDATE [{0}].StagingTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'
UPDATE [{0}].TransformTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'
UPDATE [{0}].ModelTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'", BiConstants.BiAdminSchemaName);
				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					connection.ExecuteNonQuery(sqlText);
				}

				AssertInitialLoadRequiredForBaseTable(connection, "BAS__TestTable", false);

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				AssertInitialLoadRequiredForBaseTable(connection, "BAS__TestTable", true);

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestCustomTablesDependencyOrderDirectReference()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				var configData = BiAutomationConfigLoader.Instance.ConfigData;

				var customTable1 = configData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", "CUS__UnitTestTable1", -1, "", "", "", "", false, "");
				var customTable2 = configData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", "CUS__UnitTestTable2", -1, "", "", "SELECT * FROM [Test].[CUS__UnitTestTable1]", "", false, "");
				var customTable3 = configData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", "CUS__UnitTestTable3", -1, "", "", "SELECT * FROM [Test].[CUS__UnitTestTable2]", "", false, "");

				configData.SortModelDependencyOrder();

				CombineAssertions(() =>
				{
					Assert("CUS__UnitTestTable2 should be dependent on CUS__UnitTestTable1\r\nDependencyOrder\r\nCUS__UnitTestTable1: " + customTable1.DependencyOrder + "\r\nCUS__UnitTestTable2: " + customTable2.DependencyOrder, customTable1.DependencyOrder < customTable2.DependencyOrder);
					Assert("CUS__UnitTestTable3 should be dependent on CUS__UnitTestTable2\r\nDependencyOrder\r\nCUS__UnitTestTable2: " + customTable2.DependencyOrder + "\r\nCUS__UnitTestTable3: " + customTable3.DependencyOrder, customTable2.DependencyOrder < customTable3.DependencyOrder);
				});

				foreach (var customTable in configData.EdwCustomTableConfig)
				{
					AddCustomTableToConfiguration(connection, customTable);
				}

				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					var sqlText = string.Format(@"
UPDATE [{0}].StagingTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'
UPDATE [{0}].TransformTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'
UPDATE [{0}].ModelTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'
UPDATE [{0}].CustomTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'
WHERE ModelTableName <> 'CUS__UnitTestTable1'", BiConstants.BiAdminSchemaName);
					connection.ExecuteNonQuery(sqlText);
				}

				CombineAssertions(() =>
				{
					AssertInitialLoadRequiredForCustomTable(connection, "CUS__UnitTestTable1", true);
					AssertInitialLoadRequiredForCustomTable(connection, "CUS__UnitTestTable2", false);
					AssertInitialLoadRequiredForCustomTable(connection, "CUS__UnitTestTable3", false);
				});

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertInitialLoadRequiredForCustomTable(connection, "CUS__UnitTestTable1", true);
					AssertInitialLoadRequiredForCustomTable(connection, "CUS__UnitTestTable2", true);
					AssertInitialLoadRequiredForCustomTable(connection, "CUS__UnitTestTable3", true);
				});

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestCustomTablesDependencyOrderIndirectReference()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);

				ClearConfiguration(connection);

				var configData = BiAutomationConfigLoader.Instance.ConfigData;

				var customTable1 = configData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", "CUS__UnitTestTable1", -1, "", "", "TRUNCATE TABLE [Test].[SUP__UnitTestTable1]", "", false, "");
				var customTable2 = configData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", "SUP__UnitTestTable1", -1, "", "", "", "", false, "");
				var customTable3 = configData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", "CUS__UnitTestTable2", -1, "", "", "SELECT * FROM [Test].[SUP__UnitTestTable1]", "", false, "");

				configData.SortModelDependencyOrder();

				CombineAssertions(() =>
				{
					Assert("SUP__UnitTestTable1 should be dependent on CUS__UnitTestTable1\r\nDependencyOrder\r\nCUS__UnitTestTable1: " + customTable1.DependencyOrder + "\r\nSUP__UnitTestTable1: " + customTable2.DependencyOrder, customTable1.DependencyOrder < customTable2.DependencyOrder);
					Assert("CUS__UnitTestTable2 should be dependent on SUP__UnitTestTable1\r\nDependencyOrder\r\nSUP__UnitTestTable1: " + customTable2.DependencyOrder + "\r\nCUS__UnitTestTable2: " + customTable3.DependencyOrder, customTable2.DependencyOrder < customTable3.DependencyOrder);
				});

				foreach (var customTable in configData.EdwCustomTableConfig)
				{
					AddCustomTableToConfiguration(connection, customTable);
				}

				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					var sqlText = string.Format(@"
							UPDATE [{0}].StagingTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'
							UPDATE [{0}].TransformTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'
							UPDATE [{0}].ModelTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'
							UPDATE [{0}].CustomTableState SET InitialLoadRequired = 0, CurrentState = 'Idle'
							WHERE ModelTableName <> 'CUS__UnitTestTable1'", BiConstants.BiAdminSchemaName);
					connection.ExecuteNonQuery(sqlText);
				}

				CombineAssertions(() =>
				{
					AssertInitialLoadRequiredForCustomTable(connection, "CUS__UnitTestTable1", true);
					AssertInitialLoadRequiredForCustomTable(connection, "CUS__UnitTestTable2", false);
					AssertInitialLoadRequiredForCustomTable(connection, "SUP__UnitTestTable1", false);
				});

				#region Initial Load

				var scanner = new CdcScannerForEdwTest();
				var logger = new LoggerForTest();

				var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				CombineAssertions(() =>
				{
					AssertInitialLoadRequiredForCustomTable(connection, "CUS__UnitTestTable1", true);
					AssertInitialLoadRequiredForCustomTable(connection, "CUS__UnitTestTable2", true);
					AssertInitialLoadRequiredForCustomTable(connection, "SUP__UnitTestTable1", true);
				});

				#endregion
			}
		}

		public void TestCustomTablesDependencyLoopDirectReference()
		{
			var configData = BiAutomationConfigLoader.Instance.ConfigData;

			var customTable1 = configData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", "CUS__UnitTestTable1", -1, "", "", "SELECT * FROM [Test].[CUS__UnitTestTable3]", "", false, "");
			var customTable2 = configData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", "CUS__UnitTestTable2", -1, "", "", "SELECT * FROM [Test].[CUS__UnitTestTable1]", "", false, "");
			var customTable3 = configData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", "CUS__UnitTestTable3", -1, "", "", "SELECT * FROM [Test].[CUS__UnitTestTable2]", "", false, "");

			try
			{
				configData.SortModelDependencyOrder();
				Fail("Expected a BiConfigurationException for dependency loop.");
			}
			catch (BiConfigurationException ex)
			{
				AssertEquals("[CUS__UnitTestTable1] and [CUS__UnitTestTable2] have circular dependencies. Check the parent tables to resolve the issue.", ex.Message);
			}
		}

		public void TestCustomTablesDependencyLoopIndirectReference()
		{
			var configData = BiAutomationConfigLoader.Instance.ConfigData;

			var customTable1 = configData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", "SUP__UnitTestTable1", -1, "", "", "", "", false, "");
			var customTable2 = configData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", "CUS__UnitTestTable1", -1, "", "", "TRUNCATE TABLE [Test].[SUP__UnitTestTable1]; SELECT * FROM [Test].[CUS__UnitTestTable2]", "", false, "");
			var customTable3 = configData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", "CUS__UnitTestTable2", -1, "", "", "SELECT * FROM [Test].[SUP__UnitTestTable1]", "", false, "");

			try
			{
				configData.SortModelDependencyOrder();
				Fail("Expected a BiConfigurationException for dependency loop.");
			}
			catch (BiConfigurationException ex)
			{
				AssertEquals("[SUP__UnitTestTable1] and [CUS__UnitTestTable2] have circular dependencies. Check the parent tables to resolve the issue.", ex.Message);
			}
		}

		#endregion

		#region UsesFunctions

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestUsesFunctions()
		{
			configData = new BiConfigurationDataForTest();

			try
			{
				using (var connection = Db.NewAdminConnection())
				{
					EnsureCdcIsEnabled(connection, Db.DatabaseName);

					ClearConfiguration(connection);

					CreateConvertDateToStringFunction(connection);
					CreateTestTableThatUsesFunction_New(connection, configData);

					#region Initial Load

					var scanner = new CdcScannerForEdwTest();
					var logger = new LoggerForTest();

					var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();
					InsertRowToTestTable(connection, "TestTable", 1, new DateTime(2018, 1, 1));
					InsertRowToTestTable(connection, "TestTable", 2, new DateTime(2018, 6, 15));
					InsertRowToTestTable(connection, "TestTable", 3, new DateTime(2018, 12, 31));
					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					CombineAssertions(() =>
					{
						AssertTestTableCount(connection, "BAS__TestTable", 3);
						AssertTestTableValues(connection, "BAS__TestTable", "20180101");
						AssertTestTableValues(connection, "BAS__TestTable", "20180615");
						AssertTestTableValues(connection, "BAS__TestTable", "20181231");
					});

					#endregion

					#region Incremental Load

					InsertRowToTestTable(connection, "TestTable", 4, new DateTime(2018, 8, 6));
					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					CombineAssertions(() =>
					{
						AssertTestTableCount(connection, "BAS__TestTable", 4);
						AssertTestTableValues(connection, "BAS__TestTable", "20180101");
						AssertTestTableValues(connection, "BAS__TestTable", "20180615");
						AssertTestTableValues(connection, "BAS__TestTable", "20180806");
						AssertTestTableValues(connection, "BAS__TestTable", "20181231");
					});

					#endregion
				}
			}
			finally
			{
				configData = null;
			}
		}

		#endregion

		#region EDW Filter

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestEdwFilter()
		{
			using (var connection = Db.NewAdminConnection())
			{
				configData = new BiConfigurationDataForTest();

				try
				{
					EnsureCdcIsEnabled(connection, Db.DatabaseName);
					ClearConfiguration(connection);
					CreateTestTableInEdw_New(connection, "TestTable", configData);
					var cdcTable = configData.CdcTableConfig.First(t => t.SourceTable == "TestTable");
					cdcTable.EdwFilter = "Test_String LIKE ('Included%')";

					using (((ICurrentDbControl)connection).UseDatabase(Db.EdwDatabaseName))
					{
						var sqlText = $"UPDATE [{BiConstants.BiAdminSchemaName}].[StagingTableConfiguration] SET EdwFilter = 'Test_String LIKE (''Included%'')' WHERE StagingTableName = 'TestTable'";
						connection.ExecuteNonQuery(sqlText);
					}

					#region Initial Load

					var scanner = new CdcScannerForEdwTest();
					var logger = new LoggerForTest();

					var edwTsqlScriptRunner = new EdwTsqlScriptRunner(connection, logger);

					var pk1 = Guid.NewGuid();
					var pk2 = Guid.NewGuid();
					var pk3 = Guid.NewGuid();

					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					InsertRowToTestTable(connection, "TestTable", pk1, "Included1");
					InsertRowToTestTable(connection, "TestTable", pk2, "Included2");
					InsertRowToTestTable(connection, "TestTable", pk3, "Excluded3");

					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					CombineAssertions(() =>
					{
						AssertTestTableCount(connection, "BAS__TestTable", 2);
						AssertTestTableValues(connection, pk1, exists: true);
						AssertTestTableValues(connection, pk2, exists: true);
						AssertTestTableValues(connection, pk3, exists: false);
					});

					#endregion

					#region Incremental Load

					var pk4 = Guid.NewGuid();
					var pk5 = Guid.NewGuid();
					InsertRowToTestTable(connection, "TestTable", pk4, "Included4");
					InsertRowToTestTable(connection, "TestTable", pk5, "Excluded5");

					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					CombineAssertions(() =>
					{
						AssertTestTableCount(connection, "BAS__TestTable", 3);
						AssertTestTableValues(connection, pk1, exists: true);
						AssertTestTableValues(connection, pk2, exists: true);
						AssertTestTableValues(connection, pk3, exists: false);
						AssertTestTableValues(connection, pk4, exists: true);
						AssertTestTableValues(connection, pk5, exists: false);
					});

					#endregion
				}
				finally
				{
					configData = null;
				}
			}
		}

		#endregion

		#region Partitioning

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestBaseTablePartitioningLessThanMinimumRowSizeForPartitioning()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);
				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);
				TruncateEDWTables(connection);
				CreateTestTableInEdw(connection, "TestTable");

				var scanner = new CdcScannerForEdwTest();
				scanner.ScanUntilNoTransactionsToProcessSafe();

				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);
				edwTsqlScriptRunner.MinimumRowSizeForPartitioningForTest = 3;
				edwTsqlScriptRunner.Run();

				InsertRowToTestTable(connection, "TestTable", 1, DateTime.Now.AddMonths(-1));
				InsertRowToTestTable(connection, "TestTable", 2, DateTime.Now.AddMonths(-2));
				scanner.ScanUntilNoTransactionsToProcessSafe();

				edwTsqlScriptRunner.Run();
				CombineAssertions(() =>
				{
					AssertEETLogEquals(logger.LogEntries);
					AssertTestTableHasRows(connection, expectedRowCount: 2);
					AssertPartitionCount(connection, "Test", "BAS__TestTable", expectedPartitionCount: 1, onlyNonEmptyPartitions: true);
				});
			}
		}

		static void TruncateEDWTables(AdminConnection connection)
		{
			connection.ExecuteNonQuery($@"
TRUNCATE TABLE [{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[StagingTableConfiguration]
TRUNCATE TABLE [{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[StagingTableState]
TRUNCATE TABLE [{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[TransformTableState]
TRUNCATE TABLE [{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[TransformTableConfiguration]
TRUNCATE TABLE [{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[CustomTableConfiguration]
TRUNCATE TABLE [{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[CustomTableState]
TRUNCATE TABLE [{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[ModelTableConfiguration]
TRUNCATE TABLE [{Db.EdwDatabaseName}].[{BiConstants.BiAdminSchemaName}].[ModelTableState]
");
		}

		public static void AssertEETLogEquals(IEnumerable<string> actualLog)
		{
			var expectedLogs = @"Staging Initial Master Load
Updating staging table state.
Starting initial load to staging tables.
Staging Incremental Master Load
Setting up for incremental load.
Could not find LSN near the Initial Load Completion Time. Make sure CDC Log Reader is running and re-run this SP. Exiting...
Waiting for CDC scan in the next run
Staging Incremental Master Load
Setting up for incremental load.
Previous scan was successful, so we can start a new scan.
Scanning cdc tables for changes...
Starting incremental load to staging tables.
Loaded Test.TestTable
Total Scanned: 1
All tables have been synchronised after Initital Load. We need to activate Merge Process.
Merging Incremental Changes To Initial Load
Transform Master Load
First custom table transform starting.
Base table transform starting.
Aggregate table transform starting.
Second custom table transform starting.
Cleanup staging starting.
Processed 2 row(s) for initial load
Top 1 table(s)
[Test].[BAS__TestTable] TransformId: 1 Insert(2)
";
			string actualLogSingleString = string.Join("\r\n", actualLog);
			AssertEquals(
				"Actual Output Log:\r\n\r\n" + actualLogSingleString + "\r\n",
				expectedLogs, actualLogSingleString
			);
		}

		void AssertTestTableHasRows(AdminConnection connection, int expectedRowCount)
		{
			var sqlText = $@"
				SELECT COUNT(*) FROM {EdwDbName}.Test.BAS__TestTable
			";
			AssertEquals("The EDW test table should have two rows", expectedRowCount, connection.ExecuteScalar<int>(sqlText));
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestBaseTablePartitioningMinimumRowSizeForPartitioning()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);
				TruncateEDWTables(connection);
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);
				CreateTestTableInEdw(connection, "TestTable");

				var scanner = new CdcScannerForEdwTest();
				scanner.ScanUntilNoTransactionsToProcessSafe();

				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);
				edwTsqlScriptRunner.MinimumRowSizeForPartitioningForTest = 3;
				edwTsqlScriptRunner.FuturePeriodsForPartitioningForTest = 1;
				edwTsqlScriptRunner.Run();

				InsertRowToTestTable(connection, "TestTable", 1, DateTime.Now.AddMonths(-1));
				InsertRowToTestTable(connection, "TestTable", 2, DateTime.Now.AddMonths(-2));
				InsertRowToTestTable(connection, "TestTable", 3, DateTime.Now.AddMonths(-3));
				scanner.ScanUntilNoTransactionsToProcessSafe();

				edwTsqlScriptRunner.Run();
				AssertPartitionCount(connection, "Test", "BAS__TestTable", expectedPartitionCount: 3, onlyNonEmptyPartitions: true);

				// Reinitialize
				connection.ExecuteNonQuery("DELETE FROM [Test].[TestTable]");
				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					connection.ExecuteNonQuery("TRUNCATE TABLE [Test].[BAS__TestTable]");
				}
				testHelper.TruncateTables(connection);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				InsertRowToTestTable(connection, "TestTable", 1, DateTime.Now.AddMonths(-1));
				InsertRowToTestTable(connection, "TestTable", 2, DateTime.Now.AddMonths(-2));
				scanner.ScanUntilNoTransactionsToProcessSafe();

				edwTsqlScriptRunner.Run();
				AssertPartitionCount(connection, "Test", "BAS__TestTable", expectedPartitionCount: 1, onlyNonEmptyPartitions: true);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestBaseTablePartitioningAfterRemovingPartitionKey()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);
				TruncateEDWTables(connection);
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);
				CreateTestTableInEdw(connection, "TestTable");

				var scanner = new CdcScannerForEdwTest();
				scanner.ScanUntilNoTransactionsToProcessSafe();

				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);
				edwTsqlScriptRunner.MinimumRowSizeForPartitioningForTest = 3;
				edwTsqlScriptRunner.FuturePeriodsForPartitioningForTest = 1;
				edwTsqlScriptRunner.Run();

				InsertRowToTestTable(connection, "TestTable", 1, DateTime.Now.AddMonths(-1));
				InsertRowToTestTable(connection, "TestTable", 2, DateTime.Now.AddMonths(-2));
				InsertRowToTestTable(connection, "TestTable", 3, DateTime.Now.AddMonths(-3));
				scanner.ScanUntilNoTransactionsToProcessSafe();

				edwTsqlScriptRunner.Run();
				AssertPartitionCount(connection, "Test", "BAS__TestTable", expectedPartitionCount: 3, onlyNonEmptyPartitions: true);

				// Reinitialize
				connection.ExecuteNonQuery("DELETE FROM [Test].[TestTable]");
				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					connection.ExecuteNonQuery("TRUNCATE TABLE [Test].[BAS__TestTable]");
					UpdateTableConfiguration(connection);
				}
				testHelper.TruncateTables(connection);
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				InsertRowToTestTable(connection, "TestTable", 1, DateTime.Now.AddMonths(-1));
				InsertRowToTestTable(connection, "TestTable", 2, DateTime.Now.AddMonths(-2));
				InsertRowToTestTable(connection, "TestTable", 3, DateTime.Now.AddMonths(-3));
				scanner.ScanUntilNoTransactionsToProcessSafe();

				edwTsqlScriptRunner.Run();
				AssertPartitionCount(connection, "Test", "BAS__TestTable", expectedPartitionCount: 1, onlyNonEmptyPartitions: true);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestBaseTablePartitioningFuturePeriods()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DisableCdc(connection);
				CdcDatabase.Enable(connection, Db.DatabaseName);
				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);
				TruncateEDWTables(connection);
				CreateTestTableInEdw(connection, "TestTable");

				var scanner = new CdcScannerForEdwTest();
				scanner.ScanUntilNoTransactionsToProcessSafe();

				var logger = new LoggerForTest();
				var edwTsqlScriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);
				edwTsqlScriptRunner.MinimumRowSizeForPartitioningForTest = 3;
				edwTsqlScriptRunner.FuturePeriodsForPartitioningForTest = 0;
				edwTsqlScriptRunner.Run();

				InsertRowToTestTable(connection, "TestTable", 1, DateTime.Now.AddMonths(-1));
				InsertRowToTestTable(connection, "TestTable", 2, DateTime.Now.AddMonths(-2));
				InsertRowToTestTable(connection, "TestTable", 3, DateTime.Now.AddMonths(-3));
				scanner.ScanUntilNoTransactionsToProcessSafe();

				edwTsqlScriptRunner.Run();
				AssertPartitionCount(connection, "Test", "BAS__TestTable", expectedPartitionCount: 6, onlyNonEmptyPartitions: false);

				// Reinitialize
				connection.ExecuteNonQuery("DELETE FROM [Test].[TestTable]");
				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					connection.ExecuteNonQuery("TRUNCATE TABLE [Test].[BAS__TestTable]");
				}
				testHelper.TruncateTables(connection);
				edwTsqlScriptRunner.FuturePeriodsForPartitioningForTest = 6;
				scanner.ScanUntilNoTransactionsToProcessSafe();
				edwTsqlScriptRunner.Run();

				InsertRowToTestTable(connection, "TestTable", 1, DateTime.Now.AddMonths(-1));
				InsertRowToTestTable(connection, "TestTable", 2, DateTime.Now.AddMonths(-2));
				InsertRowToTestTable(connection, "TestTable", 3, DateTime.Now.AddMonths(-3));
				scanner.ScanUntilNoTransactionsToProcessSafe();

				edwTsqlScriptRunner.Run();
				AssertPartitionCount(connection, "Test", "BAS__TestTable", expectedPartitionCount: 12, onlyNonEmptyPartitions: false);
			}
		}

		[UseSnapshotProtection]
		[RequiresLargeDatabase(databaseNameSuffix: Db.EdwDatabaseSuffix)]
		public void TestAggregateTablePartitioning()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					DisableCdc(connection);
					EnableCdc(connection);
					CreateTestTableInEdw(connection, "TestTable");
					CreateAggregateTestTable(connection, "BAS__TestTable", "AGG__TestTable");

					var scanner = new CdcScannerForEdwTest();
					scanner.ScanUntilNoTransactionsToProcessSafe();

					var logger = new LoggerForTest();
					var edwTsqlScriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);
					edwTsqlScriptRunner.Run();

					InsertRowToTestTable(connection, "TestTable", 1, DateTime.Now.AddMonths(-1));
					InsertRowToTestTable(connection, "TestTable", 2, DateTime.Now.AddMonths(-2));
					InsertRowToTestTable(connection, "TestTable", 3, DateTime.Now.AddMonths(-3));
					scanner.ScanUntilNoTransactionsToProcessSafe();

					edwTsqlScriptRunner.Run();
					AssertPartitionCount(connection, "Test", "AGG__TestTable", expectedPartitionCount: 3, onlyNonEmptyPartitions: true);
				}
				catch (SqlException ex) when (ex.Message.Contains("Invalid object name 'cdc.lsn_time_mapping'."))
				{
					var errorMessages = new StringBuilder();
					for (int i = 0; i < ex.Errors.Count; i++)
					{
						errorMessages.Append("Index #" + i + "\n" +
							"Message: " + ex.Errors[i].Message + "\n" +
							"Error Number: " + ex.Errors[i].Number + "\n" +
							"LineNumber: " + ex.Errors[i].LineNumber + "\n" +
							"Source: " + ex.Errors[i].Source + "\n" +
							"Procedure: " + ex.Errors[i].Procedure + "\n");
					}
					Fail(errorMessages.ToString() + "\n\nStack Trace: " + ex.StackTrace);
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestAggregateTablePartitioningFuturePeriods()
		{
			using (var connection = Db.NewAdminConnection())
			{
				configData = new BiConfigurationDataForTest();

				try
				{
					var testHelper = new EdwTsqlScriptTestHelper();
					testHelper.TruncateTables(connection);
					TruncateEDWTables(connection);
					EnsureCdcIsEnabled(connection, Db.DatabaseName);
					CreateTestTableInEdw_New(connection, "TestTable", configData);
					CreateAggregateTestTable_New(connection, "BAS__TestTable", "AGG__TestTable", configData);

					var scanner = new CdcScannerForEdwTest();
					scanner.ScanUntilNoTransactionsToProcessSafe();

					var logger = new LoggerForTest();
					var edwTsqlScriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);
					edwTsqlScriptRunner.MinimumRowSizeForPartitioningForTest = 3;
					edwTsqlScriptRunner.FuturePeriodsForPartitioningForTest = 0;
					edwTsqlScriptRunner.Run();

					InsertRowToTestTable(connection, "TestTable", 1, DateTime.Now.AddMonths(-1));
					InsertRowToTestTable(connection, "TestTable", 2, DateTime.Now.AddMonths(-2));
					InsertRowToTestTable(connection, "TestTable", 3, DateTime.Now.AddMonths(-3));
					scanner.ScanUntilNoTransactionsToProcessSafe();

					edwTsqlScriptRunner.Run();
					AssertPartitionCount(connection, "Test", "AGG__TestTable", expectedPartitionCount: 6, onlyNonEmptyPartitions: false);

					// Reinitialize
					connection.ExecuteNonQuery("DELETE FROM [Test].[TestTable]");
					using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
					{
						connection.ExecuteNonQuery("TRUNCATE TABLE [Test].[BAS__TestTable]");
					}
					edwTsqlScriptRunner.FuturePeriodsForPartitioningForTest = 6;
					scanner.ScanUntilNoTransactionsToProcessSafe();
					edwTsqlScriptRunner.Run();

					InsertRowToTestTable(connection, "TestTable", 1, DateTime.Now.AddMonths(-1));
					InsertRowToTestTable(connection, "TestTable", 2, DateTime.Now.AddMonths(-2));
					InsertRowToTestTable(connection, "TestTable", 3, DateTime.Now.AddMonths(-3));
					scanner.ScanUntilNoTransactionsToProcessSafe();

					edwTsqlScriptRunner.Run();
					AssertPartitionCount(connection, "Test", "AGG__TestTable", expectedPartitionCount: 12, onlyNonEmptyPartitions: false);
				}
				finally
				{
					configData = null;
				}
			}
		}

		void UpdateTableConfiguration(DbConnection connection)
		{
			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var configData = BiAutomationConfigLoader.Instance.ConfigData;
				var edwTable = configData.EdwTableConfig.First(t => t.Name == "BAS__TestTable");
				var partitionKey = edwTable.GetEdwColumnConfigRows().First(c => c.IsPartitionKey);
				partitionKey.IsPartitionKey = false;
				var sqlText = string.Format(@"
UPDATE [{0}].[TransformTableConfiguration]
SET
	InitialLoadQuery = N'{1}',
	IncrementalInsertQuery = N'{2}',
	IncrementalDeleteQuery = N'{3}',
	OltpPartitionColumnExpression = '',
	EdwPartitionColumnName = ''	
Where ModelTableName = 'BAS__TestTable'
",
					BiConstants.BiAdminSchemaName,
					configData.GetInitialLoadQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalInsertQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalDeleteQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")
				);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void AssertPartitionCount(DbConnection connection, string schemaName, string tableName, int expectedPartitionCount, bool onlyNonEmptyPartitions)
		{
			var sqlText = string.Format(@"Select count(*)
From sys.tables t
	Inner Join sys.schemas s On t.schema_id = s.schema_id
	Inner Join sys.partitions p on p.object_id = t.object_id
Where p.index_id In (0, 1) and s.name = @SchemaName and t.name = @TableName{0}",
				onlyNonEmptyPartitions ? " and p.Rows > 0" : "");

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@SchemaName", SqlDbType.VarChar, 128, schemaName);
				cmd.AddParameter("@TableName", SqlDbType.VarChar, 128, tableName);
				var partitionCount = Convert.ToInt32(cmd.ExecuteScalar());

				AssertEquals("Partition count?", expectedPartitionCount, partitionCount);
			}
		}

		#endregion

		#region BIM Nudge Time

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestShouldRunEetWhenBimIsNudged()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);
				BiMasterState.SetParameter(connection, BiConstants.BimNudgeTimeParamName, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));

				var logger = new LoggerForTest();
				var scriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);

				AssertEquals("Should run ETL?", false, scriptRunner.ShouldRunEtl());
				Assert(string.Join("\r\n", logger.LogEntries), logger.LogEntries.Contains("BIM is nudged. Skipping ETL execution until BIM is finished or nudge time has expired."));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestShouldRunEetWhenNudgeTimeHasExpired()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);
				BiMasterState.SetParameter(connection, BiConstants.BimNudgeTimeParamName, DateTime.UtcNow.AddMinutes(-15).ToString(CultureInfo.InvariantCulture));

				var logger = new LoggerForTest();
				var scriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);

				AssertEquals("Should run ETL?", true, scriptRunner.ShouldRunEtl());
				Assert(string.Join("\r\n", logger.LogEntries), logger.LogEntries.Contains("BIM Nudge Time has expired. Running ETL."));

				BiMasterState.GetParameter(connection, BiConstants.BimNudgeTimeParamName);
				AssertNull(BiConstants.BimNudgeTimeParamName, BiMasterState.GetParameterDate(connection, BiConstants.BimNudgeTimeParamName));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestShouldRunEetWhenBimIsInactive()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);

				var logger = new LoggerForTest();
				var scriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);
				AssertEquals("Should run ETL?", true, scriptRunner.ShouldRunEtl());
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestShouldRunEetWhenBimIsActiveAndIsScheduledAndNoPreviousIndexRebuild()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				InsertBimSchedule(connection, true, DateTime.UtcNow.AddHours(-1));
				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);

				var logger = new LoggerForTest();
				var scriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);

				AssertEquals("Should run ETL?", false, scriptRunner.ShouldRunEtl());
				Assert(string.Join("\r\n", logger.LogEntries), logger.LogEntries.Contains("Nudging BIM Service Task"));

				AssertNotNull(BiConstants.BimNudgeTimeParamName, BiMasterState.GetParameterDate(connection, BiConstants.BimNudgeTimeParamName));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestShouldRunEetWhenBimIsActiveAndIsScheduledAndNoPreviousLastIndexRebuildMoreThan12HoursAgo()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				InsertBimSchedule(connection, true, DateTime.UtcNow.AddHours(-1));
				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);
				BiMasterState.SetParameter(connection, BiConstants.LastIndexRebuildUtcDt, DateTime.UtcNow.AddHours(-12).ToString(CultureInfo.InvariantCulture));

				var logger = new LoggerForTest();
				var scriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);

				AssertEquals("Should run ETL?", false, scriptRunner.ShouldRunEtl());
				Assert(string.Join("\r\n", logger.LogEntries), logger.LogEntries.Contains("Nudging BIM Service Task"));

				AssertNotNull(BiConstants.BimNudgeTimeParamName, BiMasterState.GetParameterDate(connection, BiConstants.BimNudgeTimeParamName));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestShouldRunEetWhenBimIsActiveAndIsScheduledAndNoPreviousLastIndexRebuildLessThan12HoursAgo()
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				InsertBimSchedule(connection, true, DateTime.UtcNow.AddHours(-1));
				var testHelper = new EdwTsqlScriptTestHelper();
				testHelper.TruncateTables(connection);
				BiMasterState.SetParameter(connection, BiConstants.LastIndexRebuildUtcDt, DateTime.UtcNow.AddHours(-11).ToString(CultureInfo.InvariantCulture));

				var logger = new LoggerForTest();
				var scriptRunner = new EdwTsqlScriptRunnerForTest(connection, logger);

				AssertEquals("Should run ETL?", true, scriptRunner.ShouldRunEtl());
			}
		}

		#endregion

		#region Implementation

		string EdwDbName
		{
			get
			{
				return Db.EdwDatabaseName;
			}
		}

		#region ETL dates

		void SetIncorrectTableConfiguration(DbConnection connection)
		{
			var sqlText = string.Format(@"UPDATE [{0}].[{1}].StagingTableConfiguration SET PkName = 'PK'", EdwDbName, BiConstants.BiAdminSchemaName);
			connection.ExecuteNonQuery(sqlText);
		}

		DateTime GetParamValue(DbConnection connection, string paramName)
		{
			var result = DateTime.MinValue;
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
DECLARE @paramValue varchar(max)
EXEC [{0}].[{1}].usp_GetMasterStateParameter @ParamName = '{2}', @ParamValue = @paramValue OUTPUT
SELECT CONVERT(DATETIME, @paramValue, 121)", EdwDbName, BiConstants.BiAdminSchemaName, paramName);

			var paramValueObj = connection.ExecuteScalar(sqlText);
			if (paramValueObj != DBNull.Value)
			{
				result = Convert.ToDateTime(paramValueObj, CultureInfo.InvariantCulture);
			}

			return result;
		}

		#endregion

		#region Assertions

		void AssertModelTables(DbConnection connection, string schema)
		{
			CombineAssertions(@"Model tables failed to map source columns",
				delegate
				{
					foreach (var baseTable in EdwBaseTableList.Where(t => t.Schema.Equals(schema)))
					{
						CheckBaseTable(connection, baseTable);
					}

					foreach (var aggregateTable in EdwAggregateTableList.Where(t => t.Schema.Equals(schema)))
					{
						CheckAggregateTable(connection, aggregateTable);
					}
				});
		}

		void CheckBaseTable(DbConnection connection, BiAutomationConfigDataSet.EdwTableConfigRow baseTable)
		{
			var stagingTable = CdcConfigTables.FirstOrDefault(t => t.SourceSchema == baseTable.SourceSchema && t.SourceTable == baseTable.StagingTable);
			if (stagingTable == null)
			{
				Fail($"Staging table [{baseTable.StagingTable}] not found for base table [{baseTable.Schema}].[{baseTable.Name}].");
				return;
			}

			var filters = new List<string>();
			if (!string.IsNullOrEmpty(stagingTable.EdwFilter))
			{
				filters.Add(stagingTable.EdwFilter);
			}
			if (!string.IsNullOrEmpty(baseTable.WhereClause))
			{
				filters.Add(baseTable.WhereClause);
			}

			var countQuery = @"SELECT COUNT(*) FROM [{0}].[{1}].[{2}]{3}";
			var stagingTableCount = Convert.ToInt32(connection.ExecuteScalar(string.Format(countQuery, Db.DatabaseName, stagingTable.SourceSchema, stagingTable.SourceTable, filters.Any() ? " WHERE (" + string.Join(") AND (", filters) + ")" : null)));
			var modelTableCount = Convert.ToInt32(connection.ExecuteScalar(string.Format(countQuery, EdwDbName, baseTable.Schema, baseTable.Name, null)));
			AssertEquals($"Staging table ([{stagingTable.SourceSchema}].[{stagingTable.SourceTable}]) and model table ([{baseTable.Schema}].[{baseTable.Name}]) count should match.", stagingTableCount, modelTableCount);

			var edwColumns = baseTable.GetEdwColumnConfigRows().Where(c => string.IsNullOrEmpty(c.ParentColumn));
			var columnComparisonList = new List<string>();
			foreach (var edwColumn in edwColumns.Where(c => c.Name != baseTable.BaseName + "Key" && !c.UsesFunction))
			{
				var expression = edwColumn.Expression;
				foreach (var column in stagingTable.GetOrderedCdcColumnConfigRows().Where(c => c.ColumnInEdw).Select(c => c.SourceColumn))
				{
					var columnRegex = new Regex($"\\[?\\b{column}\\b\\]?(?!')", RegexOptions.IgnoreCase);
					expression = columnRegex.Replace(expression, $"[st].[{column}]");
				}
				columnComparisonList.Add(string.Format("[mt].[{0}] <> {1}", edwColumn.Name, expression));
			}

			if (columnComparisonList.Any())
			{
				var query = string.Format(
	@"SELECT COUNT(*)
FROM [{0}].[{1}].[{2}] [mt]
JOIN [{3}].[{4}].[{5}] [st]
	on [mt].[{6}] = [st].[{7}]
WHERE {8}",
						EdwDbName, baseTable.Schema, baseTable.Name,
						Db.DatabaseName, stagingTable.SourceSchema, stagingTable.SourceTable,
						baseTable.BaseName + "ID",
						stagingTable.GetCdcColumnConfigRows().First(c => c.IsPrimaryKey).SourceColumn,
						string.Join(" OR\r\n\t", columnComparisonList));
				int modelTableRows = -1;
				using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
				{
					try
					{
						var result = connection.ExecuteScalar(query);
						if (result != null && result != DBNull.Value)
						{
							modelTableRows = (int)result;
						}
					}
					catch (SqlException ex)
					{
						Fail(ex.Message + "\r\n" + query);
					}
				}

				AssertEquals(string.Format("[{0}] model table rows should match the expression output.", baseTable.Name), 0, Convert.ToInt32(modelTableRows));
			}
		}

		void CheckAggregateTable(DbConnection connection, BiAutomationConfigDataSet.EdwDenormalizedTableConfigRow aggregateTable)
		{
			var edwColumns = aggregateTable.GetEdwDenormalizedColumnConfigRows().Where(c => c.Name != aggregateTable.BaseName + "Key").Select(c => c.Name);
			var columnList = string.Join("], [", edwColumns);
			var countQuery = @"SELECT ISNULL(SUM(CAST(CHECKSUM([{3}]) AS BIGINT)),0) FROM [{0}].[{1}].[{2}]";
			var viewCheckSum = Convert.ToInt64(connection.ExecuteScalar(string.Format(countQuery, EdwDbName, aggregateTable.Schema, BiConstants.EdwAggregateViewPrefix + aggregateTable.Name, columnList)));
			var aggregateTableCheckSum = Convert.ToInt64(connection.ExecuteScalar(string.Format(countQuery, EdwDbName, aggregateTable.Schema, aggregateTable.Name, columnList)));
			AssertEquals(string.Format("[{0}] View and Aggregate table checksum should match.", aggregateTable.Name), viewCheckSum, aggregateTableCheckSum);
		}

		void CheckIndex(DbConnection connection, string schema)
		{
			var indexes = new List<string>();

			foreach (var t in BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.Where(t => t.Schema.Equals(schema)))
			{
				indexes.AddRange(EdwIndexForBaseTable[t.Name].Select(c => GetIndexName(schema, t.Name, c.Name)));
				if (!string.IsNullOrEmpty(t.CustomIndex) && TryGetIndexNamesFromScript(t.CustomIndex, out var indexNames))
				{
					indexes.AddRange(indexNames);
				}
			}

			foreach (var t in EdwAggregateTableList.Where(t => t.Schema.Equals(schema)))
			{
				indexes.AddRange(EdwIndexForAggTable[t.Name].Select(c => GetIndexName(schema, t.Name, c.Name)));
				if (!string.IsNullOrEmpty(t.CustomIndex) && TryGetIndexNamesFromScript(t.CustomIndex, out var indexNames))
				{
					indexes.AddRange(indexNames);
				}
			}

			foreach (var t in EdwCustomTableList.Where(t => t.Schema.Equals(schema)))
			{
				if (!string.IsNullOrEmpty(t.CustomIndex) && TryGetIndexNamesFromScript(t.CustomIndex, out var indexNames))
				{
					indexes.AddRange(indexNames);
				}
			}

			var sql = @"
SELECT 
	i.name as iName
FROM sys.indexes i 
	INNER JOIN sys.objects o ON i.object_id = o.object_id 
	INNER JOIN sys.schemas s ON o.schema_id = s.schema_id

WHERE o.type <> 'S' AND is_primary_key <> 1 AND i.index_id > 0 and i.is_disabled = 0 and i.type = 2
and s.name = @schemaName";

			var actual = new List<string>();
			using ((connection as ICurrentDbControl).UseDatabase(Db.EdwDatabaseName))
			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@schemaName", SqlDbType.VarChar, 100, schema);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						actual.Add(reader["iName"].ToString());
					}
				}
			}
			AssertContainsExactElementsInAnyOrder("Indexes do not match the current config.", indexes.Distinct(), actual.Distinct());
		}

		bool TryGetIndexNamesFromScript(string indexQuery, out List<string> indexNames)
		{
			indexNames = new List<string> { };
			var pattern = @"CREATE\s.*\sINDEX\s\[?(?<indexName>.*)\]?\sON\s\[?(?<schemaName>\w+)\]?\.\[?(?<tableName>\w+)\]?\s";
			var matches = Regex.Matches(indexQuery, pattern, RegexOptions.IgnoreCase);

			foreach (System.Text.RegularExpressions.Match match in matches)
			{
				string indexName = match.Result("${indexName}").Replace("[", "").Replace("]", "");
				indexNames.Add(indexName);
			}
			return indexNames.Count > 0;
		}

		string GetIndexName(string schema, string table, string column)
		{
			return $"IX_{schema}_{table}_{column}";
		}

		void AssertUnprocessedPartitionDate(DbConnection connection, string message, string schemaName, string tableName, DateTime? partitionDate, bool exists = true)
		{
			var sqlText = string.Format(@"SELECT COUNT(*) FROM [{0}].[SsasPartitionUnprocessedDate] WHERE SchemaName = @SchemaName AND TableName = @TableName AND CreateDate {1}",
				BiConstants.BiAdminSchemaName,
				(partitionDate != null) ? "= @CreateDate" : "IS NULL");

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@SchemaName", SqlDbType.VarChar, 800, schemaName);
				cmd.AddParameter("@TableName", SqlDbType.VarChar, 800, tableName);

				if (partitionDate != null)
				{
					cmd.AddParameter("@CreateDate", SqlDbType.SmallDateTime, partitionDate);
				}

				var count = Convert.ToInt32(cmd.ExecuteScalar());
				AssertEquals(message, exists ? 1 : 0, count);
			}
		}

		void AssertTestTableKeys(DbConnection connection, string tableName, int key, bool exists = true)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT NULL FROM [Test].[{0}] WHERE {2}Key = {1}) SELECT 1 ELSE SELECT 0", tableName, key, tableName.Replace(BiConstants.EdwBaseTablePrefix, ""));

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				AssertEquals(string.Format("[{0}] does not contain row with TestTableKey: {1}", tableName, key), exists, Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertTestTableValues(DbConnection connection, string tableName, int parentKey, int value)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT NULL FROM [Test].[{0}] WHERE {3}Key = {1} AND Value = {2}) SELECT 1 ELSE SELECT 0", tableName, parentKey, value, tableName.Replace(BiConstants.EdwBaseTablePrefix, ""));

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				Assert(string.Format("[{0}] does not contain row with TestTableKey: {1} and Value: {2}", tableName, parentKey, value), Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertTestTableValues(DbConnection connection, string tableName, string convertedDate)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT NULL FROM [Test].[{0}] WHERE ConvertedDate = '{1}') SELECT 1 ELSE SELECT 0", tableName, convertedDate);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				Assert(string.Format("[{0}] does not contain row with  ConvertedDate: '{1}'", tableName, convertedDate), Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertTestTableValues(DbConnection connection, Guid pk, int value)
		{
			var sqlText = @"IF EXISTS (SELECT NULL FROM [Test].[BAS__TestTable] WHERE TestTableID = @pk AND Value = @value) SELECT 1 ELSE SELECT 0";

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@value", SqlDbType.Int, value);

				Assert(string.Format("[Test].[BAS__TestTable] does not contain row with TestTableID: {0} and Value: {1}", pk.ToString(), value), Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		void AssertTestTableValues(DbConnection connection, Guid pk, bool exists)
		{
			var sqlText = @"IF EXISTS (SELECT NULL FROM [Test].[BAS__TestTable] WHERE TestTableID = @pk) SELECT 1 ELSE SELECT 0";

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);

				AssertEquals(string.Format("[Test].[BAS__TestTable] has row with TestTableID: {0}?", pk.ToString()), exists, Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		void AssertStagingTableCount(DbConnection connection, string tableName, int expectedCount)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"SELECT COUNT(*) FROM [Staging].[{0}]", tableName);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "[{0}] count", tableName), expectedCount, Convert.ToInt32(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertStagingTableValue(DbConnection connection, string tableName, Guid pk, string value)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
			@"IF EXISTS (SELECT NULL FROM [Staging].[{0}] WHERE Test_PK = @pk AND Test_String = @value) SELECT 1 ELSE SELECT 0",
			tableName);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@value", SqlDbType.VarChar, 128, value);

				Assert(string.Format("Staging table [{0}] row with Test_PK: {1}, Test_String = '{2}'?", tableName, pk.ToString(), value), Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		void AssertStagingTestTableValues(DbConnection connection, string tableName, Guid pk, byte[] startLsn, int opType, int count)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
			@"SELECT COUNT(*) FROM [Staging].[{0}] WHERE Test_PK = @pk AND __$start_lsn = @startLsn AND OP_TYPE = @opType",
			tableName);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@startLsn", SqlDbType.Binary, 10, startLsn);
				cmd.AddParameter("@opType", SqlDbType.TinyInt, opType);

				AssertEquals(string.Format("Staging table [{0}] row with Test_PK: {1}, __$start_lsn = {2}, OP_TYPE: {3}?", tableName, pk.ToString(), startLsn, opType), count, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		void AssertAggregateTestTableKeys(DbConnection connection, string tableName, int parentKey, bool exists = true)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT NULL FROM [Test].[{0}] WHERE {2}Key = {1}) SELECT 1 ELSE SELECT 0", tableName, parentKey, tableName.Replace(BiConstants.EdwAggregateTablePrefix, ""));

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				AssertEquals(string.Format("[{0}] contains row with TestTableKey: {1}?", tableName, parentKey), exists, Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertAggregateTestTableValues(DbConnection connection, string tableName, int parentKey, int value)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT NULL FROM [Test].[{0}] WHERE {3}Key = {1} AND Value = {2}) SELECT 1 ELSE SELECT 0", tableName, parentKey, value, tableName.Replace(BiConstants.EdwAggregateTablePrefix, ""));

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				Assert(string.Format("[{0}] contains row with TestTableKey: {1} and Value: {2}?", tableName, parentKey, value), Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertTestTableCount(DbConnection connection, string tableName, int expectedCount)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"SELECT COUNT(*) FROM [Test].[{0}]", tableName);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "[{0}] count", tableName), expectedCount, Convert.ToInt32(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertSelfReferencedTestTableValues(DbConnection connection, Guid tableId, Guid? parentId, int transformId = 1)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"IF EXISTS
	(SELECT NULL
		FROM [Test].[BAS__TestTable] t1
		LEFT JOIN [Test].[BAS__TestTable] t2
			ON t1.[ParentKey] = t2.[TestTableKey] AND t1.[__$transform_id] = t2.[__$transform_id]
		WHERE t1.[TestTableID] = '{0}' AND t2.[TestTableID] {1} AND t1.[__$transform_id] = {2})
	SELECT 1 ELSE SELECT 0",
					tableId.ToString(),
					(parentId == null) ? "IS NULL" : "= '" + parentId.Value + "'",
					transformId);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				Assert(string.Format("[BAS__TestTable] does not contain row with TestTableID: {0}, ParentID: {1}, TransformId: {2}", tableId, (parentId == null) ? "NULL" : parentId.Value.ToString(), transformId), Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertMultiSelfReferencedTestTableValues(DbConnection connection, Guid tableId, Guid? parentId1, Guid? parentId2, bool exists = true)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"IF EXISTS
	(SELECT NULL
		FROM [Test].[BAS__TestTable] t1
		LEFT JOIN [Test].[BAS__TestTable] t2
			ON t1.[ParentKey1] = t2.[TestTableKey]
		LEFT JOIN [Test].[BAS__TestTable] t3
			ON t1.[ParentKey2] = t3.[TestTableKey]
		WHERE t1.[TestTableID] = '{0}' AND t2.[TestTableID] {1} AND t3.[TestTableID] {2})
	SELECT 1 ELSE SELECT 0",
					tableId.ToString(),
					(parentId1 == null) ? "IS NULL" : "= '" + parentId1.Value + "'",
					(parentId2 == null) ? "IS NULL" : "= '" + parentId2.Value + "'");

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				AssertEquals(string.Format("[BAS__TestTable] does not contain row with TestTableID: {0}, ParentID1: {1}, ParentID2: {2}", tableId, (parentId1 == null) ? "NULL" : parentId1.Value.ToString(), (parentId2 == null) ? "NULL" : parentId2.Value.ToString()), exists, Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertTestChildTableValues(DbConnection connection, Guid childPk, Guid parentPk)
		{
			var sqlText = @"
IF EXISTS (SELECT NULL FROM [Test].[BAS__TestTable] p
	INNER JOIN [Test].[BAS__TestChildTable] c
		ON p.TestTableKey = c.ParentKey
	WHERE p.TestTableID = @parentPk AND c.TestChildTableID = @childPk)
	SELECT 1 ELSE SELECT 0";

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@childPk", SqlDbType.UniqueIdentifier, childPk);
				cmd.AddParameter("@parentPk", SqlDbType.UniqueIdentifier, parentPk);

				Assert(string.Format("[Test].[BAS__TestChildTable] does not contain row with TestChildTableID: {0} and ParentID: {1}", childPk.ToString(), parentPk.ToString()), Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		void AssertInitialLoadRequiredForBaseTable(DbConnection connection, string tableName, bool expectedIsRequired)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT NULL FROM [{0}].[TransformTableState] WHERE ModelTableName = '{1}' AND InitialLoadRequired = 1 AND CurrentState = 'New') SELECT 1 ELSE SELECT 0", BiConstants.BiAdminSchemaName, tableName);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				AssertEquals(string.Format("[{0}] is for Initial Load?", tableName), expectedIsRequired, Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertInitialLoadRequiredForAggregateTable(DbConnection connection, string tableName, bool expectedIsRequired)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT NULL FROM [{0}].[ModelTableState] WHERE ModelTableName = '{1}' AND InitialLoadRequired = 1 AND CurrentState = 'ToBeRepopulated') SELECT 1 ELSE SELECT 0", BiConstants.BiAdminSchemaName, tableName);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				AssertEquals(string.Format("[{0}] is for Initial Load?", tableName), expectedIsRequired, Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertInitialLoadRequiredForCustomTable(DbConnection connection, string tableName, bool expectedIsRequired)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT NULL FROM [{0}].[CustomTableState] WHERE ModelTableName = '{1}' AND InitialLoadRequired = 1 AND CurrentState = 'New') SELECT 1 ELSE SELECT 0", BiConstants.BiAdminSchemaName, tableName);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				AssertEquals(string.Format("[{0}] is for Initial Load?", tableName), expectedIsRequired, Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertCorruptedIndexErrorForBaseTable(DbConnection connection, string tableName, bool expectedHasCorruptedIndex)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT NULL FROM [{0}].[TransformTableState] WHERE ModelTableName = '{1}' AND SqlErrorMessage = 'Index corruption is detected, requires Index Rebuild.') SELECT 1 ELSE SELECT 0", BiConstants.BiAdminSchemaName, tableName);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				AssertEquals(string.Format("[{0}] has corrupted index?", tableName), expectedHasCorruptedIndex, Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertCorruptedIndexErrorForAggregateTable(DbConnection connection, string tableName, bool expectedHasCorruptedIndex)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT NULL FROM [{0}].[ModelTableState] WHERE ModelTableName = '{1}' AND SqlErrorMessage = 'Index corruption is detected, requires Index Rebuild.') SELECT 1 ELSE SELECT 0", BiConstants.BiAdminSchemaName, tableName);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				AssertEquals(string.Format("[{0}] has corrupted index?", tableName), expectedHasCorruptedIndex, Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void AssertInitialLoadCompletedBatches(DbConnection connection, string tableName, int? expectedCompletedBatches)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
SELECT @completedBatches = NumberOfCompletedBatches FROM [{0}].[StagingTableState] WHERE SourceTableName = @table",
				BiConstants.BiAdminSchemaName);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@table", SqlDbType.VarChar, 128, "Test." + tableName);
				cmd.AddOutputParameter("@completedBatches", SqlDbType.Int, 4, 0, 0, 0);

				cmd.ExecuteNonQuery();
				var actualCompletedBatchesObj = cmd.GetParameterValue("@completedBatches");

				if (actualCompletedBatchesObj == DBNull.Value)
				{
					AssertEquals("Completed batches", expectedCompletedBatches, null);
				}
				else
				{
					AssertEquals("Completed batches", expectedCompletedBatches, (int)actualCompletedBatchesObj);
				}
			}
		}

		#endregion

		#region Row Manipulations

		void InsertBimSchedule(DbConnection connection, bool isActive, DateTime dailyStartTimeUtc)
		{
			var sqlText = @"
delete from dbo.StmScheduleTask where S5_ScheduleType = 'ADM'

insert into dbo.StmScheduleTask(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_DailyStartTime, S5_ParentTableCode, S5_ParentID, S5_IsActive)
select newid(), 'Business Intelligence Maintenance', 'BIM', 'BI', @BranchPk, 'D', @DailyStartTime, 'SH', null, @IsActive";

			var dailyStartTimeLocal = Env.Time.GetLocalTimeFromUtc(dailyStartTimeUtc);

			using (((ICurrentDbControl)connection).UseDatabase(Db.DatabaseName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@BranchPk", SqlDbType.UniqueIdentifier, Env.CurrentBranchPK);
				cmd.AddParameter("@IsActive", SqlDbType.Bit, isActive);
				cmd.AddParameter("@DailyStartTime", SqlDbType.DateTime, dailyStartTimeLocal);

				cmd.ExecuteNonQuery();
			}
		}

		public void InsertRows(DbConnection connection, bool initialLoad)
		{
			foreach (var (schema, table) in StagingTableList)
			{
				InsertRow(connection, initialLoad, schema, table);
			}
		}

		void InsertRow(DbConnection connection, bool initialLoad, string schema, string table)
		{
			var tableColumnsList = ColumnsList.Where(c => c.TableName == table && !CalculatedColumns.Contains(c.ColumnName));
			var tableColumnsToInsertList = tableColumnsList.Where(c => !PrimaryKeys.Contains(c.ColumnName));
			if (tableColumnsToInsertList.Any())
			{
				var query = string.Format(@"
ALTER TABLE {0}.{1}
NOCHECK CONSTRAINT ALL
ALTER TABLE {0}.{1}
DISABLE TRIGGER ALL
INSERT INTO {0}.{1}([{2}]) VALUES(@{3})",
					schema,
					table,
					string.Join("],[", tableColumnsList.Select(c => c.ColumnName)),
					string.Join(",@", tableColumnsList.Select(c => c.ColumnName)));

				using (var cmd = connection.Command(query))
				{
					var primaryKey = tableColumnsList.FirstOrDefault(c => PrimaryKeys.Contains(c.ColumnName));

					if (primaryKey != null)
					{
						if (initialLoad)
						{
							cmd.AddParameter(string.Format("@{0}", primaryKey.ColumnName), primaryKey.DataType, initialId);
						}
						else
						{
							cmd.AddParameter(string.Format("@{0}", primaryKey.ColumnName), primaryKey.DataType, insertedId);
						}
					}

					foreach (var column in tableColumnsToInsertList)
					{
						if (initialLoad)
						{
							cmd.AddParameter(string.Format("@{0}", column.ColumnName), column.DataType, InitialValuesDictionary[column.DataType]);
						}
						else
						{
							cmd.AddParameter(string.Format("@{0}", column.ColumnName), column.DataType, InsertedValuesDictionary[column.DataType]);
						}
					}
					cmd.ExecuteNonQuery();
				}
			}
		}

		void UpdateRows(DbConnection connection)
		{
			foreach (var (schema, table) in StagingTableList)
			{
				UpdateRow(connection, schema, table);
			}
		}

		void UpdateRow(DbConnection connection, string schema, string table)
		{
			var tableColumnsList = ColumnsList.Where(c => c.TableName == table && !CalculatedColumns.Contains(c.ColumnName));
			var tableColumnsToUpdateList = tableColumnsList.Where(c => !PrimaryKeys.Contains(c.ColumnName));
			var primaryKey = tableColumnsList.FirstOrDefault(c => PrimaryKeys.Contains(c.ColumnName));

			if (tableColumnsToUpdateList.Any() && primaryKey != null)
			{
				var query = string.Format(@"
ALTER TABLE {0}.{1}
NOCHECK CONSTRAINT ALL
ALTER TABLE {0}.{1}
DISABLE TRIGGER ALL
UPDATE {0}.{1} SET {2} WHERE {3} = @PrimaryKey",
				schema,
				table,
				string.Join(", ", tableColumnsToUpdateList.Select(c => string.Format("{0} = @{0}", c.ColumnName))),
				primaryKey.ColumnName);

				using (var cmd = connection.Command(query))
				{
					cmd.AddParameter("@PrimaryKey", primaryKey.DataType, insertedId);
					foreach (var column in tableColumnsList)
					{
						cmd.AddParameter(string.Format("@{0}", column.ColumnName), column.DataType, UpdatedValuesDictionary[column.DataType]);
					}
					cmd.ExecuteNonQuery();
				}
			}
		}

		void DeleteRows(DbConnection connection)
		{
			foreach (var (schema, table) in StagingTableList)
			{
				var primaryKey = ColumnsList.FirstOrDefault(c => c.TableName == table && PrimaryKeys.Contains(c.ColumnName));

				if (primaryKey != null)
				{
					var query = string.Format(@"
ALTER TABLE {0}
NOCHECK CONSTRAINT ALL
ALTER TABLE {0}
DISABLE TRIGGER ALL
DELETE {0} WHERE {1} = @PrimaryKey",
						table,
						primaryKey.ColumnName);

					using (var cmd = connection.Command(query))
					{
						cmd.AddParameter("@PrimaryKey", primaryKey.DataType, insertedId);
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		#endregion

		#region CDC Enabling/Disabling

		public class CdcScannerForEdwTest : CdcScanner
		{
			public CdcScannerForEdwTest()
			{
				CdcScannerLogger = new CdcScannerLoggerForTest();
			}
			public void ScanUntilNoTransactionsToProcessSafe()
			{
				try
				{
					ScanUntilNoTransactionsToProcess();
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.AnotherConnectionIsRunningSpReplcmdsForCdc)
				{
				}
			}

			public class CdcScannerLoggerForTest : CdcScannerLogger
			{
				public CdcScannerLoggerForTest()
				{
					Logs = new List<string>();
				}

				public List<string> Logs;

				public override void Log(string logMessage)
				{
					Logs.Add(logMessage);
				}

				public override void Debug(string logMessage)
				{
					Logs.Add(logMessage);
				}

				public override void Error(string logMessage)
				{
					Logs.Add(logMessage);
				}

				public override void Warning(string logMessage)
				{
					Logs.Add(logMessage);
				}
			}
		}

		IEnumerable<string> GetCdcEnabledTables(DbConnection connection)
		{
			var sqlText = "SELECT name from sys.tables where is_tracked_by_cdc = 1";
			return DataUtils.GetListOfValuesFromQuery(connection, sqlText);
		}

		void ClearConfiguration(DbConnection connection)
		{
			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
					@"TRUNCATE TABLE [{0}].[StagingTableConfiguration]
						TRUNCATE TABLE [{0}].[StagingTableState]
						TRUNCATE TABLE [{0}].[TransformTableConfiguration]
						TRUNCATE TABLE [{0}].[TransformTableState]
						TRUNCATE TABLE [{0}].[ModelTableConfiguration]
						TRUNCATE TABLE [{0}].[ModelTableState]
						TRUNCATE TABLE [{0}].[CustomTableConfiguration]
						TRUNCATE TABLE [{0}].[CustomTableState]",
					BiConstants.BiAdminSchemaName);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		public void DisableAndEnableCdcForTable(AdminConnection connection, string schema, string tableName)
		{
			DisableCdc(connection);

			var cdcTable = new CdcTableForTesting(schema, tableName);
			if (!cdcTable.IsCdcEnabled(connection))
			{
				cdcTable.EnableCdc(connection);
			}
		}

		public void EnableCdc(AdminConnection connection)
		{
			ShrinkLogFile(connection);
			if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				CdcDatabase.Enable(connection, Db.DatabaseName);
			}

			foreach (var (schema, table) in StagingTableList)
			{
				var cdcTable = new CdcTableForTesting(schema, table);
				if (!cdcTable.IsCdcEnabled(connection))
				{
					cdcTable.EnableCdc(connection);
				}
			}

			AssertEquals("Database is enabled for CDC?", true, CdcDatabase.IsEnabled(connection, Db.DatabaseName));
		}

		public void DisableCdc(AdminConnection connection)
		{
			ShrinkLogFile(connection);

			if (CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				CdcDatabase.Disable(connection, Db.DatabaseName);
			}
		}

		void ShrinkLogFile(DbConnection connection)
		{
			var logFiles = DataUtils.GetListOfValuesFromQuery(connection, "SELECT name FROM sys.database_files WHERE [type] = 1");
			string sqlText = "CHECKPOINT;" + string.Join("", logFiles.Select(lf => string.Format("DBCC SHRINKFILE ({0}, 1);", lf)));
			connection.ExecuteNonQuery(sqlText);
		}

		void InitiateInitialLoad(DbConnection connection)
		{
			var sqlText = string.Format(@"
TRUNCATE TABLE [{0}].[{1}].[MasterState]
UPDATE [{0}].[{1}].[StagingTableState]
SET CurrentMaxLsn = 0x0, CurrentState = 'New', InitialLoadRequired = 1",
				EdwDbName, BiConstants.BiAdminSchemaName);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void TruncateUnprocessedPartitionDates(DbConnection connection)
		{
			var sqlText = string.Format(@"TRUNCATE TABLE [{0}].[SsasPartitionUnprocessedDate]", BiConstants.BiAdminSchemaName);
			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void EnsureCdcIsEnabled(AdminConnection testConnection, string dbName)
		{
			if (CdcDatabase.IsEnabled(testConnection, dbName))
			{
				CdcDatabase.Disable(testConnection, dbName);
			}
			CdcDatabase.Enable(testConnection, dbName);
		}

		#endregion

		#region CDC Tables and Columns

		void CreateTestTableInEdw(AdminConnection connection, string tableName)
		{
			var configData = BiAutomationConfigLoader.Instance.ConfigData;
			configData.EnforceConstraints = false;

			var edwTable = configData.EdwTableConfig.AddEdwTableConfigRow("Test", $"BAS__{tableName}", "", "Test", tableName, "", 1, true, 1, "");
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "bigint", 0, 0, 0, $"{tableName}Key", "", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "uniqueidentifier", 16, 0, 0, $"{tableName}ID", "Test_PK", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "int", 4, 10, 0, "Value", "Test_Value", false, false, "", "", "", "", "", "", false);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "varchar", 10, 0, 0, "StrValue", "Test_String", false, false, "", "", "", "", "", "", false);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "date", 4, 16, 0, "PartitionDate", "CAST(Test_PartitionDate AS DATE)", true, false, "", "", "", "", "", "", false);

			configData.EnforceConstraints = true;

			CreateTestTableInMainDb(connection, tableName);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Test].[BAS__{4}]
(
	[__$transform_id] int,
	[{4}Key] [bigint] NOT NULL,
	[{4}ID] uniqueidentifier NOT NULL,
	[Value] int,
	[StrValue] varchar(10),
	[PartitionDate] date
)

DECLARE @maxDependencyOrder int = 0
IF EXISTS (SELECT NULL FROM [{0}].TransformTableConfiguration)
	SET @maxDependencyOrder = (SELECT MAX(DependencyOrder) FROM [{0}].TransformTableConfiguration)

INSERT INTO [{0}].[TransformTableConfiguration](SourceTableName, ModelSchemaName, ModelTableName, DependencyOrder, TransformId, IsLastTransform, HasChildTables, InitialLoadQuery, IncrementalInsertQuery, IncrementalDeleteQuery, OltpPartitionColumnExpression, EdwPartitionColumnName, CustomIndexScript)
VALUES
(
	N'Test.{4}', N'Test', N'BAS__{4}', @maxDependencyOrder + 1, 1, 1, 0, N'{1}', N'{2}', N'{3}', 'CAST(Test_PartitionDate AS DATE)', 'PartitionDate', N'{5}'
)

INSERT INTO [{0}].[TransformTableState](ModelSchemaName, ModelTableName, TransformId, CurrentState, InitialLoadRequired)
VALUES ('Test', 'BAS__{4}', 1, 'New', 1)
",
					BiConstants.BiAdminSchemaName,
					configData.GetInitialLoadQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalInsertQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalDeleteQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					tableName,
					configData.GetCustomIndexScriptQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")
				);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void CreateTestTableInEdw_New(AdminConnection connection, string tableName, BiConfigurationData configData)
		{
			configData.EnforceConstraints = false;

			var edwTable = configData.EdwTableConfig.AddEdwTableConfigRow("Test", $"BAS__{tableName}", "", "Test", tableName, "", 1, true, 1, "");
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "bigint", 0, 0, 0, $"{tableName}Key", "", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "uniqueidentifier", 16, 0, 0, $"{tableName}ID", "Test_PK", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "int", 4, 10, 0, "Value", "Test_Value", false, false, "", "", "", "", "", "", false);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "varchar", 10, 0, 0, "StrValue", "Test_String", false, false, "", "", "", "", "", "", false);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "date", 4, 16, 0, "PartitionDate", "CAST(Test_PartitionDate AS DATE)", true, false, "", "", "", "", "", "", false);

			configData.EnforceConstraints = true;

			CreateTestTableInMainDb_New(connection, tableName, configData);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Test].[BAS__{4}]
(
	[__$transform_id] int,
	[{4}Key] [bigint] NOT NULL,
	[{4}ID] uniqueidentifier NOT NULL,
	[Value] int,
	[StrValue] varchar(10),
	[PartitionDate] date
)

DECLARE @maxDependencyOrder int = 0
IF EXISTS (SELECT NULL FROM [{0}].TransformTableConfiguration)
	SET @maxDependencyOrder = (SELECT MAX(DependencyOrder) FROM [{0}].TransformTableConfiguration)

INSERT INTO [{0}].[TransformTableConfiguration](SourceTableName, ModelSchemaName, ModelTableName, DependencyOrder, TransformId, IsLastTransform, HasChildTables, InitialLoadQuery, IncrementalInsertQuery, IncrementalDeleteQuery, OltpPartitionColumnExpression, EdwPartitionColumnName, CustomIndexScript)
VALUES
(
	N'Test.{4}', N'Test', N'BAS__{4}', @maxDependencyOrder + 1, 1, 1, 0, N'{1}', N'{2}', N'{3}', 'CAST(Test_PartitionDate AS DATE)', 'PartitionDate', N'{5}'
)

INSERT INTO [{0}].[TransformTableState](ModelSchemaName, ModelTableName, TransformId, CurrentState, InitialLoadRequired)
VALUES ('Test', 'BAS__{4}', 1, 'New', 1)
",
					BiConstants.BiAdminSchemaName,
					configData.GetInitialLoadQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalInsertQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalDeleteQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					tableName,
					configData.GetCustomIndexScriptQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")
				);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void CreateTestTableInMainDb(DbConnection connection, string tableName)
		{
			var sqlText = string.Format(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Test].[{0}]
(
	Test_PK uniqueidentifier NOT NULL,
	Test_Value int,
	Test_String varchar(10),
	Test_PartitionDate smalldatetime
)
ALTER TABLE [Test].[{0}]
ADD CONSTRAINT [PK_UX__{0}_Test_PK] PRIMARY KEY NONCLUSTERED ([Test_PK] ASC)", tableName);

			connection.ExecuteNonQuery(sqlText);

			var cdcTable = new CdcTableForTesting("Test", tableName);
			cdcTable.EnableCdc(connection);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				sqlText = string.Format(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Staging].[{1}]
(
	[__$start_lsn] binary(10),
	OP_TYPE tinyint,
	Test_PK uniqueidentifier,
	Test_Value int,
	Test_String varchar(10),
	Test_PartitionDate smalldatetime
 );

INSERT INTO [{0}].[StagingTableConfiguration](SourceSchemaName, StagingSchemaName, StagingTableName, PkName, IndexedColumnName, StagingTableDefinition, StagingTableColumnListSelect, StagingTableColumnListInsert, ColumnEnumeratedList, ColumnEnumeratedListWithDefinition, EdwFilter)
VALUES
(
	N'Test', N'Staging', N'{1}', N'Test_PK', N'Test_PK', N'Test_PK uniqueidentifier, Test_Value int, Test_String varchar(10), Test_PartitionDate smalldatetime', N'Test_PK, Test_Value, Test_String, Test_PartitionDate',
	N'Test_PK, Test_Value, Test_String, Test_PartitionDate',
	N'c1, c2, c3, c4', N'c1 uniqueidentifier, c2 int, c3 varchar(10), c4 smalldatetime', ''
)

INSERT INTO [{0}].[StagingTableState](SourceTableName, CurrentMaxLsn, CurrentState, InitialLoadRequired)
VALUES ('Test.{1}', 0x0, 'New', 1)",
					BiConstants.BiAdminSchemaName,
					tableName
					);
				connection.ExecuteNonQuery(sqlText);
			}

			var configData = BiAutomationConfigLoader.Instance.ConfigData;
			var table = configData.CdcTableConfig.AddCdcTableConfigRow("Test", tableName, "Test", "", "", true, true, null, null, null, false);
			configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "Test_PK", "uniqueidentifier", 16, 0, 0, false, true, "", "", true, "", "", true, true, "");
			configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "Test_Value", "int", 4, 10, 0, true, false, "", "", true, "", "", true, true, "");
			configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "Test_String", "varchar", 10, 0, 0, true, false, "", "", true, "", "", true, true, "");
			configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "Test_PartitionDate", "smalldatetime", 0, 0, 0, true, false, "", "", true, "", "", true, true, "");
		}

		void CreateTestTableInMainDb_New(DbConnection connection, string tableName, BiConfigurationData configData)
		{
			var sqlText = string.Format(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Test].[{0}]
(
	Test_PK uniqueidentifier NOT NULL,
	Test_Value int,
	Test_String varchar(10),
	Test_PartitionDate smalldatetime
)
ALTER TABLE [Test].[{0}]
ADD CONSTRAINT [PK_UX__{0}_Test_PK] PRIMARY KEY NONCLUSTERED ([Test_PK] ASC)", tableName);

			connection.ExecuteNonQuery(sqlText);

			var cdcTable = new CdcTableForTesting("Test", tableName);
			cdcTable.EnableCdc(connection);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				sqlText = string.Format(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Staging].[{1}]
(
	[__$start_lsn] binary(10),
	OP_TYPE tinyint,
	Test_PK uniqueidentifier,
	Test_Value int,
	Test_String varchar(10),
	Test_PartitionDate smalldatetime
 );

INSERT INTO [{0}].[StagingTableConfiguration](SourceSchemaName, StagingSchemaName, StagingTableName, PkName, IndexedColumnName, StagingTableDefinition, StagingTableColumnListSelect, StagingTableColumnListInsert, ColumnEnumeratedList, ColumnEnumeratedListWithDefinition, EdwFilter)
VALUES
(
	N'Test', N'Staging', N'{1}', N'Test_PK', N'Test_PK', N'Test_PK uniqueidentifier, Test_Value int, Test_String varchar(10), Test_PartitionDate smalldatetime', N'Test_PK, Test_Value, Test_String, Test_PartitionDate',
	N'Test_PK, Test_Value, Test_String, Test_PartitionDate',
	N'c1, c2, c3, c4', N'c1 uniqueidentifier, c2 int, c3 varchar(10), c4 smalldatetime', ''
)

INSERT INTO [{0}].[StagingTableState](SourceTableName, CurrentMaxLsn, CurrentState, InitialLoadRequired)
VALUES ('Test.{1}', 0x0, 'New', 1)",
					BiConstants.BiAdminSchemaName,
					tableName
					);
				connection.ExecuteNonQuery(sqlText);
			}

			var table = configData.CdcTableConfig.AddCdcTableConfigRow("Test", tableName, "Test", "", "", true, true, null, null, null, false);
			configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "Test_PK", "uniqueidentifier", 16, 0, 0, false, true, "", "", true, "", "", true, true, "");
			configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "Test_Value", "int", 4, 10, 0, true, false, "", "", true, "", "", true, true, "");
			configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "Test_String", "varchar", 10, 0, 0, true, false, "", "", true, "", "", true, true, "");
			configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "Test_PartitionDate", "smalldatetime", 0, 0, 0, true, false, "", "", true, "", "", true, true, "");
		}

		void CreateSelfReferencedTestTableInMainDb(DbConnection connection, string tableName)
		{
			var sqlText = string.Format(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Test].[{0}]
(
	Test_PK uniqueidentifier NOT NULL,
	ParentPK1 uniqueidentifier NULL,
	ParentPK2 uniqueidentifier NULL,
	ParentPK3 uniqueidentifier NULL,
	StringValue varchar(50) NULL
)
ALTER TABLE [Test].[{0}]
ADD CONSTRAINT [PK_UX__{0}_Test_PK] PRIMARY KEY NONCLUSTERED ([Test_PK] ASC)

ALTER TABLE [Test].[{0}]
ADD CONSTRAINT [FK_Test_PK_ParentPK1] FOREIGN KEY ( [ParentPK1] )
REFERENCES [Test].[{0}] ( [Test_PK] )

ALTER TABLE [Test].[{0}]
ADD CONSTRAINT [FK_Test_PK_ParentPK2] FOREIGN KEY ( [ParentPK2] )
REFERENCES [Test].[{0}] ( [Test_PK] )

ALTER TABLE [Test].[{0}]
ADD CONSTRAINT [FK_Test_PK_ParentPK3] FOREIGN KEY ( [ParentPK3] )
REFERENCES [Test].[{0}] ( [Test_PK] )", tableName);

			connection.ExecuteNonQuery(sqlText);

			var cdcTable = new CdcTableForTesting("Test", tableName);
			cdcTable.EnableCdc(connection);

			var configData = BiAutomationConfigLoader.Instance.ConfigData;
			var table = configData.CdcTableConfig.AddCdcTableConfigRow("Test", tableName, "Test", "", "", true, true, null, null, null, false);
			configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "Test_PK", "uniqueidentifier", 16, 0, 0, false, true, "", "", true, "", "", true, true, "");
			configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "ParentPK1", "uniqueidentifier", 16, 0, 0, true, false, "", "", true, "Test", tableName, true, true, "");
			configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "ParentPK2", "uniqueidentifier", 16, 0, 0, true, false, "", "", true, "Test", tableName, true, true, "");
			configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "ParentPK3", "uniqueidentifier", 16, 0, 0, true, false, "", "", true, "Test", tableName, true, true, "");
			configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "StringValue", "varchar", 50, 0, 0, true, false, "", "", true, "", "", true, true, "");
		}

		void CreateTransformTestTable(DbConnection connection, string sourceTableName, string baseTableName)
		{
			var configData = BiAutomationConfigLoader.Instance.ConfigData;

			configData.EnforceConstraints = false;

			var transformId = configData.EdwTableConfig.Count(t => t.Name == baseTableName) + 1;

			var edwTable = configData.EdwTableConfig.AddEdwTableConfigRow("Test", baseTableName, "", "Test", sourceTableName, "", transformId, true, 1, "");
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "bigint", 0, 0, 0, edwTable.BaseName + "Key", "", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "uniqueidentifier", 16, 0, 0, edwTable.BaseName + "ID", "Test_PK", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "int", 4, 10, 0, "Value", "Test_Value", false, false, "", "", "", "", "", "", false);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "smalldatetime", 4, 16, 0, "PartitionDate", "Test_PartitionDate", true, false, "", "", "", "", "", "", false);

			configData.EnforceConstraints = true;

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"
IF NOT EXISTS (SELECT NULL FROM sys.tables WHERE name = '{1}')
BEGIN
	CREATE TABLE [Test].[{1}]
	(
		[__$transform_id] int,
		[{7}Key] [bigint] NOT NULL,
		[{7}ID] uniqueidentifier NOT NULL,
		[Value] int,
		[PartitionDate] smalldatetime
	)
END

UPDATE [{0}].[TransformTableConfiguration]
SET IsLastTransform = 0
WHERE ModelSchemaName = 'Test' AND ModelTableName = '{1}'

IF EXISTS (SELECT NULL FROM [{0}].[TransformTableConfiguration] WHERE ModelSchemaName = 'Test' AND ModelTableName = '{1}')
BEGIN
	INSERT INTO [{0}].[TransformTableConfiguration](SourceTableName, ModelSchemaName, ModelTableName, DependencyOrder, TransformId, IsLastTransform, HasChildTables, InitialLoadQuery, IncrementalInsertQuery, IncrementalDeleteQuery, CustomIndexScript)
	SELECT 'Test.{2}', ModelSchemaName, ModelTableName, DependencyOrder, {3}, 1, HasChildTables, N'{4}', N'{5}', N'{6}', N'{8}'
	FROM [{0}].[TransformTableConfiguration] WHERE ModelSchemaName = 'Test' AND ModelTableName = '{1}'
END
ELSE
BEGIN
	DECLARE @DependencyOrder INT = (SELECT MAX(DependencyOrder) + 1 FROM [{0}].[TransformTableConfiguration])
	SELECT @DependencyOrder = ISNULL(@DependencyOrder, 1)
	INSERT INTO [{0}].[TransformTableConfiguration](SourceTableName, ModelSchemaName, ModelTableName, DependencyOrder, TransformId, IsLastTransform, HasChildTables, InitialLoadQuery, IncrementalInsertQuery, IncrementalDeleteQuery, CustomIndexScript)
	SELECT 'Test.{2}', 'Test', '{1}', @DependencyOrder, {3}, 1, 0, N'{4}', N'{5}', N'{6}', N'{8}'
END

INSERT INTO [{0}].[TransformTableState](ModelSchemaName, ModelTableName, TransformId, CurrentState, InitialLoadRequired)
VALUES ('Test', '{1}', {3}, 'New', 1)
",
					BiConstants.BiAdminSchemaName,
					baseTableName,
					sourceTableName,
					transformId,
					configData.GetInitialLoadQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalInsertQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalDeleteQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					edwTable.BaseName,
					configData.GetCustomIndexScriptQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")
					);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void CreateAggregateTestTable(DbConnection connection, string baseTableName, string aggTableName, string customIndex = "")
		{
			var configData = BiAutomationConfigLoader.Instance.ConfigData;

			configData.EnforceConstraints = false;

			var dependencyOrder = configData.EdwDenormalizedTableConfig.Max(t => t.DependencyOrder) + 1;

			var expression = string.Format(CultureInfo.InvariantCulture,
@"[Test].[{0}] AS tt1
LEFT JOIN [Test].[{0}] AS tt2
	ON tt1.[{1}Key] = tt2.[{1}Key]",
				baseTableName,
				baseTableName.Replace(BiConstants.EdwBaseTablePrefix, ""));

			var aggTable = configData.EdwDenormalizedTableConfig.AddEdwDenormalizedTableConfigRow("Test", aggTableName, expression, dependencyOrder, false, "", customIndex);
			configData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(aggTable, aggTable.BaseName + "Key", "bigint", 16, 0, 0, "", false, "", true);
			configData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(aggTable, baseTableName.Replace(BiConstants.EdwBaseTablePrefix, "") + "1Key", "bigint", 16, 0, 0, "tt1.[" + baseTableName.Replace(BiConstants.EdwBaseTablePrefix, "") + "Key]", false, "", false);
			configData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(aggTable, baseTableName.Replace(BiConstants.EdwBaseTablePrefix, "") + "2Key", "bigint", 16, 0, 0, "tt2.[" + baseTableName.Replace(BiConstants.EdwBaseTablePrefix, "") + "Key]", false, "", false);
			configData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(aggTable, "Value", "int", 4, 0, 0, "tt1.Value", false, "", false);
			configData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(aggTable, "PartitionDate", "date", 16, 0, 0, "tt1.PartitionDate", true, "", false);

			configData.EnforceConstraints = true;

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"
IF NOT EXISTS (SELECT NULL FROM sys.tables WHERE name = '{1}')
BEGIN
	CREATE TABLE [Test].[{1}]
	(
		[{2}Key] [bigint] NOT NULL,
		[{3}1Key] bigint NOT NULL,
		[{3}2Key] bigint NOT NULL,
		[Value] int,
		[PartitionDate] date
	)
END

INSERT INTO [{0}].[ModelTableConfiguration](ModelSchemaName, ModelTableName, DependencyOrder, SourceTables, EdwPartitionColumnName)
SELECT 'Test', '{1}', {4}, '[Test].[{5}]', 'PartitionDate'

INSERT INTO [{0}].[ModelTableState](ModelSchemaName, ModelTableName, CurrentState, InitialLoadRequired)
VALUES ('Test', '{1}', 'New', 1)
",
					BiConstants.BiAdminSchemaName,
					aggTableName,
					aggTable.BaseName,
					baseTableName.Replace(BiConstants.EdwBaseTablePrefix, ""),
					dependencyOrder,
					baseTableName,
					configData.GetCustomIndexScriptQueryForDenormalizedTable(aggTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")
					);
				connection.ExecuteNonQuery(sqlText);

				var createViewQuery = configData.GetCreateDenormalizedTableViewQuery(aggTable);
				connection.ExecuteNonQuery(createViewQuery);

				var iniLoadQuery = configData.GetInitialLoadQueryForDenormalizedTable(aggTable);
				connection.ExecuteNonQuery(iniLoadQuery);

				var incLoadQuery = configData.GetIncrementalLoadQueryForDenormalizedTable(aggTable);
				connection.ExecuteNonQuery(incLoadQuery);
			}
		}

		void CreateAggregateTestTable_New(DbConnection connection, string baseTableName, string aggTableName, BiConfigurationData configData)
		{
			configData.EnforceConstraints = false;

			int dependencyOrder;

			if (configData.EdwDenormalizedTableConfig.Count == 0)
			{
				dependencyOrder = 1;
			}
			else
			{
				dependencyOrder = configData.EdwDenormalizedTableConfig.Max(t => t.DependencyOrder) + 1;
			}

			var expression = string.Format(CultureInfo.InvariantCulture,
@"[Test].[{0}] AS tt1
LEFT JOIN [Test].[{0}] AS tt2
	ON tt1.[{1}Key] = tt2.[{1}Key]",
				baseTableName,
				baseTableName.Replace(BiConstants.EdwBaseTablePrefix, ""));

			var aggTable = configData.EdwDenormalizedTableConfig.AddEdwDenormalizedTableConfigRow("Test", aggTableName, expression, dependencyOrder, false, "", "");
			configData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(aggTable, aggTable.BaseName + "Key", "bigint", 16, 0, 0, "", false, "", true);
			configData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(aggTable, baseTableName.Replace(BiConstants.EdwBaseTablePrefix, "") + "1Key", "bigint", 16, 0, 0, "tt1.[" + baseTableName.Replace(BiConstants.EdwBaseTablePrefix, "") + "Key]", false, "", false);
			configData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(aggTable, baseTableName.Replace(BiConstants.EdwBaseTablePrefix, "") + "2Key", "bigint", 16, 0, 0, "tt2.[" + baseTableName.Replace(BiConstants.EdwBaseTablePrefix, "") + "Key]", false, "", false);
			configData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(aggTable, "Value", "int", 4, 0, 0, "tt1.Value", false, "", false);
			configData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(aggTable, "PartitionDate", "date", 16, 0, 0, "tt1.PartitionDate", true, "", false);

			configData.EnforceConstraints = true;

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"
IF NOT EXISTS (SELECT NULL FROM sys.tables WHERE name = '{1}')
BEGIN
	CREATE TABLE [Test].[{1}]
	(
		[{2}Key] [bigint] NOT NULL,
		[{3}1Key] bigint NOT NULL,
		[{3}2Key] bigint NOT NULL,
		[Value] int,
		[PartitionDate] date
	)
END

INSERT INTO [{0}].[ModelTableConfiguration](ModelSchemaName, ModelTableName, DependencyOrder, SourceTables, EdwPartitionColumnName)
SELECT 'Test', '{1}', {4}, '[Test].[{5}]', 'PartitionDate'

INSERT INTO [{0}].[ModelTableState](ModelSchemaName, ModelTableName, CurrentState, InitialLoadRequired)
VALUES ('Test', '{1}', 'New', 1)
",
					BiConstants.BiAdminSchemaName,
					aggTableName,
					aggTable.BaseName,
					baseTableName.Replace(BiConstants.EdwBaseTablePrefix, ""),
					dependencyOrder,
					baseTableName,
					configData.GetCustomIndexScriptQueryForDenormalizedTable(aggTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")
					);
				connection.ExecuteNonQuery(sqlText);

				var createViewQuery = configData.GetCreateDenormalizedTableViewQuery(aggTable);
				connection.ExecuteNonQuery(createViewQuery);

				var iniLoadQuery = configData.GetInitialLoadQueryForDenormalizedTable(aggTable);
				connection.ExecuteNonQuery(iniLoadQuery);

				var incLoadQuery = configData.GetIncrementalLoadQueryForDenormalizedTable(aggTable);
				connection.ExecuteNonQuery(incLoadQuery);
			}
		}

		void CreateSelfReferencedTransformTestTable(DbConnection connection, string sourceTableName, string baseTableName, bool hasChildTables)
		{
			var configData = BiAutomationConfigLoader.Instance.ConfigData;

			configData.EnforceConstraints = false;

			var edwTable = configData.EdwTableConfig.AddEdwTableConfigRow("Test", baseTableName, "", "Test", sourceTableName, "", 1, true, 1, "");
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "bigint", 0, 0, 0, edwTable.BaseName + "Key", "", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "uniqueidentifier", 16, 0, 0, edwTable.BaseName + "ID", "Test_PK", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "bigint", 16, 0, 0, "ParentKey", "ParentPK1", false, false, "", baseTableName, edwTable.BaseName + "ID", "", "", "", false);

			configData.EnforceConstraints = true;

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Staging].[{2}]
(
	[__$start_lsn] binary(10),
	OP_TYPE tinyint,
	Test_PK uniqueidentifier,
	ParentPK1 uniqueidentifier
 );

INSERT INTO [{0}].[StagingTableConfiguration](SourceSchemaName, StagingSchemaName, StagingTableName, PkName, IndexedColumnName, StagingTableDefinition, StagingTableColumnListSelect, StagingTableColumnListInsert, ColumnEnumeratedList, ColumnEnumeratedListWithDefinition, EdwFilter)
VALUES
(
	N'Test', N'Staging', N'{2}', N'Test_PK', N'Test_PK', N'Test_PK uniqueidentifier, ParentPK1 uniqueidentifier', N'Test_PK, ParentPK1', N'Test_PK, ParentPK1',
	N'c1, c2', N'c1 uniqueidentifier, c2 uniqueidentifier', ''
)

INSERT INTO [{0}].[StagingTableState](SourceTableName, CurrentMaxLsn, CurrentState, InitialLoadRequired)
VALUES ('Test.{2}', 0x0, 'New', 1)

IF NOT EXISTS (SELECT NULL FROM sys.tables WHERE name = '{1}')
BEGIN
	CREATE TABLE [Test].[{1}]
	(
		[__$transform_id] int,
		[{6}Key] [bigint] NOT NULL,
		[{6}ID] uniqueidentifier NOT NULL,
		[ParentKey] [bigint]
	)
END

DECLARE @DependencyOrder INT = (SELECT MAX(DependencyOrder) + 1 FROM [{0}].[TransformTableConfiguration])
SELECT @DependencyOrder = ISNULL(@DependencyOrder, 1)
INSERT INTO [{0}].[TransformTableConfiguration](SourceTableName, ModelSchemaName, ModelTableName, DependencyOrder, TransformId, IsLastTransform, IsSelfReferenced, HasChildTables, InitialLoadQuery, IncrementalInsertQuery, IncrementalDeleteQuery, CustomIndexScript)
SELECT 'Test.{2}', 'Test', '{1}', @DependencyOrder, 1, 1, 1, {7}, N'{3}', N'{4}', N'{5}', N'{8}'

INSERT INTO [{0}].[TransformTableState](ModelSchemaName, ModelTableName, TransformId, CurrentState, InitialLoadRequired)
VALUES ('Test', '{1}', 1, 'New', 1)
",
					BiConstants.BiAdminSchemaName,
					baseTableName,
					sourceTableName,
					configData.GetInitialLoadQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalInsertQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalDeleteQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					edwTable.BaseName,
					hasChildTables ? "1" : "0",
					configData.GetCustomIndexScriptQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")
					);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void CreateSelfReferencedMultiTransformTestTable(DbConnection connection, string sourceTableName, string baseTableName, bool hasChildTables)
		{
			var configData = BiAutomationConfigLoader.Instance.ConfigData;

			configData.EnforceConstraints = false;

			var edwTable1 = configData.EdwTableConfig.AddEdwTableConfigRow("Test", baseTableName, "", "Test", sourceTableName, "", 1, true, 1, "");
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable1.Name, edwTable1.TransformId, "bigint", 0, 0, 0, edwTable1.BaseName + "Key", "", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable1.Name, edwTable1.TransformId, "uniqueidentifier", 16, 0, 0, edwTable1.BaseName + "ID", "Test_PK", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable1.Name, edwTable1.TransformId, "bigint", 16, 0, 0, "ParentKey", "ParentPK1", false, false, "", baseTableName, edwTable1.BaseName + "ID", "", "", "", false);

			var edwTable2 = configData.EdwTableConfig.AddEdwTableConfigRow("Test", baseTableName, "", "Test", sourceTableName, "", 2, true, 1, "");
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable2.Name, edwTable2.TransformId, "bigint", 0, 0, 0, edwTable2.BaseName + "Key", "", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable2.Name, edwTable2.TransformId, "uniqueidentifier", 16, 0, 0, edwTable2.BaseName + "ID", "Test_PK", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable2.Name, edwTable2.TransformId, "bigint", 16, 0, 0, "ParentKey", "ParentPK2", false, false, "", baseTableName, edwTable2.BaseName + "ID", "", "", "", false);

			var edwTable3 = configData.EdwTableConfig.AddEdwTableConfigRow("Test", baseTableName, "", "Test", sourceTableName, "", 3, true, 1, "");
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable3.Name, edwTable3.TransformId, "bigint", 0, 0, 0, edwTable3.BaseName + "Key", "", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable3.Name, edwTable3.TransformId, "uniqueidentifier", 16, 0, 0, edwTable3.BaseName + "ID", "Test_PK", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable3.Name, edwTable3.TransformId, "bigint", 16, 0, 0, "ParentKey", "ParentPK3", false, false, "", baseTableName, edwTable3.BaseName + "ID", "", "", "", false);

			configData.EnforceConstraints = true;

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Staging].[{2}]
(
	[__$start_lsn] binary(10),
	OP_TYPE tinyint,
	Test_PK uniqueidentifier,
	ParentPK1 uniqueidentifier,
	ParentPK2 uniqueidentifier,
	ParentPK3 uniqueidentifier
 );

INSERT INTO [{0}].[StagingTableConfiguration](SourceSchemaName, StagingSchemaName, StagingTableName, PkName, IndexedColumnName, StagingTableDefinition, StagingTableColumnListSelect, StagingTableColumnListInsert, ColumnEnumeratedList, ColumnEnumeratedListWithDefinition, EdwFilter)
VALUES
(
	N'Test', N'Staging', N'{2}', N'Test_PK', N'Test_PK', N'Test_PK uniqueidentifier, ParentPK1 uniqueidentifier, ParentPK2 uniqueidentifier, ParentPK3 uniqueidentifier', N'Test_PK, ParentPK1, ParentPK2, ParentPK3',
	N'Test_PK, ParentPK1, ParentPK2, ParentPK3',
	N'c1, c2, c3, c4', N'c1 uniqueidentifier, c2 uniqueidentifier, c3 uniqueidentifier, c4 uniqueidentifier', ''
)

INSERT INTO [{0}].[StagingTableState](SourceTableName, CurrentMaxLsn, CurrentState, InitialLoadRequired)
VALUES ('Test.{2}', 0x0, 'New', 1)

IF NOT EXISTS (SELECT NULL FROM sys.tables WHERE name = '{1}')
BEGIN
	CREATE TABLE [Test].[{1}]
	(
		[__$transform_id] int,
		[{3}Key] [bigint] NOT NULL,
		[{3}ID] uniqueidentifier NOT NULL,
		[ParentKey] [bigint]
	)
END

DECLARE @DependencyOrder INT = (SELECT MAX(DependencyOrder) + 1 FROM [{0}].[TransformTableConfiguration])
SELECT @DependencyOrder = ISNULL(@DependencyOrder, 1)
INSERT INTO [{0}].[TransformTableConfiguration](SourceTableName, ModelSchemaName, ModelTableName, DependencyOrder, TransformId, IsLastTransform, IsSelfReferenced, HasChildTables, InitialLoadQuery, IncrementalInsertQuery, IncrementalDeleteQuery, CustomIndexScript)
VALUES
	('Test.{2}', 'Test', '{1}', @DependencyOrder, 1, 0, 1, {4}, N'{5}', N'{6}', N'{7}', N'{8}'),
	('Test.{2}', 'Test', '{1}', @DependencyOrder, 2, 0, 1, {4}, N'{9}', N'{10}', N'{11}', N'{12}'),
	('Test.{2}', 'Test', '{1}', @DependencyOrder, 3, 1, 1, {4}, N'{13}', N'{14}', N'{15}', N'{16}')

INSERT INTO [{0}].[TransformTableState](ModelSchemaName, ModelTableName, TransformId, CurrentState, InitialLoadRequired)
VALUES
	('Test', '{1}', 1, 'New', 1),
	('Test', '{1}', 2, 'New', 1),
	('Test', '{1}', 3, 'New', 1)
",
					BiConstants.BiAdminSchemaName,
					baseTableName,
					sourceTableName,
					edwTable1.BaseName,
					hasChildTables ? "1" : "0",
					configData.GetInitialLoadQueryForEdwTable(edwTable1).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalInsertQueryForEdwTable(edwTable1).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalDeleteQueryForEdwTable(edwTable1).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetCustomIndexScriptQueryForEdwTable(edwTable1).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetInitialLoadQueryForEdwTable(edwTable2).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalInsertQueryForEdwTable(edwTable2).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalDeleteQueryForEdwTable(edwTable2).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetCustomIndexScriptQueryForEdwTable(edwTable2).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetInitialLoadQueryForEdwTable(edwTable3).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalInsertQueryForEdwTable(edwTable3).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalDeleteQueryForEdwTable(edwTable3).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetCustomIndexScriptQueryForEdwTable(edwTable3).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")
					);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void CreateMultiSelfReferencedTransformTestTable(DbConnection connection, string sourceTableName, string baseTableName)
		{
			var configData = BiAutomationConfigLoader.Instance.ConfigData;

			configData.EnforceConstraints = false;

			var edwTable = configData.EdwTableConfig.AddEdwTableConfigRow("Test", baseTableName, "", "Test", sourceTableName, "[StringValue] NOT IN('Exclude')", 1, true, 1, "");
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "bigint", 0, 0, 0, edwTable.BaseName + "Key", "", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "uniqueidentifier", 16, 0, 0, edwTable.BaseName + "ID", "Test_PK", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "bigint", 16, 0, 0, "ParentKey1", "ParentPK1", false, false, "", baseTableName, edwTable.BaseName + "ID", "", "", "", false);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "bigint", 16, 0, 0, "ParentKey2", "ParentPK2", false, false, "", baseTableName, edwTable.BaseName + "ID", "", "", "", false);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, 1, "varchar", 50, 0, 0, "StrValue", "StringValue", false, false, "", "", "", "", "", "", false);

			configData.EnforceConstraints = true;

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Staging].[{2}]
(
	[__$start_lsn] binary(10),
	OP_TYPE tinyint,
	Test_PK uniqueidentifier,
	ParentPK1 uniqueidentifier,
	ParentPK2 uniqueidentifier,
	StringValue varchar(50)
 );

INSERT INTO [{0}].[StagingTableConfiguration](SourceSchemaName, StagingSchemaName, StagingTableName, PkName, IndexedColumnName, StagingTableDefinition, StagingTableColumnListSelect, StagingTableColumnListInsert, ColumnEnumeratedList, ColumnEnumeratedListWithDefinition, EdwFilter)
VALUES
(
	N'Test', N'Staging', N'{2}', N'Test_PK', N'Test_PK', N'Test_PK uniqueidentifier, ParentPK1 uniqueidentifier, ParentPK2 uniqueidentifier, StringValue varchar(50)', N'Test_PK, ParentPK1, ParentPK2, StringValue',
	N'Test_PK, ParentPK1, ParentPK2, StringValue',
	N'c1, c2, c3, c4', N'c1 uniqueidentifier, c2 uniqueidentifier, c3 uniqueidentifier, c4 varchar(50)', ''
)

INSERT INTO [{0}].[StagingTableState](SourceTableName, CurrentMaxLsn, CurrentState, InitialLoadRequired)
VALUES ('Test.{2}', 0x0, 'New', 1)

IF NOT EXISTS (SELECT NULL FROM sys.tables WHERE name = '{1}')
BEGIN
	CREATE TABLE [Test].[{1}]
	(
		[__$transform_id] int,
		[{6}Key] [bigint] NOT NULL,
		[{6}ID] uniqueidentifier NOT NULL,
		[ParentKey1] [bigint],
		[ParentKey2] [bigint],
		[StrValue] varchar(50)
	)
END

DECLARE @DependencyOrder INT = (SELECT MAX(DependencyOrder) + 1 FROM [{0}].[TransformTableConfiguration])
SELECT @DependencyOrder = ISNULL(@DependencyOrder, 1)
INSERT INTO [{0}].[TransformTableConfiguration](SourceTableName, ModelSchemaName, ModelTableName, DependencyOrder, TransformId, IsLastTransform, IsSelfReferenced, HasChildTables, InitialLoadQuery, IncrementalInsertQuery, IncrementalDeleteQuery, WhereCondition, CustomIndexScript)
SELECT 'Test.{2}', 'Test', '{1}', @DependencyOrder, 1, 1, 1, 0, N'{3}', N'{4}', N'{5}', '[StringValue] NOT IN(''Exclude'')', N'{7}'

INSERT INTO [{0}].[TransformTableState](ModelSchemaName, ModelTableName, TransformId, CurrentState, InitialLoadRequired)
VALUES ('Test', '{1}', 1, 'New', 1)
",
					BiConstants.BiAdminSchemaName,
					baseTableName,
					sourceTableName,
					configData.GetInitialLoadQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalInsertQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalDeleteQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					edwTable.BaseName,
					configData.GetCustomIndexScriptQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")
					);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void CreateCustomTable(DbConnection connection, string customTableName, bool runBeforeTransform)
		{
			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var configData = BiAutomationConfigLoader.Instance.ConfigData;

				configData.EnforceConstraints = false;

				var dependencyOrder = configData.EdwCustomTableConfig.Max(t => t.DependencyOrder) + 1;

				var customTable = configData.EdwCustomTableConfig.AddEdwCustomTableConfigRow("Test", customTableName, dependencyOrder, "", "", "", "", runBeforeTransform, "");
				configData.EdwCustomColumnConfig.AddEdwCustomColumnConfigRow(customTable, "Code1", "int", 0, 0, 0, "");
				configData.EdwCustomColumnConfig.AddEdwCustomColumnConfigRow(customTable, "Code2", "int", 0, 0, 0, "");

				if (runBeforeTransform)
				{
					var baseTable = configData.EdwTableConfig.FirstOrDefault(t => t.Name == "BAS__TestTable");
					if (baseTable != null)
					{
						var edwColumn = configData.EdwColumnConfig.FirstOrDefault(c => c.TableName == baseTable.Name && c.Name == "Value");
						if (edwColumn != null)
						{
							edwColumn.ParentTable = customTableName;
							edwColumn.ParentColumn = "Code1";
							edwColumn.TargetColumn = "Code2";
							customTable.InitialLoadQuery = "SELECT * FROM [Test].[BAS__TestTable]";
						}
						else
						{
							Fail("Column Value needs to be in the base table for this test.");
						}
					}
					else
					{
						Fail("Create the base table before the custom table.");
					}
				}

				configData.EnforceConstraints = true;
				var sqlText = string.Format(@"
IF NOT EXISTS (SELECT NULL FROM sys.tables WHERE name = '{1}')
BEGIN
	CREATE TABLE [Test].[{1}]
	(
		Code1 int,
		Code2 int
	)
END

INSERT INTO [{0}].[CustomTableConfiguration](ModelSchemaName, ModelTableName, DependencyOrder, ViewName, InitialLoadQuery, IncrementalLoadQuery, RunBeforeTransform, TableDependencyList)
SELECT 'Test', '{1}', {2}, N'{3}', N'{4}', N'{5}', {6}, N'{7}'

INSERT INTO [{0}].[CustomTableState](ModelSchemaName, ModelTableName, CurrentState, InitialLoadRequired)
VALUES ('Test', '{1}', 'New', 1)
",
					BiConstants.BiAdminSchemaName,
					customTable.Name,
					dependencyOrder,
					customTable.ViewName,
					customTable.InitialLoadQuery.Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					customTable.IncrementalLoadQuery.Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					customTable.RunBeforeTransform ? "1" : "0",
					customTable.GetTableDependencyList()
					);
				connection.ExecuteNonQuery(sqlText);

				AddCustomTableToConfiguration(connection, customTable);
			}
		}

		void CreateTestTableThatUsesFunction(DbConnection connection)
		{
			var configData = BiAutomationConfigLoader.Instance.ConfigData;
			configData.EnforceConstraints = false;

			var edwTable = configData.EdwTableConfig.AddEdwTableConfigRow("Test", "BAS__TestTable", "", "Test", "TestTable", "", 1, true, 1, "");
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "bigint", 0, 0, 0, "TestTableKey", "", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "uniqueidentifier", 16, 0, 0, "TestTableID", "Test_PK", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "varchar", 8, 0, 0, "ConvertedDate", "dbo.ufn_ConvertDateToString(Test_PartitionDate)", false, true, "", "", "", "", "", "", false);

			configData.EnforceConstraints = true;

			CreateTestTableInMainDb(connection, "TestTable");

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Test].[BAS__TestTable]
(
	[__$transform_id] int,
	[TestTableKey] [bigint] NOT NULL,
	[TestTableID] uniqueidentifier NOT NULL,
	[ConvertedDate] varchar(8)
)

DECLARE @maxDependencyOrder int = 0
IF EXISTS (SELECT NULL FROM [{0}].TransformTableConfiguration)
	SET @maxDependencyOrder = (SELECT MAX(DependencyOrder) FROM [{0}].TransformTableConfiguration)

INSERT INTO [{0}].[TransformTableConfiguration](SourceTableName, ModelSchemaName, ModelTableName, DependencyOrder, TransformId, IsLastTransform, HasChildTables, InitialLoadQuery, IncrementalInsertQuery, IncrementalDeleteQuery, CustomIndexScript)
VALUES
(
	N'Test.TestTable', N'Test', N'BAS__TestTable', @maxDependencyOrder + 1, 1, 1, 0, N'{1}', N'{2}', N'{3}', N'{4}'
)

INSERT INTO [{0}].[TransformTableState](ModelSchemaName, ModelTableName, TransformId, CurrentState, InitialLoadRequired)
VALUES ('Test', 'BAS__TestTable', 1, 'New', 1)
",
					BiConstants.BiAdminSchemaName,
					configData.GetInitialLoadQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalInsertQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalDeleteQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetCustomIndexScriptQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")
					);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void CreateTestTableThatUsesFunction_New(DbConnection connection, BiConfigurationData configData)
		{
			configData.EnforceConstraints = false;

			var edwTable = configData.EdwTableConfig.AddEdwTableConfigRow("Test", "BAS__TestTable", "", "Test", "TestTable", "", 1, true, 1, "");
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "bigint", 0, 0, 0, "TestTableKey", "", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "uniqueidentifier", 16, 0, 0, "TestTableID", "Test_PK", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "varchar", 8, 0, 0, "ConvertedDate", "dbo.ufn_ConvertDateToString(Test_PartitionDate)", false, true, "", "", "", "", "", "", false);

			configData.EnforceConstraints = true;

			CreateTestTableInMainDb_New(connection, "TestTable", configData);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Test].[BAS__TestTable]
(
	[__$transform_id] int,
	[TestTableKey] [bigint] NOT NULL,
	[TestTableID] uniqueidentifier NOT NULL,
	[ConvertedDate] varchar(8)
)

DECLARE @maxDependencyOrder int = 0
IF EXISTS (SELECT NULL FROM [{0}].TransformTableConfiguration)
	SET @maxDependencyOrder = (SELECT MAX(DependencyOrder) FROM [{0}].TransformTableConfiguration)

INSERT INTO [{0}].[TransformTableConfiguration](SourceTableName, ModelSchemaName, ModelTableName, DependencyOrder, TransformId, IsLastTransform, HasChildTables, InitialLoadQuery, IncrementalInsertQuery, IncrementalDeleteQuery, CustomIndexScript)
VALUES
(
	N'Test.TestTable', N'Test', N'BAS__TestTable', @maxDependencyOrder + 1, 1, 1, 0, N'{1}', N'{2}', N'{3}', N'{4}'
)

INSERT INTO [{0}].[TransformTableState](ModelSchemaName, ModelTableName, TransformId, CurrentState, InitialLoadRequired)
VALUES ('Test', 'BAS__TestTable', 1, 'New', 1)
",
					BiConstants.BiAdminSchemaName,
					configData.GetInitialLoadQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalInsertQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalDeleteQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetCustomIndexScriptQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")
					);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void CreateConvertDateToStringFunction(DbConnection connection)
		{
			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				connection.ExecuteNonQuery(@"IF EXISTS (SELECT NULL FROM sys.objects WHERE name = 'ufn_ConvertDateToString') DROP FUNCTION [ufn_ConvertDateToString]");
				var sqlText = @"
CREATE FUNCTION dbo.ufn_ConvertDateToString (@Date datetime)
RETURNS TABLE 
AS 
RETURN 
( 
   SELECT Convert(VARCHAR(8), @Date, 112) AS Value
)";
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void AddCustomTableToConfiguration(DbConnection connection, BiAutomationConfigDataSet.EdwCustomTableConfigRow customTable)
		{
			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"
DELETE FROM [{0}].[CustomTableConfiguration]
WHERE ModelTableName = '{1}'

DELETE FROM [{0}].[CustomTableState]
WHERE ModelTableName = '{1}'

INSERT INTO [{0}].[CustomTableConfiguration](ModelSchemaName, ModelTableName, DependencyOrder, ViewName, InitialLoadQuery, IncrementalLoadQuery, RunBeforeTransform, TableDependencyList, CustomTableDependencyList)
SELECT 'Test', '{1}', {2}, N'{3}', N'{4}', N'{5}', {6}, N'{7}', N'{8}'

INSERT INTO [{0}].[CustomTableState](ModelSchemaName, ModelTableName, CurrentState, InitialLoadRequired)
VALUES ('Test', '{1}', 'New', 1)
",
				BiConstants.BiAdminSchemaName,
				customTable.Name,
				customTable.DependencyOrder,
				customTable.ViewName,
				customTable.InitialLoadQuery.Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
				customTable.IncrementalLoadQuery.Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
				customTable.RunBeforeTransform ? "1" : "0",
				customTable.GetTableDependencyList(),
				string.Join(",", customTable.ParseCustomTablesFromQueries().Select(t => "[" + t.Schema + "].[" + t.Name + "]"))
				);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void TriggerInitialLoad(DbConnection connection)
		{
			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"UPDATE [{0}].[StagingTableState] SET InitialLoadRequired = 1, CurrentMaxLsn = 0x0, CurrentState = 'New'", BiConstants.BiAdminSchemaName);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void TriggerInitialLoadForTestTable(DbConnection connection)
		{
			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"UPDATE [{0}].[StagingTableState] SET InitialLoadRequired = 1, CurrentMaxLsn = 0x0, CurrentState = 'New' WHERE SourceTableName = 'Test.TestTable'", BiConstants.BiAdminSchemaName);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void SetInitialLoadRequestedStatus(DbConnection connection, int value)
		{
			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				var sqlText = string.Format(@"EXEC [{0}].usp_SetMasterStateParameter @ParamName = '{1}', @ParamValue = '{2}'", BiConstants.BiAdminSchemaName, BiConstants.InitialLoadRequested, value);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void CreateTestParentAndChildTable(AdminConnection connection)
		{
			var configData = BiAutomationConfigLoader.Instance.ConfigData;

			configData.EnforceConstraints = false;

			var table = configData.CdcTableConfig.AddCdcTableConfigRow("Test", "TestChildTable", "Test", "", "", true, true, null, null, null, false);
			var cdcColumn = configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "TestChild_PK", "uniqueidentifier", 16, 0, 0, false, true, "", "", true, "", "", true, true, "");
			var cdcColumn2 = configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "TestParent_PK", "uniqueidentifier", 16, 0, 0, true, false, "", "", true, "", "", true, true, "");

			var edwTable = configData.EdwTableConfig.AddEdwTableConfigRow("Test", "BAS__TestChildTable", "", "Test", "TestChildTable", "", 1, true, 1, "");
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "bigint", 0, 0, 0, "TestChildTableKey", "", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "uniqueidentifier", 16, 0, 0, "TestChildTableID", "TestChild_PK", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "bigint", 16, 0, 0, "ParentKey", "TestParent_PK", false, false, "", "BAS__TestTable", "TestTableID", "", "", "", false);

			CreateTestTableInEdw(connection, "TestTable");

			configData.EnforceConstraints = true;

			var sqlText = @"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Test].[TestChildTable]
(
	TestChild_PK uniqueidentifier NOT NULL,
	TestParent_PK uniqueidentifier
)
ALTER TABLE [Test].[TestChildTable]
ADD CONSTRAINT [PK_UX__TestChild_PK] PRIMARY KEY NONCLUSTERED ([TestChild_PK] ASC)";

			connection.ExecuteNonQuery(sqlText);

			var cdcTable = new CdcTableForTesting("Test", "TestChildTable");
			cdcTable.EnableCdc(connection);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				sqlText = string.Format(@"
CREATE TABLE [Staging].[TestChildTable]
(
	[__$start_lsn] binary(10),
	OP_TYPE tinyint,
	TestChild_PK uniqueidentifier,
	TestParent_PK uniqueidentifier
 );

CREATE TABLE [Test].[BAS__TestChildTable]
(
	[__$transform_id] int,
	[TestChildTableKey] [bigint] NOT NULL,
	[TestChildTableID] uniqueidentifier NOT NULL,
	[ParentKey] bigint
)

DECLARE @maxDependencyOrder int = 0
IF EXISTS (SELECT NULL FROM [{0}].TransformTableConfiguration)
	SET @maxDependencyOrder = (SELECT MAX(DependencyOrder) FROM [{0}].TransformTableConfiguration)

UPDATE [{0}].[TransformTableConfiguration]
SET HasChildTables = 1
WHERE ModelSchemaName = 'Test' AND ModelTableName = 'BAS__TestTable'

INSERT INTO [{0}].[StagingTableConfiguration](SourceSchemaName, StagingSchemaName, StagingTableName, PkName, IndexedColumnName, StagingTableDefinition, StagingTableColumnListSelect, StagingTableColumnListInsert, ColumnEnumeratedList, ColumnEnumeratedListWithDefinition, EdwFilter)
VALUES
(
	N'Test', N'Staging', N'TestChildTable', N'TestChild_PK', N'TestChild_PK', N'TestChild_PK uniqueidentifier, TestParent_PK uniqueidentifier', N'TestChild_PK, TestParent_PK',
	N'TestChild_PK, TestParent_PK',
	N'c1, c2', N'c1 uniqueidentifier, c2 uniqueidentifier', ''
)

INSERT INTO [{0}].[TransformTableConfiguration](SourceTableName, ModelSchemaName, ModelTableName, DependencyOrder, TransformId, IsLastTransform, HasChildTables, InitialLoadQuery, IncrementalInsertQuery, IncrementalDeleteQuery, CustomIndexScript)
VALUES
(
	N'Test.TestChildTable', N'Test', N'BAS__TestChildTable', @maxDependencyOrder + 1, 1, 1, 0, N'{1}', N'{2}', N'{3}', N'{4}'
)

INSERT INTO [{0}].[StagingTableState](SourceTableName, CurrentMaxLsn, CurrentState, InitialLoadRequired)
VALUES ('Test.TestChildTable', 0x0, 'New', 1)

INSERT INTO [{0}].[TransformTableState](ModelSchemaName, ModelTableName, TransformId, CurrentState, InitialLoadRequired)
VALUES ('Test', 'BAS__TestChildTable', 1, 'New', 1)
",
					BiConstants.BiAdminSchemaName,
					configData.GetInitialLoadQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalInsertQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalDeleteQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetCustomIndexScriptQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")
					);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void CreateTestParentAndChildTable_New(AdminConnection connection, BiConfigurationData configData)
		{
			configData.EnforceConstraints = false;

			var table = configData.CdcTableConfig.AddCdcTableConfigRow("Test", "TestChildTable", "Test", "", "", true, true, null, null, null, false);
			var cdcColumn = configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "TestChild_PK", "uniqueidentifier", 16, 0, 0, false, true, "", "", true, "", "", true, true, "");
			var cdcColumn2 = configData.CdcColumnConfig.AddCdcColumnConfigRow(table, "TestParent_PK", "uniqueidentifier", 16, 0, 0, true, false, "", "", true, "", "", true, true, "");

			var edwTable = configData.EdwTableConfig.AddEdwTableConfigRow("Test", "BAS__TestChildTable", "", "Test", "TestChildTable", "", 1, true, 1, "");
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "bigint", 0, 0, 0, "TestChildTableKey", "", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "uniqueidentifier", 16, 0, 0, "TestChildTableID", "TestChild_PK", false, false, "", "", "", "", "", "", true);
			configData.EdwColumnConfig.AddEdwColumnConfigRow(edwTable.Name, edwTable.TransformId, "bigint", 16, 0, 0, "ParentKey", "TestParent_PK", false, false, "", "BAS__TestTable", "TestTableID", "", "", "", false);

			CreateTestTableInEdw_New(connection, "TestTable", configData);

			configData.EnforceConstraints = true;

			var sqlText = @"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Test].[TestChildTable]
(
	TestChild_PK uniqueidentifier NOT NULL,
	TestParent_PK uniqueidentifier
)
ALTER TABLE [Test].[TestChildTable]
ADD CONSTRAINT [PK_UX__TestChild_PK] PRIMARY KEY NONCLUSTERED ([TestChild_PK] ASC)";

			connection.ExecuteNonQuery(sqlText);

			var cdcTable = new CdcTableForTesting("Test", "TestChildTable");
			cdcTable.EnableCdc(connection);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			{
				sqlText = string.Format(@"
CREATE TABLE [Staging].[TestChildTable]
(
	[__$start_lsn] binary(10),
	OP_TYPE tinyint,
	TestChild_PK uniqueidentifier,
	TestParent_PK uniqueidentifier
 );

CREATE TABLE [Test].[BAS__TestChildTable]
(
	[__$transform_id] int,
	[TestChildTableKey] [bigint] NOT NULL,
	[TestChildTableID] uniqueidentifier NOT NULL,
	[ParentKey] bigint
)

DECLARE @maxDependencyOrder int = 0
IF EXISTS (SELECT NULL FROM [{0}].TransformTableConfiguration)
	SET @maxDependencyOrder = (SELECT MAX(DependencyOrder) FROM [{0}].TransformTableConfiguration)

UPDATE [{0}].[TransformTableConfiguration]
SET HasChildTables = 1
WHERE ModelSchemaName = 'Test' AND ModelTableName = 'BAS__TestTable'

INSERT INTO [{0}].[StagingTableConfiguration](SourceSchemaName, StagingSchemaName, StagingTableName, PkName, IndexedColumnName, StagingTableDefinition, StagingTableColumnListSelect, StagingTableColumnListInsert, ColumnEnumeratedList, ColumnEnumeratedListWithDefinition, EdwFilter)
VALUES
(
	N'Test', N'Staging', N'TestChildTable', N'TestChild_PK', N'TestChild_PK', N'TestChild_PK uniqueidentifier, TestParent_PK uniqueidentifier', N'TestChild_PK, TestParent_PK',
	N'TestChild_PK, TestParent_PK',
	N'c1, c2', N'c1 uniqueidentifier, c2 uniqueidentifier', ''
)

INSERT INTO [{0}].[TransformTableConfiguration](SourceTableName, ModelSchemaName, ModelTableName, DependencyOrder, TransformId, IsLastTransform, HasChildTables, InitialLoadQuery, IncrementalInsertQuery, IncrementalDeleteQuery, CustomIndexScript)
VALUES
(
	N'Test.TestChildTable', N'Test', N'BAS__TestChildTable', @maxDependencyOrder + 1, 1, 1, 0, N'{1}', N'{2}', N'{3}', N'{4}'
)

INSERT INTO [{0}].[StagingTableState](SourceTableName, CurrentMaxLsn, CurrentState, InitialLoadRequired)
VALUES ('Test.TestChildTable', 0x0, 'New', 1)

INSERT INTO [{0}].[TransformTableState](ModelSchemaName, ModelTableName, TransformId, CurrentState, InitialLoadRequired)
VALUES ('Test', 'BAS__TestChildTable', 1, 'New', 1)
",
					BiConstants.BiAdminSchemaName,
					configData.GetInitialLoadQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalInsertQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetIncrementalDeleteQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " "),
					configData.GetCustomIndexScriptQueryForEdwTable(edwTable).Replace("'", "''").Replace("\r\n", " ").Replace("\t", " ")
					);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		#region Row manipulation

		void InsertRowToTestTable(DbConnection connection, string tableName, int value, DateTime? dateTime)
		{
			var sqlText = string.Format(
				@"INSERT INTO [Test].[{1}] (Test_PK, Test_Value, Test_PartitionDate) VALUES (newid(), @value, {0})",
				(dateTime != null) ? "@partitionDate" : "NULL",
				tableName);

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@value", SqlDbType.Int, value);
				if (dateTime != null)
				{
					cmd.AddParameter("@partitionDate", SqlDbType.SmallDateTime, dateTime);
				}

				cmd.ExecuteNonQuery();
			}
		}

		void InsertRowToTestTable(DbConnection connection, string tableName, Guid id, string stringValue)
		{
			var sqlText = string.Format(
				@"INSERT INTO [Test].[{0}] (Test_PK, Test_String) VALUES (@id, @stringValue)",
				tableName);

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@id", SqlDbType.UniqueIdentifier, id);
				cmd.AddParameter("@stringValue", SqlDbType.VarChar, 128, stringValue);
				cmd.ExecuteNonQuery();
			}
		}

		void InsertRowToStagingTable(DbConnection connection, string tableName, Guid pk, byte[] startLsn, int operationType)
		{
			var sqlText = string.Format(
				@"INSERT INTO [Staging].[{0}] (Test_PK, Test_Value, __$start_lsn, OP_TYPE) VALUES (@pk, @value, @startLsn, @opType)",
				tableName);

			using (((ICurrentDbControl)connection).UseDatabase(EdwDbName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@value", SqlDbType.Int, 1);
				cmd.AddParameter("@startLsn", SqlDbType.Binary, 10, startLsn);
				cmd.AddParameter("@opType", SqlDbType.TinyInt, operationType);

				cmd.ExecuteNonQuery();
			}
		}

		void UpdateRowInTestTable(DbConnection connection, string tableName, int previousValue, int value)
		{
			var sqlText = string.Format(@"UPDATE [Test].[{0}] SET Test_Value = @value WHERE Test_Value = @previousValue", tableName);

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@value", SqlDbType.Int, value);
				cmd.AddParameter("@previousValue", SqlDbType.Int, previousValue);

				cmd.ExecuteNonQuery();
			}
		}

		void UpdateRowInTestTable(DbConnection connection, string tableName, Guid pk, string value)
		{
			var sqlText = string.Format(@"UPDATE [Test].[{0}] SET Test_String = @value WHERE Test_PK = @pk", tableName);

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@value", SqlDbType.VarChar, 128, value);
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);

				cmd.ExecuteNonQuery();
			}
		}

		void InsertRowToTestTable(DbConnection connection, Guid pk, int value)
		{
			var sqlText = @"INSERT INTO [Test].[TestTable] (Test_PK, Test_Value) VALUES (@pk, @value)";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@value", SqlDbType.Int, value);
				cmd.ExecuteNonQuery();
			}
		}

		void InsertRowToTestChildTable(DbConnection connection, Guid pk, Guid parentPk)
		{
			var sqlText = @"INSERT INTO [Test].[TestChildTable] (TestChild_PK, TestParent_PK) VALUES (@pk, @parentPk)";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@parentPk", SqlDbType.UniqueIdentifier, parentPk);
				cmd.ExecuteNonQuery();
			}
		}

		void DeleteRowTestTable(DbConnection connection, string tableName, int value)
		{
			var sqlText = string.Format(@"DELETE [Test].[{1}] WHERE Test_Value = {0}", value, tableName);
			connection.ExecuteNonQuery(sqlText);
		}

		void InsertRowToSelfReferencedTestTable(DbConnection connection, string tableName, Guid pk, Guid? fk1 = null, Guid? fk2 = null, Guid? fk3 = null)
		{
			var sqlText = string.Format(
				@"INSERT INTO [Test].[{0}] (Test_PK{1}{2}{3}) VALUES ('{4}'{5}{6}{7})",
				tableName,
				(fk1 == null) ? "" : ", ParentPK1",
				(fk2 == null) ? "" : ", ParentPK2",
				(fk3 == null) ? "" : ", ParentPK3",
				pk.ToString(),
				(fk1 == null) ? "" : ", '" + fk1.ToString() + "'",
				(fk2 == null) ? "" : ", '" + fk2.ToString() + "'",
				(fk3 == null) ? "" : ", '" + fk3.ToString() + "'");

			connection.ExecuteNonQuery(sqlText);
		}

		void InsertRowToMultiSelfReferencedTestTable(DbConnection connection, string tableName, Guid pk, Guid? fk1, Guid? fk2, string strValue)
		{
			var sqlText = string.Format(
				@"INSERT INTO [Test].[{0}] (Test_PK{1}{2}, StringValue) VALUES ('{3}'{4}{5}, '{6}')",
				tableName,
				(fk1 == null) ? "" : ", ParentPK1",
				(fk2 == null) ? "" : ", ParentPK2",
				pk.ToString(),
				(fk1 == null) ? "" : ", '" + fk1.ToString() + "'",
				(fk2 == null) ? "" : ", '" + fk2.ToString() + "'",
				strValue);

			connection.ExecuteNonQuery(sqlText);
		}

		void UpdateRowInSelfReferencedTestTable(DbConnection connection, string tableName, Guid pk, Guid? fk1, Guid? fk2 = null, Guid? fk3 = null)
		{
			var sqlText = string.Format(
				@"UPDATE [Test].[{0}] SET ParentPK1 = {2}, ParentPK2 = {3}, ParentPK3 = {4} WHERE Test_PK ='{1}'",
				tableName,
				pk.ToString(),
				(fk1 == null) ? "NULL" : "'" + fk1.ToString() + "'",
				(fk2 == null) ? "NULL" : "'" + fk2.ToString() + "'",
				(fk3 == null) ? "NULL" : "'" + fk3.ToString() + "'");

			connection.ExecuteNonQuery(sqlText);
		}

		#endregion

		IEnumerable<(string, string)> StagingTableList => stagingTableList ?? (stagingTableList = CdcConfigTables.Where(t => t.TableInEdw && !t.IsEdiClient).Select(t => (t.SourceSchema, t.SourceTable)).ToArray());
		IEnumerable<(string, string)> stagingTableList;

		IEnumerable<BiAutomationConfigDataSet.EdwTableConfigRow> EdwBaseTableList
		{
			get
			{
				return edwBaseTableList ?? (edwBaseTableList = BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.Where(t => StagingTableList.Contains((t.SourceSchema, t.StagingTable))));
			}
		}
		IEnumerable<BiAutomationConfigDataSet.EdwTableConfigRow> edwBaseTableList;

		ILookup<string, BiAutomationConfigDataSet.EdwColumnConfigRow> EdwIndexForBaseTable
		{
			get
			{
				if (edwIndexForBaseTable is null)
				{
					edwIndexForBaseTable = BiAutomationConfigLoader.Instance.ConfigData.EdwColumnConfig.Where(c => c.EnableIndex).ToLookup(c => c.TableName, c => c);
				}
				return edwIndexForBaseTable;
			}
		}
		ILookup<string, BiAutomationConfigDataSet.EdwColumnConfigRow> edwIndexForBaseTable;

		BiAutomationConfigDataSet.EdwDenormalizedTableConfigDataTable EdwAggregateTableList
		{
			get
			{
				return edwAggregateTableList ?? (edwAggregateTableList = BiAutomationConfigLoader.Instance.ConfigData.EdwDenormalizedTableConfig);
			}
		}
		BiAutomationConfigDataSet.EdwDenormalizedTableConfigDataTable edwAggregateTableList;

		ILookup<string, BiAutomationConfigDataSet.EdwDenormalizedColumnConfigRow> EdwIndexForAggTable
		{
			get
			{
				if (edwIndexForAggTable is null)
				{
					edwIndexForAggTable = BiAutomationConfigLoader.Instance.ConfigData.EdwDenormalizedColumnConfig.Where(c => c.EnableIndex).ToLookup(c => c.TableName, c => c);
				}
				return edwIndexForAggTable;
			}
		}
		ILookup<string, BiAutomationConfigDataSet.EdwDenormalizedColumnConfigRow> edwIndexForAggTable;

		BiAutomationConfigDataSet.EdwCustomTableConfigDataTable EdwCustomTableList
		{
			get
			{
				return edwCustomTableList ?? (edwCustomTableList = BiAutomationConfigLoader.Instance.ConfigData.EdwCustomTableConfig);
			}
		}
		BiAutomationConfigDataSet.EdwCustomTableConfigDataTable edwCustomTableList;

		IEnumerable<BiAutomationConfigDataSet.CdcTableConfigRow> CdcConfigTables => cdcTables ?? (cdcTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => t.TableInEdw && !t.IsEdiClient));
		IEnumerable<BiAutomationConfigDataSet.CdcTableConfigRow> cdcTables;

		class ColumnDetails
		{
			public ColumnDetails(string tableName, string columnName, string dataType)
			{
				TableName = tableName;
				ColumnName = columnName;
				switch (dataType)
				{
					case "bigint":
						DataType = SqlDbType.BigInt;
						break;
					case "binary":
						DataType = SqlDbType.Binary;
						break;
					case "bit":
						DataType = SqlDbType.Bit;
						break;
					case "char":
						DataType = SqlDbType.Char;
						break;
					case "date":
						DataType = SqlDbType.Date;
						break;
					case "datetime":
						DataType = SqlDbType.DateTime;
						break;
					case "datetimeoffset":
						DataType = SqlDbType.DateTimeOffset;
						break;
					case "decimal":
						DataType = SqlDbType.Decimal;
						break;
					case "geography":
						DataType = SqlDbType.NVarChar; //or Udt, with a refactor
						break;
					case "image":
						DataType = SqlDbType.Image;
						break;
					case "int":
						DataType = SqlDbType.Int;
						break;
					case "money":
						DataType = SqlDbType.Money;
						break;
					case "nvarchar":
						DataType = SqlDbType.NVarChar;
						break;
					case "smalldatetime":
						DataType = SqlDbType.SmallDateTime;
						break;
					case "smallint":
						DataType = SqlDbType.SmallInt;
						break;
					case "tinyint":
						DataType = SqlDbType.TinyInt;
						break;
					case "uniqueidentifier":
						DataType = SqlDbType.UniqueIdentifier;
						break;
					case "varbinary":
						DataType = SqlDbType.VarBinary;
						break;
					case "varchar":
						DataType = SqlDbType.VarChar;
						break;
					case "xml":
						DataType = SqlDbType.Xml;
						break;
					case "timestamp":
						DataType = SqlDbType.Timestamp;
						break;
					default:
						throw new ArgumentException(string.Format("Unknown data type {0} for {1}", dataType, columnName));
				}
			}

			public string TableName { get; private set; }
			public string ColumnName { get; private set; }
			public SqlDbType DataType { get; private set; }
		}

		List<ColumnDetails> ColumnsList
		{
			get
			{
				if (columnsList == null)
				{
					columnsList = new List<ColumnDetails>();
					var query = string.Format(@"
select DISTINCT t.name, c.name, ty.name
from sys.columns c
inner join sys.tables t
on c.object_id = t.object_id
inner join sys.types ty
ON c.user_type_id = ty.user_type_id
WHERE t.name in ('{0}') AND ty.name NOT IN ('geography') AND ty.name NOT IN ('Timestamp')
", string.Join("','", CdcConfigTables.Select(t => t.SourceTable).Distinct()));

					using (var reader = Db.Connection.Command(query).ExecuteReader())
					{
						while (reader.Read())
						{
							var tableName = reader.GetString(0);
							var columnName = reader.GetString(1);
							var dataType = reader.GetString(2);
							columnsList.Add(new ColumnDetails(tableName, columnName, dataType));
						}
					}
				}
				return columnsList;
			}
		}
		List<ColumnDetails> columnsList;

		List<string> PrimaryKeys => primaryKeys ?? (primaryKeys = CdcConfigTables.Where(t => t.TableInEdw && !t.IsEdiClient).Select(t => t.GetCdcColumnConfigRows().FirstOrDefault(c => c.IsPrimaryKey).SourceColumn).ToList());
		List<string> primaryKeys;

		readonly Guid initialId = Guid.NewGuid();
		readonly Guid insertedId = Guid.NewGuid();

		IEnumerable<string> CalculatedColumns
		{
			get
			{
				if (calculatedColumns == null)
				{
					var sqlText = "select name from sys.columns where is_computed = 1";
					calculatedColumns = DataUtils.GetListOfValuesFromQuery(Db.Connection, sqlText);
				}
				return calculatedColumns;
			}
		}
		IEnumerable<string> calculatedColumns;

		Dictionary<SqlDbType, object> InitialValuesDictionary
		{
			get
			{
				if (initialValuesDictionary == null)
				{
					initialValuesDictionary = new Dictionary<SqlDbType, object>();
					initialValuesDictionary[SqlDbType.Binary] = 1;
					initialValuesDictionary[SqlDbType.BigInt] = 1;
					initialValuesDictionary[SqlDbType.Bit] = 0;
					initialValuesDictionary[SqlDbType.Char] = ".";
					initialValuesDictionary[SqlDbType.Date] = DateTime.Now.Date.ToString();
					initialValuesDictionary[SqlDbType.DateTime] = DateTime.Now.ToString();
					initialValuesDictionary[SqlDbType.DateTimeOffset] = DateTimeOffset.Now.ToString();
					initialValuesDictionary[SqlDbType.Decimal] = 0.1;
					initialValuesDictionary[SqlDbType.Int] = 1;
					initialValuesDictionary[SqlDbType.Money] = 1;
					initialValuesDictionary[SqlDbType.NChar] = ".";
					initialValuesDictionary[SqlDbType.NVarChar] = ".";
					initialValuesDictionary[SqlDbType.SmallDateTime] = DateTime.Now.ToShortDateString();
					initialValuesDictionary[SqlDbType.SmallInt] = 1;
					initialValuesDictionary[SqlDbType.TinyInt] = 1;
					initialValuesDictionary[SqlDbType.UniqueIdentifier] = Guid.NewGuid();
					initialValuesDictionary[SqlDbType.VarBinary] = new byte[] { 0x40 };
					initialValuesDictionary[SqlDbType.VarChar] = ".";
					initialValuesDictionary[SqlDbType.Xml] = "";
					initialValuesDictionary[SqlDbType.Timestamp] = new byte[8];
				}
				return initialValuesDictionary;
			}
		}
		Dictionary<SqlDbType, object> initialValuesDictionary;

		Dictionary<SqlDbType, object> InsertedValuesDictionary
		{
			get
			{
				if (insertedValuesDictionary == null)
				{
					insertedValuesDictionary = new Dictionary<SqlDbType, object>();
					insertedValuesDictionary[SqlDbType.Binary] = 1;
					insertedValuesDictionary[SqlDbType.BigInt] = 1;
					insertedValuesDictionary[SqlDbType.Bit] = 0;
					insertedValuesDictionary[SqlDbType.Char] = "?";
					insertedValuesDictionary[SqlDbType.Date] = DateTime.Now.Date.ToString();
					insertedValuesDictionary[SqlDbType.DateTime] = DateTime.Now.ToString();
					insertedValuesDictionary[SqlDbType.DateTimeOffset] = DateTimeOffset.Now.ToString();
					insertedValuesDictionary[SqlDbType.Decimal] = 0.2;
					insertedValuesDictionary[SqlDbType.Int] = 2;
					insertedValuesDictionary[SqlDbType.Money] = 2;
					insertedValuesDictionary[SqlDbType.NChar] = "?";
					insertedValuesDictionary[SqlDbType.NVarChar] = "?";
					insertedValuesDictionary[SqlDbType.SmallDateTime] = DateTime.Now.ToShortDateString();
					insertedValuesDictionary[SqlDbType.SmallInt] = 2;
					insertedValuesDictionary[SqlDbType.TinyInt] = 2;
					insertedValuesDictionary[SqlDbType.UniqueIdentifier] = Guid.NewGuid();
					insertedValuesDictionary[SqlDbType.VarBinary] = new byte[] { 0x20 };
					insertedValuesDictionary[SqlDbType.VarChar] = "?";
					insertedValuesDictionary[SqlDbType.Xml] = "";
					initialValuesDictionary[SqlDbType.Timestamp] = new byte[8];
				}
				return insertedValuesDictionary;
			}
		}
		Dictionary<SqlDbType, object> insertedValuesDictionary;

		Dictionary<SqlDbType, object> UpdatedValuesDictionary
		{
			get
			{
				if (updatedValuesDictionary == null)
				{
					updatedValuesDictionary = new Dictionary<SqlDbType, object>();
					updatedValuesDictionary[SqlDbType.Binary] = 0;
					updatedValuesDictionary[SqlDbType.BigInt] = 0;
					updatedValuesDictionary[SqlDbType.Bit] = 0;
					updatedValuesDictionary[SqlDbType.Char] = "#";
					updatedValuesDictionary[SqlDbType.Date] = DateTime.Now.Date.ToString();
					updatedValuesDictionary[SqlDbType.DateTime] = DateTime.Now.ToString();
					updatedValuesDictionary[SqlDbType.DateTimeOffset] = DateTimeOffset.Now.ToString();
					updatedValuesDictionary[SqlDbType.Decimal] = 0.3;
					updatedValuesDictionary[SqlDbType.Int] = 3;
					updatedValuesDictionary[SqlDbType.Money] = 3;
					updatedValuesDictionary[SqlDbType.NChar] = "#";
					updatedValuesDictionary[SqlDbType.NVarChar] = "#";
					updatedValuesDictionary[SqlDbType.SmallDateTime] = DateTime.Now.ToShortDateString();
					updatedValuesDictionary[SqlDbType.SmallInt] = 3;
					updatedValuesDictionary[SqlDbType.TinyInt] = 3;
					updatedValuesDictionary[SqlDbType.UniqueIdentifier] = Guid.NewGuid();
					updatedValuesDictionary[SqlDbType.VarBinary] = new byte[] { 0x30 };
					updatedValuesDictionary[SqlDbType.VarChar] = "#";
					updatedValuesDictionary[SqlDbType.Xml] = "";
					initialValuesDictionary[SqlDbType.Timestamp] = new byte[8];
				}
				return updatedValuesDictionary;
			}
		}
		Dictionary<SqlDbType, object> updatedValuesDictionary;

		#endregion

		class EdwTsqlScriptRunnerForTest : EdwTsqlScriptRunner
		{
			public EdwTsqlScriptRunnerForTest(DbConnection biConnection, ILogger logger)
				: base(biConnection, logger)
			{
				InitialLoadBatchSizeForTest = 10000000;
				MinimumRowSizeForPartitioningForTest = 10000000;
			}

			public EdwStagingMergeResult ExecuteStagingMerge_Exposed()
			{
				using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
				{
					return ExecuteStagingMerge();
				}
			}

			public int InitialLoadBatchSizeForTest { get; set; }

			protected override int InitialLoadBatchSize
			{
				get
				{
					return InitialLoadBatchSizeForTest;
				}
			}

			public int MinimumRowSizeForPartitioningForTest { get; set; }
			protected override long MinimumRowSizeForPartitioning => MinimumRowSizeForPartitioningForTest;

			public int FuturePeriodsForPartitioningForTest { get; set; }
			protected override int FuturePeriodsForPartitioning => FuturePeriodsForPartitioningForTest;
		}

		class TsqlScriptRunnerForTest_InsufficientMemory : TsqlScriptRunner
		{
			public TsqlScriptRunnerForTest_InsufficientMemory(DbConnection biConnection, ILogger logger) : base(biConnection, logger)
			{
			}

			protected override void RunUnsafe()
			{
				var sqlException = SqlExceptionBuilder.CreateSqlException(701, "There is insufficient system memory in resource pool default to run this query.");
				throw sqlException;
			}

			#region Not Implemented

			public override string ScriptName => throw new NotImplementedException();
			public override string BiDatabaseName => throw new NotImplementedException();

			public override bool IsInitialLoad()
			{
				throw new NotImplementedException();
			}

			public override bool ShouldRunEtl()
			{
				throw new NotImplementedException();
			}

			protected override IEnumerable<string> GetErrorList()
			{
				throw new NotImplementedException();
			}

			#endregion
		}

		class EdwTsqlScriptRunnerForDuplicateKeyTest : EdwTsqlScriptRunnerForTest
		{
			public EdwTsqlScriptRunnerForDuplicateKeyTest(DbConnection biConnection, ILogger logger) : base(biConnection, logger)
			{
			}

			protected override EdwTransformLoadResult ExecuteTransformMasterLoad(bool truncateModelTables)
			{
				return EdwTransformLoadResult.TransformFailure;
			}

			public void TransformMasterLoadExposed()
			{
				TransformMasterLoad(true);
			}
		}
		#endregion
	}
}
