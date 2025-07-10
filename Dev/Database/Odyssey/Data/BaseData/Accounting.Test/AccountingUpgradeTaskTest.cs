using System;
using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.Accounting
{
	sealed class AccountingUpgradeTaskTest : TransactionedTestCase
	{
		public void TestAllAC_AW_WithHoldingTaxRatesInXmlAreNull()
		{
			AccountingUpgradeTask testTask = new AccountingUpgradeTask();
			testTask.Run();

			string sqlText = "SELECT TOP 1 AC_PK FROM dbo.AccChargeCode WHERE AC_AW_WithHoldingTaxRate is not null";
			AssertEquals("Non null RP_OH_Supplier exists?", false, BaseDataUpgradeTask.IsRecordInDatabase(sqlText));
		}

		public void TestRun()
		{
			// Prepare test data
			Guid newCreditorGroupPk = Guid.NewGuid();
			Guid intercompanyCreditorGroupPk = new Guid("DFE48C5A-D0D3-4065-B352-466FFF4B3959");

			Guid newDebtorGroupPk = Guid.NewGuid();
			Guid intercompanyDebtorGroupPk = new Guid("6D8A91A7-781D-4FF1-9E32-8B7F41CFB49F");
			Guid thirdPartyCompanyDebtorGroupPkOld = new Guid("3A57AF93-D305-4B62-8B5A-6E94F22A766A");
			Guid thirdPartyCompanyDebtorGroupPkNew = Guid.NewGuid();

			Guid newGLHeaderPk = Guid.NewGuid();
			Guid dutyGLHeaderPk = new Guid("29551600-4BE4-427B-9155-74F2E4A9590A");

			Guid newAccGroupPk = Guid.NewGuid();
			Guid staffAccGroupPk = new Guid("8BD57F60-189C-4C0A-9694-1FD699459B5F");

			Guid newTaxRatePk = Guid.NewGuid();
			Guid gstTaxRatePk = new Guid("3013285c-2193-469c-9f30-7a552977a7b4");

			Guid newChargeCodePk = Guid.NewGuid();
			Guid originTranshipmentChargeCodePk = new Guid("05A86011-F654-4EC8-9D85-7C836E77F18B");

			string sqlText = String.Format(@"
				-- OrgCreditorGroup
				INSERT dbo.OrgCreditorGroup (OG_PK, OG_Code, OG_Desc) VALUES ('{0}', '~C1', '~CreditorGroup');
				UPDATE dbo.OrgCreditorGroup SET OG_Desc = 'Some~Desc~Not~To~Be~Changed' WHERE OG_PK = '{1}';
				-- OrgDebtorGroup
				INSERT dbo.OrgDebtorGroup (OJ_PK, OJ_Code, OJ_Desc) VALUES ('{2}', '~C1', '~DebtorGroup');
				UPDATE dbo.OrgDebtorGroup SET OJ_Desc = 'Some~Desc~Not~To~Be~Changed' WHERE OJ_PK = '{3}';
				UPDATE dbo.OrgDebtorGroup SET OJ_PK = '{4}' WHERE OJ_Code = 'TPY';
				-- AccGLHeader
				INSERT dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit) VALUES ('{5}', '~C1', '~GLHeader', 'BSH', 'DR');
				UPDATE dbo.AccGLHeader SET AG_Description = 'Some~Desc~Not~To~Be~Changed', AG_SystemLastEditTimeUtc = GETUTCDATE(), AG_SystemLastEditUser = 'TST' WHERE AG_PK = '{6}';
				-- AccGroups
				INSERT dbo.AccGroups (AR_PK, AR_Code, AR_Desc) VALUES ('{7}', '~C1', '~AccGroup');
				UPDATE dbo.AccGroups SET AR_Desc = 'Some~Desc~Not~To~Be~Changed', AR_SystemLastEditTimeUtc = GETUTCDATE(), AR_SystemLastEditUser = 'TST' WHERE AR_PK = '{8}';
				-- AccTaxRate
				INSERT dbo.AccTaxRate (AT_PK, AT_Type, AT_Code, AT_Description) VALUES ('{9}', 'RAT', '~C1', '~TaxRate');
				UPDATE dbo.AccTaxRate SET AT_Description = 'Some~Desc~Not~To~Be~Changed', AT_SystemLastEditTimeUtc = GETUTCDATE(), AT_SystemLastEditUser = 'TST' WHERE AT_PK = '{10}';
				-- AccChargeCode
				INSERT dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_Desc, AC_GC) VALUES ('{11}', '~C1', 'FRT', '~ChargeCode', '03052ED3-2C64-49AC-97D8-C6079D5015B5');
				UPDATE dbo.AccChargeCode SET AC_Desc = 'Some~Desc~Not~To~Be~Changed', AC_SystemLastEditTimeUtc = GETUTCDATE(), AC_SystemLastEditUser = 'TST' WHERE AC_PK = '{12}';",
				newCreditorGroupPk,
				intercompanyCreditorGroupPk,
				newDebtorGroupPk,
				intercompanyDebtorGroupPk,
				thirdPartyCompanyDebtorGroupPkNew,
				newGLHeaderPk,
				dutyGLHeaderPk,
				newAccGroupPk,
				staffAccGroupPk,
				newTaxRatePk,
				gstTaxRatePk,
				newChargeCodePk,
				originTranshipmentChargeCodePk);

			// OrgDebtorGroup
			AssertEquals("[INITAL DATA] Old TPY OrgDebtorGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgDebtorGroupSchema.PK, thirdPartyCompanyDebtorGroupPkOld));
			AssertEquals("[INITAL DATA] New TPY OrgDebtorGroup in database?", false, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgDebtorGroupSchema.PK, thirdPartyCompanyDebtorGroupPkNew));

			TestConnection.ExecuteNonQuery(sqlText);

			// OrgCreditorGroup
			AssertEquals("[BEFORE UPGRADE] New OrgCreditorGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgCreditorGroupSchema.PK, newCreditorGroupPk));
			AssertEquals("[BEFORE UPGRADE] Existing OrgCreditorGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgCreditorGroupSchema.PK, intercompanyCreditorGroupPk));

			// OrgDebtorGroup
			AssertEquals("[BEFORE UPGRADE] New OrgDebtorGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgDebtorGroupSchema.PK, newDebtorGroupPk));
			AssertEquals("[BEFORE UPGRADE] Existing OrgDebtorGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgDebtorGroupSchema.PK, intercompanyDebtorGroupPk));
			AssertEquals("[BEFORE UPGRADE] Old TPY OrgDebtorGroup in database?", false, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgDebtorGroupSchema.PK, thirdPartyCompanyDebtorGroupPkOld));
			AssertEquals("[BEFORE UPGRADE] New TPY OrgDebtorGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgDebtorGroupSchema.PK, thirdPartyCompanyDebtorGroupPkNew));

			// AccGLHeader
			AssertEquals("[BEFORE UPGRADE] New AccGLHeader in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccGLHeaderSchema.PK, newGLHeaderPk));
			AssertEquals("[BEFORE UPGRADE] Existing AccGLHeader in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccGLHeaderSchema.PK, dutyGLHeaderPk));

			// AccGroups
			AssertEquals("[BEFORE UPGRADE] New AccGroups in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccGroupsSchema.PK, newAccGroupPk));
			AssertEquals("[BEFORE UPGRADE] Existing AccGroups in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccGroupsSchema.PK, staffAccGroupPk));

			// AccTaxRate
			AssertEquals("[BEFORE UPGRADE] New AccTaxRate in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccTaxRateSchema.PK, newTaxRatePk));
			AssertEquals("[BEFORE UPGRADE] Existing AccTaxRate in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccTaxRateSchema.PK, gstTaxRatePk));

			// AccChargeCode
			AssertEquals("[BEFORE UPGRADE] New AccChargeCode in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccChargeCodeSchema.PK, newChargeCodePk));
			AssertEquals("[BEFORE UPGRADE] Existing AccChargeCode in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccChargeCodeSchema.PK, originTranshipmentChargeCodePk));

			AccountingUpgradeTask testTask = new AccountingUpgradeTask();
			testTask.Run();

			// Assert results

			// OrgCreditorGroup
			AssertEquals("New OrgCreditorGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgCreditorGroupSchema.PK, newCreditorGroupPk));
			AssertEquals("Update dbo.OrgCreditorGroup name not changed back", "Some~Desc~Not~To~Be~Changed", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, OrgCreditorGroupSchema.PK, intercompanyCreditorGroupPk, OrgCreditorGroupSchema.OG_Desc));

			// OrgDebtorGroup
			AssertEquals("New OrgDebtorGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgDebtorGroupSchema.PK, newDebtorGroupPk));
			AssertEquals("Update dbo.OrgDebtorGroup name not changed back", "Some~Desc~Not~To~Be~Changed", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, OrgDebtorGroupSchema.PK, intercompanyDebtorGroupPk, OrgDebtorGroupSchema.OJ_Desc));
			AssertEquals("Old TPY OrgDebtorGroup in database?", false, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgDebtorGroupSchema.PK, thirdPartyCompanyDebtorGroupPkOld));
			AssertEquals("New Third Party Company OrgDebtorGroup code:", "TPY", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, OrgDebtorGroupSchema.PK, thirdPartyCompanyDebtorGroupPkNew, OrgDebtorGroupSchema.OJ_Code));

			// AccGLHeader
			AssertEquals("New AccGLHeader in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccGLHeaderSchema.PK, newGLHeaderPk));
			AssertEquals("Update dbo.AccGLHeader name not changed back", "Some~Desc~Not~To~Be~Changed", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, AccGLHeaderSchema.PK, dutyGLHeaderPk, AccGLHeaderSchema.AG_Description));

			// AccGroups
			AssertEquals("New AccGroups in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccGroupsSchema.PK, newAccGroupPk));
			AssertEquals("Update dbo.AccGroups name not changed back", "Some~Desc~Not~To~Be~Changed", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, AccGroupsSchema.PK, staffAccGroupPk, AccGroupsSchema.AR_Desc));

			// AccTaxRate
			AssertEquals("New AccTaxRate in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccTaxRateSchema.PK, newTaxRatePk));
			AssertEquals("Update dbo.AccTaxRate name not changed back", "Some~Desc~Not~To~Be~Changed", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, AccTaxRateSchema.PK, gstTaxRatePk, AccTaxRateSchema.AT_Description));

			// AccChargeCode
			AssertEquals("New AccChargeCode in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccChargeCodeSchema.PK, newChargeCodePk));
			AssertEquals("Update dbo.AccChargeCode name not changed back", "Some~Desc~Not~To~Be~Changed", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, AccChargeCodeSchema.PK, originTranshipmentChargeCodePk, AccChargeCodeSchema.AC_Desc));
		}

		public void TestChargeCodeReferences()
		{
			Guid chargeCodePk = new Guid("05a86011-f654-4ec8-9d85-7c836e77f18b");
			TestConnection.ExecuteNonQuery("delete from dbo.JobCharge");
			TestConnection.ExecuteNonQuery("delete from dbo.AccChargeCode");
			TestConnection.ExecuteNonQuery("delete from dbo.AccGLHeader");
			TestConnection.ExecuteNonQuery("delete from dbo.AccGroups");
			TestConnection.ExecuteNonQuery("delete from dbo.AccTaxRate");
			AssertEquals("AccChargeCode", false, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccChargeCodeSchema.PK, chargeCodePk));
			new AccountingUpgradeTask().Run();
			AssertEquals("AccChargeCode", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccChargeCodeSchema.PK, chargeCodePk));
			AssertEquals("AccChargeCode.AC_AG_RevenueAccount", new Guid("584f9470-d8b0-440e-b3e5-98d93d569766"), BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, AccChargeCodeSchema.PK, chargeCodePk, AccChargeCodeSchema.AC_AG_RevenueAccount));
			AssertEquals("AccChargeCode.AC_AG_WIPAccount", new Guid("6f989720-2b96-466e-a071-91ea9eded6ef"), BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, AccChargeCodeSchema.PK, chargeCodePk, AccChargeCodeSchema.AC_AG_WIPAccount));
			AssertEquals("AccChargeCode.AC_AG_CostAccount", new Guid("2841463b-1b5d-42d6-a0ab-283d31b1f0de"), BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, AccChargeCodeSchema.PK, chargeCodePk, AccChargeCodeSchema.AC_AG_CostAccount));
			AssertEquals("AccChargeCode.AC_AG_AccrualAccount", new Guid("2d1bcf3e-4577-4c4a-bb5a-98042eecaad3"), BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, AccChargeCodeSchema.PK, chargeCodePk, AccChargeCodeSchema.AC_AG_AccrualAccount));
			AssertEquals("AccChargeCode.AC_AR_SalesGroup", new Guid("be564819-007e-475e-9fa2-7358ab971f0d"), BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, AccChargeCodeSchema.PK, chargeCodePk, AccChargeCodeSchema.AC_AR_SalesGroup));
			AssertEquals("AccChargeCode.AC_AR_ExpenseGroup", new Guid("be564819-007e-475e-9fa2-7358ab971f0d"), BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, AccChargeCodeSchema.PK, chargeCodePk, AccChargeCodeSchema.AC_AR_ExpenseGroup));
			AssertEquals("AccChargeCode.AC_AT_GSTRate", new Guid("ef810ab9-7a97-44ff-b277-826a2fdddb86"), BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, AccChargeCodeSchema.PK, chargeCodePk, AccChargeCodeSchema.AC_AT_GSTRate));
		}

		public void TestSameAccGroupAddedByUserBeforeCargoWise()
		{
			Guid userTransportAccGroupsPk = new Guid();

			string sqlText = String.Format(@"
                DECLARE @pk UNIQUEIDENTIFIER;
				SELECT @pk = AR_PK FROM dbo.AccGroups WHERE AR_Code = 'TRANSPORT';
				IF (@pk IS NULL) INSERT dbo.AccGroups (AR_PK, AR_Code, AR_Desc) VALUES ('{0}', 'TRANSPORT', 'TRANSPORT');
				SET @pk = ISNULL(@pk, '{0}');
				SELECT @pk AS AR_PK;",
				userTransportAccGroupsPk);

			userTransportAccGroupsPk = (Guid)TestConnection.ExecuteScalar(sqlText);

			AccountingUpgradeTask testTask = new AccountingUpgradeTask();
			testTask.Run();

			AssertEquals("User created Transport AccGroups in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, AccGroupsSchema.PK, userTransportAccGroupsPk));
			AssertEquals("User created Transport AccGroups in database", "TRANSPORT", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, AccGroupsSchema.PK, userTransportAccGroupsPk, AccGroupsSchema.AR_Code));
		}
	}
}
