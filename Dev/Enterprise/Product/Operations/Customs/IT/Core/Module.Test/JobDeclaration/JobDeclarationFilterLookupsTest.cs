using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Module.Testing;

sealed class JobDeclarationFilterLookupsTest : TestCaseWithFactory
{
	public void TestTransportTypeList()
	{
		var bizO = new JobDeclarationFilterBusinessObject();
		var moduleFilter = new JobDeclarationFilterLookups(bizO);
		AssertEquals("AIR, FIX, IWT, OWN, MAI, RAI, ROA, SEA", moduleFilter.TransportTypeList.CodesAsString);
	}

	public void TestContainerModeList()
	{
		var bizO = new JobDeclarationFilterBusinessObject();
		var moduleFilter = new JobDeclarationFilterLookups(bizO);
		AssertEquals("LCL, FCL, LSE, ULD, BBK, BLK, LQD, ROR, LTL, FTL, OBC, UNA, CNT, NCT", moduleFilter.ContainerModeList.CodesAsString);
	}

	public void TestMessageTypeList()
	{
		var bizO = new JobDeclarationFilterBusinessObject();
		var moduleFilter = new JobDeclarationFilterLookups(bizO);
		AssertEquals("MessageTypeList contains only core declaration types while excluding codes for Declaration Lock functionality", "EXP, IMP, MSC", moduleFilter.MessageTypeList.CodesAsString);
	}

	public void TestMessageVersionList()
	{
		var bizO = new JobDeclarationFilterBusinessObject();
		var moduleFilter = new JobDeclarationFilterLookups(bizO);
		AssertEquals("TXT, XML", moduleFilter.MessageVersionList.CodesAsString);
	}
}
