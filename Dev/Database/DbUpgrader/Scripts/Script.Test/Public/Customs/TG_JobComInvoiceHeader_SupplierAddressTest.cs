using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(TG_JobComInvoiceHeader_SupplierAddress))]
	class TG_JobComInvoiceHeader_SupplierAddressTest : DbCreateScriptTest
	{
		const string TriggerMessage = "JZ_OA_SupplierAddress should match JZ_OH_Supplier: ";

		readonly Guid org1 = Guid.NewGuid();
		readonly Guid org2 = Guid.NewGuid();
		readonly Guid org3 = Guid.NewGuid();
		readonly Guid addr1 = Guid.NewGuid();
		readonly Guid addr2 = Guid.NewGuid();
		readonly Guid addr3 = Guid.NewGuid();

		protected override void SetUp()
		{
			base.SetUp();

			TestConnection.ExecuteNonQuery($@"
				insert into dbo.OrgHeader(OH_PK, OH_Code, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) values ('{org1}', 'TESTORG1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				insert into dbo.OrgHeader(OH_PK, OH_Code, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) values ('{org2}', 'TESTORG2', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				insert into dbo.OrgHeader(OH_PK, OH_Code, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) values ('{org3}', 'TESTORG3', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				insert into dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) values ('{addr1}', '{org1}', 'ADDRESS 1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				insert into dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) values ('{addr2}', '{org2}', 'ADDRESS 2', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				insert into dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) values ('{addr3}', '{org3}', 'ADDRESS 3', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
			");
		}

		[ExpectNoExceptions]
		public void TestInsert_Valid()
		{
			TestConnection.ExecuteNonQuery($@"
				DECLARE @anyBranchPK uniqueidentifier = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch);
				insert into dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_GB, JZ_InvoiceNumber, JZ_OH_Supplier, JZ_OA_SupplierAddress, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				values
					(newid(), 'AU', @anyBranchPK, 'INV1', NULL, NULL, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', @anyBranchPK, 'INV2', '{org1}', NULL, 2, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', @anyBranchPK, 'INV3', '{org2}', NULL, 3, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', @anyBranchPK, 'INV4', '{org1}', '{addr1}', 4, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', @anyBranchPK, 'INV5', '{org2}', '{addr2}', 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");
		}

		public void TestInsert_WrongSupplier()
		{
			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery($@"
				DECLARE @anyBranchPK uniqueidentifier = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch);
				insert into dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_GB, JZ_InvoiceNumber, JZ_OH_Supplier, JZ_OA_SupplierAddress, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				values
					(newid(), 'AU', @anyBranchPK, 'INV7', '{org1}', '{addr2}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			")).Message);
		}

		public void TestInsert_EmptySupplier()
		{
			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery($@"
				DECLARE @anyBranchPK uniqueidentifier = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch);
				insert into dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_GB, JZ_InvoiceNumber, JZ_OH_Supplier, JZ_OA_SupplierAddress, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				values
					(newid(), 'AU', @anyBranchPK, 'INV7', NULL, '{addr2}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			")).Message);
		}

		public void TestInsert_MultipleRows()
		{
			var ex = AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery($@"
				DECLARE @anyBranchPK uniqueidentifier = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch);
				insert into dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_GB, JZ_InvoiceNumber, JZ_OH_Supplier, JZ_OA_SupplierAddress, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				values
					(newid(), 'AU', @anyBranchPK, 'INV1', NULL, '{addr1}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', @anyBranchPK, 'INV2', '{org1}', '{addr2}', 2, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', @anyBranchPK, 'INV3', '{org1}', '{addr1}', 3, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			"));

			AssertContains(TriggerMessage, ex.Message);
			AssertContains("INV1", ex.Message);
			AssertContains("INV2", ex.Message);
			AssertNotContains("INV3", ex.Message);
		}

		public void TestUpdate_Supplier()
		{
			Guid invoice = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				DECLARE @anyBranchPK uniqueidentifier = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch);
				insert into dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_GB, JZ_InvoiceNumber, JZ_OH_Supplier, JZ_OA_SupplierAddress, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				values
					('{invoice}', 'AU', @anyBranchPK, 'INV1', '{org1}', '{addr1}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(
				$"update dbo.JobComInvoiceHeader set JZ_OH_Supplier = '{org2}', JZ_SystemLastEditTimeUtc = GetUtcDate(), JZ_SystemLastEditUser = '~BP' where JZ_PK = '{invoice}'"
			)).Message);
		}

		public void TestUpdate_SupplierAddress()
		{
			Guid invoice = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				DECLARE @anyBranchPK uniqueidentifier = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch);
				insert into dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_GB, JZ_InvoiceNumber, JZ_OH_Supplier, JZ_OA_SupplierAddress, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				values
					('{invoice}', 'AU', @anyBranchPK, 'INV1', '{org1}', '{addr1}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(
				$"update dbo.JobComInvoiceHeader set JZ_OA_SupplierAddress = '{addr2}', JZ_SystemLastEditTimeUtc = GetUtcDate(), JZ_SystemLastEditUser = '~BP' where JZ_PK = '{invoice}'"
			)).Message);
		}

		public void TestUpdate_SupplierAddress_EmptySupplier()
		{
			Guid invoice = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				DECLARE @anyBranchPK uniqueidentifier = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch);
				insert into dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_GB, JZ_InvoiceNumber, JZ_OH_Supplier, JZ_OA_SupplierAddress, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				values
					('{invoice}', 'AU', @anyBranchPK, 'INV1', NULL, NULL, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(
				$"update dbo.JobComInvoiceHeader set JZ_OA_SupplierAddress = '{addr2}', JZ_SystemLastEditTimeUtc = GetUtcDate(), JZ_SystemLastEditUser = '~BP' where JZ_PK = '{invoice}'"
			)).Message);
		}

		public void TestUpdateExistingInvalid()
		{
			Guid invoice1 = Guid.NewGuid();
			Guid invoice2 = Guid.NewGuid();
			Guid invoice3 = Guid.NewGuid();
			Guid invoice4 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				DISABLE TRIGGER TG_JobComInvoiceHeader_SupplierAddress ON JobComInvoiceHeader;
				DECLARE @anyBranchPK uniqueidentifier = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch);
				insert into dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_GB, JZ_InvoiceNumber, JZ_OH_Supplier, JZ_OA_SupplierAddress, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser)
				values
					('{invoice1}', 'AU', @anyBranchPK, 'INV1', '{org1}', '{addr2}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{invoice2}', 'AU', @anyBranchPK, 'INV2', '{org1}', '{addr2}', 2, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{invoice3}', 'AU', @anyBranchPK, 'INV3', '{org1}', '{addr2}', 3, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{invoice4}', 'AU', @anyBranchPK, 'INV4', '{org1}', '{addr2}', 4, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				;
				ENABLE TRIGGER TG_JobComInvoiceHeader_SupplierAddress ON JobComInvoiceHeader;
			");

			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_OA_SupplierAddress = '{addr1}',
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoice1}'");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_OH_Supplier = '{org2}',
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoice2}'");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_OH_Supplier = '{org3}',
	JZ_OA_SupplierAddress = '{addr3}',
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoice3}'");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_InvoiceNumber = 'INV4UPD',
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoice4}'");

			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_OA_SupplierAddress = '{addr3}',
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoice4}'"
			)).Message);
		}
	}
}
