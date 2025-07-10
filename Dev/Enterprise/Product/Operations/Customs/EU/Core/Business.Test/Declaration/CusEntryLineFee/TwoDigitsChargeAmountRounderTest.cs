using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class FeeChargeAmountRounderTest : TestCaseWithFactory
	{
		public void TestRound()
		{
			var chargeAmountRounder = new TwoDigitsChargeAmountRounder();

			CombineAssertions("Assert Round() return value", () =>
			{
				AssertEquals("Round() case #1", 1.50m, chargeAmountRounder.Round(1.496));
				AssertEquals("Round() case #2", 1.50m, chargeAmountRounder.Round(1.504));
				AssertEquals("Round() case #3", 0m, chargeAmountRounder.Round(0.003));
				AssertEquals("Round() case #4", -1.50m, chargeAmountRounder.Round(-1.5043));
			});
		}
	}
}
