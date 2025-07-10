using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsBillLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestLookups()
	{
		AssertType<NctsBillLookups>("Lookups Type", nctsBill.Lookups);
	}

	public void TestStatusList()
	{
		var lookups = new NctsBillLookups(nctsBill);
		var statusList = lookups.StatusList;

		CombineAssertions(() =>
		{
			AssertEquals("StatusList Count", 2, statusList.Count);
			AssertEquals("StatusList CodesAsString", "DEL, DLR", statusList.CodesAsString);
			AssertEquals("DEL Description", "Deleted", statusList.GetDescriptionFromCode(NctsDeletionStatusList.Codes.Deleted));
			AssertEquals("DLR Description", "Deletion Request", statusList.GetDescriptionFromCode(NctsDeletionStatusList.Codes.DeletionRequest));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsBill = header.Bills.AddNew();
	}

	NctsBill nctsBill;
}
