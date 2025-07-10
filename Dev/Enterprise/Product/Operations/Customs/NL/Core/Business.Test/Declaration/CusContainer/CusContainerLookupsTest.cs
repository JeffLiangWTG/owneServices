namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class CusContainerLookupsTest : EU.Business.Declaration.Testing.CusContainerLookupsTest
{
	public void TestSealPartyList()
	{
		CusContainer parent = Factory.New<CusContainer>();

		var sealPartyList = parent.Lookups.SealPartyList;
		sealPartyList.Sort();

		AssertEquals("CAR, CRD, CTO, CUS", sealPartyList.CodesAsString);
	}
}
