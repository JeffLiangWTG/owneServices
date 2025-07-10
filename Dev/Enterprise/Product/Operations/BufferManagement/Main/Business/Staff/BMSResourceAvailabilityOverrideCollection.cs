using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMSResourceAvailabilityOverrideCollection : ActiveBusinessObjectCollection<BMSResourceAvailabilityOverride>
	{
		public BMSResourceAvailabilityOverrideCollection(GlbStaff staff, ZQuery query)
			: base(staff.Factory, staff, query, GlbStaffHolidaySchema.GA_GS)
		{
		}
	}
}
