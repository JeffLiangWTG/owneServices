using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Global.Staff;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class EDIGlbTimeAllocationLookupsTest : GlbStaffHolidayLookupsTest
	{
		public void TestClientSpecificAdded()
		{
			EDIGlbTimeAllocationLookups lookups = EDIGlbTimeAllocationLookups.New(Factory.New<GlbStaffHoliday>());
			AssertEquals(2, lookups.Types.Count);
		}
	}
}
