using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderProcessTask))]
	sealed class AsycudaManifestHeaderProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			AssertEquals(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest, Factory.New<AsycudaManifestHeaderProcessTask>().ParentControllerID);
		}

		public void TestParent()
		{
			var asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var task = (AsycudaManifestHeaderProcessTask)((IWorkflowProvider)asycudaManifestHeader).WorkflowItems.AddNew();
			AssertEquals(asycudaManifestHeader, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return ((IWorkflowProvider)asycudaManifestHeader).WorkflowItems.AddNew();
		}
	}
}
