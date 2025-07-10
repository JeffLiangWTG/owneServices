using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	[TestedType(typeof(AccPayableOrderHeaderProcessTaskCollection))]
	public class AccPayableOrderHeaderProcessTaskCollectionTest : ProcessTaskCollectionTest<AccPayableOrderHeaderProcessTaskCollection>
	{
		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(AccPayableOrderHeaderProcessTask), collection.AddNew().GetType());
		}

		protected override AccPayableOrderHeaderProcessTaskCollection GetCollectionToTestCore()
		{
			return (AccPayableOrderHeaderProcessTaskCollection)Factory.NewWithValidTestData<AccPayableOrderHeader>().WorkflowItems;
		}
	}
}
