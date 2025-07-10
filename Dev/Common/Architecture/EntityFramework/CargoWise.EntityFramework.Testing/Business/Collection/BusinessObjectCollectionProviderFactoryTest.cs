using System;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectCollectionProviderFactoryTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			BusinessObjectCollectionProvider provider1 = ProviderFactory.Get(typeof(DummyBusinessObjectCollection));
			AssertNotNull("provider1", provider1);
			AssertCollection("provider1(Factory)", Factory, typeof(DummyBusinessObjectCollection), provider1(Factory));

			BusinessObjectCollectionProvider provider2 = ProviderFactory.Get(typeof(DummyChildBusinessObjectCollection));
			AssertNotNull("provider2", provider2);
			AssertCollection("provider2(Factory)", Factory, typeof(DummyChildBusinessObjectCollection), provider2(Factory));
		}

		public void TestValidateArguments()
		{
			AssertExceptionThrown("(null)", typeof(ArgumentNullException), delegate
			{ ProviderFactory.Get(null); });
			AssertExceptionThrown("Not a BusinessObjectCollection", typeof(ArgumentOutOfRangeException), delegate
			{ ProviderFactory.Get(typeof(BusinessObject)); });
			AssertExceptionThrown("Abstract", typeof(ArgumentOutOfRangeException), delegate
			{ ProviderFactory.Get(typeof(BusinessObjectCollection)); });
		}

		public void TestCacheProviders()
		{
			BusinessObjectCollectionProvider provider1 = ProviderFactory.Get(typeof(DummyBusinessObjectCollection));
			BusinessObjectCollectionProvider provider2 = ProviderFactory.Get(typeof(DummyBusinessObjectCollection));

			AssertSame(provider1, provider2);
		}

		#region Implementation

		static void AssertCollection(string message, BusinessObjectFactory factory, Type collectionType, IBusinessObjectCollection collection)
		{
			AssertNotNull(message, collection);
			AssertEquals(message, collectionType, collection.GetType());
			AssertEquals(message, factory, collection.Factory);
		}

		BusinessObjectCollectionProviderFactory ProviderFactory
		{
			get { return providerFactory ?? (providerFactory = new BusinessObjectCollectionProviderFactory()); }
		}
		BusinessObjectCollectionProviderFactory providerFactory;

		#endregion
	}
}
