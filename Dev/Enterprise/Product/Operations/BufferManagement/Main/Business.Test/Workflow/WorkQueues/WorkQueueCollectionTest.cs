using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(WorkQueueCollection))]
	class WorkQueueCollectionTest : ActiveBusinessObjectCollectionTestCase<WorkQueueCollection>
	{
		public void TestCollection_ShouldContainMagnitudesInsideQueueTagGroup()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "QU1", "Queue `");
			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "QU2", "Queue @");

			Factory.Save();

			var collection = new WorkQueueCollection(Factory.CreateNewFactory());
			AssertEquals(2, collection.Count);
			AssertCollectionContains(collection, q => q.PK == queue1.PK);
			AssertCollectionContains(collection, q => q.PK == queue2.PK);
		}
	}
}
