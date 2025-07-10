using System;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class NextRunTimeCalculatorMinutesTest
	{
		[Test]
		public void TestCalculateNextRunTimeMinutes()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 1 }, new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 1, 1, 12, 1, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 5 }, new DateTimeOffset(2024, 1, 1, 23, 55, 0, TimeSpan.Zero), new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 10, StartTime = new TimeSpan(9, 30, 0) }, new DateTimeOffset(2024, 1, 31, 23, 59, 0, TimeSpan.Zero), new DateTimeOffset(2024, 2, 1, 9, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 15, StartTime = new TimeSpan(0, 13, 0) }, new DateTimeOffset(2024, 1, 31, 23, 59, 0, TimeSpan.Zero), new DateTimeOffset(2024, 2, 1, 0, 14, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 30, StartTime = new TimeSpan(14, 45, 35) }, new DateTimeOffset(2024, 2, 4, 4, 0, 35, TimeSpan.FromHours(10)), new DateTimeOffset(2024, 2, 3, 18, 30, 35, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 35, EndTime = new TimeSpan(16, 0, 0) }, new DateTimeOffset(2024, 3, 1, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 3, 1, 10, 35, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 42, EndTime = new TimeSpan(8, 42, 30) }, new DateTimeOffset(2024, 3, 2, 8, 0, 30, TimeSpan.Zero), new DateTimeOffset(2024, 3, 2, 8, 42, 30, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 45, EndTime = new TimeSpan(9, 30, 0) }, new DateTimeOffset(2024, 3, 3, 4, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 3, 4, 0, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 65, StartTime = new TimeSpan(9, 30, 0), EndTime = new TimeSpan(15, 0, 0) }, new DateTimeOffset(2024, 4, 1, 10, 35, 0, TimeSpan.Zero), new DateTimeOffset(2024, 4, 1, 11, 40, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 75, StartTime = new TimeSpan(14, 30, 0), EndTime = new TimeSpan(9, 15, 0) }, new DateTimeOffset(2024, 4, 2, 13, 15, 0, TimeSpan.Zero), new DateTimeOffset(2024, 4, 2, 14, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 90, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(2, 30, 0) }, new DateTimeOffset(2024, 4, 4, 9, 0, 0, TimeSpan.FromHours(10)), new DateTimeOffset(2024, 4, 4, 0, 30, 0, TimeSpan.Zero));
			});

			void TestFunction(NextRunTimeCalculatorMinutes nextRunTimeCalculator, DateTimeOffset calculateFrom, DateTimeOffset expectedResult)
			{
				// Act
				var result = nextRunTimeCalculator.CalculateNextRunTime(calculateFrom);

				// Assert
				Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorMinutesIsSerialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 3, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(16, 0, 0) }, "<NextRunTimeCalculatorMinutes Period=\"3\" StartTime=\"08:00:00\" EndTime=\"16:00:00\" />");
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 2, EndTime = new TimeSpan(16, 0, 0) }, "<NextRunTimeCalculatorMinutes Period=\"2\" EndTime=\"16:00:00\" />");
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 3, StartTime = new TimeSpan(4, 30, 0) }, "<NextRunTimeCalculatorMinutes Period=\"3\" StartTime=\"04:30:00\" />");
				TestFunction(new NextRunTimeCalculatorMinutes() { Period = 3 }, "<NextRunTimeCalculatorMinutes Period=\"3\" />");
			});
			void TestFunction(NextRunTimeCalculatorMinutes nextRunTimeCalculator, string expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Serialise<NextRunTimeCalculatorMinutes>(nextRunTimeCalculator);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorMinutesIsDeserialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction("<NextRunTimeCalculatorMinutes Period=\"2\" StartTime=\"10:00:00\" EndTime=\"15:00:00\"/>", new NextRunTimeCalculatorMinutes() { Period = 2, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(15, 0, 0) });
				TestFunction("<NextRunTimeCalculatorMinutes Period=\"1\" StartTime=\"06:00:00\"/>", new NextRunTimeCalculatorMinutes() { Period = 1, StartTime = new TimeSpan(6, 0, 0) });
				TestFunction("<NextRunTimeCalculatorMinutes Period=\"2\" EndTime=\"15:00:00\"/>", new NextRunTimeCalculatorMinutes() { Period = 2, EndTime = new TimeSpan(15, 0, 0) });
				TestFunction("<NextRunTimeCalculatorMinutes Period=\"4\" />", new NextRunTimeCalculatorMinutes() { Period = 4 });
			});

			void TestFunction(string configuration, NextRunTimeCalculatorMinutes expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Deserialise<NextRunTimeCalculatorMinutes>(configuration);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
	}
}
