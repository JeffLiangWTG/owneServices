using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ServiceManager.Shared;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(StmServiceTaskAdapter))]
	public class StmServiceTaskAdapterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new StmServiceTaskAdapter(Factory.NewWithValidTestData<StmServiceTask>());
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestAdapterSecondsSchedule()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorSeconds
			{
				Period = 11, StartTime = TimeSpan.FromHours(11), EndTime = TimeSpan.FromHours(22)
			};

			// Act
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(11)),
					adapter.CalcDailyStartTimeUtc);
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(22)),
					adapter.CalcDailyEndTimeUtc);
				AssertEquals("21:00", adapter.CalcDailyStartTimeLocalText);
				AssertEquals("08:00", adapter.CalcDailyEndTimeLocalText);
				AssertEquals(11, adapter.Period);
				AssertExpectedRange(adapter, seconds: true, false, false, false, false, false, false);
			});
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestAdapterMinutesSchedule()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMinutes
			{
				Period = 11, StartTime = TimeSpan.FromHours(11), EndTime = TimeSpan.FromHours(22)
			};

			// Act
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(11)),
					adapter.CalcDailyStartTimeUtc);
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(22)),
					adapter.CalcDailyEndTimeUtc);
				AssertEquals("21:00", adapter.CalcDailyStartTimeLocalText);
				AssertEquals("08:00", adapter.CalcDailyEndTimeLocalText);
				AssertEquals(11, adapter.Period);
				AssertExpectedRange(adapter, false, minutes: true, false, false, false, false, false);
			});
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestAdapterHoursSchedule()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorHours()
			{
				Period = 11, StartTime = TimeSpan.FromHours(11), EndTime = TimeSpan.FromHours(22)
			};

			// Act
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(11)),
					adapter.CalcDailyStartTimeUtc);
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(22)),
					adapter.CalcDailyEndTimeUtc);
				AssertEquals("21:00", adapter.CalcDailyStartTimeLocalText);
				AssertEquals("08:00", adapter.CalcDailyEndTimeLocalText);
				AssertEquals(11, adapter.Period);
				AssertExpectedRange(adapter, false, false, hours: true, false, false, false, false);
			});
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestAdapterDaysSchedule()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorDays
			{
				Period = 11, ScheduledRunTime = TimeSpan.FromHours(11)
			};

			// Act
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(11)),
					adapter.RecurringStartTimeUtc);
				AssertEquals("21:00", adapter.RecurringStartTimeLocalText);
				AssertEquals(11, adapter.Period);
				AssertExpectedRange(adapter, false, false, false, days: true, false, false, false);
				Assert(adapter.DailyDay);
			});
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestAdapterWorkingDaysSchedule()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorWorkingDays()
			{
				ScheduledRunTime = TimeSpan.FromHours(11)
			};

			// Act
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(11)),
					adapter.RecurringStartTimeUtc);
				AssertEquals("21:00", adapter.RecurringStartTimeLocalText);
				AssertExpectedRange(adapter, false, false, false, days: true, false, false, false);
				Assert(adapter.WeekDaysOnly);
			});
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestAdapterWeeksSchedule()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorWeeks()
			{
				Period = 11,
				ScheduledRunTime = TimeSpan.FromHours(11),
				DaysOfOccurrence = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Sunday }
			};

			// Act
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(11)),
					adapter.RecurringStartTimeUtc);
				AssertEquals("21:00", adapter.RecurringStartTimeLocalText);
				AssertEquals(11, adapter.Period);
				AssertExpectedRange(adapter, false, false, false, false, weeks: true, false, false);
				Assert(adapter.Monday);
				Assert(adapter.Wednesday);
				Assert(adapter.Sunday);
				Assert(!adapter.Tuesday);
				Assert(!adapter.Thursday);
				Assert(!adapter.Friday);
				Assert(!adapter.Saturday);
			});
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestAdapterMonthsByDateSchedule()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMonthsByDate()
			{
				Period = 11, ScheduledRunTime = TimeSpan.FromHours(11), DayOfOccurrence = 11
			};

			// Act
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(11)),
					adapter.RecurringStartTimeUtc);
				AssertEquals("21:00", adapter.RecurringStartTimeLocalText);
				AssertEquals(11, adapter.Period);
				AssertExpectedRange(adapter, false, false, false, false, false, months: true, false);
				AssertEquals(11, adapter.DayOfMonth);
				Assert(adapter.MonthlyDay);
			});
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestAdapterMonthsByDayOfWeekSchedule()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMonthsByDayOfWeek()
			{
				ScheduledRunTime = TimeSpan.FromHours(11), DayOfTheWeek = DayOfWeek.Monday, WeekOfTheMonth = 2
			};

			// Act
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(11)),
					adapter.RecurringStartTimeUtc);
				AssertEquals("21:00", adapter.RecurringStartTimeLocalText);
				AssertExpectedRange(adapter, false, false, false, false, false, months: true, false);
				Assert(adapter.MonthlyWeekDay);
				AssertEquals("2", adapter.WeekCountAsString);
				AssertEquals("MON", adapter.DayName);
			});
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestAdapterMonthsByLastDay()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMonthsByLastDay()
			{
				Period = 11, ScheduledRunTime = TimeSpan.FromHours(11)
			};

			// Act
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(11)),
					adapter.RecurringStartTimeUtc);
				AssertEquals("21:00", adapter.RecurringStartTimeLocalText);
				AssertEquals(11, adapter.Period);
				AssertExpectedRange(adapter, false, false, false, false, false, months: true, false);
				Assert(adapter.MonthsLastDay);
			});
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestAdapterYearsByDateSchedule()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorYearsByDate()
			{
				ScheduledRunTime = TimeSpan.FromHours(11), Month = 11, Day = 22
			};

			// Act
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(11)),
					adapter.RecurringStartTimeUtc);
				AssertEquals("21:00", adapter.RecurringStartTimeLocalText);
				AssertExpectedRange(adapter, false, false, false, false, false, false, years: true);
				AssertEquals(22, adapter.YearlyDay);
				AssertEquals("11", adapter.EveryMonthNumberDayAsString);
				Assert(adapter.YearlyEvery);
			});
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestAdapterYearsByDayOfMonthSchedule()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorYearsByDayOfMonth()
			{
				ScheduledRunTime = TimeSpan.FromHours(11),
				MonthOfTheYear = 11,
				DayOfTheWeek = DayOfWeek.Monday,
				WeekOfTheMonth = 2
			};

			// Act
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.MinSmallDateTimeValue.Date.Add(TimeSpan.FromHours(11)),
					adapter.RecurringStartTimeUtc);
				AssertEquals("21:00", adapter.RecurringStartTimeLocalText);
				AssertExpectedRange(adapter, false, false, false, false, false, false, years: true);
				AssertEquals("11", adapter.MonthNumberAsString);
				AssertEquals("MON", adapter.DayName);
				AssertEquals("2", adapter.WeekCountAsString);
				Assert(adapter.YearlyWeekDay);
			});
		}

		void AssertExpectedRange(StmServiceTaskAdapter adapter, bool seconds, bool minutes, bool hours, bool days,
			bool weeks, bool months, bool years)
		{
			AssertEquals(adapter.SecondsRange, seconds);
			AssertEquals(adapter.MinutesRange, minutes);
			AssertEquals(adapter.HoursRange, hours);
			AssertEquals(adapter.DaysRange, days);
			AssertEquals(adapter.WeeksRange, weeks);
			AssertEquals(adapter.MonthsRange, months);
			AssertEquals(adapter.YearsRange, years);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2024, 11, 1)]
		public void TestDailyStartTimeLocalSydWithDst() => AssertUtcToLocalConversion(
			new NextRunTimeCalculatorHours
			{
				Period = 2, StartTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30))
			}, GetAdapterStartTimeLocalText, "22:30");

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2024, 6, 1)]
		public void TestDailyStartTimeLocalSydWithoutDst() => AssertUtcToLocalConversion(
			new NextRunTimeCalculatorHours
			{
				Period = 2, StartTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30))
			}, GetAdapterStartTimeLocalText, "21:30");

		[TestTimeZoneUNLOCO("USERI")]
		[TestDate(2024, 12, 1)]
		public void TestDailyStartTimeLocalNonSydWithDst() => AssertUtcToLocalConversion(
			new NextRunTimeCalculatorHours
			{
				Period = 2, StartTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30))
			}, GetAdapterStartTimeLocalText, "06:30");

		[TestTimeZoneUNLOCO("USERI")]
		[TestDate(2024, 6, 1)]
		public void TestDailyStartTimeLocalNonSydWithoutDst() => AssertUtcToLocalConversion(
			new NextRunTimeCalculatorHours
			{
				Period = 2, StartTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30))
			}, GetAdapterStartTimeLocalText, "07:30");

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2024, 11, 1)]
		public void TestDailyEndTimeLocalSydWithDst() => AssertUtcToLocalConversion(
			new NextRunTimeCalculatorHours
			{
				Period = 2, EndTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30))
			}, GetAdapterEndTimeLocalText, "22:30");

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2024, 6, 1)]
		public void TestDailyEndTimeLocalSydWithoutDst() => AssertUtcToLocalConversion(
			new NextRunTimeCalculatorHours
			{
				Period = 2, EndTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30))
			}, GetAdapterEndTimeLocalText, "21:30");

		[TestTimeZoneUNLOCO("USERI")]
		[TestDate(2024, 12, 1)]
		public void TestDailyEndTimeLocalNonSydWithDst() => AssertUtcToLocalConversion(
			new NextRunTimeCalculatorHours
			{
				Period = 2, EndTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30))
			}, GetAdapterEndTimeLocalText, "06:30");

		[TestTimeZoneUNLOCO("USERI")]
		[TestDate(2024, 6, 1)]
		public void TestDailyEndTimeLocalNonSydWithoutDst() => AssertUtcToLocalConversion(
			new NextRunTimeCalculatorHours
			{
				Period = 2, EndTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30))
			}, GetAdapterEndTimeLocalText, "07:30");

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2024, 11, 1)]
		public void TestRecurringStartTimeLocalSydWithDst() => AssertUtcToLocalConversion(
			new NextRunTimeCalculatorWeeks
			{
				Period = 2, ScheduledRunTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30))
			}, GetAdapterRecurringStartTimeLocalText, "22:30");

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2024, 6, 1)]
		public void TestRecurringStartTimeLocalSydWithoutDst() => AssertUtcToLocalConversion(
			new NextRunTimeCalculatorWeeks
			{
				Period = 2, ScheduledRunTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30))
			}, GetAdapterRecurringStartTimeLocalText, "21:30");

		[TestTimeZoneUNLOCO("USERI")]
		[TestDate(2024, 12, 1)]
		public void TestRecurringStartTimeLocalNonSydWithDst() => AssertUtcToLocalConversion(
			new NextRunTimeCalculatorWeeks
			{
				Period = 2, ScheduledRunTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30))
			}, GetAdapterRecurringStartTimeLocalText, "06:30");

		[TestTimeZoneUNLOCO("USERI")]
		[TestDate(2024, 6, 1)]
		public void TestRecurringStartTimeLocalNonSydWithoutDst() => AssertUtcToLocalConversion(
			new NextRunTimeCalculatorWeeks
			{
				Period = 2, ScheduledRunTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30))
			}, GetAdapterRecurringStartTimeLocalText, "07:30");

		void AssertUtcToLocalConversion(INextRunTimeCalculator calculator,
			Func<StmServiceTaskAdapter, string> assertProperty, string expectedTimeString)
		{
			// Arrange
			TestDateAttribute.UseUNLOCO = true;

			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = calculator;

			// Act
			var adapter = new StmServiceTaskAdapter(serviceTask);

			AssertEquals(expectedTimeString, assertProperty(adapter));
		}

		string GetAdapterStartTimeLocalText(StmServiceTaskAdapter adapter) => adapter.CalcDailyStartTimeLocalText;
		string GetAdapterEndTimeLocalText(StmServiceTaskAdapter adapter) => adapter.CalcDailyEndTimeLocalText;

		string GetAdapterRecurringStartTimeLocalText(StmServiceTaskAdapter adapter) =>
			adapter.RecurringStartTimeLocalText;

		public void TestAddDayOfWeek()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator =
				new NextRunTimeCalculatorWeeks { DaysOfOccurrence = new[] { DayOfWeek.Monday, DayOfWeek.Friday } };

			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Act
			adapter.Wednesday = true;

			// Assert
			AssertContainsExactElementsInAnyOrder(new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday },
				(serviceTask.NextRunTimeCalculator as NextRunTimeCalculatorWeeks).DaysOfOccurrence);
		}

		public void TestRemoveDayOfWeek()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator =
				new NextRunTimeCalculatorWeeks { DaysOfOccurrence = new[] { DayOfWeek.Monday, DayOfWeek.Friday } };

			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Act
			adapter.Monday = false;

			// Assert
			AssertContainsExactElementsInAnyOrder(new[] { DayOfWeek.Friday },
				(serviceTask.NextRunTimeCalculator as NextRunTimeCalculatorWeeks).DaysOfOccurrence);
		}

		[TestDate(2025, 01, 01, 12, 0, 0)]
		public void TestPeriodExceedsMaxValue_DoesNotThrow()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorWeeks
			{
				Period = 1, DaysOfOccurrence = [DayOfWeek.Wednesday], ScheduledRunTime = TimeSpan.FromHours(9)
			};

			// Act & Assert
			AssertNoExceptionThrown(() => serviceTask.Recurrence.Period = 1111111111);
		}

		public void TestNullStartTimeSetWhenValueAssignedToCalcDailyStartTimeUtcIsNotValid()
		{
			// Arrange
			var calculator = new NextRunTimeCalculatorMinutes { Period = 15 };
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = calculator;

			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Act
			adapter.CalcDailyStartTimeUtc = DateTime.MinValue;

			// Assert
			AssertEquals(null, calculator.StartTime);
		}

		public void TestNullEndTimeSetWhenValueAssignedToCalcDailyEndTimeUtcIsNotValid()
		{
			// Arrange
			var calculator = new NextRunTimeCalculatorMinutes { Period = 15 };
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTimeCalculator = calculator;

			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Act
			adapter.CalcDailyEndTimeUtc = DateTime.MinValue;

			// Assert
			AssertEquals(null, calculator.EndTime);
		}

		public void TestWhenCalcDailyStartTimeUtcIsSetThenNextRunTimeIsUpdated()
		{
			// Arrange
			var calculator = new NextRunTimeCalculatorMinutes { Period = 15 };
			var oldNextRunTime = new ZDateTime(2025, 1, 1);
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTime = oldNextRunTime;
			serviceTask.NextRunTimeCalculator = calculator;
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Act
			adapter.CalcDailyStartTimeUtc = new ZDateTime(2025, 1, 1, 12, 0, 0);

			// Assert
			AssertNotEquals(serviceTask.NextRunTime, oldNextRunTime);
		}

		public void TestWhenCalcDailyEndTimeUtcIsSetThenNextRunTimeIsUpdated()
		{
			// Arrange
			var calculator = new NextRunTimeCalculatorMinutes { Period = 15 };
			var oldNextRunTime = new ZDateTime(2025, 1, 1);
			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.NextRunTime = oldNextRunTime;
			serviceTask.NextRunTimeCalculator = calculator;
			var adapter = new StmServiceTaskAdapter(serviceTask);

			// Act
			adapter.CalcDailyEndTimeUtc = new ZDateTime(2025, 1, 1, 12, 0, 0);

			// Assert
			AssertNotEquals(serviceTask.NextRunTime, oldNextRunTime);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>());
			var hostedServiceProviderMock = Mock.Of<IClientHostedServiceAttributeProvider>(a =>
				a.GetClientHostedServiceAttribute(It.IsAny<string>()) == hostedServiceMock);
			hostedServiceAttributeProviderDisposable = ObjectFactory.Substitute(hostedServiceProviderMock);
		}

		protected override void TearDown()
		{
			hostedServiceAttributeProviderDisposable?.Dispose();
			base.TearDown();
		}

		IDisposable hostedServiceAttributeProviderDisposable;
	}
}
