using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(GoodsItemDifferencesDetailsCollection))]
sealed class GoodsItemDifferencesDetailsCollectionTest : SingleCusCodeDataCollectionTest<GoodsItemDifferencesDetails>
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var arrivalGoodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
		return arrivalGoodsItem.GoodsItemDifferencesDetails;
	}
}
