using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Module.Testing
{
	public class LPCOFilterLookupsTest : TestCaseWithFactory
	{
		public void TestMessageStatusList()
		{
			var lookups = new LPCOFilterLookups(new LPCOFilterStripBusinessObject());
			AssertEquals("ACC, AWA, FAL, NOT, REJ", lookups.MessageStatusList().CodesAsString);
		}
	}
}
