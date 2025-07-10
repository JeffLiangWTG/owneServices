using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class UPEAirCargoFilterLookupsTest : TestCaseWithFactory
	{
		public void TestQueueTypes_List()
		{
			UPEAirCargoFilterLookups lookups = new UPEAirCargoFilterLookups(new UPEAirCargoFilterBusinessObject());
			AssertEquals("Should return the correct type of list", typeof(CargoReportQueueCodeDescriptionPairList), lookups.QueueNames_List.GetType());
		}
	}
}
