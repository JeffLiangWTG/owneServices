using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Export;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Export.Testing
{
	[TestedType(typeof(AccountingTransactionExportGetAddresses))]
	class AccountingTransactionExportGetAddressesTest : DbCreateScriptTest
	{
		public void TestGetData()
		{
			var org1Pk = Guid.NewGuid();
			var org2Pk = Guid.NewGuid();
			var addr1Pk = Guid.NewGuid();
			var addr2Pk = Guid.NewGuid();
			var addr3Pk = Guid.NewGuid();
			var addr4Pk = Guid.NewGuid();
			var addr5Pk = Guid.NewGuid();
			var addr6Pk = Guid.NewGuid();
			var addr7Pk = Guid.NewGuid();
			var addr8Pk = Guid.NewGuid();
			var addr9Pk = Guid.NewGuid();

			var invoicePk = Guid.NewGuid();
			var invoice2Pk = Guid.NewGuid();
			var invoice3Pk = Guid.NewGuid();
			var jobHeaderPk = Guid.NewGuid();

			var insertSql = $@"
declare @Org1 uniqueidentifier = '{org1Pk}'
declare @Org2 uniqueidentifier = '{org2Pk}'

declare @Addr1 uniqueidentifier = '{addr1Pk}'
declare @Addr2 uniqueidentifier = '{addr2Pk}'
declare @Addr3 uniqueidentifier = '{addr3Pk}'
declare @Addr4 uniqueidentifier = '{addr4Pk}'
declare @Addr5 uniqueidentifier = '{addr5Pk}'
declare @Addr6 uniqueidentifier = '{addr6Pk}'
declare @Addr7 uniqueidentifier = '{addr7Pk}'
declare @Addr8 uniqueidentifier = '{addr8Pk}'
declare @Addr9 uniqueidentifier = '{addr9Pk}'

insert into dbo.OrgHeader (OH_PK, OH_Code)
values
	(@Org1, 'DDDABCSYD'),
	(@Org2, 'DDDDEFMEL')

insert into dbo.OrgAddress (OA_PK, OA_OH, OA_Code, OA_Address1)
values
	(@Addr1, @Org1, '1 Test', '1 Test Rd'),
	(@Addr2, @Org1, '2 Test', '2 Test Rd'),
	(@Addr3, @Org1, '3 Test', '3 Test Rd'),
	(@Addr4, @Org1, '4 Test', '4 Test Rd'),
	(@Addr5, @Org1, '5 Test', '5 Test Rd'),
	(@Addr6, @Org1, '6 Test', '6 Test Rd'),
	(@Addr7, @Org1, '7 Test', '7 Test Rd'),
	(@Addr8, @Org1, '8 Test', '8 Test Rd'),
	(@Addr9, @Org1, '9 Test', '9 Test Rd')

insert into dbo.OrgAddressCapability (PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress)
values
	(NEWID(), @Addr1, 'ARM', 1),
	(NEWID(), @Addr2, 'APM', 1),
	(NEWID(), @Addr3, 'OFC', 1),
	(NEWID(), @Addr4, 'ARM', 1),
	(NEWID(), @Addr5, 'APM', 0),
	(NEWID(), @Addr6, 'OFC', 0)

DECLARE @CompanyPK UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
DECLARE @BranchPK UNIQUEIDENTIFIER = '27A55065-AC88-4EC3-8BED-E575E79172CB'
DECLARE @DepartmentPK UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491'

INSERT INTO dbo.JobHeader (JH_PK, JH_ParentID, JH_ParentTableCode, JH_JobNum, JH_GC, JH_GB, JH_GE, JH_Status,JH_OA_LocalChargesAddr, JH_OA_AgentCollectAddr)
VALUES ('{jobHeaderPk}', NEWID(), 'JS', '000000001', @CompanyPk, @BranchPK, @DepartmentPK, 'WRK', @Addr8, @Addr9)

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_TransactionType, AH_OH, AH_Ledger, AH_JH, AH_OA_InvoiceAddressOverride) 
VALUES ('{invoicePk}', @CompanyPk, @BranchPK, @DepartmentPK, '2015-5-5', '2015-5-5', 'INV', '{org1Pk}', 'AR', '{jobHeaderPk}', @Addr7)

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_TransactionType, AH_OH, AH_Ledger, AH_JH, AH_OA_InvoiceAddressOverride) 
VALUES ('{invoice2Pk}', @CompanyPk, @BranchPK, @DepartmentPK, '2015-5-5', '2015-5-5', 'INV', '{org1Pk}', 'AP', '{jobHeaderPk}', @Addr7)

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_TransactionType, AH_OH, AH_Ledger, AH_JH, AH_OA_InvoiceAddressOverride) 
VALUES ('{invoice3Pk}', @CompanyPk, @BranchPK, @DepartmentPK, '2015-5-5', '2015-5-5', 'INV', '{org1Pk}', 'GL', '{jobHeaderPk}', @Addr7)

INSERT INTO dbo.GenExportBatchSequence(XB_PK, XB_Type, XB_BatchNumber, XB_ParentTableCode, XB_ParentID, XB_SystemCreateTimeUtc, XB_SystemCreateUser)
VALUES (NEWID(), 'ARV', 1, 'AH', '{invoicePk}', GETUTCDATE(), '~BP')

INSERT INTO dbo.GenExportBatchSequence(XB_PK, XB_Type, XB_BatchNumber, XB_ParentTableCode, XB_ParentID, XB_SystemCreateTimeUtc, XB_SystemCreateUser)
VALUES (NEWID(), 'ARV', 1, 'AH', '{invoice2Pk}', GETUTCDATE(), '~BP')

INSERT INTO dbo.GenExportBatchSequence(XB_PK, XB_Type, XB_BatchNumber, XB_ParentTableCode, XB_ParentID, XB_SystemCreateTimeUtc, XB_SystemCreateUser)
VALUES (NEWID(), 'ARV', 1, 'AH', '{invoice3Pk}', GETUTCDATE(), '~BP')";
			TestConnection.ExecuteNonQuery(insertSql);

			var sql = $"EXEC AccountingTransactionExportGetAddresses 'EDI', 1, '{invoicePk}'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("7 Test", result.Rows[0]["OA_Code"]);

			TestConnection.ExecuteNonQuery($"UPDATE dbo.AccTransactionHeader SET AH_OA_InvoiceAddressOverride = NULL, AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK IN ('{invoicePk}','{invoice2Pk}','{invoice3Pk}')");
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("8 Test", result.Rows[0]["OA_Code"]);

			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobHeader
SET
	JH_OA_LocalChargesAddr = NULL,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = '{jobHeaderPk}'");
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("9 Test", result.Rows[0]["OA_Code"]);

			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobHeader
SET
	JH_OA_AgentCollectAddr = NULL,
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = '{jobHeaderPk}'");
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("1 Test", result.Rows[0]["OA_Code"]);

			sql = $"EXEC AccountingTransactionExportGetAddresses 'EDI', 1, '{invoice2Pk}'";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("2 Test", result.Rows[0]["OA_Code"]);

			sql = $"EXEC AccountingTransactionExportGetAddresses 'EDI', 1, '{invoice3Pk}'";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("3 Test", result.Rows[0]["OA_Code"]);
		}
	}
}
