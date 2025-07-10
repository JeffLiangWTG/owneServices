using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using TaxTransactionAnalysisReportTestHelper = Enterprise.Accounting.Business.Testing.ScriptTests.Report_TaxTransactionAnalysisbyTransLineTest.TaxTransactionAnalysisReportTestHelper;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_TaxAnalysisSummarybyTaxMsgTest : ScriptTest
	{
		public void TestLineTaxBranch()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var taxBranch = TestObjectCreator.CreateNewBranch(company, "TST");
			var taxMsg1 = TestObjectCreator.TaxMsg1;
			var taxMsg2 = TestObjectCreator.TaxMsg2;
			Factory.Save();

			var invoice1 = TestHelper.SetupInvoice("001", LedgerTypes.AccountsPayable, TestObjectCreator.GST1.PK, 300M, 100M, isSaveWithFactory: false);
			invoice1.AH_GB_TaxBranch = taxBranch.PK;
			invoice1.Lines.OfType<InvoicingLineBase>().Single(x => x.AL_LineAmount == -150m).AL_A9_VATClass = taxMsg1.PK;
			invoice1.Lines.OfType<InvoicingLineBase>().Single(x => x.AL_LineAmount == -100m).AL_A9_VATClass = taxMsg2.PK;
			TestHelper.SetupInvoice("002", LedgerTypes.AccountsReceivable, TestObjectCreator.GST2.PK, 240M, 0M, isSaveWithFactory: false);
			TestHelper.SetupInvoice("003", LedgerTypes.AccountsPayable, TestObjectCreator.GST1.PK, 140M, 100M, isSaveWithFactory: false);
			Factory.Save();

			var invoice2 = TestHelper.SetupInvoiceWithCashVATLine(LedgerTypes.AccountsReceivable, TestObjectCreator.GST2.PK, 300M, 100M, true, isSaveWithFactory: false);
			invoice2.AH_GB_TaxBranch = taxBranch.PK;
			invoice2.Lines[0].AL_A9_VATClass = taxMsg1.PK;
			TestHelper.SetupInvoiceWithCashVATLine(LedgerTypes.AccountsPayable, TestObjectCreator.GST1.PK, 200M, 50M, true, transactionNumber: "011", isSaveWithFactory: false);
			TestHelper.SetupInvoiceWithCashVATLine(LedgerTypes.AccountsReceivable, TestObjectCreator.GST1.PK, 100M, 20M, true, isSaveWithFactory: false);
			Factory.Save();

			AssertEquals("Pre-conditoin", "BNE", GlbBranch.CurrentBranch.GB_Code);

			var headers = new[] { "TaxID", "TaxMsgCode", "AH_Ledger", "LocalAmount", "TAXAmount" };
			var lines = new[]
			{
				new object[] { "ZZGST1", "MSG1", "AP", -150m, -15m },	// invoice-1 line-1
				new object[] { "ZZGST1", "MSG2", "AP", -100m, -10m },	// invoice-1 line-2
				new object[] { "ZZGST2", "MSG1", "AR", 83.33m, 16.67m }	// invoice-2 line-1
			};

			var result = RunScript(lineTaxBranchList: taxBranch.PK.ToString());
			AssertDataTableAllRows("Line Tax Branch", result, headers, lines);
		}

		DataTable RunScript(string lineBranchList = "", string lineTaxBranchList = "")
		{
			var loginCompany = GlbCompany.CurrentCompany;
			string currentCountry = loginCompany.GC_RN_NKCountryCode;
			string currentCountryTaxRegistrationOrgCusCode = Country.GetConsumptionTaxRegistrationOrgCusCode(currentCountry);
			var reportDate = ZDateTime.Today;

			return DataUtils.GetDataTableFromQuery(Db.Connection, $@"
SELECT * 
FROM Report_TaxAnalysisSummarybyTaxMsg(
'{currentCountry}',	--@CurrentCountry
'{loginCompany.PK}',	--@CompanyPK
NULL,	--@Period
'{reportDate.AddDays(-1).ToISO8601String()}',	--@StartDate
'{reportDate.AddDays(1).ToISO8601String()}',	--@EndDate
'',		--@InOutPut
'ALL',	--@TransactionLedger
'',		--@TaxID
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
