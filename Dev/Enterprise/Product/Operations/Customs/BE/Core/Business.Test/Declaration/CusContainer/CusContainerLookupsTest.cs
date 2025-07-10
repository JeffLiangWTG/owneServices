namespace Enterprise.Customs.BE.Business.Declaration.Testing;

class CusContainerLookupsTest : EU.Business.Declaration.Testing.CusContainerLookupsTest
{
	public void TestSealPartyList()
	{
		CusContainer parent = Factory.New<CusContainer>();

		var sealPartyList = parent.Lookups.SealParty_List;
		sealPartyList.Sort();

		AssertEquals("CAR, CRD, CTO, CUS", sealPartyList.CodesAsString);
	}
}
