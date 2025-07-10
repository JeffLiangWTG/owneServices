using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ServiceManager.Shared.CW.Test;

class ServiceTaskScheduleHelperTest
{
	class GetPeriodDurationTest : TestCase
	{
		public void TestInvalidString()
		{
			// Arrange
			var testData = new List<string>()
			{
				"",
				null,
				"abc",
				"Seconds",
				"Y",
				"20SS",
			};

			CombineAssertions(() =>
			{
				testData.ForEach(testValue =>
				{
					// Act
					var result =
						ServiceTaskScheduleHelper.GetPeriodDuration(testValue, TimeSpan.MaxValue,
							isRandomPeriod: false);

					// Assert
					AssertEquals($"{testValue} should not be parsed", TimeSpan.MaxValue, result);
				});
			});
		}

		public void TestCorrectTimeSpan()
		{
			// Arrange
			var testData = new List<(string, TimeSpan)>()
			{
				("1s", TimeSpan.FromSeconds(1)),
				("10s", TimeSpan.FromSeconds(10)),
				("2m", TimeSpan.FromMinutes(2)),
				("15m", TimeSpan.FromMinutes(15)),
				("3h", TimeSpan.FromHours(3)),
				("13h", TimeSpan.FromHours(13)),
				("4d", TimeSpan.FromDays(4)),
				("20d", TimeSpan.FromDays(20)),
				("4w", TimeSpan.FromDays(4 * 7)),
				("5w", TimeSpan.FromDays(5 * 7)),
				("9n", TimeSpan.FromDays(9 * 28)),
				("13n", TimeSpan.FromDays(13 * 28)),
			};

			CombineAssertions(() =>
			{
				testData.ForEach(x =>
				{
					// Act
					var result =
						ServiceTaskScheduleHelper.GetPeriodDuration(x.Item1, TimeSpan.MaxValue,
							isRandomPeriod: false);

					// Assert
					AssertEquals(x.Item2, result);
				});
			});
		}

		public void TestRandomTimeSpan()
		{
			// Arrange
			var testData = new List<(string, TimeSpan)>()
			{
				("1s", TimeSpan.FromSeconds(1)),
				("10s", TimeSpan.FromSeconds(10)),
				("2m", TimeSpan.FromMinutes(2)),
				("15m", TimeSpan.FromMinutes(15)),
				("3h", TimeSpan.FromHours(3)),
				("13h", TimeSpan.FromHours(13)),
				("4d", TimeSpan.FromDays(4)),
				("20d", TimeSpan.FromDays(20)),
				("4w", TimeSpan.FromDays(4 * 7)),
				("5w", TimeSpan.FromDays(5 * 7)),
				("9n", TimeSpan.FromDays(9 * 28)),
				("13n", TimeSpan.FromDays(13 * 28)),
			};

			CombineAssertions(() =>
			{
				testData.ForEach(x =>
				{
					// Act
					var result =
						ServiceTaskScheduleHelper.GetPeriodDuration(x.Item1, TimeSpan.MaxValue,
							isRandomPeriod: true);

					// Assert
					AssertGreaterThanOrEqualTo(result, TimeSpan.Zero);
					AssertLessThanOrEqualTo(result, x.Item2);
				});
			});
		}
	}

	class ParseFrequencyTest : TestCase
	{
		public void TestInvalidString()
		{
			// Arrange
			var testString = new List<string>()
			{
				"",
				null,
				"abc",
				"Seconds",
				"Y",
				"20SS",
			};

			CombineAssertions(() =>
			{
				testString.ForEach(testValue =>
				{
					// Act
					var result = ServiceTaskScheduleHelper.ParseFrequency(testValue, out var period, out var type);

					// Assert
					Assert($"{testValue} should not be parsed", !result);
				});
			});
		}

		public void TestSeconds()
		{
			// Arrange
			var testSuffix = new List<string>()
			{
				"s",
				" s",
				"S",
				"second",
				"seconds"
			};

			// Act
			// Assert
			TestParse(testSuffix, "S");
		}

		public void TestMinutes()
		{
			// Arrange
			var testSuffix = new List<string>()
			{
				"m",
				" m",
				"M",
				"minute",
				"minutes"
			};

			// Act
			// Assert
			TestParse(testSuffix, "T");
		}

		public void TestHours()
		{
			// Arrange
			var testSuffix = new List<string>()
			{
				"h",
				" h",
				"H",
				"hour",
				"hours"
			};

			// Act
			// Assert
			TestParse(testSuffix, "H");
		}

		public void TestDays()
		{
			// Arrange
			var testSuffix = new List<string>()
			{
				"d",
				" d",
				"D",
				"Day",
				"Days"
			};

			// Act
			// Assert
			TestParse(testSuffix, "D");
		}

		public void TestWeeks()
		{
			// Arrange
			var testSuffix = new List<string>()
			{
				"w",
				" w",
				"W",
				"week",
				"weeks"
			};

			// Act
			// Assert
			TestParse(testSuffix, "W");
		}

		public void TestMonths()
		{
			// Arrange
			var testSuffix = new List<string>()
			{
				"n",
				" n",
				"N",
				"month",
				"months"
			};

			// Act
			// Assert
			TestParse(testSuffix, "M");
		}

		public void TestParse(List<string> suffixList, string expectedType)
		{
			// Arrange
			var testData = new List<(string, int)>()
			{
				("1", 1),
				("40", 40),
				("200", 200),
				("300", 300),
				("6000", 6000),
			};

			CombineAssertions(() =>
			{
				suffixList.ForEach(suffix =>
				{
					testData.ForEach(testValuePair =>
					{
						// Act
						var testString = $"{testValuePair.Item1}{suffix}";
						var result = ServiceTaskScheduleHelper.ParseFrequency(testString, out var period, out var type);

						// Assert
						Assert($"{testString} should be parsed", result);
						AssertEquals($"value [{period}] should not be 0", testValuePair.Item2, period);
						AssertEquals($"recurrence [{type}] should not null", expectedType, type);
					});
				});
			});
		}
	}
}
