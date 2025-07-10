using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class WeightWrapperTest : Customs.Business.Testing.DataProviderTestCase<WeightWrapper>
	{
		public void TestGrossMass()
		{
			AssertEquals("Wrapper GrossMass should equal item API_GrossWeight.", 1200m, Provider.GrossMass);
		}

		protected override WeightWrapper GetProvider()
		{
			var item = Factory.New<AsycudaPackedItem>();
			item.API_GrossWeight = 1200m;
			return WeightWrapper.New(item);
		}
	}
}
