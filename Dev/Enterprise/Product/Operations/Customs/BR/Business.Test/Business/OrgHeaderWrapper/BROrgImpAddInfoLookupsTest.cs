using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BROrgImpAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBrokerList()
		{
			var lookup = new BROrgImpAddInfoLookups(new BROrgImpAddInfo(Factory));
			AssertType<GlbStaffCollection>(lookup.StaffList);
		}
	}
}
