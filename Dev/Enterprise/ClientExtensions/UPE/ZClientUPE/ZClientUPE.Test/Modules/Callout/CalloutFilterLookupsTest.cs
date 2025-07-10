using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class CalloutFilterLookupsTest : TestCaseWithFactory
	{
		public void TestQueueTypes_List()
		{
			CalloutFilterLookups lookups = new CalloutFilterLookups(new CalloutFilterBusinessObject());
			AssertEquals("Should return the correct type of list", typeof(CommercialQueueCodeDescriptionPairList), lookups.QueueNames_List.GetType());
		}
	}
}
