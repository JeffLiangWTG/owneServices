using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	[TestedType(typeof(AccCollectionOrderProcessTaskCollection))]
	public class AccCollectionOrderProcessTaskCollectionTest : ProcessTaskCollectionTest<AccCollectionOrderProcessTaskCollection>
	{
		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(AccCollectionOrderProcessTask), collection.AddNew().GetType());
		}

		protected override AccCollectionOrderProcessTaskCollection GetCollectionToTestCore()
		{
			return (AccCollectionOrderProcessTaskCollection)Factory.NewWithValidTestData<AccCollectionOrder>().WorkflowItems;
		}
	}
}
