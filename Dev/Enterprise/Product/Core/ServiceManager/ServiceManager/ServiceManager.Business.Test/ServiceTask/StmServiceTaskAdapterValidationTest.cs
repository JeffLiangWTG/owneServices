using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	class StmServiceTaskAdapterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateFrequency()
		{
			// Arrange
			// Act
			// Assert
			CombineAssertions(() =>
			{
				AssertAdapterFrequency("1 second", "5s", 5, ScheduleRecurrenceType.Second, false, null);
				AssertAdapterFrequency("1 second", "5s", 10, ScheduleRecurrenceType.Second, true, "The task should be run at least every 5 seconds");

				AssertAdapterFrequency("1 second", "15 minutes", 5, ScheduleRecurrenceType.Second, false, null);
				AssertAdapterFrequency("1 second", "15 minutes", 2, ScheduleRecurrenceType.Minute, false, null);
				AssertAdapterFrequency("1 second", "15 minutes", 16, ScheduleRecurrenceType.Minute, true, "The task should be run at least every 15 minutes");

				AssertAdapterFrequency("2minute", "1h", 90, ScheduleRecurrenceType.Second, true, "The task schedule shouldn't be more frequent than every 2 minutes");
				AssertAdapterFrequency("2minute", "1h", 150, ScheduleRecurrenceType.Second, false, null);
				AssertAdapterFrequency("2minute", "1h", 45, ScheduleRecurrenceType.Minute, false, null);
				AssertAdapterFrequency("2minute", "1h", 1, ScheduleRecurrenceType.Daily, true, "The task should be run at least every 1 hour");

				AssertAdapterFrequency("12hours", "1day", 2, ScheduleRecurrenceType.Hourly, true, "The task schedule shouldn't be more frequent than every 12 hours");
				AssertAdapterFrequency("12hours", "1day", 12, ScheduleRecurrenceType.Hourly, false, null);
				AssertAdapterFrequency("12hours", "1day", 2, ScheduleRecurrenceType.Daily, true, "The task should be run at least every 1 day");

				AssertAdapterFrequency(null, "2 days", 10, ScheduleRecurrenceType.Minute, false, null);
				AssertAdapterFrequency(null, "2 days", 49, ScheduleRecurrenceType.Hourly, true, "The task should be run at least every 2 days");

				AssertAdapterFrequency("1w", null, 6, ScheduleRecurrenceType.Daily, true, "The task schedule shouldn't be more frequent than every 1 week");
				AssertAdapterFrequency("1w", null, 7, ScheduleRecurrenceType.Hourly, false, null);

				AssertAdapterFrequency("1n", "6months", 27, ScheduleRecurrenceType.Daily, true, "The task schedule shouldn't be more frequent than every 1 month");
				AssertAdapterFrequency("1n", "6months", 1, ScheduleRecurrenceType.Monthly, false, null);
				AssertAdapterFrequency("1n", "6months", 6, ScheduleRecurrenceType.Monthly, false, null);
				AssertAdapterFrequency("1n", "6months", 7, ScheduleRecurrenceType.Monthly, true, "The task should be run at least every 6 months");
				AssertAdapterFrequency("1n", "6months", 365, ScheduleRecurrenceType.Daily, true, "The task should be run at least every 6 months");
				AssertAdapterFrequency("1n", "6months", 168, ScheduleRecurrenceType.Daily, false, null);
				AssertAdapterFrequency("1n", "6months", 169, ScheduleRecurrenceType.Daily, true, "The task should be run at least every 6 months");

				AssertAdapterFrequency("2day", "2day", 1, ScheduleRecurrenceType.Daily, true, "The task should be run every 2 days");
				AssertAdapterFrequency("2day", "2day", 3, ScheduleRecurrenceType.Daily, true, "The task should be run every 2 days"	);

				AssertAdapterFrequency(null, null, 1, ScheduleRecurrenceType.Monthly, false, null);
			});
		}

		void AssertAdapterFrequency(string minDuration, string maxDuration, int period, string frequency, bool hasErrors, string expectedMessage)
		{
			// Arrange
			var task = GetServiceTask(Factory, minDuration, maxDuration, null);
			var adapter = new StmServiceTaskAdapter(task);

			switch (frequency)
			{
				case ScheduleRecurrenceType.Second: adapter.SecondsRange = true; break;
				case ScheduleRecurrenceType.Minute: adapter.MinutesRange = true; break;
				case ScheduleRecurrenceType.Hourly: adapter.HoursRange = true; break;
				case ScheduleRecurrenceType.Daily: adapter.DaysRange = true; break;
				case ScheduleRecurrenceType.Weekly: adapter.WeeksRange = true; break;
				case ScheduleRecurrenceType.Monthly: adapter.MonthsRange = true; break;
			}

			// Act
			adapter.Period = period;

			// Assert
			AssertEquals("Has Error", hasErrors, adapter.PeriodInfo.HasError(expectedMessage));
			AssertEquals("Has Warning", false, adapter.PeriodInfo.HasWarning(expectedMessage));
		}

		public void TestValidateFrequencyWhenUpdateCalculator()
		{
			// Arrange
			// Act
			// Assert
			CombineAssertions(() =>
			{
				AssertAdapterFrequency("2minute", "1h", 30, ScheduleRecurrenceType.Minute, false, null);
				AssertAdapterFrequency("2minute", "1h", 30, ScheduleRecurrenceType.Hourly, true, "The task should be run at least every 1 hour");
				AssertAdapterFrequency("2minute", "1h", 30, ScheduleRecurrenceType.Second, true, "The task schedule shouldn't be more frequent than every 2 minutes");
			});
		}

		public void TestValidateFrequencyWhenUpdatePeriod()
		{
			// Arrange
			// Act
			// Assert
			CombineAssertions(() =>
			{
				AssertAdapterFrequency("2minute", "1h", 30, ScheduleRecurrenceType.Minute, false, null);
				AssertAdapterFrequency("2minute", "1h", 99, ScheduleRecurrenceType.Minute, true, "The task should be run at least every 1 hour");
				AssertAdapterFrequency("2minute", "1h", 1, ScheduleRecurrenceType.Minute, true, "The task schedule shouldn't be more frequent than every 2 minutes");
			});
		}

		static StmServiceTask GetServiceTask(BusinessObjectFactory factory, string minimumPeriod, string maximumPeriod, string requiresCompanyInCountry, bool canRunInAnyBranch = false)
		{
			var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
				d.RunEvery == "15minutes");

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.DefaultSchedule == defaultScheduleMock &&
				a.MinimumPeriod == minimumPeriod &&
				a.MaximumPeriod == maximumPeriod &&
				a.RequiresCompanyInCountry == requiresCompanyInCountry &&
				a.CanRunInAnyBranch == canRunInAnyBranch);

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var task = factory.New<StmServiceTask>();
				_ = task.StaticServiceAttributes;

				return task;
			}
		}

		public void TestValidateFrequency_SchedulePeriodValidationIsSkippedIfServiceTaskIsNudgeable()
		{
			// Arrange
			using (ObjectFactory.Substitute(Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(provider =>
				provider.BusinessObjectBindings == new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("222", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("333", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo2", Array.Empty<string>(), null),
				})))
			{
				var schemaResolver = new Mock<IApplicationSchemaResolver>();
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo2")).Returns(DummyBizoSchema.Instance);

				SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = true;

				var serviceTask = GetServiceTask(Factory, "2minute", "1h", null);
				serviceTask.SST_ServiceTaskCode = "111";

				var adapter = new StmServiceTaskAdapter(serviceTask);
				adapter.SecondsRange = true;
				adapter.Period = 30; // Invalid period but task is nudgeable so validation will be skipped

				var validation = new StmServiceTaskAdapterValidation(adapter);

				// Act
				validation.ValidateFrequency();

				// Assert
				AssertNoErrors(adapter.PeriodInfo);
			}
		}

		public void TestValidateCalculatorWeeks()
		{
			// Arrange
			var serviceTask = GetServiceTask(Factory, "1second", "1year", null);
			var adapter = new StmServiceTaskAdapter(serviceTask);

			CombineAssertions(() =>
			{
				AssertWeeklyValidation(adapter, false, 1, false, false, null);
				AssertWeeklyValidation(adapter, true, 1, false, true, "Please select at least one day.");
				AssertWeeklyValidation(adapter, true, 1, true, false, null);
			});

			void AssertWeeklyValidation(StmServiceTaskAdapter adapter, bool weeksRange, int period, bool oneDaySelected, bool hasErrors, string expectedMessage)
			{
				adapter.WeeksRange = weeksRange;
				adapter.DaysRange = !weeksRange;
				adapter.Period = period;

				adapter.Sunday = false;
				adapter.Monday = false;
				adapter.Tuesday = false;
				adapter.Wednesday = oneDaySelected;
				adapter.Thursday = false;
				adapter.Friday = false;
				adapter.Saturday = false;

				AssertEquals("Has Error", hasErrors, adapter.MondayInfo.HasError(expectedMessage));
			}
		}

		public void TestValidateCalculatorMonths()
		{
			// Arrange
			var serviceTask = GetServiceTask(Factory, "1second", "1year", null);
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Act
			// Assert
			CombineAssertions(() =>
			{
				AssertMonthlyValidation(adapter, false, 1, 99, false, null);
				AssertMonthlyValidation(adapter, true, 1, 28, false, null);
				AssertMonthlyValidation(adapter, true, 1, 32, true, "Please enter a number from 1 to 28 for Day of the Month.");
				AssertMonthlyValidation(adapter, true, 1, 2, false, null);
			});

			void AssertMonthlyValidation(StmServiceTaskAdapter adapter, bool monthsRange, int period, int dayOfMonth, bool hasErrors, string expectedMessage)
			{
				adapter.MonthsRange = monthsRange;
				adapter.DaysRange = !monthsRange;
				adapter.Period = period;
				adapter.DayOfMonth = dayOfMonth;

				AssertEquals("Has Error", hasErrors, adapter.DayOfMonthInfo.HasError(expectedMessage));
			}
		}

		public void TestValidateCalculatorYears()
		{
			// Arrange
			var serviceTask = GetServiceTask(Factory, "1second", "1year", null);
			var adapter = new StmServiceTaskAdapter(serviceTask);
			var message = "Please select a valid day for the chosen month.";

			CombineAssertions(() =>
			{
				AssertYearlyValidation(adapter, 1, 31, false, message);
				AssertYearlyValidation(adapter, 1, 32, true, message);

				AssertYearlyValidation(adapter, 2, 28, false, message);
				AssertYearlyValidation(adapter, 2, 29, true, message);

				AssertYearlyValidation(adapter, 3, 31, false, message);
				AssertYearlyValidation(adapter, 3, 32, true, message);

				AssertYearlyValidation(adapter, 4, 30, false, message);
				AssertYearlyValidation(adapter, 4, 31, true, message);

				AssertYearlyValidation(adapter, 5, 31, false, message);
				AssertYearlyValidation(adapter, 5, 32, true, message);

				AssertYearlyValidation(adapter, 6, 30, false, message);
				AssertYearlyValidation(adapter, 6, 31, true, message);

				AssertYearlyValidation(adapter, 7, 31, false, message);
				AssertYearlyValidation(adapter, 7, 32, true, message);

				AssertYearlyValidation(adapter, 8, 31, false, message);
				AssertYearlyValidation(adapter, 8, 32, true, message);

				AssertYearlyValidation(adapter, 9, 30, false, message);
				AssertYearlyValidation(adapter, 9, 31, true, message);

				AssertYearlyValidation(adapter, 10, 31, false, message);
				AssertYearlyValidation(adapter, 10, 32, true, message);

				AssertYearlyValidation(adapter, 11, 30, false, message);
				AssertYearlyValidation(adapter, 11, 31, true, message);

				AssertYearlyValidation(adapter, 12, 31, false, message);
				AssertYearlyValidation(adapter, 12, 32, true, message);
			});

			void AssertYearlyValidation(StmServiceTaskAdapter adapter, int monthOfYear, int dayOfMonth, bool hasErrors, string expectedMessage)
			{
				adapter.YearsRange = true;
				adapter.YearlyEvery = true;
				adapter.EveryMonthNumberDayAsString = monthOfYear.ToString();
				adapter.YearlyDay = dayOfMonth;

				AssertEquals("Has Error", hasErrors, adapter.YearlyDayInfo.HasError(expectedMessage));
			}
		}

		public void TestValidatePeriodIsSet()
		{
			// Arrange
			var message = "Period cannot be zero or negative.";

			// Act
			// Assert
			CombineAssertions(() =>
			{
				AssertAdapterFrequency("1second", "1year", 0, ScheduleRecurrenceType.Second, true, message);
				AssertAdapterFrequency("1second", "1year", 0, ScheduleRecurrenceType.Minute, true, message);
				AssertAdapterFrequency("1second", "1year", 0, ScheduleRecurrenceType.Hourly, true, message);
				AssertAdapterFrequency("1second", "1year", 0, ScheduleRecurrenceType.Daily, true, message);
				AssertAdapterFrequency("1second", "1year", 0, ScheduleRecurrenceType.Weekly, true, message);
				AssertAdapterFrequency("1second", "1year", 0, ScheduleRecurrenceType.Monthly, true, message);

				AssertAdapterFrequency("1second", "1year", -1, ScheduleRecurrenceType.Second, true, message);
				AssertAdapterFrequency("1second", "1year", -1, ScheduleRecurrenceType.Minute, true, message);
				AssertAdapterFrequency("1second", "1year", -1, ScheduleRecurrenceType.Hourly, true, message);
				AssertAdapterFrequency("1second", "1year", -1, ScheduleRecurrenceType.Daily, true, message);
				AssertAdapterFrequency("1second", "1year", -1, ScheduleRecurrenceType.Weekly, true, message);
				AssertAdapterFrequency("1second", "1year", -1, ScheduleRecurrenceType.Monthly, true, message);

				AssertAdapterFrequency("1second", "1year", 1, ScheduleRecurrenceType.Second, false, null);
				AssertAdapterFrequency("1second", "1year", 1, ScheduleRecurrenceType.Minute, false, null);
				AssertAdapterFrequency("1second", "1year", 1, ScheduleRecurrenceType.Hourly, false, null);
				AssertAdapterFrequency("1second", "1year", 1, ScheduleRecurrenceType.Daily, false, null);
				AssertAdapterFrequency("1second", "1year", 1, ScheduleRecurrenceType.Weekly, false, null);
				AssertAdapterFrequency("1second", "1year", 1, ScheduleRecurrenceType.Monthly, false, null);
			});
		}

		public void TestNoErrorsWhenCalcDailyTimesBothHaveValues()
		{
			// Arrange
			var adapter = GetServiceTaskAdapterWithTaskCalculator();
			adapter.CalcDailyStartTimeUtc = new ZDateTime(2025, 1, 1, 1, 1, 1);
			adapter.CalcDailyEndTimeUtc = new ZDateTime(2025, 1, 1, 1, 1, 1);

			// Act
			adapter.Validation.ValidateCalcDailyStartTimeUtc();
			adapter.Validation.ValidateCalcDailyEndTimeUtc();

			// Assert
			AssertEquals("Start has an error", false, adapter.CalcDailyStartTimeUtcInfo.HasError(CalcDailyTimeUtcErrorMessage));
			AssertEquals("End has an error", false, adapter.CalcDailyEndTimeUtcInfo.HasError(CalcDailyTimeUtcErrorMessage));
		}

		public void TestNoErrorsWhenCalcDailyTimesBothAreEmpty()
		{
			// Arrange
			var adapter = GetServiceTaskAdapterWithTaskCalculator();
			adapter.CalcDailyStartTimeUtc = ZDateTime.Empty;
			adapter.CalcDailyEndTimeUtc = ZDateTime.Empty;

			// Act
			adapter.Validation.ValidateCalcDailyStartTimeUtc();
			adapter.Validation.ValidateCalcDailyEndTimeUtc();

			// Assert
			AssertEquals("Start has an error", false, adapter.CalcDailyStartTimeUtcInfo.HasError(CalcDailyTimeUtcErrorMessage));
			AssertEquals("End has an error", false, adapter.CalcDailyEndTimeUtcInfo.HasError(CalcDailyTimeUtcErrorMessage));
		}

		public void TestErrorWhenOnlyCalcDailyStartTimeIsEmpty()
		{
			// Arrange
			var adapter = GetServiceTaskAdapterWithTaskCalculator();
			adapter.CalcDailyEndTimeUtc = new ZDateTime(2025, 1, 1, 1, 1, 1);
			adapter.CalcDailyStartTimeUtc = ZDateTime.Empty;

			// Act
			adapter.Validation.ValidateCalcDailyStartTimeUtc();
			adapter.Validation.ValidateCalcDailyEndTimeUtc();

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("Start doesn't have an error", true, adapter.CalcDailyStartTimeUtcInfo.HasError(CalcDailyTimeUtcErrorMessage));
				AssertEquals("End doesn't have an error", true, adapter.CalcDailyEndTimeUtcInfo.HasError(CalcDailyTimeUtcErrorMessage));
			});
		}

		public void TestErrorWhenOnlyCalcDailyEndTimeIsEmpty()
		{
			// Arrange
			var adapter = GetServiceTaskAdapterWithTaskCalculator();
			adapter.CalcDailyStartTimeUtc = new ZDateTime(2025, 1, 1, 1, 1, 1);
			adapter.CalcDailyEndTimeUtc = ZDateTime.Empty;

			// Act
			adapter.Validation.ValidateCalcDailyStartTimeUtc();
			adapter.Validation.ValidateCalcDailyEndTimeUtc();

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals("Start doesn't have an error", true, adapter.CalcDailyStartTimeUtcInfo.HasError(CalcDailyTimeUtcErrorMessage));
				AssertEquals("End doesn't have an error", true, adapter.CalcDailyEndTimeUtcInfo.HasError(CalcDailyTimeUtcErrorMessage));
			});
		}

		public void TestErrorWhenSetLargeNegativePeriod()
		{
			// Arrange
			var serviceTask = GetServiceTask(Factory, "1second", "1year", null);
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Act
			adapter.Period = -999999999;

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(true,
					adapter.PeriodInfo.HasError(
						"Period cannot be zero or negative."));
			});
		}

		StmServiceTaskAdapter GetServiceTaskAdapterWithTaskCalculator()
		{
			var serviceTask = GetServiceTask(Factory, "1minute", "10minutes", null);
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMinutes() { Period = 5 };
			return new StmServiceTaskAdapter(serviceTask);
		}

		const string CalcDailyTimeUtcErrorMessage = "You must specify either both a start time and an end time, or neither.";
	}
}
