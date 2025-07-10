using System.Linq;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class SupernumeraryGoodsDataProviderTest : BaseArrivalDataProviderTest<SupernumeraryGoodsDataProvider, NctsHeaderArrivalMessageSendingObject>
{
	protected override SupernumeraryGoodsDataProvider CreateDataProvider() => SupernumeraryGoodsDataProvider.NewCollection(NctsHeader.ArrivalMovementHeader.SupernumeraryGoods).First();

	public void TestNewCollection() => CombineAssertions(() =>
	{
		AssertNull("null argument", SupernumeraryGoodsDataProvider.NewCollection(null));

		NctsHeader.ArrivalMovementHeader.SupernumeraryGoods.AddNew();
		NctsHeader.ArrivalMovementHeader.SupernumeraryGoods.AddNew();

		var collection = SupernumeraryGoodsDataProvider.NewCollection(NctsHeader.ArrivalMovementHeader.SupernumeraryGoods).ToArray();
		AssertEquals("Count", 2, collection.Length);
		AssertEquals("item 1", 1, collection[0].SequenceNumber);
		AssertEquals("item 2", 2, collection[1].SequenceNumber);
	});

	public void TestProperties() => CombineAssertions(() =>
	{
		SupernumeraryGoodsItem.CSI_Description = "description";
		SupernumeraryGoodsItem.CSI_Quantity = 12.5;
		SupernumeraryGoodsItem.CSI_PackQty = 20;
		SupernumeraryGoodsItem.CSI_PackType = "PK";
		SupernumeraryGoodsItem.CSI_Tariff = "123456";

		AssertEquals("SequenceNumber", 1, DataProvider.SequenceNumber);
		AssertEquals("DescriptionOfGoods", "description", DataProvider.DescriptionOfGoods);
		AssertEquals("GrossMass", 12.5m, DataProvider.GrossMass);
		AssertEquals("NumberOfPackages", 20, DataProvider.NumberOfPackages);
		AssertEquals("TypeOfPackages", "PK", DataProvider.TypeOfPackages);
		AssertEquals("HarmonizedSystemSubHeadingCode", "123456", DataProvider.HarmonizedSystemSubHeadingCode);
	});
}
