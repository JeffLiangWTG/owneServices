using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.DocBuilder.SectionEditing.Testing
{
	sealed class DocBuilderTemplateBuilderTest : TestCaseWithFactory
	{
		public void TestRemoveSection()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section 1]
{B}-[My Section 1]
{A}-[#ConfigurableSection:GEN, My Section 2]
{B}-[My Section 2]
{A}-[#ConfigurableSection:GEN, My Section 3]
{B}-[My Section 3]
{A}-[#EndOfReport]");

			var template = Factory.Load<StmTemplateBase>(helper.CreateTemplate(Factory, "Customized Document Elements [ZH-CN]").PK);
			var builder = new DocBuilderTemplateBuilder(template);
			builder.RemoveSection("My Section 2");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);

				AssertMultilineASCIIEquals("My Section 2 should be removed.",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section 1]
{B}-[My Section 1]
{A}-[#ConfigurableSection:GEN, My Section 3]
{B}-[My Section 3]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestCopySectionAndInsertAtEnd()
		{
			var customizedDocumentElementsFactory = new CustomizedDocumentElementsTemplateCreator(Factory);
			var targetTemplate = customizedDocumentElementsFactory.Create("ZH-CN");
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(targetTemplate.SO_Template);

				AssertMultilineASCIIEquals("Pre-condition: Base DocBuilder Template",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [ZH-CN]]
{A}-[HideColumnIf]   {BU}-[1==1]   {BV}-[1==1]   {BW}-[1==1]   {BX}-[1==1]   {BY}-[1==1]   {BZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}

			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section]
{B}-[My Section]
{A}-[#EndOfReport]");

			var sourceTemplate = Factory.Load<StmTemplateBase>(helper.CreateTemplate(Factory, "Customized Document Elements [ZH-TW]").PK);

			var builder = new DocBuilderTemplateBuilder(targetTemplate);
			builder.CopySectionAndInsertAtEnd(sourceTemplate, "My Section");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(targetTemplate.SO_Template);

				AssertMultilineASCIIEquals("My Section should be copied over.",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [ZH-CN]]
{A}-[HideColumnIf]   {BU}-[1==1]   {BV}-[1==1]   {BW}-[1==1]   {BX}-[1==1]   {BY}-[1==1]   {BZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#ConfigurableSection:GEN, My Section]
{B}-[My Section]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestCopySectionAndInsertAtEnd_WhenNoEndOfReportSectionExists_ShouldInsertAfterLastSection()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [ZH-CN]]
{A}-[HideColumnIf]   {AV}-[1==1]   {AW}-[1==1]   {AX}-[1==1]   {AY}-[1==1]   {AZ}-[1==1]   {BA}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]");
			var targetTemplate = helper.CreateTemplate(Factory, "Customized Document Elements [ZH-CN]");
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(targetTemplate.SO_Template);

				AssertMultilineASCIIEquals("Pre-condition: Base DocBuilder Template",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [ZH-CN]]
{A}-[HideColumnIf]   {AB}-[1==1]   {AV}-[1==1]   {AW}-[1==1]   {AX}-[1==1]   {AY}-[1==1]   {AZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]",
					excelInterface.WorkSheets[0].ToString());
			}

			helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section1]
{B}-[My Section1]");

			var sourceTemplate = Factory.Load<StmTemplateBase>(helper.CreateTemplate(Factory, "Customized Document Elements [ZH-TW]").PK);

			var builder = new DocBuilderTemplateBuilder(targetTemplate);
			builder.CopySectionAndInsertAtEnd(sourceTemplate, "My Section1");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(targetTemplate.SO_Template);

				AssertMultilineASCIIEquals("My Section should be copied over.",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [ZH-CN]]
{A}-[HideColumnIf]   {AB}-[1==1]   {AV}-[1==1]   {AW}-[1==1]   {AX}-[1==1]   {AY}-[1==1]   {AZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#ConfigurableSection:GEN, My Section1]
{B}-[My Section1]",
					excelInterface.WorkSheets[0].ToString());
			}

			helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section2]
{B}-[My Section2]");

			sourceTemplate = Factory.Load<StmTemplateBase>(helper.CreateTemplate(Factory, "Customized Document Elements [ZH-TW]").PK);

			builder = new DocBuilderTemplateBuilder(targetTemplate);
			builder.CopySectionAndInsertAtEnd(sourceTemplate, "My Section2");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(targetTemplate.SO_Template);

				AssertMultilineASCIIEquals("My Section should be copied over.",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [ZH-CN]]
{A}-[HideColumnIf]   {AB}-[1==1]   {AV}-[1==1]   {AW}-[1==1]   {AX}-[1==1]   {AY}-[1==1]   {AZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#ConfigurableSection:GEN, My Section1]
{B}-[My Section1]
{A}-[#ConfigurableSection:GEN, My Section2]
{B}-[My Section2]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestCopySectionAndInsertAt()
		{
			var customizedDocumentElementsFactory = new CustomizedDocumentElementsTemplateCreator(Factory);
			var targetTemplate = customizedDocumentElementsFactory.Create("ZH-CN");
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(targetTemplate.SO_Template);

				AssertMultilineASCIIEquals("Pre-condition: Base DocBuilder Template",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [ZH-CN]]
{A}-[HideColumnIf]   {BU}-[1==1]   {BV}-[1==1]   {BW}-[1==1]   {BX}-[1==1]   {BY}-[1==1]   {BZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}

			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section]
{B}-[My Section]
{A}-[#EndOfReport]");

			var sourceTemplate = Factory.Load<StmTemplateBase>(helper.CreateTemplate(Factory, "Customized Document Elements [ZH-TW]").PK);

			var builder = new DocBuilderTemplateBuilder(targetTemplate);
			builder.CopySectionAndInsertAt(sourceTemplate, "My Section", 4);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(targetTemplate.SO_Template);

				AssertMultilineASCIIEquals("My Section should be copied over.",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [ZH-CN]]
{A}-[HideColumnIf]   {BU}-[1==1]   {BV}-[1==1]   {BW}-[1==1]   {BX}-[1==1]   {BY}-[1==1]   {BZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[#ConfigurableSection:GEN, My Section]
{B}-[My Section]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestConstructor_WithNullTemplate()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new DocBuilderTemplateBuilder(null));
		}

		public void TestCopySectionAndInsertAt_WithNullSourceTemplate()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new DocBuilderTemplateBuilder(Factory.New<StmTemplateBase>()).CopySectionAndInsertAt(null, "", 0));
		}

		public void TestCopySectionAndInsertAt_WithNoDocumentWorksheet()
		{
			var customizedDocumentElementsFactory = new CustomizedDocumentElementsTemplateCreator(Factory);
			var targetTemplate = customizedDocumentElementsFactory.Create("ZH-CN");
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(targetTemplate.SO_Template);

				AssertMultilineASCIIEquals("Pre-condition: Base DocBuilder Template",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [ZH-CN]]
{A}-[HideColumnIf]   {BU}-[1==1]   {BV}-[1==1]   {BW}-[1==1]   {BX}-[1==1]   {BY}-[1==1]   {BZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}

			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("NOT Document!!1",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section]
{B}-[My Section]
{A}-[#EndOfReport]");

			var sourceTemplate = Factory.Load<StmTemplateBase>(helper.CreateTemplate(Factory, "Customized Document Elements [ZH-TW]").PK);

			var builder = new DocBuilderTemplateBuilder(sourceTemplate);
			AssertExceptionThrown(typeof(MissingWorksheetException), "Could not find worksheet named [Document] in template [Customized Document Elements [ZH-TW]].", () => builder.CopySectionAndInsertAt(sourceTemplate, "My Section", 4));
		}

		public void TestCopySectionAndInsertAtEndWithTooManyRows()
		{
			using (Globals.SetIsUserInteractiveForTest(true))
			using (Globals.SetIsWebForTest(false))
			using (Globals.SetIsConsoleSessionForTest(false))
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var template = Factory.New<StmTemplateBase>();
				template.SO_DataContext = "GenericFreightJob";
				template.SO_Name = Core.Constants.SectionRepositoryTemplateNames.User;
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.Customized Document Elements System Empty With Too Many Rows.xls", "Customized Document Elements System Empty With Too Many Rows.xls");
				var excelTemplate = new ExcelTemplateForUnitTesting("Customized Document Elements System Empty With Too Many Rows.xls", Path.GetFullPath(tempFileName));
				template.SO_Template = excelTemplate.GetAsByteArray();
				var builder = new DocBuilderTemplateBuilder(template);

				var helper = new TemplateTestHelper();
				helper.AddWorkSheet("Document",
					@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section]
{B}-[My Section]
{A}-[#EndOfReport]");

				var sourceTemplate = Factory.Load<StmTemplateBase>(helper.CreateTemplate(Factory, "Customized Document Elements [ZH-CN]").PK);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				if (System.Environment.UserInteractive)
				{
					AssertNoExceptionThrown(() => builder.CopySectionAndInsertAtEnd(sourceTemplate, "My Section"));
					AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text,
					ExcelLimitationsHelper.Messages.GetTooManyForExcel2003WithFormatSwitchQuestion(
						new DocumentEngineTooManyRowsForThisFileFormatException()));
				}

				template.SO_Template = excelTemplate.GetAsByteArray();
				builder = new DocBuilderTemplateBuilder(template);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				AssertExceptionThrown(typeof(DocumentEngineTooManyRowsForThisFileFormatException),
					() => builder.CopySectionAndInsertAtEnd(sourceTemplate, "My Section"));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}
	}
}
