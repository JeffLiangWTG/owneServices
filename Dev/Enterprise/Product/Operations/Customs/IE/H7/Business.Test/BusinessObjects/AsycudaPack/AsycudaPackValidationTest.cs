using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	class AsycudaPackValidationTest : TestCaseWithFactory
	{
		public void TestCheckAPA_GoodsDescription()
		{
			var pack = Factory.New<AsycudaPack>();
			pack.APA_GoodsDescription = "ABC-123";

			AssertNoMessageErrors(pack.APA_GoodsDescriptionInfo);
		}
	}
}
