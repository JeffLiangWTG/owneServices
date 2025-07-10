using System;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(SingleAccountingPeriodField))]
	class SingleAccountingPeriodFieldTest : FitlerFieldTestWithClearValues
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SingleAccountingPeriodField(Factory);
		}

		public void TestJsonConverter_Standard()
		{
			apf.SinglePeriod = 200301;
			var deserialisedField = TestJsonConverter();
			AssertEquals("SinglePeriod", 200301, deserialisedField.SinglePeriod);
			AssertNull("Schedule", deserialisedField.Schedule);
		}

		public void TestJsonConverter_Schedule()
		{
			apf.SinglePeriod = 0;
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2003, 4, 2);
			apf.SetScheduleTask(scheduleTask);
			apf.Schedule.PeriodScope = string.Empty;

			var deserialisedField = TestJsonConverter();
			deserialisedField.SetScheduleTask(scheduleTask);
			AssertEquals("SinglePeriod", 0, deserialisedField.SinglePeriod);

			apf.Schedule.PeriodScope = PeriodScopeList.Codes.This;
			deserialisedField = TestJsonConverter();
			deserialisedField.SetScheduleTask(scheduleTask);
			AssertEquals("SinglePeriod", 200310, deserialisedField.SinglePeriod);
		}

		SingleAccountingPeriodField TestJsonConverter()
		{
			apf.DisplayName = "Unsalted";
			apf.FieldName = "Butter";

			var result = JsonConverterHelper.Serialize(apf);
			var deserialisedField = JsonConverterHelper.Deserialize<SingleAccountingPeriodField>(result);

			AssertEquals("deserialisedField.DisplayName", "Unsalted", deserialisedField.DisplayName);
			AssertEquals("deserialisedField.FieldName", "Butter", deserialisedField.FieldName);

			return deserialisedField;
		}

		public void TestSinglePeriodValidation()
		{
			Assert("Should not be any errors on period field", !apf.SinglePeriodInfo.HasErrors());
			apf.SinglePeriod = 9999;
			Assert("Should be error on period field", apf.SinglePeriodInfo.HasErrors());
			AssertEquals("Error should be standard invalid period error", apf.SinglePeriodInfo.GetErrors().GetFirstMessage(), AccountingPeriodCalculator.GetInvalidPeriodValidationError(9999));
		}

		public void TestSinglePeriodValidation_NonCurrentCompany()
		{
			var testObjectCreator = new AccountingTestObjectCreator(Factory);
			var nonCurrentCompany = testObjectCreator.NonCurrentCompany;
			var chart = testObjectCreator.CreateAlternateChart("CHL");
			var reportingBook = testObjectCreator.CreateAccReportingBook("RTE", chart.PK, nonCurrentCompany.PK);
			Factory.Save();

			apf.ReadOnlyIfFilter = "Reporting Book";
			apf.DependencyValue = reportingBook.PK.ToString();
			apf.SinglePeriod = 9999;
			Assert("Error with company information", apf.SinglePeriodInfo.HasError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(9999, nonCurrentCompany)));
		}

		public void TestSinglePeriodValidation_ReportingBookIsNull()
		{
			apf.ReadOnlyIfFilter = "Reporting Book";
			apf.DependencyValue = "";
			apf.SinglePeriod = 9999;
			Assert("Error without company information if reporting book is null", apf.SinglePeriodInfo.HasError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(9999)));
		}

		public void TestSinglePeriodValidation_CompanyOfPeriodIsEmpty()
		{
			var testObjectCreator = new AccountingTestObjectCreator(Factory);
			var chart = testObjectCreator.CreateAlternateChart("CHL");
			var reportingBook = testObjectCreator.CreateAccReportingBook("RTE", chart.PK, ZGuid.Empty);
			Factory.Save();

			apf.ReadOnlyIfFilter = "Reporting Book";
			apf.DependencyValue = reportingBook.PK.ToString();
			apf.SinglePeriod = 9999;
			Assert("Error without company information if CompanyOfPeriod is empty", apf.SinglePeriodInfo.HasError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(9999, GlbCompany.CurrentCompany)));
		}

		public virtual void TestSinglePeriod()
		{
			Match whereClauseMatch = Regex.Match(apf.WhereClause(), @"f = (@p[0-9]+)");
			Assert("Where clause should be f = @p1 but was: " + apf.WhereClause(), whereClauseMatch.Success);
			AssertEquals("Param 1 name", whereClauseMatch.Groups[1].Value, apf.SqlParameters()[0].ToString());
			AssertEquals("Param 1 value", 200301, apf.SqlParameters()[0].Value);
		}

		public void TestSetFilterValue()
		{
			var reportFilterData = new ReportFilterData();
			reportFilterData.SingleAccountingPeriodFilterCollection.Add(new SingleAccountingPeriodFilter { SinglePeriod = 200302, DisplayName = "Period" });
			apf.SetFilterValue(reportFilterData);
			AssertEquals("Run value: ", 200302, apf.SinglePeriod);

			reportFilterData.SingleAccountingPeriodFilterCollection.Clear();

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2003, 4, 2);
			apf.SetScheduleTask(scheduleTask);
			reportFilterData.SingleAccountingPeriodFilterCollection.Add(new SingleAccountingPeriodFilter { ScheduleStorageValue = new DateTime(1910, 1, 4), DisplayName = "Period" });

			apf.SetFilterValue(reportFilterData);

			AssertEquals("Schedule Storage value:", new DateTime(1910, 1, 4), apf.Schedule.ToStorageValue());
			AssertEquals("Schedule value:", 200310, apf.SinglePeriod);
		}

		public void TestFillFilterData()
		{
			apf.SinglePeriod = 200304;
			apf.DisplayName = "Period";

			var reportData = new SelectedValueReportData();

			apf.FillFilterData(reportData.FilterData);

			CombineAssertions("FillFilterData will produce the expected value.", () =>
			{
				AssertEquals(1, reportData.FilterData.SingleAccountingPeriodFilterCollection.Count);
				AssertEquals(200304, reportData.FilterData.SingleAccountingPeriodFilterCollection[0].SinglePeriod);
				AssertEquals("Period", reportData.FilterData.SingleAccountingPeriodFilterCollection[0].DisplayName);
			});

			reportData.FilterData.SingleAccountingPeriodFilterCollection.Clear();
			apf.ClearValues();

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2024, 4, 2);
			apf.SetScheduleTask(scheduleTask);

			var reportFilterData = new ReportFilterData();
			reportFilterData.SingleAccountingPeriodFilterCollection.Add(new SingleAccountingPeriodFilter
			{
				ScheduleStorageValue = new DateTime(1909, 10, 4),
				DisplayName = "Period"
			});

			apf.SetFilterValue(reportFilterData);
			apf.FillFilterData(reportData.FilterData);

			CombineAssertions("FillFilterData will produce the expected value.", () =>
			{
				AssertEquals(1, reportData.FilterData.SingleAccountingPeriodFilterCollection.Count);
				AssertEquals(new DateTime(1909, 10, 4), reportData.FilterData.SingleAccountingPeriodFilterCollection[0].ScheduleStorageValue);
				AssertEquals("Period", reportData.FilterData.SingleAccountingPeriodFilterCollection[0].DisplayName);
			});
		}

		SingleAccountingPeriodField apf;

		AccountingPeriodTestHelper PeriodTestHelper
		{
			get
			{
				if (fPeriodTestHelper == null)
				{
					fPeriodTestHelper = new AccountingPeriodTestHelper(Factory);
				}
				return fPeriodTestHelper;
			}
		}
		AccountingPeriodTestHelper fPeriodTestHelper;

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			new AccountingPeriodTestHelper().PostPeriodsForEntireYear(2003);
			apf = new SingleAccountingPeriodField(Factory);
			RequiredFilterValidator validator = new RequiredFilterValidator();
			apf.Validators.Add(validator);
			validator.Filters.Add(apf);
			apf.FieldName = "f";
			apf.SinglePeriod = 200301;
			apf.DisplayName = "Period";
			PeriodTestHelper.PostPeriodsForEntireYear(2002);
			PeriodTestHelper.PostPeriodsForEntireYear(2003);
		}

		public override FilterFieldWithUTSupport[] GetFieldsWithValidClearValueTestCases()
		{
			return new FilterFieldWithUTSupport[] { apf };
		}

		public override int ExpectedNumberOfClearValueTestCases
		{
			get
			{
				return 1;
			}
		}

		public void TestAllProvidersIsResponsible()
		{
			AssertProviderIsResponsible("<Period.ToDate>", PeriodCalculator.GetLastDayForPeriod(apf.SinglePeriod).Date, PeriodCalculator.GetLastDayForPeriod(apf.SinglePeriod).Date);
			AssertProviderIsResponsible("<Period.ToNextDate>", PeriodCalculator.GetLastDayForPeriod(apf.SinglePeriod).Date, PeriodCalculator.GetLastDayForPeriod(apf.SinglePeriod).Date.AddDays(1));
			AssertProviderIsResponsible("<Period.FromDate>", PeriodCalculator.GetFirstDayForPeriod(apf.SinglePeriod).Date, PeriodCalculator.GetFirstDayForPeriod(apf.SinglePeriod).Date);
			AssertProviderIsResponsible("<Period.ThisPeriodLastYearFromDate>", PeriodCalculator.GetFirstDayForPeriod(apf.SinglePeriod - 100).Date, PeriodCalculator.GetFirstDayForPeriod(apf.SinglePeriod - 100).Date);
			AssertProviderIsResponsible("<Period.ThisPeriodLastYearToDate>", PeriodCalculator.GetLastDayForPeriod(apf.SinglePeriod - 100).Date, PeriodCalculator.GetLastDayForPeriod(apf.SinglePeriod - 100).Date);
			AssertProviderIsResponsible("<Period.ThisPeriodLastYearToNextDate>", PeriodCalculator.GetLastDayForPeriod(apf.SinglePeriod - 100).Date, PeriodCalculator.GetLastDayForPeriod(apf.SinglePeriod - 100).Date.AddDays(1));
			AssertProviderIsResponsible("<Period.ThisYearFromDate>", PeriodCalculator.GetFirstDayForPeriod(200201).Date, PeriodCalculator.GetFirstDayForPeriod(200301).Date);
			AssertProviderIsResponsible("<Period.ThisYearToDate>", PeriodCalculator.GetLastDayForPeriod(200312).Date, PeriodCalculator.GetLastDayForPeriod(200312).Date);
			AssertProviderIsResponsible("<Period.ThisYearToNextDate>", PeriodCalculator.GetLastDayForPeriod(200312).Date, PeriodCalculator.GetLastDayForPeriod(200312).Date.AddDays(1));
			AssertProviderIsResponsible("<Period.LastYearFromDate>", PeriodCalculator.GetFirstDayForPeriod(200201).Date, PeriodCalculator.GetFirstDayForPeriod(200201).Date);
			AssertProviderIsResponsible("<Period.LastYearToDate>", PeriodCalculator.GetLastDayForPeriod(200212).Date, PeriodCalculator.GetLastDayForPeriod(200212).Date);
			AssertProviderIsResponsible("<Period.LastYearToNextDate>", PeriodCalculator.GetLastDayForPeriod(200212).Date, PeriodCalculator.GetLastDayForPeriod(200212).Date.AddDays(1));
			AssertProviderIsResponsible("<Period.ToNextDateWithMinuteSubtracted>", PeriodCalculator.GetLastDayForPeriod(apf.SinglePeriod).Date, PeriodCalculator.GetLastDayForPeriod(apf.SinglePeriod).Date.AddDays(1).AddMinutes(-1));
		}

		void AssertProviderIsResponsible(string providerName, ZDateTime dayForPeriod1, ZDateTime dayForPeriod2)
		{
			AssertEquals(true, apf.ValueProviders.ContainsProviderResponsibleFor(providerName));
			ValueProvider provider = apf.ValueProviders[providerName];
			AssertEquals(false, dayForPeriod1 == ZDateTime.Invalid);
			AssertEquals(dayForPeriod2, provider.GetReplacement(providerName, new Report(null, null)));
			int tempSinglePeriod = apf.SinglePeriod;
			apf.SinglePeriod = 0;
			AssertEquals(ZDateTime.Invalid, provider.GetReplacement(providerName, new Report(null, null)));
			apf.SinglePeriod = tempSinglePeriod;
		}

		public void TestSinglePeriodInfoReadOnly()
		{
			AssertEquals("SinglePeriodInfo.ReadOnly", false, apf.SinglePeriodInfo.ReadOnly);
			apf.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			AssertEquals("SinglePeriodInfo.ReadOnly", true, apf.SinglePeriodInfo.ReadOnly);
		}

		[TestDate(2003, 1, 1)]
		public void TestValidateSinglePeriodWhenScheduled()
		{
			apf.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			apf.SinglePeriod = 4;
			apf.Schedule.PeriodScope = string.Empty;
			AssertHasError(apf.SinglePeriodInfo, "'Period' should have data.\r\nPlease check if the relevant periods have been set up.");

			apf.Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertNoErrors(apf.SinglePeriodInfo);
		}

		[TestDate(2003, 1, 1)]
		public void TestIsEmpty()
		{
			apf.SinglePeriod = 0;
			AssertEquals("IsEmpty", true, apf.IsEmpty);

			apf.SinglePeriod = 2;
			AssertEquals("IsEmpty", false, apf.IsEmpty);

			apf.SinglePeriod = 0;
			apf.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			apf.Schedule.PeriodScope = string.Empty;
			AssertEquals("IsEmpty", true, apf.IsEmpty);

			apf.Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("IsEmpty", false, apf.IsEmpty);
		}

		public void TestSetScheduleTask()
		{
			AssertNull("Schedule", apf.Schedule);

			ReportScheduleTask scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			apf.SetScheduleTask(scheduleTask1);
			AssertEquals("Schedule.ScheduleTask", scheduleTask1, apf.Schedule.ScheduleTask);

			apf.Schedule.PeriodScope = PeriodScopeList.Codes.This;
			AssertEquals("SinglePeriod", 0, apf.SinglePeriod);

			ReportScheduleTask scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2003, 4, 20);
			apf.SetScheduleTask(scheduleTask2);
			AssertEquals("Schedule.ScheduleTask", scheduleTask2, apf.Schedule.ScheduleTask);
			AssertEquals("SinglePeriod", 200310, apf.SinglePeriod);
			AssertEquals("Schedule.GetSchedulePeriod()", 200310, apf.Schedule.GetSchedulePeriod());
		}

		public void TestRunAndScheduleSerialisation()
		{
			var runPeriod = 200301;

			var sourceField = new SingleAccountingPeriodField(Factory);
			sourceField.SinglePeriod = runPeriod;

			var result = JsonConverterHelper.Serialize(sourceField);
			var deserialisedField = JsonConverterHelper.Deserialize<SingleAccountingPeriodField>(result);

			AssertEquals("Run Period", runPeriod, deserialisedField.SinglePeriod);

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			deserialisedField.SetScheduleTask(scheduleTask);

			deserialisedField.Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			deserialisedField.Schedule.PeriodCount = 3;

			var serializedField = JsonConverterHelper.Serialize(deserialisedField);
			var deserialisedField2 = JsonConverterHelper.Deserialize<SingleAccountingPeriodField>(serializedField);
			deserialisedField2.SetScheduleTask(scheduleTask);

			AssertEquals("Schedule.PeriodScope", deserialisedField.Schedule.PeriodScope, deserialisedField2.Schedule.PeriodScope);
			AssertEquals("Schedule.PeriodCount", deserialisedField.Schedule.PeriodCount, deserialisedField2.Schedule.PeriodCount);
			AssertEquals("Period", deserialisedField.SinglePeriod, deserialisedField2.SinglePeriod);
		}

		public override void TestSafeCopyValuesFrom()
		{
			var runPeriod = 200301;

			var sourceField = new SingleAccountingPeriodField(Factory);
			sourceField.SinglePeriod = runPeriod;

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDate.Today;
			sourceField.SetScheduleTask(scheduleTask);

			sourceField.Schedule.PeriodScope = PeriodScopeList.Codes.Previous;
			sourceField.Schedule.PeriodCount = 3;

			var destinationField = new SingleAccountingPeriodField(Factory);
			((IFilter)destinationField).SafeCopyValuesFrom(sourceField);
			AssertEquals("Run Period", runPeriod, destinationField.SinglePeriod);

			destinationField.SetScheduleTask(scheduleTask);
			AssertEquals("Schedule.PeriodScope", sourceField.Schedule.PeriodScope, destinationField.Schedule.PeriodScope);
			AssertEquals("Schedule.PeriodCount", sourceField.Schedule.PeriodCount, destinationField.Schedule.PeriodCount);
			AssertEquals("Period", sourceField.SinglePeriod, destinationField.SinglePeriod);
		}

		public override void TestClearValues()
		{
			SingleAccountingPeriodField field = new SingleAccountingPeriodField(Factory);
			field.SinglePeriod = 200301;
			((IFilter)field).ClearValues();
			AssertEquals(ZInt.Zero, field.SinglePeriod);
		}

		AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (periodCalculator == null)
				{
					periodCalculator = new AccountingPeriodCalculator(Factory);
				}
				return periodCalculator;
			}
		}

		AccountingPeriodCalculator periodCalculator;
	}
}
