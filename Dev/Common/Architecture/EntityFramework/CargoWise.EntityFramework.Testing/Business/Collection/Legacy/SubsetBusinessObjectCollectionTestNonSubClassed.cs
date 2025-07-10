namespace CargoWise.EntityFramework.Testing
{
	sealed class SubsetBusinessObjectCollectionTestNonSubClassed : TestCaseWithDummy
	{
		public void TestSubsetFilteringOnAddedAndRebuilds()
		{
			Dummy.Z0_Description = "INVIEW";

			var filteredBO = Factory.New<DummyBusinessObject>();
			filteredBO.Z0_Description = "NOTINVIEW";

			Dummy.Collection.Load();

			var testCollection = new DummySubsetCollection(Dummy.Collection);
			testCollection.RebuildCallCount_WithIsRebuildingTrue = 0;
			testCollection.OnAddedCallCount = 0;

			testCollection.Rebuild();
			AssertEquals("Should not increment when rebuilding, when no new elements are present in the parent collection", 0, testCollection.RebuildCallCount_WithIsRebuildingTrue);
			AssertEquals("Should not increment when rebuilding, when no new elements are present in the parent collection", 0, testCollection.OnAddedCallCount);

			var newChild = Dummy.Collection.AddNew();
			newChild.Z0_Description = "INVIEW";

			testCollection.Rebuild();
			AssertEquals("Should increment when rebuilding, when a new element is present in the parent collection", 1, testCollection.RebuildCallCount_WithIsRebuildingTrue);
			AssertEquals("Should increment when rebuilding, when a new element is present in the parent collection", 1, testCollection.OnAddedCallCount);

			testCollection.AddNew();
			AssertEquals("Should not increment on adding an element", 1, testCollection.RebuildCallCount_WithIsRebuildingTrue);
			AssertEquals("Should increment on adding an element", 2, testCollection.OnAddedCallCount);
		}

		public void TestSuppressRebuild()
		{
			Dummy.Z0_Description = "INVIEW";

			var filteredBO = Factory.New<DummyBusinessObject>();
			filteredBO.Z0_Description = "NOTINVIEW";

			Dummy.Collection.Load();

			var testCollection = new DummySubsetCollection(Dummy.Collection);
			AssertEquals(1, testCollection.RebuildCallCount);
			using (testCollection.SuppressRebuild())
			{
				testCollection.Rebuild();
			}
			AssertEquals(1, testCollection.RebuildCallCount);
			testCollection.Rebuild();
			AssertEquals(2, testCollection.RebuildCallCount);
		}
	}
}
