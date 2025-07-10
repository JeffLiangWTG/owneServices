using System;
using CargoWise.Data;
using NUnit.Framework;

namespace CargoWise.Bi.ConfigLoader.Testing
{
	class DenormalizedTableLoadTest : TransactionedTestCase
	{
		#region Test Scenarios

		// Initial Test Data:
		// JobHeader										|	GLTransactionHeader																							|	GLTransactionLine
		// JobHeaderKey	|JobHeaderDesc	|	GLTransactionHeaderKey	|JobHeaderKey	|GLTransactionHeaderDesc	|	GLTransactionLineKey	|GLTransactionHeaderKey	|JobHeaderKey	|GLTransactionLineDesc
		// 1						|'JH1'					|	100											|1						|'TH100'									|	10										|100										|NULL					|'TL10'
		// 2						|'JH2'					|	101											|NULL					|'TH101'									|	11										|100										|4						|'TL11'
		// 3						|'JH3'					|	102											|2						|'TH102'									|	12										|NULL										|2						|'TL12'
		// 4						|'JH4'					|	103											|NULL					|'TH103'									|	13										|101										|3						|'TL13'
		// 5						|'JH5'					|	104											|NULL					|'TH104'									|	14										|103										|5						|'TL14'
		// 6						|'JH6'					|																																	|	15										|103										|6						|'TL15'
		//															|																																	|	16										|NULL										|NULL					|'TL16'
		//															|																																	|	17										|NULL										|NULL					|'TL17'

		public void TestNewLineToExistingJobHeaderAndTransactionHeaderWithoutChildren()
		{
			InitializeTest();
			InsertGLTransactionLine(lineKey: 20, headerKey: 104, jobHeaderKey: 4, lineDesc: "TL20");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestNewLineToExistingJobHeaderWithoutChildren()
		{
			InitializeTest();
			InsertGLTransactionLine(lineKey: 20, headerKey: null, jobHeaderKey: 4, lineDesc: "TL20");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestOrphanedLine()
		{
			InitializeTest();
			InsertGLTransactionLine(lineKey: 20, headerKey: null, jobHeaderKey: null, lineDesc: "TL20");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestNewLineToExistingTransactionHeaderWithoutChildren()
		{
			InitializeTest();
			InsertGLTransactionLine(lineKey: 20, headerKey: 104, jobHeaderKey: null, lineDesc: "TL20");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestUpdateLineWithoutTransactionHeader()
		{
			InitializeTest();
			DeleteGLTransactionLine(12);
			InsertGLTransactionLine(lineKey: 12, headerKey: null, jobHeaderKey: 2, lineDesc: "TL12-updated");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestAssignLineToJobHeaderWithLine()
		{
			InitializeTest();
			DeleteGLTransactionLine(12);
			InsertGLTransactionLine(lineKey: 12, headerKey: null, jobHeaderKey: 1, lineDesc: "TL12-updated");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestReassignLineToTransactionHeaderWithLine()
		{
			InitializeTest();
			DeleteGLTransactionLine(12);
			InsertGLTransactionLine(lineKey: 12, headerKey: 104, jobHeaderKey: null, lineDesc: "TL12-updated");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestReassignLineToTransactionHeaderWithoutLines()
		{
			InitializeTest();
			DeleteGLTransactionLine(12);
			InsertGLTransactionLine(lineKey: 12, headerKey: 103, jobHeaderKey: null, lineDesc: "TL12-updated");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestReassignLineToTransactionHeaderWithLineAndJobHeader()
		{
			InitializeTest();
			DeleteGLTransactionLine(12);
			InsertGLTransactionLine(lineKey: 12, headerKey: 100, jobHeaderKey: null, lineDesc: "TL12-updated");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestReassignMultipleLinesToTransactionHeaderAndJobHeader()
		{
			InitializeTest();
			DeleteGLTransactionLine(10);
			DeleteGLTransactionLine(11);
			InsertGLTransactionLine(lineKey: 10, headerKey: 101, jobHeaderKey: 3, lineDesc: "TL10-updated");
			InsertGLTransactionLine(lineKey: 11, headerKey: 101, jobHeaderKey: 3, lineDesc: "TL10-updated");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestDeleteLine()
		{
			InitializeTest();
			DeleteGLTransactionLine(10);
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestDeleteAllLinesInATransactionHeader()
		{
			InitializeTest();
			DeleteGLTransactionLine(10);
			DeleteGLTransactionLine(11);
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestDeleteLineWithoutTransactionHeaderAttachedToJobHeader()
		{
			InitializeTest();
			DeleteGLTransactionLine(13);
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestDeleteAllLines()
		{
			InitializeTest();
			DeleteGLTransactionLine();
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestInsertNewJobHeader()
		{
			InitializeTest();
			InsertJobHeader(jobHeaderKey: 9, jobHeaderDesc: "JH9-New");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestJobHeaderWithMultipleLines()
		{
			InitializeTest();
			DeleteJobHeader(1);
			InsertJobHeader(jobHeaderKey: 1, jobHeaderDesc: "JH1-Updated");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestUpdateJobHeaders()
		{
			InitializeTest();
			DeleteJobHeader(2);
			DeleteJobHeader(3);
			InsertJobHeader(jobHeaderKey: 2, jobHeaderDesc: "JH2-Updated");
			InsertJobHeader(jobHeaderKey: 3, jobHeaderDesc: "JH3-Updated");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestInsertTransactionHeaderWithoutJobHeader()
		{
			InitializeTest();
			InsertGLTransactionHeader(headerKey: 105, jobHeaderKey: null, headerDesc: "TH105-New");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestInsertTransactionHeaderWithJobHeader()
		{
			InitializeTest();
			InsertGLTransactionHeader(headerKey: 105, jobHeaderKey: 4, headerDesc: "TH105-New");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestReassignTransactionHeaderToDifferentJobHeader()
		{
			InitializeTest();
			DeleteGLTransactionHeader(100);
			InsertGLTransactionHeader(headerKey: 100, jobHeaderKey: 2, headerDesc: "TH100-Updated");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestAssignTransactionHeaderToJobHeader()
		{
			InitializeTest();
			DeleteGLTransactionHeader(101);
			InsertGLTransactionHeader(headerKey: 101, jobHeaderKey: 2, headerDesc: "TH100-Updated");
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestDeleteTransactionHeaderWithJobHeader()
		{
			InitializeTest();
			DeleteGLTransactionHeader(100);
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestDeleteMultipleTransactionHeaders()
		{
			InitializeTest();
			DeleteGLTransactionHeader(100);
			DeleteGLTransactionHeader(102);
			DeleteGLTransactionHeader(103);
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestDeleteTransactionHeaderWithoutJobHeaderAndLine()
		{
			InitializeTest();
			DeleteGLTransactionHeader(103);
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestDeleteAllTransactionHeaders()
		{
			InitializeTest();
			DeleteGLTransactionHeader();
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestDeleteAllTransactionHeadersAndJobHeaders()
		{
			InitializeTest();
			DeleteGLTransactionHeader();
			DeleteJobHeader();
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestDeleteAllTransactionHeadersJobHeadersAndLines()
		{
			InitializeTest();
			DeleteGLTransactionHeader();
			DeleteJobHeader();
			DeleteGLTransactionLine();
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestDeleteAllTransactionHeadersJobAndLines()
		{
			InitializeTest();
			DeleteGLTransactionHeader();
			DeleteGLTransactionLine();
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestDeleteAllJobHeadersAndLines()
		{
			InitializeTest();
			DeleteJobHeader();
			DeleteGLTransactionLine();
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestNoChange()
		{
			InitializeTest();
			AssertIncrementalLoadAggregateTableResult();
		}

		public void TestAggTableValidationWith1SourceTable()
		{
			var denormTable = ConfigData.EdwDenormalizedTableConfig.AddEdwDenormalizedTableConfigRow("Test", "AGG_Table",
@"[Test].[BAS__Table1] [t1]", -1, false, "", "");

			denormTable.Validate();
			Assert("Validation for table with 1 source table", denormTable.Message.Contains("Aggregate Table should have more than 1 source table."));
		}

		public void TestAggTableValidationWith2SourceTables()
		{
			var denormTable = ConfigData.EdwDenormalizedTableConfig.AddEdwDenormalizedTableConfigRow("Test", "AGG_Table",
 @"[Test].[BAS__Table1] [t1]
 full join [Test].[BAS__Table2] [t2] on [t1].[Key] = [t1].[Key2]", -1, false, "", "");

			denormTable.Validate();
			Assert("Validation for table with 2 source tables", !denormTable.Message.Contains("Aggregate Table should have more than 1 source table."));
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			BiAutomationConfigLoader.Instance.ResetConfiguration();
			base.TearDown();
		}

		protected override DbConnection TestConnection
		{
			get
			{
				return testConnection ?? (testConnection = Db.NewAdminConnection(Db.EdwDatabaseName));
			}
		}
		DbConnection testConnection;

		void InitializeTest()
		{
			PrepareTestSchema();
			CreateTestDenormTable();
			PopulateInitialData();
			RunInitialLoad();
		}

		void CreateTestDenormTable()
		{
			ConfigData.EdwDenormalizedColumnConfig.Clear();
			ConfigData.EdwDenormalizedTableConfig.Clear();

			var denormTable = ConfigData.EdwDenormalizedTableConfig.AddEdwDenormalizedTableConfigRow("Test", "AGG__GLTransaction",
@"[Test].[BAS__GLTransactionHeader] [th]
 full join [Test].[BAS__GLTransactionLine] [tl] on [tl].[GLTransactionHeaderKey] = [th].[GLTransactionHeaderKey]
 full join [Test].[BAS__JobHeader] [jh] on [jh].[JobHeaderKey] = case when [th].[JobHeaderKey] is not null then [th].[JobHeaderKey] else [tl].[JobHeaderKey] end", -1, false, "", "");

			ConfigData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(denormTable, "GLTransactionKey", "bigint", 0, 0, 0, "", false, "", false);
			ConfigData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(denormTable, "GLTransactionHeaderKey", "int", 0, 0, 0, "[th].[GLTransactionHeaderKey]", false, "", false);
			ConfigData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(denormTable, "GLTransactionHeaderDesc", "varchar", 50, 0, 0, "[th].[GLTransactionHeaderDesc]", false, "", false);
			ConfigData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(denormTable, "GLTransactionLineKey", "int", 0, 0, 0, "[tl].[GLTransactionLineKey]", false, "", false);
			ConfigData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(denormTable, "GLTransactionLineDesc", "varchar", 50, 0, 0, "[tl].[GLTransactionLineDesc]", false, "", false);
			ConfigData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(denormTable, "JobHeaderKey", "int", 0, 0, 0, "[jh].[JobHeaderKey]", false, "", false);
			ConfigData.EdwDenormalizedColumnConfig.AddEdwDenormalizedColumnConfigRow(denormTable, "JobHeaderDesc", "varchar", 50, 0, 0, "[jh].[JobHeaderDesc]", false, "", false);

			var createViewScript = ConfigData.GetCreateDenormalizedTableViewQuery(denormTable);
			TestConnection.ExecuteNonQuery(createViewScript);
			var initialLoadScript = ConfigData.GetInitialLoadQueryForDenormalizedTable(denormTable);
			TestConnection.ExecuteNonQuery(initialLoadScript);
			var incrementalLoadScript = ConfigData.GetIncrementalLoadQueryForDenormalizedTable(denormTable);
			TestConnection.ExecuteNonQuery(incrementalLoadScript);
		}

		void PrepareTestSchema()
		{
			var sqlText = @"IF NOT EXISTS (SELECT NULL FROM sys.schemas WHERE name = 'Test') EXEC ('CREATE SCHEMA [Test]');";
			TestConnection.ExecuteNonQuery(sqlText);

			sqlText = @"
				create table [Test].[AGG__GLTransaction]
				(
					GLTransactionKey bigint,
					GLTransactionHeaderKey int,
					GLTransactionLineKey int,
					JobHeaderKey int,
					GLTransactionHeaderDesc VARCHAR(50),
					GLTransactionLineDesc VARCHAR(50),
					JobHeaderDesc VARCHAR(50)
				)
				create table [Test].[AGG__GLTransaction_Reference]
				(
					GLTransactionHeaderKey int,
					GLTransactionLineKey int,
					JobHeaderKey int,
					GLTransactionHeaderDesc VARCHAR(50),
					GLTransactionLineDesc VARCHAR(50),
					JobHeaderDesc VARCHAR(50)
				)
				create table [Test].[BAS__GLTransactionHeader]
				(
					GLTransactionHeaderKey int,
					GLTransactionHeaderID uniqueidentifier,
					JobHeaderKey int,
					GLTransactionHeaderDesc VARCHAR(50)
				)
				create table [Test].[BAS__GLTransactionLine]
				(
					GLTransactionLineKey int,
					GLTransactionLineID uniqueidentifier,
					GLTransactionHeaderKey int,
					JobHeaderKey int,
					GLTransactionLineDesc VARCHAR(50)
				)
				create table [Test].[BAS__JobHeader]
				(
					JobHeaderKey int,
					JobHeaderID uniqueidentifier,
					JobHeaderDesc VARCHAR(50)
				)";

			TestConnection.ExecuteNonQuery(sqlText);

			sqlText = @"
				TRUNCATE TABLE [biadmin].[TransformedRow]";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void PopulateInitialData()
		{
			var sqlText = @"
				insert into [Test].[BAS__JobHeader] (JobHeaderKey, JobHeaderID, JobHeaderDesc)
				values (1, newid(), 'JH1'), (2, newid(), 'JH2'), (3, newid(), 'JH3'), (4, newid(), 'JH4'), (5, newid(), 'JH5'), (6, newid(), 'JH6')

				insert into [Test].[BAS__GLTransactionHeader] (GLTransactionHeaderKey, GLTransactionHeaderID, JobHeaderKey, GLTransactionHeaderDesc)
				values (100, newid(), 1, 'TH100'), (101, newid(), NULL, 'TH101'), (102, newid(), 2, 'TH102'), (103, newid(), NULL, 'TH103'), (104, newid(), NULL, 'TH104')

				insert into [Test].[BAS__GLTransactionLine] (GLTransactionLineKey, GLTransactionLineID, GLTransactionHeaderKey, JobHeaderKey, GLTransactionLineDesc)
				values (10, newid(), 100, NULL, 'TL10'), (11, newid(), 100, 4, 'TL11'), (12, newid(), NULL, 2, 'TL12'), (13, newid(), 101, 3, 'TL13'), (14, newid(), 103, 5, 'TL14'), (15, newid(), 103, 6, 'TL15'), (16, newid(), NULL, NULL, 'TL16'), (17, newid(), NULL, NULL, 'TL16')";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void InsertJobHeader(int jobHeaderKey, string jobHeaderDesc)
		{
			var sqlText = String.Format(@"
				INSERT INTO [Test].[BAS__JobHeader] (JobHeaderKey, JobHeaderDesc, JobHeaderID)
				VALUES ({0}, '{1}', newid())

				INSERT INTO [biadmin].[TransformedRow] (SchemaName, TableName, KeyValue)
				VALUES('Test', 'BAS__JobHeader', {0})",
					jobHeaderKey, jobHeaderDesc);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void InsertGLTransactionHeader(int headerKey, int? jobHeaderKey, string headerDesc)
		{
			var sqlText = String.Format(@"
				INSERT INTO [Test].[BAS__GLTransactionHeader] (GLTransactionHeaderKey, JobHeaderKey, GLTransactionHeaderDesc, GLTransactionHeaderID)
				VALUES ({0}, {1}, '{2}', newid())

				INSERT INTO [biadmin].[TransformedRow] (SchemaName, TableName, KeyValue)
				VALUES('Test', 'BAS__GLTransactionHeader', {0})",
					headerKey, jobHeaderKey == null ? "NULL" : jobHeaderKey.ToString(), headerDesc);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void InsertGLTransactionLine(int lineKey, int? headerKey, int? jobHeaderKey, string lineDesc)
		{
			var sqlText = String.Format(@"
				INSERT INTO [Test].[BAS__GLTransactionLine] (GLTransactionLineKey, GLTransactionHeaderKey, JobHeaderKey, GLTransactionLineDesc, GLTransactionLineID)
				VALUES ({0}, {1}, {2}, '{3}', newid())

				INSERT INTO [biadmin].[TransformedRow] (SchemaName, TableName, KeyValue)
				VALUES('Test', 'BAS__GLTransactionLine', {0})",
					lineKey, headerKey == null ? "NULL" : headerKey.ToString(), jobHeaderKey == null ? "NULL" : jobHeaderKey.ToString(), lineDesc);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void DeleteJobHeader(int? jobHeaderKey = null)
		{
			var sqlText = String.Format(@"
				DELETE FROM [Test].[BAS__JobHeader]
				OUTPUT 'Test', 'BAS__JobHeader', deleted.[JobHeaderKey], deleted.[JobHeaderID]
					INTO [biadmin].[TransformedRow] (SchemaName, TableName, KeyValue, PKValue)
				{0}",
					(jobHeaderKey != null) ? "WHERE JobHeaderKey = " + jobHeaderKey : "");

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void DeleteGLTransactionHeader(int? headerKey = null)
		{
			var sqlText = String.Format(@"
				DELETE FROM [Test].[BAS__GLTransactionHeader]
				OUTPUT 'Test', 'BAS__GLTransactionHeader', deleted.[GLTransactionHeaderKey], deleted.[GLTransactionHeaderID]
					INTO [biadmin].[TransformedRow] (SchemaName, TableName, KeyValue, PKValue)
				{0}",
					(headerKey != null) ? "WHERE GLTransactionHeaderKey = " + headerKey : "");

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void DeleteGLTransactionLine(int? lineKey = null)
		{
			var sqlText = String.Format(@"
				DELETE FROM [Test].[BAS__GLTransactionLine]
				OUTPUT 'Test', 'BAS__GLTransactionLine', deleted.[GLTransactionLineKey], deleted.[GLTransactionLineID]
					INTO [biadmin].[TransformedRow] (SchemaName, TableName, KeyValue, PKValue)
				{0}",
					(lineKey != null) ? "WHERE GLTransactionLineKey = " + lineKey : "");

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void RunInitialLoad()
		{
			var sqlText = @"
					EXEC('[Test].[usp_IniLoad_AGG__GLTransaction]');
					TRUNCATE TABLE [biadmin].[TransformedRow];";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void RunIncrementalLoad()
		{
			var sqlText = @"
					EXEC('[Test].[usp_IncLoad_AGG__GLTransaction]');
					TRUNCATE TABLE [biadmin].[TransformedRow];";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void AssertIncrementalLoadAggregateTableResult()
		{
			RunIncrementalLoad();

			var sqlText = @"
				TRUNCATE TABLE [Test].[AGG__GLTransaction_Reference]

				DELETE FROM [Test].[AGG__GLTransaction]
				OUTPUT deleted.[GLTransactionHeaderKey], deleted.[GLTransactionLineKey], deleted.[JobHeaderKey], deleted.[GLTransactionHeaderDesc], deleted.[GLTransactionLineDesc], deleted.[JobHeaderDesc]
					INTO [Test].[AGG__GLTransaction_Reference] (GLTransactionHeaderKey, GLTransactionLineKey, JobHeaderKey, GLTransactionHeaderDesc, GLTransactionLineDesc, JobHeaderDesc)";
			TestConnection.ExecuteNonQuery(sqlText);

			RunInitialLoad();

			CombineAssertions(() =>
			{
				var aggregateTableCountQuery = @"SELECT COUNT(*) FROM [Test].[{0}];";
				var expectedCount = Convert.ToInt64(TestConnection.ExecuteScalar(String.Format(aggregateTableCountQuery, "AGG__GLTransaction")));
				var actualCount = Convert.ToInt64(TestConnection.ExecuteScalar(String.Format(aggregateTableCountQuery, "AGG__GLTransaction_Reference")));
				AssertEquals("Count", expectedCount, actualCount);

				var aggregateTableCheckSumQuery = @"SELECT SUM(CAST(CHECKSUM({0}) AS BIGINT)) FROM [Test].[{1}];";
				var columnList = "GLTransactionHeaderKey, GLTransactionLineKey, JobHeaderKey, GLTransactionHeaderDesc, GLTransactionLineDesc, JobHeaderDesc";

				var expectedObj = TestConnection.ExecuteScalar(String.Format(aggregateTableCheckSumQuery, columnList, "AGG__GLTransaction"));
				var expectedCheckSum = expectedObj == DBNull.Value ? 0 : Convert.ToInt64(expectedObj);

				var actualObj = TestConnection.ExecuteScalar(String.Format(aggregateTableCheckSumQuery, columnList, "AGG__GLTransaction_Reference"));
				var actualCheckSum = actualObj == DBNull.Value ? 0 : Convert.ToInt64(actualObj);

				AssertEquals("CheckSum", expectedCheckSum, actualCheckSum);
			});
		}

		BiConfigurationData ConfigData
		{
			get
			{
				if (configData == null)
				{
					BiAutomationConfigLoader.Instance.ResetConfiguration();
					configData = BiAutomationConfigLoader.Instance.ConfigData;
				}
				return configData;
			}
		}
		BiConfigurationData configData;

		#endregion
	}
}
