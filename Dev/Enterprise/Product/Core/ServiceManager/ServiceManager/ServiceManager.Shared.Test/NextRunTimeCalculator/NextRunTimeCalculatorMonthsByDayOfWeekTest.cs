using System;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class NextRunTimeCalculatorMonthsByDayOfWeekTest
	{
		[Test]
		public void TestCalculateRunTimeMonthsByDayofWeek()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = new TimeSpan(12, 0, 0) }, new DateTimeOffset(2023, 12, 1, 12, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 12, 4, 12, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = new TimeSpan(12, 0, 0) }, new DateTimeOffset(2023, 12, 4, 11, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 12, 4, 12, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = new TimeSpan(12, 0, 0) }, new DateTimeOffset(2023, 12, 4, 12, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 12, 4, 12, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = new TimeSpan(12, 0, 0) }, new DateTimeOffset(2023, 12, 4, 12, 0, 1, TimeSpan.Zero), new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Tuesday, ScheduledRunTime = new TimeSpan(12, 30, 0) }, new DateTimeOffset(2024, 2, 12, 13, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 3, 5, 12, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 2, DayOfTheWeek = DayOfWeek.Wednesday, ScheduledRunTime = new TimeSpan(9, 45, 30) }, new DateTimeOffset(2024, 4, 18, 16, 0, 0, TimeSpan.FromHours(11)), new DateTimeOffset(2024, 5, 8, 9, 45, 30, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 2, DayOfTheWeek = DayOfWeek.Wednesday, ScheduledRunTime = new TimeSpan(16, 0, 0) }, new DateTimeOffset(2024, 4, 1, 16, 0, 0, TimeSpan.FromHours(11)), new DateTimeOffset(2024, 4, 10, 16, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 3, DayOfTheWeek = DayOfWeek.Thursday, ScheduledRunTime = new TimeSpan(18, 0, 20) }, new DateTimeOffset(2024, 5, 30, 4, 30, 0, TimeSpan.FromHours(11)), new DateTimeOffset(2024, 6, 20, 18, 0, 20, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 3, DayOfTheWeek = DayOfWeek.Sunday, ScheduledRunTime = new TimeSpan(2, 0, 0) }, new DateTimeOffset(2024, 6, 11, 13, 30, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 6, 16, 2, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 3, DayOfTheWeek = DayOfWeek.Sunday, ScheduledRunTime = new TimeSpan(2, 0, 0) }, new DateTimeOffset(2024, 6, 17, 13, 30, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 7, 21, 2, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 3, DayOfTheWeek = DayOfWeek.Sunday, ScheduledRunTime = new TimeSpan(2, 0, 0) }, new DateTimeOffset(2024, 7, 21, 2, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 7, 21, 2, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 4, DayOfTheWeek = DayOfWeek.Friday, ScheduledRunTime = new TimeSpan(3, 30, 0) }, new DateTimeOffset(2024, 6, 12, 14, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 6, 28, 3, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 4, DayOfTheWeek = DayOfWeek.Friday, ScheduledRunTime = new TimeSpan(3, 30, 0) }, new DateTimeOffset(2024, 7, 26, 3, 30, 0, TimeSpan.Zero), new DateTimeOffset(2024, 7, 26, 3, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 4, DayOfTheWeek = DayOfWeek.Saturday, ScheduledRunTime = new TimeSpan(14, 0, 15) }, new DateTimeOffset(2024, 7, 13, 12, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 7, 27, 14, 0, 15, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 4, DayOfTheWeek = DayOfWeek.Saturday, ScheduledRunTime = new TimeSpan(14, 0, 15) }, new DateTimeOffset(2024, 7, 27, 15, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 8, 24, 14, 0, 15, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 4, DayOfTheWeek = DayOfWeek.Saturday, ScheduledRunTime = new TimeSpan(12, 0, 0) }, new DateTimeOffset(2024, 7, 6, 12, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 7, 27, 12, 0, 0, TimeSpan.Zero));
			});

			void TestFunction(NextRunTimeCalculatorMonthsByDayOfWeek nextRunTimeCalculator, DateTimeOffset calculateFrom, DateTimeOffset expectedResult)
			{
				// Act
				var result = nextRunTimeCalculator.CalculateNextRunTime(calculateFrom);

				// Assert
				Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorMonthsByDayOfWeekIsSerialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() => {
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 2, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = new TimeSpan(12, 0, 0) }, "<NextRunTimeCalculatorMonthsByDayOfWeek WeekOfTheMonth=\"2\" DayOfTheWeek=\"Monday\" ScheduledRunTime=\"12:00:00\" />");
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 3, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = new TimeSpan(10, 0, 0) }, "<NextRunTimeCalculatorMonthsByDayOfWeek WeekOfTheMonth=\"3\" DayOfTheWeek=\"Monday\" ScheduledRunTime=\"10:00:00\" />");
				TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 4, DayOfTheWeek = DayOfWeek.Tuesday, ScheduledRunTime = new TimeSpan(16, 45, 0) }, "<NextRunTimeCalculatorMonthsByDayOfWeek WeekOfTheMonth=\"4\" DayOfTheWeek=\"Tuesday\" ScheduledRunTime=\"16:45:00\" />");
			});
			void TestFunction(NextRunTimeCalculatorMonthsByDayOfWeek nextRunTimeCalculator, string expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Serialise<NextRunTimeCalculatorMonthsByDayOfWeek>(nextRunTimeCalculator);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorMonthsByDayOfWeekIsDeserialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction("<NextRunTimeCalculatorMonthsByDayOfWeek DayOfTheWeek=\"Monday\" WeekOfTheMonth=\"1\" ScheduledRunTime=\"12:00:00\"/>", new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = new TimeSpan(12, 0, 0) });
				TestFunction("<NextRunTimeCalculatorMonthsByDayOfWeek DayOfTheWeek=\"Tuesday\" WeekOfTheMonth=\"2\" ScheduledRunTime=\"04:00:00\"/>", new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 2, DayOfTheWeek = DayOfWeek.Tuesday, ScheduledRunTime = new TimeSpan(4, 0, 0) });
				TestFunction("<NextRunTimeCalculatorMonthsByDayOfWeek DayOfTheWeek=\"Friday\" WeekOfTheMonth=\"4\" ScheduledRunTime=\"13:00:00\"/>", new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 4, DayOfTheWeek = DayOfWeek.Friday, ScheduledRunTime = new TimeSpan(13, 0, 0) });
			});

			void TestFunction(string configuration, NextRunTimeCalculatorMonthsByDayOfWeek expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Deserialise<NextRunTimeCalculatorMonthsByDayOfWeek>(configuration);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
	}
}
