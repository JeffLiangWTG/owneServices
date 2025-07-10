using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.ClusterKey.Testing
{
	sealed class DummyClusterKeyChildBizoTest : TestCaseWithFactory
	{
		public void TestSetClusterKey()
		{
			var dummyParent = Factory.New<DummyClusterKeyParentBizo>();
			var dummyChild = dummyParent.AddNewDependentBizo();
			var dummyGrandChild = dummyChild.AddNewDependentBizo();

			Factory.Save();

			CombineAssertions("Attached dummyChild", () =>
			{
				AssertEquals("Child ClusterKey", 1, dummyChild.ClusterKeyPty.Value);
				AssertEquals("GrandChild ClusterKey", 1, dummyGrandChild.ClusterKeyPty.Value);
				AssertEquals("Parent ClusterKey", 1, dummyParent.ClusterKeyPty.Value);
			});

			dummyChild.FkToParentPty.Value = ZGuid.Empty;
			Factory.Save();

			CombineAssertions("After detaching (becomes a master)", () =>
			{
				AssertEquals("Child ClusterKey", 2, dummyChild.ClusterKeyPty.Value);
				AssertEquals("GrandChild ClusterKey", 2, dummyGrandChild.ClusterKeyPty.Value);

				AssertEquals("Old Parent ClusterKey (no longer dummyChild's parent)", 1, dummyParent.ClusterKeyPty.Value);
			});

			var anotherDummyParent = Factory.New<DummyClusterKeyParentBizo>();
			dummyChild.FkToParentPty.Value = anotherDummyParent.PK;
			Factory.Save();

			CombineAssertions("After re-attaching", () =>
			{
				AssertEquals("Child ClusterKey", 3, dummyChild.ClusterKeyPty.Value);
				AssertEquals("GrandChild ClusterKey", 3, dummyGrandChild.ClusterKeyPty.Value);
				AssertEquals("New Parent ClusterKey", 3, anotherDummyParent.ClusterKeyPty.Value);

				AssertEquals("Old Parent ClusterKey (no longer dummyChild's parent)", 1, dummyParent.ClusterKeyPty.Value);
			});
		}

		public void TestClusterKeyChildren()
		{
			var dummyChild = Factory.New<DummyClusterKeyChildBizo>();

			var clusterKeyChildList = dummyChild.ClusterKeyChildList;
			AssertEquals("ClusterKeyChildList count", 1, clusterKeyChildList.Count());

			var clusterKeyChildInfo = clusterKeyChildList.Single();

			CombineAssertions("Child Cluster Key Info", () =>
			{
				AssertEquals("BizObjType", typeof(DummyClusterKeyGrandChildBizo), clusterKeyChildInfo.BizObjType);
				AssertEquals("FkColumn", DummyPivotSchema.ZDP_ZD1, clusterKeyChildInfo.FkColumn);
			});

			AssertEquals("LoadChildObjects count with no children", 0, clusterKeyChildInfo.LoadChildObjects(dummyChild).Count());

			var dummyGrandChild1 = dummyChild.AddNewDependentBizo();

			CombineAssertions("LoadChildObjects (after adding one child)", () =>
			{
				var childObjects = clusterKeyChildInfo.LoadChildObjects(dummyChild);
				AssertEquals("Count", 1, childObjects.Count());
				AssertEquals("Contains dummyGrandChild1?", true, childObjects.Contains(dummyGrandChild1));
			});

			var dummyGrandChild2 = dummyChild.AddNewDependentBizo();

			CombineAssertions("LoadChildObjects (after adding another child)", () =>
			{
				var childObjects = clusterKeyChildInfo.LoadChildObjects(dummyChild);
				AssertEquals("Count", 2, childObjects.Count());
				AssertEquals("Contains dummyGrandChild1?", true, childObjects.Contains(dummyGrandChild1));
				AssertEquals("Contains dummyGrandChild2?", true, childObjects.Contains(dummyGrandChild2));
			});
		}
	}
}
