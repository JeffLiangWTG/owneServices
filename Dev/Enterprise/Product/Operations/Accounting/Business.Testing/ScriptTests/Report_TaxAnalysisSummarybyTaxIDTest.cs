using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using TaxTransactionAnalysisReportTestHelper = Enterprise.Accounting.Business.Testing.ScriptTests.Report_TaxTransactionAnalysisbyTransLineTest.TaxTransactionAnalysisReportTestHelper;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_TaxAnalysisSummarybyTaxIDTest : ScriptTest
	{
		public void TestLineTaxBranch()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var taxBranch = TestObjectCreator.CreateNewBranch(company, "TST");
			Factory.Save();

			var invoice = TestHelper.SetupInvoice("001", LedgerTypes.AccountsPayable, TestObjectCreator.GST1.PK, 300M, 100M, isSaveWithFactory: false);
			invoice.AH_GB_TaxBranch = taxBranch.PK;
			TestHelper.SetupInvoice("002", LedgerTypes.AccountsReceivable, TestObjectCreator.GST2.PK, 240M, 0M, isSaveWithFactory: false);
			TestHelper.SetupInvoice("003", LedgerTypes.AccountsPayable, TestObjectCreator.GST1.PK, 140M, 100M, isSaveWithFactory: false);
			Factory.Save();

			invoice = TestHelper.SetupInvoiceWithCashVATLine(LedgerTypes.AccountsReceivable, TestObjectCreator.GST2.PK, 300M, 100M, true, isSaveWithFactory: false);
			invoice.AH_GB_TaxBranch = taxBranch.PK;
			TestHelper.SetupInvoiceWithCashVATLine(LedgerTypes.AccountsPayable, TestObjectCreator.GST1.PK, 200M, 50M, true, transactionNumber: "011", isSaveWithFactory: false);
			TestHelper.SetupInvoiceWithCashVATLine(LedgerTypes.AccountsReceivable, TestObjectCreator.GST1.PK, 100M, 20M, true, isSaveWithFactory: false);
			Factory.Save();

			AssertEquals("Pre-conditoin", "BNE", GlbBranch.CurrentBranch.GB_Code);

			var headers = new[] { "TaxID", "AH_Ledger", "LocalAmount", "TAXAmount" };
			var lines = new[]
			{
				new object[] { "ZZGST1", "AP", -250m, -25m },
				new object[] { "ZZGST2", "AR", 83.33m, 16.67m }
			};

			var result = RunScript(lineTaxBranchList: taxBranch.PK.ToString());
			AssertDataTableAllRows("Line Tax Branch", result, headers, lines);

			lines = new[]
			{
				new object[] { "ZZGST1", "AP", -250m, -25m }
			};

			result = RunScript(taxID: TestObjectCreator.GST1.AT_Code, lineTaxBranchList: taxBranch.PK.ToString());
			AssertDataTableAllRows("Line Tax Branch", result, headers, lines);
		}

		DataTable RunScript(string taxID = "", string lineBranchList = "", string lineTaxBranchList = "")
		{
			var loginCompany = GlbCompany.CurrentCompany;
			string currentCountry = loginCompany.GC_RN_NKCountryCode;
			string currentCountryTaxRegistrationOrgCusCode = Country.GetConsumptionTaxRegistrationOrgCusCode(currentCountry);
			var reportDate = ZDateTime.Today;

			return DataUtils.GetDataTableFromQuery(Db.Connection, $@"
SELECT * 
FROM Report_TaxAnalysisSummarybyTaxID(
'{currentCountry}',	--@CurrentCountry
'{loginCompany.PK}',	--@CompanyPK
NULL,	--@Period
'{reportDate.AddDays(-1).ToISO8601String()}',	--@StartDate
'{reportDate.AddDays(1).ToISO8601String()}',	--@EndDate
'',		--@InOutPut
'ALL',	--@TransactionLedger
'{taxID}',		--@TaxID
'',		--@TaxMsg
'{lineBranchList}',		--@BranchList
'{lineTaxBranchList}',	--@TaxBranchList
'{currentCountryTaxRegistrationOrgCusCode}'	--@CurrentCountryTaxRegistrationOrgCusCode
)");
		}

		TaxTransactionAnalysisReportTestHelper TestHelper => taxTransactionReportTestHelper ?? (taxTransactionReportTestHelper = new TaxTransactionAnalysisReportTestHelper(TestObjectCreator));
		TaxTransactionAnalysisReportTestHelper taxTransactionReportTestHelper;
	}
}
