using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(TG_JobDeclaration_CompanyBranch))]
	class TG_JobDeclaration_CompanyBranchTest : DbCreateScriptTest
	{
		const string TriggerMessage = "JE_GB should match JE_GC: ";
		const string CanNotNullMessage = "Cannot insert the value NULL into column 'JE_GC'";

		readonly Guid company1 = Guid.NewGuid();
		readonly Guid company2 = Guid.NewGuid();
		readonly Guid company3 = Guid.NewGuid();
		readonly Guid branch1 = Guid.NewGuid();
		readonly Guid branch2 = Guid.NewGuid();
		readonly Guid branch3 = Guid.NewGuid();

		protected override void SetUp()
		{
			base.SetUp();

			TestConnection.ExecuteNonQuery($@"
				insert into dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) values ('{company1}', 'AU1', 'AU company1', 'AU', 'AUD');
				insert into dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) values ('{company2}', 'AU2', 'AU company2', 'AU', 'AUD');
				insert into dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) values ('{company3}', 'AU3', 'AU company3', 'AU', 'AUD');
				insert into dbo.GlbBranch(GB_PK, GB_GC, GB_Code) values ('{branch1}', '{company1}', 'AU1');
				insert into dbo.GlbBranch(GB_PK, GB_GC, GB_Code) values ('{branch2}', '{company2}', 'AU2');
				insert into dbo.GlbBranch(GB_PK, GB_GC, GB_Code) values ('{branch3}', '{company3}', 'AU3');
			");
		}

		[ExpectNoExceptions]
		public void TestInsert_Valid()
		{
			TestConnection.ExecuteNonQuery($@"
				insert into dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_GB, JE_GC, JE_ClusterKey)
				values
					(newid(), 'AU', 'DEC01', '{branch1}', '{company1}', 1),
					(newid(), 'AU', 'DEC02', '{branch2}', '{company2}', 2),
					(newid(), 'AU', 'DEC03', '{branch3}', '{company3}', 3)
			");
		}

		public void TestInsert_WrongMatch()
		{
			AssertContains(TriggerMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery($@"
				insert into dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_GB, JE_GC, JE_ClusterKey)
				values (newid(), 'AU', 'DEC04', '{branch1}', '{company2}', 1)")).Message);
		}

		public void TestInsert_EmptyCompanyPK()
		{
			AssertContains(CanNotNullMessage, AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery($@"
				insert into dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_GB, JE_GC, JE_ClusterKey)
				values (newid(), 'AU', 'DEC05', '{branch1}', NULL, 1)")).Message);
		}

		public void TestInsert_MultipleRows()
		{
			var ex = AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery($@"
				insert into dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_GB, JE_GC, JE_ClusterKey)
				values
					(newid(), 'AU', 'DEC06', '{branch1}', '{company2}', 1),
					(newid(), 'AU', 'DEC07', '{branch2}', '{company1}', 2),
					(newid(), 'AU', 'DEC08', '{branch1}', '{company1}', 3)
			"));

			AssertContains(TriggerMessage, ex.Message);
			AssertContains("DEC06", ex.Message);
			AssertContains("DEC07", ex.Message);
			AssertNotContains("DEC08", ex.Message);
		}

		public void TestUpdateExistingInvalid()
		{
			Guid dec1 = Guid.NewGuid();
			Guid dec2 = Guid.NewGuid();
			Guid dec3 = Guid.NewGuid();
			Guid dec4 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				DISABLE TRIGGER TG_JobDeclaration_CompanyBranch ON JobDeclaration;
				insert into dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_GB, JE_GC, JE_ClusterKey)
				values
					('{dec1}', 'AU', 'DEC1', '{branch1}', '{company2}', 1),
					('{dec2}', 'AU', 'DEC2', '{branch1}', '{company2}', 2),
					('{dec3}', 'AU', 'DEC3', '{branch1}', '{company2}', 3),
					('{dec4}', 'AU', 'DEC4', '{branch1}', '{company2}', 4)
				;
				ENABLE TRIGGER TG_JobDeclaration_CompanyBranch ON JobDeclaration;
			");

			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobDeclaration
SET
	JE_GC = '{company1}',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{dec1}'");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobDeclaration
SET
	JE_GB = '{branch2}',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{dec2}'");
			TestConnection.ExecuteNonQuery(@$"
UPDATE dbo.JobDeclaration
SET
	JE_GC = '{company3}',
	JE_GB = '{branch3}',
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
	JE_GC = '{company3}',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = '{dec4}'"
			)).Message);
		}
	}
}
