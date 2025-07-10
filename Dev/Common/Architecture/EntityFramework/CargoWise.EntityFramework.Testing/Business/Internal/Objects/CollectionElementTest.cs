namespace CargoWise.EntityFramework.Testing
{
	sealed class CollectionElementTest : TestCaseWithDummy
	{
		public void TestHasParentCollection()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			AssertEquals(false, bizO.ParentCollections.Count > 0);

			AssertNotNull(bizO.ParentCollections); // lazy-creates an empty ParentCollections
			AssertEquals(false, bizO.ParentCollections.Count > 0);

			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.Add(bizO);
			AssertEquals(true, bizO.ParentCollections.Count > 0);
		}

		public void TestParentCollections()
		{
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			BusinessObject child = Factory.New(typeof(DummyBaseBusinessObject));

			AssertEquals("Not in collection", 0, Dummy.ParentCollections.Count);
			AssertEquals("Not in collection", 0, dummy2.ParentCollections.Count);
			AssertEquals("Not in collection", 0, child.ParentCollections.Count);

			Dummy.Collection.Add(child);
			AssertEquals("In dummy collection", 1, child.ParentCollections.Count);

			dummy2.Collection.Add(child);
			AssertEquals("In 2 collections", 2, child.ParentCollections.Count);

			Dummy.Collection.Remove(child);
			AssertEquals("In dummy2 collection", 1, child.ParentCollections.Count);

			dummy2.Collection.Remove(child);
			AssertEquals("Not in collection", 0, child.ParentCollections.Count);
		}
	}
}
