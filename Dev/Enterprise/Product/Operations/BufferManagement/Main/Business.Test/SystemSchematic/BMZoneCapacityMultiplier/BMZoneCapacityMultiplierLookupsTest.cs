using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business.Testing
{
	class BMZoneCapacityMultiplierLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReleaseGroupModuleID_WhenNoSystemIsSet()
		{
			var multiplier = Factory.New<BMZoneCapacityMultiplier>();
			AssertEquals(ModuleIDs.GlbGroup, ZMetaData.GetModuleId(multiplier.Lookups.SystemReleaseGroups));
		}

		public void TestReleaseGroups_ShouldNotDependOnBufferManagementSystem()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory);
			var system2 = BMSTestHelper.CreateSystem(Factory);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			var group4 = Factory.NewWithValidTestData<GlbGroup>();

			BMSTestHelper.CreateReleaseGroup(system1, group1);
			BMSTestHelper.CreateReleaseGroup(system1, group2);
			BMSTestHelper.CreateReleaseGroup(system2, group3);
			BMSTestHelper.CreateReleaseGroup(system2, group4);

			Factory.Save();

			var multiplier = Factory.New<BMZoneCapacityMultiplier>();

			var groups = multiplier.Lookups.ReleaseGroups;
			AssertCollectionContains(group1, groups);
			AssertCollectionContains(group2, groups);
			AssertCollectionContains(group3, groups);
			AssertCollectionContains(group4, groups);
		}
	}
}
