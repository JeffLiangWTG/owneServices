using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaBillProcessTask))]
	sealed class AsycudaBillProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			AssertEquals(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill, Factory.New<AsycudaBillProcessTask>().ParentControllerID);
		}

		public void TestParent()
		{
			var asycudaBill = Factory.NewWithValidTestData<AsycudaBill>();
			var task = (AsycudaBillProcessTask)((IWorkflowProvider)asycudaBill).WorkflowItems.AddNew();
			AssertEquals(asycudaBill, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var asycudaBill = Factory.NewWithValidTestData<AsycudaBill>();
			return ((IWorkflowProvider)asycudaBill).WorkflowItems.AddNew();
		}
	}
}
