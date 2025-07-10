using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public class PostalChargesProviderTest : DataProviderTestCase<PostalChargesProvider>
	{
		public void TestNew()
		{
			AssertNull(PostalChargesProvider.NewOrNull(null));
			AssertNotNull(PostalChargesProvider.NewOrNull(packedItem));
		}

		public void TestValue()
		{
			AssertEquals("Value", 123m, Provider.Value);
		}

		public void TestCurrency()
		{
			AssertEquals("Currency", "EUR", Provider.Currency);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			pack = bill.Packs.AddNew();
			packedItem = pack.PackedItem;
			packedItem.API_GoodsValue = 123m;
			packedItem.API_RX_NKGoodsValueCurrency = "EUR";
		}

		AsycudaManifestHeader header;
		AsycudaPack pack;
		AsycudaPackedItem packedItem;

		protected override PostalChargesProvider GetProvider()
		{
			return PostalChargesProvider.NewOrNull(packedItem);
		}
	}
}
