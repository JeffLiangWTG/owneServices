using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CustomsValueCalculatorTest : TestCase
	{
		public void TestRoundUsingCustomsValueRule()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Amount % 1 < 0.5m", 1m, CustomsValueCalculator.RoundForMinus(1.49999m));
				AssertEquals("Amount % 1 == 0.5m", 1m, CustomsValueCalculator.RoundForMinus(1.50000m));
				AssertEquals("Amount % 1 > 0.5m", 2m, CustomsValueCalculator.RoundForMinus(1.50004m));
			});
		}
	}
}
