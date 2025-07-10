using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	using Enterprise.Accounting.Business.PayableOrder;
	using Enterprise.MasterFiles.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(AccPayableOrderHeaderProcessTask))]
	public class AccPayableOrderHeaderProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var sequence = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			AccPayableOrderHeaderProcessTask processTask = ((AccPayableOrderHeaderProcessTaskCollection)sequence.WorkflowItems).AddNew();
			AssertEquals("Parent", sequence, processTask.Parent);
			AssertEquals("ParentControllerID", ControllerIDs.AccPayableOrder, processTask.ParentControllerID);
		}

		public void TestOverrideJobNumber()
		{
			var accPayableOrderHeader = Factory.New<AccPayableOrderHeader>();
			accPayableOrderHeader.APH_OrderNumber = "S00000001";
			var processTask = ((AccPayableOrderHeaderProcessTaskCollection)accPayableOrderHeader.WorkflowItems).AddNew();
			AssertEquals("S00000001", processTask.JobNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AccPayableOrderHeader>().WorkflowItems.AddNew();
		}
	}
}
