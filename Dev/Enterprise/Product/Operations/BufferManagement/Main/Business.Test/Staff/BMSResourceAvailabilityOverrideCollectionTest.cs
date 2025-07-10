using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMSResourceAvailabilityOverrideCollection))]
	class BMSResourceAvailabilityOverrideCollectionTest : ActiveBusinessObjectCollectionTestCase<BMSResourceAvailabilityOverrideCollection>
	{
		protected override BMSResourceAvailabilityOverrideCollection GetCollectionToTest()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "HEH", "Long John Silver Rod Apple Cake");
			return new BMSResourceAvailabilityOverrideCollection(staff, new ZQuery());
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(BMSResourceAvailabilityOverrideCollection);
		}
	}
}
