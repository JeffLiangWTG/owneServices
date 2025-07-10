using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using CargoWise.Definitions;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccTemplateFileStorage_Update))]
	class TG_AccTemplateFileStorage_UpdateTest : DBCreateTriggerScriptTest
	{
		public void TestAccTemplateFileStorageInsertUpdate()
		{
			var templateCode = "S01";
			var templateCodeForUpdate = "SU1";
			TestDataCreator.InsertTESJobConfig(TestConnection, CompanyPK, "", Guid.Empty, templateCode, ledger: LedgerTypeCodes.AccountsReceivable);
			TestDataCreator.InsertTemplateFile(TestConnection, CompanyPK, templateCode, ledger: LedgerTypeCodes.AccountsPayable);

			var createdConfigRecordsAR = SelectAccJobConfigWithTESConfiguration(CompanyPK, templateCode, LedgerTypeCodes.AccountsReceivable);
			var createdConfigRecordsAP = SelectAccJobConfigWithTESConfiguration(CompanyPK, templateCode, LedgerTypeCodes.AccountsPayable);
			AssertEquals("No configuration data attachted to AP template", 0, createdConfigRecordsAP);

			var updatedConfigRecordsAR = UpdateTemplateFile(CompanyPK, LedgerTypeCodes.AccountsReceivable, templateCode, templateCodeForUpdate);
			AssertEquals("Created and updated record count must be equal.", createdConfigRecordsAR, updatedConfigRecordsAR);

			var updatedConfigRecordsAP = UpdateTemplateFile(CompanyPK, LedgerTypeCodes.AccountsPayable, templateCode, templateCodeForUpdate);
			AssertEquals("No configuration data updated for AP template", 0, createdConfigRecordsAP);
			AssertEquals("Created and updated record count must be equal.", createdConfigRecordsAP, createdConfigRecordsAP);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CompanyPK = TestDbHelper.DefaultCompanyPK;
		}

		int SelectAccJobConfigWithTESConfiguration(Guid companyPK, string templateCode, string ledger)
		{
			var sql = @"SELECT COUNT(*) FROM dbo.AccJobConfig WHERE JCF_GC = @GC AND JCF_Code = @Code AND JCF_Ledger = @Ledger";

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, companyPK);
				cmd.AddParameter("@Code", SqlDbType.VarChar, templateCode);
				cmd.AddParameter("@Ledger", SqlDbType.VarChar, ledger);
				return (int)cmd.ExecuteScalar();
			}
		}

		int UpdateTemplateFile(Guid companyPK, string ledger, string templateCode, string templateCodeForUpdate)
		{
			var sql = @"UPDATE dbo.AccTemplateFileStorage SET TFS_Code = @NewCode, TFS_SystemLastEditTimeUtc = GETUTCDATE(), TFS_SystemLastEditUser = 'TST' WHERE TFS_GC = @GC AND TFS_Ledger = @Ledger AND TFS_Code = @OldCode";

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("@GC", SqlDbType.UniqueIdentifier, companyPK);
				cmd.AddParameter("@Ledger", SqlDbType.VarChar, ledger);
				cmd.AddParameter("@OldCode", SqlDbType.VarChar, templateCode);
				cmd.AddParameter("@NewCode", SqlDbType.VarChar, templateCodeForUpdate);
				return cmd.ExecuteNonQuery();
			}
		}

		Guid CompanyPK;
	}
}
