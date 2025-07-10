using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class MConsignment02ProviderTest : DataProviderTestCase<MConsignment02Provider>
	{
		public void TestTransportEquipments()
		{
			AssertEquals("Transport equipment should always be empty for H7 message", 0, Provider.TransportEquipments.Count);
		}

		public void TestLocationOfGoods()
		{
			AssertNotNull("Location Of Goods", Provider.LocationOfGoods);
			AssertEquals("ABC", Provider.LocationOfGoods.AdditionalIdentifier);
		}

		protected override MConsignment02Provider GetProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ManifestQty = 12;
			bill.CusGoodsLocation.AdditionalIdentifier = "ABC";

			return new MConsignment02Provider(bill);
		}
	}
}
