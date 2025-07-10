using System.Collections.Generic;
using System.Linq;

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA.Testing
{
	[TestedType(typeof(LIQSubmissionData))]
	public class LIQSubmissionDataTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new LIQSubmissionData(Factory);
		}

		AccComplianceReport Report => report ?? (report = Factory.New<AccComplianceReport>());
		LIQSubmissionData TestLIQSubmissionData => testLIQSubmissionData ?? (testLIQSubmissionData = new LIQSubmissionData(Factory));
		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		LIQSubmissionDataHelper Helper => helper ?? (helper = new LIQSubmissionDataHelper(report));

		AccComplianceReport report;
		LIQSubmissionData testLIQSubmissionData;
		TestObjectCreator creator;
		LIQSubmissionDataHelper helper;

		public void TestCollectDataForInvoice()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy);

			Creator.CC1.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;
			Creator.CC2.AC_GoodsServiceType = GoodServiceTypes.Codes.SRV;

			Report.FillWithValidTestData();
			Report.ACR_ReportType = "LIQ";
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "TXR");

			var creditor = Creator.ABIGAS;
			creditor.OH_RL_NKClosestPort = "DEJOH";
			var apInvoice = Creator.CreateAPInvoice<APInvoice>("I0001" + Report.ReportLines.Count.ToString(), Creator.EUR, 1m, 10m, 10m, 0m, 10m, 10m, 0m, creditor);
			Assert("Has Lines", apInvoice.Lines.Count > 0);

			var arInvoice = Creator.CreateARInvoice<ARInvoice>("R0001" + Report.ReportLines.Count.ToString(), Creator.EUR, 1m, Creator.ABIGAS);

			var job = Creator.CreateJob("123456", Creator.LocalClient, 1m, Creator.Agent, 1m);

			var invoiceLinesGDS = new List<InvoicingLineBase>();
			var invoiceLinesSRV = new List<InvoicingLineBase>();
			var invoiceLinesAR = new List<InvoicingLineBase>();

			for (int i = 0; i < 15; i++)
			{
				invoiceLinesGDS.Add(Creator.CreateAPInvoiceLine(apInvoice, job, Creator.CC1, Creator.EUR, 1m, "", 10m));
				invoiceLinesSRV.Add(Creator.CreateAPInvoiceLine(apInvoice, job, Creator.CC2, Creator.EUR, 1m, "", 10m));
				invoiceLinesAR.Add(Creator.CreateARInvoiceLine(arInvoice, job, Creator.CC2, Creator.EUR, 1m, "", 15m));
			}
			invoiceLinesAR.Add(Creator.CreateInvoiceLine(arInvoice, job, Creator.CC2, 22m, Creator.EUR, 1m));

			Assert("Has Lines", arInvoice.Lines.Count > 0);

			LIQSubmissionDataTestHelper.CreateTaxRates(invoiceLinesGDS, Creator);
			LIQSubmissionDataTestHelper.CreateTaxRates(invoiceLinesSRV, Creator);
			LIQSubmissionDataTestHelper.CreateTaxRates(invoiceLinesAR, Creator);

			var allInvoices = invoiceLinesGDS;
			allInvoices.AddRange(invoiceLinesSRV);
			allInvoices.AddRange(invoiceLinesAR);
			allInvoices.ForEach(x => Creator.CreateCharge(x));

			Factory.Save();

			Report.GenerateFromQueue();
			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 19, Report.ReportLines.Count);

			VerifyInvoice(apInvoice, Report.ReportLines);

			VerifyInvoice(arInvoice, Report.ReportLines);

			Helper.SetValues(TestLIQSubmissionData);

			AssertEquals(387m, TestLIQSubmissionData.Box1_TotalVatBaseReceivables);
			AssertEquals(25.2m, TestLIQSubmissionData.Box2_TotalVatReceivables);
			AssertEquals(-310m, TestLIQSubmissionData.Box3_TotalVatBasePayables);
			AssertEquals(-36m, TestLIQSubmissionData.Box4_TotalVatPayablesRecoverable);
			AssertEquals(0m, TestLIQSubmissionData.Box5_TotalVatPayablesNotRecoverable);
			AssertEquals(-10.8m, TestLIQSubmissionData.Box6_VatBalanceReceivablesAndPayables);
			AssertEquals(0m, TestLIQSubmissionData.Box7_BalancePreviousPeriod);
			AssertEquals(-10.8m, TestLIQSubmissionData.Box8_TotalBalance);
		}

		public static void VerifyInvoice(Invoice invoice, AccComplianceReportLineCollectionBase<AccComplianceReportLine> reportLines)
		{
			var invoiceRAT = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "ZZRAT");
			AssertNotNull("Invoice.RAT", invoiceRAT);
			var invoiceRVS = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "ZZRVS");
			AssertNotNull("Invoice.RVS", invoiceRVS);
			var invoiceEXT = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "ZZEXT");
			AssertNotNull("Invoice.EXT", invoiceEXT);
			var invoiceCAP = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "ZZCAP");
			AssertNotNull("Invoice.CAP", invoiceCAP);
			var invoiceNOT = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "ZZNOT");
			AssertNotNull("Invoice.NOT", invoiceNOT);
			var invoiceSUS = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "ZZSUS");
			AssertNotNull("Invoice.SUS", invoiceSUS);
			var invoiceBST = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "ZZBST");
			AssertNotNull("Invoice.BST", invoiceBST);
			var invoiceEXL = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "ZZEXL");
			AssertNotNull("Invoice.EXL", invoiceEXL);
			var invoiceNoTax = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "");
			AssertNotNull("Invoice.NoTax", invoiceNoTax);
		}
	}
}
