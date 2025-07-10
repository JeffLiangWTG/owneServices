using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.Accounting.Business.CashBook;
	using Enterprise.Accounting.Business.ComplianceReport;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;
	using TaxTypes = MasterFiles.Business.AccTaxRate.Types;

	[TestedType(typeof(MTDSubmissionData))]
	public class MTDSubmissionDataTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new MTDSubmissionData(Factory);
		}

		public void TestCollectDataForAPInvoice()
		{
			Creator.CC1.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;
			Creator.CC2.AC_GoodsServiceType = GoodServiceTypes.Codes.SRV;

			Report.ACR_ReportType = "MTD";
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "TXR");
			var shipment = Creator.CreateShipment("S001");
			var job = Creator.CreateJob(shipment);

			var apInvoice = Creator.CreateAPInvoice<APInvoice>("I0001" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 10m, 10m, 0m, 10m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", apInvoice.Lines.Count > 0);

			var invoiceLinesGDS = new List<InvoicingLineBase>();
			var invoiceLinesSRV = new List<InvoicingLineBase>();
			for (int i = 0; i < 12; i++)
			{
				invoiceLinesGDS.Add(Creator.CreateInvoiceLine(apInvoice, job, Creator.CC1, 10m, Creator.AUD, 1m));
				invoiceLinesSRV.Add(Creator.CreateInvoiceLine(apInvoice, job, Creator.CC2, 10m, Creator.AUD, 1m));
			}

			CreateTaxRates(invoiceLinesGDS, Creator);
			CreateTaxRates(invoiceLinesSRV, Creator);

			var allInvoices = invoiceLinesGDS;
			allInvoices.AddRange(invoiceLinesSRV);
			allInvoices.ForEach(x => Creator.CreateCharge(x));

			Factory.Save();

			Report.GenerateFromQueue();
			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 12, Report.ReportLines.Count);

			VerifyInvoice(apInvoice, Report.ReportLines);

			Helper.SetValues(TestMTDSubmissionData);

			AssertEquals(0m, TestMTDSubmissionData.Box1_VATDue);
			AssertEquals(2m, TestMTDSubmissionData.Box2_VATDueReverseChg);
			AssertEquals(2m, TestMTDSubmissionData.Box3_TotalVATDue);
			AssertEquals(26m, TestMTDSubmissionData.Box4_VATReclaimed);
			AssertEquals(24m, TestMTDSubmissionData.Box5_NetVAT);
			AssertEquals(20m, TestMTDSubmissionData.Box6_TotalSalesExVAT);
			AssertEquals(110m, TestMTDSubmissionData.Box7_TotalPurchaseExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box9_GoodsPurchaseECMembersExVAT);
		}

		public void TestCollectData_CreditorWithInvalidClosestPort()
		{
			Creator.CC1.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;
			Creator.CC2.AC_GoodsServiceType = GoodServiceTypes.Codes.SRV;

			Report.ACR_ReportType = "MTD";
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "TXR");
			var shipment = Creator.CreateShipment("S001");
			var job = Creator.CreateJob(shipment);

			var apInvoice = Creator.CreateAPInvoice<APInvoice>("I0001" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 10m, 10m, 0m, 10m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", apInvoice.Lines.Count > 0);

			var invoiceLinesGDS = new List<InvoicingLineBase>();
			var invoiceLinesSRV = new List<InvoicingLineBase>();
			for (int i = 0; i < 12; i++)
			{
				invoiceLinesGDS.Add(Creator.CreateInvoiceLine(apInvoice, job, Creator.CC1, 10m, Creator.AUD, 1m));
				invoiceLinesSRV.Add(Creator.CreateInvoiceLine(apInvoice, job, Creator.CC2, 10m, Creator.AUD, 1m));
			}

			CreateTaxRates(invoiceLinesGDS, Creator);
			CreateTaxRates(invoiceLinesSRV, Creator);

			var allInvoices = invoiceLinesGDS;
			allInvoices.AddRange(invoiceLinesSRV);
			allInvoices.ForEach(x => Creator.CreateCharge(x));

			Factory.Save();

			Report.GenerateFromQueue();
			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 12, Report.ReportLines.Count);

			VerifyInvoice(apInvoice, Report.ReportLines);

			Creator.ABIGAS.OH_RL_NKClosestPort = "";
			AssertNoExceptionThrown(() => Helper.SetValues(TestMTDSubmissionData));

			AssertEquals(0m, TestMTDSubmissionData.Box1_VATDue);
			AssertEquals(2m, TestMTDSubmissionData.Box2_VATDueReverseChg);
			AssertEquals(2m, TestMTDSubmissionData.Box3_TotalVATDue);
			AssertEquals(26m, TestMTDSubmissionData.Box4_VATReclaimed);
			AssertEquals(24m, TestMTDSubmissionData.Box5_NetVAT);
			AssertEquals(20m, TestMTDSubmissionData.Box6_TotalSalesExVAT);
			AssertEquals(110m, TestMTDSubmissionData.Box7_TotalPurchaseExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box9_GoodsPurchaseECMembersExVAT);
		}

		public void TestCollectDataForARInvoice()
		{
			Report.ACR_ReportType = "MTD";
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "TXR");

			var arInvoice = Creator.CreateARInvoice<ARInvoice>("I0001" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, Creator.ABIGAS);
			var line1 = Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 10m, 10m, 0m);
			var line2 = Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 10m, 10m, 0m);
			var line3 = Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 10m, 10m, 0m);
			var line4 = Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 10m, 10m, 0m);
			var line5 = Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 10m, 10m, 0m);
			var line6 = Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 10m, 10m, 0m);
			var line7 = Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 10m, 10m, 0m);
			var line8 = Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 10m, 10m, 0m);
			var line9 = Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 10m, 10m, 0m);
			var line10 = Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 10m, 10m, 0m);
			var line11 = Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 10m, 10m, 0m);
			var line12 = Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 10m, 10m, 0m);

			var invoiceLines = new List<InvoicingLineBase> { line1, line2, line3, line4, line5, line6, line7, line8, line9, line10, line11, line12 };
			CreateTaxRates(invoiceLines, Creator);

			Factory.Save();

			Report.GenerateFromQueue();
			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 12, Report.ReportLines.Count);

			VerifyInvoice(arInvoice, Report.ReportLines);

			Helper.SetValues(TestMTDSubmissionData);

			AssertEquals(2m, TestMTDSubmissionData.Box1_VATDue);
			AssertEquals(0m, TestMTDSubmissionData.Box2_VATDueReverseChg);
			AssertEquals(2m, TestMTDSubmissionData.Box3_TotalVATDue);
			AssertEquals(0m, TestMTDSubmissionData.Box4_VATReclaimed);
			AssertEquals(2m, TestMTDSubmissionData.Box5_NetVAT);
			AssertEquals(50m, TestMTDSubmissionData.Box6_TotalSalesExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box7_TotalPurchaseExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box9_GoodsPurchaseECMembersExVAT);
		}

		#region EUR Tests
		public void TestCollectDataForAPInvoiceEUR()
		{
			Creator.CC1.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;
			Creator.CC2.AC_GoodsServiceType = GoodServiceTypes.Codes.SRV;

			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "TXR");

			var creditor = Creator.ABIGAS;
			creditor.OH_RL_NKClosestPort = "DEJOH";
			var apInvoice = Creator.CreateAPInvoice<APInvoice>("I0001" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 10m, 10m, 0m, 10m, 10m, 0m, creditor);
			Assert("Has Lines", apInvoice.Lines.Count > 0);
			var job = Creator.CreateJob("123456", Creator.LocalClient, 1m, Creator.Agent, 1m);

			var invoiceLinesGDS = new List<InvoicingLineBase>();
			var invoiceLinesSRV = new List<InvoicingLineBase>();

			for (int i = 0; i < 12; i++)
			{
				invoiceLinesGDS.Add(Creator.CreateAPInvoiceLine(apInvoice, job, Creator.CC1, Creator.AUD, 1m, "", 10m));
				invoiceLinesSRV.Add(Creator.CreateAPInvoiceLine(apInvoice, job, Creator.CC2, Creator.AUD, 1m, "", 10m));
			}

			CreateTaxRates(invoiceLinesGDS, Creator);
			CreateTaxRates(invoiceLinesSRV, Creator);

			var allInvoices = invoiceLinesGDS;
			allInvoices.AddRange(invoiceLinesSRV);
			allInvoices.ForEach(x => Creator.CreateCharge(x));

			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, allInvoices.ToArray());
			Report.GenerateFromQueue();
			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 12, Report.ReportLines.Count);

			VerifyInvoice(apInvoice, Report.ReportLines);

			Helper.SetValues(TestMTDSubmissionData);

			AssertEquals(0m, TestMTDSubmissionData.Box1_VATDue);
			AssertEquals(2m, TestMTDSubmissionData.Box2_VATDueReverseChg);
			AssertEquals(2m, TestMTDSubmissionData.Box3_TotalVATDue);
			AssertEquals(16m, TestMTDSubmissionData.Box4_VATReclaimed);
			AssertEquals(14m, TestMTDSubmissionData.Box5_NetVAT);
			AssertEquals(20m, TestMTDSubmissionData.Box6_TotalSalesExVAT);
			AssertEquals(100m, TestMTDSubmissionData.Box7_TotalPurchaseExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(120m, TestMTDSubmissionData.Box9_GoodsPurchaseECMembersExVAT);
		}

		public void TestCollectDataForARInvoiceEUR()
		{
			Creator.CC1.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;
			Creator.CC2.AC_GoodsServiceType = GoodServiceTypes.Codes.SRV;

			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "TXR");

			var creditor = Creator.ABIGAS;
			creditor.OH_RL_NKClosestPort = "DEJOH";
			var goods = Creator.CreateChargeCode("GDS");
			goods.AC_GoodsServiceType = "GDS";
			var arInvoice = Creator.CreateARInvoice<ARInvoice>("I0001" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, creditor);
			var job = Creator.CreateJob("123456", Creator.LocalClient, 1m, Creator.Agent, 1m);

			var invoiceLinesGDS = new List<InvoicingLineBase>();
			var invoiceLinesSRV = new List<InvoicingLineBase>();

			for (int i = 0; i < 12; i++)
			{
				invoiceLinesGDS.Add(Creator.CreateARInvoiceLine(arInvoice, job, Creator.CC1, Creator.AUD, 1m, "", 10m));
				invoiceLinesSRV.Add(Creator.CreateARInvoiceLine(arInvoice, job, Creator.CC2, Creator.AUD, 1m, "", 10m));
			}

			CreateTaxRates(invoiceLinesGDS, Creator);
			CreateTaxRates(invoiceLinesSRV, Creator);

			var allInvoices = invoiceLinesGDS;
			allInvoices.AddRange(invoiceLinesSRV);
			allInvoices.ForEach(x => Creator.CreateCharge(x));

			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, allInvoices.ToArray());
			Report.GenerateFromQueue();
			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 12, Report.ReportLines.Count);

			VerifyInvoice(arInvoice, Report.ReportLines);

			Helper.SetValues(TestMTDSubmissionData);

			AssertEquals(4m, TestMTDSubmissionData.Box1_VATDue);
			AssertEquals(0m, TestMTDSubmissionData.Box2_VATDueReverseChg);
			AssertEquals(4m, TestMTDSubmissionData.Box3_TotalVATDue);
			AssertEquals(0m, TestMTDSubmissionData.Box4_VATReclaimed);
			AssertEquals(4m, TestMTDSubmissionData.Box5_NetVAT);
			AssertEquals(100m, TestMTDSubmissionData.Box6_TotalSalesExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box7_TotalPurchaseExVAT);
			AssertEquals(120m, TestMTDSubmissionData.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box9_GoodsPurchaseECMembersExVAT);
		}
		#endregion

		#region Cash Book Tests
		public void TestCollectDataForCashBookDRC()
		{
			Report.ACR_ReportType = "MTD";
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "TXR");

			var cashBook = Creator.CreateDirectReceipt(ZDateTime.Today, 10m, 10m, 10m, 10m);
			var glHeader = Creator.CreateGLHeader();
			var line1 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line2 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line3 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line4 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line5 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line6 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line7 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line8 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line9 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line10 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line11 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line12 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");

			var directTransactionLines = new List<DirectTransactionLineBase> { line1, line2, line3, line4, line5, line6, line7, line8, line9, line10, line11, line12 };
			CreateTaxRates(directTransactionLines, Creator);

			Factory.Save();

			Report.GenerateFromQueue();
			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 12, Report.ReportLines.Count);

			Helper.SetValues(TestMTDSubmissionData);

			AssertEquals(2m, TestMTDSubmissionData.Box1_VATDue);
			AssertEquals(0m, TestMTDSubmissionData.Box2_VATDueReverseChg);
			AssertEquals(2m, TestMTDSubmissionData.Box3_TotalVATDue);
			AssertEquals(0m, TestMTDSubmissionData.Box4_VATReclaimed);
			AssertEquals(2m, TestMTDSubmissionData.Box5_NetVAT);
			AssertEquals(70m, TestMTDSubmissionData.Box6_TotalSalesExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box7_TotalPurchaseExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box9_GoodsPurchaseECMembersExVAT);
		}

		public void TestCollectDataForCashBookDPY()
		{
			Report.ACR_ReportType = "MTD";
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "TXR");

			var cashBook = Creator.CreateDirectPayment(ZDateTime.Today, 10m, 10m, 10m, 10m);
			var glHeader = Creator.CreateGLHeader();
			var line1 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line2 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line3 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line4 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line5 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line6 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line7 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line8 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line9 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line10 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line11 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");
			var line12 = Creator.CreateDirectTransactionLine(cashBook, glHeader, 10m, 10m, "");

			var directTransactionLines = new List<DirectTransactionLineBase> { line1, line2, line3, line4, line5, line6, line7, line8, line9, line10, line11, line12 };
			CreateTaxRates(directTransactionLines, Creator);

			Factory.Save();

			Report.GenerateFromQueue();
			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 12, Report.ReportLines.Count);

			Helper.SetValues(TestMTDSubmissionData);

			AssertEquals(0m, TestMTDSubmissionData.Box1_VATDue);
			AssertEquals(1m, TestMTDSubmissionData.Box2_VATDueReverseChg);
			AssertEquals(1m, TestMTDSubmissionData.Box3_TotalVATDue);
			AssertEquals(37m, TestMTDSubmissionData.Box4_VATReclaimed);
			AssertEquals(36m, TestMTDSubmissionData.Box5_NetVAT);
			AssertEquals(10m, TestMTDSubmissionData.Box6_TotalSalesExVAT);
			AssertEquals(120m, TestMTDSubmissionData.Box7_TotalPurchaseExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box9_GoodsPurchaseECMembersExVAT);
		}
		#endregion

		public void TestLinesProperlySeperatedIntoCorrectColumns()
		{
			var oldReport = Factory.NewWithValidTestData<AccComplianceReport>();

			oldReport.ACR_DateFrom = new ZDate(2019, 1, 1);
			oldReport.ACR_DateTo = new ZDate(2019, 1, 31);
			oldReport.ACR_ReportType = "MTD";

			Report.ACR_DateFrom = new ZDate(2019, 2, 1);
			Report.ACR_DateTo = new ZDate(2019, 2, 28);
			Report.ACR_ReportType = "MTD";

			Creator.CreateConfigurationForComplianceReport(Report, "AL", "TXR", includeQueuedForPreviousPeriod: true);

			var apInvoice1 = Creator.CreateAPInvoice<APInvoice>("I0001" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 10m, 10m, 0m, 10m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", apInvoice1.Lines.Count > 0);
			var lineAP101 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP102 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP103 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP104 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP105 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP106 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP107 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP108 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP109 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP110 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP111 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP112 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);

			var apInvoice2 = Creator.CreateAPInvoice<APInvoice>("I0002" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 10m, 10m, 0m, 10m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", apInvoice2.Lines.Count > 0);
			var lineAP201 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP202 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP203 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP204 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP205 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP206 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP207 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP208 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP209 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP210 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP211 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP212 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);

			var testDate1 = new ZDateTime(2019, 2, 15);
			var testDate2 = new ZDateTime(2019, 1, 15);

			apInvoice1.AH_PostDate = testDate1;
			lineAP101.AL_PostDate = testDate1;
			lineAP102.AL_PostDate = testDate1;
			lineAP103.AL_PostDate = testDate1;
			lineAP104.AL_PostDate = testDate1;
			lineAP105.AL_PostDate = testDate1;
			lineAP106.AL_PostDate = testDate1;
			lineAP107.AL_PostDate = testDate1;
			lineAP108.AL_PostDate = testDate1;
			lineAP109.AL_PostDate = testDate1;
			lineAP110.AL_PostDate = testDate1;
			lineAP111.AL_PostDate = testDate1;

			apInvoice2.AH_PostDate = testDate2;
			lineAP201.AL_PostDate = testDate2;
			lineAP202.AL_PostDate = testDate2;
			lineAP203.AL_PostDate = testDate2;
			lineAP204.AL_PostDate = testDate2;
			lineAP205.AL_PostDate = testDate2;
			lineAP206.AL_PostDate = testDate2;
			lineAP207.AL_PostDate = testDate2;
			lineAP208.AL_PostDate = testDate2;
			lineAP209.AL_PostDate = testDate2;
			lineAP210.AL_PostDate = testDate2;
			lineAP211.AL_PostDate = testDate2;

			var invoiceLines = new List<InvoicingLineBase> { lineAP101, lineAP102, lineAP103, lineAP104, lineAP105, lineAP106, lineAP107, lineAP108, lineAP109, lineAP110, lineAP111, lineAP112 };
			var oldLines = new List<InvoicingLineBase> { lineAP201, lineAP202, lineAP203, lineAP204, lineAP205, lineAP206, lineAP207, lineAP208, lineAP209, lineAP210, lineAP211, lineAP212 };

			CreateTaxRates(invoiceLines, Creator);
			CreateTaxRates(oldLines, Creator);

			Factory.Save();

			Report.GenerateFromQueue();
			Report.ClearReportLines_ForTestOnly();

			AssertEquals("ReportLines.Count", 24, Report.ReportLines.Count);

			VerifyInvoice(apInvoice1, Report.ReportLines);
			VerifyInvoice(apInvoice2, Report.ReportLines);

			var secondSubData = new MTDSubmissionData(Factory);

			Helper.SetValues(TestMTDSubmissionData, secondSubData);

			AssertEquals(0m, TestMTDSubmissionData.Box1_VATDue);
			AssertEquals(1m, TestMTDSubmissionData.Box2_VATDueReverseChg);
			AssertEquals(1m, TestMTDSubmissionData.Box3_TotalVATDue);
			AssertEquals(28m, TestMTDSubmissionData.Box4_VATReclaimed);
			AssertEquals(27m, TestMTDSubmissionData.Box5_NetVAT);
			AssertEquals(10m, TestMTDSubmissionData.Box6_TotalSalesExVAT);
			AssertEquals(60m, TestMTDSubmissionData.Box7_TotalPurchaseExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box9_GoodsPurchaseECMembersExVAT);

			AssertEquals(0m, secondSubData.Box1_VATDue);
			AssertEquals(1m, secondSubData.Box2_VATDueReverseChg);
			AssertEquals(1m, secondSubData.Box3_TotalVATDue);
			AssertEquals(28m, secondSubData.Box4_VATReclaimed);
			AssertEquals(27m, secondSubData.Box5_NetVAT);
			AssertEquals(10m, secondSubData.Box6_TotalSalesExVAT);
			AssertEquals(60m, secondSubData.Box7_TotalPurchaseExVAT);
			AssertEquals(0m, secondSubData.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(0m, secondSubData.Box9_GoodsPurchaseECMembersExVAT);
		}

		public void TestLinesThatAreTooOld()
		{
			var oldReport = Factory.NewWithValidTestData<AccComplianceReport>();
			oldReport.ACR_ReportType = "MTD";

			oldReport.ACR_ReportType = "MTD";
			oldReport.ACR_DateFrom = new ZDate(2012, 2, 1);
			oldReport.ACR_DateTo = new ZDate(2012, 2, 28);

			Report.ACR_ReportType = "MTD";

			Report.ACR_DateFrom = new ZDate(2019, 2, 1);
			Report.ACR_DateTo = new ZDate(2019, 2, 28);

			Creator.CreateConfigurationForComplianceReport(Report, "AL", "TXR", includeQueuedForPreviousPeriod: true);

			var apInvoice1 = Creator.CreateAPInvoice<APInvoice>("I0001" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 10m, 10m, 0m, 10m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", apInvoice1.Lines.Count > 0);
			var lineAP101 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP102 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP103 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP104 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP105 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP106 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP107 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP108 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP109 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP110 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP111 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP112 = Creator.CreateInvoiceLine(apInvoice1, Creator.AUD, 1m, 10m, 10m, 0m);

			var apInvoice2 = Creator.CreateAPInvoice<APInvoice>("I0002" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 10m, 10m, 0m, 10m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", apInvoice2.Lines.Count > 0);
			var lineAP201 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP202 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP203 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP204 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP205 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP206 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP207 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP208 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP209 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP210 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP211 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP212 = Creator.CreateInvoiceLine(apInvoice2, Creator.AUD, 1m, 10m, 10m, 0m);

			var testDate1 = new ZDateTime(2012, 2, 15);
			var testDate2 = new ZDateTime(2019, 2, 15);

			apInvoice1.AH_PostDate = testDate1;
			lineAP101.AL_PostDate = testDate1;
			lineAP102.AL_PostDate = testDate1;
			lineAP103.AL_PostDate = testDate1;
			lineAP104.AL_PostDate = testDate1;
			lineAP105.AL_PostDate = testDate1;
			lineAP106.AL_PostDate = testDate1;
			lineAP107.AL_PostDate = testDate1;
			lineAP108.AL_PostDate = testDate1;
			lineAP109.AL_PostDate = testDate1;
			lineAP110.AL_PostDate = testDate1;
			lineAP111.AL_PostDate = testDate1;

			apInvoice2.AH_PostDate = testDate2;
			lineAP201.AL_PostDate = testDate2;
			lineAP202.AL_PostDate = testDate2;
			lineAP203.AL_PostDate = testDate2;
			lineAP204.AL_PostDate = testDate2;
			lineAP205.AL_PostDate = testDate2;
			lineAP206.AL_PostDate = testDate2;
			lineAP207.AL_PostDate = testDate2;
			lineAP208.AL_PostDate = testDate2;
			lineAP209.AL_PostDate = testDate2;
			lineAP210.AL_PostDate = testDate2;
			lineAP211.AL_PostDate = testDate2;

			var invoiceLines = new List<InvoicingLineBase> { lineAP101, lineAP102, lineAP103, lineAP104, lineAP105, lineAP106, lineAP107, lineAP108, lineAP109, lineAP110, lineAP111, lineAP112 };
			var oldLines = new List<InvoicingLineBase> { lineAP201, lineAP202, lineAP203, lineAP204, lineAP205, lineAP206, lineAP207, lineAP208, lineAP209, lineAP210, lineAP211, lineAP212 };

			CreateTaxRates(invoiceLines, Creator);
			CreateTaxRates(oldLines, Creator);
			Factory.Save();

			Report.GenerateFromQueue();
			Report.ClearReportLines_ForTestOnly();

			AssertEquals("ReportLines.Count", 24, Report.ReportLines.Count);

			VerifyInvoice(apInvoice1, Report.ReportLines);
			VerifyInvoice(apInvoice2, Report.ReportLines);

			var secondSubData = new MTDSubmissionData(Factory);

			Helper.SetValues(TestMTDSubmissionData, secondSubData);

			AssertEquals(0m, TestMTDSubmissionData.Box1_VATDue);
			AssertEquals(1m, TestMTDSubmissionData.Box2_VATDueReverseChg);
			AssertEquals(1m, TestMTDSubmissionData.Box3_TotalVATDue);
			AssertEquals(28m, TestMTDSubmissionData.Box4_VATReclaimed);
			AssertEquals(27m, TestMTDSubmissionData.Box5_NetVAT);
			AssertEquals(10m, TestMTDSubmissionData.Box6_TotalSalesExVAT);
			AssertEquals(60m, TestMTDSubmissionData.Box7_TotalPurchaseExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(0m, TestMTDSubmissionData.Box9_GoodsPurchaseECMembersExVAT);

			AssertEquals(0m, secondSubData.Box1_VATDue);
			AssertEquals(0m, secondSubData.Box2_VATDueReverseChg);
			AssertEquals(0m, secondSubData.Box3_TotalVATDue);
			AssertEquals(0m, secondSubData.Box4_VATReclaimed);
			AssertEquals(0m, secondSubData.Box5_NetVAT);
			AssertEquals(0m, secondSubData.Box6_TotalSalesExVAT);
			AssertEquals(0m, secondSubData.Box7_TotalPurchaseExVAT);
			AssertEquals(0m, secondSubData.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(0m, secondSubData.Box9_GoodsPurchaseECMembersExVAT);
		}

		void CreateTaxRates(List<InvoicingLineBase> lines, TestObjectCreator creator)
		{
			lines[0].AL_AT = creator.CreateTaxRate("RAT", "Rated", TaxTypes.Rated, 10, string.Empty, 0, 1).PK;
			lines[1].AL_AT = creator.CreateTaxRate("RVS", "ReverseRated", TaxTypes.ReverseRated, 10, string.Empty, 0, 1).PK;
			lines[2].AL_AT = creator.CreateTaxRate("EXT", "Exempt", TaxTypes.Exempt, 10, string.Empty, 0, 1).PK;
			lines[3].AL_AT = creator.CreateTaxRate("CAP", "CapitalRated", TaxTypes.CapitalRated, 10, string.Empty, 0, 1).PK;
			lines[4].AL_AT = creator.CreateTaxRate("NOT", "NotReportable", TaxTypes.NotReportable, 10, string.Empty, 0, 1).PK;
			lines[5].AL_AT = creator.CreateTaxRate("SUS", "Suspended", TaxTypes.Suspended, 10, string.Empty, 0, 1).PK;
			lines[6].AL_AT = creator.CreateTaxRate("RAX", "RatedInAnotherCountry", TaxTypes.RatedInAnotherCountry, 10, string.Empty, 0, 1).PK;
			lines[7].AL_AT = creator.CreateTaxRate("BST", "ReportableUnderBusinessTax", TaxTypes.ReportableUnderBusinessTax, 10, string.Empty, 0, 1).PK;
			lines[8].AL_AT = creator.CreateTaxRate("EXL", "ExcludedFromTheTaxBase", TaxTypes.ExcludedFromTheTaxBase, 10, string.Empty, 0, 1).PK;
			lines[9].AL_AT = creator.CreateTaxRate("INT", "IntegratedGST", TaxTypes.IntegratedGST, 10, string.Empty, 0, 1).PK;
			lines[10].AL_AT = creator.CreateTaxRate("SER", "ServiceTax", TaxTypes.ServiceTax, 10, string.Empty, 0, 1).PK;
			lines[11].AL_AT = ZGuid.Empty;
		}

		void CreateTaxRates(List<DirectTransactionLineBase> lines, TestObjectCreator creator)
		{
			lines[0].AL_AT = creator.CreateTaxRate("RAT", "Rated", TaxTypes.Rated, 10, string.Empty, 0, 1).PK;
			lines[1].AL_AT = creator.CreateTaxRate("RVS", "ReverseRated", TaxTypes.ReverseRated, 10, string.Empty, 0, 1).PK;
			lines[2].AL_AT = creator.CreateTaxRate("EXT", "Exempt", TaxTypes.Exempt, 10, string.Empty, 0, 1).PK;
			lines[3].AL_AT = creator.CreateTaxRate("CAP", "CapitalRated", TaxTypes.CapitalRated, 10, string.Empty, 0, 1).PK;
			lines[4].AL_AT = creator.CreateTaxRate("NOT", "NotReportable", TaxTypes.NotReportable, 10, string.Empty, 0, 1).PK;
			lines[5].AL_AT = creator.CreateTaxRate("SUS", "Suspended", TaxTypes.Suspended, 10, string.Empty, 0, 1).PK;
			lines[6].AL_AT = creator.CreateTaxRate("RAX", "RatedInAnotherCountry", TaxTypes.RatedInAnotherCountry, 10, string.Empty, 0, 1).PK;
			lines[7].AL_AT = creator.CreateTaxRate("BST", "ReportableUnderBusinessTax", TaxTypes.ReportableUnderBusinessTax, 10, string.Empty, 0, 1).PK;
			lines[8].AL_AT = creator.CreateTaxRate("EXL", "ExcludedFromTheTaxBase", TaxTypes.ExcludedFromTheTaxBase, 10, string.Empty, 0, 1).PK;
			lines[9].AL_AT = creator.CreateTaxRate("INT", "IntegratedGST", TaxTypes.IntegratedGST, 10, string.Empty, 0, 1).PK;
			lines[10].AL_AT = creator.CreateTaxRate("SER", "ServiceTax", TaxTypes.ServiceTax, 10, string.Empty, 0, 1).PK;
			lines[11].AL_AT = ZGuid.Empty;
		}

		void VerifyInvoice(Invoice invoice, AccComplianceReportLineCollectionBase<AccComplianceReportLine> reportLines)
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
			var invoiceRAX = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "ZZRAX");
			AssertNotNull("Invoice.RAX", invoiceRAX);
			var invoiceBST = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "ZZBST");
			AssertNotNull("Invoice.BST", invoiceBST);
			var invoiceEXL = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "ZZEXL");
			AssertNotNull("Invoice.EXL", invoiceEXL);
			var invoiceINT = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "ZZINT");
			AssertNotNull("Invoice.INT", invoiceINT);
			var invoiceSER = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "ZZSER");
			AssertNotNull("Invoice.SER", invoiceSER);
			var invoiceNoTax = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum && x.AT_Code == "");
			AssertNotNull("Invoice.NoTax", invoiceNoTax);
		}

		public void TestBox5_NetVATNotification()
		{
			AssertEquals(0m, TestMTDSubmissionData.Box5_NetVAT);
			AssertNoNotifications(TestMTDSubmissionData.Box5_NetVATInfo);

			TestMTDSubmissionData.Box4_VATReclaimed = 0.01m;

			AssertEquals(0m, TestMTDSubmissionData.Box3_TotalVATDue);
			AssertEquals(0.01m, TestMTDSubmissionData.Box4_VATReclaimed);
			AssertEquals(0.01m, TestMTDSubmissionData.Box5_NetVAT);

			TestMTDSubmissionData.UpdateBox5_NetVATNotification();
			AssertHasWarning(TestMTDSubmissionData.Box5_NetVATInfo, "VAT is recoverable from HMRC");

			TestMTDSubmissionData.Box1_VATDue = 0.02m;

			AssertEquals(0.02m, TestMTDSubmissionData.Box3_TotalVATDue);
			AssertEquals(0.01m, TestMTDSubmissionData.Box4_VATReclaimed);
			AssertEquals(0.01m, TestMTDSubmissionData.Box5_NetVAT);

			TestMTDSubmissionData.UpdateBox5_NetVATNotification();
			AssertHasWarning(TestMTDSubmissionData.Box5_NetVATInfo, "VAT is payable to HMRC");
		}

		AccComplianceReport Report => report ??= Factory.NewWithValidTestData<AccComplianceReport>();
		MTDSubmissionData TestMTDSubmissionData => testMTDSubmissionData ?? (testMTDSubmissionData = new MTDSubmissionData(Factory));
		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		MTDSubmissionDataHelper Helper => helper ?? (helper = new MTDSubmissionDataHelper(Report));

		protected override void SetUp()
		{
			base.SetUp();

			CountrySwitch = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom);
		}

		protected override void TearDown()
		{
			CountrySwitch.Dispose();

			base.TearDown();
		}

		AccComplianceReport report;
		MTDSubmissionData testMTDSubmissionData;
		TestObjectCreator creator;
		MTDSubmissionDataHelper helper;
		IDisposable CountrySwitch;
	}
}
