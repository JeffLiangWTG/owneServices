using System.Linq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class GlobalTrackingShipmentVisibilityServiceEhubIDListTest : TestCase
	{
		public void TestDefaultShipmentVisibilityServiceEhubIDList()
		{
			var serviceEhubIDList = new GlobalTrackingShipmentVisibilityServiceEhubIDList().GetDefaultGlobalTrackingShipmentVisibilityServiceEhubIDs();

			AssertEquals(2, serviceEhubIDList.Count);
			AssertContainsCode(serviceEhubIDList, "CA", "CONTAINER_TRACKING");
			AssertContainsCode(serviceEhubIDList, "AWBA", "FLIGHT_MONITORING_SYSTEM");
		}

		void AssertContainsCode(GlobalTrackingShipmentVisibilityServiceEhubIDCollection list, string service, string ehubID)
		{
			AssertEquals(true, list.Cast<GlobalTrackingShipmentVisibilityServiceEhubID>().Any(x => x.Service == service && x.EhubID == ehubID));
		}
	}
}
