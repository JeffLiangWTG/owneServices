using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMComponentReleaseGroupLinkLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReleaseGroups_ShouldIncludeSystemGroupsWhenPresent()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			var group4 = Factory.NewWithValidTestData<GlbGroup>();

			BMSTestHelper.CreateReleaseGroup(system, group1);
			BMSTestHelper.CreateReleaseGroup(system, group2);

			var componentReleaseGroupLink = Factory.New<BMComponentReleaseGroupLink>();

			var groups = componentReleaseGroupLink.Lookups.ReleaseGroups;

			AssertCollectionContains(group1, groups);
			AssertCollectionContains(group2, groups);
			AssertCollectionContains(group3, groups);
			AssertCollectionContains(group4, groups);

			componentReleaseGroupLink.FO_FC_Component = bucket.PK;

			groups = componentReleaseGroupLink.Lookups.ReleaseGroups;

			AssertCollectionContains(group1, groups);
			AssertCollectionContains(group2, groups);
			AssertCollectionContains(group3, groups);
			AssertCollectionContains(group4, groups);
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

			var componentReleaseGroupLink = Factory.New<BMComponentReleaseGroupLink>();

			var groups = componentReleaseGroupLink.Lookups.ReleaseGroups;

			AssertCollectionContains(group1, groups);
			AssertCollectionContains(group2, groups);
			AssertCollectionContains(group3, groups);
			AssertCollectionContains(group4, groups);
		}
	}
}
