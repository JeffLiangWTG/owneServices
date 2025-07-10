using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(JPAFRHeaderProcessTask))]
	class JPAFRHeaderProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			AssertEquals(ControllerIDs.Customs.JP.AFR, Factory.New<JPAFRHeaderProcessTask>().ParentControllerID);
		}

		public void TestParent()
		{
			var oceanBill = Factory.New<JPAFRHeader>();
			var task = (JPAFRHeaderProcessTask)((IWorkflowProvider)oceanBill).WorkflowItems.AddNew();
			AssertEquals(oceanBill, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var oceanBill = Factory.New<JPAFRHeader>();
			return ((IWorkflowProvider)oceanBill).WorkflowItems.AddNew();
		}
	}
}
