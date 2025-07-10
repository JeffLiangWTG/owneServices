using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

internal class NctsHeaderLookupsTest : TestCaseWithFactory
{
	public void TestCommunicationLanguageList()
	{
		AssertEquals("DE, FR, IT", lookups.CommunicationLanguageList.CodesAsString);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		lookups = nctsHeader.Lookups;
	}
	NctsHeader nctsHeader;
	NctsHeaderLookups lookups;
}
