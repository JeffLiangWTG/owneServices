using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusMAWBProcessTask))]
	sealed class CusMAWBProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			CusMAWBProcessTask task = Factory.New<CusMAWBProcessTask>();
			AssertEquals(ControllerIDs.Customs.AU.AirCargo, task.ParentControllerID);
		}

		public void TestParent()
		{
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			CusMAWBProcessTask task = (CusMAWBProcessTask)((IWorkflowProvider)cusMAWB).WorkflowItems.AddNew();
			AssertEquals(cusMAWB, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject() => ((IWorkflowProvider)Factory.New<CusMAWB>()).WorkflowItems.AddNew();
	}
}
