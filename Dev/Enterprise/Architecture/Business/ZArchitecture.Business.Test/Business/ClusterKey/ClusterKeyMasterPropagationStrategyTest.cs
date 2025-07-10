using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Test.Business.ClusterKey.Test;

namespace Enterprise.ZArchitecture.Business.ClusterKey.Testing
{
	sealed class ClusterKeyMasterPropagationStrategyTest : TestCaseWithFactory
	{
		public void TestSetClusterKeyIfRequired()
		{
			var dummyBizObj = Factory.New<DummyClusterKeyParentBizo>();
			AssertEquals("[PRE-CONDITION] Parent ClusterKey", 0, dummyBizObj.Z0_Number);

			ClusterKeyMasterPropagationStrategy.New(dummyBizObj).SetClusterKeyIfRequired();
			AssertEquals("[After Setting Cluster Key] Parent ClusterKey", 1, dummyBizObj.Z0_Number);
		}

		public void TestSetWorkerOrMasterClusterKeyIfRequired()
		{
			var dummyBizObj = Factory.New<DummyClusterKeyParentBizo>();
			var dummyWorkerOrMaster = Factory.New<DummyClusterKeyChildBizo>();

			CombineAssertions("[PRE-CONDITION] Initial Values", () =>
			{
				AssertEquals("Parent ClusterKey", 0, dummyBizObj.Z0_Number);
				AssertEquals("WorkerOrMaster ClusterKey", 0, dummyWorkerOrMaster.ZD1_Number);
			});

			ClusterKeyMasterPropagationStrategy.New(dummyWorkerOrMaster).SetClusterKeyIfRequired();

			CombineAssertions("After Calling Set Cluster Key on detached worker-or-master entity", () =>
			{
				AssertEquals("Parent ClusterKey", 0, dummyBizObj.Z0_Number);
				AssertEquals("WorkerOrMaster ClusterKey", 1, dummyWorkerOrMaster.ZD1_Number);
			});
		}

		public void TestClusterKeyNotSetOnDeletedObject()
		{
			var dummyBizObj = Factory.New<DummyClusterKeyParentBizo>();
			AssertEquals("[PRE-CONDITION] Parent ClusterKey", 0, dummyBizObj.Z0_Number);

			dummyBizObj.MockIsDeleted = true;
			ClusterKeyMasterPropagationStrategy.New(dummyBizObj).SetClusterKeyIfRequired();
			AssertEquals("[After calling SetClusterKeyIfRequired] Parent ClusterKey", 0, dummyBizObj.Z0_Number);
		}

		public void TestSetClusterKeyIfRequired_DoesNotThrowException_WhenFkToParentPtyIsNull()
		{
			var dummyWorkerOrMaster = Factory.New<DummyClusterKeyChildBizoWithNullParentPty>();

			CombineAssertions("[PRE-CONDITION] Initial Values", () =>
			{
				AssertEquals("WorkerOrMaster ClusterKey", 0, dummyWorkerOrMaster.ZD1_Number);
				AssertNull("WorkerOrMaster FkToParentPty", dummyWorkerOrMaster.FkToParentPty);
			});
			Factory.Save();

			AssertNoExceptionThrown("SetClusterKeyIfRequired Should not throw exception", () => ClusterKeyMasterPropagationStrategy.New(dummyWorkerOrMaster).SetClusterKeyIfRequired());

			CombineAssertions("After Calling Set Cluster Key on detached worker-or-master entity", () =>
			{
				AssertEquals("WorkerOrMaster ClusterKey", 1, dummyWorkerOrMaster.ZD1_Number);
			});
		}
	}
}
