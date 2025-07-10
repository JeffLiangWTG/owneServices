using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAHouseProcessTask))]
	sealed class CusSCAHouseProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			var task = Factory.New<CusSCAHouseProcessTask>();
			AssertEquals(ControllerIDs.Customs.AU.SeaCargoHouseController, task.ParentControllerID);
		}

		public void TestParent()
		{
			var house = Factory.New<CusSCAHouse>();
			var task = (CusSCAHouseProcessTask)((IWorkflowProvider)house).WorkflowItems.AddNew();
			Assert(typeof(CusSCAHouse).IsInstanceOfType(task.Parent));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var house = Factory.New<CusSCAHouse>();
			return ((IWorkflowProvider)house).WorkflowItems.AddNew();
		}
	}
}
