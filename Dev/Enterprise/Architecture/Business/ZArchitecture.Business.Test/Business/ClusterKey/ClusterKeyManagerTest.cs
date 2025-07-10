using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Test.Business.ClusterKey.Test;

namespace Enterprise.ZArchitecture.Business.ClusterKey.Testing
{
	sealed class ClusterKeyManagerTest : TestCaseWithFactory
	{
		public void TestClusterKeySetting()
		{
			var dummyParent = Factory.New<DummyClusterKeyParentBizo>();
			var dummyChild1 = dummyParent.AddNewDependentBizo();
			var dummyGrandChild11 = dummyChild1.AddNewDependentBizo();

			CombineAssertions("[PRE-CONDITION] Initial Values", () =>
			{
				AssertEquals("Parent ClusterKey", 0, dummyParent.Z0_Number);
				AssertEquals("Child 1 ClusterKey", 0, dummyChild1.ZD1_Number);
				AssertEquals("Grand-Child 1.1 ClusterKey", 0, dummyGrandChild11.ZDP_Number);
			});

			Factory.Save();

			CombineAssertions("After Save", () =>
			{
				AssertEquals("Parent ClusterKey", 1, dummyParent.Z0_Number);
				AssertEquals("Child 1 ClusterKey", 1, dummyChild1.ZD1_Number);
				AssertEquals("Grand-Child 1.1 ClusterKey", 1, dummyGrandChild11.ZDP_Number);
			});

			var dummyGrandChild12 = dummyChild1.AddNewDependentBizo();
			var dummyChild2 = dummyParent.AddNewDependentBizo();
			var dummyGrandChild21 = dummyChild2.AddNewDependentBizo();

			Factory.Save();

			CombineAssertions("After Adding new dependent objects and saving", () =>
			{
				AssertEquals("Parent ClusterKey", 1, dummyParent.Z0_Number);
				AssertEquals("Child 1 ClusterKey", 1, dummyChild1.ZD1_Number);
				AssertEquals("Grand-Child 1.1 ClusterKey", 1, dummyGrandChild11.ZDP_Number);
				AssertEquals("Grand-Child 1.2 ClusterKey", 1, dummyGrandChild12.ZDP_Number);
				AssertEquals("Child 2 ClusterKey", 1, dummyChild2.ZD1_Number);
				AssertEquals("Grand-Child 2.1 ClusterKey", 1, dummyGrandChild21.ZDP_Number);
			});
		}

		public void TestClusterKeyCascadingToDescendants()
		{
			var dummyParent = Factory.New<DummyClusterKeyParentBizo>();
			var dummyChild = dummyParent.AddNewDependentBizo();
			var dummyGrandChild1 = dummyChild.AddNewDependentBizo();
			var dummyGrandChild2 = dummyChild.AddNewDependentBizo();
			Factory.Save();

			CombineAssertions("After 1st Save", () =>
			{
				AssertEquals("Parent ClusterKey", 1, dummyParent.Z0_Number);
				AssertEquals("Child ClusterKey", 1, dummyChild.ZD1_Number);
				AssertEquals("Grand-Child 1 ClusterKey", 1, dummyGrandChild1.ZDP_Number);
				AssertEquals("Grand-Child 2 ClusterKey", 1, dummyGrandChild2.ZDP_Number);
			});

			var anotherDummyBizObj = Factory.New<DummyClusterKeyParentBizo>();
			Factory.Save();

			CombineAssertions("After saving a new master", () =>
			{
				AssertEquals("Current Parent ClusterKey", 1, dummyParent.Z0_Number);
				AssertEquals("Child-less master ClusterKey", 2, anotherDummyBizObj.Z0_Number);
				AssertEquals("Child ClusterKey", 1, dummyChild.ZD1_Number);
				AssertEquals("Grand-Child 1 ClusterKey", 1, dummyGrandChild1.ZDP_Number);
				AssertEquals("Grand-Child 2 ClusterKey", 1, dummyGrandChild2.ZDP_Number);
			});

			dummyChild.ZD1_Z0 = anotherDummyBizObj.PK;
			Factory.Save();

			CombineAssertions("After linking Child to a new Parent", () =>
			{
				AssertEquals("Old Parent ClusterKey", 1, dummyParent.Z0_Number);
				AssertEquals("New Parent ClusterKey", 2, anotherDummyBizObj.Z0_Number);
				AssertEquals("Child ClusterKey", 2, dummyChild.ZD1_Number);
				AssertEquals("Grand-Child 1 ClusterKey", 2, dummyGrandChild1.ZDP_Number);
				AssertEquals("Grand-Child 2 ClusterKey", 2, dummyGrandChild2.ZDP_Number);
			});

			dummyChild.ZD1_Z0 = ZGuid.Empty;
			Factory.Save();

			CombineAssertions("After detaching Child from Parent (becoming a subtree master)", () =>
			{
				AssertEquals("Old Parent ClusterKey", 1, dummyParent.Z0_Number);
				AssertEquals("New Parent ClusterKey", 2, anotherDummyBizObj.Z0_Number);
				AssertEquals("Child ClusterKey (subtree master)", 3, dummyChild.ZD1_Number);
				AssertEquals("Grand-Child 1 ClusterKey", 3, dummyGrandChild1.ZDP_Number);
				AssertEquals("Grand-Child 2 ClusterKey", 3, dummyGrandChild2.ZDP_Number);
			});
		}

		public void TestSetClusterKeyIfNeeded_DoesNotThrowException_WhenFkToParentPtyIsNull()
		{
			var dummyWorkerOrMaster = Factory.New<DummyClusterKeyChildBizoWithNullParentPty>();

			AssertNoExceptionThrown(() => ClusterKeyManager.SetClusterKeyIfNeeded(dummyWorkerOrMaster));
		}

		public void TestTwoDifferentClusterKeyBizosUseSameClusterKeyNumberFountain()
		{
			var dummyParent1 = Factory.New<DummyClusterKeyParentBizo>();
			var dummyChild1 = dummyParent1.AddNewDependentBizo();
			var dummyGrandChild1 = dummyChild1.AddNewDependentBizo();

			var dummyParent2 = Factory.New<DummyClusterKeyChildBizo>();
			var dummyChild2 = dummyParent2.AddNewDependentBizo();

			CombineAssertions("[PRE-CONDITION] Initial Values", () =>
			{
				AssertEquals("Parent 1 ClusterKey", 0, dummyParent1.Z0_Number);
				AssertEquals("Child 1 ClusterKey", 0, dummyChild1.ZD1_Number);
				AssertEquals("Grand-Child 1 ClusterKey", 0, dummyGrandChild1.ZDP_Number);

				AssertEquals("Parent 2 ClusterKey", 0, dummyChild1.ZD1_Number);
				AssertEquals("Child 2 ClusterKey", 0, dummyGrandChild1.ZDP_Number);
			});

			Factory.Save();

			CombineAssertions("After Save", () =>
			{
				AssertEquals("Parent 1 ClusterKey", 1, dummyParent1.Z0_Number);
				AssertEquals("Child 1 ClusterKey", 1, dummyChild1.ZD1_Number);
				AssertEquals("Grand-Child 1 ClusterKey", 1, dummyGrandChild1.ZDP_Number);

				AssertEquals("Parent 2 ClusterKey", 2, dummyParent2.ZD1_Number);
				AssertEquals("Child 2 ClusterKey", 2, dummyChild2.ZDP_Number);
			});
		}
	}
}
