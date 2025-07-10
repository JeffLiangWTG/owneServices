using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class TwoDigitCustomChargeAmountRounderTest : TestCaseWithFactory
{
	public void TestRoundChargeAmountUsingCustomAlgorithm()
	{
		var chargeAmountRounder = new TwoDigitCustomChargeAmountRounder();

		CombineAssertions("Assert many scenarios", () =>
		{
			AssertEquals("Round() case #1", 0.65m, chargeAmountRounder.Round(0.6542m));
			AssertEquals("Round() case #2", 0.66m, chargeAmountRounder.Round(0.6555));
			AssertEquals("Round() case #3", -0.66m, chargeAmountRounder.Round(-0.6555));
			AssertEquals("Round() case #4, When absolute value is greater than zero.", 0.01m, chargeAmountRounder.Round(0.004m));
			AssertEquals("Round() case #5. When absolute value is less than zero.", -0.01m, chargeAmountRounder.Round(-0.004));
		});
	}
}
