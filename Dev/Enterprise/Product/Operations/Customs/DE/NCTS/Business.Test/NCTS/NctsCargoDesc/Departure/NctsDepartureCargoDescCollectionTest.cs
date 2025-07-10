using CargoWise.EntityFramework.Testing;

using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>))]
	sealed class NctsDepartureCargoDescCollectionTest : ActiveBusinessObjectCollectionTestCase<INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>
	{
		public void TestBondAmountIsUpdatedByAdditionalLine()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMovementHeader = header.MovementHeader;
			var guarantee1 = departureMovementHeader.Guarantees.AddNew();
			guarantee1.PW_Override = false;
			var guarantee2 = departureMovementHeader.Guarantees.AddNew();
			guarantee2.PW_Override = true;

			var bill = header.Bills.AddNew();
			var goodsItemOnBill = bill.GoodsItems.AddNew();
			SetDataGoodsItem(goodsItemOnBill);

			CombineAssertions(() =>
			{
				goodsItemOnBill.BY_LinePrice = 8000;
				AssertEquals("Override=false", 2000M, guarantee1.PW_BondAmount);
				AssertEquals("Override=true", 0M, guarantee2.PW_BondAmount);

				bill.GoodsItems.CopyLastGoodsItemToNewLines = true;
				var goodsItemOnBill3 = bill.GoodsItems.AddNew();
				AssertEquals("Copy goodsItem, Override=false", 4000M, guarantee1.PW_BondAmount);
				AssertEquals("Copy goodsItem, Override=true", 0M, guarantee2.PW_BondAmount);
			});

			void SetDataGoodsItem(NctsDepartureCargoDesc goodsItem)
			{
				goodsItem.BY_GrossWeight = 130;
				goodsItem.BY_NetWeight = 120;
				goodsItem.BY_HarmonisedTariff = "0304798001";
				goodsItem.BY_CustomsQuantity = 120;
				goodsItem.BY_LinePrice = 12000;
				goodsItem.BY_RX_NKLinePriceCurrency = "EUR";
				goodsItem.BY_ZZF_NKTaxType = "REG";
			}
		}
		protected override INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			return bill.GoodsItems;
		}
	}
}
