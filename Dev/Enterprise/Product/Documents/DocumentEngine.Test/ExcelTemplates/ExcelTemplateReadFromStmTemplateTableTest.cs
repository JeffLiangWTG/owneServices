namespace Enterprise.ExcelTemplates.Testing
{
	sealed class ExcelTemplateReadFromStmTemplateTableTest : BaseExcelTemplateTest
	{
		public override void TestTemplateNameAndTemplateSourceLocation()
		{
			ExcelTemplateReadFromStmTemplateTable excelTemplate = new ExcelTemplateReadFromStmTemplateTable(new TestStmTemplate());
			AssertEquals("excelTemplate.TemplateName", "TestTemplate", excelTemplate.TemplateName);
			AssertEquals("excelTemplate.TemplateSourceLocation", "TestTemplate.xls", excelTemplate.TemplateSourceLocation);
		}

		public override void TestGetAsStreamVsGetAsByteArray()
		{
			ExcelTemplate excelTemplate = new ExcelTemplateReadFromStmTemplateTable(new TestStmTemplate());
			AssertStreamAndByteArrayExpectedLength(excelTemplate, 4);
		}
	}
}
