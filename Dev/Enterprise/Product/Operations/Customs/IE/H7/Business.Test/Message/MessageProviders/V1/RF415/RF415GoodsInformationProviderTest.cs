using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class RF415GoodsInformationProviderTest : DataProviderTestCase<RF415GoodsInformationProvider>
	{
		public void TestCommodityCode()
		{
			PackedItem.API_Tariff = "123456";
			AssertEquals("123456", Provider.CommodityCode);
		}

		public void TestGoodsDescription()
		{
			PackedItem.API_GoodsDescription = "GoodsDescription";
			AssertEquals("GoodsDescription", Provider.GoodsDescription);
		}

		public void TestGoodsQuantity()
		{
			AssertNull(Provider.GoodsQuantity);
		}

		public void TestCustomsValue()
		{
			PackedItem.API_RX_NKGoodsValueCurrency = "AUD";
			PackedItem.API_GoodsValue = 100.222m;

			AssertEquals("AUD", Provider.CustomsValue.Currency);
			AssertEquals(100.22m, Provider.CustomsValue.Amount);
		}

		public void TestTypeOfDuty()
		{
			AssertEquals(Array.Empty<ITypeOfDuty>(), Provider.TypeOfDuty);
		}

		protected override RF415GoodsInformationProvider GetProvider() => new RF415GoodsInformationProvider(PackedItem);

		AsycudaPackedItem PackedItem => packedItem ?? (packedItem = Factory.New<AsycudaManifestHeader>()
																	.Bills.AddNew()
																	.PackedItems.AddNew());
		AsycudaPackedItem packedItem;
	}
}
