using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.ClusterKey.Testing
{
	sealed class DummyClusterKeyGrandChildBizoTest : TestCaseWithFactory
	{
		public void TestSetClusterKey()
		{
			var dummyParent = Factory.New<DummyClusterKeyParentBizo>();
			var dummyChild = dummyParent.AddNewDependentBizo();
			var dummyGrandChild = dummyChild.AddNewDependentBizo();

			Factory.Save();

			CombineAssertions("Attached dummyGrandChild", () =>
			{
				AssertEquals("GrandChild ClusterKey", 1, dummyGrandChild.ClusterKeyPty.Value);
				AssertEquals("Child ClusterKey", 1, dummyChild.ClusterKeyPty.Value);
				AssertEquals("Parent ClusterKey", 1, dummyParent.ClusterKeyPty.Value);
			});

			dummyGrandChild.FkToParentPty.Value = ZGuid.Empty;
			Factory.Save();
			AssertEquals("[Detached dummyGrandChild] ClusterKey", 0, dummyGrandChild.ClusterKeyPty.Value);

			var anotherDummyChild = Factory.New<DummyClusterKeyChildBizo>();
			dummyGrandChild.FkToParentPty.Value = anotherDummyChild.PK;
			Factory.Save();

			CombineAssertions("After re-attaching", () =>
			{
				AssertEquals("GrandChild ClusterKey", 2, dummyGrandChild.ClusterKeyPty.Value);
				AssertEquals("Child ClusterKey (new master)", 2, anotherDummyChild.ClusterKeyPty.Value);

				AssertEquals("Old Parent ClusterKey (no longer an ancestor of dummyGrandChild)", 1, dummyParent.ClusterKeyPty.Value);
				AssertEquals("Old Child ClusterKey (no longer an ancestor of dummyGrandChild)", 1, dummyChild.ClusterKeyPty.Value);
			});
		}

		public void TestClusterKeyChildren()
		{
			AssertNull("ClusterKeyChildList", Factory.New<DummyClusterKeyGrandChildBizo>().ClusterKeyChildList);
		}
	}
}
