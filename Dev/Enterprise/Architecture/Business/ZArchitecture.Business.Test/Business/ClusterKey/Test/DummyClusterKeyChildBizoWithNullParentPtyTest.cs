using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.ZArchitecture.Business.Test.Business.ClusterKey.Test
{
	sealed class DummyClusterKeyChildBizoWithNullParentPtyTest : TestCaseWithFactory
	{
		public void TestFkToParentPtyIsNull()
		{
			var dummyWorkerOrMaster = Factory.New<DummyClusterKeyChildBizoWithNullParentPty>();

			AssertNull("WorkerOrMaster FkToParentPty should be null", dummyWorkerOrMaster.FkToParentPty);

			var dummyAsWorker = dummyWorkerOrMaster as IClusterKeyWorker;
			AssertNotNull("dummyWorkerOrMaster should be IClusterKeyWorker", dummyAsWorker);
			AssertNull("dummyAsWorker FkToParentPty should be null", dummyWorkerOrMaster.FkToParentPty);
		}
	}
}
