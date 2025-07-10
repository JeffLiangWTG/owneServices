using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMSResourceAvailabilityOverride))]
	class BMSResourceAvailabilityOverrideTest : GlbStaffHolidayTest
	{
		public void TestConstruction()
		{
			var holidayOverride = Factory.New<BMSResourceAvailabilityOverride>();

			AssertEquals(BMConstants.BMSLeaveType, holidayOverride.GA_RecordType);
			AssertEquals(BMConstants.BMSLeaveType, holidayOverride.GA_WorkHolidayType);
			AssertEquals(BMComponentSchema.Constants.Prefix, holidayOverride.GA_ParentTableCode);
		}
	}
}
