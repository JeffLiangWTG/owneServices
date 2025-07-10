using System;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class NextRunTimeCalculatorWorkingDaysTest
	{
		[Test]
		public void TestCalculateNextRunTimeWorkingDays()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction(new NextRunTimeCalculatorWorkingDays() { ScheduledRunTime = new TimeSpan(16, 45, 0) }, new DateTimeOffset(2024, 2, 1, 13, 30, 0, TimeSpan.FromHours(10)), new DateTimeOffset(2024, 2, 1, 16, 45, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorWorkingDays() { ScheduledRunTime = new TimeSpan(9, 30, 30) }, new DateTimeOffset(2024, 5, 31, 11, 30, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 6, 3, 9, 30, 30, TimeSpan.Zero));
			});

			void TestFunction(NextRunTimeCalculatorWorkingDays nextRunTimeCalculator, DateTimeOffset calculateFrom, DateTimeOffset expectedResult)
			{
				// Act
				var result = nextRunTimeCalculator.CalculateNextRunTime(calculateFrom);

				// Assert
				Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorWorkingDaysIsSerialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() => {
				TestFunction(new NextRunTimeCalculatorWorkingDays() { ScheduledRunTime = new TimeSpan(13, 30, 0) }, "<NextRunTimeCalculatorWorkingDays ScheduledRunTime=\"13:30:00\" />");
				TestFunction(new NextRunTimeCalculatorWorkingDays() { ScheduledRunTime = new TimeSpan(11, 0, 0) }, "<NextRunTimeCalculatorWorkingDays ScheduledRunTime=\"11:00:00\" />");
				TestFunction(new NextRunTimeCalculatorWorkingDays() { ScheduledRunTime = new TimeSpan(16, 0, 0) }, "<NextRunTimeCalculatorWorkingDays ScheduledRunTime=\"16:00:00\" />");
			});
			void TestFunction(NextRunTimeCalculatorWorkingDays nextRunTimeCalculator, string expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Serialise<NextRunTimeCalculatorWorkingDays>(nextRunTimeCalculator);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorWorkingDaysIsDeserialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction("<NextRunTimeCalculatorWorkingDays ScheduledRunTime=\"12:00:00\" />", new NextRunTimeCalculatorWorkingDays() { ScheduledRunTime = new TimeSpan(12, 0, 0) });
				TestFunction("<NextRunTimeCalculatorWorkingDays ScheduledRunTime=\"10:00:00\" />", new NextRunTimeCalculatorWorkingDays() { ScheduledRunTime = new TimeSpan(10, 0, 0) });
				TestFunction("<NextRunTimeCalculatorWorkingDays ScheduledRunTime=\"18:00:00\" />", new NextRunTimeCalculatorWorkingDays() { ScheduledRunTime = new TimeSpan(18, 0, 0) });
			});

			void TestFunction(string configuration, NextRunTimeCalculatorWorkingDays expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Deserialise<NextRunTimeCalculatorWorkingDays>(configuration);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
	}
}
