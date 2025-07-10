using System;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class NextRunTimeCalculatorMonthsByLastDayTest
	{
		[Test]
		public void TestCalculateNextRunTimeMonthsByDate()
		{
			// Arrange
			Assert.Multiple(() => {
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 1, ScheduledRunTime = new TimeSpan(9, 0, 0) }, new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 1, 31, 9, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 1, ScheduledRunTime = new TimeSpan(9, 0, 0) }, new DateTimeOffset(2024, 1, 31, 1, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 1, 31, 9, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 1, ScheduledRunTime = new TimeSpan(9, 0, 0) }, new DateTimeOffset(2024, 1, 31, 9, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 1, 31, 9, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 1, ScheduledRunTime = new TimeSpan(9, 0, 0) }, new DateTimeOffset(2024, 1, 31, 9, 30, 0, TimeSpan.Zero), new DateTimeOffset(2024, 2, 29, 9, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 2, ScheduledRunTime = new TimeSpan(9, 30, 0) }, new DateTimeOffset(2023, 1, 31, 0, 0, 0, TimeSpan.FromHours(10)), new DateTimeOffset(2023, 1, 31, 9, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 2, ScheduledRunTime = new TimeSpan(9, 30, 0) }, new DateTimeOffset(2023, 1, 31, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 3, 31, 9, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 12, ScheduledRunTime = new TimeSpan(18, 0, 0) }, new DateTimeOffset(2023, 2, 28, 18, 0, 1, TimeSpan.Zero), new DateTimeOffset(2024, 2, 29, 18, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 24, ScheduledRunTime = new TimeSpan(19, 0, 0) }, new DateTimeOffset(2024, 12, 1, 15, 10, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 12, 31, 19, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 24, ScheduledRunTime = new TimeSpan(19, 0, 0) }, new DateTimeOffset(2024, 12, 31, 20, 10, 0, TimeSpan.Zero), new DateTimeOffset(2026, 12, 31, 19, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 2, ScheduledRunTime = new TimeSpan(8, 0, 0) }, new DateTimeOffset(2024, 2, 10, 14, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 2, 29, 8, 0, 0, TimeSpan.Zero)); });

			void TestFunction(NextRunTimeCalculatorMonthsByLastDay nextRunTimeCalculator, DateTimeOffset calculateFrom, DateTimeOffset expectedResult)
			{
				// Act
				var result = nextRunTimeCalculator.CalculateNextRunTime(calculateFrom);

				// Assert
				Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorMonthsByDateIsSerialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() => {
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 1, ScheduledRunTime = new TimeSpan(12, 0, 0) }, "<NextRunTimeCalculatorMonthsByLastDay Period=\"1\" ScheduledRunTime=\"12:00:00\" />");
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 5, ScheduledRunTime = new TimeSpan(14, 0, 0) }, "<NextRunTimeCalculatorMonthsByLastDay Period=\"5\" ScheduledRunTime=\"14:00:00\" />");
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 15, ScheduledRunTime = new TimeSpan(2, 30, 30) }, "<NextRunTimeCalculatorMonthsByLastDay Period=\"15\" ScheduledRunTime=\"02:30:30\" />");
				TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 12, ScheduledRunTime = new TimeSpan(2, 30, 5) }, "<NextRunTimeCalculatorMonthsByLastDay Period=\"12\" ScheduledRunTime=\"02:30:05\" />");
			});
			void TestFunction(NextRunTimeCalculatorMonthsByLastDay nextRunTimeCalculator, string expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Serialise<NextRunTimeCalculatorMonthsByLastDay>(nextRunTimeCalculator);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorMonthsByDateIsDeserialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction("<NextRunTimeCalculatorMonthsByLastDay Period=\"1\" ScheduledRunTime=\"12:00:00\" />", new NextRunTimeCalculatorMonthsByLastDay() { Period = 1, ScheduledRunTime = new TimeSpan(12, 0, 0) });
				TestFunction("<NextRunTimeCalculatorMonthsByLastDay Period=\"3\" ScheduledRunTime=\"10:00:00\" />", new NextRunTimeCalculatorMonthsByLastDay() { Period = 3, ScheduledRunTime = new TimeSpan(10, 0, 0) });
				TestFunction("<NextRunTimeCalculatorMonthsByLastDay Period=\"4\" ScheduledRunTime=\"03:25:00\" />", new NextRunTimeCalculatorMonthsByLastDay() { Period = 4, ScheduledRunTime = new TimeSpan(3, 25, 0) });
				TestFunction("<NextRunTimeCalculatorMonthsByLastDay Period=\"10\" ScheduledRunTime=\"09:08:22\" />", new NextRunTimeCalculatorMonthsByLastDay() { Period = 10, ScheduledRunTime = new TimeSpan(9, 8, 22) });
			});

			void TestFunction(string configuration, NextRunTimeCalculatorMonthsByLastDay expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Deserialise<NextRunTimeCalculatorMonthsByLastDay>(configuration);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
	}
}
