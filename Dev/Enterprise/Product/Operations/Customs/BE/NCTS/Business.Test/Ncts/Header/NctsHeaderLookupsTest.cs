using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class NctsHeaderLookupsTest : TestCaseWithFactory
{
	public void TestCommunicationLanguageTest()
	{
		AssertEquals("NL, FR, DE, EN", Factory.New<NctsHeader>().Lookups.CommunicationLanguageList.CodesAsString);
	}
}
