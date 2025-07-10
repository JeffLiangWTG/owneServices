using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class WeightRoundingTest : TestCase
	{
		public void TestRound_InTransitionPeriod()
		{
			var weightForTest = new ZDecimal(12345.123123123m);
			var actualResult = WeightRounding.Round(true, weightForTest);
			AssertEquals("In transition period: weight should be rounded to 3 decimal digits", 12345.123m, actualResult);
		}

		public void TestRound_NotInTransitionPeriod()
		{
			var weightForTest = new ZDecimal(12345.123123123m);
			var actualResult = WeightRounding.Round(false, weightForTest);
			AssertEquals("Not in transition period: weight should be rounded to 6 decimal digits", 12345.123123m, actualResult);
		}

		public void TestRoundToElevenAndThree()
		{
			var testCases = new (string description, ZDecimal weight, decimal? ecpected)[]
			{
				("No decimal places needed", 12345678m, 12345678m),
				("Decimal places needed", 12345678.123456m, 12345678.123m),

				("Last digit is not increased", 12345678.1234m, 12345678.123m),
				("Last digit is increased", 12345678.1235m, 12345678.124m),

				("No integer part and decimal part is less than 3", 0.01m, 0.01m),
				("No integer part and decimal part is 3", 0.012m, 0.012m),
				("No integer part and decimal part is more than 3", 0.0123m, 0.012m),

				("Integer part is less then 8 and decimal part is les than 3", 1234567.12m, 1234567.12m),
				("Integer part is less then 8 and decimal part is 3", 1234567.123m, 1234567.123m),
				("Integer part is less then 8 and decimal part is more than 3", 1234567.1234m, 1234567.123m),

				("Integer part is 8 and decimal part is les than 3", 1234567.12m, 1234567.12m),
				("Integer part is 8 and decimal part is 3", 1234567.123m, 1234567.123m),
				("Integer part is 8 and decimal part is more than 3", 1234567.1234m, 1234567.123m),

				("Integer part is 9 and decimal part is less then 2", 123456789.1m, 123456789.1m),
				("Integer part is 9 and decimal part is 2", 123456789.12m, 123456789.12m),
				("Integer part is 9 and decimal part is more then 2", 123456789.123m, 123456789.12m),

				("Integer part is 10 and no decimal part", 1234567890m, 1234567890m),
				("Integer part is 10 and decimal part is 1", 1234567890.1m, 1234567890.1m),
				("Integer part is 10 and decimal part is more then 1", 1234567890.12m, 1234567890.1m),

				("Integer part is 11 and no decimal part", 12345678901m, 12345678901m),
				("Integer part is 11 and decimal part is 1", 12345678901.1m, 12345678901m),

				("Integer part is more than 11 and no decimal part", 123456789012.123m, 123456789012m),
				("Integer part is more than 11 and there is decimal part", 123456789012.123m, 123456789012m),

				("Integer part is too long", 123456789012345.123m, 123456789012345m),
			};

			CombineAssertions(() =>
			{
				foreach (var (description, weight, ecpectedResult) in testCases)
				{
					var actualResult = WeightRounding.Round(true, weight);
					AssertEquals(description, ecpectedResult, actualResult);
				}
			});
		}
	}
}
