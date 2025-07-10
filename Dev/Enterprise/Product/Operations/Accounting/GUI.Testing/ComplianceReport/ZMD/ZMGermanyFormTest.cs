using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.GUI.ComplianceReport.ZMD;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.GUI.Testing.ComplianceReport.ZMGermany
{
	public class ZMGermanyFormTest : TestCaseWithFactory
	{
		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;
		AccComplianceReport Report;

		protected override void SetUp()
		{
			base.SetUp();
			Report = Factory.NewWithValidTestData<AccComplianceReport>();
			Report.ACR_ReportType = ComplianceReportTypes.ZusammenfassendeMeldungGermanyReportType;
			Report.ACR_DateFrom = new ZDate(2021, 3, 1);
			Report.ACR_DateTo = new ZDate(2021, 3, 31);
		}

		[ExpectNoExceptions]
		public void TestLoad()
		{
			using (ZMGermanyForm form = new ZMGermanyForm(Report))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("ZMD Summary", form.FormCaption);
			}
		}

		public void TestValidateCurrentTaxReturn()
		{
			var taxReturnHeader = CreateTaxReturn(1, Creator.Debtor, "FR", "987654", 1500, AccTaxReturn.Status.Saved);
			taxReturnHeader.ATR_VATRegNo = "";
			using (ZMGermanyForm form = new ZMGermanyForm(Report))
			{
				form.Show();
				Application.DoEvents();
				var errorMessage = form.ZMReport.ValidateCurrentTaxReturn();
				AssertEquals("Failed validation because registration ID not entered", false, errorMessage.IsEmpty);
			}

			CreateTaxReturn(2, Creator.Debtor, "FR", "987654", 1500, AccTaxReturn.Status.Saved);
			using (ZMGermanyForm form = new ZMGermanyForm(Report))
			{
				form.Show();
				Application.DoEvents();
				var errorMessage = form.ZMReport.ValidateCurrentTaxReturn();
				AssertEquals("Successful validation because registration ID was entered", true, errorMessage.IsEmpty);
			}
		}

		public void TestHandleButtonsAvailability()
		{
			var taxReturnVersion1 = CreateTaxReturn(1, Creator.Debtor, "FR", "987654", 1500, AccTaxReturn.Status.Saved);
			using (ZMGermanyForm form = new ZMGermanyForm(Report))
			{
				form.Show();
				form.ZMReport.SelectedVersion = "1";
				Application.DoEvents();
				AssertEquals("GenerateFileButton.Enabled for new report version 1", true, form.GenerateButton_ForTestOnly.Enabled);
				AssertEquals("MarkAsSubmittedButton.Enabled for new report version 1", false, form.MarkAsSubmittedButton_ForTestOnly.Enabled);
			}

			taxReturnVersion1.ATR_Status = AccTaxReturn.Status.Generated;
			using (ZMGermanyForm form = new ZMGermanyForm(Report))
			{
				form.Show();
				form.ZMReport.SelectedVersion = "1";
				Application.DoEvents();
				AssertEquals("GenerateFileButton.Enabled for generated report version 1", false, form.GenerateButton_ForTestOnly.Enabled);
				AssertEquals("MarkAsSubmittedButton.Enabled for generated report version 1", true, form.MarkAsSubmittedButton_ForTestOnly.Enabled);
			}

			CreateTaxReturn(2, Creator.Debtor1, "ES", "777888", 2200, AccTaxReturn.Status.Saved);
			using (ZMGermanyForm form = new ZMGermanyForm(Report))
			{
				form.Show();
				form.ZMReport.SelectedVersion = "1";
				Application.DoEvents();
				AssertEquals("GenerateFileButton.Enabled for report version 1 of 2", false, form.GenerateButton_ForTestOnly.Enabled);
				AssertEquals("MarkAsSubmittedButton.Enabled for report version 1 of 2", true, form.MarkAsSubmittedButton_ForTestOnly.Enabled);

				form.ZMReport.SelectedVersion = "2";
				form.HandleButtonsAvailability(Report, "");
				AssertEquals("GenerateFileButton.Enabled for report version 2 of 2", true, form.GenerateButton_ForTestOnly.Enabled);
				AssertEquals("MarkAsSubmittedButton.Enabled for report version 2 of 2", false, form.MarkAsSubmittedButton_ForTestOnly.Enabled);

				form.ZMReport.SelectedVersion = "ALL";
				form.HandleButtonsAvailability(Report, "");
				AssertEquals("GenerateFileButton.Enabled for report version ALL of 2", false, form.GenerateButton_ForTestOnly.Enabled);
				AssertEquals("MarkAsSubmittedButton.Enabled for report version ALL of 2", false, form.MarkAsSubmittedButton_ForTestOnly.Enabled);
			}
		}

		public void TestFormDetails()
		{
			// Check if details fields have been correctly populated from the report
			CreateTaxReturn(1, Creator.Debtor, "FR", "987654", 1500, AccTaxReturn.Status.Submitted);
			CreateTaxReturn(2, Creator.Debtor1, "ES", "777888", 2200, AccTaxReturn.Status.Saved);
			using (ZMGermanyForm form = new ZMGermanyForm(Report))
			{
				form.Show();
				Application.DoEvents();
				AssertState(1, "FR", "987654", Creator.Debtor.OH_Code, Creator.Debtor.OH_FullName, Creator.Debtor.CityName, 1500);
				AssertState(2, "ES", "777888", Creator.Debtor1.OH_Code, Creator.Debtor1.OH_FullName, Creator.Debtor1.CityName, 2200);
				AssertEquals("Number of entries in version dropdown", 3, form.VersionDropEdit_ForTestOnly.List.Count);

				// filter on version 1
				form.VersionDropEdit_ForTestOnly.SelectItem("1");
				form.VersionDropEdit_ForTestOnly.OnItemSelected(form.VersionDropEdit_ForTestOnly.LastSelectedItem, true);
				AssertEquals("Number of rows shown in grid for version 1", 1, form.ReportLinesGrid_ForTestOnly.ListManager.Count);
				AssertState(1, "FR", "987654", Creator.Debtor.OH_Code, Creator.Debtor.OH_FullName, Creator.Debtor.CityName, 1500);

				// filter on version 2
				form.VersionDropEdit_ForTestOnly.SelectItem("2");
				form.VersionDropEdit_ForTestOnly.OnItemSelected(form.VersionDropEdit_ForTestOnly.LastSelectedItem, true);
				AssertEquals("Number of rows shown in grid for version 2", 1, form.ReportLinesGrid_ForTestOnly.ListManager.Count);
				AssertState(2, "ES", "777888", Creator.Debtor1.OH_Code, Creator.Debtor1.OH_FullName, Creator.Debtor1.CityName, 2200);

				void AssertState(int version, string expectedCountry, string expectedRegno,	string expectedOrgCode, string expectedOrgName, string expectedOrgCity, ZDecimal expectedAmount)
				{
					int gridRow;
					for (gridRow = 0; gridRow < form.ReportLinesGrid_ForTestOnly.ListManager.Count && (ZInt)form.ReportLinesGrid_ForTestOnly[gridRow, 0] != version; gridRow++)
					{
					}
					if (gridRow >= form.ReportLinesGrid_ForTestOnly.ListManager.Count)
					{
						Fail($"No grid row found for version {version}.");
						return;
					}
					AssertEquals("Country for version " + version, expectedCountry, (ZString)form.ReportLinesGrid_ForTestOnly[gridRow, 1]);
					AssertEquals("Registration number for version " + version, expectedRegno, (ZString)form.ReportLinesGrid_ForTestOnly[gridRow, 2]);
					AssertEquals("Org. code for version " + version, expectedOrgCode, (ZString)form.ReportLinesGrid_ForTestOnly[gridRow, 3]);
					AssertEquals("Org. name for version " + version, expectedOrgName, (ZString)form.ReportLinesGrid_ForTestOnly[gridRow, 4]);
					AssertEquals("Org. city for version " + version, expectedOrgCity, (ZString)form.ReportLinesGrid_ForTestOnly[gridRow, 5]);
					AssertEquals("Amount for version " + version, expectedAmount, (ZDecimal)form.ReportLinesGrid_ForTestOnly[gridRow, 6]);
				}
			}
		}

		public void TestCustomsCodes()
		{
			CreateTaxReturn(1, Creator.Debtor, "FR", "987654", 1500, AccTaxReturn.Status.Saved);
			using (ZMGermanyForm form = new ZMGermanyForm(Report))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Registration ID", "123456789", form.RegistrationIDTextBox_ForTestOnly.Text);
				AssertEquals("Sender ID", "ABCDEF", form.SenderIDTextBox_ForTestOnly.Text);
			}
		}

		public void TestGenerateButton()
		{
			const string expectedFilename = "m5_zm_ABCDEF_001EDI_v01_z20210301_m21095_ca.mgp";
			Creator.CreateTestPeriods(new ZDateTime(2021, 1, 1));

			var taxReturn = CreateTaxReturn(1, Creator.Debtor, "FR", "987654", 1500, AccTaxReturn.Status.Saved);
			using (ZMGermanyForm form = new ZMGermanyForm(Report))
			{
				form.Show();
				Application.DoEvents();
				var generateButton = FindButtonByName(form, "generateButton");
				var markAsSubmittedButton = FindButtonByName(form, "markAsSubmittedButton");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				EnableAndClickButton(generateButton);
				var docManager = Report.DocManagerInfo();
				AssertEquals("Report exported successfully", $"Number of new records: 1\r\nNumber of updated records: 0\r\n\r\nFile {expectedFilename} has been generated and attached to eDocs. Please submit this file to Fiscal Authorities now.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Number of files attached to eDocs", docManager.AllEDocs.Count, 1);
				AssertEquals("Name of the file attached to eDocs", docManager.AllEDocs[0].FileName, expectedFilename);
				AssertEquals("Report version is marked as generated", AccTaxReturn.Status.Generated, taxReturn.ATR_Status);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				EnableAndClickButton(generateButton);
				AssertEquals("Click Generate when there are unsubmitted reports", "Please submit your last generated file before generating a new version.", UnitTestUserNotification.Instance.LastMessage.Text);

				EnableAndClickButton(markAsSubmittedButton);
			}

			CreateTaxReturn(2, null, "", "", 0, AccTaxReturn.Status.Submitted);
			using (ZMGermanyForm form = new ZMGermanyForm(Report))
			{
				form.Show();
				Application.DoEvents();
				var generateButton = FindButtonByName(form, "generateButton");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				EnableAndClickButton(generateButton);
				var docManager = Report.DocManagerInfo();
				AssertEquals("Report exported again", "Selected version does not contain any records that differ from previous versions.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Number of files attached to eDocs after exporting again", docManager.AllEDocs.Count, 1);
				AssertEquals("Name of the file attached to eDocs after exporting again", docManager.AllEDocs[0].FileName, expectedFilename);
			}
		}

		AccTaxReturn CreateTaxReturn(int version, OrgHeader organisation, string countryCode, string businessRegNo, decimal amount, string status)
		{
			var taxReturnHeader = Factory.New<AccTaxReturn>();
			taxReturnHeader.ATR_ACR_ComplianceReport = Report.PK;
			taxReturnHeader.ATR_Version = version;
			taxReturnHeader.ATR_Status = status;
			taxReturnHeader.ATR_ReturnType = "ZMD";
			taxReturnHeader.ATR_VATRegNo = "123456789";
			taxReturnHeader.ATR_GovtReturnIdentifier = "ABCDEF";
			taxReturnHeader.ATR_SystemCreateUser = Env.CurrentUser.Initials;
			taxReturnHeader.ATR_SystemLastEditUser = Env.CurrentUser.Initials;

			if (organisation != null)
			{
				var taxReturnLine = taxReturnHeader.Lines.AddNew();
				taxReturnLine.ARL_RN_NKCountryCode = countryCode;
				taxReturnLine.ARL_OrgRegNo = businessRegNo;
				taxReturnLine.ARL_TotalAmountIncludingTax = amount;
				taxReturnLine.ARL_OH_Organisation = organisation.PK;
				taxReturnLine.ARL_OrgName = organisation.OH_FullName;
				taxReturnLine.ARL_City = organisation.CityName;
				taxReturnLine.ARL_Comment = organisation.OH_Code;
			}

			return taxReturnHeader;
		}

		Button FindButtonByName(Form form, string name)
		{
			return form.Find((c) => c.Name == name).Single() as Button;
		}

		void EnableAndClickButton(Button button)
		{
			button.Enabled = true;
			button.PerformClick();
		}
	}

	[TestedType(typeof(ZMGermanyForm))]
	public class ZMDFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_ReportType = ComplianceReportTypes.ZusammenfassendeMeldungGermanyReportType;
			return new ZMGermanyForm(complianceReport);
		}
	}
}
