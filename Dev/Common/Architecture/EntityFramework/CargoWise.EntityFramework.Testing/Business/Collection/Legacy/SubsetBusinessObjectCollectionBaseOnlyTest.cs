namespace CargoWise.EntityFramework.Testing
{
	sealed class SubsetBusinessObjectCollectionBaseOnlyTest : SubsetBusinessObjectCollectionTestCase<DummyBaseBusinessObject>
	{
		public void TestGetTypeDeciderContext()
		{
			var dummyWithDependentsAndTypeDeciderContext = Factory.New<DummyWithDependentsAndTypeDeciderContextBusinessObject>();
			var newCollection = GetNewCollection(dummyWithDependentsAndTypeDeciderContext.Dependents) as BusinessObjectCollection;

			var typeDeciderContext = newCollection.GetTypeDeciderContext();
			AssertNotNull(typeDeciderContext);
			AssertSame(dummyWithDependentsAndTypeDeciderContext, typeDeciderContext);
		}

		protected override ISubsetBusinessObjectCollection GetNewCollection(BusinessObjectCollection collectionToFilter)
		{
			return new DummySubsetCollection(collectionToFilter);
		}
	}
}
