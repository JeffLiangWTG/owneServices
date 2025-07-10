using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.ComplianceReport.PTRS.Testing
{
	[TestedType(typeof(ImportABNsForm))]
	public class ImportABNsBasherTest : ZFormBasherTest
	{
		public void TestLoad()
		{
			using (var form = NewImportABNsForm(out _))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("IsReadOnlyProgressTextBox", true, form.ProgressTextBox_ForTestOnly.ReadOnly);
				AssertEquals("Form caption should be correct", "Accounting ABN Small Business List Import", form.Text);
			}
		}

		protected override Form GetFormToBashCore() => NewImportABNsForm(out _);

		public void TestImportFromFileThrowsException()
		{
			using (var tempFile = TempFile.New())
			using (var form = NewImportABNsForm(out _))
			{
				File.WriteAllText(tempFile.Filename, "Blaticus", System.Text.Encoding.UTF8);

				form.ImportFileException_ForTestOnly = () =>
				{
					throw new Exception("Ka-BOOM!");
				};
				form.Show();
				Application.DoEvents();

				AssertEquals("PreCondition", false, form.SubmitButton_ForTestOnly.Enabled);
				AssertEquals("PreCondition", true, form.ImportFromFileButton_ForTestOnly.Enabled);
				AssertEquals("precondition:", 0, ExceptionReporterTestListener.Instance.Count);

				form.ImportFromFile_ForTestOnly(tempFile.Filename);
				AssertEquals(false, form.SubmitButton_ForTestOnly.Enabled);
				AssertEquals(false, form.ImportFromFileButton_ForTestOnly.Enabled);
				AssertEquals("should have reported exception", 1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("should have reported exception", "Ka-BOOM!", ExceptionReporterTestListener.Instance[0].Message);
				ErrorReporter.Clear();

				string expected =
					$@"Importing data from file [{tempFile.Filename}]...
Error: Ka-BOOM!
";

				AssertMultilineASCIIEquals("Log output", expected, form.ProgressTextBox_ForTestOnly.Text.Trim());
			}
		}

		public void TestImportFromFileNotThrowsException()
		{
			using (var tempFile = TempFile.New())
			using (var form = NewImportABNsForm(out _))
			{
				form.Show();
				AssertEquals("PreCondition", false, form.SubmitButton_ForTestOnly.Enabled);
				AssertEquals("PreCondition", true, form.ImportFromFileButton_ForTestOnly.Enabled);
				Application.DoEvents();

				File.WriteAllText(tempFile.Filename, @"Blaticus", System.Text.Encoding.UTF8);

				form.ImportFromFile_ForTestOnly(tempFile.Filename);
				AssertEquals(true, form.SubmitButton_ForTestOnly.Enabled);
				AssertEquals(false, form.ImportFromFileButton_ForTestOnly.Enabled);
				AssertEquals("should have no reported exception", "None", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());

				string expected =
					$@"Importing data from file [{tempFile.Filename}]...
Skip line 1 because it's not a valid ABN: Blaticus
There is no valid ABN in this file.
";

				AssertMultilineASCIIEquals("Log output", expected, form.ProgressTextBox_ForTestOnly.Text.Trim());
			}
		}

		public void TestImportFromFileWorking()
		{
			var reportingPeriod = new ZDateTime(2021, 6, 30);
			var cusCodeCanImportABN = TestObjectCreator.Creditor1.CustomsCodes.AddNew();
			cusCodeCanImportABN.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			cusCodeCanImportABN.OK_CustomsRegNo = "85126029299";
			cusCodeCanImportABN.OK_RN_NKCodeCountry = CountryCodes.Australia;

			var cusCodeDuplicatedImportABN = TestObjectCreator.Creditor2.CustomsCodes.AddNew();
			cusCodeDuplicatedImportABN.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			cusCodeDuplicatedImportABN.OK_CustomsRegNo = "31623703971";
			cusCodeDuplicatedImportABN.OK_RN_NKCodeCountry = CountryCodes.Australia;

			var requiredDocumentDuplicatedImportABN = cusCodeDuplicatedImportABN.Organisation.RequiredDocuments.AddNew();
			requiredDocumentDuplicatedImportABN.EQ_DocCategory = ReferenceTypes.ComplianceReport;
			requiredDocumentDuplicatedImportABN.EQ_DocType = AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines;
			requiredDocumentDuplicatedImportABN.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			requiredDocumentDuplicatedImportABN.EQ_DocPeriod = JobRequiredDocuments.DocumentPeriods.Periodic;
			requiredDocumentDuplicatedImportABN.EQ_ValidToDate = reportingPeriod;
			requiredDocumentDuplicatedImportABN.EQ_DateReceived = requiredDocumentDuplicatedImportABN.EQ_ValidToDate.ToDateTimeOffset(null);
			requiredDocumentDuplicatedImportABN.EQ_RN_NKRelatedCountry = CountryCodes.Australia;

			TestObjectCreator.Factory.Save();

			using (var tempFile = TempFile.New())
			using (var form = NewImportABNsForm(out var report))
			{
				report.ACR_DateTo = reportingPeriod.Date;

				form.Show();

				Application.DoEvents();

				File.WriteAllText(tempFile.Filename, @"ABN
dummydata
85126029203
85126029299
31623703971
", System.Text.Encoding.UTF8);
				form.ImportFromFile_ForTestOnly(tempFile.Filename);
				AssertEquals("should have no reported exception", "None", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());

				string expected =
					$@"Importing data from file [{tempFile.Filename}]...
Skip line 2 because it's not a valid ABN: dummydata

Below organizations / ABNs had document tracking record added:
85126029299 - ZCreditor1

Below organizations / ABNs already had document tracking record created for given reporting period:
31623703971 - ZCreditor2

Below ABNs did not have matched organization:
85126029203
";

				AssertMultilineASCIIEquals("Log output", string.Format(expected, tempFile.Filename).Trim(), form.ProgressTextBox_ForTestOnly.Text.Trim());
			}
		}

		[TestDate(2021, 07, 30)]
		public void TestSubmitButton()
		{
			var reportingPeriod = new ZDateTime(2021, 6, 30);
			var cusCodeCanImportABN = TestObjectCreator.Creditor1.CustomsCodes.AddNew();
			cusCodeCanImportABN.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			cusCodeCanImportABN.OK_CustomsRegNo = "85126029299";
			cusCodeCanImportABN.OK_RN_NKCodeCountry = CountryCodes.Australia;

			using (var tempFile = TempFile.New())
			using (var form = NewImportABNsForm(out var report))
			{
				var formClosed = false;
				form.FormClosed += (s, e) => formClosed = true;
				form.Show();
				Application.DoEvents();

				File.WriteAllText(tempFile.Filename, @"ABN
dummydata
85126029299
", System.Text.Encoding.UTF8);

				form.ImportFromFile_ForTestOnly(tempFile.Filename);
				AssertEquals("should have no reported exception", "None", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());

				var submitButton = (ZButton)form.Controls.Find("SubmitButton", true)[0];
				submitButton.PerformClick();
				using (var msg = ZFormModaliser.LastFormShownDialogForTest as ZMessageBox)
				{
					AssertNotNull(msg);
					AssertEquals("ABNs are imported successfully", msg.Text);
					AssertEquals("Please re-queue Compliance Report to see the effect of imported ABNs", msg.MessageMultilingual);
				}
				AssertEquals(true, formClosed);

				var newFactory = new BusinessObjectFactory();
				var reloadReport = newFactory.Load<AccComplianceReport>(report.PK);
				IDocManagerSupport docSupport = reloadReport;
				AssertEquals(1, docSupport.DocManagerInfo.EDocsView.Count);

				AssertContains("SBI Tool ABN Small Bus List", docSupport.DocManagerInfo.EDocsView[0].FileName);
				AssertEquals("csv", docSupport.DocManagerInfo.EDocsView[0].DataType);
				AssertEquals("SBI Tool ABN Small Bus List", docSupport.DocManagerInfo.EDocsView[0].Description);
				AssertEquals(Enterprise.Core.Constants.RefDocTypes.MiscellaneousDocument, docSupport.DocManagerInfo.EDocsView[0].DocType);

				var creditorReload = newFactory.Load<OrgHeader>(TestObjectCreator.Creditor1.PK);
				AssertEquals("Submit will save change", 1, creditorReload.RequiredDocuments.Count);
				CombineAssertions("Submit will save change", () => {
					AssertEquals(nameof(JobRequiredDocument.EQ_DocCategory),ReferenceTypes.ComplianceReport, creditorReload.RequiredDocuments[0].EQ_DocCategory);
					AssertEquals(nameof(JobRequiredDocument.EQ_DocType),AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines, creditorReload.RequiredDocuments[0].EQ_DocType);
					AssertEquals(nameof(JobRequiredDocument.EQ_DocUsage),JobRequiredDocument.DocUsage.Creditor, creditorReload.RequiredDocuments[0].EQ_DocUsage);
					AssertEquals(nameof(JobRequiredDocument.EQ_DocPeriod),JobRequiredDocuments.DocumentPeriods.Periodic, creditorReload.RequiredDocuments[0].EQ_DocPeriod);
					AssertEquals(nameof(JobRequiredDocument.EQ_ValidToDate),report.ACR_DateTo, creditorReload.RequiredDocuments[0].EQ_ValidToDate);
					AssertEquals(nameof(JobRequiredDocument.EQ_DateReceived), new ZDateTimeOffset(2021, 07, 30), creditorReload.RequiredDocuments[0].EQ_DateReceived);
					AssertEquals(nameof(JobRequiredDocument.EQ_RN_NKRelatedCountry),CountryCodes.Australia, creditorReload.RequiredDocuments[0].EQ_RN_NKRelatedCountry);
				});
			}
		}

		public void TestCloseButton()
		{
			using (var tempFile = TempFile.New())
			using (var form = NewImportABNsForm(out var report))
			{
				var formClosed = false;
				form.FormClosed += (s, e) => formClosed = true;
				form.Show();
				Application.DoEvents();

				File.WriteAllText(tempFile.Filename, @"ABN
dummydata
85126029203
", System.Text.Encoding.UTF8);

				form.ImportFromFile_ForTestOnly(tempFile.Filename);
				AssertEquals("should have no reported exception", "None", UnitTestUserNotification.Instance.LastMessage.ToString().Trim());

				var closeButton = (ZButton)form.Controls.Find("CloseButton", true)[0];
				closeButton.PerformClick();
				AssertEquals(true, formClosed);

				var newFactory = new BusinessObjectFactory();
				var reloadReport = newFactory.Load<AccComplianceReport>(report.PK);
				IDocManagerSupport docSupport = reloadReport;
				AssertEquals("Close form will not have any change ", 0, docSupport.DocManagerInfo.EDocsView.Count);

				var creditorReload = newFactory.Load<OrgHeader>(TestObjectCreator.Creditor1.PK);
				AssertEquals("Close form will not have any change ", 0, creditorReload.RequiredDocuments.Count);
			}
			Assert(true);
		}

		#region Implementation

		ImportABNsForm NewImportABNsForm(out AccComplianceReport report)
		{
			report = TestObjectCreator.CreateComplianceReport(AccComplianceReport.ReportTypes.SAFT, AccComplianceReport.Status.ReportCreated);
			TestObjectCreator.Factory.Save();
			return (ImportABNsForm)Activator.CreateInstance(FormToBashType, report);
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "ProgressTextBox";
		}

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
