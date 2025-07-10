using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class Commodity09ProviderTest : DataProviderTestCase<Commodity09Provider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("AsycudaPackedItem missing", () => GenerateProvider(null));
				AssertNoExceptionThrown(() => GenerateProvider(packedItem));
			});
		}

		public void TestDescriptionOfGoods()
		{
			packedItem.API_GoodsDescription = "GoodsDescription";
			AssertEquals("DescriptionOfGoods", "GoodsDescription", Provider.DescriptionOfGoods);
		}

		public void TestCusCode()
		{
			packedItem.API_ChemicalSubstanceCode = "CS";
			AssertEquals("CusCode", "CS", Provider.CusCode);
		}

		public void TestCommodityCode()
		{
			Assert("Should be ICommodityCode02", Provider.CommodityCode is ICommodityCode02);
		}

		protected override void SetUp()
		{
			base.SetUp();

			packedItem = Factory.New<AsycudaPackedItem>();
		}
		AsycudaPackedItem packedItem;

		Commodity09Provider GenerateProvider(AsycudaPackedItem packedItem) => new Commodity09Provider(packedItem);

		protected sealed override Commodity09Provider GetProvider()
		{
			return GenerateProvider(packedItem);
		}
	}
}
