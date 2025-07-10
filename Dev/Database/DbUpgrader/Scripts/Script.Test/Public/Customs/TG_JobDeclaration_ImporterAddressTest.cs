using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(TG_JobDeclaration_ImporterAddress))]
	class TG_JobDeclaration_ImporterAddressTest : DbCreateScriptTest
	{
		const string TriggerMessage = "JE_OA_ImporterAddress should match JE_OH_Importer: ";

		readonly Guid org1 = Guid.NewGuid();
		readonly Guid org2 = Guid.NewGuid();
		readonly Guid org3 = Guid.NewGuid();
		readonly Guid addr1 = Guid.NewGuid();
		readonly Guid addr2 = Guid.NewGuid();
		readonly Guid addr3 = Guid.NewGuid();
		readonly Guid companyPK = Guid.NewGuid();
		readonly Guid branchPK = Guid.NewGuid();

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
				insert into dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('{companyPK}', 'DAN', 'AU company', 'AU', 'AUD');
				insert into dbo.GlbBranch (GB_PK, GB_GC) VALUES ('{branchPK}', '{companyPK}');
			");
		}

		[ExpectNoExceptions]
		public void TestInsert_Valid()
		{
			TestConnection.ExecuteNonQuery($@"
				insert into dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_OH_Importer, JE_OA_ImporterAddress, JE_GB, JE_GC, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
				values
					(newid(), 'AU', 'DEC01', NULL, NULL, '{branchPK}', '{companyPK}',1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', 'DEC02', '{org1}', NULL, '{branchPK}', '{companyPK}', 2, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', 'DEC03', '{org2}', NULL, '{branchPK}', '{companyPK}', 3, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', 'DEC04', '{org1}', '{addr1}', '{branchPK}', '{companyPK}', 4, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', 'DEC05', '{org2}', '{addr2}', '{branchPK}', '{companyPK}', 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");
		}

		public void TestInsert_WrongImporter()
		{
			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery($@"
				insert into dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_OH_Importer, JE_OA_ImporterAddress, JE_GB, JE_GC, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
				values (newid(), 'AU', 'DEC06', '{org1}', '{addr2}', '{branchPK}', '{companyPK}', 6, GetUtcDate(), '~BP', GetUtcDate(), '~BP')")).Message);
		}

		public void TestInsert_EmptyImporter()
		{
			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery($@"
				insert into dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_OH_Importer, JE_OA_ImporterAddress, JE_GB, JE_GC, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
				values (newid(), 'AU', 'DEC07', NULL, '{addr2}', '{branchPK}', '{companyPK}', 7, GetUtcDate(), '~BP', GetUtcDate(), '~BP')")).Message);
		}

		public void TestInsert_MultipleRows()
		{
			var ex = AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery($@"
				insert into dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_OH_Importer, JE_OA_ImporterAddress, JE_GB, JE_GC, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
				values
					(newid(), 'AU', 'DEC08', NULL, '{addr1}', '{branchPK}', '{companyPK}', 8, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', 'DEC09', '{org1}', '{addr2}', '{branchPK}', '{companyPK}', 9, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					(newid(), 'AU', 'DEC10', '{org1}', '{addr1}', '{branchPK}', '{companyPK}', 10, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			"));

			AssertContains(TriggerMessage, ex.Message);
			AssertContains("DEC08", ex.Message);
			AssertContains("DEC09", ex.Message);
			AssertNotContains("DEC10", ex.Message);
		}

		public void TestUpdate_Importer()
		{
			Guid decPK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				insert into dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_OH_Importer, JE_OA_ImporterAddress, JE_GB, JE_GC, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
				values
					('{decPK}', 'AU', 'DEC11', '{org1}', '{addr1}', '{branchPK}', '{companyPK}', 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(
				$"update dbo.JobDeclaration set JE_OH_Importer = '{org2}', JE_SystemLastEditTimeUtc = GetUtcDate(), JE_SystemLastEditUser = '~BP' where JE_PK = '{decPK}'"
			)).Message);
		}

		public void TestUpdate_ImporterAddress()
		{
			Guid decPK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				insert into dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_OH_Importer, JE_OA_ImporterAddress, JE_GB, JE_GC, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
				values
					('{decPK}', 'AU', 'DEC12', '{org1}', '{addr1}', '{branchPK}', '{companyPK}', 12, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(
				$"update dbo.JobDeclaration set JE_OA_ImporterAddress = '{addr2}', JE_SystemLastEditTimeUtc = GetUtcDate(), JE_SystemLastEditUser = '~BP' where JE_PK = '{decPK}'"
			)).Message);
		}

		public void TestUpdate_ImporterAddress_EmptyImporter()
		{
			Guid decPK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				insert into dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_OH_Importer, JE_OA_ImporterAddress, JE_GB, JE_GC, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
				values ('{decPK}', 'AU', 'DEC1', NULL, NULL, '{branchPK}', '{companyPK}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(
				$"update dbo.JobDeclaration set JE_OA_ImporterAddress = '{addr2}', JE_SystemLastEditTimeUtc = GetUtcDate(), JE_SystemLastEditUser = '~BP' where JE_PK = '{decPK}'"
			)).Message);
		}

		public void TestUpdateExistingInvalid()
		{
			Guid dec1 = Guid.NewGuid();
			Guid dec2 = Guid.NewGuid();
			Guid dec3 = Guid.NewGuid();
			Guid dec4 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				DISABLE TRIGGER TG_JobDeclaration_ImporterAddress ON JobDeclaration;
				insert into dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_OH_Importer, JE_OA_ImporterAddress, JE_GB, JE_GC, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
				values
					('{dec1}', 'AU', 'DEC1', '{org1}', '{addr2}', '{branchPK}', '{companyPK}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{dec2}', 'AU', 'DEC2', '{org1}', '{addr2}', '{branchPK}', '{companyPK}', 2, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{dec3}', 'AU', 'DEC3', '{org1}', '{addr2}', '{branchPK}', '{companyPK}', 3, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
					('{dec4}', 'AU', 'DEC4', '{org1}', '{addr2}', '{branchPK}', '{companyPK}', 4, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				;
				ENABLE TRIGGER TG_JobDeclaration_ImporterAddress ON JobDeclaration;
			");

			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobDeclaration
SET
	JE_OA_ImporterAddress = '{addr1}',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{dec1}'");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobDeclaration
SET
	JE_OH_Importer = '{org2}',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{dec2}'");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobDeclaration
SET
	JE_OH_Importer = '{org3}',
	JE_OA_ImporterAddress = '{addr3}',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{dec3}'");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobDeclaration
SET
	JE_DeclarationReference = 'DEC4UPD',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{dec4}'");

			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobDeclaration
SET
	JE_OA_ImporterAddress = '{addr3}',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{dec4}'"
			)).Message);
		}
	}
}
