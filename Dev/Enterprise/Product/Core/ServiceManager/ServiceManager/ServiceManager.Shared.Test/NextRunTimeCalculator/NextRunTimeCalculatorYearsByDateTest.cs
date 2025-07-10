using System;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class NextRunTimeCalculatorYearsByDateTest
	{
		[Test]
		public void TestCalculateNextRunTimeYearsByDate()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 1, Day = 1, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2023, 1, 1, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 1, 1, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 1, Day = 1, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2023, 1, 1, 9, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 1, 1, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 1, Day = 1, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2023, 1, 1, 11, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 2, Day = 28, ScheduledRunTime = new TimeSpan(7, 45, 0) }, new DateTimeOffset(2023, 2, 28, 8, 30, 0, TimeSpan.Zero), new DateTimeOffset(2024, 2, 28, 7, 45, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 2, Day = 29, ScheduledRunTime = new TimeSpan(7, 45, 0) }, new DateTimeOffset(2023, 2, 28, 8, 30, 0, TimeSpan.Zero), new DateTimeOffset(2024, 2, 29, 7, 45, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 2, Day = 29, ScheduledRunTime = new TimeSpan(11, 30, 0) }, new DateTimeOffset(2024, 2, 9, 22, 30, 0, TimeSpan.FromHours(10)), new DateTimeOffset(2024, 2, 29, 11, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 2, Day = 29, ScheduledRunTime = new TimeSpan(11, 30, 0) }, new DateTimeOffset(2024, 3, 1, 21, 30, 0, TimeSpan.FromHours(10)), new DateTimeOffset(2028, 2, 29, 11, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 6, Day = 30, ScheduledRunTime = new TimeSpan(16, 45, 30) }, new DateTimeOffset(2024, 6, 29, 11, 45, 30, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 6, 30, 16, 45, 30, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 6, Day = 30, ScheduledRunTime = new TimeSpan(16, 45, 30) }, new DateTimeOffset(2024, 6, 28, 0, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 6, 30, 16, 45, 30, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 6, Day = 30, ScheduledRunTime = new TimeSpan(16, 45, 30) }, new DateTimeOffset(2024, 7, 1, 14, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2025, 6, 30, 16, 45, 30, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 12, Day = 14, ScheduledRunTime = new TimeSpan(19, 0, 0) }, new DateTimeOffset(2024, 10, 11, 19, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 12, 14, 19, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 12, Day = 14, ScheduledRunTime = new TimeSpan(19, 0, 0) }, new DateTimeOffset(2023, 12, 31, 1, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 12, 14, 19, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 10, Day = 24, ScheduledRunTime = new TimeSpan(1, 0, 0) }, new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 10, 24, 1, 0, 0, TimeSpan.Zero));
			});

			void TestFunction(NextRunTimeCalculatorYearsByDate nextRunTimeCalculator, DateTimeOffset calculateFrom, DateTimeOffset expectedResult)
			{
				// Act
				var result = nextRunTimeCalculator.CalculateNextRunTime(calculateFrom);

				// Assert
				Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorYearsByDateIsSerialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() => {
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 12, Day = 31, ScheduledRunTime = new TimeSpan(12, 0, 0) }, "<NextRunTimeCalculatorYearsByDate Month=\"12\" Day=\"31\" ScheduledRunTime=\"12:00:00\" />");
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 1, Day = 1, ScheduledRunTime = new TimeSpan(8, 0, 0) }, "<NextRunTimeCalculatorYearsByDate Month=\"1\" Day=\"1\" ScheduledRunTime=\"08:00:00\" />");
				TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 6, Day = 8, ScheduledRunTime = new TimeSpan(16, 0, 40) }, "<NextRunTimeCalculatorYearsByDate Month=\"6\" Day=\"8\" ScheduledRunTime=\"16:00:40\" />");
			});
			void TestFunction(NextRunTimeCalculatorYearsByDate nextRunTimeCalculator, string expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Serialise<NextRunTimeCalculatorYearsByDate>(nextRunTimeCalculator);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorYearsByDateIsDeserialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction("<NextRunTimeCalculatorYearsByDate Month=\"12\" Day=\"31\" ScheduledRunTime=\"12:00:00\"/>", new NextRunTimeCalculatorYearsByDate() { Month = 12, Day = 31, ScheduledRunTime = new TimeSpan(12, 0, 0) });
				TestFunction("<NextRunTimeCalculatorYearsByDate Month=\"1\" Day=\"1\" ScheduledRunTime=\"10:00:00\"/>", new NextRunTimeCalculatorYearsByDate() { Month = 1, Day = 1, ScheduledRunTime = new TimeSpan(10, 0, 0) });
				TestFunction("<NextRunTimeCalculatorYearsByDate Month=\"5\" Day=\"10\" ScheduledRunTime=\"16:00:20\"/>", new NextRunTimeCalculatorYearsByDate() { Month = 5, Day = 10, ScheduledRunTime = new TimeSpan(16, 0, 20) });
			});

			void TestFunction(string configuration, NextRunTimeCalculatorYearsByDate expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Deserialise<NextRunTimeCalculatorYearsByDate>(configuration);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
	}
}
