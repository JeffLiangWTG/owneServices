using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusOutturnHeaderProcessTask))]
	sealed class CusOutturnHeaderProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			var task = Factory.New<CusOutturnHeaderProcessTask>();
			AssertEquals(ControllerIDs.Customs.AU.SeaCargoOutturnBillsController, task.ParentControllerID);
		}

		public void TestParent()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			var task = (CusOutturnHeaderProcessTask)((IWorkflowProvider)outturnHeader).WorkflowItems.AddNew();
			AssertEquals(outturnHeader, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			return ((IWorkflowProvider)outturnHeader).WorkflowItems.AddNew();
		}
	}
}
