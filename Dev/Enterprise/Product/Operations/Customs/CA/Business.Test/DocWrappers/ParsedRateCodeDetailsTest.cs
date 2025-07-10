using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ParsedRateCodeDetailsTest : TestCaseWithFactory
	{
		public void TestParsedRateCodeDetailsWithNone()
		{
			var rateInfo = new ParsedRateCodeDetails("", 0m);
			AssertEquals("Amount", 0m, rateInfo.Amount);
			AssertEquals("RateType", ZString.Empty, rateInfo.RateType);
			AssertEquals("Rate", 0m, rateInfo.Rate);
			AssertEquals("ExecemptionCode", ZString.Empty, rateInfo.ExecemptionCode);
		}

		public void TestParsedRateCodeDetailsWithGarbage()
		{
			var rateInfo = new ParsedRateCodeDetails("GARBAGE", 1.23m);
			AssertEquals("Amount", 1.23m, rateInfo.Amount);
			AssertEquals("RateType", "X", rateInfo.RateType);
			AssertEquals("Rate", 0m, rateInfo.Rate);
			AssertEquals("ExecemptionCode", ZString.Empty, rateInfo.ExecemptionCode);
		}

		public void TestParsedRateCodeDetailsWithZero()
		{
			var rateInfo = new ParsedRateCodeDetails("0.00", 1.23m);
			AssertEquals("Amount", 1.23m, rateInfo.Amount);
			AssertEquals("RateType", "X", rateInfo.RateType);
			AssertEquals("Rate", 0m, rateInfo.Rate);
			AssertEquals("ExecemptionCode", ZString.Empty, rateInfo.ExecemptionCode);
		}

		public void TestParsedRateCodeDetailsWithZeroAndZero()
		{
			var rateInfo = new ParsedRateCodeDetails("0.00", 0m);
			AssertEquals("Amount", 0m, rateInfo.Amount);
			AssertEquals("RateType", "F", rateInfo.RateType);
			AssertEquals("Rate", 0m, rateInfo.Rate);
			AssertEquals("ExecemptionCode", ZString.Empty, rateInfo.ExecemptionCode);
		}

		public void TestParsedRateCodeDetailsWithExemptionCode()
		{
			var rateInfo = new ParsedRateCodeDetails("89", 0m);
			AssertEquals("Amount", 0m, rateInfo.Amount);
			AssertEquals("RateType", "E", rateInfo.RateType);
			AssertEquals("Rate", 0m, rateInfo.Rate);
			AssertEquals("ExecemptionCode", "89", rateInfo.ExecemptionCode);
		}

		public void TestParsedRateCodeDetailsWithSpecificRate()
		{
			var rateInfo = new ParsedRateCodeDetails("5.00", 1.23m);
			AssertEquals("Amount", 1.23m, rateInfo.Amount);
			AssertEquals("RateType", "S", rateInfo.RateType);
			AssertEquals("Rate", 5m, rateInfo.Rate);
			AssertEquals("ExecemptionCode", ZString.Empty, rateInfo.ExecemptionCode);
		}

		public void TestParsedRateCodeDetailsWithPercentageRate()
		{
			var rateInfo = new ParsedRateCodeDetails("5.0", 1.23m);
			AssertEquals("Amount", 1.23m, rateInfo.Amount);
			AssertEquals("RateType", "V", rateInfo.RateType);
			AssertEquals("Rate", 5m, rateInfo.Rate);
			AssertEquals("ExecemptionCode", ZString.Empty, rateInfo.ExecemptionCode);

			rateInfo = new ParsedRateCodeDetails("5.", 1.23m);
			AssertEquals("Amount", 1.23m, rateInfo.Amount);
			AssertEquals("RateType", "V", rateInfo.RateType);
			AssertEquals("Rate", 5m, rateInfo.Rate);
			AssertEquals("ExecemptionCode", ZString.Empty, rateInfo.ExecemptionCode);
		}
	}
}
