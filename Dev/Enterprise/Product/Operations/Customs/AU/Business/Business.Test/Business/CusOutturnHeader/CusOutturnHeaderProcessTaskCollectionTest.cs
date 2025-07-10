using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusOutturnHeaderProcessTaskCollection))]
	sealed class CusOutturnHeaderProcessTaskCollectionTest : ProcessTaskCollectionTest<CusOutturnHeaderProcessTaskCollection>
	{
		public void TestParent()
		{
			var parent = Factory.New<CusOutturnHeader>();
			AssertEquals(parent, ((IWorkflowProvider)parent).WorkflowItems.Parent);
		}

		public void TestIndexer()
		{
			var parent = Factory.New<CusOutturnHeader>();
			var collection = (CusOutturnHeaderProcessTaskCollection)((IWorkflowProvider)parent).WorkflowItems;
			var task = collection.AddNew();
			AssertEquals(task, collection[0]);
		}

		protected override CusOutturnHeaderProcessTaskCollection GetCollectionToTestCore()
		{
			var parent = Factory.New<CusOutturnHeader>();
			return new CusOutturnHeaderProcessTaskCollection(parent);
		}
	}
}
