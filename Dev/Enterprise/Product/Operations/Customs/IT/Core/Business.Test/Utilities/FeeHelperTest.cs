using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class FeeHelperTest : TestCaseWithFactory
{
	public void TestRoundChargeAmountIfNeeded()
	{
		CombineAssertions("Assert many scenarios", () =>
		{
			AssertEquals("Rounded Result", 0.65m, FeeHelper.RoundChargeAmountIfNeeded(0.6542m));
			AssertEquals("Rounded Result", 0.66m, FeeHelper.RoundChargeAmountIfNeeded(0.6555));
			AssertEquals("Rounded Result", -0.66m, FeeHelper.RoundChargeAmountIfNeeded(-0.6555));
			AssertEquals("When absolute value is greater than zero, Rounded Result", 0.01m, FeeHelper.RoundChargeAmountIfNeeded(0.004m));
			AssertEquals("When absolute value is less than zero, Rounded Result", -0.01m, FeeHelper.RoundChargeAmountIfNeeded(-0.004));
		});
	}
}
