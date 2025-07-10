using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class NameOfGoodsElementStrategyTest : TestCaseWithFactory
	{
		public void TestMaxLength()
		{
			AssertEquals("MaxLength", 50, new NameOfGoodsElementStrategy().MaxLength);
		}
	}
}
