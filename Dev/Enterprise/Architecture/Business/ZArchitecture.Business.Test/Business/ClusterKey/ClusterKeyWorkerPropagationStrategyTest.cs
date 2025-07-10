using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Test.Business.ClusterKey.Test;

namespace Enterprise.ZArchitecture.Business.ClusterKey.Testing
{
	sealed class ClusterKeyWorkerPropagationStrategyTest : TestCaseWithFactory
	{
		public void TestSetClusterKeyIfRequired()
		{
			var dummyParent = Factory.New<DummyClusterKeyParentBizo>();
			var dummyChild = dummyParent.AddNewDependentBizo();

			CombineAssertions("[PRE-CONDITION] Initial Values", () =>
			{
				AssertEquals("Parent ClusterKey", 0, dummyParent.Z0_Number);
				AssertEquals("Child ClusterKey", 0, dummyChild.ZD1_Number);
			});

			ClusterKeyWorkerPropagationStrategy.New(dummyChild).SetClusterKeyIfRequired();

			CombineAssertions("After Setting Child Cluster Key", () =>
			{
				AssertEquals("Parent ClusterKey", 1, dummyParent.Z0_Number);
				AssertEquals("Child ClusterKey", 1, dummyChild.ZD1_Number);
			});
		}

		public void TestSetClusterKeyIfRequired_DetachedEntity()
		{
			var dummyParent = Factory.New<DummyClusterKeyParentBizo>();
			var dummyChild = dummyParent.AddNewDependentBizo();
			Factory.Save();

			CombineAssertions("After Save", () =>
			{
				AssertEquals("Parent ClusterKey", 1, dummyParent.Z0_Number);
				AssertEquals("Child ClusterKey", 1, dummyChild.ZD1_Number);
			});

			var dummyGrandChild = Factory.New<DummyClusterKeyGrandChildBizo>();
			ClusterKeyWorkerPropagationStrategy.New(dummyGrandChild).SetClusterKeyIfRequired();

			CombineAssertions("After Setting Detached Grand-Child Cluster Key", () =>
			{
				AssertEquals("Parent ClusterKey", 1, dummyParent.Z0_Number);
				AssertEquals("Child ClusterKey", 1, dummyChild.ZD1_Number);
				AssertEquals("Grand-Child ClusterKey", 0, dummyGrandChild.ZDP_Number);
			});
		}

		public void TestLoadAndFlagDescendantsForSavingIfKeyCascadingRequired()
		{
			var dummyParent = Factory.New<DummyClusterKeyParentBizo>();
			var dummyChild = dummyParent.AddNewDependentBizo();
			var dummyGrandChild1 = dummyChild.AddNewDependentBizo();
			var dummyGrandChild2 = dummyChild.AddNewDependentBizo();
			Factory.Save();

			CombineAssertions("After Save", () =>
			{
				AssertEquals("Parent ClusterKey", 1, dummyParent.Z0_Number);
				AssertEquals("Child ClusterKey", 1, dummyChild.ZD1_Number);
				AssertEquals("Grand-Child 1 ClusterKey", 1, dummyGrandChild1.ZDP_Number);
				AssertEquals("Grand-Child 2 ClusterKey", 1, dummyGrandChild2.ZDP_Number);
			});

			var anotherDummyBizObj = Factory.New<DummyClusterKeyParentBizo>();
			Factory.Save();
			AssertEquals("New Parent ClusterKey", 2, anotherDummyBizObj.Z0_Number);

			dummyChild.ZD1_Z0 = anotherDummyBizObj.PK;
			ClusterKeyWorkerPropagationStrategy.New(dummyChild).LoadAndFlagDescendantsForSavingIfRequired();

			CombineAssertions("After flagging descendants for saving", () =>
			{
				AssertEquals("Old Parent ClusterKey", 1, dummyParent.Z0_Number);
				AssertEquals("New Parent ClusterKey", 2, anotherDummyBizObj.Z0_Number);
				AssertEquals("Child ClusterKey", -1, dummyChild.ZD1_Number);
				AssertEquals("Grand-Child 1 ClusterKey", -1, dummyGrandChild1.ZDP_Number);
				AssertEquals("Grand-Child 2 ClusterKey", -1, dummyGrandChild2.ZDP_Number);
			});

			ClusterKeyWorkerPropagationStrategy.New(dummyGrandChild1).SetClusterKeyIfRequired();

			CombineAssertions("After setting grand-child 1 cluster key", () =>
			{
				AssertEquals("Old Parent ClusterKey", 1, dummyParent.Z0_Number);
				AssertEquals("New Parent ClusterKey", 2, anotherDummyBizObj.Z0_Number);
				AssertEquals("Child ClusterKey", 2, dummyChild.ZD1_Number);
				AssertEquals("Grand-Child 1 ClusterKey", 2, dummyGrandChild1.ZDP_Number);
				AssertEquals("Grand-Child 2 ClusterKey", -1, dummyGrandChild2.ZDP_Number);
			});

			ClusterKeyWorkerPropagationStrategy.New(dummyGrandChild2).SetClusterKeyIfRequired();
			AssertEquals("Grand-Child 2 ClusterKey", 2, dummyGrandChild2.ZDP_Number);
		}

		public void TestClusterKeyNotSetOnDeletedObject()
		{
			var dummyDependentBizObj = Factory.New<DummyClusterKeyChildBizo>();
			AssertEquals("[PRE-CONDITION] ClusterKey", 0, dummyDependentBizObj.ZD1_Number);

			dummyDependentBizObj.MockIsDeleted = true;
			ClusterKeyWorkerPropagationStrategy.New(dummyDependentBizObj).SetClusterKeyIfRequired();
			AssertEquals("[After calling SetClusterKeyIfRequired] ClusterKey", 0, dummyDependentBizObj.ZD1_Number);
		}

		public void TestClusterKeyParent_DoesNotThrowException_WhenFkToParentPtyIsNull()
		{
			var dummyDependentBizObj = Factory.New<DummyClusterKeyChildBizoWithNullParentPty>();
			dummyDependentBizObj.ClusterKeyPty.Value = 1;

			CombineAssertions("[PRE-CONDITION] Initial Values", () =>
			{
				AssertEquals("ClusterKey value should be set", 1, dummyDependentBizObj.ClusterKeyPty.Value);
				AssertDbHits("Db hit count", new Dictionary<string, int> { { DummyClusterKeyParentBizo.Schema.TableName, 0 } }, Factory);
				AssertNull("WorkerOrMaster FkToParentPty", dummyDependentBizObj.FkToParentPty);
			});

			AssertNoExceptionThrown("SetClusterKeyIfRequired Should not throw exception", () => ClusterKeyWorkerPropagationStrategy.New(dummyDependentBizObj).SetClusterKeyIfRequired());
			AssertEquals("ClusterKey value should be reset", 0, dummyDependentBizObj.ClusterKeyPty.Value);
			AssertDbHits("Parent load should not happen", new Dictionary<string, int> { { DummyClusterKeyParentBizo.Schema.TableName, 0 } }, Factory);
		}
	}
}
