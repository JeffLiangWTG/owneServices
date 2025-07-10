using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class TextBuilderTest : FilterBuilderTestWithTempFile
	{
		public void TestFilterMethodStartsWith()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet(@"Document",
@"{A}-[#Config]
{A}-[#EndOfReport]");
			helper.AddWorkSheet(@"Filters",
@"{A}-[Test Filter]
{B}-[Type]    {C}-[Text]
{B}-[Field]    {C}-[Z0_VarCharMax]
{B}-[FilterMethod]    {C}-[StartsWith]
{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Test Report");
			var excelTemplate = template.GetExcelTemplate();

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, excelTemplate))
				{
					report.UpdateAndSynchroniseFilters();

					var field = (TextField)report.FilterCollection["Test Filter"];

					AssertEquals("FilterMethod should be StartsWith.", TextFieldFilterMethodList.Codes.StartsWith, field.FilterMethod);
				}
			}
		}

		public void TestFilterMethodContains()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet(@"Document",
@"{A}-[#Config]
{A}-[#EndOfReport]");
			helper.AddWorkSheet(@"Filters",
@"{A}-[Test Filter]
{B}-[Type]    {C}-[Text]
{B}-[Field]    {C}-[Z0_VarCharMax]
{B}-[FilterMethod]    {C}-[Contains]
{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Test Report");
			var excelTemplate = template.GetExcelTemplate();

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, excelTemplate))
				{
					report.UpdateAndSynchroniseFilters();

					var field = (TextField)report.FilterCollection["Test Filter"];

					AssertEquals("FilterMethod should be Contains.", TextFieldFilterMethodList.Codes.Contains, field.FilterMethod);
				}
			}
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefault()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TextFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var analyser = new ReportAnalyser(report);
				var builder = new FilterCollectionBuilder(analyser.DataSourceParameters, analyser.ValidatorPack, new StringTreeBuilder(report.FilterSheet).GetTree(), DummyEvaluator);

				builder.Build();

				AssertEquals("Number of fields", 2, builder.IFilterCollection.Count);
				AssertEquals("Field with default ", "Hello", ((TextField)(builder.IFilterCollection[1])).Value);
			}
		}
	}
}
