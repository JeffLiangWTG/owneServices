using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using TaxTypes = Enterprise.MasterFiles.Business.AccTaxRate.Types;

namespace Enterprise.Accounting.GUI.ComplianceReport.HMRC.Testing
{
	public class MTDSubmissionFormTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			DataCollection.PopulateDataFromReport();

			using (MTDSubmissionForm form = new MTDSubmissionForm(DataCollection))
			{
				form.Show();
				Application.DoEvents();
			}
		}

		public void TestSubmitButton_AsNull()
		{
			var testingCompany = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			testingCompany.OK_CodeType = "VAT";
			testingCompany.OK_RN_NKCodeCountry = "GB";
			testingCompany.OK_CustomsRegNo = "12345678";

			using (MTDSubmissionForm form = new MTDSubmissionForm(DataCollection))
			{
				DataCollection.ComplianceReport.ACR_ReportType = "AAA";
				DataCollection.ComplianceReport.ACR_DateFrom = new ZDate(2019, 1, 1);
				DataCollection.ComplianceReport.ACR_DateTo = new ZDate(2019, 12, 31);

				form.Show();
				form.SubmitButton_ForTestOnly = null;
				form.Call_HandleSubmitButtonAvailability_ForTestOnly();
				AssertNotNull(ErrorReporter.LastMessageReported);

				AssertEquals("LastKeyReported", "MTD HandleSubmitButtonAvailability - SubmitButton is Null.", ErrorReporter.LastKeyReported);

				var expectedMessage = @"ORGProxy Name & Code: EDI CUSTOMS BROKERS (EDICUS)
VAT Registration Company: 12345678
Compliance Report Type: AAA
Compliance Report Date From: 01-Jan-19 00:00:00
Compliance Report Date To: 31-Dec-19 00:00:00
Compliance Report Status: ADD
";
				AssertEquals("LastMessageReported", expectedMessage, ErrorReporter.LastMessageReported);

				ErrorReporter.Instance.Clear();
			}
		}

		public void TestSubmitButton_SuccessfulSubmission()
		{
			using (MTDSubmissionForm form = new MTDSubmissionForm(DataCollection))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.Show();
				form.IsDeclarationAccepted_TestOnly = true;
				form.SuccessfulReturnMessage = "VAT return submitted successfully.";
				form.ClickSubmitButton();
				AssertEquals("VAT return submitted successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestSubmitButton_FailedSubmission()
		{
			using (MTDSubmissionForm form = new MTDSubmissionForm(DataCollection))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.Show();
				form.IsDeclarationAccepted_TestOnly = true;
				form.ErrorMessage = "An Error occurred. Could not submit VAT return.";
				form.ClickSubmitButton();
				AssertEquals("An Error occurred. Could not submit VAT return.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestSubmitButton_IfDeclerationIsNotAccepted()
		{
			using (MTDSubmissionForm form = new MTDSubmissionForm(DataCollection))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.Show();
				AssertEquals("Submit VAT Return to HMRC", form.SubmitButton_ForTestOnly.Text);

				form.IsDeclarationAccepted_TestOnly = false;
				form.ClickSubmitButton();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestSubmitButton_GroupMemberCompany()
		{
			(var groupCompany, var memberCompany) = MTDSubmissionDataTestEnvironmentCreator.CreateUKGroupAndMemberCompany(Factory, "UKG", "UK1");
			Factory.Save();
			Report.Finalise();

			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, memberCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			using (MTDSubmissionForm form = new MTDSubmissionForm(DataCollection))
			{
				form.Show();
				AssertEquals("Submit VAT Return to Group", form.SubmitButton_ForTestOnly.Text);

				form.ClickSubmitButton();
				AssertEquals("VAT Return has been submitted successfully to Group.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCheckRow4()
		{
			InvoiceDataForTesting();
			DataCollection.PopulateDataFromReport();
			DataCollection.Adjustments.Box2_VATDueReverseChg = 23m;
			DataCollection.Adjustments.Box7_TotalPurchaseExVAT = -5m;

			AssertEquals(2m, DataCollection.ValuesToSubmitToHMRC.Box1_VATDue);
			AssertEquals(24m, DataCollection.ValuesToSubmitToHMRC.Box2_VATDueReverseChg);
			AssertEquals(26m, DataCollection.ValuesToSubmitToHMRC.Box3_TotalVATDue);
			AssertEquals(28m, DataCollection.ValuesToSubmitToHMRC.Box4_VATReclaimed);
			AssertEquals(2m, DataCollection.ValuesToSubmitToHMRC.Box5_NetVAT);
			AssertEquals(60m, DataCollection.ValuesToSubmitToHMRC.Box6_TotalSalesExVAT);
			AssertEquals(55m, DataCollection.ValuesToSubmitToHMRC.Box7_TotalPurchaseExVAT);
			AssertEquals(0m, DataCollection.ValuesToSubmitToHMRC.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(0m, DataCollection.ValuesToSubmitToHMRC.Box9_GoodsPurchaseECMembersExVAT);

			using (MTDSubmissionForm form = new MTDSubmissionForm(DataCollection))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
			}
		}

		public void TestIfErrorsArentIncludedInTotalOver50000()
		{
			using (MTDSubmissionForm form = new MTDSubmissionForm(DataCollection))
			{
				DataCollection.UnsubmitedPreviousValues.Box1_VATDue = 30000m;
				DataCollection.Adjustments.Box1_VATDue = 30000m;

				AssertEquals(30000m, DataCollection.ValuesToSubmitToHMRC.Box1_VATDue);

				form.Show();
				Application.DoEvents();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
				MTDSubmissionDataColumns.DidNotIncludeChangesInCurrentVATReturnMessage));
			}
		}

		public void TestIfErrorsArentIncludedInTotalOver1Percent()
		{
			using (MTDSubmissionForm form = new MTDSubmissionForm(DataCollection))
			{
				DataCollection.ComputedByCW1.Box6_TotalSalesExVAT = 400000m;
				DataCollection.UnsubmitedPreviousValues.Box1_VATDue = 10000m;
				DataCollection.Adjustments.Box1_VATDue = 10000m;

				AssertEquals(10000m, DataCollection.ValuesToSubmitToHMRC.Box1_VATDue);

				form.Show();
				Application.DoEvents();
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
				MTDSubmissionDataColumns.DidNotIncludeChangesInCurrentVATReturnMessage));
			}
		}

		public void TestIfErrorsAreIncludedInTotalUnder1Percent()
		{
			using (MTDSubmissionForm form = new MTDSubmissionForm(DataCollection))
			{
				DataCollection.ComputedByCW1.Box6_TotalSalesExVAT = 4000000m;
				DataCollection.UnsubmitedPreviousValues.Box1_VATDue = 10000m;
				DataCollection.Adjustments.Box1_VATDue = 10000m;

				AssertEquals(20000m, DataCollection.ValuesToSubmitToHMRC.Box1_VATDue);

				form.Show();
				AssertEquals(false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
				MTDSubmissionDataColumns.DidNotIncludeChangesInCurrentVATReturnMessage));
			}
		}

		public void TestIfErrorsAreIncludedInTotalUnder10000()
		{
			using (MTDSubmissionForm form = new MTDSubmissionForm(DataCollection))
			{
				DataCollection.UnsubmitedPreviousValues.Box1_VATDue = 500m;
				DataCollection.Adjustments.Box1_VATDue = 20m;

				AssertEquals(520m, DataCollection.ValuesToSubmitToHMRC.Box1_VATDue);

				form.Show();
				AssertEquals(false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
				MTDSubmissionDataColumns.DidNotIncludeChangesInCurrentVATReturnMessage));
			}
		}

		public void TestReceiptReferenceLabelText()
		{
			using (MTDSubmissionForm form = new MTDSubmissionForm(DataCollection))
			{
				form.Show();

				var receiptReferenceLabel = form.Controls.Find("receiptReferenceText", true).FirstOrDefault() as ZArchitecture.ZLabel;
				AssertNotNull("Receipt Reference Label", receiptReferenceLabel);
				var expectedText = "Submission Receipt Reference: ";
				AssertEquals("Receipt Reference Label Text", expectedText, receiptReferenceLabel.Text);
			}
		}

		MTDSubmissionData InvoiceDataForTesting()
		{
			var testMTDSubmissionData = new MTDSubmissionData(Factory);

			TestObjectCreator creator = new TestObjectCreator(Factory);

			creator.CreateConfigurationForComplianceReport(Report, "AL", "TXR");

			var apInvoice = creator.CreateAPInvoice<APInvoice>("I0001" + Report.ReportLines.Count.ToString(), creator.AUD, 1m, 10m, 10m, 0m, 10m, 10m, 0m, creator.ABIGAS);
			Assert("Has Lines", apInvoice.Lines.Count > 0);
			var lineAP1 = creator.CreateInvoiceLine(apInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP2 = creator.CreateInvoiceLine(apInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP3 = creator.CreateInvoiceLine(apInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP4 = creator.CreateInvoiceLine(apInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP5 = creator.CreateInvoiceLine(apInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP6 = creator.CreateInvoiceLine(apInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP7 = creator.CreateInvoiceLine(apInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP8 = creator.CreateInvoiceLine(apInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP9 = creator.CreateInvoiceLine(apInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP10 = creator.CreateInvoiceLine(apInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP11 = creator.CreateInvoiceLine(apInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAP12 = creator.CreateInvoiceLine(apInvoice, creator.AUD, 1m, 10m, 10m, 0m);

			var arInvoice = creator.CreateARInvoice<ARInvoice>("I0001" + Report.ReportLines.Count.ToString(), creator.AUD, 1m, creator.ABIGAS);
			var lineAR1 = creator.CreateInvoiceLine(arInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAR2 = creator.CreateInvoiceLine(arInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAR3 = creator.CreateInvoiceLine(arInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAR4 = creator.CreateInvoiceLine(arInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAR5 = creator.CreateInvoiceLine(arInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAR6 = creator.CreateInvoiceLine(arInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAR7 = creator.CreateInvoiceLine(arInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAR8 = creator.CreateInvoiceLine(arInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAR9 = creator.CreateInvoiceLine(arInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAR10 = creator.CreateInvoiceLine(arInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAR11 = creator.CreateInvoiceLine(arInvoice, creator.AUD, 1m, 10m, 10m, 0m);
			var lineAR12 = creator.CreateInvoiceLine(arInvoice, creator.AUD, 1m, 10m, 10m, 0m);

			lineAP1.AL_AT = creator.CreateTaxRate("RAT", "Rated", TaxTypes.Rated, 10, string.Empty, 0, 1).PK;
			lineAP2.AL_AT = creator.CreateTaxRate("RVS", "ReverseRated", TaxTypes.ReverseRated, 10, string.Empty, 0, 1).PK;
			lineAP3.AL_AT = creator.CreateTaxRate("EXT", "Exempt", TaxTypes.Exempt, 10, string.Empty, 0, 1).PK;
			lineAP4.AL_AT = creator.CreateTaxRate("CAP", "CapitalRated", TaxTypes.CapitalRated, 10, string.Empty, 0, 1).PK;
			lineAP5.AL_AT = creator.CreateTaxRate("NOT", "NotReportable", TaxTypes.NotReportable, 10, string.Empty, 0, 1).PK;
			lineAP6.AL_AT = creator.CreateTaxRate("SUS", "Suspended", TaxTypes.Suspended, 10, string.Empty, 0, 1).PK;
			lineAP7.AL_AT = creator.CreateTaxRate("RAX", "RatedInAnotherCountry", TaxTypes.RatedInAnotherCountry, 10, string.Empty, 0, 1).PK;
			lineAP8.AL_AT = creator.CreateTaxRate("BST", "ReportableUnderBusinessTax", TaxTypes.ReportableUnderBusinessTax, 10, string.Empty, 0, 1).PK;
			lineAP9.AL_AT = creator.CreateTaxRate("EXL", "ExcludedFromTheTaxBase", TaxTypes.ExcludedFromTheTaxBase, 10, string.Empty, 0, 1).PK;
			lineAP10.AL_AT = creator.CreateTaxRate("INT", "IntegratedGST", TaxTypes.IntegratedGST, 10, string.Empty, 0, 1).PK;
			lineAP11.AL_AT = creator.CreateTaxRate("SER", "ServiceTax", TaxTypes.ServiceTax, 10, string.Empty, 0, 1).PK;
			lineAP12.AL_AT = ZGuid.Empty;

			lineAR1.AL_AT = creator.CreateTaxRate("RAT", "Rated", TaxTypes.Rated, 10, string.Empty, 0, 1).PK;
			lineAR2.AL_AT = creator.CreateTaxRate("RVS", "ReverseRated", TaxTypes.ReverseRated, 10, string.Empty, 0, 1).PK;
			lineAR3.AL_AT = creator.CreateTaxRate("EXT", "Exempt", TaxTypes.Exempt, 10, string.Empty, 0, 1).PK;
			lineAR4.AL_AT = creator.CreateTaxRate("CAP", "CapitalRated", TaxTypes.CapitalRated, 10, string.Empty, 0, 1).PK;
			lineAR5.AL_AT = creator.CreateTaxRate("NOT", "NotReportable", TaxTypes.NotReportable, 10, string.Empty, 0, 1).PK;
			lineAR6.AL_AT = creator.CreateTaxRate("SUS", "Suspended", TaxTypes.Suspended, 10, string.Empty, 0, 1).PK;
			lineAR7.AL_AT = creator.CreateTaxRate("RAX", "RatedInAnotherCountry", TaxTypes.RatedInAnotherCountry, 10, string.Empty, 0, 1).PK;
			lineAR8.AL_AT = creator.CreateTaxRate("BST", "ReportableUnderBusinessTax", TaxTypes.ReportableUnderBusinessTax, 10, string.Empty, 0, 1).PK;
			lineAR9.AL_AT = creator.CreateTaxRate("EXL", "ExcludedFromTheTaxBase", TaxTypes.ExcludedFromTheTaxBase, 10, string.Empty, 0, 1).PK;
			lineAR10.AL_AT = creator.CreateTaxRate("INT", "IntegratedGST", TaxTypes.IntegratedGST, 10, string.Empty, 0, 1).PK;
			lineAR11.AL_AT = creator.CreateTaxRate("SER", "ServiceTax", TaxTypes.ServiceTax, 10, string.Empty, 0, 1).PK;
			lineAR12.AL_AT = ZGuid.Empty;

			Factory.Save();

			Report.GenerateFromQueue();
			Report.ClearReportLines_ForTestOnly();

			AssertEquals("ReportLines.Count", 24, Report.ReportLines.Count);

			VerifyInvoice(apInvoice, Report.ReportLines);
			VerifyInvoice(arInvoice, Report.ReportLines);

			return testMTDSubmissionData;
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

		protected override void SetUp()
		{
			base.SetUp();

			CountrySwitch = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom);
			Report = Factory.NewWithValidTestData<AccComplianceReport>();
			Report.ACR_ReportType = "MTD";

			DataCollection = new MTDSubmissionDataColumns(Factory, Report);
		}

		protected override void TearDown()
		{
			CountrySwitch.Dispose();

			base.TearDown();
		}

		AccComplianceReport Report;
		MTDSubmissionDataColumns DataCollection;
		IDisposable CountrySwitch; 
	}
}
