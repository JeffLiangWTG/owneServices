using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class ChargeAmountRounderFiftyCentsDownMinimumOneTest : TestCaseWithFactory
	{
		public void TestRound()
		{
			var rounder = new ChargeAmountRounderFiftyCentsDownMinimumOne();
			CombineAssertions(() =>
			{
				AssertEquals(0m, rounder.Round(0));
				AssertEquals(1m, rounder.Round(0.01));
				AssertEquals(12m, rounder.Round(12.49));
				AssertEquals(12m, rounder.Round(12.50));
				AssertEquals(13m, rounder.Round(12.51));
				AssertEquals(13m, rounder.Round(13.49));
				AssertEquals(13m, rounder.Round(13.50));
				AssertEquals(14m, rounder.Round(13.51));
			});
		}
	}
}
