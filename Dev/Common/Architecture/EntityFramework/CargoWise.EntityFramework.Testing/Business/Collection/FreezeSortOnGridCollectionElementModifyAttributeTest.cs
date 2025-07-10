namespace CargoWise.EntityFramework.Testing
{
	sealed class FreezeSortOnGridCollectionElementModifyAttributeTest : TestCaseWithFactory
	{
		public void TestIsEnabled()
		{
			AssertEquals(true, FreezeSortOnGridCollectionElementModifyAttribute.IsEnabled(new CollectionWithFreezeSort(Factory)));
			AssertEquals(false, FreezeSortOnGridCollectionElementModifyAttribute.IsEnabled(new CollectionWithoutFreezeSort(Factory)));
		}

		class CollectionWithFreezeSort : ActiveBusinessObjectCollection<DummyBusinessObject>
		{
			public CollectionWithFreezeSort(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		[FreezeSortOnGridCollectionElementModify(false)]
		class CollectionWithoutFreezeSort : ActiveBusinessObjectCollection<DummyBusinessObject>
		{
			public CollectionWithoutFreezeSort(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}
	}
}
