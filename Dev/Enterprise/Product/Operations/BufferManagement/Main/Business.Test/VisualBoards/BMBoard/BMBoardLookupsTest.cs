using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMBoardLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReleaseGroups()
		{
			var system = Factory.New<BMSystem>();
			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "CCC";
			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "BBB";
			var group3 = Factory.New<GlbGroup>();
			group3.GG_Code = "AAA";

			var releaseGroup1 = system.ReleaseGroups.AddNew();
			releaseGroup1.FSG_GG_Group = group1.PK;
			var releaseGroup3 = system.ReleaseGroups.AddNew();
			releaseGroup3.FSG_GG_Group = group3.PK;

			var board = system.Boards.AddNew();
			var groups = board.Lookups.SystemReleaseGroups;
			AssertEquals(8, groups.Count);
			AssertCollectionContains(group1, groups);
			AssertCollectionContains(group2, groups);
			AssertCollectionContains(group3, groups);

			AssertEquals(0, groups.IndexOf(group1));
			AssertEquals(1, groups.IndexOf(group2));
			AssertEquals(2, groups.IndexOf(group3));
		}

		public void TestReleaseGroupModuleID_WhenNoSystemIsSet()
		{
			var board = Factory.New<BMBoard>();
			AssertEquals(ModuleIDs.GlbGroup, ZMetaData.GetModuleId(board.Lookups.SystemReleaseGroups));
		}
	}
}
