using CargoWise.EntityFramework.Testing;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class DataTransferSingleBusinessObjectFactoryProviderTest : TestCaseWithFactory
	{
		public void TestCreateNewWithoutSave()
		{
			DataTransferSingleBusinessObjectFactoryProvider factoryProvider = new DataTransferSingleBusinessObjectFactoryProvider(Factory);
			factoryProvider.CreateNewWithoutSave();
			AssertEquals("Same factory should always be retained", Factory, factoryProvider.Current);
		}

		public void TestSaveCurrentAndCreateNew()
		{
			DummyBusinessObject dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			DataTransferSingleBusinessObjectFactoryProvider factoryProvider = new DataTransferSingleBusinessObjectFactoryProvider(Factory);

			AssertEquals("Factory should not be saved initially for the test", false, dummy.IsInDatabase);
			factoryProvider.SaveCurrentAndCreateNew();
			AssertEquals("Factory should be saved", true, dummy.IsInDatabase);
			AssertEquals("Same factory should always be retained", Factory, factoryProvider.Current);
		}
	}
}
