using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class GlbGroupExtensionsTest : TestCaseWithFactory
	{
		public void TestFindManager()
		{
			GlbGroup emptyGroup = Factory.NewWithValidTestData<GlbGroup>();
			AssertNull(emptyGroup.FindManager());

			GlbGroup groupNoManager = Factory.NewWithValidTestData<GlbGroup>();
			groupNoManager.Staff.Add(Factory.NewWithValidTestData<GlbStaff>());
			AssertNull(groupNoManager.FindManager());

			GlbGroup groupWithManager = Factory.NewWithValidTestData<GlbGroup>();
			groupWithManager.Staff.Add(Factory.NewWithValidTestData<GlbStaff>());
			GlbStaff manager = Factory.NewWithValidTestData<GlbStaff>();
			groupWithManager.Staff.Add(manager);
			manager.CurrentGroupLink.GK_MembershipType = "MGR";
			AssertEquals(groupWithManager.FindManager().PK, manager.PK);
		}
	}
}