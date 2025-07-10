using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class DataTransferBusinessObjectFactoryProviderTest : TransactionedTestCase
	{
		public void TestSaveTogether()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = factory1.New<DummyBusinessObject>();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = factory2.New<DummyBusinessObject>();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			DummyBusinessObject dummy3 = factory3.New<DummyBusinessObject>();

			Assert("Objects should not be yet saved into db", !dummy1.IsInDatabase);
			Assert("Objects should not be yet saved into db", !dummy2.IsInDatabase);
			Assert("Objects should not be yet saved into db", !dummy3.IsInDatabase);

			DataTransferBusinessObjectFactoryProvider factoryProvider = new DataTransferBusinessObjectFactoryProvider(factory1);

			AssertEquals(factory1, factoryProvider.Current);

			factoryProvider.SaveCurrentWithAdditionalParticipantsWithoutCreateNew(factory2, factory3);

			AssertEquals("Current factory should not be replaced with new", factory1, factoryProvider.Current);

			Assert("Objects should be already saved into db", dummy1.IsInDatabase);
			Assert("Objects should be already saved into db", dummy2.IsInDatabase);
			Assert("Objects should be already saved into db", dummy3.IsInDatabase);
		}

		public void TestSaveTogetherAndCreateNew()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = factory1.New<DummyBusinessObject>();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = factory2.New<DummyBusinessObject>();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			DummyBusinessObject dummy3 = factory3.New<DummyBusinessObject>();

			Assert("Objects should not be yet saved into db", !dummy1.IsInDatabase);
			Assert("Objects should not be yet saved into db", !dummy2.IsInDatabase);
			Assert("Objects should not be yet saved into db", !dummy3.IsInDatabase);

			DataTransferBusinessObjectFactoryProvider factoryProvider = new DataTransferBusinessObjectFactoryProvider(factory1);

			AssertEquals(factory1, factoryProvider.Current);

			factoryProvider.SaveCurrentWithAdditionalParticipantsAndCreateNew(factory2, factory3);

			AssertNotEquals("Current factory should be replaced with new", factory1, factoryProvider.Current);

			Assert("Objects should be already saved into db", dummy1.IsInDatabase);
			Assert("Objects should be already saved into db", dummy2.IsInDatabase);
			Assert("Objects should be already saved into db", dummy3.IsInDatabase);
		}

		public void TestUpdateRecordCounts()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = factory1.New<DummyBusinessObject>();
			factory1.Save();
			dummy1.Z0_Code = "AAA";
			factory1.New<DummyBusinessObject>();
			factory1.New<DummyBusinessObject>();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = factory2.New<DummyBusinessObject>();
			factory2.Save();
			dummy2.Z0_Code = "BBB";

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			factory3.New<DummyBusinessObject>();

			DataTransferBusinessObjectFactoryProvider factoryProvider = new DataTransferBusinessObjectFactoryProvider(factory1);

			AssertEquals("Nothing yet saved within factory provider", 0, factoryProvider.RecordsAdded);
			AssertEquals("Nothing yet saved within factory provider", 0, factoryProvider.RecordsModified);

			factoryProvider.SaveCurrentWithAdditionalParticipantsWithoutCreateNew(factory2, factory3);

			AssertEquals("Only records from main factory should be counted", 2, factoryProvider.RecordsAdded);
			AssertEquals("Only records from main factory should be counted", 1, factoryProvider.RecordsModified);
		}
	}
}
