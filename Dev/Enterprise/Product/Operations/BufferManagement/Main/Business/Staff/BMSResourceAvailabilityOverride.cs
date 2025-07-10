using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMSResourceAvailabilityOverride : GlbStaffHoliday
	{
		public BMSResourceAvailabilityOverride(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override GlbStaffHolidayLookups GetNewLookups()
		{
			return new BMSResourceAvailabilityOverrideLookups(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.GA_RecordType = BMConstants.BMSLeaveType;
			this.GA_WorkHolidayType = BMConstants.BMSLeaveType;
			this.GA_ParentTableCode = BMComponentSchema.Constants.Prefix;
			this.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			this.GA_DaysLeaveTaken = 1;
		}
	}
}
