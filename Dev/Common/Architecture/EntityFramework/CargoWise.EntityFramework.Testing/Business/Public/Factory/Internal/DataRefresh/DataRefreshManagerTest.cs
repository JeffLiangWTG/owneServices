using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DataRefreshManagerTest : TestCaseWithFactory
	{
		public void TestNotEnabledIfIsWeb()
		{
			DataRefreshManager manager = new DataRefreshManager();
			Assert("Enabled by default", manager.Enabled);

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.IsWeb = true;
				manager = new DataRefreshManager();
				Assert("Disabled for Web", !manager.Enabled);
			}
		}

		public void TestNotEnabledIfIsWebService()
		{
			DataRefreshManager manager = new DataRefreshManager();
			Assert("Enabled by default", manager.Enabled);

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.IsWebService = true;
				manager = new DataRefreshManager();
				Assert("Disabled for Web Service", !manager.Enabled);
			}
		}

		public void TestUpdateAll()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			ZGuid pK = dummy.PK;
			dummy.Z0_Code = "111";
			Factory.Save();

			RowFactory rowFactory1 = new RowFactory();
			DataRow dummyRow2 = rowFactory1.LoadFromPK(DummyBusinessObject.Schema.TableName, pK);
			DummyBusinessObject dummy2 = new DummyBusinessObject(new BusinessObjectFactory(), dummyRow2);

			DataRefreshManager manager = new DataRefreshManager();
			manager.StartManaging(dummy);
			manager.StartManaging(dummy2);

			dummy.Z0_Code = "222";
			Factory.Save();

			AssertEquals("Dummy2.Z0_Code", "222", dummy2.Z0_Code);

			List<BusinessObject> list = new List<BusinessObject>();
			list.Add(dummy2);
		}

		public void TestManagingCollections()
		{
			DataRefreshManager manager = new DataRefreshManager();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			DummyBusinessObjectCollection dummyCollection1 = new DummyBusinessObjectCollection(factory1);
			dummyCollection1.Load();
			manager.StartManaging(dummyCollection1);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			DummyBusinessObject dummy1 = factory2.New<DummyBusinessObject>();
			dummy1.Z0_Code = "123";
			BusinessObject[] cachedBusinessObjects = new BusinessObject[] { dummy1 };
			factory2.Save();

			AssertEquals("Collection.Count", 0, dummyCollection1.Count);
			manager.Publish(cachedBusinessObjects);
			AssertEquals("Collection.Count", 1, dummyCollection1.Count);
		}

		class BadDummy : DummyBusinessObject
		{
			public BadDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void CheckCanCopyPersistentValuesFrom()
			{
				throw new Exception("Pretending to blow up.");
			}
		}

		[ExpectNoExceptions()]
		public void TestPhantomObjectsWillNotBeUpdated()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BadDummy badDummy1 = factory2.New<BadDummy>();
			Guid badDummy1PK = badDummy1.PK.ToGuid();
			factory2.Save();

			factory2 = null;
			badDummy1 = null;

			BadDummy badDummy1Reloaded = Factory.Load<BadDummy>(badDummy1PK);
			badDummy1Reloaded.Z0_Code = "123";
			Factory.Save();
		}

		public void TestIsRefreshing()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();

			DataRefreshManager manager = new DataRefreshManager();
			AssertEquals("IsRefreshing", false, manager.IsRefreshing(dummy1));
			AssertEquals("IsRefreshing", false, manager.IsRefreshing(dummy2));

			manager.SetIsRefreshing(dummy1, true);
			AssertEquals("IsRefreshing", true, manager.IsRefreshing(dummy1));
			AssertEquals("IsRefreshing", false, manager.IsRefreshing(dummy2));

			manager.SetIsRefreshing(dummy2, true);
			AssertEquals("IsRefreshing", true, manager.IsRefreshing(dummy1));
			AssertEquals("IsRefreshing", true, manager.IsRefreshing(dummy2));

			manager.SetIsRefreshing(dummy1, false);
			AssertEquals("IsRefreshing", false, manager.IsRefreshing(dummy1));
			AssertEquals("IsRefreshing", true, manager.IsRefreshing(dummy2));

			manager.SetIsRefreshing(dummy2, false);
			AssertEquals("IsRefreshing", false, manager.IsRefreshing(dummy1));
			AssertEquals("IsRefreshing", false, manager.IsRefreshing(dummy2));
		}

		public void TestIfDeletedRowChangesHaveBeenAcceptedItShouldNotBlowUpWhenInPreparingAgain()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			Factory.RefreshEnabled = false;
			Factory.Save();

			dummy1.Delete();

			DataRefreshManager manager = new DataRefreshManager();

			((IBusinessObjectInternals)dummy1).AcceptChangesDelayedUntilJustBeforeSavingToDatabase = true;
			dummy1.Row.AcceptChanges();

			ICollection<BusinessObject> cachedBusinessObjects = new BusinessObject[] { dummy1 };

			AssertEquals("AcceptChangesDelayedUntilJustBeforeSavingToDatabase", true, ((IBusinessObjectInternals)dummy1).AcceptChangesDelayedUntilJustBeforeSavingToDatabase);
			manager.AcceptChangesOnRowsWithDelayedAcceptChanges(cachedBusinessObjects);
			AssertEquals("AcceptChangesDelayedUntilJustBeforeSavingToDatabase", false, ((IBusinessObjectInternals)dummy1).AcceptChangesDelayedUntilJustBeforeSavingToDatabase);
		}
		public void TestThreadSentryIsNotTriggeredByBusinessObjectCollectionSubscription()
		{
			var obj = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBizo = newFactory.Load<DummyBusinessObject>(obj.PK);

			AssertEquals(obj.PK, loadedBizo.PK);

			var subscribedCollection = new MySpecialCollection(newFactory);
			subscribedCollection.IsManagedForDataRefresh = true;
			subscribedCollection.Load();

			subscribedCollection.Add(loadedBizo);

			// emulating the situation when newFactory belongs to another thread - ThreadSentry will act similarly
			newFactory.ThreadSentry.RelinquishThreadOwnership();

			// adding some changes in the first factory to affect the collection through DataRefreshBus
			Factory.NewWithValidTestData<DummyBusinessObject>();

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestCollectionIsNotUpdatedWhenUsingRefreshDisable()
		{
			using (DataRefreshManager.BeginDisableRefresh())
			{
				var obj = Factory.NewWithValidTestData<DummyBusinessObject>();
				obj.Z0_Number = 5;
				Factory.Save();

				var newFactory = Factory.CreateNewFactory();
				var dummy = newFactory.Load<DummyBusinessObject>(obj.PK);

				var subscribedCollection = new MySpecialCollection(newFactory);
				subscribedCollection.IsManagedForDataRefresh = true;
				subscribedCollection.Load();

				subscribedCollection.Add(dummy);

				obj.Z0_Number = 1000;

				Factory.NewWithValidTestData<DummyBusinessObject>();

				AssertNoExceptionThrown(Factory.Save);
				AssertEquals(1, subscribedCollection.Count);
				AssertEquals(5, subscribedCollection[0].Z0_Number);
			}
		}

		public void TestCollectionIsUpdatedByBusinessObjectCollectionSubscriptionInSingleThread()
		{
			var obj = Factory.NewWithValidTestData<DummyBusinessObject>();
			obj.Z0_Number = 5;
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedBizo = newFactory.Load<DummyBusinessObject>(obj.PK);

			var subscribedCollection = new MySpecialCollection(newFactory);
			subscribedCollection.IsManagedForDataRefresh = true;
			// to be updated by DataRefreshBus the collection must load something at least once
			subscribedCollection.Load();
			int beforeCount = subscribedCollection.Count;

			subscribedCollection.Add(loadedBizo);

			obj.Z0_Number = 1000;

			// adding some changes in the first factory to affect the collection through DataRefreshBus
			Factory.NewWithValidTestData<DummyBusinessObject>();

			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(beforeCount + 1, subscribedCollection.Count);
			AssertEquals(1000, subscribedCollection[0].Z0_Number);
		}

		public void TestCollectionIsUpdatedByBusinessObjectCollectionSubscriptionInSingleThreadBackwards()
		{
			var newFactory = Factory.CreateNewFactory();
			var obj = newFactory.NewWithValidTestData<DummyBusinessObject>();
			obj.Z0_Number = 5;
			newFactory.Save();

			var loadedBizo = Factory.Load<DummyBusinessObject>(obj.PK);

			var subscribedCollection = new MySpecialCollection(Factory);
			subscribedCollection.IsManagedForDataRefresh = true;
			// to be updated by DataRefreshBus the collection must load something at least once
			subscribedCollection.Load();
			int beforeCount = subscribedCollection.Count;

			subscribedCollection.Add(loadedBizo);

			obj.Z0_Number = 1000;

			// adding some changes in the first factory to affect the collection through DataRefreshBus
			newFactory.NewWithValidTestData<DummyBusinessObject>();

			AssertNoExceptionThrown(newFactory.Save);
			AssertEquals(beforeCount + 1, subscribedCollection.Count);
			AssertEquals(1000, subscribedCollection[0].Z0_Number);
		}
	}
}
