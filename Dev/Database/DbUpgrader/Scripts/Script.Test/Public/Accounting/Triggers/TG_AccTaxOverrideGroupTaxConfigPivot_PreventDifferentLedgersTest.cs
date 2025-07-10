using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccTaxOverrideGroupTaxConfigPivot_PreventDifferentLedgers))]
	class TG_AccTaxOverrideGroupTaxConfigPivot_PreventDifferentLedgersTest : DBCreateTriggerScriptTest
	{
		public void TestInsertTaxConfigurationOfSameLedgerAndUpdateDifferentLedger()
		{
			AssertNoExceptionThrown($"Insert Tax Configuration of same Ledger for same Tax Override Group", () => DbHelper.InsertTaxOverrideGroupTaxConfigurationPivot(taxOverrideGroupPK, taxConfigurationPK2AR, taxIdPK));
			AssertNoExceptionThrown($"Update Tax Configuration of same Ledger for same Tax Override Group", () => DbHelper.RunSQL(new { TaxConfigurationPK = taxConfigurationPK3AR, PivotPK = pivot1PK }, "UPDATE dbo.AccTaxOverrideGroupTaxConfigurationPivot SET AXP_ETC_TaxConfiguration = @TaxConfigurationPK, AXP_SystemLastEditTimeUtc = GETUTCDATE(), AXP_SystemLastEditUser = 'TST' WHERE AXP_PK = @PivotPK"));
			AssertDbAction($"Update Tax Configuration of different Ledger for same Tax Override Group", true, () => DbHelper.RunSQL(new { TaxConfigurationPK = taxConfigurationPK1AP, PivotPK = pivot1PK }, "UPDATE dbo.AccTaxOverrideGroupTaxConfigurationPivot SET AXP_ETC_TaxConfiguration = @TaxConfigurationPK, AXP_SystemLastEditTimeUtc = GETUTCDATE(), AXP_SystemLastEditUser = 'TST' WHERE AXP_PK = @PivotPK"));
		}

		public void TestUpdateTaxConfigurationOfDifferentLedgerThenInsert()
		{
			AssertNoExceptionThrown($"Update Tax Configuration of different Ledger for same Tax Override Group", () => DbHelper.RunSQL(new { TaxConfigurationPK = taxConfigurationPK1AP, PivotPK = pivot1PK }, "UPDATE dbo.AccTaxOverrideGroupTaxConfigurationPivot SET AXP_ETC_TaxConfiguration = @TaxConfigurationPK, AXP_SystemLastEditTimeUtc = GETUTCDATE(), AXP_SystemLastEditUser = 'TST' WHERE AXP_PK = @PivotPK"));
			AssertDbAction($"Insert Tax Configuration of different Ledger for same Tax Override Group", true, () => DbHelper.InsertTaxOverrideGroupTaxConfigurationPivot(taxOverrideGroupPK, taxConfigurationPK2AR, taxIdPK));
		}

		static void AssertDbAction(string message, bool hasDifferentLedgers, AnonymousMethod dbAction)
		{
			if (hasDifferentLedgers)
			{
				AssertExceptionThrown<SqlException>(message, "Tax Override Group cannot have Tax configurations of different Ledgers.\r\nThe transaction ended in the trigger. The batch has been aborted.", dbAction);
			}
			else
			{
				AssertNoExceptionThrown(message, dbAction);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var companyPK = TestDbHelper.DefaultCompanyPK;
			taxIdPK = DbHelper.InsertTaxRate("TAX1");
			taxConfigurationPK1AR = DbHelper.InsertTaxConfiguration("TCONFIG1AR", companyPK, ledger: "AR");
			taxConfigurationPK2AR = DbHelper.InsertTaxConfiguration("TCONFIG2AR", companyPK, ledger: "AR");
			taxConfigurationPK3AR = DbHelper.InsertTaxConfiguration("TCONFIG3AR", companyPK, ledger: "AR");
			taxConfigurationPK1AP = DbHelper.InsertTaxConfiguration("TCONFIG4AP", companyPK, ledger: "AP");
			taxOverrideGroupPK = DbHelper.InsertTaxOverrideGroup("GRP1");
			pivot1PK = DbHelper.InsertTaxOverrideGroupTaxConfigurationPivot(taxOverrideGroupPK, taxConfigurationPK1AR, taxIdPK);
		}

		Guid taxIdPK;
		Guid taxConfigurationPK1AR;
		Guid taxConfigurationPK2AR;
		Guid taxConfigurationPK3AR;
		Guid taxConfigurationPK1AP;
		Guid taxOverrideGroupPK;
		Guid pivot1PK;
	}
}
