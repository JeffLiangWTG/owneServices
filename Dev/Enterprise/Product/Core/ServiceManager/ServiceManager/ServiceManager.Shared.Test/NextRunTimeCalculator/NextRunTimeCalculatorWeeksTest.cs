using System;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class NextRunTimeCalculatorWeeksTest
	{
		[Test]
		public void TestCalculateRunTimeWeeks()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 2, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Tuesday, DayOfWeek.Thursday }, ScheduledRunTime = new TimeSpan(3, 0, 0) }, new DateTimeOffset(2024, 2, 1, 22, 45, 0, TimeSpan.FromHours(11)), new DateTimeOffset(2024, 2, 13, 3, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 2, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Tuesday, DayOfWeek.Thursday }, ScheduledRunTime = new TimeSpan(5, 45, 0) }, new DateTimeOffset(2024, 2, 13, 3, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 2, 13, 5, 45, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 2, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Tuesday, DayOfWeek.Thursday }, ScheduledRunTime = new TimeSpan(5, 45, 0) }, new DateTimeOffset(2024, 2, 13, 6, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 2, 15, 5, 45, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 5, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Saturday, DayOfWeek.Sunday }, ScheduledRunTime = new TimeSpan(14, 45, 0) }, new DateTimeOffset(2024, 5, 4, 11, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 5, 5, 14, 45, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 5, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Saturday, DayOfWeek.Sunday }, ScheduledRunTime = new TimeSpan(1, 0, 0) }, new DateTimeOffset(2024, 5, 5, 2, 0, 0, TimeSpan.Zero), new DateTimeOffset(2024, 6, 8, 1, 0, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 5, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Sunday, DayOfWeek.Saturday }, ScheduledRunTime = new TimeSpan(14, 45, 0) }, new DateTimeOffset(2024, 6, 2, 8, 45, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 6, 2, 14, 45, 0, TimeSpan.Zero));
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 5, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Sunday, DayOfWeek.Saturday }, ScheduledRunTime = new TimeSpan(18, 0, 0) }, new DateTimeOffset(2024, 6, 2, 18, 0, 0, TimeSpan.FromHours(-5)), new DateTimeOffset(2024, 7, 6, 18, 0, 0, TimeSpan.Zero));
			});

			void TestFunction(NextRunTimeCalculatorWeeks nextRunTimeCalculator, DateTimeOffset calculateFrom, DateTimeOffset expectedResult)
			{
				// Act
				var result = nextRunTimeCalculator.CalculateNextRunTime(calculateFrom);

				// Assert
				Assert.That(result.Offset, Is.EqualTo(TimeSpan.Zero));
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestCalculateRunTimeWeeks_WithoutDaysOfOccurrenceDefaultsToCalculateFromDate()
		{
			// Arrange
			var calculator = new NextRunTimeCalculatorWeeks()
			{
				Period = 1,
				DaysOfOccurrence = Array.Empty<DayOfWeek>(),
				ScheduledRunTime = new TimeSpan(12, 0, 0)
			};

			var expectedDate = DateTimeOffset.UtcNow;

			// Act
			var result = calculator.CalculateNextRunTime(expectedDate);

			// Assert
			Assert.That(result, Is.EqualTo(expectedDate));
		}

		[Test]
		public void TestDefaultDaysOfOccurrenceValueIsEmpty()
		{
			//Arrange
			var calculator = new NextRunTimeCalculatorWeeks()
			{
				Period = 1,
				ScheduledRunTime = new TimeSpan(10, 0, 0)
			};

			// Act
			// Assert
			Assert.That(calculator.DaysOfOccurrence, Is.EqualTo(Array.Empty<DayOfWeek>()));
		}

		[Test]
		public void TestNextRunTimeCalculatorWeeksIsSerialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() => {
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 1, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Thursday }, ScheduledRunTime = new TimeSpan(13, 30, 0) }, "<NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"13:30:00\"><DaysOfOccurrence><DayOfWeek>Monday</DayOfWeek><DayOfWeek>Thursday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks>");
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 2, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Tuesday, DayOfWeek.Wednesday }, ScheduledRunTime = new TimeSpan(8, 30, 0) }, "<NextRunTimeCalculatorWeeks Period=\"2\" ScheduledRunTime=\"08:30:00\"><DaysOfOccurrence><DayOfWeek>Tuesday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks>");
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 4, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Friday, DayOfWeek.Saturday }, ScheduledRunTime = new TimeSpan(16, 30, 0) }, "<NextRunTimeCalculatorWeeks Period=\"4\" ScheduledRunTime=\"16:30:00\"><DaysOfOccurrence><DayOfWeek>Friday</DayOfWeek><DayOfWeek>Saturday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks>");
			});
			void TestFunction(NextRunTimeCalculatorWeeks nextRunTimeCalculator, string expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Serialise<NextRunTimeCalculatorWeeks>(nextRunTimeCalculator);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorWeeksIsDeserialisedCorrectly()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction("<NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"12:00:00\"><DaysOfOccurrence><DayOfWeek>Monday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Friday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks>", new NextRunTimeCalculatorWeeks() { Period = 1, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }, ScheduledRunTime = new TimeSpan(12, 0, 0) });
				TestFunction("<NextRunTimeCalculatorWeeks Period=\"2\" ScheduledRunTime=\"10:00:00\"><DaysOfOccurrence><DayOfWeek>Tuesday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks>", new NextRunTimeCalculatorWeeks() { Period = 2, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Tuesday }, ScheduledRunTime = new TimeSpan(10, 0, 0) });
				TestFunction("<NextRunTimeCalculatorWeeks Period=\"4\" ScheduledRunTime=\"14:00:00\"><DaysOfOccurrence><DayOfWeek>Thursday</DayOfWeek><DayOfWeek>Sunday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks>", new NextRunTimeCalculatorWeeks() { Period = 4, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Thursday, DayOfWeek.Sunday }, ScheduledRunTime = new TimeSpan(14, 0, 0), });
			});

			void TestFunction(string configuration, NextRunTimeCalculatorWeeks expectedResult)
			{
				// Act
				var result = CalculatorTestSerialiser.Deserialise<NextRunTimeCalculatorWeeks>(configuration);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}

		[Test]
		public void TestNextRunTimeCalculatorWeeksEquals()
		{
			// Arrange
			Assert.Multiple(() =>
			{
				TestFunction(new NextRunTimeCalculatorWeeks(), new NextRunTimeCalculatorWeeks(), true);
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 10 }, new NextRunTimeCalculatorWeeks() { Period = 10 }, true);
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 1 }, new NextRunTimeCalculatorWeeks() { Period = 2 }, false);
				TestFunction(new NextRunTimeCalculatorWeeks() { DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Friday, DayOfWeek.Saturday } }, new NextRunTimeCalculatorWeeks() { DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Friday, DayOfWeek.Saturday } }, true);
				TestFunction(new NextRunTimeCalculatorWeeks() { DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Friday, DayOfWeek.Saturday } }, new NextRunTimeCalculatorWeeks() { DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Friday, DayOfWeek.Saturday } }, false);
				TestFunction(new NextRunTimeCalculatorWeeks() { ScheduledRunTime = new TimeSpan(10, 59, 59) }, new NextRunTimeCalculatorWeeks() { ScheduledRunTime = new TimeSpan(10, 59, 59) }, true);
				TestFunction(new NextRunTimeCalculatorWeeks() { ScheduledRunTime = new TimeSpan(10, 59, 59) }, new NextRunTimeCalculatorWeeks() { ScheduledRunTime = new TimeSpan(10, 59, 58) }, false);
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 1, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }, ScheduledRunTime = new TimeSpan(12, 0, 0) }, new NextRunTimeCalculatorWeeks() { Period = 1, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }, ScheduledRunTime = new TimeSpan(12, 0, 0) }, true);
				TestFunction(new NextRunTimeCalculatorWeeks() { Period = 10, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Saturday, DayOfWeek.Sunday }, ScheduledRunTime = new TimeSpan(12, 0, 0) }, new NextRunTimeCalculatorWeeks() { Period = 5, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }, ScheduledRunTime = new TimeSpan(12, 0, 0) }, false);
			});

			void TestFunction(NextRunTimeCalculatorWeeks calculator1, NextRunTimeCalculatorWeeks calculator2, bool expectedResult)
			{
				// Act
				var result = calculator1.Equals(calculator2);

				// Assert
				Assert.That(result, Is.EqualTo(expectedResult));
			}
		}
	}
}
