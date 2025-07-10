namespace CargoWise.EntityFramework.Testing
{
	sealed class SingleBusinessObjectFactoryProviderTest : TestCaseWithFactory
	{
		public void TestCreateNewWithoutSave()
		{
			SingleBusinessObjectFactoryProvider factoryProvider = new SingleBusinessObjectFactoryProvider(Factory);
			factoryProvider.CreateNewWithoutSave();
			AssertEquals("Same factory should always be retained", Factory, factoryProvider.Current);
		}

		public void TestSaveCurrentAndCreateNew()
		{
			DummyBusinessObject dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			SingleBusinessObjectFactoryProvider factoryProvider = new SingleBusinessObjectFactoryProvider(Factory);

			AssertEquals("Factory should not be saved initially for the test", false, dummy.IsInDatabase);
			factoryProvider.SaveCurrentAndCreateNew();
			AssertEquals("Factory should be saved", true, dummy.IsInDatabase);
			AssertEquals("Same factory should always be retained", Factory, factoryProvider.Current);
		}
	}
}
