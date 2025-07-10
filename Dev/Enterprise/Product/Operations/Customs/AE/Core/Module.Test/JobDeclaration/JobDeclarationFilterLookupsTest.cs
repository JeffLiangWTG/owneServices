using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AE;

namespace Enterprise.Customs.AE.Module.Testing;

public class JobDeclarationFilterLookupsTest : TestCaseWithFactory
{
	public void TestMesageTypeList()
	{
		var filterLookups = new JobDeclarationFilterLookups(new JobDeclarationFilterStripBusinessObject());
		AssertEquals(typeof(AEJobMessageTypeList), filterLookups.MessageTypeList.GetType());
	}

	public void TestMessageSubTypeList()
	{
		var filterLookups = new JobDeclarationFilterLookups(new JobDeclarationFilterStripBusinessObject());
		AssertEquals("MessageSubTypeList Should Be Override to Empty List", 0, filterLookups.MessageSubTypeList().Count);
	}
}
