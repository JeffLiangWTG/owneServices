using System;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class NextRunTimeCalculatorSecondsTest
	{
		[Test]
		public void TestCalculateNextRunTimeSeconds()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 1 }, new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 1, 1, 10, 0, 1, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 10 }, new DateTimeOffset(2024, 1, 2, 18, 59, 59, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 1, 3, 0, 0, 9, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 15, StartTime = new TimeSpan(9, 0, 0) }, new DateTimeOffset(2024, 2, 1, 8, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 2, 1, 9, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 20, StartTime = new TimeSpan(10, 30, 0) }, new DateTimeOffset(2024, 2, 2, 10, 29, 45, TimeSpan.Zero), new DateTimeOffset(2024, 2, 2, 10, 30, 5, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 30, StartTime = new TimeSpan(13, 45, 0) }, new DateTimeOffset(2024, 2, 3, 10, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 2, 3, 15, 0, 30, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 35, StartTime = new TimeSpan(16, 0, 0) }, new DateTimeOffset(2024, 2, 4, 23, 59, 24, TimeSpan.Zero), new DateTimeOffset(2024, 2, 4, 23, 59, 59, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 40, StartTime = new TimeSpan(4, 0, 20) }, new DateTimeOffset(2024, 2, 4, 23, 59, 20, TimeSpan.Zero), new DateTimeOffset(2024, 2, 5, 4, 0, 20, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 50, EndTime = new TimeSpan(20, 0, 0) }, new DateTimeOffset(2024, 3, 1, 12, 0, 0, TimeSpan.FromHours(10)), new DateTimeOffset(2024, 3, 1, 2, 0, 50, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 55, EndTime = new TimeSpan(16, 0, 55) }, new DateTimeOffset(2024, 3, 2, 16, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 3, 2, 16, 0, 55, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 60, EndTime = new TimeSpan(18, 0, 30) }, new DateTimeOffset(2024, 3, 3, 18, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 3, 4, 0, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 75, StartTime = new TimeSpan(12, 0, 0), EndTime = new TimeSpan(12, 0, 10) }, new DateTimeOffset(2024, 4, 1, 12, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 4, 2, 12, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 90, StartTime = new TimeSpan(8, 30, 0), EndTime = new TimeSpan(4, 30, 0) }, new DateTimeOffset(2024, 4, 2, 19, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 4, 2, 19, 1, 30, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 100, StartTime = new TimeSpan(13, 0, 30), EndTime = new TimeSpan(18, 0, 40) }, new DateTimeOffset(2024, 4, 3, 17, 59, 0, TimeSpan.Zero), new DateTimeOffset(2024, 4, 3, 18, 0, 40, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 120, StartTime = new TimeSpan(17, 0, 0), EndTime = new TimeSpan(5, 0, 0) }, new DateTimeOffset(2024, 4, 4, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 4, 4, 17, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 245, StartTime = new TimeSpan(20, 0, 0), EndTime = new TimeSpan(5, 0, 0) }, new DateTimeOffset(2024, 4, 6, 23, 59, 0, TimeSpan.Zero), new DateTimeOffset(2024, 4, 7, 0, 3, 5, TimeSpan.Zero));
			});

			void TestFunction(NextRunTimeCalculatorSeconds nextRunTimeCalculator, DateTimeOffset calculateFrom, DateTimeOffset expectedResult)
			{
				// Act
				var result = nextRunTimeCalculator.CalculateNextRunTime(calculateFrom);

				// Assert
				Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorSecondsIsSerialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() => {
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 3, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(16, 0, 0) }, "<NextRunTimeCalculatorSeconds Period=\"3\" StartTime=\"08:00:00\" EndTime=\"16:00:00\" />");
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 2, EndTime = new TimeSpan(16, 0, 0) }, "<NextRunTimeCalculatorSeconds Period=\"2\" EndTime=\"16:00:00\" />");
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 3, StartTime = new TimeSpan(4, 30, 0) }, "<NextRunTimeCalculatorSeconds Period=\"3\" StartTime=\"04:30:00\" />");
				TestFunction(new NextRunTimeCalculatorSeconds() { Period = 3 }, "<NextRunTimeCalculatorSeconds Period=\"3\" />");
			});
			void TestFunction(NextRunTimeCalculatorSeconds nextRunTimeCalculator, string expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Serialise<NextRunTimeCalculatorSeconds>(nextRunTimeCalculator);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorSecondsIsDeserialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction("<NextRunTimeCalculatorSeconds Period=\"2\" StartTime=\"10:00:00\" EndTime=\"15:00:00\"/>", new NextRunTimeCalculatorSeconds() { Period = 2, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(15, 0, 0) });
				TestFunction("<NextRunTimeCalculatorSeconds Period=\"1\" StartTime=\"06:00:00\"/>", new NextRunTimeCalculatorSeconds() { Period = 1, StartTime = new TimeSpan(6, 0, 0) });
				TestFunction("<NextRunTimeCalculatorSeconds Period=\"2\" EndTime=\"15:00:00\" />", new NextRunTimeCalculatorSeconds() { Period = 2, EndTime = new TimeSpan(15, 0, 0) });
				TestFunction("<NextRunTimeCalculatorSeconds Period=\"4\"/>", new NextRunTimeCalculatorSeconds() { Period = 4 });
			});

			void TestFunction(string configuration, NextRunTimeCalculatorSeconds expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Deserialise<NextRunTimeCalculatorSeconds>(configuration);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
	}
}
