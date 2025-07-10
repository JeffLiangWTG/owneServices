using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class RateInfoCalculatorTest : TestCase
	{
		public void TestGetRateApplicable()
		{
			dummyRate.CustomsRateExposed = 10m;
			RateInfo[] result = calculator.GetRateApplicable(dummyRate);
			AssertEquals("Rates applicable", 1, result.Length);
			AssertEquals("CustomsValue Applicable", DutyRateField.CustomsValue, result[0].DutyRateField);
			AssertEquals("Rate Applicable", 10m, result[0].Rate);
			AssertEquals("Unit", "", result[0].Unit);

			dummyRate.FirstQtyRateExposed = 20m;
			dummyRate.FirstUQExpsoed = "AA";
			result = calculator.GetRateApplicable(dummyRate);
			AssertEquals("Rates applicable", 2, result.Length);
			AssertEquals("FirstUQ Applicable", DutyRateField.FirstQty, result[1].DutyRateField);
			AssertEquals("Rate Applicable", 20m, result[1].Rate);
			AssertEquals("Unit", "AA", result[1].Unit);

			dummyRate.SecondQtyRateExposed = 30m;
			dummyRate.SecondUQExposed = "BB";
			result = calculator.GetRateApplicable(dummyRate);
			AssertEquals("Rates applicable", 3, result.Length);
			AssertEquals("SecondUQ Applicable", DutyRateField.SecondQty, result[2].DutyRateField);
			AssertEquals("Rate Applicable", 30m, result[2].Rate);
			AssertEquals("Unit", "BB", result[2].Unit);

			dummyRate.OtherDutyFactorRateExposed = 40m;
			result = calculator.GetRateApplicable(dummyRate);
			AssertEquals("Rates applicable", 4, result.Length);
			AssertEquals("OtherDutyFactorRate Applicable", DutyRateField.OtherDutyFactor, result[3].DutyRateField);
			AssertEquals("Rate Applicable", 40m, result[3].Rate);
			AssertEquals("Unit", "", result[3].Unit);
		}

		RateInfoCalculator calculator;
		DummyFourRates dummyRate;

		protected override void SetUp()
		{
			base.SetUp();
			calculator = new RateInfoCalculator();
			dummyRate = new DummyFourRates();
		}
	}
}
