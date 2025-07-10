using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class OnlyOneFilterNotEmptyValidatorTest : TestCaseWithFactory
	{
		[TestDate(2003, 1, 1)]
		public void TestEndToEndWithAScheduledReportUsingSingleAccountingPeriodAndDateRangeFilters()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document", @"
{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Period: <Period>]
{B}-[Post Date: <PostDate>]
{A}-[#EndOfReport]");

			helper.AddWorkSheet("Filters", @"
{A}-[Period]      {B}-[Type]              {C}-[Single Accounting Period]
                  {B}-[Field]             {C}-[AM_Period]
                  {B}-[OnlyOneOfGroup]    {C}-[g1]

{A}-[PostDate]    {B}-[Type]              {C}-[Date Range]
                  {B}-[Field]             {C}-[Post Date]
                  {B}-[OnlyOneOfGroup]    {C}-[g1]

{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Test");

			using (var documentPack = new DocumentPack())
			{
				var report = new Report(documentPack, template.GetExcelTemplate());
				documentPack.Add(report);

				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				report.SetScheduleTask(scheduleTask);
				report.PrepareForRender();

				var periodFilter = report.FilterCollection["Period"] as SingleAccountingPeriodField;

				var postDateFilter = report.FilterCollection["PostDate"] as DateRangeField;
				postDateFilter.LowSchedule.PeriodScope = PeriodScopeList.Codes.This;
				postDateFilter.HighSchedule.PeriodScope = PeriodScopeList.Codes.This;

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("Report Output",
@"{B}-[Period: 0]
{B}-[Post Date: From: 01-Jan-03 To: 01-Jan-03]", excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestAtLeastOneWithOneNonEmptyAndOneEmpty()
		{
			OnlyOneFilterNotEmptyValidator testFilterCollectionValidator = new OnlyOneFilterNotEmptyValidator();
			DummyFilterField emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			DummyFilterField nonEmptyFilter = new DummyFilterField(false, "NonEmptyFilter", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(nonEmptyFilter);
			AssertEquals(true, testFilterCollectionValidator.IsValid(nonEmptyFilter));
			AssertEquals(true, testFilterCollectionValidator.IsValid(emptyFilter));
		}

		public void TestAtLeastOneWithMoreThanOneNonEmpty()
		{
			OnlyOneFilterNotEmptyValidator testFilterCollectionValidator = new OnlyOneFilterNotEmptyValidator();
			DummyFilterField emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			DummyFilterField nonEmptyFilter = new DummyFilterField(false, "NonEmptyFilter", new BusinessObjectFactory());
			DummyFilterField nonEmptyFilter2 = new DummyFilterField(false, "NonEmptyFilter2", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(nonEmptyFilter);
			testFilterCollectionValidator.Filters.Add(nonEmptyFilter2);
			AssertEquals(false, testFilterCollectionValidator.IsValid(nonEmptyFilter));
			AssertEquals(false, testFilterCollectionValidator.IsValid(nonEmptyFilter2));
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter));
			AssertEquals("Only one of the 'EmptyFilter', 'NonEmptyFilter' and 'NonEmptyFilter2' should have data.", testFilterCollectionValidator.GetErrorMessage(nonEmptyFilter));
		}

		public void TestAtLeastOneWithAllEmpty()
		{
			OnlyOneFilterNotEmptyValidator testFilterCollectionValidator = new OnlyOneFilterNotEmptyValidator();
			DummyFilterField emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			DummyFilterField emptyFilter2 = new DummyFilterField(true, "EmptyFilter2", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(emptyFilter2);
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter2));
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter));
			AssertEquals("Only one of the 'EmptyFilter' and 'EmptyFilter2' should have data.", testFilterCollectionValidator.GetErrorMessage(emptyFilter));
		}

		public void TestAtLeastOneWith3Empty()
		{
			OnlyOneFilterNotEmptyValidator testFilterCollectionValidator = new OnlyOneFilterNotEmptyValidator();
			DummyFilterField emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			DummyFilterField emptyFilter2 = new DummyFilterField(true, "EmptyFilter2", new BusinessObjectFactory());
			DummyFilterField emptyFilter3 = new DummyFilterField(true, "EmptyFilter3", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			testFilterCollectionValidator.Filters.Add(emptyFilter2);
			testFilterCollectionValidator.Filters.Add(emptyFilter3);
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter));
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter2));
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter3));
			AssertEquals("Only one of the 'EmptyFilter', 'EmptyFilter2' and 'EmptyFilter3' should have data.", testFilterCollectionValidator.GetErrorMessage(emptyFilter));
		}

		public void TestAtLeastOneWithOneEmpty()
		{
			OnlyOneFilterNotEmptyValidator testFilterCollectionValidator = new OnlyOneFilterNotEmptyValidator();
			DummyFilterField emptyFilter = new DummyFilterField(true, "EmptyFilter", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(emptyFilter);
			AssertEquals(false, testFilterCollectionValidator.IsValid(emptyFilter));
			AssertEquals("'EmptyFilter' should have data.", testFilterCollectionValidator.GetErrorMessage(emptyFilter));
		}

		public void TestAtLeastOneWithOneNonEmpty()
		{
			OnlyOneFilterNotEmptyValidator testFilterCollectionValidator = new OnlyOneFilterNotEmptyValidator();
			DummyFilterField nonEmptyFilter = new DummyFilterField(false, "NonEmptyFilter", new BusinessObjectFactory());
			testFilterCollectionValidator.Filters.Add(nonEmptyFilter);
			AssertEquals(true, testFilterCollectionValidator.IsValid(nonEmptyFilter));
		}
	}
}
