using System;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class NextRunTimeCalculatorDaysTest
	{
		[Test]
		public void TestCalculateNextRunTimeDays()
		{
			// Arrange

			Assert.Multiple(() =>
			{
				TestFunction(new NextRunTimeCalculatorDays() { Period = 1, ScheduledRunTime = new TimeSpan(10, 0, 0) }, new DateTimeOffset(2024, 1, 1, 10, 0, 1, TimeSpan.Zero), new DateTimeOffset(2024, 1, 2, 10, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorDays() { Period = 5, ScheduledRunTime = new TimeSpan(9, 30, 0) }, new DateTimeOffset(2024, 1, 2, 3, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 1, 2, 9, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorDays() { Period = 10, ScheduledRunTime = new TimeSpan(16, 30, 0) }, new DateTimeOffset(2024, 1, 12, 17, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 1, 22, 16, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorDays() { Period = 20, ScheduledRunTime = new TimeSpan(0, 0, 1) }, new DateTimeOffset(2024, 1, 31, 19, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 2, 1, 0, 0, 1, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorDays() { Period = 33, ScheduledRunTime = new TimeSpan(14, 0, 0) }, new DateTimeOffset(2024, 2, 4, 9, 30, 0, TimeSpan.FromHours(10)), new DateTimeOffset(2024, 3, 7, 14, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorDays() { Period = 50, ScheduledRunTime = new TimeSpan(11, 0, 0) }, new DateTimeOffset(2024, 6, 20, 21, 0, 0, TimeSpan.FromHours(10)), new DateTimeOffset(2024, 6, 20, 11, 0, 0, TimeSpan.Zero));
			});

			void TestFunction(NextRunTimeCalculatorDays nextRunTimeCalculator, DateTimeOffset calculateFrom, DateTimeOffset expectedResult)
			{
				// Act
				var result = nextRunTimeCalculator.CalculateNextRunTime(calculateFrom);

				// Assert
				Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorDaysIsSerialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() => {
				TestFunction(new NextRunTimeCalculatorDays() { Period = 1, ScheduledRunTime = new TimeSpan(12, 0, 0) }, "<NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"12:00:00\" />");
				TestFunction(new NextRunTimeCalculatorDays() { Period = 31, ScheduledRunTime = new TimeSpan(10, 0, 0) }, "<NextRunTimeCalculatorDays Period=\"31\" ScheduledRunTime=\"10:00:00\" />");
				TestFunction(new NextRunTimeCalculatorDays() { Period = 100, ScheduledRunTime = new TimeSpan(9, 0, 0) }, "<NextRunTimeCalculatorDays Period=\"100\" ScheduledRunTime=\"09:00:00\" />");
			});
			void TestFunction(NextRunTimeCalculatorDays nextRunTimeCalculator, string expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Serialise<NextRunTimeCalculatorDays>(nextRunTimeCalculator);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorDaysIsDeserialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction("<NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"12:00:00\"/>", new NextRunTimeCalculatorDays() { Period = 1, ScheduledRunTime = new TimeSpan(12, 0, 0) });
				TestFunction("<NextRunTimeCalculatorDays Period=\"20\" ScheduledRunTime=\"10:00:00\"/>", new NextRunTimeCalculatorDays() { Period = 20, ScheduledRunTime = new TimeSpan(10, 0, 0) });
				TestFunction("<NextRunTimeCalculatorDays Period=\"140\" ScheduledRunTime=\"14:00:00\"/>", new NextRunTimeCalculatorDays() { Period = 140, ScheduledRunTime = new TimeSpan(14, 0, 0) });
				TestFunction("<NextRunTimeCalculatorDays Period=\"99\" ScheduledRunTime=\"06:52:17\"/>", new NextRunTimeCalculatorDays() { Period = 99, ScheduledRunTime = new TimeSpan(6, 52, 17) });
			});

			void TestFunction(string configuration, NextRunTimeCalculatorDays expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Deserialise<NextRunTimeCalculatorDays>(configuration);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
	}
}
