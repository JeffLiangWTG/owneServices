using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMSystemReleaseGroupCollection))]
	class BMSystemReleaseGroupCollectionTest : ActiveBusinessObjectCollectionTestCase<BMSystemReleaseGroupCollection>
	{
		public void TestToGroupCollection()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();

			var releaseGroup1 = system.ReleaseGroups.AddNew();
			var releaseGroup2 = system.ReleaseGroups.AddNew();
			releaseGroup1.FSG_GG_Group = group1.PK;
			releaseGroup2.FSG_GG_Group = group2.PK;

			var groups = system.ReleaseGroups.ToGroupCollection();
			AssertContainsExactElementsInAnyOrder(groups, new[] { group1, group2 });
		}

		public void TestSystemReleaseGroups_ShouldNotCreateMultipleCollectionsForTheSameSystem()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var systemReleaseGroup = system.ReleaseGroups.AddNew();
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			systemReleaseGroup.FSG_GG_Group = releaseGroup.PK;

			var board1 = BMSTestHelper.CreateBoard(system);
			var board2 = BMSTestHelper.CreateBoard(system, "Other Board");

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var board1Loaded = newFactory.Load<BMBoard>(board1.PK);
			var board2Loaded = newFactory.Load<BMBoard>(board2.PK);

			var board1Groups = board1Loaded.Lookups.SystemReleaseGroups;
			var board2Groups = board2Loaded.Lookups.SystemReleaseGroups;

			AssertEquals("Both boards are associated with the same system, so both SystemReleaseGroups collections should be the same reference (rather than creating a new object for each board), and yet...", board1Groups, board2Groups);
		}

		public void TestSystemReleaseGroups_ShouldUpdateWhenNewGroupAdded()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var systemReleaseGroup = system.ReleaseGroups.AddNew();
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			systemReleaseGroup.FSG_GG_Group = releaseGroup.PK;

			AssertEquals(1, system.ReleaseGroupLookups.Count);

			systemReleaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			systemReleaseGroup.FSG_GG_Group = releaseGroup.PK;

			AssertEquals(2, system.ReleaseGroupLookups.Count);
		}

		protected override BMSystemReleaseGroupCollection GetCollectionToTest()
		{
			return new BMSystemReleaseGroupCollection(Factory.NewWithValidTestData<BMSystem>());
		}
	}
}
