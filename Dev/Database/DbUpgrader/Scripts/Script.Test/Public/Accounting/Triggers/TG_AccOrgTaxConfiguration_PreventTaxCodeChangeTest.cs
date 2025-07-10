using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccOrgTaxConfiguration_PreventTaxCodeChange))]
	class TG_AccOrgTaxConfiguration_PreventTaxCodeChangeTest : DBCreateTriggerScriptTest
	{
		public void TestTriggerErrorOnUpdateOTC_ETC()
		{
			Assert("Precondition: TG_AccOrgTaxConfiguration_PreventTaxCodeChange trigger does exist", DoesTG_AccOrgTaxConfiguration_PreventTaxCodeChange_Exists());

			var expectedErrorMessage = "OTC_ETC value can not be updated.";
			var taxConfigurationPK1 = DbHelper.InsertTaxConfiguration("XX1", companyPK);

			var sql = @"UPDATE dbo.AccOrgTaxConfiguration SET OTC_ETC = @OTC_ETC, OTC_SystemLastEditTimeUtc = GETUTCDATE(), OTC_SystemLastEditUser = 'TST' WHERE OTC_PK = @OTC_PK";
			AssertExceptionThrown<SqlException>("Should be: " + expectedErrorMessage, expectedErrorMessage + "\r\nThe transaction ended in the trigger. The batch has been aborted.", () => DbHelper.RunSQL(new { OTC_ETC = taxConfigurationPK1, OTC_PK = orgTaxConfigPK }, sql));
		}

		public void TestTriggerNotExecutedOnUpdateOtherField()
		{
			Assert("Precondition: TG_AccOrgTaxConfiguration_PreventTaxCodeChange trigger does exist", DoesTG_AccOrgTaxConfiguration_PreventTaxCodeChange_Exists());

			var sql = @"UPDATE dbo.AccOrgTaxConfiguration SET OTC_IsActive = @OTC_IsActive, OTC_SystemLastEditTimeUtc = GETUTCDATE(), OTC_SystemLastEditUser = 'TST' WHERE OTC_PK = @OTC_PK";
			AssertNoExceptionThrown(() => DbHelper.RunSQL(new { OTC_IsActive = 0, OTC_PK = orgTaxConfigPK }, sql));
		}

		public void TestTriggerNotExecutedOnInsert()
		{
			Assert("Precondition: TG_AccOrgTaxConfiguration_PreventTaxCodeChange trigger does exist", DoesTG_AccOrgTaxConfiguration_PreventTaxCodeChange_Exists());
			var taxConfigurationPK1 = DbHelper.InsertTaxConfiguration("XX1", companyPK);

			var sql = @"INSERT INTO dbo.AccOrgTaxConfiguration (OTC_PK, OTC_ETC, OTC_OB, OTC_IsActive, OTC_RecoverTax, OTC_IsThresholdUsed, OTC_OCT, OTC_SystemCreateTimeUtc, OTC_SystemCreateUser, OTC_SystemLastEditTimeUtc, OTC_SystemLastEditUser)
VALUES (@OTC_PK, @OTC_ETC, @OTC_OB, 1, 0, 0, NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			AssertNoExceptionThrown(() => DbHelper.RunSQL(new { OTC_PK = Guid.NewGuid(), OTC_ETC = taxConfigurationPK1, OTC_OB = orgCompanyDataPK }, sql));
		}

		public void TestTriggerNotExecutedOnDeleted()
		{
			Assert("Precondition: TG_AccOrgTaxConfiguration_PreventTaxCodeChange trigger does exist", DoesTG_AccOrgTaxConfiguration_PreventTaxCodeChange_Exists());

			var sql = @"DELETE FROM dbo.AccOrgTaxConfiguration WHERE OTC_PK = @OTC_PK";
			AssertNoExceptionThrown(() => DbHelper.RunSQL(new { OTC_PK = orgTaxConfigPK }, sql));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var organizationPK = DbHelper.InsertOrgHeader("ARCO1", "MY ORGANISATION");
			orgCompanyDataPK = DbHelper.InsertOrgCompanyData(organizationPK, companyPK);
			taxConfigurationPK = DbHelper.InsertTaxConfiguration("AR1", companyPK);

			orgTaxConfigPK = DbHelper.InsertOrgTaxConfiguration(taxConfigurationPK, orgCompanyDataPK);
		}

		readonly Guid companyPK = TestDbHelper.DefaultCompanyPK;
		Guid orgTaxConfigPK;
		Guid taxConfigurationPK;
		Guid orgCompanyDataPK;

		bool DoesTG_AccOrgTaxConfiguration_PreventTaxCodeChange_Exists() => TestConnection.Exists("FROM sys.triggers WHERE name = 'TG_AccOrgTaxConfiguration_PreventTaxCodeChange'");
	}
}
