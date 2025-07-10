using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescPhase5LookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCPCList()
	{
		AssertEquals("CPCList Count", 0, lookups.CPCList.Count);
	}

	public void TestStatusList()
	{
		CombineAssertions(() =>
		{
			var statusList = lookups.StatusList;
			AssertEquals("StatusList Count", 2, statusList.Count);
			AssertEquals("StatusList CodesAsString", "DEL, DLR", statusList.CodesAsString);
			AssertEquals("DEL Description", "Deleted", statusList.GetDescriptionFromCode(NctsDeletionStatusList.Codes.Deleted));
			AssertEquals("DLR Description", "Deletion Request", statusList.GetDescriptionFromCode(NctsDeletionStatusList.Codes.DeletionRequest));
		});
	}

	public void TestPortTaxRateList()
	{
		AssertEquals("PortTaxRateList Count", 0, lookups.PortTaxRateList.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.NewDepartureNctsHeader();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
		lookups = new NctsDepartureCargoDescPhase5Lookups(goodsItem);
	}

	NctsDepartureCargoDescPhase5Lookups lookups;
}
