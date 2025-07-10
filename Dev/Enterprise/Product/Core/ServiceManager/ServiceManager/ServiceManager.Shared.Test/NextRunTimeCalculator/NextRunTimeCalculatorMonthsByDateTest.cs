using System;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class NextRunTimeCalculatorMonthsByDateTest
	{
		[Test]
		public void TestCalculateNextRunTimeMonthsByDate()
		{
			// Arrange
			Assert.Multiple(() => {
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = 1, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2023, 12, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 12, 1, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = 1, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2023, 11, 15, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 12, 1, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = 1, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2023, 12, 1, 10, 45, 0, TimeSpan.Zero), new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 5, DayOfOccurrence = 21, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2024, 10, 21, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 10, 21, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 5, DayOfOccurrence = 15, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2024, 10, 25, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2025, 3, 15, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 5, DayOfOccurrence = 21, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2024, 10, 7, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 10, 21, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 5, DayOfOccurrence = 27, ScheduledRunTime = new TimeSpan(1, 0, 0) }, new DateTimeOffset(2024, 12, 31, 1, 0, 1, TimeSpan.Zero), new DateTimeOffset(2025, 5, 27, 1, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 3, DayOfOccurrence = 15, ScheduledRunTime = new TimeSpan(1, 30, 5) }, new DateTimeOffset(2023, 12, 15, 16, 0, 0, TimeSpan.FromHours(10)), new DateTimeOffset(2024, 3, 15, 1, 30, 5, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = 28, ScheduledRunTime = new TimeSpan(10, 30, 45) }, new DateTimeOffset(2023, 1, 31, 10, 40, 0, TimeSpan.Zero), new DateTimeOffset(2023, 2, 28, 10, 30, 45, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = 29, ScheduledRunTime = new TimeSpan(10, 30, 45) }, new DateTimeOffset(2023, 1, 31, 10, 40, 0, TimeSpan.Zero), new DateTimeOffset(2023, 3, 29, 10, 30, 45, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = 29, ScheduledRunTime = new TimeSpan(12, 0, 0) }, new DateTimeOffset(2024, 1, 29, 10, 30, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 2, 29, 12, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 6, DayOfOccurrence = 31, ScheduledRunTime = new TimeSpan(18, 0, 0) }, new DateTimeOffset(2024, 3, 10, 20, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 3, 31, 18, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 7, DayOfOccurrence = 31, ScheduledRunTime = new TimeSpan(18, 0, 0) }, new DateTimeOffset(2024, 3, 31, 19, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 10, 31, 18, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 2, DayOfOccurrence = 29, ScheduledRunTime = new TimeSpan(12, 0, 0) }, new DateTimeOffset(2022, 12, 29, 13, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 4, 29, 12, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = 31, ScheduledRunTime = new TimeSpan(12, 0, 0) }, new DateTimeOffset(2024, 3, 31, 13, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 5, 31, 12, 0, 0, TimeSpan.Zero));
			});

			void TestFunction(NextRunTimeCalculatorMonthsByDate nextRunTimeCalculator, DateTimeOffset calculateFrom, DateTimeOffset expectedResult)
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
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = 2, ScheduledRunTime = new TimeSpan(12, 0, 0) }, "<NextRunTimeCalculatorMonthsByDate Period=\"1\" DayOfOccurrence=\"2\" ScheduledRunTime=\"12:00:00\" />");
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 5, DayOfOccurrence = 1, ScheduledRunTime = new TimeSpan(14, 0, 0) }, "<NextRunTimeCalculatorMonthsByDate Period=\"5\" DayOfOccurrence=\"1\" ScheduledRunTime=\"14:00:00\" />");
				TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 15, DayOfOccurrence = 31, ScheduledRunTime = new TimeSpan(2, 30, 30) }, "<NextRunTimeCalculatorMonthsByDate Period=\"15\" DayOfOccurrence=\"31\" ScheduledRunTime=\"02:30:30\" />");
			});
			void TestFunction(NextRunTimeCalculatorMonthsByDate nextRunTimeCalculator, string expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Serialise<NextRunTimeCalculatorMonthsByDate>(nextRunTimeCalculator);

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
				TestFunction("<NextRunTimeCalculatorMonthsByDate Period=\"1\" DayOfOccurrence=\"1\" ScheduledRunTime=\"12:00:00\" />", new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = 1, ScheduledRunTime = new TimeSpan(12, 0, 0) });
				TestFunction("<NextRunTimeCalculatorMonthsByDate Period=\"3\" DayOfOccurrence=\"15\" ScheduledRunTime=\"10:00:00\" />", new NextRunTimeCalculatorMonthsByDate() { Period = 3, DayOfOccurrence = 15, ScheduledRunTime = new TimeSpan(10, 0, 0) });
				TestFunction("<NextRunTimeCalculatorMonthsByDate Period=\"4\" DayOfOccurrence=\"31\" ScheduledRunTime=\"13:00:00\" />", new NextRunTimeCalculatorMonthsByDate() { Period = 4, DayOfOccurrence = 31, ScheduledRunTime = new TimeSpan(13, 0, 0) });
			});

			void TestFunction(string configuration, NextRunTimeCalculatorMonthsByDate expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Deserialise<NextRunTimeCalculatorMonthsByDate>(configuration);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
	}
}
