
using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ARProfileTest : ScriptTest
	{
		public void TestARProfile()
		{
			// This test doesn't use a TestDate attribute because we are testing date logic within SQL Server for temporary credit limit, and
			// setting that attribute doesn't change the date in SQL Server.

			var org1 = TestObjectCreator.ABIGAS;
			var org2 = TestObjectCreator.LocalClient;
			var debtorGroupPK = TestObjectCreator.CreateDebtorGroup().PK;

			org1.OH_IsActive = true;
			org1.OH_IsTempAccount = false;
			org1.CompanyData.OB_OJ_ARDebtorGroup = debtorGroupPK;
			org1.ARSettlementGroupPK = org2.PK;
			org1.CompanyData.OB_ARCategory = "STD";
			org1.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";
			org1.CompanyData.OverrideBankAccountFromDebtorGroup = true;
			org1.CompanyData.ARBankAccountToDisplay = TestObjectCreator.AUDBankAccount.PK;
			org1.CompanyData.OB_RX_NKARDDefltCurrency = TestObjectCreator.USD.RX_Code;
			org1.CompanyData.SetARTaxApplicable(true);
			org1.CompanyData.OB_ARWHTApplicable = true;
			org1.CompanyData.OB_ARDontShowTaxOnDocs = true;
			org1.CompanyData.OB_ARAllowMultiCurrencyPayment = true;
			org1.CompanyData.OB_ARExternalDebtorCode = "EXORG1";
			org1.CompanyData.OB_ARQualityAssured = true;
			org1.CompanyData.OB_ARQualityAssuredCheckedDate = new ZDateTime(2011, 05, 01);
			org1.CompanyData.OB_ARCreditLimit = 0m;
			org1.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			org1.CompanyData.OB_ARCreditApproved = true;
			org1.CompanyData.OB_AROnCreditHold = true;
			org1.CompanyData.OB_ARAccountAndCreditReviewDue = new ZDateTime(2011, 05, 05);
			org1.CompanyData.OB_ARBuyersConsolInvoicingStyle = "DEF";

			var org1Term1 = org1.CompanyData.ARTerms[0];
			org1Term1.PY_InvoiceClass = "ALL";
			org1Term1.PY_InvoiceTerm = "DEF";
			org1Term1.PY_InvoiceDays = 5;

			org1.CompanyData.OB_ARTreatDisbursementsAsStandardValue = 10.5M;
			org1.CompanyData.OB_ARReceiptInvoiceAfterPostingDefault = true;
			org1.CompanyData.OB_ARIncludeInwardsWhsConsolidatedInvoice = true;
			org1.CompanyData.OB_ARIncludeOutwardsWhsConsolidatedInvoice = true;
			org1.CompanyData.OB_ARWhsStorageCalcMethod = "XYZ";

			org1.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 1.1m, 1.2m);
			org1.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 2.1m, 2.2m);
			org1.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, 3.1m, 3.2m);
			org1.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, 4.1m, 4.2m);

			var org2MiscServ = org2.MiscServ;
			org2MiscServ.OM_ARGlobalCreditApproved = true;
			org2MiscServ.OM_ARGlobalCreditLimit = 10000M;
			org2MiscServ.OM_ARGlobalOnCreditHold = true;
			org2MiscServ.OM_RX_NKARGlobalCreditCurrency = "USD";

			var org1MiscServ = org1.MiscServ;
			org1MiscServ.OM_OH_ARGlobalCreditGroup = org2.PK;

			org2.OH_IsActive = true;
			org2.OH_IsTempAccount = true;
			org2.CompanyData.OB_OJ_ARDebtorGroup = debtorGroupPK;
			org2.ARSettlementGroupPK = org2.PK;
			org2.CompanyData.OB_ARCategory = "KEY";
			org2.CompanyData.OB_ARConsolidatedAccountingCategory = "WHO";
			org2.CompanyData.OverrideBankAccountFromDebtorGroup = true;
			org2.CompanyData.ARBankAccountToDisplay = TestObjectCreator.AUDBankAccount2.PK;
			org2.CompanyData.OB_RX_NKARDDefltCurrency = TestObjectCreator.USD.RX_Code;
			org2.CompanyData.SetARTaxApplicable(false);
			org2.CompanyData.OB_ARWHTApplicable = false;
			org2.CompanyData.OB_ARDontShowTaxOnDocs = false;
			org2.CompanyData.OB_ARAllowMultiCurrencyPayment = false;
			org2.CompanyData.OB_ARExternalDebtorCode = "EXORG2";
			org2.CompanyData.OB_ARQualityAssured = false;
			org2.CompanyData.OB_ARQualityAssuredCheckedDate = new ZDateTime(2011, 05, 02);
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(org2.CompanyData, 2000m, 200m);
			org2.CompanyData.OB_ARUseSettlementGroupCreditLimit = false;
			org2.CompanyData.OB_ARCreditApproved = true;
			org2.CompanyData.OB_AROnCreditHold = false;
			org2.CompanyData.OB_ARAccountAndCreditReviewDue = new ZDateTime(2011, 05, 05);
			org2.CompanyData.OB_ARBuyersConsolInvoicingStyle = "MAS";

			var org2Term1 = org2.CompanyData.ARTerms[0];
			org2Term1.PY_InvoiceClass = "DSB";
			org2Term1.PY_InvoiceTerm = "DEF";
			org2Term1.PY_InvoiceDays = 15;

			org2.CompanyData.OB_ARTreatDisbursementsAsStandardValue = 15.5M;
			org2.CompanyData.OB_ARReceiptInvoiceAfterPostingDefault = false;
			org2.CompanyData.OB_ARIncludeInwardsWhsConsolidatedInvoice = false;
			org2.CompanyData.OB_ARIncludeOutwardsWhsConsolidatedInvoice = false;
			org2.CompanyData.OB_ARWhsStorageCalcMethod = "TES";

			Factory.Save();

			DataTable resultOrgs = RunScript(debtorGroupPK);
			AssertEquals("Result Rows", 2, resultOrgs.Rows.Count);

			var headers = new[] {	 "AccountCode", "AccountName", "AccountPK", "IsTemporary", "AccountsRelationship", "ConsolidationCategory",
									"CurrencyPK", "CurrencyCode", "CreditLimit", "TemporaryCreditLimitIncrease", "TemporaryCreditLimitIncreaseExpiry", "AdjustedCreditLimit", "OnCreditHold", "ExDebtorCode", "PayToAccount",
									"TreatDisbursementsAsStandardValue", "Quality", "Date", "CreditReview",
									"DontShowTaxOnDocs", "GST", "WHT", "AllowMultiCurrencyPayment", "BuyersConsolInvoicingStyle", "CreditApproved",
									"GlobalCreditGroup", "GlobalCreditCurrency", "GlobalCreditLimit", "GlobalCreditApproved", "GlobalOnCreditHold"
						};

			DataRow[] exportedRows = resultOrgs.Select("AccountCode = 'ABIGAS      '");
			AssertEquals("exportedRows.Count", 1, exportedRows.Length);
			AssertDataRow(exportedRows[0], headers, new object[] {	"ABIGAS      ", TestObjectCreator.ABIGAS.OH_FullName, TestObjectCreator.ABIGAS.PK, "N", "STD", "UNR",
											TestObjectCreator.USD.PK, TestObjectCreator.USD.RX_Code, 0M, 0M, DBNull.Value, 0M, true, "EXORG1", TestObjectCreator.AUDBankAccount.AB_Code,
											10.5M, true, new ZDateTime(2011, 05, 01), new ZDateTime(2011, 05, 05),
											true, "Y", true, true, "DEF", true, "ZLOCCLT", DBNull.Value, 0M, "Y", "Y" });

			exportedRows = resultOrgs.Select(string.Format("AccountCode = '{0}'", TestObjectCreator.LocalClient.OH_Code));
			AssertEquals("exportedRows.Count", 1, exportedRows.Length);
			AssertDataRow(exportedRows[0], headers, new object[] {	TestObjectCreator.LocalClient.OH_Code, TestObjectCreator.LocalClient.OH_FullName, TestObjectCreator.LocalClient.PK, "Y", "KEY", "WHO",
											TestObjectCreator.USD.PK, TestObjectCreator.USD.RX_Code, 2000M, 200M, org2.CompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry, 2200M, false, "EXORG2", TestObjectCreator.AUDBankAccount2.AB_Code,
											15.5M, false, new ZDateTime(2011, 05, 02), new ZDateTime(2011, 05, 05),
											false, "N", false, false, "MAS", true, DBNull.Value, "USD", 10000M, "Y", "Y" });
		}

		DataTable RunScript(ZGuid debtorGroupPK)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
EXEC Report_ARProfile
'{0}',	--@Company
'{1}',		--@AccountTypeActive
'',		--@IsTemporary
'',		--@Debtors
'{2}',	--@DebtorGroupPK
NULL,		--@BranchPK
NULL,	--@UNLOCO
NULL,	--@CountryCode
NULL,	--@SalesID
''		--@StaffRole
",
			GlbCompany.CurrentCompany.PK,
			"Active",
			debtorGroupPK
			));
		}
	}
}


