using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescCloneTest : TestCaseWithFactory
{
	public void TestCloneRemarks()
	{
		goodsItem.Remarks = "GOODS REMARKS";
		var clonedGoodsItem = (NctsDepartureCargoDesc)goodsItem.Clone();
		AssertEquals("Remarks", "GOODS REMARKS", clonedGoodsItem.Remarks);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.NewDepartureNctsHeader();
		goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
	}

	NctsDepartureCargoDesc goodsItem;
}
