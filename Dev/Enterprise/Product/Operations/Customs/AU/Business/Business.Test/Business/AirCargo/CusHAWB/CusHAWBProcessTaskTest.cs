using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusHAWBProcessTask))]
	sealed class CusHAWBProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			var task = Factory.New<CusHAWBProcessTask>();
			AssertEquals(ControllerIDs.Customs.AU.HouseAirCargo, task.ParentControllerID);
		}

		public void TestParent()
		{
			var cusHAWB = Factory.New<CusHAWB>();
			var task = (CusHAWBProcessTask)((IWorkflowProvider)cusHAWB).WorkflowItems.AddNew();
			AssertEquals(cusHAWB, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var cusHAWB = Factory.New<CusHAWB>();
			return ((IWorkflowProvider)cusHAWB).WorkflowItems.AddNew();
		}
	}
}
