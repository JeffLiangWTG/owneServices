using CargoWise.Types;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class DateRangeBuilderTest : FilterBuilderTestWithTempFile
	{
		public void TestRequireBothFromAndToDates()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet(@"Document",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			helper.AddWorkSheet(@"Filters",
@"{A}-[My Filter]    {B}-[Type]    {C}-[Date Range]
{B}-[Field]    {C}-[Z0_Date]
{B}-[RequireBothFromAndToDates]
{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Dummy Report");
			var excelTemplate = template.GetExcelTemplate();

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.UpdateAndSynchroniseFilters();

					var field = (DateRangeField)report.FilterCollection["My Filter"];

					AssertEquals(true, field.RequireBothFromAndToDates);
				}
			}
		}

		public void TestRequireBothFromAndToDates_WhenDateRangeMaxYearsExists()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet(@"Document",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			helper.AddWorkSheet(@"Filters",
@"{A}-[My Filter]    {B}-[Type]    {C}-[Date Range]
{B}-[Field]    {C}-[Z0_Date]
{B}-[DateRangeMaxYears]    {C}-[1]
{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Dummy Report");
			var excelTemplate = template.GetExcelTemplate();

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.UpdateAndSynchroniseFilters();

					var field = (DateRangeField)report.FilterCollection["My Filter"];

					AssertEquals(true, field.RequireBothFromAndToDates);
				}
			}
		}

		public void TestRequireBothFromAndToDates_WhenDateRangeMaxMonthsExists()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet(@"Document",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			helper.AddWorkSheet(@"Filters",
@"{A}-[My Filter]    {B}-[Type]    {C}-[Date Range]
{B}-[Field]    {C}-[Z0_Date]
{B}-[DateRangeMaxMonths]    {C}-[6]
{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Dummy Report");
			var excelTemplate = template.GetExcelTemplate();

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.UpdateAndSynchroniseFilters();

					var field = (DateRangeField)report.FilterCollection["My Filter"];

					AssertEquals(true, field.RequireBothFromAndToDates);
				}
			}
		}

		public void TestDateRangeMaxYears()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet(@"Document",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			helper.AddWorkSheet(@"Filters",
@"{A}-[My Filter]    {B}-[Type]    {C}-[Date Range]
{B}-[Field]    {C}-[Z0_Date]
{B}-[DateRangeMaxYears]    {C}-[1]
{A}-[My Filter2]    {B}-[Type]    {C}-[Date Range]
{B}-[Field]    {C}-[Z0_Date]
{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Dummy Report");
			var excelTemplate = template.GetExcelTemplate();

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.UpdateAndSynchroniseFilters();

					var field = (DateRangeField)report.FilterCollection["My Filter"];

					CombineAssertions(() =>
					{
						AssertEquals("DateRangeMaxYears valid", 1, field.DateRangeMaxYears);
						field = (DateRangeField)report.FilterCollection["My Filter2"];
						AssertEquals("DateRangeMaxYears not set", false, field.DateRangeMaxYears.HasValue);
					});
				}
			}
		}

		public void TestDateRangeMaxYears_InvalidValues_NotPositive()
		{
			AssertDateRangeMaxYears_Invalid("0");
			AssertDateRangeMaxYears_Invalid("-1");
		}

		public void TestDateRangeMaxYears_InvalidValues_NonNumericString()
		{
			AssertDateRangeMaxYears_Invalid("xx");
		}

		public void AssertDateRangeMaxYears_Invalid(string dateRangeMaxYearsValue)
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet(@"Document",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			helper.AddWorkSheet(@"Filters",
$@"{{A}}-[My Filter]    {{B}}-[Type]    {{C}}-[Date Range]
{{B}}-[Field]    {{C}}-[Z0_Date]
{{B}}-[DateRangeMaxYears]    {{C}}-[{dateRangeMaxYearsValue}]
{{A}}-[#End]");

			var template = helper.CreateTemplate(Factory, "Dummy Report");
			var excelTemplate = template.GetExcelTemplate();

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.UpdateAndSynchroniseFilters();
					Assert(report.ErrorManager.ToString().Contains($"The DateRangeMaxYears value for a Date Range field must be a positive number. \"{dateRangeMaxYearsValue}\" is not a positive number."));
				}
			}
		}

		public void TestDateRangeMaxMonths()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet(@"Document",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			helper.AddWorkSheet(@"Filters",
@"{A}-[My Filter]    {B}-[Type]    {C}-[Date Range]
{B}-[Field]    {C}-[Z0_Date]
{B}-[DateRangeMaxMonths]    {C}-[12]
{A}-[My Filter2]    {B}-[Type]    {C}-[Date Range]
{B}-[Field]    {C}-[Z0_Date]
{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Dummy Report");
			var excelTemplate = template.GetExcelTemplate();

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.UpdateAndSynchroniseFilters();

					var field = (DateRangeField)report.FilterCollection["My Filter"];

					CombineAssertions(() =>
					{
						AssertEquals("DateRangeMaxMonths valid", 12, field.DateRangeMaxMonths);
						field = (DateRangeField)report.FilterCollection["My Filter2"];
						AssertEquals("DateRangeMaxMonths not set", false, field.DateRangeMaxMonths.HasValue);
					});
				}
			}
		}

		public void TestDateRangeMaxMonths_InvalidValues_NotPositive()
		{
			AssertDateRangeMaxMonths_Invalid("0");
			AssertDateRangeMaxMonths_Invalid("-1");
		}

		public void TestDateRangeMaxMonths_InvalidValues_NonNumericString()
		{
			AssertDateRangeMaxMonths_Invalid("xx");
		}

		public void AssertDateRangeMaxMonths_Invalid(string dateRangeMaxMonthsValue)
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet(@"Document",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			helper.AddWorkSheet(@"Filters",
$@"{{A}}-[My Filter]    {{B}}-[Type]    {{C}}-[Date Range]
{{B}}-[Field]    {{C}}-[Z0_Date]
{{B}}-[DateRangeMaxMonths]    {{C}}-[{dateRangeMaxMonthsValue}]
{{A}}-[#End]");

			var template = helper.CreateTemplate(Factory, "Dummy Report");
			var excelTemplate = template.GetExcelTemplate();

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.UpdateAndSynchroniseFilters();
					Assert(report.ErrorManager.ToString().Contains($"The DateRangeMaxMonths value for a Date Range field must be a positive number. \"{dateRangeMaxMonthsValue}\" is not a positive number."));
				}
			}
		}

		public void TestConvertToUtc()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet(@"Document",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			helper.AddWorkSheet(@"Filters",
@"{A}-[My Filter]    {B}-[Type]    {C}-[Date Range]
{B}-[Field]    {C}-[Z0_Date]
{B}-[ConvertToUtc]
{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Dummy Report");
			var excelTemplate = template.GetExcelTemplate();

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.UpdateAndSynchroniseFilters();

					var field = (DateRangeField)report.FilterCollection["My Filter"];

					AssertEquals(true, field.ConvertToUtc);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWithDefaults()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("DateRangeFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				FilterCollectionBuilder testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);

				testFilterCollectionBuilder.Build();

				AssertEquals("Number of fields", 5, testFilterCollectionBuilder.IFilterCollection.Count);

				AssertEquals("First filter with default from 14 Jan 2004", new ZDateTime(2004, 1, 14), ((DateRangeField)(testFilterCollectionBuilder.IFilterCollection[1])).ValueLow);
				AssertEquals("First filter with default to 14 March 2005", new ZDateTime(2005, 3, 14), ((DateRangeField)(testFilterCollectionBuilder.IFilterCollection[1])).ValueHigh);

				AssertEquals("Second filter with default from now", ZDateTime.Now.ToShortDateString(), ((DateRangeField)(testFilterCollectionBuilder.IFilterCollection[2])).ValueLow.ToShortDateString());
				AssertEquals("Second filter with default to 16 March 2005", new ZDateTime(2005, 3, 16), ((DateRangeField)(testFilterCollectionBuilder.IFilterCollection[2])).ValueHigh);

				AssertEquals("Third filter with default from 14 Aug 2004", new ZDateTime(2004, 8, 14), ((DateRangeField)(testFilterCollectionBuilder.IFilterCollection[3])).ValueLow);
				AssertEquals("Third filter with default to now", ZDateTime.Now.ToShortDateString(), ((DateRangeField)(testFilterCollectionBuilder.IFilterCollection[3])).ValueHigh.ToShortDateString());
				AssertEquals("SubstituteMinDateForNullFrom will be false on thrid filter", false, ((DateRangeField)(testFilterCollectionBuilder.IFilterCollection[3])).SubstituteMinDateForNullFrom);
				AssertEquals("SubstituteMaxDateForNullTo will be false on third filter ", false, ((DateRangeField)(testFilterCollectionBuilder.IFilterCollection[3])).SubstituteMaxDateForNullTo);
				AssertEquals("ConvertToUtc will be false on third filter ", false, ((DateRangeField)(testFilterCollectionBuilder.IFilterCollection[3])).ConvertToUtc);

				AssertEquals("SubstituteMinDateForNullFrom will be true on fourth filter", true, ((DateRangeField)(testFilterCollectionBuilder.IFilterCollection[4])).SubstituteMinDateForNullFrom);
				AssertEquals("SubstituteMaxDateForNullTo will be true on Fourth filter ", true, ((DateRangeField)(testFilterCollectionBuilder.IFilterCollection[4])).SubstituteMaxDateForNullTo);
				AssertEquals("ConvertToUtc will be false on fourth filter ", false, ((DateRangeField)(testFilterCollectionBuilder.IFilterCollection[4])).ConvertToUtc);

				CombineAssertions("PickerFormat should be built", () =>
				{
					AssertEquals(DocEngineDatePickerFormats.Short, ((DateRangeField)testFilterCollectionBuilder.IFilterCollection[1]).PickerFormat);
					AssertEquals(DocEngineDatePickerFormats.Short, ((DateRangeField)testFilterCollectionBuilder.IFilterCollection[2]).PickerFormat);
					AssertEquals(DocEngineDatePickerFormats.Long, ((DateRangeField)testFilterCollectionBuilder.IFilterCollection[3]).PickerFormat);
					AssertEquals(DocEngineDatePickerFormats.Short, ((DateRangeField)testFilterCollectionBuilder.IFilterCollection[4]).PickerFormat);
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestYearAndMonth()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("DateRangeFilterYearAndMonthOnly.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				FilterCollectionBuilder testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);

				testFilterCollectionBuilder.Build();

				AssertEquals("Number of fields", 2, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals(DocEngineDatePickerFormats.YearAndMonth, ((DateRangeField)(testFilterCollectionBuilder.IFilterCollection[1])).PickerFormat);
			}
		}

		public void TestLanguage()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet(@"Document",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			helper.AddWorkSheet(@"Filters",
@"{A}-[My Filter]    {B}-[Type]    {C}-[Date Range]
{B}-[Language]    {C}-[ZH-CN]
{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Dummy Report");
			var excelTemplate = template.GetExcelTemplate();

			using (var documentPack = new DocumentPack())
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				report.UpdateAndSynchroniseFilters();

				var field = (DateRangeField)report.FilterCollection["My Filter"];
				AssertEquals("ZH-CN", field.Language);
			}
		}

		public void TestLanguage_Invalid()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet(@"Document",
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			helper.AddWorkSheet(@"Filters",
@"{A}-[My Filter]    {B}-[Type]    {C}-[Date Range]
{B}-[Language]    {C}-[ZH-ZH]
{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Dummy Report");
			var excelTemplate = template.GetExcelTemplate();

			using (var documentPack = new DocumentPack())
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				report.UpdateAndSynchroniseFilters();
				AssertContains("Unknown Language \"ZH-ZH\"", report.ErrorManager.ToString());
			}
		}
	}
}
