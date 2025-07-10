using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	sealed class TemplateEditorTest : TestCaseWithFactory
	{
		[GuiTest]
		public void TestEdit()
		{
			using (var form = new Form())
			{
				var tempFileName = Temp.GetTempFileName();
				try
				{
					var template = Factory.New<StmTemplateBase>();
					template.SO_IsSystemDefined = ZBool.True;
					template.SO_Name = "Unit Test";
					template.SO_ExcelTemplatePath = tempFileName;
					template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromStringAndSaveToFile(
	@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]", tempFileName);
					template.IsCheckedOutByMe = false;

					var dummyExcelManager = new DummyExcelManager();
					dummyExcelManager.IsSupported = true;

					var templateEditor = new TemplateEditor(template, form);
					templateEditor.SetExcelManagerForTesting(dummyExcelManager);
					templateEditor.Edit();
					AssertEquals("dummyExcelManager.log.Count", 1, dummyExcelManager.log.Count);
					var result = string.Join(System.Environment.NewLine, dummyExcelManager.log.ToArray());
					AssertEquals(string.Format("Working file [{0}] edited at Row: -1 Column: -1", tempFileName), result);

					dummyExcelManager = new DummyExcelManager();
					dummyExcelManager.IsSupported = false;
					templateEditor = new TemplateEditor(template, form);
					templateEditor.SetExcelManagerForTesting(dummyExcelManager);
					templateEditor.Edit();
					AssertEquals("dummyExcelManager.log.Count", 0, dummyExcelManager.log.Count);
					AssertType(typeof(FileSaveToOpenForm), ZFormModaliser.LastFormShownDialogForTest);
				}
				finally
				{
					File.Delete(tempFileName);
				}
			}
		}

		[GuiTest]
		[RequiresSTA]
		public void TestEditWithChangingDataContext()
		{
			var dummy = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var customisation = DocumentMenuCustomisation.New(dummy, null);

			using (var form = new DocumentCustomisationForm(customisation))
			{
				var tempFileName = Temp.GetTempFileName();
				try
				{
					var template = Factory.New<StmTemplateBase>();
					template.SO_IsSystemDefined = ZBool.True;
					template.SO_Name = "Unit Test";
					template.SO_DataContext = "Organisation";
					template.SO_ExcelTemplatePath = tempFileName;
					template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromStringAndSaveToFile(
	@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]", tempFileName);
					template.IsCheckedOutByMe = false;

					var dummyExcelManager = new DummyExcelManager();
					dummyExcelManager.IsSupported = true;

					var templateEditor = new TemplateEditor(template, form);
					templateEditor.SetExcelManagerForTesting(dummyExcelManager);
					templateEditor.Edit();
					AssertEquals(@"The template has following error(s) after modification:
This template cannot be added because it does not have a specified data context.

Click YES to fix the error or click NO to discard the changes (If you have made significant changes in the template, it is recommended to click YES and backup your modifications first.)", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Template not updated", "Organisation", template.SO_DataContext);
					UnitTestUserNotification.Instance.ClearMessages();

					template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromStringAndSaveToFile(
@"{A}-[#Config]
{A}-[DataContext=Consol]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]", tempFileName);
					templateEditor = new TemplateEditor(template, form);
					templateEditor.SetExcelManagerForTesting(dummyExcelManager);
					templateEditor.Edit();
					AssertEquals(@"The template has following error(s) after modification:
This template cannot be added because it requires a data context of 'Consol', but the form only supports 'Organisation, OrgSupplierLink, OrgBuyerLink, Notes, OrganisationIRS1099, GenericFreightJob, .OrgHeader, .Business.OrgHeader, .MasterFiles.Business.OrgHeader, .OrgHeader, .Business.OrgHeader, .MasterFiles.Business.OrgHeader, .TWLetterOfAuthorization'.

Click YES to fix the error or click NO to discard the changes (If you have made significant changes in the template, it is recommended to click YES and backup your modifications first.)", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Template not updated", "Organisation", template.SO_DataContext);
					UnitTestUserNotification.Instance.ClearMessages();

					template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromStringAndSaveToFile(
@"{A}-[#Config]
{A}-[DataContext=.OrgHeader]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]", tempFileName);
					templateEditor = new TemplateEditor(template, form);
					templateEditor.SetExcelManagerForTesting(dummyExcelManager);
					templateEditor.Edit();
					AssertEquals("The data context of this template is changed from Organisation to .OrgHeader", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Template updated", ".OrgHeader", template.SO_DataContext);
				}
				finally
				{
					File.Delete(tempFileName);
				}
			}
		}

		[GuiTest]
		public void TestDoNotValidateNorUpdateDataContextForForms()
		{
			var tempFileName = Temp.GetTempFileName();

			var templateContent = DocumentEngineTestHelper.CreateTemplateFromStringAndSaveToFile(
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#Body]
{B}-[Hello World]
{A}-[#End]", tempFileName);

			var template = Factory.New<StmTemplateBase>();
			template.SO_IsSystemDefined = ZBool.True;
			template.SO_TemplateType = StmTemplateTypes.Codes.Form;
			template.SO_Name = "Unit Test";
			template.SO_DataContext = "UXML";
			template.SO_ExcelTemplatePath = tempFileName;
			template.SO_Template = templateContent;

			var dummy = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var customisation = DocumentMenuCustomisation.New(dummy, null);

			using (var form = new DocumentCustomisationForm(customisation))
			{
				try
				{
					var dummyExcelManager = new DummyExcelManager();
					dummyExcelManager.IsSupported = true;

					var templateEditor = new TemplateEditor(template, form);
					templateEditor.SetExcelManagerForTesting(dummyExcelManager);
					templateEditor.Edit();

					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertEquals("Template SO_DataContext was not updated", "UXML", template.SO_DataContext);
				}
				finally
				{
					UnitTestUserNotification.Instance.ClearMessages();
					File.Delete(tempFileName);
				}
			}
		}

		[GuiTest]
		public void TestCloseEditingFormHandlesTemplateFileReadException()
		{
			using (var form = new Form())
			{
				var tempFileName = Temp.GetTempFileName();
				try
				{
					using (var stream = File.Open(tempFileName, FileMode.Open))
					{
						var template = Factory.New<StmTemplateBase>();
						template.SO_IsSystemDefined = ZBool.True;
						template.SO_Name = "Unit Test";
						template.SO_ExcelTemplatePath = tempFileName;
						template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
							@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
						template.IsCheckedOutByMe = false;

						var manager = new DummyExcelManager();
						manager.IsSupported = true;

						var editor = new TemplateEditor(template, form);
						editor.SetExcelManagerForTesting(manager);
						editor.Edit();

						Assert(UnitTestUserNotification.Instance.LastMessage.Text.EndsWith(@"This file is currently used by another application.

Please make sure Excel is closed and the file is not locked by any processes before closing the dialog."));
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}
				}
				finally
				{
					File.Delete(tempFileName);
				}
			}
		}

		[GuiTest]
		public void TestEdit_DeletedTemplate()
		{
			using (var form = new Form())
			{
				var userNotification = UnitTestUserNotification.Instance;
				var prevMessagesLength = userNotification.PreviousMessages.Length;

				var tempFileName = Temp.GetTempFileName();
				try
				{
					var template = Factory.New<StmTemplateBase>();
					template.SO_IsSystemDefined = ZBool.True;
					template.SO_Name = "Unit Test";
					template.SO_ExcelTemplatePath = tempFileName;
					template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
	@"{A}-[#Config]
	{A}-[Name=Test]
	{A}-[#SectionBody]
	{B}-[Hello World]
	{A}-[#EndOfReport]");
					template.IsCheckedOutByMe = false;

					var dummyExcelManager = new DummyExcelManager();
					dummyExcelManager.IsSupported = true;
					dummyExcelManager.ExcelClosed += result => template.Delete();

					var templateEditor = new TemplateEditor(template, form);
					templateEditor.SetExcelManagerForTesting(dummyExcelManager);
					templateEditor.Edit();

					AssertEquals("Should only have 1 new Error Message", 1, userNotification.PreviousMessages.Length - prevMessagesLength);
					AssertEquals("Error Message of deleted-template should be displayed", UnitTestUserNotification.Instance.LastMessage.Text, "Error Building Template: Template has been deleted.");
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
				finally
				{
					File.Delete(tempFileName);
				}
			}
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestNoEndFileMarker()
		{
			using (var tempFileName = TempFile.New())
			using (var form = new Form())
			{
				var excelTemplate = new ExcelTemplateForUnitTesting("UDF without end marker.xls", TestFilesSubFolder.ReportTestFiles);
				var excelTemplateFilePath = Path.Combine(BaseSourcePath, excelTemplate.TemplateSourceLocation);
				File.Copy(excelTemplateFilePath, tempFileName.Filename, true);

				var template = Factory.New<StmTemplateBase>();
				template.SO_IsSystemDefined = ZBool.True;
				template.SO_Name = "Unit Test";
				template.SO_ExcelTemplatePath = tempFileName.Filename;

				var manager = new DummyExcelManager();
				manager.IsSupported = true;

				var editor = new TemplateEditor(template, form);
				editor.SetExcelManagerForTesting(manager);
				editor.Edit();

				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(@"There was no #End marker in the sheet"));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		sealed class DummyExcelManager : IExcelManager
		{
			internal DummyExcelManager()
			{
				this.log = new List<string>();
			}
			public readonly List<string> log;

			public void Edit(string workingFile, int row, int column)
			{
				log.Add(string.Format("Working file [{0}] edited at Row: {1} Column: {2}", workingFile, row, column));
				ExcelClosed(DialogResult.OK);
			}

			public event ExcelClosedEventHandler ExcelClosed;

			public bool IsSupported
			{
				get;
				set;
			}
		}
	}
}
