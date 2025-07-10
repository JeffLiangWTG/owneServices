using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZTreePathTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy3 = Factory.New<DummyBusinessObject>();
			var dummyNode1 = new DummyGenPivotNode(null, dummy1);
			var dummyNode2 = new DummyGenPivotNode(null, dummy2);
			var dummyNode3 = new DummyGenPivotNode(null, dummy3);

			var path = new ZTreePath<DummyBusinessObject>(new[] { dummyNode1, dummyNode2, dummyNode3 });

			AssertArrayEqualsByElements(new[] { dummyNode1, dummyNode2, dummyNode3 }, path.FullPath);
			AssertEquals(dummyNode1, path.FirstNode);
			AssertEquals(false, path.IsEmpty());

			var emptyPath = new ZTreePath<DummyBusinessObject>(System.Array.Empty<DummyGenPivotNode>());
			AssertEquals(true, emptyPath.IsEmpty());
		}
	}
}
