using System;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class NextRunTimeCalculatorHoursTest
	{
		[Test]
		public void TestCalculateNextRunTimeHours()
		{
			// Arrange			
			Assert.Multiple(() =>
			{
				TestFunction(new NextRunTimeCalculatorHours() { Period = 1 }, new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 1, 1, 1, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 1 }, new DateTimeOffset(2023, 1, 2, 13, 30, 15, TimeSpan.Zero), new DateTimeOffset(2023, 1, 2, 14, 30, 15, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 2 }, new DateTimeOffset(2024, 1, 3, 17, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 1, 4, 0, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 3 }, new DateTimeOffset(2024, 2, 4, 22, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 2, 5, 1, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 16, StartTime = new TimeSpan(0, 0, 0) }, new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 2, 1, 16, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 3, StartTime = new TimeSpan(8, 0, 0) }, new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 2, 1, 8, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 3, StartTime = new TimeSpan(7, 0, 0) }, new DateTimeOffset(2024, 2, 1, 2, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 2, 1, 7, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 3, StartTime = new TimeSpan(6, 30, 0) }, new DateTimeOffset(2024, 2, 2, 13, 30, 0, TimeSpan.FromHours(10)), new DateTimeOffset(2024, 2, 2, 6, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 4, StartTime = new TimeSpan(14, 30, 45) }, new DateTimeOffset(2023, 2, 3, 13, 0, 0, TimeSpan.Zero), new DateTimeOffset(2023, 2, 3, 17, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 4, StartTime = new TimeSpan(4, 0, 0) }, new DateTimeOffset(2024, 2, 3, 23, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 2, 4, 8, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 5, StartTime = new TimeSpan(9, 45, 0) }, new DateTimeOffset(2025, 2, 5, 12, 0, 0, TimeSpan.Zero), new DateTimeOffset(2025, 2, 5, 17, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 5, StartTime = new TimeSpan(20, 0, 0) }, new DateTimeOffset(2024, 4, 16, 19, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 4, 17, 20, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 6, StartTime = new TimeSpan(20, 45, 15) }, new DateTimeOffset(2024, 4, 17, 18, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 4, 18, 20, 45, 15, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 6, StartTime = new TimeSpan(18, 0, 0) }, new DateTimeOffset(2024, 4, 18, 18, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 4, 19, 18, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 6, StartTime = new TimeSpan(23, 30, 0) }, new DateTimeOffset(2024, 4, 20, 23, 30, 0, TimeSpan.Zero), new DateTimeOffset(2024, 4, 21, 23, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 7, StartTime = new TimeSpan(9, 0, 30) }, new DateTimeOffset(2025, 4, 21, 17, 0, 0, TimeSpan.Zero), new DateTimeOffset(2025, 4, 22, 9, 0, 30, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 7, StartTime = new TimeSpan(4, 30, 0) }, new DateTimeOffset(2024, 4, 25, 17, 0, 1, TimeSpan.Zero), new DateTimeOffset(2024, 4, 26, 4, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 8, StartTime = new TimeSpan(3, 0, 0) }, new DateTimeOffset(2024, 4, 25, 22, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 4, 26, 6, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 3, EndTime = new TimeSpan(14, 0, 0) }, new DateTimeOffset(2024, 2, 29, 16, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 3, EndTime = new TimeSpan(14, 30, 0) }, new DateTimeOffset(2024, 3, 1, 21, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 3, 2, 0, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 4, EndTime = new TimeSpan(15, 0, 0) }, new DateTimeOffset(2024, 3, 2, 20, 0, 30, TimeSpan.Zero), new DateTimeOffset(2024, 3, 3, 0, 0, 30, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 4, EndTime = new TimeSpan(16, 45, 15) }, new DateTimeOffset(2024, 3, 4, 10, 0, 0, TimeSpan.FromHours(10)), new DateTimeOffset(2024, 3, 4, 4, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 5, EndTime = new TimeSpan(8, 30, 0) }, new DateTimeOffset(2025, 5, 9, 22, 30, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2025, 5, 10, 8, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 5, EndTime = new TimeSpan(19, 45, 0) }, new DateTimeOffset(2024, 5, 11, 14, 45, 0, TimeSpan.Zero), new DateTimeOffset(2024, 5, 11, 19, 45, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 6, EndTime = new TimeSpan(15, 30, 15) }, new DateTimeOffset(2024, 5, 12, 5, 45, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 5, 13, 0, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 13, EndTime = new TimeSpan(13, 0, 0) }, new DateTimeOffset(2024, 5, 17, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 5, 17, 13, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 15, EndTime = new TimeSpan(8, 30, 0) }, new DateTimeOffset(2024, 5, 18, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 5, 19, 0, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 17, EndTime = new TimeSpan(0, 0, 0) }, new DateTimeOffset(2024, 5, 19, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 5, 20, 0, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 18, EndTime = new TimeSpan(8, 30, 0) }, new DateTimeOffset(2024, 5, 20, 6, 0, 0, TimeSpan.FromHours(10)), new DateTimeOffset(2024, 5, 21, 0, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 3, StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(9, 0, 0) }, new DateTimeOffset(2024, 4, 1, 6, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 4, 1, 9, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 5, StartTime = new TimeSpan(7, 30, 0), EndTime = new TimeSpan(17, 0, 0) }, new DateTimeOffset(2024, 4, 2, 9, 0, 30, TimeSpan.Zero), new DateTimeOffset(2024, 4, 2, 14, 0, 30, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 6, StartTime = new TimeSpan(12, 30, 0), EndTime = new TimeSpan(14, 30, 0) }, new DateTimeOffset(2023, 4, 3, 21, 0, 0, TimeSpan.FromHours(10)), new DateTimeOffset(2023, 4, 4, 12, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 4, StartTime = new TimeSpan(19, 0, 0), EndTime = new TimeSpan(4, 0, 0) }, new DateTimeOffset(2024, 7, 2, 19, 30, 0, TimeSpan.Zero), new DateTimeOffset(2024, 7, 2, 23, 30, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 8, StartTime = new TimeSpan(22, 0, 0), EndTime = new TimeSpan(9, 0, 0) }, new DateTimeOffset(2024, 7, 3, 23, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 7, 4, 7, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 9, StartTime = new TimeSpan(19, 0, 0), EndTime = new TimeSpan(8, 30, 0) }, new DateTimeOffset(2024, 7, 5, 2, 30, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 7, 5, 19, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorHours() { Period = 1, StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(14, 0, 0) }, new DateTimeOffset(2024, 7, 10, 14, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 7, 11, 14, 0, 0, TimeSpan.Zero));
			});

			void TestFunction(NextRunTimeCalculatorHours nextRunTimeCalculator, DateTimeOffset calculateFrom, DateTimeOffset expectedResult)
			{ 
				// Act
				var result = nextRunTimeCalculator.CalculateNextRunTime(calculateFrom);

				// Assert
				Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorHoursIsSerialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() => {
				TestFunction(new NextRunTimeCalculatorHours() { Period = 3, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(16, 0, 0) }, "<NextRunTimeCalculatorHours Period=\"3\" StartTime=\"08:00:00\" EndTime=\"16:00:00\" />");
				TestFunction(new NextRunTimeCalculatorHours() { Period = 3, EndTime = new TimeSpan(16, 0, 0) }, "<NextRunTimeCalculatorHours Period=\"3\" EndTime=\"16:00:00\" />");
				TestFunction(new NextRunTimeCalculatorHours() { Period = 11, StartTime = new TimeSpan(8, 0, 0) }, "<NextRunTimeCalculatorHours Period=\"11\" StartTime=\"08:00:00\" />");
				TestFunction(new NextRunTimeCalculatorHours() { Period = 3 }, "<NextRunTimeCalculatorHours Period=\"3\" />");
			});
			void TestFunction(NextRunTimeCalculatorHours nextRunTimeCalculator, string expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Serialise<NextRunTimeCalculatorHours>(nextRunTimeCalculator);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorHoursIsDeserialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction("<NextRunTimeCalculatorHours Period=\"2\" StartTime=\"10:00:00\" EndTime=\"15:00:00\"/>", new NextRunTimeCalculatorHours() { Period = 2, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(15, 0, 0) });
				TestFunction("<NextRunTimeCalculatorHours Period=\"2\" StartTime =\"10:00:00\"/>", new NextRunTimeCalculatorHours() { Period = 2, StartTime = new TimeSpan(10, 0, 0) });
				TestFunction("<NextRunTimeCalculatorHours Period=\"4\" EndTime=\"13:00:00\"/>", new NextRunTimeCalculatorHours() { Period = 4, EndTime = new TimeSpan(13, 0, 0) });
				TestFunction("<NextRunTimeCalculatorHours Period=\"4\" />", new NextRunTimeCalculatorHours() { Period = 4 });
			});

			void TestFunction(string configuration, NextRunTimeCalculatorHours expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Deserialise<NextRunTimeCalculatorHours>(configuration);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
	}
}
