using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.DocBuilder.SectionEditing.Testing
{
	sealed class CustomizedDocumentElementsFactoryTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var factory = new CustomizedDocumentElementsTemplateCreator(Factory);
			var template = factory.Create();
			AssertEquals("template.SO_Name", "Customized Document Elements", template.SO_Name);
			AssertEquals("template.SO_DataContext", nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob), template.SO_DataContext);
			AssertEquals("template.SO_ExcelTemplatePath", "Customized Document Elements.XLS", template.SO_ExcelTemplatePath);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);

				AssertMultilineASCIIEquals("Document Work Sheet",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements]
{A}-[HideColumnIf]   {BU}-[1==1]   {BV}-[1==1]   {BW}-[1==1]   {BX}-[1==1]   {BY}-[1==1]   {BZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#EndOfReport]", excelInterface.WorkSheets[0].ToString());

				AssertEquals("No Translations sheet in base template", 1, excelInterface.WorkSheets.Count);
			}
		}

		public void TestCreateForEnglishLanguage()
		{
			var factory = new CustomizedDocumentElementsTemplateCreator(Factory);
			var template = factory.Create(Enterprise.Core.SharedConstants.Languages.English);
			AssertEquals("template.SO_Name", "Customized Document Elements", template.SO_Name);
			AssertEquals("template.SO_DataContext", nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob), template.SO_DataContext);
			AssertEquals("template.SO_ExcelTemplatePath", "Customized Document Elements.XLS", template.SO_ExcelTemplatePath);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);

				AssertMultilineASCIIEquals("Document Work Sheet",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements]
{A}-[HideColumnIf]   {BU}-[1==1]   {BV}-[1==1]   {BW}-[1==1]   {BX}-[1==1]   {BY}-[1==1]   {BZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#EndOfReport]", excelInterface.WorkSheets[0].ToString());

				AssertEquals("No Translations sheet in base template", 1, excelInterface.WorkSheets.Count);
			}
		}

		public void TestCreateForOtherLanguage()
		{
			var factory = new CustomizedDocumentElementsTemplateCreator(Factory);
			var template = factory.Create(Enterprise.Core.SharedConstants.Languages.ChineseTraditional);
			AssertEquals("template.SO_Name", "Customized Document Elements [ZH-TW]", template.SO_Name);
			AssertEquals("template.SO_DataContext", nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob), template.SO_DataContext);
			AssertEquals("template.SO_ExcelTemplatePath", "Customized Document Elements [ZH-TW].XLS", template.SO_ExcelTemplatePath);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);

				AssertMultilineASCIIEquals("Document Work Sheet",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [ZH-TW]]
{A}-[HideColumnIf]   {BU}-[1==1]   {BV}-[1==1]   {BW}-[1==1]   {BX}-[1==1]   {BY}-[1==1]   {BZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#EndOfReport]", excelInterface.WorkSheets[0].ToString());

				AssertEquals("No Translations sheet in base template", 1, excelInterface.WorkSheets.Count);
			}
		}
	}
}
