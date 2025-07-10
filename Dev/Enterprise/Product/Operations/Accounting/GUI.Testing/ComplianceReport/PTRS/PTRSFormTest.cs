namespace Enterprise.Accounting.GUI.ComplianceReport.PTRS.Testing
{
	using System.Windows.Forms;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ComplianceReport;
	using Enterprise.Accounting.Business.ComplianceReport.PTRS;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI.Testing;
	using NUnit.Framework;
	using static Enterprise.Accounting.Business.AccountingConstants;

	public class PTRSFormTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			using (var form = new PTRSForm(ReportData))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Payment Times Reporting Scheme - Reportable Small Businesses", form.FormCaption);
			}
		}

		public void TestHandleButtonsAvailability()
		{
			using (var form = new PTRSForm(ReportData))
			{
				form.Show();
				Application.DoEvents();

				AssertState(false, false, false, true, false);

				ReportData.ATR_Status = AccTaxReturn.Status.Saved;
				AssertState(true, false, false, true, false);

				ReportData.ATR_Status = AccTaxReturn.Status.Generated;
				AssertState(false, true, false, true, true);

				ReportData.ATR_Status = AccTaxReturn.Status.Submitted;
				AssertState(false, false, true, false, false);

				void AssertState(bool saved, bool generated, bool submitted, bool generateEnabled, bool submittedEnabled)
				{
					AssertEquals("IsSaved", saved, ReportData.IsSaved);
					AssertEquals("IsGenerated", generated, ReportData.IsGenerated);
					AssertEquals("IsSubmitted", submitted, ReportData.IsSubmitted);
					AssertEquals("GenerateFileButton.Enabled", generateEnabled, form.GenerateFileButton_ForTestOnly.Enabled);
					AssertEquals("MarkAsSubmittedButton", submittedEnabled, form.MarkAsSubmittedButton_ForTestOnly.Enabled);
				}
			}
		}

		public void TestGenerateFileButton_Click()
		{
			using (var form = new PTRSForm(ReportData))
			{
				form.Show();
				Application.DoEvents();

				Assert("Pre-condition: Status is empty", ReportData.ATR_Status.IsEmpty);
				Assert("GenerateFileButton.Enabled", form.GenerateFileButton_ForTestOnly.Enabled);
				form.GenerateFileButton_ForTestOnly.PerformClick();

				AssertEquals("Last Message", "Please save this form before generating file.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Status is empty", ReportData.ATR_Status.IsEmpty);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Factory.Save();
				Assert("GenerateFileButton.Enabled", form.GenerateFileButton_ForTestOnly.Enabled);
				form.GenerateFileButton_ForTestOnly.PerformClick();

				AssertEquals("Last Message", @"You are about to generate the output file for PTRS. which should be submitted to Payment Times Reporting Portal.
The file will be added to the Compliance Report eDocs.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Last Confirmation", "Yes", UnitTestUserNotification.Instance.LastConfirmationStringShown);
				Assert("result message contains the name of the file", UnitTestUserNotification.Instance.PreviousMessages[0].Text.StartsWith(@"The output file for PTRS was successfully generated.
The file has been added to the Compliance Report eDocs with the file name: PTRS-EDI 01-Jan-21"));
				Assert("The file extension is txt", UnitTestUserNotification.Instance.PreviousMessages[0].Text.EndsWith(".csv"));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals("ATR_Status", AccTaxReturn.Status.Generated, ReportData.ATR_Status);
			}
		}

		public void TestShowShowPreSaveDialogs()
		{
			using (var form = new PTRSForm(ReportData))
			{
				form.Show();
				Application.DoEvents();

				ReportData.ATR_Comment = "Change something";

				form.FireSaveButton();

				AssertNull("No special message on saving", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("ATR_Status", AccTaxReturn.Status.Saved, ReportData.ATR_Status);
			}
		}

		public void TestMarkAsSubmittedButton_Click()
		{
			Factory.Save();
			var fileName = ReportData.GenerateReport();
			Factory.Save();
			AssertEquals(AccTaxReturn.Status.Generated, ReportData.ATR_Status);
			AssertEquals("One eDoc added", 1, ReportData.DocManagerInfo.AllEDocs.Count);

			using (var form = new PTRSForm(ReportData))
			{
				form.Show();
				Application.DoEvents();

				Assert("MarkAsSubmittedButton.Enabled", form.MarkAsSubmittedButton_ForTestOnly.Enabled);
				form.MarkAsSubmittedButton_ForTestOnly.PerformClick();

				AssertEquals("Last Info Message", "Close and re-open form to see the submitted state.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Last Confirmation Message", $"Please confirm that the latest generated PTRS file: {fileName} has been submitted to Payment Times Reporting Portal.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Last Confirmation", "Submit", UnitTestUserNotification.Instance.LastConfirmationStringShown);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals("ATR_Status", AccTaxReturn.Status.Submitted, ReportData.ATR_Status);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Report = Factory.NewWithValidTestData<AccComplianceReport>();
			Report.ACR_ReportType = ComplianceReportTypes.TaxablePaymentsAnnualReportType;
			Report.ACR_DateFrom = new ZDate(2021, 1, 1);
			Report.ACR_DateTo = new ZDate(2021, 6, 30);
			ReportData = Factory.New<PtrsReport>();
			ReportData.ATR_ACR_ComplianceReport = Report.PK;
		}

		AccComplianceReport Report;
		PtrsReport ReportData;
	}

	[TestedType(typeof(PTRSForm))]
	public class PTRSFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_ReportType = ComplianceReportTypes.PaymentTimesSmallBusinessReportType;
			var reportData = Factory.NewWithValidTestData<PtrsReport>();
			reportData.ATR_ACR_ComplianceReport = complianceReport.PK;

			PopulateAllColumns(reportData);
			Factory.Save();

			return new PTRSForm(reportData);
		}

		internal static void PopulateAllColumns(PtrsReport reportData)
		{
			reportData.ReasonToOverrideInvoicesPaidWithin20Days = "";
			reportData.ReasonToOverrideInvoicesPaidBetween21And30Days = "";
			reportData.ReasonToOverrideInvoicesPaidBetween31And60Days = "";
			reportData.ReasonToOverrideInvoicesPaidBetween61And90Days = "";
			reportData.ReasonToOverrideInvoicesPaidBetween91And120Days = "";
			reportData.ReasonToOverrideInvoicesPaidInMoreThan120Days = "";
			reportData.ReasonForInvoicesPaidSupplyChainFinanceArrangements = "";

			reportData.C.Value = "";
			reportData.D.Value = "";
			reportData.E.Value = "";
			reportData.F.Value = "";
			reportData.G.Value = "";
			reportData.H.Value = "";
			reportData.I.Value = "";
			reportData.J.Value = "";

			reportData.M.Value = 0;
			reportData.N.Value = 0;
			reportData.O.Value = "";
			reportData.P.Value = 0;
			reportData.Q.Value = 0;
			reportData.R.Value = "";
			reportData.T.Value = 0;
			reportData.U.Value = "";

			reportData.AH.Value = "";
			reportData.AI.Value = "";
			reportData.AJ.Value = "";

			reportData.AO.Value = "";
			reportData.AP.Value = "";
			reportData.AQ.Value = ZDateTime.Empty;
			reportData.AR.Value = "";
			reportData.AS.Value = "";
			reportData.AT.Value = "";
			reportData.AU.Value = "";
			reportData.AV.Value = "";
			reportData.AW.Value = "";
			reportData.AX.Value = "";
			reportData.AY.Value = "";
			reportData.AZ.Value = "";
			reportData.BB.Value = "";
			reportData.BC.Value = "";
			reportData.BD.Value = "";
			reportData.BE.Value = ZDateTime.Empty;
			reportData.BF.Value = "";
			reportData.BG.Value = "";
			reportData.BH.Value = ZDateTime.Empty;
		}
	}
}
