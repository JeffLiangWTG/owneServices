using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(AccountingPeriodsRangeField))]
	sealed class AccountingPeriodsRangeFieldTest : FitlerFieldTestWithClearValues
	{
		[TestDate(2003, 1, 1)]
		public void TestScheduledReportShowsDefaultValues()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			helper.AddWorkSheet("Filters",
@"{A}-[Period Range]  {B}-[Type]  {C}-[Period Accounting Range]
{B}-[OnlyOneOfGroup]  {C}-[Group 1]
{B}-[AtLeastOneOfGroup]  {C}-[Group 2]

{A}-[Date Range]  {B}-[Type]  {C}-[Period Date Range]
{B}-[OnlyOneOfGroup]  {C}-[Group 1]
{B}-[AtLeastOneOfGroup]  {C}-[Group 2]

{A}-[#End]");

			var template = helper.CreateTemplate(Factory, "Test");
			var excelTemplate = template.GetExcelTemplate();

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();

					report.SetScheduleTask(scheduleTask);
					report.UpdateAndSynchroniseFilters();

					var filters = report.FilterCollection;
					var periodRangeFilter = (AccountingPeriodsRangeField)filters["Period Range"];

					AssertEquals("Pre-condition: Default value expected", PeriodScopeList.Codes.This, periodRangeFilter.LowSchedule.PeriodScope);
					AssertEquals("Pre-condition: Default value expected", PeriodScopeList.Codes.This, periodRangeFilter.HighSchedule.PeriodScope);

					AssertEquals("PeriodFrom should be visible.", 200307, periodRangeFilter.PeriodFrom);
					AssertEquals("PeriodFrom should be visible.", 200307, periodRangeFilter.PeriodTo);
				}
			}
		}

		public void TestJsonConverter_Standard()
		{
			TestPeriodRangeField.DependentFilter = "dependentFilter";
			TestPeriodRangeField.DependencyValue = Guid.NewGuid().ToString();
			TestPeriodRangeField.DisplayName = "Json Test";
			TestPeriodRangeField.PeriodFrom = 200001;
			TestPeriodRangeField.PeriodTo = 200606;
			TestPeriodRangeField.FieldName = "TestSerialization";

			var result = JsonConverterHelper.Serialize(TestPeriodRangeField);
			var newTestPeriodRangeField = JsonConverterHelper.Deserialize<AccountingPeriodsRangeField>(result);

			AssertEquals("Json Test", newTestPeriodRangeField.DisplayName);
			AssertEquals("TestSerialization", newTestPeriodRangeField.FieldName);
			AssertEquals((ZInt)200001, newTestPeriodRangeField.PeriodFrom);
			AssertEquals((ZInt)200606, newTestPeriodRangeField.PeriodTo);
		}

		public void TestJsonConverter_Scheduled()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2003, 4, 1);
			TestPeriodRangeField.SetScheduleTask(scheduleTask);

			TestPeriodRangeField.DisplayName = "Orange";
			TestPeriodRangeField.FieldName = "Juice";
			TestPeriodRangeField.LowSchedule.PeriodScope = PeriodScopeList.Codes.This;
			TestPeriodRangeField.HighSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			TestPeriodRangeField.HighSchedule.PeriodCount = 1;

			var result = JsonConverterHelper.Serialize(TestPeriodRangeField);
			var deserialisedField = JsonConverterHelper.Deserialize<AccountingPeriodsRangeField>(result);
			deserialisedField.SetScheduleTask(scheduleTask);

			AssertEquals("DisplayName", "Orange", deserialisedField.DisplayName);
			AssertEquals("FieldName", "Juice", deserialisedField.FieldName);
			AssertEquals("PeriodFrom", 200310, deserialisedField.PeriodFrom);
			AssertEquals("PeriodTo", 200311, deserialisedField.PeriodTo);

			deserialisedField.LowSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			deserialisedField.LowSchedule.PeriodCount = 2;
			deserialisedField.HighSchedule.PeriodCount = 2;
			AssertEquals("PeriodFrom", 200308, deserialisedField.PeriodFrom);
			AssertEquals("PeriodTo", 200312, deserialisedField.PeriodTo);
		}

		public void TestPeriodRangeValidation()
		{
			Assert("Should be no errors", !TestPeriodRangeField.PeriodFromInfo.HasErrors());
			Assert("Should be no errors", !TestPeriodRangeField.PeriodToInfo.HasErrors());

			TestPeriodRangeField.PeriodFrom = 200201;
			TestPeriodRangeField.PeriodTo = 200212;

			Assert("Should be no errors", !TestPeriodRangeField.PeriodFromInfo.HasErrors());
			Assert("Should be no errors", !TestPeriodRangeField.PeriodToInfo.HasErrors());

			TestPeriodRangeField.PeriodTo = 200301;
			AssertEquals("Should have error", true, TestPeriodRangeField.PeriodToInfo.HasErrors());
			AssertEquals(true, TestPeriodRangeField.PeriodToInfo.HasError(AccountingPeriodsRangeField.PeriodRangeDifferentYearErrorMessage));

			TestPeriodRangeField.PeriodFrom = 200210;
			AssertEquals("Should have error", true, TestPeriodRangeField.PeriodFromInfo.HasErrors());
			AssertEquals(true, TestPeriodRangeField.PeriodFromInfo.HasError(AccountingPeriodsRangeField.PeriodRangeDifferentYearErrorMessage));

			TestPeriodRangeField.PeriodFrom = 200201;
			TestPeriodRangeField.PeriodTo = 200212;

			Assert("Should be no errors", !TestPeriodRangeField.PeriodFromInfo.HasErrors());
			Assert("Should be no errors", !TestPeriodRangeField.PeriodToInfo.HasErrors());

			TestPeriodRangeField.PeriodFrom = 200210;
			TestPeriodRangeField.PeriodTo = 200209;
			AssertEquals("Should have error", true, TestPeriodRangeField.PeriodToInfo.HasErrors());
			AssertEquals(true, TestPeriodRangeField.PeriodToInfo.HasError(AccountingPeriodsRangeField.PeriodToGreaterFromErrorMessage));
			AssertEquals(true, TestPeriodRangeField.PeriodFromInfo.HasError(AccountingPeriodsRangeField.PeriodFromLessToErrorMessage));

			TestPeriodRangeField.PeriodFrom = 200201;
			TestPeriodRangeField.PeriodTo = 200206;
			Assert("Should be no errors", !TestPeriodRangeField.PeriodFromInfo.HasErrors());
			Assert("Should be no errors", !TestPeriodRangeField.PeriodToInfo.HasErrors());

			TestPeriodRangeField.RequireBothFromAndToPeriods = false;
			TestPeriodRangeField.PeriodFrom = ZInt.Zero;
			TestPeriodRangeField.PeriodTo = ZInt.Zero;
			AssertEquals(false, TestPeriodRangeField.PeriodFromInfo.HasError("period From must be entered."));
			AssertEquals(false, TestPeriodRangeField.PeriodToInfo.HasError("period To must be entered."));

			TestPeriodRangeField.PeriodFrom = 200201;
			TestPeriodRangeField.PeriodTo = 200206;
			Assert("Should be no errors", !TestPeriodRangeField.PeriodFromInfo.HasErrors());
			Assert("Should be no errors", !TestPeriodRangeField.PeriodToInfo.HasErrors());

			TestPeriodRangeField.RequireBothFromAndToPeriods = true;
			TestPeriodRangeField.PeriodFrom = ZInt.Zero;
			TestPeriodRangeField.PeriodTo = ZInt.Zero;
			AssertEquals("Should have error", true, TestPeriodRangeField.PeriodToInfo.HasErrors());
			AssertEquals(true, TestPeriodRangeField.PeriodFromInfo.HasError("period From must be entered."));
			AssertEquals(true, TestPeriodRangeField.PeriodToInfo.HasError("period To must be entered."));
		}

		public void TestPeriodValidation_NonCurrentCompany()
		{
			var testObjectCreator = new AccountingTestObjectCreator(Factory);
			var nonCurrentCompany = testObjectCreator.NonCurrentCompany;
			var chart = testObjectCreator.CreateAlternateChart("CHL");
			var reportingBook = testObjectCreator.CreateAccReportingBook("RTE", chart.PK, nonCurrentCompany.PK);
			Factory.Save();

			TestPeriodRangeField.ReadOnlyIfFilter = "Reporting Book";
			TestPeriodRangeField.DependencyValue = reportingBook.PK.ToString();
			TestPeriodRangeField.PeriodFrom = 202101;
			TestPeriodRangeField.PeriodTo = 202103;
			AssertHasError(TestPeriodRangeField.PeriodFromInfo, AccountingPeriodCalculator.GetInvalidPeriodValidationError(202101, nonCurrentCompany));
		}

		[ExpectNoExceptions()]
		public void TestInvalidPeriodValue()
		{
			TestPeriodRangeField.PeriodFrom = 200201;
			TestPeriodRangeField.PeriodTo = 200212;

			TestPeriodRangeField.PeriodFrom = 2;
			TestPeriodRangeField.PeriodTo = 2;
		}

		public void TestIsEmpty()
		{
			TestPeriodRangeField.PeriodFrom = 0;
			TestPeriodRangeField.PeriodTo = 1;
			AssertEquals("IsEmpty", true, TestPeriodRangeField.IsEmpty);

			TestPeriodRangeField.PeriodFrom = 1;
			TestPeriodRangeField.PeriodTo = 0;
			AssertEquals("IsEmpty", true, TestPeriodRangeField.IsEmpty);

			TestPeriodRangeField.PeriodTo = 2;
			AssertEquals("IsEmpty", false, TestPeriodRangeField.IsEmpty);

			TestPeriodRangeField.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());

			TestPeriodRangeField.LowSchedule.PeriodScope = PeriodScopeList.Codes.This;
			TestPeriodRangeField.HighSchedule.PeriodScope = "";
			AssertEquals("IsEmpty", true, TestPeriodRangeField.IsEmpty);

			TestPeriodRangeField.LowSchedule.PeriodScope = "";
			TestPeriodRangeField.HighSchedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("IsEmpty", true, TestPeriodRangeField.IsEmpty);

			TestPeriodRangeField.LowSchedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("IsEmpty", false, TestPeriodRangeField.IsEmpty);
		}

		public void TestPeriodRangeValidationWhenScheduled()
		{
			MockFilterCollectionValidator validator = new MockFilterCollectionValidator();
			TestPeriodRangeField.Validators.Add(validator);
			TestPeriodRangeField.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());

			validator.SetIsValid(false);
			TestPeriodRangeField.PeriodFrom = 200202;
			TestPeriodRangeField.PeriodTo = 200203;
			AssertHasError(TestPeriodRangeField.PeriodFromInfo, MockFilterCollectionValidator.ErrorMessage);
			AssertHasError(TestPeriodRangeField.PeriodToInfo, MockFilterCollectionValidator.ErrorMessage);

			validator.SetIsValid(true);
			TestPeriodRangeField.PeriodFrom = 200202;
			TestPeriodRangeField.PeriodTo = 200203;
			AssertNoErrors(TestPeriodRangeField.PeriodFromInfo);
			AssertNoErrors(TestPeriodRangeField.PeriodToInfo);
		}

		public void TestSetScheduleTask()
		{
			AssertNull("LowSchedule", TestPeriodRangeField.LowSchedule);
			AssertNull("HighSchedule", TestPeriodRangeField.HighSchedule);

			ReportScheduleTask scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			TestPeriodRangeField.SetScheduleTask(scheduleTask1);
			AssertEquals("LowSchedule.ScheduleTask", scheduleTask1, TestPeriodRangeField.LowSchedule.ScheduleTask);
			AssertEquals("HighSchedule.ScheduleTask", scheduleTask1, TestPeriodRangeField.HighSchedule.ScheduleTask);

			TestPeriodRangeField.LowSchedule.PeriodScope = PeriodScopeList.Codes.This;
			TestPeriodRangeField.HighSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			TestPeriodRangeField.HighSchedule.PeriodCount = 1;
			AssertEquals("PeriodFrom", 0, TestPeriodRangeField.PeriodFrom);
			AssertEquals("PeriodTo", 0, TestPeriodRangeField.PeriodTo);

			ReportScheduleTask scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2003, 4, 20);
			TestPeriodRangeField.SetScheduleTask(scheduleTask2);
			AssertEquals("LowSchedule.ScheduleTask", scheduleTask2, TestPeriodRangeField.LowSchedule.ScheduleTask);
			AssertEquals("HighSchedule.ScheduleTask", scheduleTask2, TestPeriodRangeField.HighSchedule.ScheduleTask);
			AssertEquals("PeriodFrom", 200310, TestPeriodRangeField.PeriodFrom);
			AssertEquals("PeriodTo", 200311, TestPeriodRangeField.PeriodTo);
			AssertEquals("LowSchedule.GetSchedulePeriod()", 200310, TestPeriodRangeField.LowSchedule.GetSchedulePeriod());
			AssertEquals("HighSchedule.GetSchedulePeriod()", 200311, TestPeriodRangeField.HighSchedule.GetSchedulePeriod());
		}

		public void TestPeriodToReadOnly()
		{
			AssertEquals("PeriodToInfo.ReadOnly", false, TestPeriodRangeField.PeriodToInfo.ReadOnly);
			TestPeriodRangeField.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			AssertEquals("PeriodToInfo.ReadOnly", true, TestPeriodRangeField.PeriodToInfo.ReadOnly);
		}

		public void TestPeriodFromReadOnly()
		{
			AssertEquals("PeriodFromInfo.ReadOnly", false, TestPeriodRangeField.PeriodFromInfo.ReadOnly);
			TestPeriodRangeField.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			AssertEquals("PeriodFromInfo.ReadOnly", true, TestPeriodRangeField.PeriodFromInfo.ReadOnly);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1136:DoNotUseSystemRuntimeSerializationFormattersBinary", Justification = "WI00700503 - Pending migration")]
		public void TestRunAndScheduleSerialisation()
		{
			var runPeriodFrom = 200201;
			var runPeriodTo = 200210;

			var sourceField = new AccountingPeriodsRangeField(Factory);
			sourceField.PeriodFrom = runPeriodFrom;
			sourceField.PeriodTo = runPeriodTo;

			var result = JsonConverterHelper.Serialize(sourceField);
			var deserialisedField = JsonConverterHelper.Deserialize<AccountingPeriodsRangeField>(result);

			AssertEquals("Run PeriodFrom", runPeriodFrom, deserialisedField.PeriodFrom);
			AssertEquals("Run PeriodTo", runPeriodTo, deserialisedField.PeriodTo);

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			deserialisedField.SetScheduleTask(scheduleTask);

			deserialisedField.LowSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			deserialisedField.LowSchedule.PeriodCount = 3;

			deserialisedField.HighSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			deserialisedField.HighSchedule.PeriodCount = 5;

			result = JsonConverterHelper.Serialize(deserialisedField);
			var deserialisedField2 = JsonConverterHelper.Deserialize<AccountingPeriodsRangeField>(result);

			AssertEquals("Run PeriodFrom", runPeriodFrom, deserialisedField2.PeriodFrom);
			AssertEquals("Run PeriodTo", runPeriodTo, deserialisedField2.PeriodTo);

			deserialisedField2.SetScheduleTask(scheduleTask);

			AssertEquals("LowSchedule.PeriodScope", deserialisedField.LowSchedule.PeriodScope, deserialisedField2.LowSchedule.PeriodScope);
			AssertEquals("LowSchedule.PeriodCount", deserialisedField.LowSchedule.PeriodCount, deserialisedField2.LowSchedule.PeriodCount);
			AssertEquals("PeriodFrom", deserialisedField.PeriodFrom, deserialisedField2.PeriodFrom);

			AssertEquals("HighSchedule.PeriodScope", deserialisedField.HighSchedule.PeriodScope, deserialisedField2.HighSchedule.PeriodScope);
			AssertEquals("HighSchedule.PeriodCount", deserialisedField.HighSchedule.PeriodCount, deserialisedField2.HighSchedule.PeriodCount);
			AssertEquals("PeriodTo", deserialisedField.PeriodTo, deserialisedField2.PeriodTo);
		}

		public override void TestSafeCopyValuesFrom()
		{
			var runPeriodFrom = 200201;
			var runPeriodTo = 200212;

			var sourceField = new AccountingPeriodsRangeField(Factory);
			sourceField.PeriodFrom = runPeriodFrom;
			sourceField.PeriodTo = runPeriodTo;

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			sourceField.SetScheduleTask(scheduleTask);

			sourceField.LowSchedule.PeriodScope = PeriodScopeList.Codes.Previous;
			sourceField.LowSchedule.PeriodCount = 3;

			sourceField.HighSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			sourceField.HighSchedule.PeriodCount = 5;

			var destinationField = new AccountingPeriodsRangeField(Factory);
			((IFilter)destinationField).SafeCopyValuesFrom(sourceField);
			AssertEquals("Run PeriodFrom", runPeriodFrom, destinationField.PeriodFrom);
			AssertEquals("Run PeriodTo", runPeriodTo, destinationField.PeriodTo);

			destinationField.SetScheduleTask(scheduleTask);
			AssertEquals("LowSchedule.PeriodScope", sourceField.LowSchedule.PeriodScope, destinationField.LowSchedule.PeriodScope);
			AssertEquals("LowSchedule.PeriodCount", sourceField.LowSchedule.PeriodCount, destinationField.LowSchedule.PeriodCount);
			AssertEquals("PeriodFrom", sourceField.PeriodFrom, destinationField.PeriodFrom);

			AssertEquals("HighSchedule.PeriodScope", sourceField.HighSchedule.PeriodScope, destinationField.HighSchedule.PeriodScope);
			AssertEquals("HighSchedule.PeriodCount", sourceField.HighSchedule.PeriodCount, destinationField.HighSchedule.PeriodCount);
			AssertEquals("PeriodTo", sourceField.PeriodTo, destinationField.PeriodTo);
		}

		public override void TestClearValues()
		{
			AccountingPeriodsRangeField field = new AccountingPeriodsRangeField(Factory);
			field.PeriodFrom = 200201;
			field.PeriodTo = 200212;
			((IFilter)field).ClearValues();
			AssertEquals("PeriodFrom", ZInt.Zero, field.PeriodFrom);
			AssertEquals("PeriodTo", ZInt.Zero, field.PeriodTo);
		}

		public void TestPeriodsValidation()
		{
			AssertNull("LowSchedule", TestPeriodRangeField.LowSchedule);
			AssertNull("HighSchedule", TestPeriodRangeField.HighSchedule);

			AssertEquals("PeriodFrom", 0, TestPeriodRangeField.PeriodFrom);
			AssertEquals("PeriodTo", 0, TestPeriodRangeField.PeriodTo);
			AssertNoWarnings("No warnings if not scheduled", TestPeriodRangeField.PeriodFromInfo);
			AssertNoWarnings("No warnings if not scheduled", TestPeriodRangeField.PeriodToInfo);

			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2003, 4, 1);
			TestPeriodRangeField.SetScheduleTask(scheduleTask);

			TestPeriodRangeField.LowSchedule.PeriodScope = PeriodScopeList.Codes.This;
			TestPeriodRangeField.HighSchedule.PeriodScope = PeriodScopeList.Codes.Next;
			TestPeriodRangeField.HighSchedule.PeriodCount = 1;

			AssertEquals("PeriodFrom", 200310, TestPeriodRangeField.PeriodFrom);
			AssertEquals("PeriodTo", 200311, TestPeriodRangeField.PeriodTo);
			AssertNoWarnings(TestPeriodRangeField.PeriodFromInfo);
			AssertNoWarnings(TestPeriodRangeField.PeriodToInfo);

			scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2011, 4, 1);
			TestPeriodRangeField.SetScheduleTask(scheduleTask);

			AssertEquals("PeriodFrom", 0, TestPeriodRangeField.PeriodFrom);
			AssertEquals("PeriodTo", 0, TestPeriodRangeField.PeriodTo);
			AssertHasWarning("There are no periods setup for 2011 year", TestPeriodRangeField.PeriodFromInfo, "Setup periods in Accounts -> Period Management.");
			AssertHasWarning("There are no periods setup for 2011 year", TestPeriodRangeField.PeriodFromInfo, "Setup periods in Accounts -> Period Management.");
		}

		public void TestPeriodScheduleDefaults()
		{
			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2003, 4, 20);
			Factory.Save();

			TestPeriodRangeField.SetScheduleTask(scheduleTask);

			AssertEquals(ZString.Empty, TestPeriodRangeField.LowSchedule.PeriodScope);
			AssertEquals(ZString.Empty, TestPeriodRangeField.HighSchedule.PeriodScope);
		}

		public void TestValueAsObjectTranslatable()
		{
			var field = new AccountingPeriodsRangeField(Factory);
			field.PeriodFrom = 200201;
			field.PeriodTo = 200212;
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("ca44afdb-2576-4ef4-8905-6d46d0574ea0", new ResourceStringData("ca44afdb-2576-4ef4-8905-6d46d0574ea0", "从: {0}, 到: {1}"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals("从: 200201, 到: 200212", field.ValueAsObject);
				}
			}
		}

		public void TestSetFilterValue()
		{
			var field = new AccountingPeriodsRangeField(Factory);
			field.DisplayName = "abc";

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2002, 5, 2);

			var reportFilterData = new ReportFilterData();

			reportFilterData.AccountingPeriodsRangeFilterCollection.Add(new AccountingPeriodsRangeFilter { DisplayName = "abc" });
			field.SetFilterValue(reportFilterData);
			AssertEquals("Run value - From: ", 0, field.PeriodFrom);
			AssertEquals("Run value - To: ", 0, field.PeriodTo);
			Assert("Empty filter value is valid when it is not required",field.IsValid);

			reportFilterData.AccountingPeriodsRangeFilterCollection.Clear();
			reportFilterData.AccountingPeriodsRangeFilterCollection.Add(new AccountingPeriodsRangeFilter { DisplayName = "abc" });

			var requiredFilterValidator = new RequiredFilterValidator();
			field.Validators.Add(requiredFilterValidator);

			field.SetFilterValue(reportFilterData);
			AssertEquals("Run value - From: ", 0, field.PeriodFrom);
			AssertEquals("Run value - To: ", 0, field.PeriodTo);
			Assert("Empty filter value is invalid when it is required", !field.IsValid);

			reportFilterData.AccountingPeriodsRangeFilterCollection.Clear();
			reportFilterData.AccountingPeriodsRangeFilterCollection.Add(new AccountingPeriodsRangeFilter { PeriodFrom = 202404, PeriodTo = 202405, DisplayName = "abc" });

			field.SetFilterValue(reportFilterData);
			AssertEquals("Run value - From: ", 202404, field.PeriodFrom);
			AssertEquals("Run value - To: ", 202405, field.PeriodTo);
			Assert("Valid filter value when it is required", field.IsValid);

			field.SetScheduleTask(scheduleTask);
			reportFilterData.AccountingPeriodsRangeFilterCollection.Clear();
			reportFilterData.AccountingPeriodsRangeFilterCollection.Add(new AccountingPeriodsRangeFilter { DisplayName = "abc" });
			field.SetFilterValue(reportFilterData);

			Assert("Invalid schedule Storage value - Low:", !field.LowSchedule.IsValid);
			AssertEquals("Schedule value - Low:", 0, field.PeriodFrom);
			Assert("Invalid schedule Storage value - High:", !field.HighSchedule.IsValid);
			AssertEquals("Schedule value - High:", 0, field.PeriodTo);
			Assert("Empty filter value is invalid when it is required", !field.IsValid);

			reportFilterData.AccountingPeriodsRangeFilterCollection.Clear();
			reportFilterData.AccountingPeriodsRangeFilterCollection.Add(new AccountingPeriodsRangeFilter { ScheduleStorageFrom = new DateTime(1909, 10, 4), ScheduleStorageTo = new DateTime(1910, 7, 4), DisplayName = "abc" });

			field.SetFilterValue(reportFilterData);

			AssertEquals("Schedule Storage value - Low:", new DateTime(1909, 10, 4), field.LowSchedule.ToStorageValue());
			AssertEquals("Schedule value - Low:", 200208, field.PeriodFrom);
			AssertEquals("Schedule Storage value - High:", new DateTime(1910, 7, 4), field.HighSchedule.ToStorageValue());
			AssertEquals("Schedule value - High:", 200305, field.PeriodTo);
			Assert("Valid filter value when it is required", field.IsValid);

			field.Validators.Remove(requiredFilterValidator);
			reportFilterData.AccountingPeriodsRangeFilterCollection.Clear();
			reportFilterData.AccountingPeriodsRangeFilterCollection.Add(new AccountingPeriodsRangeFilter { DisplayName = "abc" });
			field.SetFilterValue(reportFilterData);

			Assert("Invalid schedule Storage value - Low:", !field.LowSchedule.IsValid);
			AssertEquals("Schedule value - Low:", 0, field.PeriodFrom);
			Assert("Invalid schedule Storage value - High:", !field.HighSchedule.IsValid);
			AssertEquals("Schedule value - High:", 0, field.PeriodTo);
			Assert("Empty filter value is valid when it is not required", field.IsValid);
		}

		public void TestFillFilterData()
		{
			var field = new AccountingPeriodsRangeField(Factory);
			field.PeriodFrom = 200304;
			field.PeriodTo = 200305;

			field.DisplayName = "Period";

			var reportData = new SelectedValueReportData();

			field.FillFilterData(reportData.FilterData);

			CombineAssertions("FillFilterData will produce the expected value.", () =>
			{
				AssertEquals(1, reportData.FilterData.AccountingPeriodsRangeFilterCollection.Count);
				AssertEquals(200304, reportData.FilterData.AccountingPeriodsRangeFilterCollection[0].PeriodFrom);
				AssertEquals(200305, reportData.FilterData.AccountingPeriodsRangeFilterCollection[0].PeriodTo);
				AssertEquals("Period", reportData.FilterData.AccountingPeriodsRangeFilterCollection[0].DisplayName);
			});

			reportData.FilterData.AccountingPeriodsRangeFilterCollection.Clear();
			field.ClearValues();

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2024, 4, 2);
			field.SetScheduleTask(scheduleTask);

			var reportFilterData = new ReportFilterData();
			reportFilterData.AccountingPeriodsRangeFilterCollection.Add(new AccountingPeriodsRangeFilter
			{
				ScheduleStorageFrom = new DateTime(1909, 10, 4),
				ScheduleStorageTo = new DateTime(1909, 11, 4),
				DisplayName = "Period"
			});

			field.SetFilterValue(reportFilterData);
			field.FillFilterData(reportData.FilterData);

			CombineAssertions("FillFilterData will produce the expected value.", () =>
			{
				AssertEquals(1, reportData.FilterData.AccountingPeriodsRangeFilterCollection.Count);
				AssertEquals(new DateTime(1909, 10, 4), reportData.FilterData.AccountingPeriodsRangeFilterCollection[0].ScheduleStorageFrom);
				AssertEquals(new DateTime(1909, 11, 4), reportData.FilterData.AccountingPeriodsRangeFilterCollection[0].ScheduleStorageTo);
				AssertEquals("Period", reportData.FilterData.AccountingPeriodsRangeFilterCollection[0].DisplayName);
			});
		}

		#region Implementation

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			TestPeriodRangeField.PeriodFrom = 200201;
			TestPeriodRangeField.PeriodTo = 200212;
			return new FilterFieldWithUTSupport[] { TestPeriodRangeField };
		}

		public override int ExpectedNumberOfClearValueTestCases
		{
			get { return 1; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestPeriodRangeField = new AccountingPeriodsRangeField(new BusinessObjectFactory());

			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			PeriodTestHelper = new AccountingPeriodTestHelper();
			PeriodTestHelper.PostPeriodsForEntireYear(2003);
			PeriodTestHelper.PostPeriodsForEntireYear(2002);

			TestPeriodRangeField.FieldName = "test";
		}

		AccountingPeriodsRangeField TestPeriodRangeField;
		AccountingPeriodTestHelper PeriodTestHelper;

		#endregion
	}
}
