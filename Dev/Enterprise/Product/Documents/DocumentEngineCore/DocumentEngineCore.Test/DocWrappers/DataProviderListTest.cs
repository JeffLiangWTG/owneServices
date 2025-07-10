using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class DataProviderListTest : TestCaseWithFactory
	{
		public void TestConstructionWithNullDataProviderDoesntAddANullToTheListOfProviders()
		{
			var dataProviders = new DataProviderList((IBODocDataProvider)null);
			AssertEquals("dataProviders.AllDataProviders.Length", 0, dataProviders.AllDataProviders.Length);
			AssertEquals("dataProviders.PrimaryDataProvider", null, dataProviders.PrimaryDataProvider);
		}

		public void TestAddingAdditionalProviders()
		{
			DummyBusinessObject mainBO = Factory.New<DummyBusinessObject>();
			var mainDataProvider = BODocDataProvider.Get(mainBO);
			var dataProviders = new DataProviderList(mainDataProvider);
			AssertEquals("dataProviders.AllDataProviders.Length", 1, dataProviders.AllDataProviders.Length);
			AssertEquals("dataProviders.AllDataProviders[0]", mainDataProvider, dataProviders.AllDataProviders[0]);
			AssertEquals("dataProviders.PrimaryDataProvider should always be the first one passed in", mainDataProvider, dataProviders.PrimaryDataProvider);

			DummyBusinessObject secondaryBO = Factory.New<DummyBusinessObject>();
			var secondaryDataProvider = BODocDataProvider.Get(secondaryBO);
			dataProviders.Add(secondaryDataProvider);
			AssertEquals("dataProviders.AllDataProviders.Length", 2, dataProviders.AllDataProviders.Length);
			AssertEquals("dataProviders.AllDataProviders[0]", mainDataProvider, dataProviders.AllDataProviders[0]);
			AssertEquals("dataProviders.AllDataProviders[1]", secondaryDataProvider, dataProviders.AllDataProviders[1]);
			AssertEquals("dataProviders.PrimaryDataProvider should always be the first one passed in", mainDataProvider, dataProviders.PrimaryDataProvider);
		}

		public void TestPrimaryProvider()
		{
			DummyBusinessObject mainBO = Factory.New<DummyBusinessObject>();
			var enteredProvider = BODocDataProvider.Get(mainBO);

			var dataProviders = new DataProviderList(enteredProvider);
			AssertEquals("The returned IBODocDataProvider is the same one passed in", enteredProvider, dataProviders.PrimaryDataProvider);
		}

		public void TestYouCanPassMultipleDataProvidersIntoTheConstructor()
		{
			DummyBusinessObject mainBO = Factory.New<DummyBusinessObject>();
			var mainDataProvider = BODocDataProvider.Get(mainBO);
			DummyBusinessObject secondaryBO = Factory.New<DummyBusinessObject>();
			var secondaryDataProvider = BODocDataProvider.Get(secondaryBO);

			var dataProviders = new DataProviderList(mainDataProvider, secondaryDataProvider);
			AssertEquals("dataProviders.AllDataProviders.Length", 2, dataProviders.AllDataProviders.Length);
			AssertEquals("dataProviders.AllDataProviders[0]", mainDataProvider, dataProviders.AllDataProviders[0]);
			AssertEquals("dataProviders.AllDataProviders[1]", secondaryDataProvider, dataProviders.AllDataProviders[1]);
			AssertEquals("dataProviders.PrimaryDataProvider should always be the first one passed in", mainDataProvider, dataProviders.PrimaryDataProvider);
		}
	}
}
