using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	#region ZSqlLoaderMSBugWorkAroundTest

	sealed class ZSqlLoaderMSBugWorkAroundTest : TestCase
	{
		#region Sql Scripts

		readonly string SetupTableScript = @"

create table MyTable1
(
	M1_PK uniqueidentifier not null CONSTRAINT [MyTable1_PK_NotNull] DEFAULT (newid()),
	M1_Data char(10),
CONSTRAINT [MyTable1_PK_Unique] PRIMARY KEY  NONCLUSTERED
	(
		[M1_PK]
	)  ON [PRIMARY]
)

insert into MyTable1(M1_PK, M1_Data) values ('F57E2C09-98F2-48ee-9764-D14E0CF4730A', 'data1')

create table MyTable2
(
	M2_PK uniqueidentifier not null CONSTRAINT [MyTable2_PK_NotNull] DEFAULT (newid()),
	M2_M1 uniqueidentifier,
	M2_Data char(10),
CONSTRAINT [MyTable2_PK_Unique] PRIMARY KEY  NONCLUSTERED
	(
		[M2_PK]
	)  ON [PRIMARY]
)

insert into MyTable2(M2_PK, M2_M1, M2_Data) values (newid(), 'F57E2C09-98F2-48ee-9764-D14E0CF4730A', 'data2')

";

		readonly string SetupViewScript = @"
create view MyView as
select
	M1_PK as MY_PK,
	M1_Data as MY_Data1,
	M2_Data as MY_Data2
from MyTable1
inner join MyTable2 on M1_PK = M2_M1
";

		readonly string CleanUpScript = @"
if ((select count(*) from sys.objects where type in ('U', 'V') and name='MyTable1') = 1)
drop table MyTable1

if ((select count(*) from sys.objects where type in ('U', 'V') and name='MyTable2') = 1)
drop table MyTable2

if ((select count(*) from sys.objects where type in ('U', 'V') and name='MyView') = 1)
drop view MyView
";

		#endregion
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]

		public void TestWorkaroundForMSBugWithFillSchemaAndDataReader()
		{
			try
			{
				ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
				ZSqlLoader loader = new ZSqlLoader(new DataSet(), connectionInfo, ObjectFactory.Get<IApplicationSchemaResolver>());

				SetupTablesAndView();

				DataTable myViewTable = loader.GetTable("MyView");
				myViewTable.PrimaryKey = new DataColumn[] { myViewTable.Columns[0] };
				AssertEquals("Should only have columns from MyView", 3, myViewTable.Columns.Count);
				Assert("Contains MY_PK", myViewTable.Columns.Contains("MY_PK"));
				Assert("Contains MY_DATA1", myViewTable.Columns.Contains("MY_DATA1"));
				Assert("Contains MY_DATA2", myViewTable.Columns.Contains("MY_DATA2"));
				Assert("Does not contain M2_PK. .NET Bug is to add M2_PK.", !myViewTable.Columns.Contains("M2_PK"));
				AssertEquals("Empty table", 0, myViewTable.Rows.Count);

				try
				{
					loader.GetTable("IDoNotExist");
				}
				catch (ZException e)
				{
					AssertEquals("The message should be as shown", "Table IDoNotExist does not exist.", e.Message);
				}

				loader.LoadPersistentRowsIntoDataSet(new ZDataQuery(connectionInfo, "MyView", new ZQuery()));
				AssertEquals("Successful load", 1, myViewTable.Rows.Count);
			}
			finally
			{
				DropTablesAndViews();
				DbCommitTracker.Ignore("MyTable");
			}
		}

		void SetupTablesAndView()
		{
			Db.Connection.ExecuteNonQuery(SetupTableScript);
			Db.Connection.ExecuteNonQuery(SetupViewScript);
		}

		void DropTablesAndViews()
		{
			Db.Connection.ExecuteNonQuery(CleanUpScript);
		}
	}

	#endregion

	#region ZSqlLoaderTest

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
	sealed class ZSqlLoaderTest : TestCaseWithDummy
	{
		#region Test LoadBlobFields

		public void TestLoadBlobFields()
		{
			Dummy.FillWithValidTestData();
			Dummy.Z0_VarCharMax = new string('x', 1050);
			byte[] buffer = new byte[1050];
			new Random().NextBytes(buffer);
			Dummy.Z0_VarBinaryMax = buffer;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DataRow row = factory2.Load(typeof(DummyBusinessObject), Dummy.PK).Row;

			ZDataRowDictionary rows = new ZDataRowDictionary();
			rows.Add(row[DummyBizoSchema.Constants.PK], row);
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());

			Assert("Precondition - blob Z0_VarCharMax should not be loaded.", LazyLoading.LoadRequired(row[DummyBizoSchema.Constants.Z0_VarCharMax]));
			Assert("Precondition - blob Z0_VarBinaryMax should not be loaded.", LazyLoading.LoadRequired(row[DummyBizoSchema.Constants.Z0_VarBinaryMax]));
			AssertEquals("Precondition - DataRow state should be unchanged.", DataRowState.Unchanged, row.RowState);

			loader.LoadBlobFieldsForTable(DummyBizoSchema.Constants.TableName, rows, new HashSet<SchemaColumn>(new SchemaColumn[] { DummyBizoSchema.Z0_VarCharMax, DummyBizoSchema.Z0_VarBinaryMax }));

			AssertEquals("Should have loaded in blob Z0_VarCharMax.", new string('x', 1050), row[DummyBizoSchema.Constants.Z0_VarCharMax]);
			AssertEquals("Should have loaded in blob Z0_VarBinaryMax.", new ZBlob(buffer), new ZBlob(row[DummyBizoSchema.Constants.Z0_VarBinaryMax]));
			AssertEquals("After loaded in blobs, DataRow state should remain unchanged.", DataRowState.Unchanged, row.RowState);
		}

		public void TestLoadBlobField_ConcurrentlyDeleted()
		{
			Dummy.FillWithValidTestData();
			byte[] buffer = new byte[1050];
			new Random().NextBytes(buffer);
			Dummy.Z0_VarBinaryMax = buffer;
			Dummy.Z0_VarCharMax = Encoding.ASCII.GetString(buffer);

			Factory.Save();

			var factory = new BusinessObjectFactory();
			DataRow row = factory.Load(typeof(DummyBusinessObject), Dummy.PK).Row;
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());

			Db.Connection.Command(String.Format("delete from dbo.dummybizo where z0_pk = '{0}'", Dummy.PK)).ExecuteNonQuery();

			Assert(LazyLoading.LoadRequired(row[DummyBizoSchema.Z0_VarBinaryMax.Name]));
			Assert(LazyLoading.LoadRequired(row[DummyBizoSchema.Z0_VarCharMax.Name]));
			AssertExceptionThrown(typeof(SqlStreamReaderRowNotFoundException), () => { loader.LoadBlobField(row, DummyBizoSchema.Z0_VarBinaryMax); });
			AssertExceptionThrown(typeof(SqlStreamReaderRowNotFoundException), () => { loader.LoadBlobField(row, DummyBizoSchema.Z0_VarCharMax); });
			Assert(LazyLoading.LoadRequired(row[DummyBizoSchema.Z0_VarBinaryMax.Name]));
			Assert(LazyLoading.LoadRequired(row[DummyBizoSchema.Z0_VarCharMax.Name]));
		}

		public void TestLoadBlobField_ConcurrentlyNulled()
		{
			Dummy.FillWithValidTestData();
			byte[] buffer = new byte[1050];
			new Random().NextBytes(buffer);
			Dummy.Z0_VarBinaryMax = buffer;
			Dummy.Z0_VarCharMax = Encoding.ASCII.GetString(buffer);

			Factory.Save();

			var factory = new BusinessObjectFactory();
			DataRow row = factory.Load(typeof(DummyBusinessObject), Dummy.PK).Row;
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());

			Db.Connection.Command(String.Format("update dbo.dummybizo set z0_varbinarymax = null, z0_varcharmax = '' where z0_pk = '{0}'", Dummy.PK)).ExecuteNonQuery();

			Assert(LazyLoading.LoadRequired(row[DummyBizoSchema.Z0_VarBinaryMax.Name]));
			Assert(LazyLoading.LoadRequired(row[DummyBizoSchema.Z0_VarCharMax.Name]));
			loader.LoadBlobField(row, DummyBizoSchema.Z0_VarBinaryMax);
			loader.LoadBlobField(row, DummyBizoSchema.Z0_VarCharMax);
			Assert(!LazyLoading.LoadRequired(row[DummyBizoSchema.Z0_VarBinaryMax.Name]));
			Assert(!LazyLoading.LoadRequired(row[DummyBizoSchema.Z0_VarCharMax.Name]));
			AssertEquals(System.DBNull.Value, row[DummyBizoSchema.Z0_VarBinaryMax.Name]);
			AssertEquals(String.Empty, row[DummyBizoSchema.Z0_VarCharMax.Name]);
		}

		[ExpectNoExceptions]
		public void TestLoadBlobField_Exception()
		{
			Dummy.FillWithValidTestData();

			// Creating compressed data
			byte[] bytes = { 4, 7, 2, 54, 7, 8, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 6 };
			var compressedStream = new MemoryStream();
			using (var compressor = Compressor.Compress(compressedStream))
			{
				compressor.Write(bytes, 0, bytes.Length);
			}
			byte[] compressed = compressedStream.ToArray();
			Dummy.Z0_VarBinaryMax = new ZBlob(compressed);
			byte[] buffer = new byte[1050];
			new Random().NextBytes(buffer);
			Dummy.Z0_VarBinaryMax = buffer;

			Factory.Save();

			var factory = new BusinessObjectFactory();
			DataRow row = factory.Load(typeof(DummyBusinessObject), Dummy.PK).Row;

			var mockStream = new StreamForTest_TestLoadBlobField_Exception();

			var mockSqlLoader = new Mock<ZSqlLoader>(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), ObjectFactory.Get<IApplicationSchemaResolver>()) { CallBase = true };
			mockSqlLoader.Setup(x => x.GetStream(It.IsAny<DataRow>(), It.IsAny<string>(), It.IsAny<string>())).Returns(mockStream);

			AssertExceptionThrown("On Exception, original stream must be disposed to avoid error: there is already an open datareader", typeof(OutOfMemoryException), "Test out of memory exception", () =>
			{
				mockSqlLoader.Object.LoadBlobField(row, DummyBizoSchema.Z0_VarBinaryMax);
			});
			Assert("Stream must be disposed, even if there's an exception.", mockStream.DisposeCallCount > 0);
		}

		public void TestLoadingBlobsUsingFillDataTableFromDB_AcceptsChanges()
		{
			// Arrange
			var pk = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery($"INSERT INTO dbo.ProcessTasks(P9_PK, P9_Notes, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser) VALUES('{pk.ToString()}', 0x0123456789, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");

			var testConnectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			var testFilter = new ZQuery(ProcessTasksSchema.PK, pk);
			testFilter.LoadSmallBlobs = 1;

			var loader = new ZSqlLoader(new DataSet(), testConnectionInfo, GetNewSchemaResolver());
			var testQuery = new ZDataQuery(testConnectionInfo, ProcessTasksSchema.Constants.TableName, testFilter);
			var response = loader.LoadPersistentRowsIntoDataSet(testQuery);

			testFilter.IncludeBlob(ProcessTasksSchema.P9_Notes);
			testQuery = new ZDataQuery(testConnectionInfo, ProcessTasksSchema.Constants.TableName, testFilter);

			// Act
			response = loader.LoadPersistentRowsIntoDataSet(testQuery);

			// Assert
			AssertEquals("Should have 1 row", 1, response.DataRowLoadResponses[0].AllRows.Length);
			var row = response.DataRowLoadResponses[0].AllRows[0];
			AssertNotEquals("P9_Notes should be populated", LazyLoading.BinaryPlaceholder, row[ProcessTasksSchema.Constants.P9_Notes]);

			var rowOriginal = (byte[])row[ProcessTasksSchema.Constants.P9_Notes, DataRowVersion.Original];
			AssertNotEquals("Expected a populated DataRowVersion.Original version but got the binary placeholder", LazyLoading.BinaryPlaceholder, rowOriginal);
			AssertEquals("Expected DataRowVersion.Original version to be updated with the value retrieved from DB", row[ProcessTasksSchema.Constants.P9_Notes], rowOriginal);
		}

		#endregion

		public void TestLoadPersistentRowsIntoDataSetTimeout()
		{
			using (var connection = Db.Connection)
			{
				var testConnectionInfo = new ZSqlConnectionInfo(connection, "");
				var testFilter = new ZQueryForTest(DummyBizoSchema.Z0_Bool, true);
				var loader = new ZSqlLoader(new DataSet(), testConnectionInfo, GetNewSchemaResolver());
				var testQuery = new ZDataQuery(testConnectionInfo, DummyBizoSchema.Constants.TableName, testFilter);
				AssertNoExceptionThrown(() => loader.LoadPersistentRowsIntoDataSet(testQuery));

				testFilter.Timeout = 1;
				testQuery = new ZDataQuery(testConnectionInfo, DummyBizoSchema.Constants.TableName, testFilter);
				AssertExceptionThrown<SqlException>("Timeout Exception", "Execution Timeout Expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.", () => loader.LoadPersistentRowsIntoDataSet(testQuery));
			}
		}

		class ZQueryForTest : ZQuery
		{
			public ZQueryForTest(SchemaColumn schemaColumn, object value)
					: base(schemaColumn, value)
			{
			}

			public override void AddAsCompleteSQLStatement(SqlBuilder sqlBuilder, string tableName, bool combineFilterAndParams, SchemaColumn[] selectList = null)
			{
				base.AddAsCompleteSQLStatement(sqlBuilder, tableName, combineFilterAndParams, selectList);
				sqlBuilder.Append(" WAITFOR DELAY '00:00:02'");
			}
		}

		public void TestStringInterningOnSingleLoad()
		{
			Db.Connection.ExecuteNonQuery("insert into dbo.StmaLog (SL_PK, SL_Parent, SL_Table, SL_EventTime, SL_SE_NKEvent, SL_PostedTimeUtc) values (NEWID(), 'CE4034FA-8638-4B08-BA6C-E6071F7D3047', 'StmData', getdate(), 'XYZ', getutcdate())");
			Db.Connection.ExecuteNonQuery("insert into dbo.StmaLog (SL_PK, SL_Parent, SL_Table, SL_EventTime, SL_SE_NKEVENT, SL_PostedTimeUtc) values (NEWID(), 'CE4034FA-8638-4B08-BA6C-E6071F7D3047', 'StmData', getdate(), 'XYZ', getutcdate())");

			ZSqlConnectionInfo testConnectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			ZQuery testFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, "XYZ");

			ZSqlLoader loader = new ZSqlLoader(new DataSet(), testConnectionInfo, GetNewSchemaResolver());
			ZDataQuery testQuery = new ZDataQuery(testConnectionInfo, StmALogSchema.Constants.TableName, testFilter);
			LoaderResponse response = loader.LoadPersistentRowsIntoDataSet(testQuery);
			var row0 = response.DataRowLoadResponses[0].AllRows[0];
			var row1 = response.DataRowLoadResponses[0].AllRows[1];
			Assert("Objects must be identical", object.ReferenceEquals(row1["SL_SE_NKEvent"], row0["SL_SE_NKEvent"]));
		}

		public void TestDuplicateLogRecordsAreIgnored()
		{
			const string query = "insert into dbo.StmaLog (SL_PK, SL_Parent, SL_Table, SL_EventTime, SL_PostedTimeUtc) values ('7666920F-F2BD-4EB9-85BD-219F7F1995FE', 'CE4034FA-8638-4B08-BA6C-E6071F7D3047', 'StmData', getdate(), getutcdate())";
			Db.Connection.ExecuteNonQuery(query);
			Db.Connection.ExecuteNonQuery(query);

			ZSqlConnectionInfo testConnectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			ZQuery testFilter = new ZQuery(StmALogSchema.SL_Parent, new ZGuid("CE4034FA-8638-4B08-BA6C-E6071F7D3047"));

			ZSqlLoader loader = new ZSqlLoader(new DataSet(), testConnectionInfo, GetNewSchemaResolver());
			ZDataQuery testQuery = new ZDataQuery(testConnectionInfo, StmALogSchema.Constants.TableName, testFilter);
			LoaderResponse response = loader.LoadPersistentRowsIntoDataSet(testQuery);
			AssertEquals(1, response.DataRowLoadResponses[0].AllRows.Length);
			AssertEquals(1, response.DataRowLoadResponses[0].NewRows.Length);
		}

		public void TestGetTextFieldReaderOnDeletedRow()
		{
			Dummy.FillWithValidTestData();
			Dummy.Z0_VarCharMax = new string('x', 1050);
			byte[] buffer = new byte[1050];
			new Random().NextBytes(buffer);
			Dummy.Z0_VarBinaryMax = buffer;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DataRow row = factory2.Load(typeof(DummyBusinessObject), Dummy.PK).Row;
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());

			row.Delete();
			AssertNoExceptionThrown("Should not have thrown an exception with deleted row", () => loader.GetTextFieldReader(row, DummyBizoSchema.Constants.TableName, DummyBizoSchema.Z0_AnotherNumber.Name, false));
		}

		public void TestDoesTableExist()
		{
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());

			string sqlText = @"
Create Table ExistingTable
(
	ExistingTable_PK uniqueidentifier,
	ExistingTable_Data char(10)
)
";
			string createViewText = @"
Create View ExistingView as
select
	ExistingTable_PK as EV_PK,
	ExistingTable_Data as EV_Data
from ExistingTable
";

			Db.Connection.ExecuteNonQuery(sqlText);
			Db.Connection.ExecuteNonQuery(createViewText);
			Assert("ExistingTable should exist", loader.DoesTableExist("ExistingTable"));
			Assert("ExistingView should exist", loader.DoesTableExist("ExistingView"));

			AssertExceptionThrown<NotSupportedException>("2 part not suppoted", () => loader.DoesTableExist("dbo.spt_values"));
			AssertExceptionThrown<NotSupportedException>("3 part not suppoted", () => loader.DoesTableExist("master.dbo.spt_values"));
			AssertExceptionThrown<NotSupportedException>("4 part not suppoted", () => loader.DoesTableExist("blah.master.dbo.spt_values"));

			Assert("NonExistentTable should not exist", !loader.DoesTableExist("NonExistentTable"));
			Assert("NonExistentView should not exist", !loader.DoesTableExist("NonExistentView"));
		}

		public void TestDecompressionOnNonPersistentLoad()
		{
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());

			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());

			ZNonPersistentDataQuery query = new ZNonPersistentDataQuery("select * from " + DummyBizoSchema.Constants.TableName + " order by " + DummyBizoSchema.Z0_Description.Name, ZSqlParameter.EmptyArray);
			var response = loader.LoadNonPersistentRows(query);

			AssertEquals("Loaded and decompressed", DummyTableCreator.DummyByteArray1, (byte[])response.Rows[0][DummyBizoSchema.Z0_VarBinaryMax.Name]);
			AssertEquals("Loaded and decompressed", DummyTableCreator.DummyByteArray2, (byte[])response.Rows[1][DummyBizoSchema.Z0_VarBinaryMax.Name]);
		}

		public void TestSmallStatementsAreGroupedForSingleDbHit()
		{
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), connectionInfo, GetNewSchemaResolver());
			ZDataQuery querySmall = new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery());

			ZQuery largeQuery = new ZQuery();
			largeQuery.AddFilterAndZSQLParameterCollection("1 = 1" + new string(' ', ZSqlLoader.MaximumCommandTextLengthInCharacters), null);
			ZDataQuery queryLarge = new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, largeQuery);

			ZDataQuery[] queries = new ZDataQuery[] { querySmall, querySmall, querySmall };
			LoaderResponse response = loader.LoadPersistentRowsIntoDataSet(queries);
			AssertEquals(3, response.DatabaseRoundTrips);
			AssertEquals(3, response.DataRowLoadResponses.Length);

			queries = new ZDataQuery[] { querySmall, queryLarge };
			response = loader.LoadPersistentRowsIntoDataSet(queries);
			AssertEquals(2, response.DatabaseRoundTrips);
			AssertEquals(2, response.DataRowLoadResponses.Length);

			queries = new ZDataQuery[] { queryLarge, queryLarge };
			response = loader.LoadPersistentRowsIntoDataSet(queries);
			AssertEquals(2, response.DatabaseRoundTrips);
			AssertEquals(2, response.DataRowLoadResponses.Length);
		}

		public void TestDecompressionOnPersistentLoad()
		{
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());

			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());

			ZQuery filter = new ZQuery();
			filter.OrderBy = DummyBizoSchema.Z0_Description.Name;
			ZDataQuery query = new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, filter);
			loader.LoadPersistentRowsIntoDataSet(query);
			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);

			AssertEquals(false, LazyLoading.LoadRequired(table.Rows[0]));
			AssertEquals(false, LazyLoading.LoadRequired(table.Rows[1]));

			AssertEquals("Loaded and decompressed", DummyTableCreator.DummyByteArray1, (byte[])table.Rows[0][DummyBizoSchema.Z0_VarBinaryMax.Name]);
			AssertEquals("Loaded and decompressed", DummyTableCreator.DummyByteArray2, (byte[])table.Rows[1][DummyBizoSchema.Z0_VarBinaryMax.Name]);
		}

		public void TestEagleBooleanCharFieldFromKnownTable()
		{
			Db.Connection.ExecuteNonQuery("insert into dbo.dummyBizo (z0_pk, z0_bool) values (newid(), 'Y')");
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), connectionInfo, GetNewSchemaResolver());
			ZNonPersistentDataQuery query = new ZNonPersistentDataQuery("select * from dbo.dummyBizO");
			var response = loader.LoadNonPersistentRows(query);
			AssertEquals(typeof(string), response.Rows[0]["z0_bool"].GetType());
		}

		[ExpectExceptionMessage(typeof(ZTypeValueException), "Cannot initialise a CargoWise.Types.ZBool with <z> (System.String).")]
		public void TestLoadingInvalidBoolValue()
		{
			Guid guid1 = Guid.NewGuid();
			Guid guid2 = Guid.NewGuid();
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());
			DummyTableCreator.AddDummyBusinessObjectsToDB(guid1, guid2);
			Db.Connection.ExecuteNonQuery(String.Format("update dbo.DummyBizo set z0_bool = 'z' where z0_pk = '{0}'", guid1.ToString()));

			ZQuery query = new ZQuery(DummyBizoSchema.PK, guid1);
			ZDataQuery dataQuery = new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, query);

			DataRowLoadResponse[] trueResponse = loader.LoadPersistentRowsIntoDataSet(dataQuery).DataRowLoadResponses;
		}

		public void TestBitFieldFromUnknownTable()
		{
			Db.Connection.ExecuteNonQuery("create table #bb (myBit bit); insert into #bb values (1)");
			try
			{
				ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
				ZSqlLoader loader = new ZSqlLoader(new DataSet(), connectionInfo, GetNewSchemaResolver());
				ZNonPersistentDataQuery query = new ZNonPersistentDataQuery("select * from #bb");
				var response = loader.LoadNonPersistentRows(query);
				AssertEquals(typeof(bool), response.Rows[0]["myBit"].GetType());
			}
			finally
			{
				Db.Connection.ExecuteNonQuery("drop table #bb");
			}
		}

		public void TestCharOneFieldFromUnknownOrigin()
		{
			Db.Connection.ExecuteNonQuery("create table #bb (myChar char(1)); insert into #bb values ('x')");
			try
			{
				ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
				ZSqlLoader loader = new ZSqlLoader(new DataSet(), connectionInfo, GetNewSchemaResolver());
				ZNonPersistentDataQuery query = new ZNonPersistentDataQuery("select * from #bb");
				var response = loader.LoadNonPersistentRows(query);
				AssertEquals(typeof(string), response.Rows[0]["myChar"].GetType());
			}
			finally
			{
				Db.Connection.ExecuteNonQuery("drop table #bb");
			}
		}

		/// <summary>
		/// As this test measures DB hits, the test needs to run twice (the first to prime registry hits etc)
		/// </summary>
		public void TestPersistentLoadLoadsTwoQueriesInOneDbHit()
		{
			GetPersistentLoadLoadsTwoQueriesInOneDbHitResult(); //init
			AssertEquals(4, GetPersistentLoadLoadsTwoQueriesInOneDbHitResult());
		}

		int GetPersistentLoadLoadsTwoQueriesInOneDbHitResult()
		{
			Guid guid1 = Guid.NewGuid();
			Guid guid2 = Guid.NewGuid();
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());
			DummyTableCreator.AddDummyBusinessObjectsToDB(guid1, guid2);

			ZDataQuery query1 = new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.PK, guid1));
			ZDataQuery query2 = new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.PK, guid2));
			ZDataQuery query3 = new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Guid, guid1));
			ZDataQuery query4 = new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Guid, guid2));

			int orginalCount = Db.Connection.ExecutedCommandCount;
			DataRowLoadResponse[] responses = loader.LoadPersistentRowsIntoDataSet(new[] { query1, query2, query3, query4 }).DataRowLoadResponses;

			AssertEquals(4, responses.Length);

			AssertEquals(1, responses[0].NewRows.Length);
			AssertEquals(guid1, responses[0].NewRows[0]["Z0_PK"]);

			AssertEquals(1, responses[1].NewRows.Length);
			AssertEquals(guid2, responses[1].NewRows[0]["Z0_PK"]);

			AssertEquals(0, responses[2].NewRows.Length);
			AssertEquals(0, responses[3].NewRows.Length);

			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);
			AssertEquals(2, table.Rows.Count);

			return Db.Connection.ExecutedCommandCount - orginalCount;
		}

		public void TestNoExceptionWasThrownIfZDataTableHasDetachedRowsWhenLoadingData_ReLoadExistingRowsIsTrue()
		{
			var table = new ZDataTable(RefCountrySchema.Constants.TableName);

			using (var cmd = Db.Connection.Command("SELECT TOP(0) * FROM dbo.RefCountry"))
			using (var reader = cmd.ExecuteReader())
			{
				table.Load(reader);
			}

			table.PrimaryKey = new DataColumn[] { table.Columns[0] };
			table.RowChanged += new DataRowChangeEventHandler((o, a) => { table.Rows.Remove(a.Row); });
			table.Columns.Remove("RN_AutoVersion");

			var dataSet = new DataSet();
			dataSet.Tables.Add(table);
			var connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			var query = new ZQuery().GetDataQuery(connectionInfo, RefCountrySchema.Constants.TableName);
			var loader = new ZSqlLoader(dataSet, connectionInfo, GetNewSchemaResolver());

			AssertNoExceptionThrown(() => loader.LoadPersistentRowsIntoDataSet(query));
		}

		public void TestGetTable()
		{
			DataSet data = new DataSet();
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			ZLoader loader = new ZSqlLoader(data, connectionInfo, GetNewSchemaResolver());

			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);
			AssertNotNull("returned table", table);
			AssertNotNull("in DS", data.Tables[DummyBizoSchema.Constants.TableName]);
			AssertEquals("Loaded right table", table.TableName, DummyBizoSchema.Constants.TableName);
			AssertEquals("Table in DS", table, data.Tables[DummyBizoSchema.Constants.TableName]);
			AssertEquals("One PK", 1, table.PrimaryKey.Length);
			AssertEquals("Correct PK", table.PrimaryKey[0], table.Columns[0]);

			DataTable table2 = loader.GetTable(DummyBizoSchema.Constants.TableName);
			AssertEquals("Cached table load", table, table2);
		}

		public void TestGetCount()
		{
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());
			ZCountDataQuery query = new ZCountDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, new ZQuery());

			AssertEquals("Number of dummies", 0, loader.GetCount(query));

			Guid pK1 = Guid.NewGuid();
			Guid pK2 = Guid.NewGuid();
			DummyTableCreator.AddDummyBusinessObjectsToDB(pK1, pK2);
			AssertEquals("Number of dummies", 2, loader.GetCount(query));

			ZConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");

			query = new ZCountDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.PK, pK1));
			AssertEquals("Number of dummies", 1, loader.GetCount(query));

			query = new ZCountDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.PK, pK2));
			AssertEquals("Number of dummies", 1, loader.GetCount(query));

			query = new ZCountDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Description, "Something that won't be in the database. OK!@!!???"));
			AssertEquals("Number of dummies", 0, loader.GetCount(query));
		}

		public void TestLoadNonPersistentRows()
		{
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());

			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@Num", 1, DummyBizoSchema.Z0_Number);

			var response = loader.LoadNonPersistentRows(new ZNonPersistentDataQuery(
				"select " + DummyBizoSchema.Z0_Description.Name + ", sum(" + DummyBizoSchema.Z0_Number.Name + ") as sum " +
				"from " + DummyBizoSchema.Constants.TableName + " " +
				"where " + DummyBizoSchema.Z0_Number.Name + " > @Num " +
				" group by " + DummyBizoSchema.Z0_Description.Name +
				" order by " + DummyBizoSchema.Z0_Description.Name,
				@params));

			AssertEquals("Should be two rows, thanks to group by", 2, response.Rows.Length);
			AssertEquals(DummyTableCreator.DummyDescription1, response.Rows[0][DummyBizoSchema.Z0_Description.Name].ToString());
			AssertEquals("Sum working and where clause OK", DummyTableCreator.DummyNumber1, response.Rows[0]["sum"]);
			AssertEquals(DummyTableCreator.DummyDescription2, response.Rows[1][DummyBizoSchema.Z0_Description.Name].ToString());
			AssertEquals("Sum working and where clause OK", DummyTableCreator.DummyNumber2, response.Rows[1]["sum"]);
		}

		public void TestLoadNonPersistentRowsWithSelectStarGetsCorrectColumns()
		{
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());

			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());

			ZNonPersistentDataQuery query = new ZNonPersistentDataQuery("select * from " + DummyBizoSchema.Constants.TableName, new ZSqlParameterCollection());
			var response = loader.LoadNonPersistentRows(query);

			AssertEquals(typeof(Microsoft.SqlServer.Types.SqlGeography), response.Rows[0].Table.Columns[DummyBizoSchema.Z0_Geography.Name].DataType); //it should stay SqlGeography in ZSqlLoader.SetRowFields
			AssertEquals(typeof(DateTimeOffset), response.Rows[0].Table.Columns[DummyBizoSchema.Z0_DateTimeOffset.Name].DataType);
			AssertEquals(typeof(DateTime), response.Rows[0].Table.Columns[DummyBizoSchema.Z0_AnotherDate.Name].DataType);
			AssertEquals(typeof(TimeSpan), response.Rows[0].Table.Columns[DummyBizoSchema.Z0_Time.Name].DataType);
			AssertEquals(typeof(Decimal), response.Rows[0].Table.Columns[DummyBizoSchema.Z0_AnotherDecimal.Name].DataType);
			AssertEquals(typeof(int), response.Rows[0].Table.Columns[DummyBizoSchema.Z0_AnotherNumber.Name].DataType);
			AssertEquals(typeof(string), response.Rows[0].Table.Columns[DummyBizoSchema.Z0_Bool.Name].DataType);
			AssertEquals(typeof(byte), response.Rows[0].Table.Columns[DummyBizoSchema.Z0_Byte.Name].DataType);
			AssertEquals(typeof(byte[]), response.Rows[0].Table.Columns[DummyBizoSchema.Z0_VarBinaryMax.Name].DataType);
			AssertEquals(typeof(string), response.Rows[0].Table.Columns[DummyBizoSchema.Z0_Code.Name].DataType);
			AssertEquals(typeof(string), response.Rows[0].Table.Columns[DummyBizoSchema.Z0_Description.Name].DataType);
			AssertEquals(typeof(Guid), response.Rows[0].Table.Columns[DummyBizoSchema.Z0_Guid.Name].DataType);
			AssertEquals(typeof(short), response.Rows[0].Table.Columns[DummyBizoSchema.Z0_Short.Name].DataType);
		}

		public void TestLoadPersistentRowsIntoDataSetHasCorrectReloadBehaviour()
		{
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());

			Guid pK1 = Guid.NewGuid();
			DummyTableCreator.AddDummyBusinessObjectsToDB(pK1, Guid.NewGuid());

			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Description, DummyTableCreator.DummyDescription1);
			ZDataQuery query = new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, filter);
			AssertEquals("ReloadExistingRows", false, query.ReloadExisitingRows);
			loader.LoadPersistentRowsIntoDataSet(query);
			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);
			AssertEquals("Found one row", 1, table.Rows.Count);
			AssertEquals("AcceptChanges() called", false, table.DataSet.HasChanges());

			string changedDescription = "xx";
			Db.Connection.ExecuteNonQuery("UPDATE " + DummyBizoSchema.Constants.TableName +
				" SET " + DummyBizoSchema.Z0_Description.Name + " ='" + changedDescription +
					"' WHERE " + DummyBizoSchema.Z0_Description.Name + " like @desc",
					cmd => cmd.AddParameterBasedOnDbColumn("@desc", $"%{DummyTableCreator.DummyDescription1}%", DummyBizoSchema.Z0_Description));
			loader.LoadPersistentRowsIntoDataSet(query);
			AssertEquals("No reload, no change", DummyTableCreator.DummyDescription1, table.Rows[0][DummyBizoSchema.Z0_Description.Name].ToString());

			table.Rows[0][DummyBizoSchema.Z0_Description.Name] = "blah";
			AssertEquals("Has changes", true, table.DataSet.HasChanges());

			filter = new ZQuery(DummyBizoSchema.Z0_Description, changedDescription);
			filter.ReLoadExistingRows = true;
			loader.LoadPersistentRowsIntoDataSet(new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, filter));
			AssertEquals("FillDataTableFromDB with reload", changedDescription, table.Rows[0][DummyBizoSchema.Z0_Description.Name].ToString());
			AssertEquals("AcceptChanges() called", false, table.DataSet.HasChanges());
		}

		public void TestLoadPersistentRowsWithStoredProcedure()
		{
			ZSqlLoader loader = SetupStoredProcedureTests();
			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);
			AssertEquals(0, table.Rows.Count);

			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZDataQuery("TestStoredProcedure", parameter, DummyBizoSchema.Constants.TableName);
			loader.LoadPersistentRowsIntoDataSet(query);

			AssertEquals("Stored procedure returns 1 row", 1, table.Rows.Count);
			AssertEquals(DummyTableCreator.DummyDescription1, table.Rows[0][DummyBizoSchema.Z0_Description.Name]);
		}

		public void TestCallStoredProcedureTwice()
		{
			ZSqlLoader loader = SetupStoredProcedureTests();
			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);
			AssertEquals(0, table.Rows.Count);

			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription2, DummyBizoSchema.Z0_Description) };
			var query = new ZDataQuery("TestStoredProcedure", parameter, DummyBizoSchema.Constants.TableName);

			loader.LoadPersistentRowsIntoDataSet(query);
			AssertEquals("Stored procedure returns 1 row", 1, table.Rows.Count);
			AssertEquals(DummyTableCreator.DummyDescription2, table.Rows[0][DummyBizoSchema.Z0_Description.Name]);

			loader.LoadPersistentRowsIntoDataSet(query);
			AssertEquals("Stored procedure returns 1 row", 1, table.Rows.Count);
			AssertEquals(DummyTableCreator.DummyDescription2, table.Rows[0][DummyBizoSchema.Z0_Description.Name]);
		}

		public void TestStoredProcedureDataLoadWhenDataChanged()
		{
			ZSqlLoader loader = SetupStoredProcedureTests();
			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);
			AssertEquals(0, table.Rows.Count);

			var parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description) };
			var query = new ZDataQuery("TestStoredProcedure", parameter, DummyBizoSchema.Constants.TableName);

			loader.LoadPersistentRowsIntoDataSet(query);
			AssertEquals("Stored procedure returns 1 row", 1, table.Rows.Count);
			AssertEquals(DummyTableCreator.DummyDescription1, table.Rows[0][DummyBizoSchema.Z0_Description.Name]);

			var newDescription = "ENEMY";
			Db.Connection.ExecuteNonQuery("UPDATE " + DummyBizoSchema.Constants.TableName +
				" SET " + DummyBizoSchema.Z0_Description.Name + " ='" + newDescription +
				"' WHERE " + DummyBizoSchema.Z0_Description.Name + "=@desc",
				cmd => cmd.AddParameterBasedOnDbColumn("@desc", DummyTableCreator.DummyDescription1, DummyBizoSchema.Z0_Description));

			var loader2 = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());
			var table2 = loader2.GetTable(DummyBizoSchema.Constants.TableName);
			loader2.LoadPersistentRowsIntoDataSet(query);
			AssertEquals("Stored procedure returns no rows", 0, table2.Rows.Count);

			parameter = new ZSqlParameter[1] { ZSqlParameter.New("@Desc", newDescription, DummyBizoSchema.Z0_Description) };
			query = new ZDataQuery("TestStoredProcedure", parameter, DummyBizoSchema.Constants.TableName);

			loader2.LoadPersistentRowsIntoDataSet(query);
			AssertEquals("Stored procedure returns 1 row", 1, table2.Rows.Count);
			AssertEquals(newDescription, table2.Rows[0][DummyBizoSchema.Z0_Description.Name]);
		}

		public void TestLoadPersistentRows_WhenFilterContainsKeywordsWithInclusionRelationships()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());

			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());

			var filter = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			filter.AddToFilter_PossiblyCommaSeparated(DummyBizoSchema.Z0_Description, SQLComparisonOperator.StartsWith, new ZString("COMRAD,COMRADE,COMRADE,COMRADE,COMRADE,COMRADE,COMRADE,COMRADE,COMRADE,COMRADE"));
			ZDataQuery query = new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, filter);

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => loader.LoadPersistentRowsIntoDataSet(query));
				AssertEquals("Filter contains keywords with inclusion relationships must not cause error in ErrorReporter", 0, ErrorReporter.TotalErrorCount);
				AssertEquals("Filter contains keywords with inclusion relationships must not cause error in ErrorReporter", "", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();

				DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);
				AssertEquals("Found one row", 1, table.Rows.Count);
			});
		}

		public void TestLoad_SkipsTheComputedColumns()
		{
			var dataSet = new DataSet();
			var schemaResolver = GetNewSchemaResolver();

			var loader = new ZSqlLoader(dataSet, new ZSqlConnectionInfo(Db.Connection, ""), schemaResolver);

			var filter = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			var query = new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, filter);

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => schemaResolver.GetSchemaColumn(DummyBizoSchema.Constants.Z0_ComputedChar, DummyBizoSchema.Constants.TableName));
				AssertNotNull(schemaResolver.GetSchemaColumn(DummyBizoSchema.Constants.Z0_ComputedChar, DummyBizoSchema.Constants.TableName));
				AssertNoExceptionThrown(() => loader.LoadPersistentRowsIntoDataSet(query));
				var table = loader.GetTable(DummyBizoSchema.Constants.TableName);
				Assert(!table.Columns.Contains(DummyBizoSchema.Constants.Z0_ComputedChar));
			});
		}

		public void TestGetUncompressedSanitisedRowValue_InternShortStrings()
		{
			const string string1 = "A1";
			var string2 = "A" + (DateTime.UtcNow.Year - DateTime.UtcNow.AddYears(-1).Year).ToString();
			var string3 = "A" + (string2.Length - 1).ToString();

			var schemaColumn = new SchemaStringColumn(DummyBizoSchema.Instance, "test", 1, SqlDbType.VarChar, "", false, 3, tvpName: "dbo.TVP_varchar_250");

			var o1 = ZSqlLoader.GetUncompressedSanitisedRowValue("test", string1, schemaColumn);
			var o2 = ZSqlLoader.GetUncompressedSanitisedRowValue("test", string2, schemaColumn);
			var o3 = ZSqlLoader.GetUncompressedSanitisedRowValue("test", string3, schemaColumn);

			AssertSame("Should be interned and return same string instance", o1, o2);
			AssertSame("Should be interned and return same string instance", o2, o3);

			var schemaColumnLong = new SchemaStringColumn(DummyBizoSchema.Instance, "test", 1, SqlDbType.VarChar, "", false, 4, tvpName: "dbo.TVP_varchar_250");

			o1 = ZSqlLoader.GetUncompressedSanitisedRowValue("test", string1, schemaColumnLong);
			o2 = ZSqlLoader.GetUncompressedSanitisedRowValue("test", string2, schemaColumnLong);
			o3 = ZSqlLoader.GetUncompressedSanitisedRowValue("test", string3, schemaColumnLong);

			AssertNotSame("Should not intern strings for columns with MaxLength > 3", o1, o2);
			AssertNotSame("Should not intern strings for columns with MaxLength > 3", o2, o3);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(DummyBizoSchema.Constants.TableName);
		}
		IApplicationSchemaResolver GetNewSchemaResolver() => ObjectFactory.Get<IApplicationSchemaResolver>();

		ZSqlLoader SetupStoredProcedureTests()
		{
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());

			ZNonPersistentDataQuery query1 = new ZNonPersistentDataQuery("select * from " + DummyBizoSchema.Constants.TableName, new ZSqlParameterCollection());
			var response1 = loader.LoadNonPersistentRows(query1);
			AssertEquals("Two Records in Table", 2, response1.Rows.Length);

			Db.Connection.ExecuteNonQuery("CREATE PROCEDURE TestStoredProcedure @Desc VARCHAR(" + DummyBizoSchema.Z0_Description.MaxLength + ") AS BEGIN SELECT " + GetTableColumsForStoredProcedure +
				" FROM " + DummyBizoSchema.Constants.TableName + " WHERE " + DummyBizoSchema.Z0_Description.Name + "=@Desc END");

			return loader;
		}

		ZString GetTableColumsForStoredProcedure => String.Join(",", ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumns(DummyBizoSchema.Constants.TableName).Select(t => t.Name));

		#endregion
	}

	sealed class StreamForTest_TestLoadBlobField_Exception : Stream
	{
		#region Unused overidden Fields, Props, & Methods
		bool _canRead;
		public override bool CanRead => _canRead;

		bool _canSeek;
		public override bool CanSeek => _canSeek;

		bool _canTimeout;
		public override bool CanTimeout => _canTimeout;

		bool _canWrite;
		public override bool CanWrite => _canWrite;

		long _length;
		public override long Length => _length;
		long _position;
		public override long Position { get => _position; set => _position = value; }
		public override void Flush()
		{
			throw new NotImplementedException();
		}
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		internal void DoCode()
		{
			_canRead = true;
			_canSeek = true;
			_canWrite = true;
			_canTimeout = true;
			_length = 0;
		}
		#endregion

		public override int Read(byte[] buffer, int offset, int count)
		{
			// Simulating out of memory during stream read; especially in Compressor.IsCompressed
			throw new OutOfMemoryException("Test out of memory exception");
		}

		public int DisposeCallCount { get; set; }

		protected override void Dispose(bool disposing)
		{
			DisposeCallCount++;
			base.Dispose(disposing);
		}
	}
	#endregion

	#region ZSqlLoaderNonTransactionalTest

	sealed class ZSqlLoaderNonTransactionalTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestMaxLengthSetCorrectly()
		{
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), new ZSqlConnectionInfo(Db.Connection, ""), ObjectFactory.Get<IApplicationSchemaResolver>());
			DataTable dataTable = new DataTable();
			loader.AddDataColumnToTable(dataTable, DummyBizoSchema.Z0_Code);
			AssertEquals(DummyBizoSchema.Z0_Code.MaxLength, dataTable.Columns[DummyBizoSchema.Constants.Z0_Code].MaxLength);
		}
	}

	#endregion
}
