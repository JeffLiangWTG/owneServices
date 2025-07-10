using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class LocationOfGoodsGNSSProviderTest : DataProviderTestCase<LocationOfGoodsGNSSProvider>
	{
		public void TestGoodsLocation()
		{
			AssertType<GoodsLocationProvider>(Provider.GoodsLocation);
		}

		protected override LocationOfGoodsGNSSProvider GetProvider()
		{
			SetUpTestData();
			return new LocationOfGoodsGNSSProvider(bill.CusGoodsLocation);
		}

		void SetUpTestData()
		{
			if (bill == null)
			{
				bill = Factory.New<AsycudaBill>();
			}
		}
		AsycudaBill bill;
	}
}
