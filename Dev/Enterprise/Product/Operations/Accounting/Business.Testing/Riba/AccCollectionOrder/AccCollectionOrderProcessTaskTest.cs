using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	[TestedType(typeof(AccCollectionOrderProcessTask))]
	public class AccCollectionOrderProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var order = Factory.NewWithValidTestData<AccCollectionOrder>();
			var processTask = ((AccCollectionOrderProcessTaskCollection)order.WorkflowItems).AddNew();
			AssertEquals("Parent", order, processTask.Parent);
			AssertEquals("ParentControllerID", ControllerIDs.AccCollectionOrder, processTask.ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AccCollectionOrder>().WorkflowItems.AddNew();
		}
	}
}
