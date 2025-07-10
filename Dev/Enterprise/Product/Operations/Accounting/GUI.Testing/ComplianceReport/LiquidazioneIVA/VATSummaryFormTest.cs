using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ComplianceReport.LiquidazioneIVA.Testing
{
	public class VATSummaryFormTest : TestCaseWithFactory
	{
		AccComplianceReport Report;
		LIQSubmissionDataColumns DataCollection;
		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		[ExpectNoExceptions]
		public void TestLoad()
		{
			DataCollection.PopulateDataFromReport();

			using (VATSummaryForm form = new VATSummaryForm(DataCollection))
			{
				form.Show();
				Application.DoEvents();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			CountrySwitch = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy);

			Report = Factory.NewWithValidTestData<AccComplianceReport>();
			Report.FillWithValidTestData();
			Report.ACR_ReportType = "LIQ";
			DataCollection = new LIQSubmissionDataColumns(Factory, Report);
		}

		protected override void TearDown()
		{
			CountrySwitch.Dispose();

			base.TearDown();
		}

		IDisposable CountrySwitch;

		public void TestRowsValues()
		{
			InvoiceDataForTesting();
			DataCollection.PopulateDataFromReport();
			DataCollection.Adjustments.Box2_TotalVatReceivables = 23m;
			DataCollection.Adjustments.Box7_BalancePreviousPeriod = -5m;

			AssertEquals(250m, DataCollection.ValuesToSubmit.Box1_TotalVatBaseReceivables);
			AssertEquals(37.2m, DataCollection.ValuesToSubmit.Box2_TotalVatReceivables);
			AssertEquals(-160m, DataCollection.ValuesToSubmit.Box3_TotalVatBasePayables);
			AssertEquals(-33m, DataCollection.ValuesToSubmit.Box4_TotalVatPayablesRecoverable);
			AssertEquals(0m, DataCollection.ValuesToSubmit.Box5_TotalVatPayablesNotRecoverable);
			AssertEquals(4.2m, DataCollection.ValuesToSubmit.Box6_VatBalanceReceivablesAndPayables);
			AssertEquals(-5m, DataCollection.ValuesToSubmit.Box7_BalancePreviousPeriod);
			AssertEquals(-0.8m, DataCollection.ValuesToSubmit.Box8_TotalBalance);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (VATSummaryForm form = new VATSummaryForm(DataCollection))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				AssertEquals("It is expect warning message", "No Liquidazione VAT Summary Report found for previous period. If this is your first Liquidazione IVA, you may enter a manual Adjustment. Otherwise, please ensure a 'LIQ' Compliance Report with VAT Summary exists in Generated or Finalized status.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		LIQSubmissionData InvoiceDataForTesting()
		{
			var testLIQSubmissionData = new LIQSubmissionData(Factory);

			TestObjectCreator creator = new TestObjectCreator(Factory);

			creator.CreateConfigurationForComplianceReport(Report, "AL", "TXR");

			var invoiceLinesAP = new List<InvoicingLineBase>();
			var invoiceLinesAR = new List<InvoicingLineBase>();

			var arInvoice = creator.CreateARInvoice<ARInvoice>("I0001" + Report.ReportLines.Count.ToString(), creator.EUR, 1m, creator.ABIGAS);
			var apInvoice = creator.CreateAPInvoice<APInvoice>("I0001" + Report.ReportLines.Count.ToString(), creator.EUR, 1m, 10m, 10m, 0m, 10m, 10m, 0m, creator.ABIGAS);
			Assert("Has Lines", apInvoice.Lines.Count > 0);
			for (int i = 0; i < 15; i++)
			{
				invoiceLinesAP.Add(creator.CreateInvoiceLine(apInvoice, creator.EUR, 1m, 10m, 10m, 5m));
				invoiceLinesAR.Add(creator.CreateInvoiceLine(arInvoice, creator.EUR, 1m, 12m, 12m, 5m));
			}
			Assert("Has Lines", arInvoice.Lines.Count > 0);

			LIQSubmissionDataTestHelper.CreateTaxRates(invoiceLinesAP, Creator);
			LIQSubmissionDataTestHelper.CreateTaxRates(invoiceLinesAR, Creator);

			Factory.Save();

			Report.GenerateFromQueue();
			Report.ClearReportLines_ForTestOnly();

			AssertEquals("ReportLines.Count", 18, Report.ReportLines.Count);

			VerifyInvoice(apInvoice, Report.ReportLines);
			VerifyInvoice(arInvoice, Report.ReportLines);

			return testLIQSubmissionData;
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

		public void TestInitializeSubmissionDataWithPreviousReport()
		{
			InvoiceDataForTesting();
			DataCollection.PopulateDataFromReport();
			DataCollection.Adjustments.Box2_TotalVatReceivables = 23m;
			DataCollection.Adjustments.Box7_BalancePreviousPeriod = -5m;

			var previousReport = GetPreviousReport();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (VATSummaryForm form = new VATSummaryForm(DataCollection))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				AssertEquals("It is expect warning message", "No Liquidazione VAT Summary Report found for previous period. If this is your first Liquidazione IVA, you may enter a manual Adjustment. Otherwise, please ensure a 'LIQ' Compliance Report with VAT Summary exists in Generated or Finalized status.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			SetPreviousDataCollection(previousReport);

			AssertEquals(250m, DataCollection.ValuesToSubmit.Box1_TotalVatBaseReceivables);
			AssertEquals(37.2m, DataCollection.ValuesToSubmit.Box2_TotalVatReceivables);
			AssertEquals(-160m, DataCollection.ValuesToSubmit.Box3_TotalVatBasePayables);
			AssertEquals(-33m, DataCollection.ValuesToSubmit.Box4_TotalVatPayablesRecoverable);
			AssertEquals(0m, DataCollection.ValuesToSubmit.Box5_TotalVatPayablesNotRecoverable);
			AssertEquals(4.2m, DataCollection.ValuesToSubmit.Box6_VatBalanceReceivablesAndPayables);
			AssertEquals(-5m, DataCollection.ValuesToSubmit.Box7_BalancePreviousPeriod);
			AssertEquals(-0.8m, DataCollection.ValuesToSubmit.Box8_TotalBalance);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (VATSummaryForm form = new VATSummaryForm(DataCollection))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				AssertEquals("The data in the VAT summary report has changed. You must save before exiting.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertNotNullOrEmpty("Warning message is expect.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(0, DataCollection.PageToAP);
				AssertEquals(0, DataCollection.PageToAR);
				AssertEquals(0, DataCollection.PageToLiquidazione);

				AssertFireSaveButtonWithoutErrors(form);
			}
		}

		void SetPreviousDataCollection(AccComplianceReport previousReport)
		{
			var previousDataCollection = new LIQSubmissionDataColumns(Factory, previousReport);

			previousDataCollection.ComputedByCW1.Box1_TotalVatBaseReceivables = 1500m;
			previousDataCollection.ComputedByCW1.Box2_TotalVatReceivables = 100m;
			previousDataCollection.ComputedByCW1.Box3_TotalVatBasePayables = 1000m;
			previousDataCollection.ComputedByCW1.Box4_TotalVatPayablesRecoverable = 200m;
			previousDataCollection.ComputedByCW1.Box5_TotalVatPayablesNotRecoverable = 200m;
			previousDataCollection.ComputedByCW1.Box7_BalancePreviousPeriod = -100m;

			previousDataCollection.Adjustments.Box1_TotalVatBaseReceivables = 25m;
			previousDataCollection.Adjustments.Box2_TotalVatReceivables = -200m;
			previousDataCollection.Adjustments.Box3_TotalVatBasePayables = 10m;
			previousDataCollection.Adjustments.Box4_TotalVatPayablesRecoverable = -120m;
			previousDataCollection.Adjustments.Box5_TotalVatPayablesNotRecoverable = 20m;
			previousDataCollection.Adjustments.Box7_BalancePreviousPeriod = 50m;

			previousDataCollection.PageFromAP = 2;
			previousDataCollection.PageFromAR = 3;
			previousDataCollection.PageFromLiquidazione = 5;
			previousDataCollection.PageToAP = 0;
			previousDataCollection.PageToAR = 0;
			previousDataCollection.PageToLiquidazione = 0;

			Factory.Save();
		}

		AccComplianceReport GetPreviousReport()
		{
			AccComplianceReport previousReport = Factory.NewWithValidTestData<AccComplianceReport>();
			previousReport.FillWithValidTestData();
			previousReport.ACR_ReportType = "LIQ";
			previousReport.ACR_DateFrom = Report.ACR_DateFrom.AddDays(-7);
			previousReport.ACR_DateTo = Report.ACR_DateFrom.AddDays(-1);
			previousReport.ACR_Status = "GEN";

			Factory.Save();

			return previousReport;
		}

		public void TestPageFromErrorsOnSave()
		{
			DataCollection.PopulateDataFromReport();

			using (VATSummaryForm form = new VATSummaryForm(DataCollection))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();

				AssertFireSaveButtonWithErrors(form);

				DataCollection.PageFromAP = 0;
				DataCollection.PageFromAR = 2;
				DataCollection.PageFromLiquidazione = 2;
				AssertFireSaveButtonWithErrors(form);

				DataCollection.PageFromAP = 2;
				DataCollection.PageFromAR = 0;
				DataCollection.PageFromLiquidazione = 2;
				AssertFireSaveButtonWithErrors(form);

				DataCollection.PageFromAP = 2;
				DataCollection.PageFromAR = 2;
				DataCollection.PageFromLiquidazione = 0;
				AssertFireSaveButtonWithErrors(form);

				AssertFireSaveButtonWithoutErrors(form);
			}
		}

		public void TestOpenFormWithModifiedData()
		{
			InvoiceDataForTesting();
			DataCollection.PopulateDataFromReport();

			var previousReport = GetPreviousReport();
			SetPreviousDataCollection(previousReport);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (VATSummaryForm form = new VATSummaryForm(DataCollection))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				AssertEquals("The data in the VAT summary report has changed. You must save before exiting.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertFireSaveButtonWithoutErrors(form);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (VATSummaryForm form = new VATSummaryForm(DataCollection))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				AssertNullOrEmpty("Not Warning message is expect.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertFireSaveButtonWithErrors(VATSummaryForm form)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			form.FireSaveButton();
			AssertNotNullOrEmpty("Warning message is expect.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		void AssertFireSaveButtonWithoutErrors(VATSummaryForm form)
		{
			DataCollection.PageFromAP = 1;
			DataCollection.PageFromAR = 2;
			DataCollection.PageFromLiquidazione = 2;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			form.FireSaveButton();
			AssertNullOrEmpty("Not Warning message is expect.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
