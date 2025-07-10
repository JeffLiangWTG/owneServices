namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectCollectionFetchStrategyTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			BusinessObjectCollectionFetchStrategyForTest strategy = (BusinessObjectCollectionFetchStrategyForTest)collection.FetchStrategy;
			AssertEquals(collection, strategy.Collection);
		}

		public void TestFetchForValidateCoreCalledOnceOnly()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			BusinessObjectCollectionFetchStrategyForTest strategy = (BusinessObjectCollectionFetchStrategyForTest)collection.FetchStrategy;
			AssertEquals(0, strategy.FetchForValidateCoreCount);
			((IBusiness)collection).RunPreSaveValidationFetch(true);
			AssertEquals(1, strategy.FetchForValidateCoreCount);
			((IBusiness)collection).RunPreSaveValidationFetch(true);
			AssertEquals(1, strategy.FetchForValidateCoreCount);
		}

		public void TestAdditionalFetchForView()
		{
			bool additionalFetchInvoked = false;
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.FetchStrategy.AdditionalFetchForView += (sender, eventArgs) => additionalFetchInvoked = true;

			Assert("Initial state", !additionalFetchInvoked);
			collection.FetchStrategy.FetchForView(System.Array.Empty<BusinessObject>(), System.Array.Empty<TableColumn>());
			Assert(additionalFetchInvoked);
		}
	}
}
