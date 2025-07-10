using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.PopulateAuditColumns;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.OnlineDataTransformation.Testing
{
	class BasePostOnPopulateAuditColumnsBaseOnlyTest : TransactionedTestCase
	{
		public void TestLargeNumber()
		{
			Db.Connection.Command(@"
INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES ('CF4F160C-20B1-4123-8DFF-3BE57C86BC6E', 'US', 'USD', 'US1', 'US company')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES ('BD4B8C70-181D-46EC-A1D0-D659A24960BB', 'CF4F160C-20B1-4123-8DFF-3BE57C86BC6E', 'US1', 'US')
INSERT INTO dbo.JobDeclaration (JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel) VALUES ('0BC81905-E4D5-41B4-A8F8-C0692097EE44', 'BD4B8C70-181D-46EC-A1D0-D659A24960BB', 'CF4F160C-20B1-4123-8DFF-3BE57C86BC6E', 99999, '~!')
INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_GroupInvoice, JZ_ClusterKey, JZ_GB, JZ_DataModel)
	SELECT '06C14D04-EB33-44E3-AC44-1ED28DCE7D7F', 0, 1, GB_PK, '~!' FROM dbo.GlbBranch WHERE GB_Code = 'DEM'
").ExecuteNonQuery();

			for (var idx = 1; idx < 102; idx++)
			{
				Db.Connection.Command($"INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_DataModel) VALUES ('75105320-82D4-4462-843E-000000000{idx:000}', '06C14D04-EB33-44E3-AC44-1ED28DCE7D7F', {idx}, '~!')").ExecuteNonQuery();
				Db.Connection.Command($@"
INSERT INTO dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser)
VALUES
	(NEWID(), 'JobComInvoiceLine', '75105320-82D4-4462-843E-000000000{idx:000}', '2016-07-21 04:07:55.490', '2016-07-21 14:07:54.790', 'ADD', 'BOB'),
	(NEWID(), 'JobComInvoiceLine', '75105320-82D4-4462-843E-000000000{idx:000}', '2016-07-21 04:08:50.103', '2016-07-21 14:07:44.790', 'EDT', 'W.J')").ExecuteNonQuery();
			}

			CreateNewTransform().Run();

			var selectSql = @"SELECT JI_PK, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser FROM dbo.JobComInvoiceLine WHERE JI_JZ = '06C14D04-EB33-44E3-AC44-1ED28DCE7D7F' ORDER BY JI_PK";

			var resultList = new List<string>();
			Db.Connection.ExecuteReader(selectSql, reader => resultList.Add($"{reader["JI_PK"]}, {ConvertDateToTest(reader["JI_SystemCreateTimeUtc"])}, {reader["JI_SystemCreateUser"]}, {ConvertDateToTest(reader["JI_SystemLastEditTimeUtc"])}, {reader["JI_SystemLastEditUser"]}".ToUpper()));

			var expectedStringBuilder = new List<string>();
			for (var idx = 1; idx < 102; idx++)
			{
				expectedStringBuilder.Add($"75105320-82D4-4462-843E-000000000{idx:000}, 2016-07-21T04:08:00, BOB, 2016-07-21T04:09:00, W.J");
			}

			AssertContainsExactElementsInExactOrder("Result", expectedStringBuilder, resultList);
		}

		public void TestCancellation()
		{
			PrepareTestData();
			var logger = new List<string>();
			var cancellationToken = new CancellationToken(true);
			var transformation = CreateNewTransform();
			var tableSchema = GetTableSchema(transformation);
			var totalCount = Db.Connection.ExecuteScalar<long>($"SELECT SUM(rows) FROM sys.partitions WHERE object_id = OBJECT_ID('{tableSchema.SqlSchemaName}.{tableSchema.TableName}') AND index_id in (0, 1)");

			AssertNull("ExtProperty.Table should be empty.", GetTableExtProperty(tableSchema));
			AssertExceptionThrown<OperationCanceledException>(() => ((IOnlineTransformation)transformation).Run(s => logger.Add(s), cancellationToken));
			AssertContainsExactElementsInExactOrder(new[] { $"Finished processing 1 item(s) of {totalCount:N0} in {tableSchema.TableName}." }, logger);
			AssertNotNull("ExtProperty.Table should not be empty.", GetTableExtProperty(tableSchema));

			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			AssertNull("ExtProperty.Table should have been cleared.", GetTableExtProperty(tableSchema));
		}

		public void TestPopulateAuditColumns()
		{
			PrepareTestData();
			CreateNewTransform().Run();

			var selectSql = @"SELECT JI_PK, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser FROM dbo.JobComInvoiceLine WHERE JI_JZ = '2AF736B9-3BF9-43AF-AD7A-8D3586DC325C' ORDER BY JI_PK";

			var resultList = new List<string>();
			Db.Connection.ExecuteReader(selectSql, reader => resultList.Add($"{reader["JI_PK"]}, {ConvertDateToTest(reader["JI_SystemCreateTimeUtc"])}, '{reader["JI_SystemCreateUser"]}', {ConvertDateToTest(reader["JI_SystemLastEditTimeUtc"])}, '{reader["JI_SystemLastEditUser"]}'".ToUpper()));

			CombineAssertions(() =>
			{
				AssertEquals("CreateTime and User entered and EditTime and User empty with just an ADD record.", "AC01122A-A197-4489-9D01-000000000001, 2020-02-01T00:00:00, 'SVJ', 2020-01-01T00:00:00, 'BOB'", resultList[0]);
				AssertEquals("CreateTime and User entered and EditTime and User empty with just an ADD and an EDT record.", "AC01122A-A197-4489-9D01-000000000002, 2020-02-02T00:00:00, 'SVJ', 2020-03-02T00:00:00, 'WJ2'", resultList[1]);
				AssertEquals("CreateTime and User entered and EditTime and User empty with multiple EDT records.", "AC01122A-A197-4489-9D01-000000000003, 2020-02-03T00:00:00, 'SVJ', 2020-03-06T00:00:00, 'XRO'", resultList[2]);
				AssertEquals("CreateTime and User empty and EditTime and User created with an ADD record.", "AC01122A-A197-4489-9D01-000000000004, 2020-01-01T00:00:00, 'MHY', 2020-01-08T00:00:00, 'SRW'", resultList[3]);
				AssertEquals("CreateTime and User empty and EditTime and User created with an ADD and and EDT record.", "AC01122A-A197-4489-9D01-000000000005, 2020-01-01T00:00:00, 'ISP', 2021-02-07T00:00:00, 'CKO'", resultList[4]);
				AssertEquals("CreateTime and User entered and EditTime and User entered with an ADD and EDT record.", "AC01122A-A197-4489-9D01-000000000006, 2022-01-04T00:00:00, 'JDE', 2022-01-12T00:00:00, 'EDU'", resultList[5]);
				AssertEquals("CreateTime and User empty and EditTime and User empty with an ADD and EDT record.", "AC01122A-A197-4489-9D01-000000000007, 2020-01-01T00:00:00, 'MNY', 2020-03-06T00:00:00, 'SRG'", resultList[6]);
				AssertEquals("CreateTime and User empty and EditTime and User empty with invalid stmalog records.", "AC01122A-A197-4489-9D01-000000000008, NDT, '', NDT, ''", resultList[7]);
			});
		}

		public static string GetTableExtProperty(ITableSchema tableSchema) => ExtProperty.Table.Select(Db.Connection, tableSchema.SqlSchemaName, tableSchema.TableName, "PostOnPopulateAuditColumnStartingPoint");

		public static string ConvertDateToTest(object readerField) => (readerField is DateTime dateTime) ? dateTime.ToString("s") : "NDT";

		static ITableSchema GetTableSchema(IOnlineTransformation trans) => (ITableSchema)trans.GetType().GetProperty("TableSchema").GetValue(trans);

		void PrepareTestData()
		{
			var sqlText = @"
INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES ('CF4F160C-20B1-4123-8DFF-3BE57C86BC6E', 'US', 'USD', 'US1', 'US company')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES ('BD4B8C70-181D-46EC-A1D0-D659A24960BB', 'CF4F160C-20B1-4123-8DFF-3BE57C86BC6E', 'US1', 'US')
INSERT INTO dbo.JobDeclaration (JE_PK, JE_GB, JE_GC, JE_ClusterKey, JE_DataModel) VALUES ('0BC81905-E4D5-41B4-A8F8-C0692097EE44', 'BD4B8C70-181D-46EC-A1D0-D659A24960BB', 'CF4F160C-20B1-4123-8DFF-3BE57C86BC6E', 99999, '~!')
INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_GroupInvoice, JZ_ClusterKey, JZ_GB, JZ_DataModel)
	SELECT '2AF736B9-3BF9-43AF-AD7A-8D3586DC325C', 0, 1, GB_PK, '~!' FROM dbo.GlbBranch WHERE GB_Code = 'DEM'

INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_DataModel, JI_SystemCreateTimeUtc, JI_SystemCreateUser)
VALUES
	('AC01122A-A197-4489-9D01-000000000001', '2AF736B9-3BF9-43AF-AD7A-8D3586DC325C', 1, '~!', '2020-2-1', 'SVJ')
INSERT INTO dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser)
VALUES
	('F8E49CBB-FB36-4A12-8D78-000000000010', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000001', '2020-1-1', '2020-1-2', 'ADD', 'BOB')

INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_DataModel, JI_SystemCreateTimeUtc, JI_SystemCreateUser)
VALUES
	('AC01122A-A197-4489-9D01-000000000002', '2AF736B9-3BF9-43AF-AD7A-8D3586DC325C', 2, '~!', '2020-2-2', 'SVJ')
INSERT INTO dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser)
VALUES
	('F8E49CBB-FB36-4A12-8D78-000000000021', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000002', '2020-1-1', '2020-3-2', 'ADD', 'F.L'),
	('F8E49CBB-FB36-4A12-8D78-000000000022', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000002', '2020-3-2', '2020-3-3', 'EDT', 'WJ2')

INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_DataModel, JI_SystemCreateTimeUtc, JI_SystemCreateUser)
VALUES
	('AC01122A-A197-4489-9D01-000000000003', '2AF736B9-3BF9-43AF-AD7A-8D3586DC325C', 3, '~!', '2020-2-3', 'SVJ')
INSERT INTO dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser)
VALUES
	('F8E49CBB-FB36-4A12-8D78-000000000023', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000003', '2020-1-1', '2020-3-2', 'ADD', 'G.M'),
	('F8E49CBB-FB36-4A12-8D78-000000000024', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000003', '2020-3-2', '2020-3-3', 'EDT', 'DC'),
	('F8E49CBB-FB36-4A12-8D78-000000000025', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000003', '2020-3-4', '2020-3-5', 'EDT', 'E'),
	('F8E49CBB-FB36-4A12-8D78-000000000026', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000003', '2020-3-6', '2020-3-7', 'EDT', 'XRO')

INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_DataModel, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser)
VALUES
	('AC01122A-A197-4489-9D01-000000000004', '2AF736B9-3BF9-43AF-AD7A-8D3586DC325C', 4, '~!', '2020-1-8', 'SRW')
INSERT INTO dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser)
VALUES
	('F8E49CBB-FB36-4A12-8D78-000000000027', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000004', '2020-1-1', '2020-3-2', 'ADD', 'MHY')

INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_DataModel, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser)
VALUES
	('AC01122A-A197-4489-9D01-000000000005', '2AF736B9-3BF9-43AF-AD7A-8D3586DC325C', 5, '~!', '2021-2-7', 'CKO')
INSERT INTO dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser)
VALUES
	('F8E49CBB-FB36-4A12-8D78-000000000028', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000005', '2020-1-1', '2020-3-2', 'ADD', 'ISP'),
	('F8E49CBB-FB36-4A12-8D78-000000000029', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000005', '2020-3-6', '2020-3-7', 'EDT', 'BKG')

INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_DataModel, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser)
VALUES
	('AC01122A-A197-4489-9D01-000000000006', '2AF736B9-3BF9-43AF-AD7A-8D3586DC325C', 6, '~!', '2022-1-4', 'JDE', '2022-1-12', 'EDU')
INSERT INTO dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser)
VALUES
	('F8E49CBB-FB36-4A12-8D78-000000000030', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000006', '2020-1-1', '2020-3-2', 'ADD', 'FBR'),
	('F8E49CBB-FB36-4A12-8D78-000000000031', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000006', '2020-3-6', '2020-3-7', 'EDT', 'WTC')

INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_DataModel)
VALUES
	('AC01122A-A197-4489-9D01-000000000007', '2AF736B9-3BF9-43AF-AD7A-8D3586DC325C', 7, '~!')
INSERT INTO dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser)
VALUES
	('F8E49CBB-FB36-4A12-8D78-000000000030', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000007', '2020-1-1', '2020-3-2', 'ADD', 'MNY'),
	('F8E49CBB-FB36-4A12-8D78-000000000031', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000007', '2020-3-6', '2020-3-7', 'EDT', 'SRG')

INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_ClusterKey, JI_DataModel)
VALUES
	('AC01122A-A197-4489-9D01-000000000008', '2AF736B9-3BF9-43AF-AD7A-8D3586DC325C', 8, '~!')
INSERT INTO dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_EventTime, SL_SE_NKEvent, SL_GS_NKUser)
VALUES
	('F8E49CBB-FB36-4A12-8D78-000000000032', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000008', '2020-1-1', '2020-3-2', 'REJ', 'LCT'),
	('F8E49CBB-FB36-4A12-8D78-000000000033', 'JobComInvoiceLine', 'AC01122A-A197-4489-9D01-000000000008', '2020-3-6', '2020-3-7', 'DEL', 'ISX')
";
			Db.Connection.Command(sqlText).ExecuteNonQuery();
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.StmData WHERE SD_Name='JobComInvoiceLineAuditPoped'");
		}

		BasePostOnPopulateAuditColumnsForTest CreateNewTransform() => new BasePostOnPopulateAuditColumnsForTest();

		class BasePostOnPopulateAuditColumnsForTest : BasePostOnPopulateAuditColumns<JobComInvoiceLineSchema>
		{
			public override int BatchSize => 1;
		}
	}
}
