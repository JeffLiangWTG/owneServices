using System.Collections.Generic;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class CommodityCodeProviderTest : DataProviderTestCase<CommodityCodeProvider>
	{
		public void TestCombinedNomenclatureCode()
		{
			AssertEquals("CombinedNomenclatureCode", "123456", Provider.CombinedNomenclatureCode);
		}

		public void TestTaricCode()
		{
			AssertEquals("TaricCode", "56", Provider.TaricCode);
		}

		public void TestTaricAdditionalCode()
		{
			AssertNull(Provider.TaricAdditionalCode);
		}

		public void TestNationalAdditionalCommodityCode()
		{
			AssertNull(Provider.NationalAdditionalCommodityCode);
		}

		public void TestHarmonizedSystemSubheadingCode()
		{
			AssertNull(Provider.HarmonizedSystemSubheadingCode);
		}

		public void TestNationalAdditionalCode()
		{
			CombineAssertions("NationalAdditionalCode", () =>
			{
				AssertEquals("Should have nothing", 0, Provider.NationalAdditionalCode.Count);
				Assert("Should be IReadOnlyCollection<string>", Provider.NationalAdditionalCode is IReadOnlyCollection<string>);
			});
		}

		public void TestTypeGoods()
		{
			AssertNull(Provider.TypeGoods);
		}

		protected sealed override CommodityCodeProvider GetProvider()
		{
			return new CommodityCodeProvider(packedItem);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			packedItem = bill.PackedItems.AddNew();
			packedItem.API_Tariff = "12345678";
		}

		AsycudaBill bill;
		AsycudaPackedItem packedItem;
	}
}

