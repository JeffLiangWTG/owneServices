using System;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class NextRunTimeCalculatorYearsByDayOfMonthTest
	{
		[Test]
		public void TestNextRunTimeCalculatorYearsByDayOfMonth()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, MonthOfTheYear = 1, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2023, 1, 1, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 1, 2, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, MonthOfTheYear = 1, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2023, 1, 2, 9, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 1, 2, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, MonthOfTheYear = 1, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2023, 1, 2, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 1, 2, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, MonthOfTheYear = 1, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2023, 1, 2, 11, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Tuesday, MonthOfTheYear = 2, ScheduledRunTime = new TimeSpan(12, 30, 0) }, new DateTimeOffset(2023, 1, 1, 9, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 2, 7, 12, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 2, DayOfTheWeek = DayOfWeek.Thursday, MonthOfTheYear = 4, ScheduledRunTime = new TimeSpan(11, 45, 0) }, new DateTimeOffset(2023, 11, 10, 22, 45, 0, TimeSpan.FromHours(11)), new DateTimeOffset(2024, 4, 11, 11, 45, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 2, DayOfTheWeek = DayOfWeek.Saturday, MonthOfTheYear = 7, ScheduledRunTime = new TimeSpan(18, 0, 30) }, new DateTimeOffset(2023, 6, 30, 4, 30, 0, TimeSpan.FromHours(11)), new DateTimeOffset(2023, 7, 8, 18, 0, 30, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 3, DayOfTheWeek = DayOfWeek.Wednesday, MonthOfTheYear = 8, ScheduledRunTime = new TimeSpan(22, 15, 30) }, new DateTimeOffset(2023, 12, 31, 8, 30, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 8, 21, 22, 15, 30, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 4, DayOfTheWeek = DayOfWeek.Friday, MonthOfTheYear = 10, ScheduledRunTime = new TimeSpan(23, 0, 0) }, new DateTimeOffset(2024, 11, 15, 23, 30, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2025, 10, 24, 23, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 4, DayOfTheWeek = DayOfWeek.Sunday, MonthOfTheYear = 12, ScheduledRunTime = new TimeSpan(0, 0, 0) }, new DateTimeOffset(2026, 12, 27, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 12, 27, 0, 0, 0, TimeSpan.Zero));
			});

			void TestFunction(NextRunTimeCalculatorYearsByDayOfMonth nextRunTimeCalculator, DateTimeOffset calculateFrom, DateTimeOffset expectedResult)
			{
				// Act
				var result = nextRunTimeCalculator.CalculateNextRunTime(calculateFrom);

				// Assert
				Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorYearsByDayOfMonthIsSerialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() => {
				TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Tuesday, MonthOfTheYear = 3, ScheduledRunTime = new TimeSpan(14, 0, 0) }, "<NextRunTimeCalculatorYearsByDayOfMonth WeekOfTheMonth=\"1\" DayOfTheWeek=\"Tuesday\" MonthOfTheYear=\"3\" ScheduledRunTime=\"14:00:00\" />");
				TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 3, DayOfTheWeek = DayOfWeek.Tuesday, MonthOfTheYear = 10, ScheduledRunTime = new TimeSpan(12, 0, 0) }, "<NextRunTimeCalculatorYearsByDayOfMonth WeekOfTheMonth=\"3\" DayOfTheWeek=\"Tuesday\" MonthOfTheYear=\"10\" ScheduledRunTime=\"12:00:00\" />");
				TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 2, DayOfTheWeek = DayOfWeek.Tuesday, MonthOfTheYear = 5, ScheduledRunTime = new TimeSpan(8, 0, 0) }, "<NextRunTimeCalculatorYearsByDayOfMonth WeekOfTheMonth=\"2\" DayOfTheWeek=\"Tuesday\" MonthOfTheYear=\"5\" ScheduledRunTime=\"08:00:00\" />");
			});
			void TestFunction(NextRunTimeCalculatorYearsByDayOfMonth nextRunTimeCalculator, string expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Serialise<NextRunTimeCalculatorYearsByDayOfMonth>(nextRunTimeCalculator);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorYearsByDayOfMonthIsDeserialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction("<NextRunTimeCalculatorYearsByDayOfMonth WeekOfTheMonth=\"3\" DayOfTheWeek=\"Monday\" MonthOfTheYear=\"10\" ScheduledRunTime=\"12:00:00\"/>", new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 3, DayOfTheWeek = DayOfWeek.Monday, MonthOfTheYear = 10, ScheduledRunTime = new TimeSpan(12, 0, 0) });
				TestFunction("<NextRunTimeCalculatorYearsByDayOfMonth WeekOfTheMonth=\"1\" DayOfTheWeek=\"Sunday\" MonthOfTheYear=\"3\" ScheduledRunTime=\"04:30:00\"/>", new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Sunday, MonthOfTheYear = 3, ScheduledRunTime = new TimeSpan(4, 30, 0) });
				TestFunction("<NextRunTimeCalculatorYearsByDayOfMonth WeekOfTheMonth=\"2\" DayOfTheWeek=\"Tuesday\" MonthOfTheYear=\"8\" ScheduledRunTime=\"16:00:00\"/>", new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 2, DayOfTheWeek = DayOfWeek.Tuesday, MonthOfTheYear = 8, ScheduledRunTime = new TimeSpan(16, 0, 0) });
			});

			void TestFunction(string configuration, NextRunTimeCalculatorYearsByDayOfMonth expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Deserialise<NextRunTimeCalculatorYearsByDayOfMonth>(configuration);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
	}
}
