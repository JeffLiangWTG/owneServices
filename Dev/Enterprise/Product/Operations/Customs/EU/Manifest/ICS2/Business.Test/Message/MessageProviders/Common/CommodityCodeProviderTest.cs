using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public class CommodityCodeProviderTest : DataProviderTestCase<CommodityCodeProvider>
	{
		public void TestNew()
		{
			AssertNull(GenerateProvider(string.Empty));
			AssertNotNull(GenerateProvider(pack.PackedItem.API_Tariff));
		}

		public void TestHarmonizedSystemSubHeadingCode()
		{
			AssertEquals("When API_Tariff has 8 digits", "123456", Provider.HarmonizedSystemSubHeadingCode);

			pack.PackedItem.API_Tariff = "123456";

			AssertEquals("When API_Tariff has 6 digits", "123456", GetProvider().HarmonizedSystemSubHeadingCode);

			pack.PackedItem.API_Tariff = "1234";
			AssertEquals("When API_Tariff has 4 digits", "1234", GetProvider().HarmonizedSystemSubHeadingCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			AssertEquals("When API_Tariff has 8 digits", "78", Provider.CombinedNomenclatureCode);

			pack.PackedItem.API_Tariff = "123456";
			AssertEquals("When API_Tariff has 6 digits", string.Empty, GetProvider().CombinedNomenclatureCode);

			pack.PackedItem.API_Tariff = "1234";
			AssertEquals("When API_Tariff has 4 digits", string.Empty, GetProvider().CombinedNomenclatureCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			pack = bill.Packs.AddNew();
			pack.PackedItem.API_Tariff = "12345678";
		}
		AsycudaManifestHeader header;
		AsycudaPack pack;

		CommodityCodeProvider GenerateProvider(string tariff) => CommodityCodeProvider.NewOrNull(tariff);

		protected sealed override CommodityCodeProvider GetProvider()
		{
			return GenerateProvider(pack.PackedItem.API_Tariff);
		}
	}
}
