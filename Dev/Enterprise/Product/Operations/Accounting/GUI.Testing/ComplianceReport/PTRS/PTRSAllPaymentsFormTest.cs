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

	public class PTRSAllPaymentsFormTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			using (var form = new PTRSAllPaymentsForm(ReportData))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Payment Times Reporting Scheme - All Payments", form.FormCaption);
			}
		}

		public void TestHandleButtonsAvailability()
		{
			using (var form = new PTRSAllPaymentsForm(ReportData))
			{
				form.Show();
				Application.DoEvents();

				AssertState(false, false, false, false);

				ReportData.ATR_Status = AccTaxReturn.Status.Saved;
				AssertState(true, false, false, true);

				ReportData.ATR_Status = AccTaxReturn.Status.Submitted;
				AssertState(false, false, true, false);

				void AssertState(bool saved, bool generated, bool submitted, bool submittedEnabled)
				{
					AssertEquals("IsSaved", saved, ReportData.IsSaved);
					AssertEquals("IsGenerated", generated, ReportData.IsGenerated);
					AssertEquals("IsSubmitted", submitted, ReportData.IsSubmitted);
					AssertEquals("MarkAsSubmittedButton", submittedEnabled, form.MarkAsSubmittedButton_ForTestOnly.Enabled);
				}
			}
		}

		public void TestShowShowPreSaveDialogs()
		{
			Factory.Save();

			using (var form = new PTRSAllPaymentsForm(ReportData))
			{
				form.Show();
				Application.DoEvents();

				ReportData.ATR_Comment = "Change something";

				form.FireSaveButton();

				AssertEquals("Last Message", @"Saving this form will affect the AK column value in the Payment Times Reportable Small Business report.
Please, open and save that report to apply the changes you are saving now.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Last Confirmation", "Yes", UnitTestUserNotification.Instance.LastConfirmationStringShown);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals("ATR_Status", AccTaxReturn.Status.Saved, ReportData.ATR_Status);
			}
		}

		public void TestMarkAsSubmittedButton_Click()
		{
			Factory.Save();
			AssertEquals(AccTaxReturn.Status.Saved, ReportData.ATR_Status);

			using (var form = new PTRSAllPaymentsForm(ReportData))
			{
				form.Show();
				Application.DoEvents();

				Assert("MarkAsSubmittedButton.Enabled", form.MarkAsSubmittedButton_ForTestOnly.Enabled);
				form.MarkAsSubmittedButton_ForTestOnly.PerformClick();

				AssertEquals("Last Info Message", "Close and re-open form to see the submitted state.", UnitTestUserNotification.Instance.LastMessage.Text);
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
			Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			ReportData = Factory.New<PtrsAllPaymentsReport>();
			ReportData.ATR_ACR_ComplianceReport = Report.PK;
		}

		AccComplianceReport Report;
		PtrsAllPaymentsReport ReportData;
	}

	[TestedType(typeof(PTRSAllPaymentsForm))]
	public class PTRSAllPaymentsFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_ReportType = ComplianceReportTypes.PaymentTimesAllPaymentsReportType;
			complianceReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			var reportData = Factory.NewWithValidTestData<PtrsAllPaymentsReport>();
			reportData.ATR_ACR_ComplianceReport = complianceReport.PK;
			_ = reportData.Columns;
			Factory.Save();

			return new PTRSAllPaymentsForm(reportData);
		}
	}
}
