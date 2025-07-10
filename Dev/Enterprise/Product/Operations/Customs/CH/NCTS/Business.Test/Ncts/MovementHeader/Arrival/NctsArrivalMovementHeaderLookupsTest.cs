using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class NctsArrivalMovementHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTransportAtArrivalTypeList() => AssertSame(NctsLookupsHelper.TransportTypeOfIdList(Factory), Lookups.TransportAtArrivalTypeList);

	public void TestNationalityList() => AssertSame(NctsLookupsHelper.NationalityList(Factory), Lookups.NationalityList);

	public void TestYesNoList()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Codes", "N, Y", Lookups.YesNoList.CodesAsString);
			AssertSame("Cached", Lookups.YesNoList, Lookups.YesNoList);
		});
	}

	public void TestNctsTransitStatusList()
	{
		LookupsTestHelper.AssertListCodesAndIsCached(() => Lookups.NctsTransitStatusList, new EU.NCTS.Business.CodeDescriptionPairLists.NCTS5ArrivalCustomsStatusList().CodesAsString + ", ACT, CLR");
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;
	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader;
	}

	NctsArrivalMovementHeaderLookups Lookups => NctsHeader.ArrivalMovementHeader.Lookups;
}
