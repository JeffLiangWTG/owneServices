using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DataRefreshBusTest : TransactionedTestCase
	{
		public void TestAddByDataRefreshForDependentRegisteredEditableCollectionsThatImplementMatchingCollectionFilter()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject master = factory.New<DummyBusinessObject>();
			factory.Save();

			DummyDependentBusinessObjectCollection collection = new DummyDependentBusinessObjectCollection(master, master.Factory);
			master.RegisterEditableChildObject(collection);
			Assert("PreCondition: collection is IBusinessObjectFilterFactory", collection is IBusinessObjectFilterFactory);
			collection.Load();

			BusinessObjectFactory childFactory = new BusinessObjectFactory();
			DummyDependantBusinessObject child = childFactory.New<DummyDependantBusinessObject>();
			child.ZD1_Z0 = master.PK;
			var isMatching = ((IBusinessObjectFilterFactory)collection).NewBusinessObjectFilter().IsMatching(child);
			Assert("PreCondition: matches filter", isMatching);
			AssertEquals("PreCondition: OnAddedCalledByDataRefresh not set", null, collection.OnAddedCalledByDataRefresh);

			childFactory.Save();

			AssertEquals("IsUpdatingByDataRefreshBus should be true when adding for data refresh", true, collection.OnAddedCalledByDataRefresh);
		}

		[ExpectNoExceptions]
		public void TestFetchForRefresh_ShouldNotFetchDeletedObjects()
		{
			var collection = new DummyBusinessObjectCollectionWhichAccessesPropertyOnIsMatchingCollectionFilter(new BusinessObjectFactory());
			var child = new BusinessObjectFactory().New<DummyBusinessObject>();
			child.Delete();

			var subscription = new DataRefreshBus.BusinessObjectCollectionSubscription(collection);
			subscription.FetchForRefresh(new[] { child });
		}

		Thread RunThreadWithErrorHandling(out Action assert, Action a)
		{
			Exception exception = null;
			assert = () => AssertNull(exception);
			return new Thread(() =>
			{
				try
				{
					a();
				}
				catch (Exception e)
				{
					exception = e;
				}
			});
		}

		[GuiTest]
		public void TestFetchForRefresh_SubscriptionInForeground()
		{
			var factory = new BusinessObjectFactory();
			Action assert;
			var collection = new DummyBusinessObjectCollectionWhichAccessesPropertyOnIsMatchingCollectionFilter(factory);
			var subscription = new DataRefreshBus.BusinessObjectCollectionSubscription(collection);
			using (var testSyncContext = SynchronizationContextForTest.Enable())
			using (var foregroundWaitEvent = new AutoResetEvent(false))
			using (var backgroundWaitEvent = new AutoResetEvent(false))
			{
				var thread = RunThreadWithErrorHandling(out assert, () =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var threadTestSyncContext = SynchronizationContextForTest.Enable())
					{
						var dummy = new BusinessObjectFactory().New<DummyBusinessObject>();
						dummy.Z0_Bool = true;
						subscription.FetchForRefresh(new[] { dummy });
						foregroundWaitEvent.Set();

						//IsMatching was initiated
						backgroundWaitEvent.WaitOne(A_NICE_AMOUNT_TO_WAIT);
						testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1));
					}
				});

				thread.Start();
				Assert("Fetch was initiated", foregroundWaitEvent.WaitOne(A_NICE_AMOUNT_TO_WAIT));
				testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1));
				backgroundWaitEvent.Set();
				thread.Join();

				AssertEquals("Precondition", 0, factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
				testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1));
				AssertEquals("Thread queued fetch hint on factory", 0, factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			}
			assert();
		}

		public void TestOnlyRemoveOneSubscription()
		{
			var rowFactory = new RowFactory();
			var dummyRow3 = rowFactory.LoadFromPK(DummyBusinessObject.Schema.TableName, PK);
			var dummy3 = new DummyBusinessObject(new BusinessObjectFactory(), dummyRow3);
			Bus.Subscribe(dummy3);
			AssertEquals("3 Subscriptions", 3, Bus.SubscriptionCount);
			Dummy1.Delete();
			RowFactory1.Save();
			Dummy1.HasChanges = false;
			AssertEquals("Publisher -> RowState is detached", DataRowState.Detached, Dummy1.Row.RowState);
			AssertEquals("Publisher is deleted", true, Dummy1.IsDeleted);
			AssertEquals("Subscriber1 is not deleted", false, Dummy2.IsDeleted);
			AssertEquals("Subscriber2 is not deleted", false, dummy3.IsDeleted);
			Bus.Publish(Dummy1);
			AssertEquals("Publisher should remain deleted", true, Dummy1.IsDeleted);
			AssertEquals("Subscriber1 should be refreshed to deleted", true, Dummy2.IsDeleted);
			AssertEquals("Subscriber2 should be refreshed to deleted", true, dummy3.IsDeleted);

			AssertEquals("Both Subscriber1 and Subscriber2 should be removed from Subscriptions", 1, Bus.Subscriptions.Count);
		}

		public void TestFetchForRefresh_SubscriptionInBackground()
		{
			DataRefreshBus.BusinessObjectCollectionSubscription subscription = null;
			BusinessObjectFactory factory = null;

			Action assert;
			var thread = RunThreadWithErrorHandling(out assert, () =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					factory = new BusinessObjectFactory();
					var collection = new DummyBusinessObjectCollectionWhichAccessesPropertyOnIsMatchingCollectionFilter(factory);
					subscription = new DataRefreshBus.BusinessObjectCollectionSubscription(collection);
				}
			});

			thread.Start();
			thread.Join();

			var dummy = new BusinessObjectFactory().New<DummyBusinessObject>();
			dummy.Z0_Bool = true;
			AssertNoExceptionThrown("Should not add fetch hint for a factory in the background", () => subscription.FetchForRefresh(new[] { dummy }));
			assert();
		}

		class DummyBusinessObjectCollectionWhichAccessesPropertyOnIsMatchingCollectionFilter : NonPersistentBusinessObjectCollection<DummyNonPersistentBusinessObject>, IBusinessObjectFilterFactory
		{
			internal DummyBusinessObjectCollectionWhichAccessesPropertyOnIsMatchingCollectionFilter(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			IBusinessObjectFilter IBusinessObjectFilterFactory.NewBusinessObjectFilter()
			{
				return new DummyPropertyMatchingBusinessObjectFilter();
			}

			class DummyPropertyMatchingBusinessObjectFilter : BusinessObjectFilter
			{
				protected override bool IsMatching(BusinessObject bizObj)
				{
					return ((DummyBusinessObject)bizObj).Z0_Bool;
				}
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				throw new NotImplementedException();
			}
		}

		public void TestPublishSingleObjectAddWithMultipleDependentCollectionsGeneratesNoDBHit()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();

			int dependentCollectionCount = 5;
			DummyWithDependentsBusinessObject[] objectsInFactory1 = new DummyWithDependentsBusinessObject[dependentCollectionCount];
			for (int i = 0; i < dependentCollectionCount; i++)
			{
				objectsInFactory1[i] = factory1.New<DummyWithDependentsBusinessObject>();
				objectsInFactory1[i].Dependents.AddNew();
			}
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			DummyWithDependentsBusinessObject[] objectsInFactory2 = new DummyWithDependentsBusinessObject[dependentCollectionCount];
			for (int i = 0; i < dependentCollectionCount; i++)
			{
				objectsInFactory2[i] = factory2.Load<DummyWithDependentsBusinessObject>(objectsInFactory1[i].PK);
				object dependents = objectsInFactory2[i].Dependents;  // Force lazy create / load
			}

			objectsInFactory1[0].Dependents.AddNew();
			AssertEquals("Precondition", 1, objectsInFactory2[0].Dependents.Count);
			int loadCountBeforeSave = factory2.DatabaseLoadCount;
			factory1.Save();
			AssertEquals("Precondition", 2, factory1.PublishCountForLastSave);
			AssertEquals("Precondition", true, factory1.SubscriptionFiredForLastSave);
			AssertEquals("Factory load count", loadCountBeforeSave, factory2.DatabaseLoadCount);
			AssertEquals("Objects after refresh", 2, objectsInFactory2[0].Dependents.Count);
		}

		public void TestPublishUpdates()
		{
			Dummy1.Z0_Code = "22222";
			RowFactory1.Save();
			Dummy1.HasChanges = false;

			Bus.Publish(Dummy1);

			AssertEquals("Dummy2 Z0_Code", "22222", Dummy2.Z0_Code);
			AssertEquals("Dummy1.HasChanges", false, Dummy1.HasChanges);
			AssertEquals("Dummy2.HasChanges", false, Dummy2.HasChanges);
			AssertEquals("Dummy2.RowState", DataRowState.Unchanged, Dummy2.Row.RowState);
		}

		public void TestPublishDelete()
		{
			RowFactory rowFactory3 = new RowFactory();
			DataRow dummyRow3 = rowFactory3.LoadFromPK(DummyBusinessObject.Schema.TableName, PK);
			DummyBusinessObject dummy3 = new DummyBusinessObject(new BusinessObjectFactory(), dummyRow3);

			Dummy2.Delete();
			RowFactory2.Save();
			Dummy2.HasChanges = false;

			AssertEquals("Dummy1.IsDeleted", false, Dummy1.IsDeleted);
			Bus.Publish(Dummy2);

			AssertEquals("Dummy1.IsDeleted", true, Dummy1.IsDeleted);
			AssertEquals("Dummy1.HasChanges", false, Dummy1.HasChanges);
			AssertEquals("Dummy2.IsDeleted", true, Dummy2.IsDeleted);
			AssertEquals("Dummy2.HasChanges", false, Dummy2.HasChanges);

			//Checking that Dummy1 and Dummy2 are actually removed from the subscriptions.
			dummy3.Z0_Code = "44444";
			AssertNoExceptionThrown(() => Bus.Publish(dummy3));
		}

		public void TestPublishDeleteOnADeletedSubscriberWillDelayAcceptChangesOnIt()
		{
			RowFactory rowFactory3 = new RowFactory();
			DataRow dummyRow3 = rowFactory3.LoadFromPK(DummyBusinessObject.Schema.TableName, PK);
			DummyBusinessObject dummy3 = new DummyBusinessObject(new BusinessObjectFactory(), dummyRow3);

			Dummy2.Delete();
			RowFactory2.Save();

			AssertEquals("Dummy1.IsDeleted", false, Dummy1.IsDeleted);
			Dummy1.Delete();

			Bus.Publish(Dummy2);

			AssertEquals("Dummy1.IsDeleted", true, Dummy1.IsDeleted);
			AssertEquals("Dummy1.RowState", DataRowState.Deleted, Dummy1.Row.RowState);
			AssertEquals("Dummy1.AcceptChangesDelayedUntilJustBeforeSavingToDatabase", true, ((IBusinessObjectInternals)Dummy1).AcceptChangesDelayedUntilJustBeforeSavingToDatabase);

			//publishing on a deleted should not be a problem also.
			Bus.Publish(Dummy2);
		}

		#region Do not reanimate deleted bizo

		public void TestDoNotReanimateDeletedBizo()
		{
			var factory1 = new BusinessObjectFactory();
			var dummy1 = factory1.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Number = 100;
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
			AssertNotNull(dummy2);

			dummy1.Z0_Number = 200;
			factory1.Save();
			AssertEquals(200, dummy2.Z0_Number);

			dummy2.Delete();
			dummy1.Z0_Number = 300;
			factory1.Save();
			Assert("Should not be reanimated", dummy2.IsDeleted);
			AssertNull("Should not be duplicated", factory2.Load<DummyBusinessObject>(dummy1.PK));

			AssertNoExceptionThrown(() => factory2.Save());
		}

		public void TestDoNotReanimateDeletedBizoInCollection()
		{
			var filter = new ZQuery(DummyBizoSchema.Z0_Description, "aoeui");

			var factory1 = new BusinessObjectFactory();
			var collection1 = new DummyBusinessObjectCollection(factory1, filter);
			var dummy1 = collection1.AddNew();
			dummy1.Z0_Description = "aoeui";
			dummy1.Z0_Number = 100;
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var collection2 = new DummyBusinessObjectCollection(factory2, filter) { IsManagedForDataRefresh = true };
			collection2.Load();
			AssertEquals(1, collection2.Count);
			var dummy2 = collection2[0];
			AssertEquals(dummy1.PK, dummy2.PK);

			dummy1.Z0_Number = 200;
			factory1.Save();
			AssertEquals(200, dummy2.Z0_Number);

			collection2.RemoveAndDeleteAll();
			dummy1.Z0_Number = 300;
			factory1.Save();
			Assert("Should not be reanimated", dummy2.IsDeleted);
			AssertNull("Should not be duplicated", factory2.Load<DummyBusinessObject>(dummy1.PK));
			AssertEquals("Should be nothing in collection", 0, collection2.Count);

			AssertNoExceptionThrown(() => factory2.Save());
		}

		public void TestDoNotReanimateDeletedBizoInCollectionWithMatchingFilter()
		{
			var filter = new ZQuery(DummyBizoSchema.Z0_Description, "aoeui");

			var factory1 = new BusinessObjectFactory();
			var collection1 = new DummyCollectionWithMatchingFilter(factory1, filter);
			var dummy1 = collection1.AddNew();
			dummy1.Z0_Description = "aoeui";
			dummy1.Z0_Number = 100;
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var collection2 = new DummyCollectionWithMatchingFilter(factory2, filter) { IsManagedForDataRefresh = true };
			collection2.Load();
			AssertEquals(1, collection2.Count);
			var dummy2 = collection2[0];
			AssertEquals(dummy1.PK, dummy2.PK);

			dummy1.Z0_Number = 200;
			factory1.Save();
			AssertEquals(200, dummy2.Z0_Number);

			collection2.RemoveAndDeleteAll();
			dummy1.Z0_Number = 300;
			factory1.Save();
			Assert("Should not be reanimated", dummy2.IsDeleted);
			AssertNull("Should not be duplicated", factory2.Load<DummyBusinessObject>(dummy1.PK));
			AssertEquals("Should be nothing in collection", 0, collection2.Count);

			AssertNoExceptionThrown(() => factory2.Save());
		}

		class DummyCollectionWithMatchingFilter : DummyBusinessObjectCollection, IBusinessObjectFilterFactory
		{
			public DummyCollectionWithMatchingFilter(BusinessObjectFactory factory, ZQuery filter = null) : base(factory, filter)
			{
				this.filter = new DummyMatchingFilter(filter);
			}

			readonly DummyMatchingFilter filter;

			class DummyMatchingFilter : BusinessObjectFilter
			{
				readonly ZQuery filter;

				public DummyMatchingFilter(ZQuery filter)
				{
					this.filter = filter;
				}

				protected override bool IsMatching(BusinessObject bizObj)
				{
					return bizObj.MatchesFilter(filter);
				}
			}

			public IBusinessObjectFilter NewBusinessObjectFilter()
			{
				return filter;
			}
		}

		#endregion

		#region Delete related children

		public void TestPublishDeleteWithRelatedChildrenInDifferentFactory()
		{
			var factory1 = new BusinessObjectFactory();
			var dummyInFactory1 = factory1.NewWithValidTestData<DummyWithDeleteableDependents>();
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var dummyInFactory2 = factory2.Load<DummyWithDeleteableDependents>(dummyInFactory1.PK);
			var dependent1InFactory2 = dummyInFactory2.SingleDependent;
			var dependent2InFactory2 = dummyInFactory2.Dependents.AddNew();

			dummyInFactory1.Delete();
			factory1.Save();

			Assert("Main object should be deleted by DataRefreshBus", dummyInFactory2.IsDeleted);
			Assert("Related object should be deleted with main by DataRefreshBus", dependent1InFactory2.IsDeleted);
			Assert("Related object should be deleted with main by DataRefreshBus", dependent2InFactory2.IsDeleted);
		}

		class DummyWithDeleteableDependents : DummyBusinessObject
		{
			public DummyWithDeleteableDependents(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public DeleteableDependent SingleDependent
			{
				get
				{
					if (singleDependent == null)
					{
						singleDependent = Factory.New<DeleteableDependent>();
						singleDependent.ZD1_Z0 = PK;
						RegisterEditableChildObject(singleDependent);
					}
					return singleDependent;
				}
			}
			DeleteableDependent singleDependent;

			public DeleteableDependentCollection Dependents
			{
				get
				{
					if (dependents == null)
					{
						dependents = new DeleteableDependentCollection(this, Factory);
						RegisterEditableChildObject(dependents);
					}
					return dependents;
				}
			}
			DeleteableDependentCollection dependents;

			public override void Delete()
			{
				if (singleDependent != null && !singleDependent.IsDeleted)
				{
					singleDependent.Delete();
				}
				Dependents.RemoveAndDeleteAll();
				base.Delete();
			}

			protected override void DeleteForDataRefresh()
			{
				if (singleDependent != null && !singleDependent.IsDeleted && !singleDependent.IsInDatabase)
				{
					((IBusiness)singleDependent).DeleteForDataRefresh();
				}
				foreach (var dependent in Dependents.Where(dependent => !dependent.IsInDatabase).ToArray())
				{
					((IBusiness)dependent).DeleteForDataRefresh();
				}
				base.DeleteForDataRefresh();
			}
		}

		class DeleteableDependent : DummyDependantBusinessObject
		{
			public DeleteableDependent(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		class DeleteableDependentCollection : DependentBusinessObjectCollection<DeleteableDependent, DummyBusinessObject>
		{
			public DeleteableDependentCollection(DummyBusinessObject master, BusinessObjectFactory factory) : base(master, factory) { }
		}

		#endregion

		public void TestUnSubscribe()
		{
			Bus.UnSubscribe(Dummy2);

			Dummy1.Z0_Code = "33333";
			RowFactory1.Save();
			Dummy1.HasChanges = false;

			Bus.Publish(Dummy1);
			AssertEquals("Dummy2.HasChanges", false, Dummy2.HasChanges);
			AssertEquals("Dummy2.Z0_Code", "11111", Dummy2.Z0_Code); // changes not affected
		}

		public void TestChangedObjectsWillAlsoBeUpdatedByBus()
		{
			Dummy2.Z0_Code = "999";

			Dummy1.Z0_Code = "111";
			RowFactory1.Save();
			Bus.Publish(Dummy1);

			AssertEquals("Dummy2.Z0_Code should be updated", "111", Dummy2.Z0_Code);
			AssertEquals("Dummy2.Row.RowState", DataRowState.Unchanged, Dummy2.Row.RowState);
		}

		public void TestUnchangedObjectsWithChangedChildrenCanStillBeUpdated()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject childDummy = factory.New<DummyBusinessObject>();

			Dummy2.RegisterEditableChildObject(childDummy);
			childDummy.Z0_Description = "ChangedChild";

			Dummy1.Z0_Code = "111";
			RowFactory1.Save();
			Bus.Publish(Dummy1);

			AssertEquals("Dummy2.Z0_Code should be updated", "111", Dummy2.Z0_Code);
		}

		public void TestSubscribeCollection()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			DummyBusinessObjectCollection dummyCollection = new DummyBusinessObjectCollection(factory1);
			dummyCollection.Load();

			AssertEquals("CollectionSubscription", 0, Bus.CollectionSubscriptionCount);
			Bus.Subscribe(dummyCollection);
			try
			{
				AssertEquals("CollectionSubscription", 1, Bus.CollectionSubscriptionCount);

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;

				DummyBusinessObject dummy3 = factory2.New<DummyBusinessObject>();
				dummy3.Z0_Code = "333";
				factory2.Save();

				AssertEquals("RowCount", 2, dummyCollection.Count);
				Bus.PublishByTableName(dummy3);
				AssertEquals("RowCount", 3, dummyCollection.Count);
				Assert("Dummy3 should be added to DummyCollection", dummyCollection.Contains(dummy3.PK));
				DummyBusinessObject dummy3Copy = dummyCollection.FindByPK(dummy3.PK) as DummyBusinessObject;
				AssertNotNull(dummy3Copy);
				Assert("BusinessObject added to collection is a clone", dummy3Copy != dummy3);
				AssertEquals("Dummy3Copy.Row.RowState", DataRowState.Unchanged, dummy3Copy.Row.RowState);
				AssertEquals("Dummy3Copy.HasChanges", false, dummy3Copy.HasChanges);
				Bus.Subscribe(dummy3Copy);

				dummy3.Delete();
				factory2.Save();
				Bus.Publish(dummy3);
				AssertEquals("RowCount", 2, dummyCollection.Count);
				Assert("Dummy3 should be removed from DummyCollection", !dummyCollection.Contains(dummy3.PK));
			}
			finally
			{
				Bus.UnSubscribe(dummyCollection);
			}
		}

		[SnailTest]
		[StressTest]
		[UseSnapshotProtection]
		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestSubscribeCollection_MultiThreaded()
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;

			var subscribers = new List<DummyBusinessObjectCollection>();
			var exceptions = new ConcurrentBag<Exception>();
			try
			{
				for (var i = 0; i < 30000; i++)
				{
					var subscriber = new DummyBusinessObjectCollection(factory);

					subscribers.Add(subscriber);
					Bus.Subscribe(subscriber);
				}

				var endTime = DateTime.UtcNow.AddSeconds(60);
				var threads = Enumerable.Range(0, 10).Select(_ => new Thread(() =>
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							var otherFactory = new BusinessObjectFactory();

							while (endTime > DateTime.UtcNow)
							{
								var dummy = otherFactory.New<DummyBusinessObject>();
								otherFactory.Save();
								Bus.PublishByTableName(dummy);
							}
						}
					}
					catch (Exception e)
					{
						exceptions.Add(e);
					}
				})).ToArray();

				foreach (var thread in threads)
				{
					thread.Start();
				}

				var oneSub = subscribers[0];
				while (endTime > DateTime.UtcNow)
				{
					Bus.UnSubscribe(oneSub);
					Bus.Subscribe(oneSub);
				}

				foreach (var thread in threads)
				{
					thread.Join();
				}

				CombineAssertions(() =>
				{
					foreach (var exception in exceptions)
					{
						AssertNull(exception?.ToString(), exception);
					}
				});
			}
			finally
			{
				foreach (var subscriber in subscribers)
				{
					Bus.UnSubscribe(subscriber);
				}
			}
		}

		class DummyBusinessObjectCollectionWithMatchingCollectionFilter : DummyBusinessObjectCollection, IBusinessObjectFilterFactory
		{
			public DummyBusinessObjectCollectionWithMatchingCollectionFilter(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public DummyBusinessObjectCollectionWithMatchingCollectionFilter(BusinessObjectFactory factory, ZQuery filter)
				: base(factory, filter)
			{
			}

			#region IBusinessObjectFilterFactory Members

			public void SetReturnValueForMatchingCollectionFilter(bool value)
			{
				filter.MatchingCollectionFilterReturnValue = value;
			}

			readonly DummySetMatchFilter filter = new DummySetMatchFilter();

			class DummySetMatchFilter : BusinessObjectFilter
			{
				public bool MatchingCollectionFilterReturnValue;

				protected override bool IsMatching(BusinessObject bizObject)
				{
					return MatchingCollectionFilterReturnValue;
				}
			}

			IBusinessObjectFilter IBusinessObjectFilterFactory.NewBusinessObjectFilter()
			{
				return filter;
			}

			#endregion
		}

		#region TestPublishBizosOfDifferentType

		public void TestPublishBizoOfDifferentType()
		{
			var factory1 = new BusinessObjectFactory();

			var collection1 = new DummyBizo1Collection(factory1, new ZQuery(DummyBizoSchema.Z0_Number, 1000)) { IsManagedForDataRefresh = true };
			collection1.Load();
			AssertEquals("Precondition", 0, collection1.Count);

			var factory2 = new BusinessObjectFactory();

			var dummy1InFactory2 = factory2.New<DummyBusinessObject>();
			dummy1InFactory2.Z0_Code = "DU1";
			dummy1InFactory2.Z0_Number = 1000;

			var dummy2InFactory2 = factory2.New<DummyBusinessObject>();
			dummy2InFactory2.Z0_Code = "DU2";
			dummy2InFactory2.Z0_Number = 1000;

			AssertEquals("Should not be loaded yet", 0, collection1.Count);

			factory2.Save();

			AssertEquals("Object should be added by DataRefreshBus", 2, collection1.Count);
			AssertCollectionContains(dummy1InFactory2.PK, collection1.Select(s => s.PK));
		}

		public void TestPublishBizosOfDifferentType()
		{
			var factory1 = new BusinessObjectFactory();

			var collection1 = new DummyBizo1Collection(factory1, new ZQuery(DummyBizoSchema.Z0_Number, 1000)) { IsManagedForDataRefresh = true };
			collection1.Load();
			AssertEquals("Precondition", 0, collection1.Count);

			var collection2 = new DummyBizo2Collection(factory1, new ZQuery(DummyBizoSchema.Z0_Number, 2000)) { IsManagedForDataRefresh = true };
			collection2.Load();
			AssertEquals("Precondition", 0, collection2.Count);

			var factory2 = new BusinessObjectFactory();

			var dummy1InFactory2 = factory2.New<DummyBusinessObject>();
			dummy1InFactory2.Z0_Code = "DU1";
			dummy1InFactory2.Z0_Number = 1000;

			var dummy2InFactory2 = factory2.New<DummyBusinessObject>();
			dummy2InFactory2.Z0_Code = "DU2";
			dummy2InFactory2.Z0_Number = 2000;

			AssertEquals("Should not be loaded yet", 0, collection1.Count);
			AssertEquals("Should not be loaded yet", 0, collection2.Count);

			factory2.Save();

			AssertEquals("Object should be added by DataRefreshBus", 1, collection1.Count);
			AssertEquals(dummy1InFactory2.PK, collection1[0].PK);

			AssertEquals("Object should be added by DataRefreshBus", 1, collection2.Count);
			AssertEquals(dummy2InFactory2.PK, collection2[0].PK);
		}

		public void TestMarkForDelete()
		{
			var factory = new BusinessObjectFactory();
			var bizObjInFactory = factory.New<DummyBizo1>();
			var bizObjDependentInFactory = factory.New<DummyDependantBusinessObjectHasSamePk>();
			bizObjDependentInFactory.MockedPK = bizObjInFactory.PK;
			factory.Save();

			var bus = new DataRefreshBus();
			bus.Subscribe(bizObjInFactory);
			bus.Subscribe(bizObjDependentInFactory);

			var newFacotry = new BusinessObjectFactory();
			var bizObjInNewFacotory = newFacotry.Load<DummyBizo1>(bizObjInFactory.PK);
			bizObjInNewFacotory.Delete();
			bus.Publish(bizObjInNewFacotory);
			Assert("bizObjViewInFactory1 shoule not be marked as delete and removed from subscription as it has a differnet table name.", bus.Subscriptions.Any(s => s.Participant.GetType() == typeof(DummyDependantBusinessObjectHasSamePk)));
		}

		[SingleObjectAroundARow]
		class DummyBizo1 : DummyBusinessObject
		{
			public DummyBizo1(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		class DummyBizo1Collection : BusinessObjectCollection<DummyBizo1>
		{
			public DummyBizo1Collection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter) { }
		}

		class DummyDependantBusinessObjectHasSamePk : DummyDependantBusinessObject
		{
			public DummyDependantBusinessObjectHasSamePk(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ZGuid MockedPK { get; set; } = ZGuid.NewZGuid();

			internal override ZGuid GetPKInternal()
			{
				return MockedPK;
			}
		}

		[SingleObjectAroundARow]
		class DummyBizo2 : DummyBusinessObject
		{
			public DummyBizo2(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		class DummyBizo2Collection : BusinessObjectCollection<DummyBizo2>
		{
			public DummyBizo2Collection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter) { }
		}

		#endregion

		[ExpectNoExceptions]
		public void TestPublishOnATableWithDBOnlyQueryRefreshBinding()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			ZDBOnlySubQuery subquery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			query.AddSubQuery(subquery, JoinCondition.And);
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(factory1, query);
			collection.Load();
			DummyBusinessObject bizO = collection.AddNew();
			bizO.Z0_Code = "BAS";
			bizO.Z0_Date = ZDateTime.BrettsBirthday;
			DummyBusinessObject dummy = factory1.New<DummyBusinessObject>();
			dummy.Z0_Description = "ABC";
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject bizO2 = factory2.Load<DummyBusinessObject>(bizO.PK);
			bizO2.Z0_Code = "XYZ";
			((IBusiness)bizO).ListChanged += new System.ComponentModel.ListChangedEventHandler(delegate
			{ collection.Load(); });
			factory2.Save();
		}

		public void TestSubscribeCollectionThatImplementsIMatchingCollectionFilter()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = true;

			DummyBusinessObjectCollectionWithMatchingCollectionFilter dummyCollection = new DummyBusinessObjectCollectionWithMatchingCollectionFilter(factory1);
			dummyCollection.IsManagedForDataRefresh = true;
			dummyCollection.Load();

			AssertEquals("CollectionSubscription", 0, Bus.CollectionSubscriptionCount);
			Bus.Subscribe(dummyCollection);
			try
			{
				AssertEquals("CollectionSubscription", 1, Bus.CollectionSubscriptionCount);

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = true;

				dummyCollection.SetReturnValueForMatchingCollectionFilter(false);

				DummyBusinessObject dummy3 = factory2.New<DummyBusinessObject>();
				dummy3.Z0_Code = "333";
				factory2.Save();

				AssertEquals("RowCount", 2, dummyCollection.Count);

				dummyCollection.SetReturnValueForMatchingCollectionFilter(true);

				DummyBusinessObject dummy4 = factory2.New<DummyBusinessObject>();
				dummy4.Z0_Code = "444";
				factory2.Save();

				AssertEquals("RowCount", 3, dummyCollection.Count);
				Assert("Dummy4 should be added to DummyCollection", dummyCollection.Contains(dummy4.PK));
				DummyBusinessObject dummy4Copy = dummyCollection.FindByPK(dummy4.PK) as DummyBusinessObject;
				AssertNotNull(dummy4Copy);
				Assert("BusinessObject added to collection is a clone", dummy4Copy != dummy4);
				AssertEquals("Dummy4Copy.Row.RowState", DataRowState.Unchanged, dummy4Copy.Row.RowState);
				AssertEquals("Dummy4Copy.HasChanges", false, dummy4Copy.HasChanges);
				Bus.Subscribe(dummy4Copy);
			}
			finally
			{
				Bus.UnSubscribe(dummyCollection);
			}
		}

		public void TestSubscribingCollectionUsesWeakReference()
		{
			var bus = new DataRefreshBus();
			var @ref = TemporarilySubscribeCollection(bus);
			GC.Collect();

			AssertEquals("IsAlive", false, @ref.IsAlive);
			AssertEquals("CollectionSubscription", 0, bus.CollectionSubscriptionCount);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference TemporarilySubscribeCollection(DataRefreshBus bus)
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			var subscriber = new DummyBusinessObjectCollection(factory);
			subscriber.Load();
			AssertEquals("SubscriptionCount", 0, bus.CollectionSubscriptionCount);
			bus.Subscribe(subscriber);
			AssertEquals("SubscriptionCount", 1, bus.CollectionSubscriptionCount);
			var @ref = new WeakReference(subscriber);
			AssertEquals("Ref.IsAlive", true, @ref.IsAlive);
			return @ref;
		}

		public void TestSubscribingBusinessObjectUsesWeakReference()
		{
			var bus = new DataRefreshBus();
			var @ref = TemporarilySubscribeBusinssObject(bus);
			GC.Collect();

			AssertEquals("Ref.IsAlive", false, @ref.IsAlive);
			AssertEquals("SubscriptionCount", 0, bus.SubscriptionCount);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference TemporarilySubscribeBusinssObject(DataRefreshBus bus)
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			var subscriber = factory.New<DummyBusinessObject>();
			AssertEquals("SubscriptionCount", 0, bus.SubscriptionCount);
			bus.Subscribe(subscriber);
			AssertEquals("SubscriptionCount", 1, bus.SubscriptionCount);
			var @ref = new WeakReference(subscriber);
			AssertEquals("Ref.IsAlive", true, @ref.IsAlive);
			return @ref;
		}

		#region class DummyBankAccount

		internal class DummyBankAccount : DummyBusinessObject
		{
			public DummyBankAccount(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override DummyBizoValidation GetNewValidation()
			{
				return new DummyBankAccountValidation(this);
			}
		}

		internal class DummyBankAccountValidation : DummyBizoValidation
		{
			public DummyBankAccountValidation(DummyBusinessObject parent)
				: base(parent)
			{
			}

			protected override void CheckZ0_Code()
			{
				base.CheckZ0_Code();

				//somehow causing the same object to be subscribed again to the bus. to emulate real life
				//bug where saving bankaccount changes updates the bank account on an open chequebook which
				//in turn loads the same bank account for validation during the publish.   henry.
				BusinessObjectFactory factory1 = new BusinessObjectFactory();
				factory1.Load(Parent.GetType(), Parent.PK);
			}
		}

		#endregion

		public void TestPublishCausesAnotherObjectAddedToSameSubscriptionList()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			DummyBusinessObject dummy1 = factory1.New<DummyBusinessObject>();
			dummy1.Z0_Code = "tst";
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBankAccount account1 = factory2.Load<DummyBankAccount>(dummy1.PK);

			dummy1.Z0_Code = "123";
			factory1.Save();

			AssertEquals("Code", "123", account1.Z0_Code);
		}

		public void TestPublishWillNotUpdateBusinessObjectWrapperAroundTheSameRow()
		{
			Bus.UnSubscribe(Dummy2);

			Dummy1.Z0_Code = "22222";
			RowFactory1.Save();
			Dummy1.HasChanges = false;

			DummyBusinessObject dummy3 = new DummyBusinessObjectThatThrowsExceptionDuringCopying(Dummy1.Factory, DummyRow1);
			Bus.Subscribe(dummy3);

			Bus.Publish(Dummy1);

			AssertEquals("HasChanges", false, dummy3.HasChanges);
		}

		class DummyBusinessObjectThatThrowsExceptionDuringCopying : DummyBusinessObject
		{
			public DummyBusinessObjectThatThrowsExceptionDuringCopying(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void CheckCanCopyPersistentValuesFrom()
			{
				throw new Exception("Cannot CopyPersistentValue for this object during test.");
			}
		}

		public void TestSameBusinessObjectCannotBeSubscribedMultipleTimes()
		{
			int oldCount = Bus.SubscriptionCount;
			Bus.Subscribe(Dummy1);
			Bus.Subscribe(Dummy1);
			AssertEquals("SubscriptionCount", oldCount, Bus.SubscriptionCount);

			Bus.UnSubscribe(Dummy1);
			AssertEquals("SubscriptionCount", oldCount - 1, Bus.SubscriptionCount);
		}

		public void TestSameCollectionCannotBeSubscribedMultipleTimes()
		{
			int oldCount = Bus.CollectionSubscriptionCount;

			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			Bus.Subscribe(collection);

			AssertEquals("Count", oldCount + 1, Bus.CollectionSubscriptionCount);

			Bus.Subscribe(collection);
			AssertEquals("Count", oldCount + 1, Bus.CollectionSubscriptionCount);

			Bus.UnSubscribe(collection);
			AssertEquals("Count", oldCount, Bus.CollectionSubscriptionCount);
		}

		public void TestIsRefreshing()
		{
			Dummy1.Z0_Code = "22222";
			RowFactory1.Save();
			Dummy1.HasChanges = false;

			Dummy2.UpdatedByDataRefresh += new EventHandler(CheckThatDummy2IsUpdatingByDataRefreshBus);

			Bus.SetIsRefreshing(Dummy1, true);
			AssertEquals("Dummy2.IsUpdatingByDataRefreshBus", false, Bus.IsRefreshing(Dummy2));
			Bus.Publish(Dummy1);
			AssertEquals("Dummy2.IsUpdatingByDataRefreshBus", false, Bus.IsRefreshing(Dummy2));
		}

		void CheckThatDummy2IsUpdatingByDataRefreshBus(object sender, EventArgs e)
		{
			AssertEquals("Dummy2.IsUpdatingByDataRefreshBus", true, Bus.IsRefreshing(Dummy2));
		}

		public void TestSetIsRefreshing()
		{
			AssertEquals("Dummy1.IsUpdatingByDataRefreshBus", false, Bus.IsRefreshing(Dummy1));
			AssertEquals("Dummy2.IsUpdatingByDataRefreshBus", false, Bus.IsRefreshing(Dummy2));

			Bus.SetIsRefreshing(Dummy1, true);
			AssertEquals("Dummy1.IsUpdatingByDataRefreshBus", true, Bus.IsRefreshing(Dummy1));
			AssertEquals("Dummy2.IsUpdatingByDataRefreshBus", false, Bus.IsRefreshing(Dummy2));

			Bus.SetIsRefreshing(Dummy2, true);
			AssertEquals("Dummy1.IsUpdatingByDataRefreshBus", true, Bus.IsRefreshing(Dummy1));
			AssertEquals("Dummy2.IsUpdatingByDataRefreshBus", true, Bus.IsRefreshing(Dummy2));

			Bus.SetIsRefreshing(Dummy1, false);
			AssertEquals("Dummy1.IsUpdatingByDataRefreshBus", false, Bus.IsRefreshing(Dummy1));
			AssertEquals("Dummy2.IsUpdatingByDataRefreshBus", true, Bus.IsRefreshing(Dummy2));

			Bus.SetIsRefreshing(Dummy2, false);
			AssertEquals("Dummy1.IsUpdatingByDataRefreshBus", false, Bus.IsRefreshing(Dummy1));
			AssertEquals("Dummy2.IsUpdatingByDataRefreshBus", false, Bus.IsRefreshing(Dummy2));
		}

		public void TestPublishUsesCollectionFetchHints()
		{
			ZGuid pk1 = new ZGuid(Guid.NewGuid());
			ZGuid pk2 = new ZGuid(Guid.NewGuid());
			ZGuid pk3 = new ZGuid(Guid.NewGuid());
			ZGuid pk4 = new ZGuid(Guid.NewGuid());

			RowFactory rowFactory1 = new RowFactory();
			RowFactory rowFactory2 = new RowFactory();

			DummyTableCreator.AddDummyBusinessObjectsToDB(pk1.ToGuid(), pk2.ToGuid());

			DataRow dummyRow1 = rowFactory1.LoadFromPK(DummyBusinessObject.Schema.TableName, pk1);
			DataRow dummyRow2 = rowFactory1.LoadFromPK(DummyBusinessObject.Schema.TableName, pk2);
			DataRow dummyRow5 = rowFactory2.LoadFromPK(DummyBusinessObject.Schema.TableName, pk1);
			DataRow dummyRow6 = rowFactory2.LoadFromPK(DummyBusinessObject.Schema.TableName, pk2);

			BusinessObjectFactory businessObjectFactory1 = new BusinessObjectFactory();
			BusinessObjectFactory businessObjectFactory2 = new BusinessObjectFactory();

			DummyBusinessObject dummy1 = new DummyBusinessObject(businessObjectFactory1, dummyRow1);
			DummyBusinessObject dummy2 = new DummyBusinessObject(businessObjectFactory1, dummyRow2);
			DummyBusinessObject dummy5 = new DummyBusinessObject(businessObjectFactory2, dummyRow5);
			DummyBusinessObject dummy6 = new DummyBusinessObject(businessObjectFactory2, dummyRow6);

			dummy1.Z0_Code = "11111";
			dummy2.Z0_Code = "22222";
			rowFactory1.Save();

			DummyBusinessObjectCollection dummyCollection1 = new DummyBusinessObjectCollection(businessObjectFactory1);
			DummyBusinessObjectCollection dummyCollection2 = new DummyBusinessObjectCollection(businessObjectFactory2);

			dummyCollection1.Add(dummy1);
			dummyCollection1.Add(dummy2);
			dummyCollection2.Load();

			AssertEquals("dummyCollection2.IsLoaded", true, dummyCollection2.IsLoaded);
			AssertEquals("dummyCollection1.Count", 2, dummyCollection1.Count);
			AssertEquals("dummyCollection2.Count", 4, dummyCollection2.Count);  //including 2 DummyBusinessObjects from Setup

			var tableHitCount = businessObjectFactory2.GetTableHitCount(DummyBusinessObject.Schema.TableName);
			AssertEquals("TableHitCount should equal 1", 1, tableHitCount);

			DummyTableCreator.AddDummyBusinessObjectsToDB(pk3.ToGuid(), pk4.ToGuid());

			DataRow dummyRow3 = rowFactory1.LoadFromPK(DummyBusinessObject.Schema.TableName, pk3);
			DataRow dummyRow4 = rowFactory1.LoadFromPK(DummyBusinessObject.Schema.TableName, pk4);

			DummyBusinessObject dummy3 = new DummyBusinessObject(businessObjectFactory1, dummyRow3);
			DummyBusinessObject dummy4 = new DummyBusinessObject(businessObjectFactory1, dummyRow4);

			dummy3.Z0_Code = "33333";
			dummy4.Z0_Code = "44444";
			rowFactory1.Save();

			dummyCollection1.Add(dummy3);
			dummyCollection1.Add(dummy4);

			DataRefreshManager manager = new DataRefreshManager();
			manager.StartManaging(dummy1);
			manager.StartManaging(dummy2);
			manager.StartManaging(dummy3);
			manager.StartManaging(dummy4);
			manager.StartManaging(dummy5);
			manager.StartManaging(dummy6);
			manager.StartManaging(dummyCollection1);
			manager.StartManaging(dummyCollection2);

			manager.Publish(new[] { dummy1, dummy2, dummy3, dummy4 });

			AssertEquals("11111", dummy5.Z0_Code);
			AssertEquals("22222", dummy6.Z0_Code);
			AssertEquals("dummyCollection1.Count", 4, dummyCollection1.Count);
			AssertEquals("dummyCollection2.Count", 6, dummyCollection2.Count);

			tableHitCount = businessObjectFactory2.GetTableHitCount(DummyBusinessObject.Schema.TableName);
			AssertEquals("TableHitCount should equal 2", 1, tableHitCount);
		}

		public void TestPublishingBizObjUpdatesPrefetchedDataRows()
		{
			ZGuid pK2 = new ZGuid(Guid.NewGuid());
			DummyTableCreator.AddDummyBusinessObjectsToDB(pK2.ToGuid(), Guid.NewGuid());

			DataRow dummyRow3 = RowFactory1.LoadFromPK(DummyBusinessObject.Schema.TableName, pK2);
			DataRow dummyRow4 = RowFactory2.LoadFromPK(DummyBusinessObject.Schema.TableName, pK2);
			DummyBusinessObject dummy3 = new DummyBusinessObject(DummyFactory1, dummyRow3);
			dummy3.Z0_Code = "12486";
			dummy3.HasChanges = true;
			RowFactory1.Save();

			DataRefreshManager manager = new DataRefreshManager();
			manager.StartManaging(dummy3);
			manager.StartManaging(Dummy2);
			manager.Publish(new[] { dummy3 });

			AssertEquals("dummy3 updated dummyRow4", dummy3.Z0_Code, dummyRow4[DummyBusinessObject.Schema.Z0_Code]);
		}

		public void TestPublishingBizObjDeletesPrefetchedDataRows()
		{
			ZGuid pK2 = new ZGuid(Guid.NewGuid());
			DummyTableCreator.AddDummyBusinessObjectsToDB(pK2.ToGuid(), Guid.NewGuid());

			DataRow dummyRow3 = RowFactory1.LoadFromPK(DummyBusinessObject.Schema.TableName, pK2);
			DataRow dummyRow4 = RowFactory2.LoadFromPK(DummyBusinessObject.Schema.TableName, pK2);
			DummyBusinessObject dummy3 = new DummyBusinessObject(DummyFactory1, dummyRow3);
			dummy3.Delete();
			dummy3.HasChanges = true;
			AssertEquals("dummy3 is deleted", true, dummy3.IsDeleted);

			DataRow dummyRow4Copy = RowFactory2.GetRow(DummyBusinessObject.Schema.TableName, pK2);
			AssertEquals("dummyRow4 exists in RowFactory2", true, dummyRow4Copy != null);

			RowFactory1.Save();

			DataRefreshManager manager = new DataRefreshManager();
			manager.StartManaging(dummy3);
			manager.StartManaging(Dummy2);
			manager.Publish(new[] { dummy3 });

			dummyRow4Copy = RowFactory2.GetRow(DummyBusinessObject.Schema.TableName, pK2);

			AssertEquals("dummyRow4 no longer exists in RowFactory2", true,
				dummyRow4Copy == null || dummyRow4Copy.RowState == DataRowState.Deleted || dummyRow4Copy.RowState == DataRowState.Detached);
		}

		public void TestPublishingBizObjDeletesPrefetchedDataRowInMultipleCollections()
		{
			var dummyCollection1 = new DummyBusinessObjectCollectionWithMatchingCollectionFilter(DummyFactory2);
			dummyCollection1.IsManagedForDataRefresh = true;
			dummyCollection1.Load();

			var dummyCollection2 = new DummyBusinessObjectCollectionWithMatchingCollectionFilter(DummyFactory2);
			dummyCollection2.IsManagedForDataRefresh = true;
			dummyCollection2.Load();

			Bus.Subscribe(dummyCollection1);
			Bus.Subscribe(dummyCollection2);

			AssertEquals("RowCount", 2, dummyCollection1.Count);
			AssertEquals("RowCount", 2, dummyCollection1.Count);

			dummyCollection1.SetReturnValueForMatchingCollectionFilter(false);
			dummyCollection2.SetReturnValueForMatchingCollectionFilter(false);

			DummyFactory2.ClearQueryCache();

			var dummyPK = new ZGuid(Guid.NewGuid());
			DummyTableCreator.AddDummyBusinessObjectsToDB(dummyPK.ToGuid(), Guid.NewGuid());

			var dummyRowInPublisherFactory = RowFactory1.LoadFromPK(DummyBusinessObject.Schema.TableName, dummyPK);
			var dummyRowInSubscriberFactory = RowFactory2.LoadFromPK(DummyBusinessObject.Schema.TableName, dummyPK);
			var dummyPublishedBO = new DummyBusinessObject(DummyFactory1, dummyRowInPublisherFactory);

			dummyCollection1.SetReturnValueForMatchingCollectionFilter(true);
			dummyCollection2.SetReturnValueForMatchingCollectionFilter(true);

			var addedFromDataRefresh = false;
			DummyBizo1 anotherBOAroundRow = null;
			void DummyCollectionOnAddedFromDataRefresh(object sender, EventArgs e)
			{
				var collection = (BusinessObjectCollection)sender;
				if (!addedFromDataRefresh)
				{
					addedFromDataRefresh = true;
				}
				else
				{
					var factory = collection.Factory;
					anotherBOAroundRow = new DummyBizo1(factory, dummyRowInSubscriberFactory);
					dummyPublishedBO.Delete();
				}
			}

			DummyBusinessObject addedBO = null;
			void DummyCollectionOnCountChanged(object sender, CollectionCountChangedEventArgs args)
			{
				if (!addedFromDataRefresh && args.ItemAdded)
				{
					addedBO = (DummyBusinessObject)args.BizObject;
				}
			}

			dummyCollection1.OnAddedFromDataRefresh += DummyCollectionOnAddedFromDataRefresh;
			dummyCollection1.CountChanged += DummyCollectionOnCountChanged;
			dummyCollection2.OnAddedFromDataRefresh += DummyCollectionOnAddedFromDataRefresh;
			dummyCollection2.CountChanged += DummyCollectionOnCountChanged;

			RowFactory1.Save();

			var manager = new DataRefreshManager();
			manager.Publish(new[] { dummyPublishedBO });

			AssertNotNull("BO is added by data refresh bus", addedBO);
			Assert("BO is deleted after being added by data refresh bus", addedBO.IsDeleted);
			AssertEquals(DataRowState.Deleted, addedBO.Row.RowState);
			AssertNotNull(anotherBOAroundRow);
			Assert(anotherBOAroundRow.IsDeleted);
			AssertEquals(DataRowState.Deleted, anotherBOAroundRow.Row.RowState);
			AssertEquals("RowCount remains 2", 2, dummyCollection1.Count);
			AssertEquals("RowCount remains 2", 2, dummyCollection2.Count);
		}

		[ExpectNoExceptions]
		public void TestPublishOnObjectInAnotherDependentCollection_ShouldSwallowException()
		{
			var subscriptionProvider = new Mock<ISubscriptionsBySubjectProvider>();
			Bus.SubscriptionProvider = subscriptionProvider.Object;

			subscriptionProvider.Setup(m => m.GetSubscriptions(It.IsAny<BusinessObjectFactory>(), It.IsAny<ISubscriptionSubject>(), It.IsAny<IEnumerable<object>>(), It.IsAny<Func<ISubscription, bool>>())).Throws(new CannotAddToCollectionException(""));

			Bus.Publish(new BusinessObjectFactory().New<DummyBusinessObject>());
		}

		#region Test Can Databus Refresh

		public void TestCanApplyDataRefreshWhenSubscriberDeletedPublisherDeleted()
		{
			int oldSubscriptionCount = Bus.SubscriptionCount;

			var rowFactory = new RowFactory();
			var row = rowFactory.LoadFromPK(DummyBusinessObject.Schema.TableName, PK);
			var dummySubscriberToSkipRefresh = new DummyObjectToSkipDataRefresh(new BusinessObjectFactory(), row, DataRefreshAction.UpdateDeletedSubscriberWhenPublisherDeleted);
			Bus.Subscribe(dummySubscriberToSkipRefresh);
			AssertEquals("Subscription Count increase by 1", 1, Bus.SubscriptionCount - oldSubscriptionCount);

			Dummy2.Delete();
			AssertEquals("Subscriber -> RowState is deleted", DataRowState.Deleted, Dummy2.Row.RowState);
			AssertEquals("Subscriber is deleted", true, Dummy2.IsDeleted);
			dummySubscriberToSkipRefresh.Delete();
			AssertEquals("Subscriber that can skip refresh -> RowState is deleted", DataRowState.Deleted, dummySubscriberToSkipRefresh.Row.RowState);
			AssertEquals("Subscriber that can skip refresh is deleted", true, dummySubscriberToSkipRefresh.IsDeleted);

			Dummy1.Delete();
			RowFactory1.Save();
			Dummy1.HasChanges = false;
			AssertEquals("Publisher -> RowState is detached", DataRowState.Detached, Dummy1.Row.RowState);
			AssertEquals("Publisher is deleted", true, Dummy1.IsDeleted);

			AssertEquals("Publisher is not refreshed", false, ((IBusinessObjectInternals)Dummy1).AcceptChangesDelayedUntilJustBeforeSavingToDatabase);
			AssertEquals("Subscriber is not refreshed", false, ((IBusinessObjectInternals)Dummy2).AcceptChangesDelayedUntilJustBeforeSavingToDatabase);
			AssertEquals("Subscriber that can skip refresh is not refreshed", false, ((IBusinessObjectInternals)dummySubscriberToSkipRefresh).AcceptChangesDelayedUntilJustBeforeSavingToDatabase);

			Bus.Publish(Dummy1);

			AssertEquals("Publisher is not refreshed", false, ((IBusinessObjectInternals)Dummy1).AcceptChangesDelayedUntilJustBeforeSavingToDatabase);
			AssertEquals("Subscriber should be refreshed", true, ((IBusinessObjectInternals)Dummy2).AcceptChangesDelayedUntilJustBeforeSavingToDatabase);
			AssertEquals("Subscriber that can skip refresh should not be refreshed", false, ((IBusinessObjectInternals)dummySubscriberToSkipRefresh).AcceptChangesDelayedUntilJustBeforeSavingToDatabase);

			AssertEquals("Publisher -> RowState should remain detached", DataRowState.Detached, Dummy1.Row.RowState);
			AssertEquals("Subscriber -> RowState should remain deleted", DataRowState.Deleted, Dummy2.Row.RowState);
			AssertEquals("Subscriber can skip refresh -> RowState should remain deleted", DataRowState.Deleted, dummySubscriberToSkipRefresh.Row.RowState);

			AssertEquals("Publisher should remain deteled", true, Dummy1.IsDeleted);
			AssertEquals("Subscriber should remain deteled", true, Dummy2.IsDeleted);
			AssertEquals("Subscriber that can skip refresh should remain deteled", true, dummySubscriberToSkipRefresh.IsDeleted);
		}

		public void TestCanApplyDataRefreshWhenSubscriberDeletedPublisherUpdated()
		{
			int oldSubscriptionCount = Bus.SubscriptionCount;

			var rowFactory = new RowFactory();
			var row = rowFactory.LoadFromPK(DummyBusinessObject.Schema.TableName, PK);
			var dummySubscriberToSkipRefresh = new DummyObjectToSkipDataRefresh(new BusinessObjectFactory(), row, DataRefreshAction.UpdateDeletedSubscriberWhenPublisherUpdated);
			Bus.Subscribe(dummySubscriberToSkipRefresh);
			AssertEquals("Subscription Count increase by 1", 1, Bus.SubscriptionCount - oldSubscriptionCount);

			Dummy2.Delete();
			AssertEquals("Subscriber is deleted", true, Dummy2.IsDeleted);
			dummySubscriberToSkipRefresh.Delete();
			AssertEquals("Subscriber that can not apply refresh is deleted", true, dummySubscriberToSkipRefresh.IsDeleted);

			Dummy1.Z0_Code = "22222";
			RowFactory1.Save();
			Dummy1.HasChanges = false;
			AssertEquals("Publisher is not deleted", false, Dummy1.IsDeleted);

			AssertEquals("Publisher -> Z0_Code", "22222", Dummy1.Z0_Code);
			AssertEquals("Subscriber -> Z0_Code", "11111", DummyRow2["Z0_Code", DataRowVersion.Original]);
			AssertEquals("Subscriber that can skip refresh -> Z0_Code", "11111", row["Z0_Code", DataRowVersion.Original]);

			Bus.Publish(Dummy1);

			AssertEquals("Pubilsher -> Z0_Code is not changes", "22222", Dummy1.Z0_Code);
			AssertEquals("Subscriber -> Z0_Code should be refreshed", "22222", DummyRow2["Z0_Code", DataRowVersion.Original]);  // should be '11111'
			AssertEquals("Subscriber that can skip refresh -> Z0_Code should not be refreshed", "11111", row["Z0_Code", DataRowVersion.Original]);

			AssertEquals("Pubilsher should remain non-deleted", false, Dummy1.IsDeleted);
			AssertEquals("Subscriber should remain deleted", true, Dummy2.IsDeleted);
			AssertEquals("Subscriber that can skip refresh should remain deleted", true, dummySubscriberToSkipRefresh.IsDeleted);
		}

		public void TestCanApplyDataRefreshWhenSubscriberNonDeletedPublisherUpdated()
		{
			int oldSubscriptionCount = Bus.SubscriptionCount;

			var rowFactory = new RowFactory();
			var row = rowFactory.LoadFromPK(DummyBusinessObject.Schema.TableName, PK);
			var dummySubscriberToSkipRefresh = new DummyObjectToSkipDataRefresh(new BusinessObjectFactory(), row, DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated);
			Bus.Subscribe(dummySubscriberToSkipRefresh);
			AssertEquals("Subscription Count increase by 1", 1, Bus.SubscriptionCount - oldSubscriptionCount);

			Dummy1.Z0_Code = "22222";
			RowFactory1.Save();
			Dummy1.HasChanges = false;

			AssertEquals("Publisher -> Z0_Code", "22222", Dummy1.Z0_Code);
			AssertEquals("Subscriber -> Z0_Code", "11111", Dummy2.Z0_Code);
			AssertEquals("Subscriber can skip refresh -> Z0_Code", "11111", dummySubscriberToSkipRefresh.Z0_Code);

			Bus.Publish(Dummy1);

			AssertEquals("Publisher -> Z0_Code is not changed", "22222", Dummy1.Z0_Code);
			AssertEquals("Subscriber -> Z0_Code should be refreshed", "22222", Dummy2.Z0_Code);
			AssertEquals("Subscriber can skip refresh -> Z0_Code should not be refreshed", "11111", dummySubscriberToSkipRefresh.Z0_Code);
		}

		public void TestCanApplyDataRefreshWhenSubscriberNonDeletedPublisherDeleted()
		{
			int oldSubscriptionCount = Bus.SubscriptionCount;

			var rowFactory = new RowFactory();
			var row = rowFactory.LoadFromPK(DummyBusinessObject.Schema.TableName, PK);
			var dummySubscriberToSkipRefresh = new DummyObjectToSkipDataRefresh(new BusinessObjectFactory(), row, DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherDeleted);
			Bus.Subscribe(dummySubscriberToSkipRefresh);
			AssertEquals("Subscription Count increase by 1", 1, Bus.SubscriptionCount - oldSubscriptionCount);

			int subscriptionCountBeforeRefresh = Bus.SubscriptionCount;

			Dummy1.Delete();
			RowFactory1.Save();
			Dummy1.HasChanges = false;
			AssertEquals("Publisher -> RowState is detached", DataRowState.Detached, Dummy1.Row.RowState);

			AssertEquals("Publisher is deteled", true, Dummy1.IsDeleted);
			AssertEquals("Subscriber is non-deteled", false, Dummy2.IsDeleted);
			AssertEquals("Subscriber can not apply refresh is non-deteled", false, dummySubscriberToSkipRefresh.IsDeleted);

			Bus.Publish(Dummy1);

			AssertEquals("Publisher should remain deteled", true, Dummy1.IsDeleted);
			AssertEquals("Subscriber should be refreshed to deteled", true, Dummy2.IsDeleted);
			AssertEquals("Subscriber that can skip refresh should remain non-deteled", false, dummySubscriberToSkipRefresh.IsDeleted);

			AssertEquals("Publisher -> HasChanges should be false", false, Dummy1.HasChanges);
			AssertEquals("Subscriber -> HasChanges should be false", false, Dummy2.HasChanges);
			AssertEquals("Subscriber can skip refresh -> HasChanges should be false", false, dummySubscriberToSkipRefresh.HasChanges);
		}

		#endregion

		#region Test Order

		public void TestGetSortedSubscriptions()
		{
			var factory = new BusinessObjectFactory();
			var bus = new DataRefreshBus();
			bus.EnableOverrideGetHashCode_ForTest = true;

			var random = new Random();
			var set = new SubscriptionCollection.SubscriptionSet();
			set.Add("A", new SubscriptionForTest(factory, new DummyObjectB(random.Next())));
			set.Add("A", new SubscriptionForTest(factory, new DummyObjectSubClassA(random.Next())));

			var result = set.GetSubscriptions("A").ToList();
			result.Sort(bus.SortByType);
			AssertEquals(2, result.Count);
			AssertEquals(typeof(DummyObjectSubClassA), result[0].Participant.GetType());
			AssertEquals(typeof(DummyObjectB), result[1].Participant.GetType());
		}

		public void TestCollectionOrder()
		{
			var factory = new BusinessObjectFactory();
			var bus = new DataRefreshBus();
			bus.EnableOverrideGetHashCode_ForTest = true;

			/*
				A and B have no inheritance, A value is smaller than B. Expect order A -> B
				A and B have no inheritance, A value is bigger than B. Expect order B -> A
			*/
			var set = new SubscriptionCollection.SubscriptionSet();
			set.Add("Test", new SubscriptionForTest(factory, new DummyObjectA(1)));
			set.Add("Test", new SubscriptionForTest(factory, new DummyObjectB(2)));
			var result = set.GetSubscriptions("Test").ToList();
			result.Sort(bus.SortByType);
			Assert(!result[0].Participant.GetType().IsSubclassOf(result[1].Participant.GetType()));
			Assert(!result[1].Participant.GetType().IsSubclassOf(result[0].Participant.GetType()));
			AssertEquals(typeof(DummyObjectA), result[0].Participant.GetType());
			AssertEquals(typeof(DummyObjectB), result[1].Participant.GetType());

			set = new SubscriptionCollection.SubscriptionSet();
			set.Add("Test", new SubscriptionForTest(factory, new DummyObjectA(2)));
			set.Add("Test", new SubscriptionForTest(factory, new DummyObjectB(1)));
			result = set.GetSubscriptions("Test").ToList();
			result.Sort(bus.SortByType);
			Assert(!result[0].Participant.GetType().IsSubclassOf(result[1].Participant.GetType()));
			Assert(!result[1].Participant.GetType().IsSubclassOf(result[0].Participant.GetType()));
			AssertEquals(typeof(DummyObjectB), result[0].Participant.GetType());
			AssertEquals(typeof(DummyObjectA), result[1].Participant.GetType());

			/*
				A is a sub class of B, A value is smaller than B. Expect order A -> B
				A is a sub class of B, A value is bigger than B. Expect order A -> B
			*/
			set = new SubscriptionCollection.SubscriptionSet();
			set.Add("Test", new SubscriptionForTest(factory, new DummyObjectSubClassA(1)));
			set.Add("Test", new SubscriptionForTest(factory, new DummyObjectB(2)));
			result = set.GetSubscriptions("Test").ToList();
			result.Sort(bus.SortByType);
			Assert(result[0].Participant.GetType().IsSubclassOf(result[1].Participant.GetType()));
			Assert(!result[1].Participant.GetType().IsSubclassOf(result[0].Participant.GetType()));
			AssertEquals(typeof(DummyObjectSubClassA), result[0].Participant.GetType());
			AssertEquals(typeof(DummyObjectB), result[1].Participant.GetType());

			set = new SubscriptionCollection.SubscriptionSet();
			set.Add("Test", new SubscriptionForTest(factory, new DummyObjectSubClassA(2)));
			set.Add("Test", new SubscriptionForTest(factory, new DummyObjectB(1)));
			result = set.GetSubscriptions("Test").ToList();
			result.Sort(bus.SortByType);
			Assert(result[0].Participant.GetType().IsSubclassOf(result[1].Participant.GetType()));
			Assert(!result[1].Participant.GetType().IsSubclassOf(result[0].Participant.GetType()));
			AssertEquals(typeof(DummyObjectSubClassA), result[0].Participant.GetType());
			AssertEquals(typeof(DummyObjectB), result[1].Participant.GetType());

			/*
				B is a sub class of A, A value is smaller than B. Expect order B -> A
				B is a sub class of A, A value is bigger than B. Expect order B -> A
			*/
			set = new SubscriptionCollection.SubscriptionSet();
			set.Add("Test", new SubscriptionForTest(factory, new DummyObjectA(1)));
			set.Add("Test", new SubscriptionForTest(factory, new DummyObjectSubClassB(2)));
			result = set.GetSubscriptions("Test").ToList();
			result.Sort(bus.SortByType);
			Assert(result[0].Participant.GetType().IsSubclassOf(result[1].Participant.GetType()));
			Assert(!result[1].Participant.GetType().IsSubclassOf(result[0].Participant.GetType()));
			AssertEquals(typeof(DummyObjectSubClassB), result[0].Participant.GetType());
			AssertEquals(typeof(DummyObjectA), result[1].Participant.GetType());

			set = new SubscriptionCollection.SubscriptionSet();
			set.Add("Test", new SubscriptionForTest(factory, new DummyObjectA(2)));
			set.Add("Test", new SubscriptionForTest(factory, new DummyObjectSubClassB(1)));
			result = set.GetSubscriptions("Test").ToList();
			result.Sort(bus.SortByType);
			Assert(result[0].Participant.GetType().IsSubclassOf(result[1].Participant.GetType()));
			Assert(!result[1].Participant.GetType().IsSubclassOf(result[0].Participant.GetType()));
			AssertEquals(typeof(DummyObjectSubClassB), result[0].Participant.GetType());
			AssertEquals(typeof(DummyObjectA), result[1].Participant.GetType());
		}

		class SubscriptionForTest : DataRefreshBus.Subscription
		{
			public SubscriptionForTest(BusinessObjectFactory factory, object participant)
				: base(factory, participant)
			{
			}

			internal override void DoAction(IEnumerable<BusinessObject> publishedObjects)
			{
				OnSubscriptionUpdate(false);
			}
		}

		class DummyObjectA
		{
			public DummyObjectA(int i)
			{
				this.i = i;
			}

			public readonly int i;

			public override int GetHashCode()
			{
				return i;
			}
		}

		class DummyObjectB
		{
			public DummyObjectB(int i)
			{
				this.i = i;
			}

			public readonly int i;

			public override int GetHashCode()
			{
				return i;
			}
		}

		class DummyObjectSubClassA : DummyObjectB
		{
			public DummyObjectSubClassA(int i)
				: base(i)
			{
			}
		}

		class DummyObjectSubClassB : DummyObjectA
		{
			public DummyObjectSubClassB(int i)
				: base(i)
			{
			}
		}

		#endregion

		#region TestPublishItemsAreAddedToCorrectCollection

		public void TestPublishItemsAreAddedToCorrectCollection()
		{
			var factory1 = new BusinessObjectFactory { RefreshEnabled = true };

			var collectionT1 = new DummyT1Collection(factory1);
			collectionT1.Load();
			factory1.RefreshManager.StartManaging(collectionT1);
			AssertEquals(0, collectionT1.Count);

			var collectionT2 = new DummyT2Collection(factory1);
			collectionT2.Load();
			factory1.RefreshManager.StartManaging(collectionT2);
			AssertEquals(0, collectionT2.Count);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = true };

			var dummyT1 = factory2.NewWithValidTestData<DummyT1>();
			dummyT1.Z0_Code = "T12";
			factory2.Save();

			AssertEquals(1, collectionT1.Count);
			AssertEquals(dummyT1.PK, collectionT1[0].PK);
			AssertEquals(0, collectionT2.Count);

			var dummyT2 = factory2.NewWithValidTestData<DummyT2>();
			dummyT2.Z0_Code = "T12";
			factory2.Save();

			AssertEquals(1, collectionT1.Count);
			AssertEquals(dummyT1.PK, collectionT1[0].PK);
			AssertEquals(1, collectionT2.Count);
			AssertEquals(dummyT2.PK, collectionT2[0].PK);
		}

		class DummyT1 : DummyBusinessObject
		{
			public DummyT1(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		class DummyT1Collection : BusinessObjectCollection<DummyT1>
		{
			public DummyT1Collection(BusinessObjectFactory factory) : base(factory, new ZQuery(DummyBizoSchema.Z0_Code, "T12")) { }
		}

		class DummyT2 : DummyBusinessObject
		{
			public DummyT2(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		class DummyT2Collection : BusinessObjectCollection<DummyT2>
		{
			public DummyT2Collection(BusinessObjectFactory factory) : base(factory, new ZQuery(DummyBizoSchema.Z0_Code, "T12")) { }
		}

		#endregion

		#region Implementation

		readonly ZGuid PK = new ZGuid(Guid.NewGuid());
		RowFactory RowFactory1;
		DataRow DummyRow1;
		DummyBusinessObject Dummy1;
		BusinessObjectFactory DummyFactory1;
		RowFactory RowFactory2;
		DataRow DummyRow2;
		DummyBusinessObject Dummy2;
		BusinessObjectFactory DummyFactory2;
		DataRefreshBus Bus;

		protected override void SetUp()
		{
			base.SetUp();

			DummyFactory1 = new BusinessObjectFactory();
			DummyFactory2 = new BusinessObjectFactory();
			RowFactory1 = DummyFactory1.RowFactory;
			RowFactory2 = DummyFactory2.RowFactory;
			Bus = new DataRefreshBus();

			AssertEquals("SubscriptionCount", 0, Bus.SubscriptionCount);

			DummyTableCreator.AddDummyBusinessObjectsToDB(PK.ToGuid(), Guid.NewGuid());

			DummyRow1 = RowFactory1.LoadFromPK(DummyBusinessObject.Schema.TableName, PK);
			Dummy1 = new DummyBusinessObject(DummyFactory1, DummyRow1);
			Dummy1.Z0_Code = "11111";
			RowFactory1.Save();
			Dummy1.HasChanges = false;

			DummyRow2 = RowFactory2.LoadFromPK(DummyBusinessObject.Schema.TableName, PK);
			Dummy2 = new DummyBusinessObject(DummyFactory2, DummyRow2);
			Dummy2.HasChanges = false;

			Bus.Subscribe(Dummy1);
			AssertEquals("SubscriptionCount", 1, Bus.SubscriptionCount);

			Bus.Subscribe(Dummy2);
			AssertEquals("SubscriptionCount", 2, Bus.SubscriptionCount);
			AssertEquals("CollectionSubscription", 0, Bus.CollectionSubscriptionCount);

			AssertEquals("Z0_Code", Dummy1.Z0_Code, Dummy2.Z0_Code);
			AssertEquals("Dummy1 RowState", DataRowState.Unchanged, Dummy1.Row.RowState);
			AssertEquals("Dummy2 RowState", DataRowState.Unchanged, Dummy2.Row.RowState);
			AssertEquals("Dummy1 HasChanges", false, Dummy1.HasChanges);
			AssertEquals("Dummy2 HasChanges", false, Dummy2.HasChanges);
		}

		public class DummyObjectToSkipDataRefresh : DummyBusinessObject, ICanApplyDataRefresh
		{
			public DummyObjectToSkipDataRefresh(BusinessObjectFactory factory, DataRow row, DataRefreshAction actionsToSkip) : base(factory, row)
			{
				this.actionsToSkip = actionsToSkip;
			}

			public bool CanApplyDataRefresh(DataRefreshAction action, BusinessObject publisher)
			{
				return !actionsToSkip.HasFlag(action);
			}

			readonly DataRefreshAction actionsToSkip;
		}

		const int A_NICE_AMOUNT_TO_WAIT = 1000;

		#endregion
	}
}
