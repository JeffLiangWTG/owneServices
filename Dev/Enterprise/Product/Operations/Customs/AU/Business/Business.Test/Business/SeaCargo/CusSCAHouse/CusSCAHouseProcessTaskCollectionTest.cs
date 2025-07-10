using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAHouseProcessTaskCollection))]
	sealed class CusSCAHouseProcessTaskCollectionTest : ProcessTaskCollectionTest<CusSCAHouseProcessTaskCollection>
	{
		public void TestParent()
		{
			var parent = Factory.New<CusSCAHouse>();
			AssertEquals(parent, ((IWorkflowProvider)parent).WorkflowItems.Parent);
		}

		public void TestIndexer()
		{
			var parent = Factory.New<CusSCAHouse>();
			var collection = (CusSCAHouseProcessTaskCollection)((IWorkflowProvider)parent).WorkflowItems;
			var task = collection.AddNew();
			AssertEquals(task, collection[0]);
		}

		protected override CusSCAHouseProcessTaskCollection GetCollectionToTestCore()
		{
			var parent = Factory.New<CusSCAHouse>();
			return new CusSCAHouseProcessTaskCollection(parent);
		}
	}
}
