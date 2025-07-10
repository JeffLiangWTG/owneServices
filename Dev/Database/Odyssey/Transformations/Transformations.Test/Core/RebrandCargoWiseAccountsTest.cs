using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Core;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Core
{
	[TestedType(typeof(RebrandCargoWiseAccounts))]
	class RebrandCargoWiseAccountsTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			// The transform has already been run by the time this test is executed, so we need to put the usernames back to the old names
			TestConnection.ExecuteNonQuery("""
				UPDATE dbo.GlbStaff
				SET
					GS_LoginName = 'CW1Support',
					GS_FullName = 'CargoWise One Support',
					GS_SystemLastEditUser = '~BP',
					GS_SystemLastEditTimeUtc = GETUTCDATE()
				WHERE GS_LoginName = 'CWSupport';

				UPDATE dbo.GlbStaff
				SET
					GS_LoginName = 'CW1PostMaster',
					GS_FullName = 'CargoWise One Post Master',
					GS_SystemLastEditUser = '~BP',
					GS_SystemLastEditTimeUtc = GETUTCDATE()
				WHERE GS_LoginName = 'CWPostMaster';
				""");

			// Also add some test data in case the customer has manually created accounts which match the new names, or there is a registry item for the certificate
			TestConnection.ExecuteNonQuery("""
				INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_FullName, GS_IsSystemAccount, GS_Code, GS_SystemLastEditUser, GS_SystemLastEditTimeUtc, GS_SystemCreateUser, GS_SystemCreateTimeUtc)
				VALUES (newid(), 'CWSupport', 'Some manually created support account', 0, 'CWS', '~BP', GETUTCDATE(), '~BP', GETUTCDATE());

				INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_FullName, GS_IsSystemAccount, GS_Code, GS_SystemLastEditUser, GS_SystemLastEditTimeUtc, GS_SystemCreateUser, GS_SystemCreateTimeUtc)
				VALUES (newid(), 'CWSupport_', 'Some other created support account', 0, 'CW2', '~BP', GETUTCDATE(), '~BP', GETUTCDATE());
				
				INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_FullName, GS_IsSystemAccount, GS_Code, GS_SystemLastEditUser, GS_SystemLastEditTimeUtc, GS_SystemCreateUser, GS_SystemCreateTimeUtc)
				VALUES (newid(), 'CWPostMaster', 'Some manually created post master', 0, 'POS', '~BP', GETUTCDATE(), '~BP', GETUTCDATE());

				INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemCreateTimeUtc)
				VALUES (newid(), 'CW1SupportLoginTokenCertificate', 1234, '~BP', GETUTCDATE(), '~BP', GETUTCDATE());

				INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemCreateTimeUtc)
				VALUES (newid(), 'CW1SupportLoginTokenPrivateKey', 9876, '~BP', GETUTCDATE(), '~BP', GETUTCDATE());
				""");
		}

		protected override DataTransformation GetNewTestTransformationInstance()
			=> new RebrandCargoWiseAccounts();

		protected override void AssertPreConditions()
		{
			var cw1SupportAccountCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_Code = 'E' AND GS_LoginName = 'CW1Support' AND GS_IsSystemAccount = 1");
			AssertEquals("CW1Support account should be created", 1, cw1SupportAccountCount);

			var cw1PostMasterAccountCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_Code = 'PM' AND GS_LoginName = 'CW1PostMaster' AND GS_IsSystemAccount = 1");
			AssertEquals("CW1PostMaster account should be created", 1, cw1PostMasterAccountCount);

			AssertEquals("Certificate registry override under the old name should exist", 1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'CW1SupportLoginTokenCertificate'"));
			AssertEquals("Certificate registry override under the new name should NOT exist", 0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'CWSupportLoginTokenCertificate'"));
			AssertEquals("Private key registry override under the old name should exist", 1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'CW1SupportLoginTokenPrivateKey'"));
			AssertEquals("Private key registry override under the new name should NOT exist", 0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'CWSupportLoginTokenPrivateKey'"));
		}

		override protected void AssertTransformationResults()
		{
			var cw1SupportAccountCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_LoginName = 'CW1Support'");
			AssertEquals("CW1Support account should be renamed", 0, cw1SupportAccountCount);

			var cw1PostMasterAccountCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_LoginName = 'CW1PostMaster'");
			AssertEquals("CW1PostMaster account should be renamed", 0, cw1PostMasterAccountCount);

			var cwSupportAccountCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_Code = 'E' AND GS_LoginName = 'CWSupport' AND GS_FullName = 'CargoWise Support' AND GS_IsSystemAccount = 1");
			AssertEquals("CWSupport account should have new name", 1, cwSupportAccountCount);

			var cwPostMasterAccountCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_Code = 'PM' AND GS_LoginName = 'CWPostMaster' AND GS_FullName = 'CargoWise Post Master' AND GS_IsSystemAccount = 1");
			AssertEquals("CWPostMaster account should have new name", 1, cwPostMasterAccountCount);

			var cwSupportUnderscoreAccountCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_LoginName = 'CWSupport_' AND GS_FullName = 'Some manually created support account' AND GS_IsSystemAccount = 0");
			AssertEquals("CWSupport account should have underscore appended", 1, cwSupportUnderscoreAccountCount);

			var cwSupportUnderscoreUnderscoreAccountCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_LoginName = 'CWSupport__' AND GS_FullName = 'Some other created support account' AND GS_IsSystemAccount = 0");
			AssertEquals("CWSupport_ account should have underscore appended", 1, cwSupportUnderscoreUnderscoreAccountCount);

			AssertEquals("Certificate registry override under the old name should NOT exist", 0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'CW1SupportLoginTokenCertificate'"));
			AssertEquals("Certificate registry override under the new name should exist", 1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'CWSupportLoginTokenCertificate' and SD_BinaryValue = 1234"));
			AssertEquals("Private key registry override under the old name should NOT exist", 0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'CW1SupportLoginTokenPrivateKey'"));
			AssertEquals("Private key registry override under the new name should exist", 1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'CWSupportLoginTokenPrivateKey' and SD_BinaryValue = 9876"));
		}
	}
}
