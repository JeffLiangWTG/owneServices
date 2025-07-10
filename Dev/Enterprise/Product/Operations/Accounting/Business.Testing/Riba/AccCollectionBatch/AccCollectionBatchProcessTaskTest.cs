using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	using Enterprise.MasterFiles.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(AccCollectionBatchProcessTask))]
	public class AccCollectionBatchProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var sequence = Factory.NewWithValidTestData<AccCollectionBatch>();
			AccCollectionBatchProcessTask processTask = ((AccCollectionBatchProcessTaskCollection)sequence.WorkflowItems).AddNew();
			AssertEquals("Parent", sequence, processTask.Parent);
			AssertEquals("ParentControllerID", ControllerIDs.AccCollectionBatch, processTask.ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AccCollectionBatch>().WorkflowItems.AddNew();
		}
	}
}
