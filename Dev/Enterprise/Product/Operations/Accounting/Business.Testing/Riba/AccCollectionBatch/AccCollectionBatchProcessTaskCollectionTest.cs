using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	[TestedType(typeof(AccCollectionBatchProcessTaskCollection))]
	public class AccCollectionBatchProcessTaskCollectionTest : ProcessTaskCollectionTest<AccCollectionBatchProcessTaskCollection>
	{
		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(AccCollectionBatchProcessTask), collection.AddNew().GetType());
		}

		protected override AccCollectionBatchProcessTaskCollection GetCollectionToTestCore()
		{
			return (AccCollectionBatchProcessTaskCollection)Factory.NewWithValidTestData<AccCollectionBatch>().WorkflowItems;
		}
	}
}
