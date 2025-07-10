using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class RF415GoodsInformationProviderTest : DataProviderTestCase<RF415GoodsInformationProvider>
	{
		public void TestCommodityCode()
		{
			packedItem.API_Tariff = "123456";

			CombineAssertions(() =>
			{
				AssertType<CommodityCodeProvider>(Provider.CommodityCode);
				AssertEquals("CombinedNomenclatureCode", "123456", Provider.CommodityCode.CombinedNomenclatureCode);
				AssertEquals("TaricCode", "56", Provider.CommodityCode.TaricCode);
				Assert("NationalAdditionalCode should be IReadOnlyCollection<string>", Provider.CommodityCode.NationalAdditionalCode is IReadOnlyCollection<string>);
			});
		}

		public void TestGoodsDescription()
		{
			packedItem.API_GoodsDescription = "Test Description";
			AssertEquals("GoodsDescription", "Test Description", Provider.GoodsDescription);
		}

		public void TestGoodsQuantity()
		{
			AssertEquals("GoodsQuantity", null, Provider.GoodsQuantity);
		}

		public void TestCustomsValue()
		{
			packedItem.API_GoodsValue = 1000;
			packedItem.API_RX_NKGoodsValueCurrency = "AUD";
			CombineAssertions(() =>
			{
				AssertType<CustomsValueProvider>(Provider.CustomsValue);
				AssertEquals("Amount", 1000m, Provider.CustomsValue.Amount);
				AssertEquals("Currency", "AUD", Provider.CustomsValue.Currency);
			});
		}

		public void TestTypeOfDuty()
		{
			CombineAssertions(() =>
			{
				AssertEquals("TypeofDuty should have length 0", 0, Provider.TypeOfDuty.Count);
				Assert("TypeOfDuty should be IReadOnlyCollection<ITypeOfDuty>", Provider.TypeOfDuty is IReadOnlyCollection<ITypeOfDuty>);
			});
		}

		protected sealed override RF415GoodsInformationProvider GetProvider()
		{
			return new RF415GoodsInformationProvider(packedItem);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			pack = bill.Packs.AddNew();
			packedItem = bill.PackedItems.AddNew();
			packedItem.AsycudaPackPackedItemPivots.AddPivotFor(pack);
		}

		AsycudaBill bill;
		AsycudaPackedItem packedItem;
		AsycudaPack pack;
	}
}

