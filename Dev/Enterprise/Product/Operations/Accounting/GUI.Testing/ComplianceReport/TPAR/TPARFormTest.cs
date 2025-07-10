namespace Enterprise.Accounting.GUI.ComplianceReport.TPAR.Testing
{
	using System.Windows.Forms;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ComplianceReport;
	using Enterprise.Accounting.Business.ComplianceReport.TPAR;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI.Testing;
	using NUnit.Framework;
	using static Enterprise.Accounting.Business.AccountingConstants;

	public class TPARfFormTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			using (TPARForm form = new TPARForm(ReportData))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("TPAR Summary by Creditors", form.FormCaption);
			}
		}

		public void TestHandleButtonsAvailability()
		{
			using (TPARForm form = new TPARForm(ReportData))
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
			using (TPARForm form = new TPARForm(ReportData))
			{
				form.Show();
				Application.DoEvents();

				Assert("Pre-condition: Status is empty", ReportData.ATR_Status.IsEmpty);
				Assert("GenerateFileButton.Enabled", form.GenerateFileButton_ForTestOnly.Enabled);
				form.GenerateFileButton_ForTestOnly.PerformClick();

				AssertEquals("Last Message", "Please save TPAR Summary before generating file.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Status is empty", ReportData.ATR_Status.IsEmpty);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Factory.Save();
				Assert("GenerateFileButton.Enabled", form.GenerateFileButton_ForTestOnly.Enabled);
				form.GenerateFileButton_ForTestOnly.PerformClick();

				AssertEquals("Last Message", @"You are about to generate an output file for TPAR, which should be submitted to ATO.
The file will be added to the Compliance Report eDocs.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Last Confirmation", "Yes", UnitTestUserNotification.Instance.LastConfirmationStringShown);
				Assert("result message contains the name of the file", UnitTestUserNotification.Instance.PreviousMessages[0].Text.StartsWith(@"The output file for TPAR was generated with success.
The file has been added to the Compliance Report eDocs with the file name: TPAR-EDI-2020-v1-"));
				Assert("The file extension is txt", UnitTestUserNotification.Instance.PreviousMessages[0].Text.EndsWith(".txt"));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertEquals("ATR_Status", AccTaxReturn.Status.Generated, ReportData.ATR_Status);
			}
		}

		public void TestShowShowPreSaveDialogs()
		{
			Factory.Save();
			var fileName = ReportData.GenerateReport();
			Factory.Save();
			AssertEquals(AccTaxReturn.Status.Generated, ReportData.ATR_Status);
			AssertEquals("One eDoc added", 1, ReportData.DocManagerInfo.AllEDocs.Count);

			using (TPARForm form = new TPARForm(ReportData))
			{
				form.Show();
				Application.DoEvents();

				ReportData.ATR_Comment = "Change something";

				form.FireSaveButton();

				AssertEquals("Last Message", @"Saving this form will make the latest generated TPAR file not valid for submitting to ATO.
A new file must be generated.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Last Confirmation", "Yes", UnitTestUserNotification.Instance.LastConfirmationStringShown);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

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

			using (TPARForm form = new TPARForm(ReportData))
			{
				form.Show();
				Application.DoEvents();

				Assert("MarkAsSubmittedButton.Enabled", form.MarkAsSubmittedButton_ForTestOnly.Enabled);
				form.MarkAsSubmittedButton_ForTestOnly.PerformClick();

				AssertEquals("Last Info Message", "Close and re-open form to see the submitted state.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Last Confirmation Message", $"Please confirm that the latest generated TPAR file: {fileName} has been submitted to ATO.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
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
			Report.ACR_DateFrom = new ZDate(2019, 7, 1);
			Report.ACR_DateTo = new ZDate(2020, 6, 30);
			ReportData = Factory.New<TparReport>();
			ReportData.ATR_ACR_ComplianceReport = Report.PK;

			ReportData.ComplianceReport.Company.OrgProxy.MainAddress.OA_Phone = "+61280012666";
		}

		AccComplianceReport Report;
		TparReport ReportData;
	}

	[TestedType(typeof(TPARForm))]
	public class TPARFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_ReportType = ComplianceReportTypes.TaxablePaymentsAnnualReportType;
			var reportData = Factory.NewWithValidTestData<TparReport>();
			reportData.ATR_ACR_ComplianceReport = complianceReport.PK;
			_ = reportData.Lines;
			reportData.HasChanges = false;

			return new TPARForm(reportData);
		}
	}
}
