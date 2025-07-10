using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectCollectionTest : TestCaseWithDummy
	{
		class ListChangedSuspendableCollection : BusinessObjectCollection
		{
			public ListChangedSuspendableCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public new IDisposable SuspendListChanged()
			{
				return base.SuspendListChanged();
			}
		}

		[DeveloperOnlyTest]
		public void TestHasChangesPerformance_Get()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			for (var i = 0; i < 1000000; i++)
			{
				_ = Collection.HasChanges;
			}
			AssertLessThan(stopWatch.ElapsedMilliseconds, 500);
		}

		[DeveloperOnlyTest]
		public void TestHasChangesPerformance_Set()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			for (var i = 0; i < 1000000; i++)
			{
				Collection.HasChanges = true;
			}
			AssertLessThan(stopWatch.ElapsedMilliseconds, 500);
		}

		public void TestIsRefreshingByDataRefreshBusAlsoConsidersIsUpdatingByDataRefreshBus()
		{
			var collection = new MockBusinessObjectCollectionForDataRefreshTest(Factory);
			collection.IsUpdatingByDataRefreshBusExposed = false;
			AssertEquals(false, collection.IsRefreshingByDataRefreshBus);
			collection.IsUpdatingByDataRefreshBusExposed = true;
			AssertEquals(true, collection.IsRefreshingByDataRefreshBus);
		}

		public void TestMultiThreadedAccessToEnumeration()
		{
			var collection = new TestBusinessObjectCollectionWithFetchFromLocalCacheOnly(Factory);
			for (var i = 0; i < 10; i++)
			{
				collection.AddNew();
			}

			var endTime = DateTime.Now + TimeSpan.FromSeconds(15);
			Exception thrownException = null;
			void ThreadMethod()
			{
				try
				{
					while (thrownException == null && DateTime.Now < endTime)
					{
						collection.Select(bo => bo).ToArray(); // Force Iteration
					}
				}
				catch (Exception ex)
				{
					thrownException = ex;
				}
			}

			var t1 = new Thread(ThreadMethod);
			var t2 = new Thread(ThreadMethod);

			t1.Start();
			t2.Start();

			t1.Join();
			t2.Join();

			AssertNull("We should not crash when iterating a collection in an unsafe manner", thrownException);
			AssertContains("The thread sentry should have warned us about cross thread access", "Attempted to access an object owned by another thread. Please call RelinquishThreadOwnership() from the thread that owns the object, and TakeThreadOwnership() on the the new thread before attempting to use the object.", ErrorReporter.LastMessageReported);
			AssertContains("typeOfElements:CargoWise.EntityFramework.Testing.DummyBusinessObject", "type:CargoWise.EntityFramework.Testing.BusinessObjectCollectionTest+TestBusinessObjectCollectionWithFetchFromLocalCacheOnly", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		class MockBusinessObjectCollectionForDataRefreshTest : BusinessObjectCollection
		{
			public MockBusinessObjectCollectionForDataRefreshTest(BusinessObjectFactory factory)
				: base(factory)
			{ }

			public bool IsUpdatingByDataRefreshBusExposed
			{
				get { return IsUpdatingByDataRefreshBus; }
				set { IsUpdatingByDataRefreshBus = value; }
			}
		}

		[ExpectNoExceptions]
		public void TestAddDeletedObject()
		{
			DummyBusinessObject element1 = Factory.New<DummyBusinessObject>();
			element1.Delete();
			Collection.Add(element1);
			Collection.Factory.Save();
		}

		public void TestOrderBy()
		{
			var a = Collection.AddNew().Z0_Code = "A";
			var c = Collection.AddNew().Z0_Code = "C";
			var b = Collection.AddNew().Z0_Code = "B";

			var orderedCollection = Collection.OrderBy(x => x.Z0_Code);
			AssertContainsExactElementsInAnyOrder("Result of return value", [a, b, c], orderedCollection.Select(x => x.Z0_Code));
		}

		public void TestSelect()
		{
			Collection.AddNew().Z0_Code = "A";
			Collection.AddNew().Z0_Code = "B";

			Assert(Collection.Select(x => x.Z0_Code.ToString()).SequenceEqual(new[] { "A", "B" }));
		}

		public void TestWhere()
		{
			Collection.AddNew().Z0_Code = "A";
			Collection.AddNew().Z0_Code = "B";
			AssertEquals(1, Collection.Where(x => x.Z0_Code == "B").Count());
		}

		public void TestFindWithPredicate()
		{
			DummyBusinessObject element1 = Factory.New<DummyBusinessObject>();
			element1.Z0_Code = "AAA";

			DummyBusinessObject element2 = Factory.New<DummyBusinessObject>();
			element2.Z0_Code = "BBB";

			DummyBusinessObject element3 = Factory.New<DummyBusinessObject>();
			element3.Z0_Code = "AAA";

			Factory.Save();

			TestBusinessObjectCollectionWithFetchFromLocalCacheOnly collectionInNewFactory = new TestBusinessObjectCollectionWithFetchFromLocalCacheOnly(Factory);
			collectionInNewFactory.Load();

			List<DummyBusinessObject> findResult = new List<DummyBusinessObject>(collectionInNewFactory.Find(element => element.Z0_Code == "BBB"));
			AssertEquals(1, findResult.Count);
			AssertEquals(element2.PK, findResult[0].PK);

			findResult = new List<DummyBusinessObject>(collectionInNewFactory.Find(element => element.Z0_Code == "AAA"));
			AssertEquals(2, findResult.Count);
			Assert(findResult[0].PK == element1.PK || findResult[1].PK == element1.PK);
			Assert(findResult[0].PK != element2.PK && findResult[1].PK != element2.PK);
			Assert(findResult[0].PK == element3.PK || findResult[1].PK == element3.PK);
		}

		public void TestSuspendListChanged()
		{
			ListChangedSuspendableCollection collection = new ListChangedSuspendableCollection(Factory);
			((IBindingList)collection).ListChanged += new ListChangedEventHandler(BusinessObjectCollectionTest_ListChanged);
			AssertEquals(0, listChangedCount);
			collection.Add(Factory.New<DummyBusinessObject>());
			AssertEquals(1, listChangedCount);
			collection.Add(Factory.New<DummyBusinessObject>());
			AssertEquals(2, listChangedCount);

			using (collection.SuspendListChanged())
			{
				collection.RemoveAll();
				collection.Add(Factory.New<DummyBusinessObject>());
				collection.Add(Factory.New<DummyBusinessObject>());

				AssertEquals("Should not raise any list changed events while suspended", 2, listChangedCount);
			}

			AssertEquals("Should raise one list changed event when the suspension has finished, because something changed while the suspension was in effect.", 3, listChangedCount);

			collection.SuspendListChanged().Dispose();
			AssertEquals("Should not raise any list changed events because nothing changed while it was suspended", 3, listChangedCount);
		}

		void BusinessObjectCollectionTest_ListChanged(object sender, ListChangedEventArgs e)
		{
			listChangedCount++;
		}

		int listChangedCount;

		public void TestLightValidation()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.LightValidationEnabled = true;

				DummyBusinessObject dummy1 = Collection.AddNew();
				DummyBusinessObject dummy2 = Collection.AddNew();
				DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
				dummy2.RegisterEditableChildObject(dummy3);

				AssertEquals(true, dummy1.ShouldValidateOnSave);
				AssertEquals(true, dummy2.ShouldValidateOnSave);
				AssertEquals(true, dummy3.ShouldValidateOnSave);

				Collection.RunPreSaveValidation();
				AssertEquals(false, dummy1.ShouldValidateOnSave);
				AssertEquals(false, dummy2.ShouldValidateOnSave);
				AssertEquals(false, dummy3.ShouldValidateOnSave);

				Collection.MarkAsNeedingValidation();
				AssertEquals(true, dummy1.ShouldValidateOnSave);
				AssertEquals(true, dummy2.ShouldValidateOnSave);
				AssertEquals(false, dummy3.ShouldValidateOnSave);

				Collection.MarkAsNeedingValidationIncludingChildren();
				AssertEquals(true, dummy1.ShouldValidateOnSave);
				AssertEquals(true, dummy2.ShouldValidateOnSave);
				AssertEquals(true, dummy3.ShouldValidateOnSave);
			}
		}

		public void TestListChanged()
		{
			List<ListChangedType> listChangeTypes = new List<ListChangedType>();
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject dummy0 = collection.AddNew();
			((IBindingList)collection).ListChanged += delegate(object sender, ListChangedEventArgs e)
			{ listChangeTypes.Add(e.ListChangedType); };

			AssertContainsExactElementsInAnyOrder("Should not be fired yet",
					Array.Empty<ListChangedType>(),
					listChangeTypes);

			DummyBusinessObject dummy1 = collection.AddNew();
			AssertContainsExactElementsInAnyOrder("AddNew",
					new ListChangedType[] { ListChangedType.ItemAdded },
					listChangeTypes);

			listChangeTypes.Clear();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			collection.Add(dummy2);
			AssertContainsExactElementsInAnyOrder("Add",
					new ListChangedType[] { ListChangedType.ItemAdded },
					listChangeTypes);

			listChangeTypes.Clear();
			DummyBusinessObject dummy3 = (DummyBusinessObject)((IBindingList)collection).AddNew();
			AssertContainsExactElementsInAnyOrder("IBindingList.AddNew",
					new ListChangedType[] { ListChangedType.ItemAdded },
					listChangeTypes);

			listChangeTypes.Clear();
			((ICancelAddNew)collection).CancelNew(((IList)collection).IndexOf(dummy3));
			AssertContainsExactElementsInAnyOrder("IEditableObject.CancelEdit",
					new ListChangedType[] { ListChangedType.ItemDeleted },
					listChangeTypes);

			listChangeTypes.Clear();
			collection.Remove(dummy1);
			AssertContainsExactElementsInAnyOrder("Remove",
					new ListChangedType[] { ListChangedType.ItemDeleted },
					listChangeTypes);

			listChangeTypes.Clear();
			collection.RemoveAndDelete(dummy2);
			AssertContainsExactElementsInAnyOrder("RemoveAndDelete",
					// Should only be ListChangedType.ItemDeleted but the ManyToManyBusinessObjectCollection
					// needs RemoveAndDelete to do the suspend thing while deleting the pivot.
					// We can't drop the ListChangedType.ItemDeleted because the grid seems to ignore
					// ListChangedType.Reset and so does not remove the offending row (but throws an exception
					// when it trys to access the now missing row).
					new ListChangedType[] { ListChangedType.Reset, ListChangedType.ItemDeleted },
					listChangeTypes);

			dummy0 = collection.AddNew();
			dummy1 = collection.AddNew();
			dummy2 = collection.AddNew();
			dummy3 = collection.AddNew();
			listChangeTypes.Clear();
			collection.RemoveAndDeleteAll();
			AssertContainsExactElementsInAnyOrder("RemoveAndDeleteAll",
					new ListChangedType[] { ListChangedType.Reset },
					listChangeTypes);

			dummy0 = collection.AddNew();
			dummy1 = collection.AddNew();
			dummy2 = collection.AddNew();
			dummy3 = collection.AddNew();
			listChangeTypes.Clear();
			collection.RemoveAll();
			AssertContainsExactElementsInAnyOrder("RemoveAll",
					new ListChangedType[] { ListChangedType.Reset },
					listChangeTypes);
		}

		public void TestchangesNotRaisedBecauseListChangeSuspendedDoesNotOverflow()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.changesNotRaisedBecauseListChangeSuspended = int.MaxValue;
			using (collection.SuspendListChanged())
			{
				AssertNoExceptionThrown(() =>
				{
					collection.AddNew();
				});
			}
		}

		public void TestCompleteFilterLoadSmallBlobs()
		{
			var savingFactory = new BusinessObjectFactory();
			var dummy = savingFactory.New<DummyBusinessObject>();
			dummy.Z0_Description = "MATCH";
			savingFactory.Save();

			var collection = new DummyBusinessObjectCollection(Factory);
			var filter = new ZQuery(DummyBizoSchema.Z0_Description, "MATCH");
			filter.LoadSmallBlobs = 0;
			collection.Load(filter);

			Assert(LazyLoading.LoadRequired(collection[0].Row[DummyBizoSchema.Constants.Z0_VarCharMax]));
		}

		public void TestGetTypeDeciderContext()
		{
			AssertNull(Collection.GetTypeDeciderContext());
		}

		#region Type of Elements

		public void TestTypeofElementsFromPK()
		{
			DummyWithOverriddenTypeOfElementsCollection coll = new DummyWithOverriddenTypeOfElementsCollection(Factory);
			coll.Load();

			AssertEquals(typeof(DummyBusinessObject), coll.GetTypeOfElementsFromPK(coll.Bizo1.PK));
			AssertEquals(typeof(DummyBusinessObjectForRemoveAndDeleteAll), coll.GetTypeOfElementsFromPK(coll.Bizo2.PK));
			AssertEquals(typeof(DummyBusinessObject), coll.GetTypeOfElementsFromPK(ZGuid.Empty));
			AssertEquals(typeof(DummyBusinessObject), coll.TypeOfElements);

			coll.RemoveAll();

			coll.AddFromDatabase(coll.Bizo1.PK);
			coll.AddFromDatabase(coll.Bizo2.PK);

			AssertEquals(typeof(DummyBusinessObject), coll[0].GetType());
			AssertEquals(typeof(DummyBusinessObjectForRemoveAndDeleteAll), coll[1].GetType());
		}

		class DummyWithOverriddenTypeOfElementsCollection : DummyBusinessObjectCollection
		{
			public DummyWithOverriddenTypeOfElementsCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public DummyBusinessObject Bizo1;
			public DummyBusinessObjectForRemoveAndDeleteAll Bizo2;

			public override void Load()
			{
				Bizo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
				Bizo1.Z0_AnotherNumber = 10;

				Bizo2 = Factory.NewWithValidTestData<DummyBusinessObjectForRemoveAndDeleteAll>();
				Bizo2.Z0_AnotherNumber = 20;

				Factory.Save();

				Add(Bizo1);
				Add(Bizo2);
			}

			public override Type GetTypeOfElementsFromPK(ZGuid pK)
			{
				DummyBusinessObject bizo = Factory.Load<DummyBusinessObject>(pK);
				if (bizo != null && bizo.Z0_AnotherNumber == 20)
				{
					return typeof(DummyBusinessObjectForRemoveAndDeleteAll);
				}
				return typeof(DummyBusinessObject);
			}
		}

		#endregion

		#region TestAddFromDataRefresh

		public void TestAddFromDataRefresh()
		{
			bool? addedByDataRefresh = null;

			DummyBusinessObject bizObj = Factory.New<DummyBusinessObject>();

			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			collection.CountChanged += delegate(object sender, CollectionCountChangedEventArgs e)
			{
				addedByDataRefresh = (bool)typeof(BusinessObjectCollection).GetField("IsUpdatingByDataRefreshBus", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(collection);
			};

			AssertEquals("precondition: ", null, addedByDataRefresh);

			collection.Add(bizObj);
			AssertEquals("Standard add should be unaffected by add", false, addedByDataRefresh);

			addedByDataRefresh = null;
			collection.RemoveAll();
			AssertEquals("Standard add should be unaffected by remove", false, addedByDataRefresh);

			collection.AddFromDataRefresh(new[] { bizObj });
			AssertEquals("IsUpdatingByDataRefreshBus should be true when adding for data refresh", true, addedByDataRefresh);
		}

		public void TestOnAddFromDataRefresh()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			bizObj.Z0_Code = "TST";

			var additionalFilter = new ZQuery(DummyBizoSchema.Z0_Code, "TST");
			var collection = new DummyBusinessObjectCollection(Factory, additionalFilter);

			bool addedByDataRefresh = false;
			collection.OnAddedFromDataRefresh += (object sender, EventArgs e) => addedByDataRefresh = true;

			collection.Add(bizObj);
			AssertEquals("event is not triggered by Standard add", false, addedByDataRefresh);

			collection.RemoveAll();
			AssertEquals("event is not triggered by remove", false, addedByDataRefresh);

			collection.AddFromDataRefresh(new[] { bizObj });
			AssertEquals("event is triggered when adding for data refresh", true, addedByDataRefresh);
			AssertEquals("added", 1, collection.Count);

			collection.RemoveAll();
			addedByDataRefresh = false;
			bizObj.Z0_Code = "XXX";

			collection.AddFromDataRefresh(new[] { bizObj });
			AssertEquals("event is not triggered when bizo fails filter (not added)", false, addedByDataRefresh);
			AssertEquals("not added", 0, collection.Count);
		}

		#endregion

		#region TestOnLoadedOverride

		public void TestOnLoadedOverride()
		{
			OnLoadedTestCollection collection = new OnLoadedTestCollection(Factory);
			AssertEquals("collection.OnLoadedCalled", 0, collection.OnLoadedCalled);
			collection.Load();
			AssertEquals("collection.OnLoadedCalled", 1, collection.OnLoadedCalled);
			collection.Load(new ZQuery());
			AssertEquals("collection.OnLoadedCalled", 2, collection.OnLoadedCalled);
			collection.Load(new ZQuery());
			AssertEquals("collection.OnLoadedCalled", 3, collection.OnLoadedCalled);
		}

		class OnLoadedTestCollection : DummyBusinessObjectCollection
		{
			public OnLoadedTestCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
			protected override void OnLoaded()
			{
				base.OnLoaded();
				OnLoadedCalled++;
			}
			public int OnLoadedCalled;
		}

		#endregion

		#region MaximumRowsForValidation

		public void TestMaximumRowsForValidation()
		{
			DummyBusinessObject dummy1 = Collection.AddNew();
			DummyBusinessObject dummy2 = Collection.AddNew();
			DummyBusinessObject dummy3 = Collection.AddNew();
			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
			AssertNoRowErrors(dummy3);
			Collection.MaxCountValidationEnable(1);
			AssertNoRowErrors(dummy1);
			AssertHasRowError(dummy2, "You are only allowed a maximum of 1 DummyBizo here.");
			AssertHasRowError(dummy3, "You are only allowed a maximum of 1 DummyBizo here.");
			Collection.MaxCountValidationEnable(2);
			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
			AssertHasRowError(dummy3, "You are only allowed a maximum of 2 DummyBizos here.");
			Collection.MaxCountValidationEnable(3);
			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
			AssertNoRowErrors(dummy3);
		}

		public void TestMaximumRowsForValidation_ConcurrentModification_Addition()
		{
			var hits = 0;
			var dummy1 = Collection.AddNew();
			var dummy2 = Collection.AddNew();
			var dummy3 = Collection.AddNew();
			DummyBusinessObject dummy4 = null;
			dummy3.NotificationsChanged += (o, e) =>
			{
				hits += 1;
				if (hits == 1)
				{
					dummy4 = Collection.AddNew();
				}
			};

			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
			AssertNoRowErrors(dummy3);

			Collection.MaxCountValidationEnable(1);
			AssertNoRowErrors(dummy1);
			AssertHasRowError(dummy2, "You are only allowed a maximum of 1 DummyBizo here.");
			AssertHasRowError(dummy3, "You are only allowed a maximum of 1 DummyBizo here.");

			Collection.MaxCountValidationEnable(2);
			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
			AssertHasRowError(dummy3, "You are only allowed a maximum of 2 DummyBizos here.");
			AssertHasRowError(dummy4, "You are only allowed a maximum of 2 DummyBizos here.");

			Collection.MaxCountValidationEnable(3);
			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
			AssertNoRowErrors(dummy3);
			AssertHasRowError(dummy4, "You are only allowed a maximum of 3 DummyBizos here.");

			Collection.MaxCountValidationEnable(4);
			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
			AssertNoRowErrors(dummy3);
			AssertNoRowErrors(dummy4);
		}

		public void TestMaximumRowsForValidation_ConcurrentModification_RemoveAndDelete()
		{
			var hits = 0;
			var dummy1 = Collection.AddNew();
			var dummy2 = Collection.AddNew();
			var dummy3 = Collection.AddNew();
			var dummy4 = Collection.AddNew();
			dummy3.NotificationsChanged += (o, e) =>
			{
				hits += 1;
				if (hits == 1)
				{
					Collection.RemoveAndDelete(dummy4);
				}
			};

			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
			AssertNoRowErrors(dummy3);

			Collection.MaxCountValidationEnable(1);
			AssertNoRowErrors(dummy1);
			AssertHasRowError(dummy2, "You are only allowed a maximum of 1 DummyBizo here.");
			AssertHasRowError(dummy3, "You are only allowed a maximum of 1 DummyBizo here.");

			Collection.MaxCountValidationEnable(2);
			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
			AssertHasRowError(dummy3, "You are only allowed a maximum of 2 DummyBizos here.");

			Collection.MaxCountValidationEnable(3);
			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
			AssertNoRowErrors(dummy3);

			Collection.MaxCountValidationEnable(4);
			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
			AssertNoRowErrors(dummy3);
		}

		public void TestMaximumRowsForValidation_ConcurrentModification_RemoveAndDeleteNested()
		{
			var hits = 0;
			var dummy1 = Collection.AddNew();
			var dummy2 = Collection.AddNew();
			var dummy3 = Collection.AddNew();
			var dummy4 = Collection.AddNew();
			dummy2.NotificationsChanged += (o, e) =>
			{
				hits += 1;
				if (hits == 2)
				{
					Collection.RemoveAndDelete(dummy3);
				}
			};

			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
			AssertNoRowErrors(dummy3);

			Collection.MaxCountValidationEnable(1);
			AssertNoRowErrors(dummy1);
			AssertHasRowError(dummy2, "You are only allowed a maximum of 1 DummyBizo here.");
			AssertHasRowError(dummy3, "You are only allowed a maximum of 1 DummyBizo here.");

			Collection.RemoveAndDelete(dummy4);
			AssertNoRowErrors(dummy1);
			AssertHasRowError(dummy2, "You are only allowed a maximum of 1 DummyBizo here.");

			Collection.MaxCountValidationEnable(2);
			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
			ErrorReporter.Clear();
		}

		public void TestAddingARowTriggersMaximumRowsValidation()
		{
			Collection.MaxCountValidationEnable(1);
			DummyBusinessObject dummy1 = Collection.AddNew();
			AssertNoRowErrors(dummy1);
			DummyBusinessObject dummy2 = Collection.AddNew();
			AssertNoRowErrors(dummy1);
			AssertHasRowError(dummy2, "You are only allowed a maximum of 1 DummyBizo here.");
		}

		public void TestAddingARowTriggersMaximumRowsMessageErrorValidation()
		{
			var msgError = "You are only allowed a maximum of 1 DummyBizo here.";

			Collection.MaxCountValidationWithMessageErrorEnable(1, msgError);
			DummyBusinessObject dummy1 = Collection.AddNew();
			AssertNoRowMessageError(dummy1, msgError);
			DummyBusinessObject dummy2 = Collection.AddNew();
			AssertNoRowMessageError(dummy1, msgError);
			AssertHasRowMessageError(dummy2, msgError);
		}

		public void TestRemovingARowReValidatesTheWholeCollection()
		{
			Collection.MaxCountValidationEnable(1);
			DummyBusinessObject dummy1 = Collection.AddNew();
			DummyBusinessObject dummy2 = Collection.AddNew();
			AssertNoRowErrors("Precondition", dummy1);
			AssertHasRowError("Precondition", dummy2, "You are only allowed a maximum of 1 DummyBizo here.");
			Collection.Remove(dummy1);
			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
		}

		public void TestMaxCountEnableAndDisable()
		{
			Collection.MaxCountValidationEnable(1);
			DummyBusinessObject dummy1 = Collection.AddNew();
			DummyBusinessObject dummy2 = Collection.AddNew();
			AssertNoRowErrors("Precondition", dummy1);
			AssertHasRowError("Precondition", dummy2, "You are only allowed a maximum of 1 DummyBizo here.");

			Collection.MaxCountValidationDisable();
			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);

			Collection.MaxCountValidationEnable(1);
			AssertNoRowErrors(dummy1);
			AssertHasRowError(dummy2, "You are only allowed a maximum of 1 DummyBizo here.");
		}

		public void TestMaxRowsValidationHappensOnPreSave()
		{
			Collection.MaxCountValidationEnable(1);
			DummyBusinessObject dummy1 = Collection.AddNew();
			DummyBusinessObject dummy2 = Collection.AddNew();
			dummy1.ClearAllNotifications();
			dummy2.ClearAllNotifications();
			AssertNoRowErrors("Precondition", dummy1);
			AssertNoRowErrors("Precondition", dummy2);
			Collection.RunPreSaveValidation();
			AssertNoRowErrors("Precondition", dummy1);
			AssertHasRowError("Precondition", dummy2, "You are only allowed a maximum of 1 DummyBizo here.");
		}

		public void TestOtherRowErrorsAreNotLost()
		{
			Collection.MaxCountValidationEnable(1);
			DummyBusinessObject dummy1 = Collection.AddNew();
			DummyBusinessObject dummy2 = Collection.AddNew();
			AssertNoRowErrors("Precondition", dummy1);
			AssertHasRowError("Precondition", dummy2, "You are only allowed a maximum of 1 DummyBizo here.");
			dummy2.AddRowError("Other Peoples Damn Row Errors");
			Collection.Remove(dummy1);
			AssertHasRowError(dummy2, "Other Peoples Damn Row Errors");
			Assert("We should only have one row error left. ie: Some Other Damn Person's Row Error", dummy2.RowNotifications.Count() == 1);
		}

		public void TestMaxCountMessageOverride()
		{
			DummyBusinessObject dummy1 = Collection.AddNew();
			DummyBusinessObject dummy2 = Collection.AddNew();
			Collection.MaxCountValidationEnable(1, "SometimesYouLaughSometimesYouCry");
			AssertNoRowErrors(dummy1);
			AssertHasRowError(dummy2, "SometimesYouLaughSometimesYouCry");
			Collection.MaxCountValidationDisable();
			AssertNoRowErrors(dummy1);
			AssertNoRowErrors(dummy2);
		}

		#endregion

		#region Additional Filter Not Met Error Message

		public void TestGetNotificationWhenAdditionalFilterNotMet()
		{
			DummyBusinessObjectCollectionForAdditionalFilterNotMetError coll = new DummyBusinessObjectCollectionForAdditionalFilterNotMetError(Factory);

			DummyBusinessObject dummy1 = coll.AddNew();
			dummy1.Z0_Code = "ABCD";

			DummyBusinessObject dummy2 = coll.AddNew();
			dummy2.Z0_Code = "AAAA";

			string error = coll.GetAllNotificationsWhenAdditionalFilterNotMet(dummy1);
			AssertEquals("Dummy code cannot be ABCD", error);

			error = coll.GetAllNotificationsWhenAdditionalFilterNotMet(dummy2);
			AssertEquals("This DummyBizo cannot be chosen here. Please choose another DummyBizo.", error);

			error = coll.GetAllNotificationsWhenAdditionalFilterNotMet(null);
			AssertEquals("This record cannot be chosen here. Please choose another record.", error);

			coll.SetOverrideNotificationWhenAdditionalFilterNotMet("This is the overidden error");

			error = coll.GetAllNotificationsWhenAdditionalFilterNotMet(dummy1);
			AssertEquals("This is the overidden error", error);
		}

		class DummyBusinessObjectCollectionForAdditionalFilterNotMetError : DummyBusinessObjectCollection
		{
			public DummyBusinessObjectCollectionForAdditionalFilterNotMetError(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override ZQuery CreateAdditionalFilter()
			{
				return new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, "ABCD");
			}

			protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
			{
				base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
				DummyBusinessObject dummy = (DummyBusinessObject)selectedBusinessObject;
				if (dummy != null && dummy.Z0_Code == "ABCD")
				{
					errors.Add("Dummy code cannot be ABCD");
				}
			}
		}

		#endregion

		#region Last Change Number

		public void TestLastChangeNumber()
		{
			IBusiness collection = Dummy.Collection;
			AssertEquals("LastChangeNumber", 0, collection.LastChangeNumber);
			collection.AddNew();
			AssertEquals("LastChangeNumber", 0, collection.LastChangeNumber);
			Dummy.Collection[0].Z0_AnotherDecimal = 34234m;
			AssertEquals("LastChangeNumber", 0, collection.LastChangeNumber);
			Dummy.Collection.RemoveAndDeleteAll();
			Assert("LastChangeNumber > 0", collection.LastChangeNumber > 0);
		}

		#endregion

		#region Name

		class DummyCollectionWithOverridenName : DummyBusinessObjectCollection
		{
			public DummyCollectionWithOverridenName(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override ZString HumanReadableNameCore
			{
				get { return "NewName"; }
			}
		}

		class NullTableCollection : DummyBusinessObjectCollection
		{
			public NullTableCollection()
				: base(new BusinessObjectFactory())
			{
			}

			protected override ZDataTable Table
			{
				get { return null; }
			}
		}

		public void TestName()
		{
			AssertEquals("Test overriding", "NewName", new DummyCollectionWithOverridenName(Factory).HumanReadableName);
			AssertEquals("Test resource string", "DummyBizo", new DummyBusinessObjectCollection(Factory).HumanReadableName);
			AssertEquals("With null", "record", new NullTableCollection().HumanReadableName);
		}

		#endregion

		#region TestSortChanged

		public void TestSortChangedEvent()
		{
			SortChangedCount = 0;
			Collection.SortChanged += new EventHandler(Collection_SortChanged);

			Collection.Sort(DummyBusinessObject.Schema.Z0_Code, ListSortDirection.Ascending);
			AssertEquals("SortChangedCount", 1, SortChangedCount);

			Collection.Sort(new SortInfo(DummyBusinessObject.Schema.Z0_Code, ListSortDirection.Descending));
			AssertEquals("SortChangedCount", 2, SortChangedCount);

			Collection.Sort(new DummyComparer());
			AssertEquals("SortChangedCount", 3, SortChangedCount);
		}

		int SortChangedCount;
		void Collection_SortChanged(object sender, EventArgs e)
		{
			SortChangedCount++;
		}

		#endregion

		#region IBindingListView

		public void TestAdvancedSortingAfterCollectionLoaded()
		{
			Collection.RemoveAll();
			var dummy0 = Collection.AddNew();
			dummy0.Z0_Code = "XX1";
			dummy0.Z0_Number = 1;

			var dummy1 = Collection.AddNew();
			dummy1.Z0_Code = "XX1";
			dummy1.Z0_Number = 2;

			var dummy3 = Collection.AddNew();
			dummy3.Z0_Code = "XX3";
			dummy3.Z0_Number = 1;

			var dummy4 = Collection.AddNew();
			dummy4.Z0_Code = "XX3";
			dummy4.Z0_Number = 2;

			Factory.Save();

			var sorts = new ListSortDescription[]
			{
				new ListSortDescription(GetPropertyDescriptor(DummyBusinessObject.Schema.Z0_Number), ListSortDirection.Ascending),
				new ListSortDescription(GetPropertyDescriptor(DummyBusinessObject.Schema.Z0_Code), ListSortDirection.Descending)
			};
			var sortCollection = new ListSortDescriptionCollection(sorts);

			var listView = Collection as IBindingListView;
			listView.ApplySort(sortCollection);

			AssertEquals("XX3", Collection[0].Z0_Code);
			AssertEquals("XX3", Collection[1].Z0_Code);
			AssertEquals("XX1", Collection[2].Z0_Code);
			AssertEquals("XX1", Collection[3].Z0_Code);

			AssertEquals(1, Collection[0].Z0_Number);
			AssertEquals(2, Collection[1].Z0_Number);
			AssertEquals(1, Collection[2].Z0_Number);
			AssertEquals(2, Collection[3].Z0_Number);

			var savedSorts = listView.SortDescriptions;
			AssertEquals(2, savedSorts.Count);
			AssertEquals(DummyBusinessObject.Schema.Z0_Number, savedSorts[0].PropertyDescriptor.Name);
			AssertEquals(ListSortDirection.Ascending, savedSorts[0].SortDirection);
			AssertEquals(DummyBusinessObject.Schema.Z0_Code, savedSorts[1].PropertyDescriptor.Name);
			AssertEquals(ListSortDirection.Descending, savedSorts[1].SortDirection);

			Collection.Load();

			AssertEquals("XX3", Collection[0].Z0_Code);
			AssertEquals("XX3", Collection[1].Z0_Code);
			AssertEquals("XX1", Collection[2].Z0_Code);
			AssertEquals("XX1", Collection[3].Z0_Code);

			AssertEquals(1, Collection[0].Z0_Number);
			AssertEquals(2, Collection[1].Z0_Number);
			AssertEquals(1, Collection[2].Z0_Number);
			AssertEquals(2, Collection[3].Z0_Number);

			savedSorts = listView.SortDescriptions;
			AssertEquals(2, savedSorts.Count);
			AssertEquals(DummyBusinessObject.Schema.Z0_Number, savedSorts[0].PropertyDescriptor.Name);
			AssertEquals(ListSortDirection.Ascending, savedSorts[0].SortDirection);
			AssertEquals(DummyBusinessObject.Schema.Z0_Code, savedSorts[1].PropertyDescriptor.Name);
			AssertEquals(ListSortDirection.Descending, savedSorts[1].SortDirection);

			Collection.RemoveSorts();
			Collection.Load();

			AssertEquals("XX3", Collection[0].Z0_Code);
			AssertEquals("XX3", Collection[1].Z0_Code);
			AssertEquals("XX1", Collection[2].Z0_Code);
			AssertEquals("XX1", Collection[3].Z0_Code);

			savedSorts = listView.SortDescriptions;
			AssertEquals(1, savedSorts.Count);
			AssertEquals(DummyBusinessObject.Schema.Z0_Code, savedSorts[0].PropertyDescriptor.Name);
			AssertEquals(ListSortDirection.Descending, savedSorts[0].SortDirection);
		}

		public void TestAdvancedSorting()
		{
			IBindingListView listView = Collection;
			Assert(listView.SupportsAdvancedSorting);

			ListSortDescription[] sorts = new ListSortDescription[]
			{
				new ListSortDescription(GetPropertyDescriptor(DummyBusinessObject.Schema.Z0_Code), ListSortDirection.Descending),
				new ListSortDescription(GetPropertyDescriptor(DummyBusinessObject.Schema.Z0_Date), ListSortDirection.Ascending),
				new ListSortDescription(GetPropertyDescriptor(DummyBusinessObject.Schema.Z0_Decimal), ListSortDirection.Ascending)
			};

			listView.ApplySort(new ListSortDescriptionCollection(sorts));

			ListSortDescriptionCollection savedSorts = listView.SortDescriptions;

			AssertEquals(DummyBusinessObject.Schema.Z0_Code, savedSorts[0].PropertyDescriptor.Name);
			AssertEquals(ListSortDirection.Descending, savedSorts[0].SortDirection);
			AssertEquals(DummyBusinessObject.Schema.Z0_Date, savedSorts[1].PropertyDescriptor.Name);
			AssertEquals(ListSortDirection.Ascending, savedSorts[1].SortDirection);
			AssertEquals(DummyBusinessObject.Schema.Z0_Decimal, savedSorts[2].PropertyDescriptor.Name);
			AssertEquals(ListSortDirection.Ascending, savedSorts[2].SortDirection);
		}

		PropertyDescriptor GetPropertyDescriptor(string propertyName)
		{
			return ZCustomTypeDescriptor.GetProperties(Collection.TypeOfElements)[propertyName];
		}

		#endregion

		#region TestMastersAreInDatabase

		public void TestMastersAreInDatabase()
		{
			BusinessObjectCollection collection = new VanillaBusinessObjectCollection(Factory);
			AssertEquals("No masters not in database", true, ((IBusinessObjectCollectionInternals)collection).MastersAreInDatabase);
		}

		class VanillaBusinessObjectCollection : BusinessObjectCollection
		{
			public VanillaBusinessObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		#endregion

		#region Test IsLoading

		public void TestIsLoading()
		{
			AssertEquals("Not loading now", false, Dummy.Collection.ExposedIsLoading);
			Dummy.Collection.CountChanged += new CollectionCountChangedEventHandler(Collection_CountChanged_IsLoadingShouldBeTrueWhenLoading);
			CountChangedCalled = 0;
			Dummy.Clone();
			Dummy.Collection.Load();
			Assert("CountChanged should be called", CountChangedCalled > 0);
		}

		void Collection_CountChanged_IsLoadingShouldBeTrueWhenLoading(object sender, CollectionCountChangedEventArgs e)
		{
			CountChangedCalled++;
			if (!Dummy.Collection.ExposedIsLoading)
			{
				throw new Exception("Not IsLoading - see TestIsLoading");
			}
		}

		#endregion

		#region Data refresh of non-persistent business objects

		public void TestIsNonPersistentCollection()
		{
			Assert("Elements are non-persistent", new DummyNonPersistentCollection().IsNonPersistent);
			Assert("Elements are not non-persistent", !(new DummyBusinessObjectCollection(new BusinessObjectFactory()).IsNonPersistent));
		}

		class DummyNonPersistent : NonPersistentBusinessObject { }

		class DummyNonPersistentCollection : BusinessObjectCollection
		{
			public DummyNonPersistentCollection() : base(new BusinessObjectFactory()) { }

			public DummyNonPersistent this[int index]
			{
				get { return null; }
			}
		}

		#endregion

		#region EstimatedLoadCount

		public void TestEstimatedLoadCountUsingQueryWithoutOrderByStatement()
		{
			TestEstimatedLoadCount(false);
		}

		public void TestEstimatedLoadCountUsingQueryWithOrderByStatement()
		{
			TestEstimatedLoadCount(true);
		}

		void TestEstimatedLoadCount(bool includeOrderByStatement)
		{
			for (int i = 0; i < 50; i++)
			{
				DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Description = "DECOY";
			}
			for (int i = 0; i < 50; i++)
			{
				DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Description = "MATCH";
			}
			Factory.Save();

			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Description, "MATCH");
			if (includeOrderByStatement)
			{
				filter.OrderBy = DummyBizoSchema.Z0_Description.Name;
			}

			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			int countObtainedFromCollection = collection.GetEstimatedLoadCount(filter);
			AssertEquals("Estimated load count", 50, countObtainedFromCollection);
		}

		public void TestEstimatedLoadCountUsesAllAdditionalFilters()
		{
			DummyBusinessObjectCollectionWithRelationshipFilter collection = new DummyBusinessObjectCollectionWithRelationshipFilter(Factory);
			int countOfExistingInDatabase = (int)Db.Connection.ExecuteScalar(
				"SELECT COUNT(*) FROM dbo.DummyBizo where Z0_Description = @desc and Z0_Guid = @guid",
				cmd =>
				{
					cmd.AddParameterBasedOnDbColumn("@desc", "EstimatedLoadCount Test", DummyBizoSchema.Z0_Description);
					cmd.AddParameterBasedOnDbColumn("@guid", collection.AGuid.ToGuid(), DummyBizoSchema.Z0_Guid);
				});
			AssertEquals("Should be no such records in the database", 0, countOfExistingInDatabase);

			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Description, "EstimatedLoadCount Test");

			DummyBusinessObjectWithActiveFilter dummy1 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			dummy1.Z0_Code = "$X1";
			dummy1.Z0_Bool = false;

			DummyBusinessObjectWithActiveFilter dummy2 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			dummy2.Z0_Description = "EstimatedLoadCount Test";
			dummy2.Z0_Code = "$X2";
			dummy2.Z0_Bool = true;

			DummyBusinessObjectWithActiveFilter dummy3 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			dummy3.Z0_Description = "EstimatedLoadCount Test";
			dummy3.Z0_Guid = collection.AGuid;
			dummy3.Z0_Code = "$X3";
			dummy3.Z0_Bool = false;

			DummyBusinessObjectWithActiveFilter dummy4 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			dummy4.Z0_Description = "EstimatedLoadCount Test";
			dummy4.Z0_Guid = collection.AGuid;
			dummy4.Z0_Code = "$X4";
			dummy4.Z0_Bool = true;

			DummyBusinessObjectWithActiveFilter dummy5 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			dummy5.Z0_Description = "EstimatedLoadCount Test";
			dummy5.Z0_Guid = collection.AGuid;
			dummy5.Z0_Code = "$X5";
			dummy5.Z0_Bool = true;

			Factory.Save();

			int countObtainedFromDatabase = (int)Db.Connection.ExecuteScalar(
				"SELECT COUNT(*) FROM dbo.DummyBizo where Z0_Description = @desc and Z0_Bool = @bool and Z0_Guid = @guid",
				cmd =>
				{
					cmd.AddParameterBasedOnDbColumn("@desc", "EstimatedLoadCount Test", DummyBizoSchema.Z0_Description);
					cmd.AddParameterBasedOnDbColumn("@bool", 'Y', DummyBizoSchema.Z0_Bool);
					cmd.AddParameterBasedOnDbColumn("@guid", collection.AGuid.ToGuid(), DummyBizoSchema.Z0_Guid);
				});
			AssertEquals("Should be two records - Active and Relationship Filters should both be used", 2, countObtainedFromDatabase);

			int countObtainedFromCollection = collection.GetEstimatedLoadCount(filter);

			collection.Load(filter);
			int collectionLoadCount = collection.Count;

			AssertEquals("Estimated load count", countObtainedFromDatabase, countObtainedFromCollection);
			AssertEquals("Collection load count", countObtainedFromDatabase, collectionLoadCount);

			filter.IgnoreActiveFilter = true;

			countObtainedFromDatabase = (int)Db.Connection.ExecuteScalar(
				"SELECT COUNT(*) FROM dbo.DummyBizo where Z0_Description = @desc and Z0_Guid = @guid",
				cmd =>
				{
					cmd.AddParameterBasedOnDbColumn("@desc", "EstimatedLoadCount Test", DummyBizoSchema.Z0_Description);
					cmd.AddParameterBasedOnDbColumn("@guid", collection.AGuid.ToGuid(), DummyBizoSchema.Z0_Guid);
				});
			AssertEquals("Should be three records - Active should be ignored, relationship should be used", 3, countObtainedFromDatabase);

			countObtainedFromCollection = collection.GetEstimatedLoadCount(filter);

			collection.Load(filter);
			collectionLoadCount = collection.Count;

			AssertEquals("Estimated load count", countObtainedFromDatabase, countObtainedFromCollection);
			AssertEquals("Collection load count", countObtainedFromDatabase, collectionLoadCount);
		}

		class DummyBusinessObjectWithActiveFilter : DummyBusinessObject
		{
			public DummyBusinessObjectWithActiveFilter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public static ZQuery ActiveFilter
			{
				get { return new ZQuery(DummyBizoSchema.Z0_Bool, true); }
			}
		}

		class DummyBusinessObjectCollectionWithRelationshipFilter : BusinessObjectCollection<DummyBusinessObjectWithActiveFilter>
		{
			public DummyBusinessObjectCollectionWithRelationshipFilter(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override ZQuery CreateRelationshipFilter()
			{
				ZQuery result = base.CreateRelationshipFilter();
				result.AddToFilter(new ZQuery(DummyBizoSchema.Z0_Guid, AGuid));
				return result;
			}

			public readonly ZGuid AGuid = ZGuid.NewZGuid();
		}

		#endregion

		#region FetchFromLocalCacheOnly

		public void TestFetchOnlyFromLocalCache()
		{
			BusinessObjectFactory savingFactory = new BusinessObjectFactory();
			DummyBusinessObject savedDummy = savingFactory.New<DummyBusinessObject>();
			savingFactory.Save();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject inMemoryDummy = factory.New<DummyBusinessObject>();

			TestBusinessObjectCollectionWithFetchFromLocalCacheOnly collection = new TestBusinessObjectCollectionWithFetchFromLocalCacheOnly(factory);
			collection.SetFetchOnlyFromLocalCache(true);
			collection.Load();
			AssertEquals("Only 1 is in the local cache", 1, collection.Count);
			AssertEquals("Only 1 is in the local cache", inMemoryDummy.PK, collection[0].PK);
		}

		public void TestFetchOnlyFromLocalCache_OnAdditionalFilter()
		{
			BusinessObjectFactory savingFactory = new BusinessObjectFactory();
			DummyBusinessObject savedDummy = savingFactory.New<DummyBusinessObject>();
			savingFactory.Save();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject inMemoryDummy = factory.New<DummyBusinessObject>();

			TestBusinessObjectCollectionWithFetchFromLocalCacheOnly collection = new TestBusinessObjectCollectionWithFetchFromLocalCacheOnly(factory);
			ZQuery fetchOnlyFromLocalCacheFilter = new ZQuery();
			fetchOnlyFromLocalCacheFilter.FetchOnlyFromLocalCache = true;
			collection.Load(fetchOnlyFromLocalCacheFilter);
			AssertEquals("Only 1 is in the local cache", 1, collection.Count);
			AssertEquals("Only 1 is in the local cache", inMemoryDummy.PK, collection[0].PK);
		}

		class TestBusinessObjectCollectionWithFetchFromLocalCacheOnly : BusinessObjectCollection<DummyBusinessObject>
		{
			public TestBusinessObjectCollectionWithFetchFromLocalCacheOnly(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public void SetFetchOnlyFromLocalCache(bool value)
			{
				fFetchOnlyFromLocalCache = value;
			}

			protected override bool FetchOnlyFromLocalCache
			{
				get { return fFetchOnlyFromLocalCache; }
			}
			bool fFetchOnlyFromLocalCache;
		}

		#endregion

		#region LoadUsesIgnoreActiveFilterFromAdditionalFilter

		public void TestLoadUsesIgnoreActiveFilterFromAdditional()
		{
			TestCaseHelper.ClearTable(DummyBizoSchema.Constants.TableName);

			DummyBusinessObjectWithActiveFilterCollection collection = new DummyBusinessObjectWithActiveFilterCollection(Factory);

			DummyBusinessObjectWithActiveFilter bizO1 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			bizO1.Z0_Description = "TestLoadUsesIgnoreActiveFilterFromAdditional";
			bizO1.Z0_Bool = false;

			DummyBusinessObjectWithActiveFilter bizO2 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			bizO2.Z0_Description = "TestLoadUsesIgnoreActiveFilterFromAdditional";
			bizO2.Z0_Bool = true;

			DummyBusinessObjectWithActiveFilter bizO3 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			bizO3.Z0_Description = "TestLoadUsesIgnoreActiveFilterFromAdditional";
			bizO3.Z0_Bool = false;

			DummyBusinessObjectWithActiveFilter bizO4 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			bizO4.Z0_Description = "TestLoadUsesIgnoreActiveFilterFromAdditional";
			bizO4.Z0_Bool = true;

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Description, "TestLoadUsesIgnoreActiveFilterFromAdditional");
			filter.IgnoreActiveFilter = false;

			collection.Load(filter);

			AssertEquals("Collection should load only active", 2, collection.Count);
			Assert("Collection should contain bizo2", collection.Contains(bizO2.PK));
			Assert("Collection should contain bizo4", collection.Contains(bizO4.PK));

			filter.IgnoreActiveFilter = true;
			collection.Load(filter);

			AssertEquals("Collection should load only active", 4, collection.Count);
			Assert("Collection should contain bizo1", collection.Contains(bizO1.PK));
			Assert("Collection should contain bizo2", collection.Contains(bizO2.PK));
			Assert("Collection should contain bizo3", collection.Contains(bizO3.PK));
			Assert("Collection should contain bizo4", collection.Contains(bizO4.PK));
		}

		#endregion

		#region Notification Propogation

		int MainObjNotificationChangedFiredCount;
		int CollectionNotificationChangedFiredCount;
		int ChildDummyNotificationChangedFiredCount;

		public void TestNotificationChangedIsNotFiredWhenValidationIsSuspended()
		{
			using (Dummy.SuspendValidationTesting())
			{
				Collection.SuspendValidation();
				CollectionNotificationChangedFiredCount = 0;
				((IBusinessObjectState)Collection).NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(BusinessObjectCollectionTest_NotificationsChanged);
				Dummy.Z0_DescriptionInfo.AddError("err");
				Collection.Add(Dummy);
				AssertEquals("CollectionNotificationChangedFiredCount", 0, CollectionNotificationChangedFiredCount);
				Dummy.Delete();
				AssertEquals("CollectionNotificationChangedFiredCount", 0, CollectionNotificationChangedFiredCount);
			}
		}

		object expectedSourceOfNotificationChange;

		void BusinessObjectCollectionTest_NotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
			AssertEquals("expectedSourceOfNotificationChange", expectedSourceOfNotificationChange, e.SourceOfNotificationChange);
			CollectionNotificationChangedFiredCount++;
		}

		public void TestAddAndRemoveFireNotificationsChangedWhenElementHasNotifications()
		{
			CollectionNotificationChangedFiredCount = 0;
			((IBusinessObjectState)Collection).NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(BusinessObjectCollectionTest_NotificationsChanged);
			AssertEquals(false, Dummy.HasNotifications());
			Collection.Add(Dummy);
			AssertEquals("CollectionNotificationChangedFiredCount", 0, CollectionNotificationChangedFiredCount);
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.AddRowError("err");
			expectedSourceOfNotificationChange = dummy2;
			Collection.Add(dummy2);
			AssertEquals("CollectionNotificationChangedFiredCount", 1, CollectionNotificationChangedFiredCount);
			expectedSourceOfNotificationChange = Dummy;
			Collection.Remove(Dummy);
			AssertEquals("CollectionNotificationChangedFiredCount", 1, CollectionNotificationChangedFiredCount);
			expectedSourceOfNotificationChange = dummy2;
			Collection.Remove(dummy2);
			AssertEquals("CollectionNotificationChangedFiredCount", 2, CollectionNotificationChangedFiredCount);
		}

		public void TestSenderPropagation()
		{
			CollectionNotificationChangedFiredCount = 0;
			((IBusinessObjectState)Collection).NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(BusinessObjectCollectionTest_NotificationsChanged);
			AssertEquals(false, Dummy.HasNotifications());
			Collection.Add(Dummy);
			AssertEquals("CollectionNotificationChangedFiredCount", 0, CollectionNotificationChangedFiredCount);
			expectedSourceOfNotificationChange = Dummy;
			Dummy.AddRowError("err");
			AssertEquals("CollectionNotificationChangedFiredCount", 1, CollectionNotificationChangedFiredCount);
		}

		public void TestNotificationPropagation()
		{
			using (Dummy.SuspendValidationTesting())
			{
				MainObjNotificationChangedFiredCount = 0;
				CollectionNotificationChangedFiredCount = 0;
				ChildDummyNotificationChangedFiredCount = 0;

				Dummy.NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(DummyNotificationChangedEventHandler);
				Dummy.Z0_AnotherNumberInfo.AddError("Some Generic Error");

				AssertEquals("NotificationChanged Fired", 1, MainObjNotificationChangedFiredCount);

				DummyChildBusinessObject newDummy = Dummy.Collection.AddNew();
				using (newDummy.SuspendValidationTesting())
				{
					newDummy.NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(NewDummyNotificationChangedEventHandler);

					BusinessObjectCollection dummyCollection = Dummy.Collection;    // Get Dummy's Collection (which contains NewDummy)
					Dummy.RegisterEditableChildObject(dummyCollection);

					((IBusinessObjectState)dummyCollection).NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(DummyCollectionNotificationChangedEventHandler);

					newDummy.Z0_AnotherNumberInfo.AddError("Some other Generic Error");

					AssertEquals("Collection's NotificationChanged Fired", 1, CollectionNotificationChangedFiredCount);
					AssertEquals("Biz Obj NotificationChanged Fired", 1, ChildDummyNotificationChangedFiredCount);
					AssertEquals("Parent Biz Obj NotificationChanged Fired", 2, MainObjNotificationChangedFiredCount);

					dummyCollection.Remove(newDummy);
					AssertEquals("Removing object changes notifications", 2, CollectionNotificationChangedFiredCount);
					AssertEquals("Parent Biz Obj NotificationChanged due to removing object", 3, MainObjNotificationChangedFiredCount);

					newDummy.Z0_AnotherNumberInfo.AddError("And the Last Generic Error");

					AssertEquals("Collection's NotificationChanged Not Fired", 2, CollectionNotificationChangedFiredCount);
					AssertEquals("Biz Obj NotificationChanged Fired", 2, ChildDummyNotificationChangedFiredCount);
					AssertEquals("Parent Biz Obj NotificationChanged Not Fired", 3, MainObjNotificationChangedFiredCount);
				}
			}
		}

		public void TestNotificationPropagationOnRowError()
		{
			MainObjNotificationChangedFiredCount = 0;
			CollectionNotificationChangedFiredCount = 0;
			ChildDummyNotificationChangedFiredCount = 0;

			Dummy.NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(DummyNotificationChangedEventHandler);
			Dummy.AddRowError("Some Row Error");

			AssertEquals("NotificationChanged Fired", 1, MainObjNotificationChangedFiredCount);

			DummyChildBusinessObject newDummy = Dummy.Collection.AddNew();
			newDummy.NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(NewDummyNotificationChangedEventHandler);

			BusinessObjectCollection dummyCollection = Dummy.Collection;    // Get Dummy's Collection (which contains NewDummy)
			Dummy.RegisterEditableChildObject(dummyCollection);

			((IBusinessObjectState)dummyCollection).NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(DummyCollectionNotificationChangedEventHandler);

			newDummy.AddRowError("Some New Row Error");

			AssertEquals("Collection's NotificationChanged Fired", 1, CollectionNotificationChangedFiredCount);
			AssertEquals("Biz Obj NotificationChanged Fired", 1, ChildDummyNotificationChangedFiredCount);
			AssertEquals("Parent Biz Obj NotificationChanged Fired", 2, MainObjNotificationChangedFiredCount);

			dummyCollection.Remove(newDummy);
			AssertEquals("Removing object changes notifications", 2, CollectionNotificationChangedFiredCount);
			AssertEquals("Parent Biz Obj NotificationChanged due to removing object", 3, MainObjNotificationChangedFiredCount);

			newDummy.AddRowError("The Last Row Error");

			AssertEquals("Collection's NotificationChanged Not Fired", 2, CollectionNotificationChangedFiredCount);
			AssertEquals("Biz Obj NotificationChanged Fired", 2, ChildDummyNotificationChangedFiredCount);
			AssertEquals("Parent Biz Obj NotificationChanged Not Fired", 3, MainObjNotificationChangedFiredCount);
		}

		void DummyNotificationChangedEventHandler(object sender, EventArgs args)
		{
			MainObjNotificationChangedFiredCount++;
		}

		void NewDummyNotificationChangedEventHandler(object sender, EventArgs args)
		{
			ChildDummyNotificationChangedFiredCount++;
		}

		void DummyCollectionNotificationChangedEventHandler(object sender, EventArgs args)
		{
			CollectionNotificationChangedFiredCount++;
		}

		public void TestNotificationCounts_OnAddNotificationsFrom()
		{
			using (Dummy.SuspendValidationTesting())
			{
				Dummy.ClearAllNotifications();

				Dummy.Z0_AnotherNumberInfo.AddError("An Error");
				Dummy.Z0_AnotherNumberInfo.AddError("Another Error");
				Dummy.Z0_AnotherNumberInfo.AddError("A final Error");
				AssertEquals("Property Error Count Incremented.", 3, Dummy.Z0_AnotherNumberInfo.GetErrors().Count());
				AssertEquals("BizObj Error Count Incremented.", 3, Dummy.Notifications.GetErrors().Count());

				Dummy.Z0_BoolInfo.AddError("A bool Error");
				Dummy.Z0_BoolInfo.AddError("Another bool Error");
				Dummy.Z0_BoolInfo.AddError("A final bool Error");

				AssertEquals("BizObj Error Count Incremented.", 6, Dummy.Notifications.GetErrors().Count());

				Dummy.Z0_AnotherNumberInfo.AddAllNotificationsFrom(Dummy.Z0_BoolInfo);
				AssertEquals("Property Error Count Incremented with copied Notifications", 6, Dummy.Z0_AnotherNumberInfo.GetErrors().Count());
				AssertEquals("BizObj Error Count Incremented with copied errors", 9, Dummy.Notifications.GetErrors().Count());
			}
		}

		public void TestIListWithNotificationsIndexOf()
		{
			IBusinessObjectCollection list = Collection;
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();

			Collection.Add(dummy1);
			Collection.Add(dummy2);
			Collection.Add(dummy3);

			AssertEquals(-1, list.IndexOf(dummy3, 0, 1));
			AssertEquals(2, list.IndexOf(dummy3, 0, 100));
			AssertEquals(2, list.IndexOf(dummy3, 2, 1));

			Collection.Remove(dummy2);
			AssertEquals(1, list.IndexOf(dummy3, 1, 1));
		}

		public void TestIListWithNotificationsIndexer()
		{
			IBusinessObjectCollection list = Collection;
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();

			Collection.Add(dummy1);
			Collection.Add(dummy2);
			Collection.Add(dummy3);

			AssertEquals(dummy1, list[0]);
			AssertEquals(dummy2, list[1]);
			AssertEquals(dummy3, list[2]);
		}

		// work item created for Niall, please do not delete this code unless you are NIALL.

		//		[ToDo("Niall", "1/3/2005", "The code never worked properly in the first place")]
		//		public void TestNotificationCountForDuplicateMessageCreatesDeveloperError()
		//		{
		//			const string WarningMessage = "This is the warning message";
		//			ZPropertyInfo Info = Dummy.Z0_BoolInfo;
		//			Assert("Precondition", !Info.HasNotifications());
		//			Info.AddWarning(WarningMessage);
		//			ErrorReporter.Clear();
		//			try
		//			{
		//				Info.AddWarning(WarningMessage);
		//				Assert(!string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		//			}
		//			finally
		//			{
		//				Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
		//			}
		//		}

		#endregion

		#region Test Validation Suspended During Remove

		class RemoveTestCollection : DummyBusinessObjectCollection
		{
			public RemoveTestCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public bool WasValidationSuspendedDuringRemove;

			protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
			{
				base.RemoveCollectionRelationshipsCore(child, forDelete);
				WasValidationSuspendedDuringRemove = true;
			}
		}

		public void TestRemoveCollectionRelationshipsInternalSuspendsAndResumesValidation()
		{
			RemoveTestCollection collection = new RemoveTestCollection(Factory);
			collection.Add(Dummy);
			collection.WasValidationSuspendedDuringRemove = false;
			Assert("Validation active", !collection.IsValidationSuspended);
			collection.Remove(Dummy);
			Assert("Validation disabled during remove relationships", collection.WasValidationSuspendedDuringRemove);
			Assert("Validation active", !collection.IsValidationSuspended);
		}

		#endregion

		#region Test Elements Changed During PreSaveValidation

		public void TestElementsChangedDuringPreSaveValidation()
		{
			ElementsChangedDuringPreSaveValidationTestBizO child1 = Factory.New<ElementsChangedDuringPreSaveValidationTestBizO>();
			ElementsChangedDuringPreSaveValidationTestBizO child2 = Factory.New<ElementsChangedDuringPreSaveValidationTestBizO>();
			ElementsChangedDuringPreSaveValidationTestBizO child3 = Factory.New<ElementsChangedDuringPreSaveValidationTestBizO>();
			ElementsChangedDuringPreSaveValidationTestBizO child4 = Factory.New<ElementsChangedDuringPreSaveValidationTestBizO>();

			child1.BizOToDeleteDuringValidation = child2;
			child4.AddNewDuringValidation = true;
			child4.BizOToDeleteDuringValidation = child3;

			Collection.Add(child1);
			Collection.Add(child2);
			Collection.Add(child3);
			Collection.Add(child4);
			AssertEquals("Count", 4, Collection.Count);

			Collection.RunPreSaveValidation();
			AssertEquals("Count", 3, Collection.Count);

			AssertEquals("[0] should be child1.", child1, Collection[0]);
			AssertEquals("[1] should be child4.", child4, Collection[1]);
			AssertNotEquals("[2] should not be child1.", child1, Collection[2]);
			AssertNotEquals("[2] should not be child2.", child2, Collection[2]);
			AssertNotEquals("[2] should not be child3.", child3, Collection[2]);
			AssertNotEquals("[2] should not be child4.", child4, Collection[2]);

			AssertEquals("[0].Validated", true, child1.Validated);
			AssertEquals("[1].Validated", true, child4.Validated);
			AssertEquals("[2].Validated", true, ((ElementsChangedDuringPreSaveValidationTestBizO)Collection[2]).Validated);
		}

		class ElementsChangedDuringPreSaveValidationTestBizO : DummyBusinessObject
		{
			bool addNewDuringValidation;
			ElementsChangedDuringPreSaveValidationTestBizO bizOToDeleteDuringValidation;
			bool validated;

			public ElementsChangedDuringPreSaveValidationTestBizO(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool AddNewDuringValidation
			{
				get { return addNewDuringValidation; }
				set { addNewDuringValidation = value; }
			}

			public ElementsChangedDuringPreSaveValidationTestBizO BizOToDeleteDuringValidation
			{
				get { return bizOToDeleteDuringValidation; }
				set { bizOToDeleteDuringValidation = value; }
			}

			public bool Validated
			{
				get { return validated; }
			}

			protected override void RunPreSaveValidationCore()
			{
				base.RunPreSaveValidationCore();
				if (AddNewDuringValidation)
				{
					ElementsChangedDuringPreSaveValidationTestBizO newBizO = Factory.New<ElementsChangedDuringPreSaveValidationTestBizO>();
					ParentCollections.First().Add(newBizO);
				}
				if (BizOToDeleteDuringValidation != null)
				{
					BizOToDeleteDuringValidation.Delete();
				}
				validated = true;
			}
		}

		#endregion

		#region CanContinueWithSave

		public void TestCanContinueWithSaveDefaultValue()
		{
			Mock<BusinessObjectCollection> mock = new Mock<BusinessObjectCollection>(new object[] { Factory });
			mock.CallBase = true;
			AssertEquals(true, mock.Object.CanContinueWithSave);
		}

		public void TestCanContinueWithSaveCallsVirtualMethod()
		{
			Mock<BusinessObjectCollection> mock = new Mock<BusinessObjectCollection>(new object[] { Factory });
			mock.CallBase = true;
			mock.Protected()
				.Setup<bool>("CanContinueWithSaveCore")
				.Returns(false);
			AssertEquals(false, mock.Object.CanContinueWithSave);

			mock.Protected()
				.Setup<bool>("CanContinueWithSaveCore")
				.Returns(true);
			AssertEquals(true, mock.Object.CanContinueWithSave);
		}

		#endregion

		#region Prefetch

		public void TestRunPreSaveValidationFetchCallsChildObjectsOnceOnly()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bizO1 = collection.AddNew();
			BusinessObject dep1 = Factory.New(typeof(DummyDependantBusinessObject));
			bizO1.RegisterEditableChildObject(dep1);

			BusinessObjectCollectionFetchStrategyForTest collectionFetch = (BusinessObjectCollectionFetchStrategyForTest)collection.FetchStrategy;
			BusinessObjectFetchStrategyForTest dep1Fetch = (BusinessObjectFetchStrategyForTest)dep1.FetchStrategy;
			BusinessObjectFetchStrategyForTest bizO1Fetch = (BusinessObjectFetchStrategyForTest)bizO1.FetchStrategy;
			AssertEquals(0, collectionFetch.FetchForValidateCoreCount);
			AssertEquals(0, dep1Fetch.FetchForValidateCoreCount);
			AssertEquals(0, bizO1Fetch.FetchForValidateCoreCount);
			((IBusiness)collection).RunPreSaveValidationFetch(true);
			AssertEquals(1, dep1Fetch.FetchForValidateCoreCount);
			AssertEquals(1, bizO1Fetch.FetchForValidateCoreCount);
			AssertEquals(1, collectionFetch.FetchForValidateCoreCount);

			((IBusiness)collection).RunPreSaveValidationFetch(true);
			AssertEquals(1, dep1Fetch.FetchForValidateCoreCount);
			AssertEquals(1, bizO1Fetch.FetchForValidateCoreCount);
			AssertEquals(1, collectionFetch.FetchForValidateCoreCount);
		}

		#endregion

		#region RunFetchForSort

		public void TestRunFetchForSort()
		{
			CalculableSortableObject bizo = Factory.NewWithValidTestData<CalculableSortableObject>();
			bizo.Z0_Code = "SRT";

			bizo = Factory.NewWithValidTestData<CalculableSortableObject>();
			bizo.Z0_Code = "SRT";

			bizo = Factory.NewWithValidTestData<CalculableSortableObject>();
			bizo.Z0_Code = "SRT";

			bizo = Factory.NewWithValidTestData<CalculableSortableObject>();
			bizo.Z0_Code = "SRT";

			bizo = Factory.NewWithValidTestData<CalculableSortableObject>();
			bizo.Z0_Code = "SRT";

			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();

			CalculableSortableObjectCollection collection = new CalculableSortableObjectCollection(factory1, new ZQuery(DummyBizoSchema.Z0_Code, "SRT"));
			collection.Load();
			AssertEquals(5, collection.Count);
			AssertEquals(1, factory1.GetTableHitCount(bizo.TableName));

			((IBindingList)collection).ApplySort(bizo.GetProperties()["Calculable1"], ListSortDirection.Ascending);
			AssertEquals(5, collection.Count);
			AssertEquals(6, factory1.GetTableHitCount(bizo.TableName));

			((IBindingList)collection).ApplySort(bizo.GetProperties()["Calculable2"], ListSortDirection.Ascending);
			AssertEquals(5, collection.Count);
			AssertEquals(7, factory1.GetTableHitCount(bizo.TableName));
		}

		class CalculableSortableObject : DummyBusinessObject
		{
			public CalculableSortableObject(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public readonly ZGuid FK1 = ZGuid.NewZGuid();
			public readonly ZGuid FK2 = ZGuid.NewZGuid();

			public ZString Calculable1
			{
				get
				{
					DummyBusinessObject bizo = Factory.Load<DummyBusinessObject>(FK1);
					return bizo == null ? "<NULL>" : FK1.ToString();
				}
			}

			public ZString Calculable2
			{
				get
				{
					DummyBusinessObject bizo = Factory.Load<DummyBusinessObject>(FK2);
					return bizo == null ? "<NULL>" : FK2.ToString();
				}
			}

			protected override IBusinessObjectFetchStrategy GetFetchStrategy()
			{
				return new DummyFetch(this);
			}
		}

		class CalculableSortableObjectCollection : BusinessObjectCollection<CalculableSortableObject>
		{
			public CalculableSortableObjectCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter) { }
		}

		class DummyFetch : BusinessObjectFetchStrategy
		{
			public DummyFetch(CalculableSortableObject businessObject) : base(businessObject)
			{
				calculable = businessObject;
			}

			readonly CalculableSortableObject calculable;

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);

				foreach (TableColumn column in columns)
				{
					if (column.ColumnName == "Calculable2")
					{
						Factory.AddFetchHint(typeof(DummyBusinessObject), calculable.FK2);
					}
				}
			}
		}

		#endregion

		#region RefreshBinding

		public void TestRefreshBindingIncludingChildren()
		{
			bool child1OnElementFired = false;
			bool child2OnElementFired = false;

			DummyChildBusinessObjectCollection coll = new DummyChildBusinessObjectCollection(Factory);
			DummyChildBusinessObject testChild1 = coll.AddNew();
			((IBusiness)testChild1).ListChanged += new ListChangedEventHandler(delegate
			{ child1OnElementFired = true; });

			DummyChildBusinessObject testChild2 = coll.AddNew();
			((IBusiness)testChild2).ListChanged += new ListChangedEventHandler(delegate
			{ child2OnElementFired = true; });

			coll.RefreshBindingIncludingChildren();
			AssertEquals("child 1 element changed event fired", true, child1OnElementFired);
			AssertEquals("child 2 element changed event fired", true, child2OnElementFired);
		}

		public void TestRefreshBinding()
		{
			bool child1OnElementFired = false;
			bool child2OnElementFired = false;
			bool collectionListChangedFired = false;

			DummyChildBusinessObjectCollection coll = new DummyChildBusinessObjectCollection(Factory);
			((IBusiness)coll).ListChanged += new ListChangedEventHandler(delegate
			{ collectionListChangedFired = true; });

			DummyChildBusinessObject testChild1 = coll.AddNew();
			((IBusiness)testChild1).ListChanged += new ListChangedEventHandler(delegate
			{ child1OnElementFired = true; });

			DummyChildBusinessObject testChild2 = coll.AddNew();
			((IBusiness)testChild2).ListChanged += new ListChangedEventHandler(delegate
			{ child2OnElementFired = true; });

			coll.RefreshBinding();
			AssertEquals("collection list changed event fired", true, collectionListChangedFired);
			AssertEquals("child 1 element changed event should not be fired", false, child1OnElementFired);
			AssertEquals("child 2 element changed event should not be fired", false, child2OnElementFired);
		}

		#endregion

		#region Rest of Testing

		public void TestAddNewWithTypeAddsToParentCollections()
		{
			BusinessObject bizO = Collection.AddNew(typeof(DummyBusinessObject));
			AssertEquals(1, bizO.ParentCollections.Count);
		}

		public void TestHasChangesNotIncludingChildren()
		{
			IBusiness collection = Dummy.Collection;
			Assert("No changes", !collection.HasChangesNotIncludingChildren);
			Assert("No changes", !collection.HasChanges);

			Dummy.Collection.AddNew().Z0_Number = 343;
			Assert("No changes without kids", !collection.HasChangesNotIncludingChildren);
			Assert("Changes with kids", collection.HasChanges);

			Dummy.Collection[0].Delete();
			Assert("Changes without kids", collection.HasChangesNotIncludingChildren);
			Assert("Changes with kids", collection.HasChanges);
		}

		public void TestFilterBusinessObjectDefaultsGet()
		{
			AssertNotNull("FilterBusinessObjectDefaults not null", Dummy.Collection.FilterBusinessObjectDefaults);
		}

		public void TestGetComparerForSort()
		{
			AssertEquals("Clear collection", 0, Dummy.Collection.Count);
			Dummy.Collection.AddNew().Z0_Number = 1;
			Dummy.Collection.AddNew().Z0_Number = 2;
			Dummy.Collection.AddNew().Z0_Number = 0;

			Dummy.Collection.Sort(DummyBusinessObject.Schema.Z0_Number, ListSortDirection.Ascending);
			AssertEquals("Normal ascending order", 0, Dummy.Collection[0].Z0_Number);
			AssertEquals("Normal ascending order", 1, Dummy.Collection[1].Z0_Number);
			AssertEquals("Normal ascending order", 2, Dummy.Collection[2].Z0_Number);

			Dummy.Collection.UseBig2Sort = true;
			Dummy.Collection.Sort(DummyBusinessObject.Schema.Z0_Number, ListSortDirection.Ascending);
			AssertEquals("Big2 order", 2, Dummy.Collection[0].Z0_Number);
			AssertEquals("Normal ascending order", 0, Dummy.Collection[1].Z0_Number);
			AssertEquals("Normal ascending order", 1, Dummy.Collection[2].Z0_Number);
		}

		public void TestTimeComparerForSort()
		{
			var list = new DummyBusinessObjectInListList(Factory);
			var bizO1 = list.AddNew();
			var bizO2 = list.AddNew();
			var bizO3 = list.AddNew();
			bizO1.Z0_SmallDateTime = new ZDateTime(2022, 1, 1, 0, 1, 0);
			bizO2.Z0_SmallDateTime = new ZDateTime(2020, 1, 1, 0, 2, 0);
			bizO3.Z0_SmallDateTime = new ZDateTime(2021, 1, 1, 0, 3, 0);

			Factory.Save();

			((IBusinessObjectCollection)list).AddIsTime(DummyBizoSchema.Z0_SmallDateTime.Name);
			list.Sort(DummyBizoSchema.Z0_SmallDateTime.Name, ListSortDirection.Ascending);

			AssertEquals(bizO1.PK, list[0].PK);
			AssertEquals(bizO2.PK, list[1].PK);
			AssertEquals(bizO3.PK, list[2].PK);

			list.Sort(DummyBizoSchema.Z0_SmallDateTime.Name, ListSortDirection.Descending);

			AssertEquals(bizO3.PK, list[0].PK);
			AssertEquals(bizO2.PK, list[1].PK);
			AssertEquals(bizO1.PK, list[2].PK);
		}

		public void TestGetGuidComparerForSort()
		{
			DummyBusinessObjectInListList list = new DummyBusinessObjectInListList(Factory);
			for (int i = 0; i < 10; i++)
			{
				DummyBusinessObjectInList bizO = list.AddNew();
				bizO.Z0_Code = (9 - i).ToString();
				bizO.Z0_Guid = bizO.PK;
				bizO.LookupList.Add(bizO);
			}

			Factory.Save();

			((IBusinessObjectCollection)list).AddGuidListMapping(DummyBizoSchema.Z0_Guid.Name, "LookupList");
			list.Sort(DummyBizoSchema.Z0_Guid.Name, ListSortDirection.Ascending);

			for (int i = 0; i < 10; i++)
			{
				AssertEquals(i.ToString(), list[i].Z0_Code);
			}
		}

		public void TestNonReadOnlyCollectionDoesNotChangeReadonlyOfNewElements()
		{
			AssertEquals("Clear collection", 0, Dummy.Collection.Count);
			AssertEquals("Nonreadonly collection", false, Dummy.Collection.ReadOnly);
			BusinessObject nonReadOnlyDummy = Factory.New(typeof(DummyBusinessObject));

			BusinessObject readOnlyDummy = Factory.New(typeof(DummyBusinessObject));
			readOnlyDummy.ReadOnly = true;

			AssertEquals("Should be readonly", true, readOnlyDummy.ReadOnly);
			AssertEquals("Should not be readonly", false, nonReadOnlyDummy.ReadOnly);

			Dummy.Collection.Add(nonReadOnlyDummy);
			Dummy.Collection.Add(readOnlyDummy);

			AssertEquals("Should be readonly", true, readOnlyDummy.ReadOnly);
			AssertEquals("Should not be readonly", false, nonReadOnlyDummy.ReadOnly);
			AssertEquals("Should not be readonly", false, Dummy.Collection.AddNew().ReadOnly);
		}

		public void TestReadOnlyCollectionMakesNewElementsReadOnly()
		{
			AssertEquals("Clear collection", 0, Dummy.Collection.Count);
			BusinessObject nonReadOnlyDummy = Factory.New(typeof(DummyBusinessObject));

			BusinessObject readOnlyDummy = Factory.New(typeof(DummyBusinessObject));
			readOnlyDummy.ReadOnly = true;

			AssertEquals("Should be readonly", true, readOnlyDummy.ReadOnly);
			AssertEquals("Should not be readonly", false, nonReadOnlyDummy.ReadOnly);

			Dummy.Collection.SetReadOnlyIncludingChildren(true);
			Dummy.Collection.Add(nonReadOnlyDummy);
			Dummy.Collection.Add(readOnlyDummy);

			AssertEquals("Should be readonly", true, readOnlyDummy.ReadOnly);
			AssertEquals("Should be readonly", true, nonReadOnlyDummy.ReadOnly);
			AssertEquals("Should be readonly", true, Dummy.Collection.AddNew().ReadOnly);
		}

		public void TestSetReadOnlyIncludingChildren_FiresListReset()
		{
			ListChangedEventArgs lastListChanged = null;
			bool expectedReadonly = false;
			((IBindingList)Collection).ListChanged += (sender, e) =>
			{
				lastListChanged = e;
				AssertEquals("Collection.ReadOnly should already have correct state", expectedReadonly, Collection.ReadOnly);
			};

			expectedReadonly = true;
			Collection.SetReadOnlyIncludingChildren(true);
			AssertEquals("ListChangedType.Reset fired", ListChangedType.Reset, lastListChanged.ListChangedType);
			lastListChanged = null;

			expectedReadonly = false;
			Collection.SetReadOnlyIncludingChildren(false);
			AssertEquals("ListChangedType.Reset fired", ListChangedType.Reset, lastListChanged.ListChangedType);
			lastListChanged = null;
		}

		public void TestSortWithIComparer()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bizObj1 = collection.AddNew();
			DummyBusinessObject bizObj2 = collection.AddNew();
			DummyBusinessObject bizObj3 = collection.AddNew();

			bizObj1.Z0_Code = "BBC";
			bizObj1.Z0_Date = new ZDateTime(1980, 9, 10);

			bizObj2.Z0_Code = "ABC";
			bizObj2.Z0_Date = new ZDateTime(1980, 9, 10);

			bizObj3.Z0_Code = "BCD";
			bizObj3.Z0_Date = new ZDateTime(1984, 6, 15);

			AssertEquals("IndexOf BizObj1 in Collection", 0, ((IList)collection).IndexOf(bizObj1));
			AssertEquals("IndexOf BizObj2 in Collection", 1, ((IList)collection).IndexOf(bizObj2));
			AssertEquals("IndexOf BizObj3 in Collection", 2, ((IList)collection).IndexOf(bizObj3));

			collection.Sort(new DummyComparer());

			AssertEquals("IndexOf BizObj1 in Collection", 1, ((IList)collection).IndexOf(bizObj1));
			AssertEquals("IndexOf BizObj2 in Collection", 0, ((IList)collection).IndexOf(bizObj2));
			AssertEquals("IndexOf BizObj3 in Collection", 2, ((IList)collection).IndexOf(bizObj3));
		}

		public void TestSortWithComparisonDelegate()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bizObj1 = collection.AddNew();
			DummyBusinessObject bizObj2 = collection.AddNew();
			DummyBusinessObject bizObj3 = collection.AddNew();

			bizObj1.Z0_Code = "BBC";
			bizObj1.Z0_Date = new ZDateTime(1980, 9, 10);

			bizObj2.Z0_Code = "ABC";
			bizObj2.Z0_Date = new ZDateTime(1980, 9, 10);

			bizObj3.Z0_Code = "BCD";
			bizObj3.Z0_Date = new ZDateTime(1984, 6, 15);

			AssertEquals("IndexOf BizObj1 in Collection", 0, ((IList)collection).IndexOf(bizObj1));
			AssertEquals("IndexOf BizObj2 in Collection", 1, ((IList)collection).IndexOf(bizObj2));
			AssertEquals("IndexOf BizObj3 in Collection", 2, ((IList)collection).IndexOf(bizObj3));

			collection.Sort(new Comparison<BusinessObject>(new DummyComparer().Compare));

			AssertEquals("IndexOf BizObj1 in Collection", 1, ((IList)collection).IndexOf(bizObj1));
			AssertEquals("IndexOf BizObj2 in Collection", 0, ((IList)collection).IndexOf(bizObj2));
			AssertEquals("IndexOf BizObj3 in Collection", 2, ((IList)collection).IndexOf(bizObj3));
		}

		public void TestToArray()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);
			BusinessObject bizObj1 = collection.AddNew();
			BusinessObject bizObj2 = collection.AddNew();
			BusinessObject bizObj3 = collection.AddNew();

			AssertEquals("Collection Count", 3, collection.Count);

			BusinessObject[] array = collection.ToArray();
			AssertEquals("Array Length", 3, array.Length);
			AssertEquals("Element 0", bizObj1, array[0]);
			AssertEquals("Element 1", bizObj2, array[1]);
			AssertEquals("Element 2", bizObj3, array[2]);

			BusinessObject[] array2 = collection.ToArray();
			AssertEquals("Array Length", 3, array2.Length);
			AssertEquals("Element 0", bizObj1, array2[0]);
			AssertEquals("Element 1", bizObj2, array2[1]);
			AssertEquals("Element 2", bizObj3, array2[2]);
		}

		public void TestSwapFactoryAndRemoveAll()
		{
			((DummyBusinessObject)Factory.New(typeof(DummyBusinessObject))).Z0_Code = "BLAH";
			((DummyBusinessObject)Factory.New(typeof(DummyBusinessObject))).Z0_Code = "BLAH";

			Dummy.Collection.Load(new ZQuery(DummyBizoSchema.Z0_Code, "BLAH"));
			AssertEquals(2, Dummy.Collection.Count);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			Dummy.Collection.SwapFactoryAndRemoveAll(factory2);
			AssertEquals(0, Dummy.Collection.Count);

			((DummyBusinessObject)factory2.New(typeof(DummyBusinessObject))).Z0_Code = "BLAH";
			Dummy.Collection.Load(new ZQuery(DummyBizoSchema.Z0_Code, "BLAH"));
			AssertEquals(1, Dummy.Collection.Count);
		}

		public void TestOnAdded()
		{
			Dummy.Collection.OnAddedCalled = false;
			Dummy.Collection.RemoveAll();
			Dummy.Collection.AddNew();
			AssertEquals(true, Dummy.Collection.OnAddedCalled);

			Dummy.Collection.OnAddedCalled = false;
			Dummy.Collection.RemoveAll();
			Dummy.Collection.Add(Dummy);
			AssertEquals(true, Dummy.Collection.OnAddedCalled);

			Dummy.Collection.OnAddedCalled = false;
			Dummy.Collection.RemoveAll();
			Dummy.Collection.AddRange(Dummy, Dummy, Dummy);
			AssertEquals(true, Dummy.Collection.OnAddedCalled);

			Dummy.Collection.OnAddedCalled = false;
			Dummy.Collection.RemoveAll();
			((IList)Dummy.Collection).Add(Dummy);
			AssertEquals(true, Dummy.Collection.OnAddedCalled);

			Dummy.Collection.OnAddedCalled = false;
			Dummy.Collection.RemoveAll();
			((IList)Dummy.Collection).Insert(0, Dummy);
			AssertEquals(true, Dummy.Collection.OnAddedCalled);
		}

		public void TestOnRemoving()
		{
			Dummy.Collection.OnRemovingCalled = false;
			Dummy.Collection.RemoveAll();
			AssertEquals(false, Dummy.Collection.OnRemovingCalled);

			Dummy.Collection.OnRemovingCalled = false;
			Dummy.Collection.RemoveAndDeleteAll();
			AssertEquals(false, Dummy.Collection.OnRemovingCalled);

			Dummy.Collection.OnRemovingCalled = false;
			Dummy.Collection.Load();
			AssertEquals(false, Dummy.Collection.OnRemovingCalled);

			Dummy.Collection.OnRemovingCalled = false;
			Dummy.Collection.AddNew();
			Dummy.Collection.RemoveAll();
			AssertEquals(true, Dummy.Collection.OnRemovingCalled);

			Dummy.Collection.OnRemovingCalled = false;
			Dummy.Collection.AddNew();
			Dummy.Collection.Load();
			AssertEquals(true, Dummy.Collection.OnRemovingCalled);

			Dummy.Collection.OnRemovingCalled = false;
			((IList)Dummy.Collection).Remove(Dummy.Collection.AddNew());
			AssertEquals(true, Dummy.Collection.OnRemovingCalled);

			Dummy.Collection.OnRemovingCalled = false;
			Dummy.Collection.AddNew();
			((IList)Dummy.Collection).RemoveAt(0);
			AssertEquals(true, Dummy.Collection.OnRemovingCalled);
		}

		public void TestFactoryConstructor()
		{
			DummyBusinessObjectCollection dummy = new DummyBusinessObjectCollection(Factory);
			Assert(dummy != null);
		}

		public void TestFind()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyBusinessObjectCollection dummyCollection1 = new DummyBusinessObjectCollection(newFactory);
			for (int i = 0; i < 400; i++)
			{
				DummyBaseBusinessObject dummyBizo = dummyCollection1.AddNew();
				dummyBizo.Z0_Description = "Coffee";
				dummyBizo.Z0_Number = i;
			}
			newFactory.Save();

			ArrayList resultsFound;
			ZQuery filter;

			DummyBaseBusinessObject dummy1 = Dummy.Collection.AddNew();
			DummyBaseBusinessObject dummy2 = Dummy.Collection.AddNew();
			DummyBaseBusinessObject dummy3 = Dummy.Collection.AddNew();

			dummy1.Z0_Description = "Coffee";
			dummy1.Z0_Number = 10;
			dummy2.Z0_Description = "Coffee";
			dummy2.Z0_Number = 7;
			dummy3.Z0_Description = "Marsala";
			dummy3.Z0_Number = 10;

			resultsFound = new ArrayList(Dummy.Collection.Find(new ZQuery(DummyBizoSchema.Z0_Description, "Coffee")));
			AssertEquals("Should have found 2 BusinessObjects matching the ZQuery.", 2, resultsFound.Count);
			AssertEquals("Z0_Description is incorrect - Find() returned an incorrect result.", ((DummyBaseBusinessObject)resultsFound[0]).Z0_Description, "Coffee");
			AssertEquals("Z0_Description is incorrect - Find() returned an incorrect result.", ((DummyBaseBusinessObject)resultsFound[1]).Z0_Description, "Coffee");

			filter = new ZQuery(
					new ZQuery(DummyBizoSchema.Z0_Description, "Coffee"),
					new ZQuery(DummyBizoSchema.Z0_Number, 0));
			resultsFound = new ArrayList(Dummy.Collection.Find(filter));
			AssertEquals("Should have found 0 BusinessObjects matching the ZQuery.", 0, resultsFound.Count);

			filter = new ZQuery(
					new ZQuery(DummyBizoSchema.Z0_Description, "Coffee"),
					new ZQuery(DummyBizoSchema.Z0_Number, 7));
			resultsFound = new ArrayList(Dummy.Collection.Find(filter));
			AssertEquals("Should have found 1 BusinessObjects matching the ZQuery.", 1, resultsFound.Count);
			AssertEquals("Z0_Description is incorrect - Find() returned an incorrect result.", ((DummyBaseBusinessObject)resultsFound[0]).Z0_Description, "Coffee");
			AssertEquals("Z0_Number is incorrect - Find() returned an incorrect result.", ((DummyBaseBusinessObject)resultsFound[0]).Z0_Number, 7);

			filter.IsNoResultQuery = true;
			resultsFound = new ArrayList(Dummy.Collection.Find(filter));
			AssertEquals("No results returned for a NoResultQuery", 0, resultsFound.Count);
		}

		public void TestLoad()
		{
			DummyBusinessObjectCollection dummy = new DummyBusinessObjectCollection(Factory);

			dummy.Load();
			AssertEquals(1, dummy.Count);

			Factory.New(typeof(DummyBusinessObject));
			Factory.New(typeof(DummyBusinessObject));

			dummy.Load();

			AssertEquals(3, dummy.Count);
		}

		public void TestLoadWithZQuery()
		{
			DummyBusinessObjectCollection dummy = new DummyBusinessObjectCollection(Factory);

			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			dummy.Load(new ZQuery(DummyBizoSchema.Z0_Description, "COMRADE"));
			AssertEquals("Loaded object count", 1, dummy.Count);
		}

		public void TestLoadedFilterMatches()
		{
			DummyBusinessObjectCollection dummy = new DummyBusinessObjectCollection(Factory);
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Description, "COMRADE");
			AssertEquals("Loaded Filter Matches", false, dummy.LoadedFilterMatches(filter));

			dummy.Load(filter);
			AssertEquals("Loaded Filter Matches", true, dummy.LoadedFilterMatches(filter));
		}

		[TestDate(2022, 8, 1)]
		public void TestReloadWhenTimeout()
		{
			var dummy = new DummyBusinessObjectCollection(Factory);
			CombineAssertions(() =>
			{
				TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(30);
				dummy.HasReloadedFromDb = false;
				dummy.ReloadIfTimeout(false, 10);
				AssertEquals("should reload when have never reloaded before", true, dummy.HasReloadedFromDb);

				TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(30);
				dummy.HasReloadedFromDb = false;
				dummy.ReloadIfTimeout(false, 60);
				AssertEquals("should not reload when not timeout", false, dummy.HasReloadedFromDb);

				TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(30);
				dummy.ReloadIfTimeout(false, 10);
				AssertEquals("should reload when timeout", true, dummy.HasReloadedFromDb);
			});
		}

		public void TestAddFromDatabase()
		{
			DummyBusinessObjectCollection dummy = new DummyBusinessObjectCollection(Factory);

			Guid pK = Guid.NewGuid();
			Guid pK2 = Guid.NewGuid();
			DummyTableCreator.AddDummyBusinessObjectsToDB(pK, pK2);

			AssertEquals(0, dummy.Count);

			AssertEquals(true, dummy.AddFromDatabase(pK));
			AssertEquals(1, dummy.Count);

			AssertEquals(false, dummy.AddFromDatabase(Guid.Empty));
			AssertEquals(1, dummy.Count);

			AssertEquals(true, dummy.AddFromDatabase(pK2));
			AssertEquals(2, dummy.Count);

			AssertEquals(true, dummy.AddFromDatabase(pK2));
			AssertEquals(2, dummy.Count);
		}

		public void TestLastLoadedAdditionalFilterAddOptionRecompile()
		{
			var filter = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "78");
			filter.AddOptionRecompileConditionally = true;
			filter.OrderBy = DummyBizoSchema.Z0_Code.Name;
			var dummyCollection = new DummyBusinessObjectCollection(Factory, filter);
			dummyCollection.Load();
			Assert(dummyCollection.LastLoadedAdditionalFilter_Exposed.AddOptionRecompileConditionally);
		}

		public void TestAddFromDatabaseIfMatchLastLoadedFilter()
		{
			DummyBusinessObject dummy1 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			dummy1.Z0_Code = "789";

			DummyBusinessObject dummy2 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			dummy2.Z0_Code = "abc";

			Factory.Save();

			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "789");
			DummyBusinessObjectCollection dummyCollection = new DummyBusinessObjectCollection(Factory, filter);

			AssertEquals("AddFromDatabase", false, dummyCollection.AddFromDatabaseIfMatchLastLoadedFilter(dummy1.PK));
			AssertEquals("Count", 0, dummyCollection.Count);

			AssertEquals("AddFromDatabase", false, dummyCollection.AddFromDatabaseIfMatchLastLoadedFilter(dummy2.PK));
			AssertEquals("Count", 0, dummyCollection.Count);

			dummyCollection.Load();

			AssertEquals("AddFromDatabase with empty guid", false, dummyCollection.AddFromDatabaseIfMatchLastLoadedFilter(ZGuid.Empty));

			AssertEquals("AddFromDatabase", true, dummyCollection.AddFromDatabaseIfMatchLastLoadedFilter(dummy1.PK));
			AssertEquals("Count", 1, dummyCollection.Count);

			AssertEquals("AddFromDatabase", false, dummyCollection.AddFromDatabaseIfMatchLastLoadedFilter(dummy2.PK));
			AssertEquals("Count", 1, dummyCollection.Count);
		}

		public void TestAddFromDatabaseIfMatchLastLoadedFilter_WithMultiplePKs()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "789");
			DummyBusinessObjectCollection dummyCollection = new DummyBusinessObjectCollection(Factory, filter);
			dummyCollection.Load();

			DummyBusinessObject dummy1 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			dummy1.Z0_Code = "789";

			DummyBusinessObject dummy2 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			dummy2.Z0_Code = "abc";

			Factory.Save();

			bool? addedByDataRefresh = null;
			dummyCollection.CountChanged += delegate(object sender, CollectionCountChangedEventArgs e)
			{
				addedByDataRefresh = (bool)typeof(BusinessObjectCollection).GetField("IsUpdatingByDataRefreshBus", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dummyCollection);
			};

			AssertEquals("AddFromDatabase", false, dummyCollection.AddFromDatabaseIfMatchLastLoadedFilter(new ZGuid[] { dummy2.PK, ZGuid.Empty }));
			AssertEquals("Count", 0, dummyCollection.Count);

			AssertEquals("AddFromDatabase", true, dummyCollection.AddFromDatabaseIfMatchLastLoadedFilter(new ZGuid[] { dummy2.PK, dummy1.PK, ZGuid.Empty }));
			AssertEquals("IsUpdatingByDataRefreshBus should be true when adding for data refresh", true, addedByDataRefresh);
			AssertEquals("Count", 1, dummyCollection.Count);
		}

		public void TestGetPKs()
		{
			DummyBusinessObjectCollection dummy = new DummyBusinessObjectCollection(Factory);
			BusinessObject element = Factory.New(typeof(DummyBusinessObject));
			dummy.Add(element);
			List<ZGuid> result = dummy.GetPKs();
			AssertEquals(1, result.Count);
			AssertEquals(element.PK, result[0]);

			element = Factory.New(typeof(DummyBusinessObject));
			dummy.Add(element);
			result = dummy.GetPKs();
			AssertEquals(2, result.Count);
			AssertEquals(true, result.Contains(element.PK));
		}

		public void TestAdd()
		{
			DummyBusinessObjectCollection dummy = new DummyBusinessObjectCollection(Factory);
			AssertEquals(0, dummy.Count);

			BusinessObject element = Factory.New(typeof(DummyBusinessObject));

			dummy.Add(element);
			AssertEquals(1, dummy.Count);

			dummy.Add(element);
			AssertEquals(1, dummy.Count);
		}

		#region TestAddRaisesListChangedItemAddedEvent

		public void TestAddRaisesListChangedItemAddedEvent()
		{
			var listChangedEvents = new List<ListChangedEventArgs>();
			ListChangedEventHandler listChangedHandler = (o, e) => listChangedEvents.Add(e);

			var collection = new DummyCollectionWithRelationship(Factory);
			((IBindingList)collection).ListChanged += listChangedHandler;

			try
			{
				var element = Factory.New<DummyBusinessObject>();

				AssertEquals("Precondtion", 0, listChangedEvents.Count);

				collection.Add(element);

				AssertEquals("SetCollectionRelationship should have been called", 100, element.Z0_Number);

				AssertEquals(1, listChangedEvents.Count);
				AssertEquals(ListChangedType.ItemAdded, listChangedEvents[0].ListChangedType);
			}
			finally
			{
				((IBindingList)collection).ListChanged -= listChangedHandler;
			}
		}

		class DummyCollectionWithRelationship : DummyBusinessObjectCollection
		{
			public DummyCollectionWithRelationship(BusinessObjectFactory factory) : base(factory) { }

			protected internal override void SetCollectionRelationships(BusinessObject child)
			{
				base.SetCollectionRelationships(child);

				((DummyBusinessObject)child).Z0_Number = 100;
			}
		}

		#endregion

		[ExpectException(typeof(ArgumentNullException))]
		public void TestAddThrowsArgumentNullException()
		{
			DummyBusinessObjectCollection dummy = new DummyBusinessObjectCollection(Factory);
			dummy.Add(null);
		}

		public void TestAddElementsFromBusinessObjectCollection()
		{
			DummyBusinessObjectCollection dummy1 = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObjectCollection dummy2 = new DummyBusinessObjectCollection(Factory);
			AssertEquals(0, dummy1.Count);
			AssertEquals(0, dummy2.Count);

			dummy1.Add(Factory.New(typeof(DummyBusinessObject)));
			dummy1.Add(Factory.New(typeof(DummyBusinessObject)));
			AssertEquals(2, dummy1.Count);
			AssertEquals(0, dummy2.Count);

			dummy2.AddRange(dummy1);
			AssertEquals(2, dummy1.Count);
			AssertEquals(2, dummy2.Count);
		}

		public void TestSetDefaultsForNewChild()
		{
			Collection.AddNew();
			Assert(Collection[0].Z0_Description == "NowSet");
		}

		public void TestRemoveAndDeleteAll()
		{
			ZQuery filter = new ZQuery();
			filter.OrderBy = DummyBusinessObject.Schema.Z0_Code;
			DummyBusinessObjectCollectionForRemoveAndDeleteAll collection = new DummyBusinessObjectCollectionForRemoveAndDeleteAll(Factory, filter);
			DummyBusinessObject dummy1 = collection.AddNew();
			DummyBusinessObject dummy2 = collection.AddNew();
			dummy2.Z0_Code = "del";
			AssertEquals(2, collection.Count);

			DataRow row1 = ((INeedRow)dummy1).Row;
			row1.AcceptChanges(); // pretend we're in the DB

			DataRow row2 = ((INeedRow)dummy2).Row;

			collection.RemoveAndDeleteAll();
			AssertEquals(0, collection.Count);

			AssertEquals(DataRowState.Deleted, row1.RowState);
			AssertEquals(DataRowState.Detached, row2.RowState);
		}

		class DummyBusinessObjectCollectionForRemoveAndDeleteAll : DummyBusinessObjectCollection
		{
			public DummyBusinessObjectCollectionForRemoveAndDeleteAll(BusinessObjectFactory factory, ZQuery filter)
				: base(factory, filter)
			{
			}

			public new DummyBusinessObjectForRemoveAndDeleteAll this[int index]
			{
				get { return (DummyBusinessObjectForRemoveAndDeleteAll)base[index]; }
			}
		}

		class DummyBusinessObjectForRemoveAndDeleteAll : DummyBusinessObject
		{
			public DummyBusinessObjectForRemoveAndDeleteAll(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void Delete()
			{
				base.Delete();
				ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "del");
				DummyBusinessObject forDelete = (DummyBusinessObject)Factory.LoadTop1(typeof(DummyBusinessObject), filter);
				if (forDelete != null)
				{
					forDelete.Delete();
				}
			}
		}

		public void TestRemoveForDataRefresh()
		{
			var collection = new DummyBusinessObjectCollection(Factory);

			BusinessObjectCollection.ElementChangedHandler handler = (bizObj) => AssertEquals(true, collection.IsRefreshingByDataRefreshBus);

			var dummy = collection.AddNew();

			try
			{
				collection.ElementRemoving += handler;
				AssertEquals(false, collection.IsRefreshingByDataRefreshBus);
				collection.RemoveForDataRefresh(dummy);
			}
			finally
			{
				collection.ElementRemoving -= handler;
				AssertEquals(false, collection.IsRefreshingByDataRefreshBus);
			}
		}

		public void TestRemoveForDataRefresh_IsRefreshingByDataRefreshBusIsResetWhenExceptionIsRaisedDuringDeletion()
		{
			var collection = new DummyBusinessObjectCollection(Factory);

			BusinessObjectCollection.ElementChangedHandler handler = (bizObj) => { throw new Exception("test"); };

			var dummy = collection.AddNew();

			try
			{
				collection.ElementRemoving += handler;
				AssertEquals(false, collection.IsRefreshingByDataRefreshBus);
				AssertExceptionThrown("test", typeof(Exception), () => collection.RemoveForDataRefresh(dummy));
			}
			finally
			{
				collection.ElementRemoving -= handler;
				AssertEquals(false, collection.IsRefreshingByDataRefreshBus);
			}
		}

		public void TestRemoveAll()
		{
			Collection.AddNew();
			Collection.AddNew();
			AssertEquals(2, Collection.Count);

			Collection.RemoveAll();
			AssertEquals(0, Collection.Count);
		}

		public void TestRemoveDuringRemoveRange()
		{
			var el1 = Collection.AddNew();
			var el2 = Collection.AddNew();
			var el3 = Collection.AddNew();
			var el4 = Collection.AddNew();

			AssertEquals(4, Collection.Count);
			var removedDuringRemoveRange = false;
			Collection.ElementRemoving += x =>
			{
				if (x != el2)
				{
					Collection.Remove(el2);
					removedDuringRemoveRange = true;
				}
			};

			Collection.RemoveAll();
			AssertEquals(0, Collection.Count);
			Assert(removedDuringRemoveRange);
		}

		public void TestRemoveWithAPK()
		{
			Collection.RemoveAll();
			Collection.AddNew();
			AssertEquals("precondition", 1, Collection.Count);

			Collection.Remove(ZGuid.Empty);
			AssertEquals("should not have removed anything", 1, Collection.Count);

			DummyBaseBusinessObject newDummy = (DummyBaseBusinessObject)Factory.New(typeof(DummyBaseBusinessObject));
			Collection.Remove(newDummy.PK);
			AssertEquals("should not have removed anything", 1, Collection.Count);

			Collection.Remove(Collection[0].PK);
			AssertEquals("should have removed collection's only element", 0, Collection.Count);
		}

		public void TestHasChanges()
		{
			Collection.AddNew();
			Assert(!Collection.HasChanges);

			Collection[0].Z0_Description = "SomethingElse";
			Assert(Collection.HasChanges);

			// pretend we've written to DB
			Collection.HasChanges = false;
			Assert(!Collection.HasChanges);
		}

		public void TestAddNew()
		{
			AssertEquals(0, Collection.Count);
			Collection.AddNew();
			AssertEquals(1, Collection.Count);
		}

		public void TestAddNewWithTypeOf()
		{
			AssertEquals(0, Collection.Count);
			Collection.AddNew(typeof(DummyBusinessObject));
			AssertEquals(1, Collection.Count);
		}

		public void TestRefreshFromTableNoFilter()
		{
			AssertEquals(0, Collection.Count);

			Collection.Load(); // default dummy created in inherited test case will be loaded
			AssertEquals(1, Collection.Count);
		}

		#region TestCount

		public void TestCountChanged()
		{
			CountChangedCalled = 0;
			Collection.CountChanged += new CollectionCountChangedEventHandler(Collection_CountChanged);
			AssertEquals(0, CountChangedCalled);

			Collection.RemoveAll();
			AssertEquals(0, CountChangedCalled);

			Collection.AddNew();
			Assert(CountChangedCalled > 0);

			CountChangedCalled = 0;
			Collection.RemoveAll();
			Assert(CountChangedCalled > 0);
		}

		int CountChangedCalled;

		void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			CountChangedCalled++;
		}

		#endregion

		public void TestSort()
		{
			Assert(Collection.Count == 0);
			DummyBusinessObject b1 = Collection.AddNew();
			DummyBusinessObject b2 = Collection.AddNew();
			DummyBusinessObject b3 = Collection.AddNew();

			b1.Z0_Description = "A";
			b3.Z0_Description = "B";
			b2.Z0_Description = "C";

			AssertEquals(Collection[0], b1);
			AssertEquals(Collection[1], b2);
			AssertEquals(Collection[2], b3);

			Collection.Sort("Z0_Description", ListSortDirection.Ascending);
			AssertEquals(Collection[0], b1);
			AssertEquals(Collection[1], b3);
			AssertEquals(Collection[2], b2);

			Collection.Sort("Z0_Description", ListSortDirection.Descending);
			AssertEquals(Collection[0], b2);
			AssertEquals(Collection[1], b3);
			AssertEquals(Collection[2], b1);
		}

		public void TestIsInDatabaseIncludingChildrenWhenCollectionEmpty()
		{
			DummyBusinessObjectCollection tempObjectCollection = new DummyBusinessObjectCollection(Factory);
			AssertEquals("New collection should be empty.", 0, tempObjectCollection.Count);
			Assert("IsInDatabaseIncludingChildren of an empty collection should be True.", tempObjectCollection.IsInDatabaseIncludingChildren);
		}

		public void TestContains()
		{
			DummyBusinessObjectCollection tempObjectCollection = new DummyBusinessObjectCollection(Factory);

			BusinessObject bizO = tempObjectCollection.AddNew();
			AssertEquals(true, tempObjectCollection.Contains(bizO));
			AssertEquals(true, tempObjectCollection.Contains(bizO.PK));
			AssertEquals(false, tempObjectCollection.Contains(Factory.New(typeof(DummyBusinessObject))));
			AssertEquals(false, tempObjectCollection.Contains(ZGuid.NewZGuid()));
		}

		public void TestSuspendValidation()
		{
			DummyBusinessObjectCollection tempObjectCollection = new DummyBusinessObjectCollection(Factory);

			BusinessObject bizO = tempObjectCollection.AddNew();
			Assert(!bizO.IsValidationSuspended);
			Assert(!tempObjectCollection.IsValidationSuspended);

			tempObjectCollection.SuspendValidation();
			Assert(tempObjectCollection.IsValidationSuspended);
			Assert(bizO.IsValidationSuspended);

			tempObjectCollection.ResumeValidation();
			Assert(!bizO.IsValidationSuspended);
			Assert(!tempObjectCollection.IsValidationSuspended);
		}

		public void TestSuspendValidationPropagatesAfterAdd()
		{
			DummyBusinessObjectCollection tempObjectCollection = new DummyBusinessObjectCollection(Factory);

			Assert(!tempObjectCollection.IsValidationSuspended);

			tempObjectCollection.SuspendValidation();
			BusinessObject bizO = tempObjectCollection.AddNew();
			Assert(tempObjectCollection.IsValidationSuspended);
			Assert(bizO.IsValidationSuspended);

			tempObjectCollection.ResumeValidation();
			Assert(!bizO.IsValidationSuspended);
			Assert(!tempObjectCollection.IsValidationSuspended);
		}

		public void TestSortInformation()
		{
			AssertNull("No sort yet", Dummy.Collection.SortInformation);
			SortInfo sort = new SortInfo(AutoDummyBizo.Schema.Z0_Description, ListSortDirection.Ascending);
			Dummy.Collection.AddNew();
			Dummy.Collection.Sort(sort);
			AssertEquals("Should be sorted", sort, Dummy.Collection.SortInformation);
		}

		// So the save button doesn't get enabled when you make a new uncommitted row then cancel it in a grid.
		public void TestCancelEditDoesntModifyHasChanges()
		{
			DummyBusinessObject parent = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			DummyDependentBusinessObjectCollection dummies = new DummyDependentBusinessObjectCollection(parent, Factory);
			fDummies_ForTestCancelEditDoesntModifyHasChanges = dummies;
			dummies.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(OnHasChangesChanged_ForTestCancelEditDoesntModifyHasChanges);

			try
			{
				AssertEquals("No changes to collection initially", false, dummies.HasChanges);
				DummyDependantBusinessObject newElement = (DummyDependantBusinessObject)((IBindingList)dummies).AddNew();
				((ICancelAddNew)dummies).CancelNew(dummies.Count - 1);
				AssertEquals("No changes to collection after cancel edit", false, dummies.HasChanges);
			}
			finally
			{
				dummies.HasChangesChanged -= new EventHandler<HasChangesChangedEventArgs>(OnHasChangesChanged_ForTestCancelEditDoesntModifyHasChanges);
				fDummies_ForTestCancelEditDoesntModifyHasChanges = null;
			}
		}

		public void TestAdd_ItemIsValid_RaiseHasChangesChangedEvent()
		{
			var item = Factory.New(typeof(DummyBusinessObject));
			item.HasChanges = true;

			var collection = new DummyDependentBusinessObjectCollection(Factory);

			HasChangesChangedEventArgs eventArgs = null;
			collection.HasChangesChanged += (s, e) =>
			{
				eventArgs = e;
			};

			collection.Add(item);

			AssertEquals("HasChangesChanged event is raised", true, eventArgs != null);
			AssertEquals("Collection just was changed", true, eventArgs.ObjectJustWasChanged);
		}

		public void TestRemoveAndDelete_ItemIsInDatabase_RaiseHasChangesChangedEvent()
		{
			var item = Factory.New(typeof(DummyBusinessObject));
			var collection = new DummyDependentBusinessObjectCollection(Factory);
			collection.Add(item);

			Factory.Save();

			HasChangesChangedEventArgs eventArgs = null;
			collection.HasChangesChanged += (s, e) =>
			{
				eventArgs = e;
			};

			collection.RemoveAndDelete(item);

			AssertEquals("HasChangesChanged event is raised", true, eventArgs != null);
			AssertEquals("Collection just was changed", true, eventArgs.ObjectJustWasChanged);
		}

		public void TestCopyToList()
		{
			IList list = new ArrayList();
			list.Add(null);
			AssertEquals("Initial count", 1, list.Count);

			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory);
			dummies.CopyToList(list);
			AssertEquals("Count after adding empty list", 0, list.Count);

			dummies.Add(Factory.New(typeof(DummyBusinessObject)));
			dummies.CopyToList(list);
			AssertEquals("Count after adding 1", 1, list.Count);
			AssertEquals("List[0] after adding 1", dummies[0], list[0]);

			dummies.Add(Factory.New(typeof(DummyBusinessObject)));
			dummies.CopyToList(list);
			AssertEquals("Count after adding 2", 2, list.Count);
			AssertEquals("List[0] after adding 2", dummies[0], list[0]);
			AssertEquals("List[1] after adding 2", dummies[1], list[1]);
		}

		public void TestCopyToListWithStartIndex()
		{
			IList list = new ArrayList();
			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory);
			dummies.CopyToList(list, 0);
			AssertEquals("Count after adding empty list", 0, list.Count);

			dummies.CopyToList(list, -1);
			AssertEquals("Count after adding empty list", 0, list.Count);

			dummies.CopyToList(list, 1);
			AssertEquals("Count after adding empty list", 0, list.Count);

			dummies.Add(Factory.New(typeof(DummyBusinessObject)));
			dummies.Add(Factory.New(typeof(DummyBusinessObject)));
			dummies.Add(Factory.New(typeof(DummyBusinessObject)));
			dummies.Add(Factory.New(typeof(DummyBusinessObject)));

			dummies.CopyToList(list, 0);
			AssertEquals(4, list.Count);
			AssertEquals(dummies[0], list[0]);
			AssertEquals(dummies[1], list[1]);
			AssertEquals(dummies[2], list[2]);
			AssertEquals(dummies[3], list[3]);

			dummies.CopyToList(list, -1);
			AssertEquals(4, list.Count);
			AssertEquals(dummies[0], list[0]);
			AssertEquals(dummies[1], list[1]);
			AssertEquals(dummies[2], list[2]);
			AssertEquals(dummies[3], list[3]);

			dummies.CopyToList(list, 2);
			AssertEquals(2, list.Count);
			AssertEquals(dummies[2], list[0]);
			AssertEquals(dummies[3], list[1]);

			dummies.CopyToList(list, 4);
			AssertEquals(0, list.Count);

			dummies.CopyToList(list, 5);
			AssertEquals(0, list.Count);
		}

		public void TestCopyToListWithStartIndexAndNumberOfElementsToCopy()
		{
			IList list = new ArrayList();
			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory);
			dummies.CopyToList(list, 0, 0);
			AssertEquals("Count after adding empty list", 0, list.Count);

			dummies.CopyToList(list, 0, 1);
			AssertEquals("Count after adding empty list", 0, list.Count);

			dummies.CopyToList(list, -1, -1);
			AssertEquals("Count after adding empty list", 0, list.Count);

			dummies.CopyToList(list, -1, 1);
			AssertEquals("Count after adding empty list", 0, list.Count);

			dummies.CopyToList(list, 1, -1);
			AssertEquals("Count after adding empty list", 0, list.Count);

			dummies.CopyToList(list, 1, 1);
			AssertEquals("Count after adding empty list", 0, list.Count);

			dummies.Add(Factory.New(typeof(DummyBusinessObject)));
			dummies.Add(Factory.New(typeof(DummyBusinessObject)));
			dummies.Add(Factory.New(typeof(DummyBusinessObject)));
			dummies.Add(Factory.New(typeof(DummyBusinessObject)));

			// test overload with startIndex
			dummies.CopyToList(list, 0, 4);
			AssertEquals(4, list.Count);
			AssertEquals(dummies[0], list[0]);
			AssertEquals(dummies[1], list[1]);
			AssertEquals(dummies[2], list[2]);
			AssertEquals(dummies[3], list[3]);

			dummies.CopyToList(list, 0, 6);
			AssertEquals(4, list.Count);
			AssertEquals(dummies[0], list[0]);
			AssertEquals(dummies[1], list[1]);
			AssertEquals(dummies[2], list[2]);
			AssertEquals(dummies[3], list[3]);

			dummies.CopyToList(list, -1, 6);
			AssertEquals(4, list.Count);
			AssertEquals(dummies[0], list[0]);
			AssertEquals(dummies[1], list[1]);
			AssertEquals(dummies[2], list[2]);
			AssertEquals(dummies[3], list[3]);

			dummies.CopyToList(list, 2, 2);
			AssertEquals(2, list.Count);
			AssertEquals(dummies[2], list[0]);
			AssertEquals(dummies[3], list[1]);

			dummies.CopyToList(list, 2, 1);
			AssertEquals(1, list.Count);
			AssertEquals(dummies[2], list[0]);

			dummies.CopyToList(list, 4, -1);
			AssertEquals(0, list.Count);

			dummies.CopyToList(list, 4, 4);
			AssertEquals(0, list.Count);

			dummies.CopyToList(list, 3, 0);
			AssertEquals(0, list.Count);

			dummies.CopyToList(list, 3, 1);
			AssertEquals(1, list.Count);
			AssertEquals(dummies[3], list[0]);
		}

		public void TestRemoveAndDeleteWhenCollectionValidationIsSuspended()
		{
			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory);
			dummies.SuspendValidation();
			AssertEquals("Precondition - Collection ValidationIndex", 1, dummies.ValidationIndex);
			BusinessObject bizO = dummies.AddNew();
			AssertEquals("Precondition - Object ValidationIndex", 1, bizO.ValidationIndex);
			dummies.RemoveAndDeleteAll();
			AssertEquals("Object ValidationIndex after delete", 0, bizO.ValidationIndex);

			bizO = dummies.AddNew();
			AssertEquals("Precondition - Object ValidationIndex", 1, bizO.ValidationIndex);
			dummies.RemoveAllButLeaveRelationshipsIntact();
			AssertEquals("Object ValidationIndex when removed from collection", 0, bizO.ValidationIndex);
		}

		public void TestResumeValidationOnRemoved()
		{
			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory);
			dummies.SuspendValidation();
			AssertEquals("Precondition - Collection ValidationIndex", 1, dummies.ValidationIndex);

			BusinessObject bizO = dummies.AddNew();
			AssertEquals("Precondition - Object ValidationIndex", 1, bizO.ValidationIndex);

			dummies.Load();//RemoveAllButLeaveRelationshipsIntact() should reindex to zero and when added, it should be 1 again
			AssertEquals("Object ValidationIndex when removed from collection", 1, bizO.ValidationIndex);
		}

		public void TestModifyInEnumeratorCheckingForDelete()
			=> AssertNotifiesOfModificationWhileIterating((dummies, d) => dummies.RemoveAndDelete(d));

		public void TestModifyInEnumeratorCheckingForAdd()
			=> AssertNotifiesOfModificationWhileIterating((dummies, d) => dummies.AddNew());

		void AssertNotifiesOfModificationWhileIterating(Action<DummyBusinessObjectCollection, DummyBusinessObject> modify)
		{
			var dummies = new DummyBusinessObjectCollection(Factory);
			dummies.AddNew();
			dummies.AddNew();

			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				foreach (var d in dummies)
				{
					modify(dummies, (DummyBusinessObject)d);
				}
			});

			AssertContains("Should contain the stacktrace", nameof(AssertNotifiesOfModificationWhileIterating), ErrorReporter.LastMessageReported);
			AssertStartsWith("Error Report Prefix", "EnumerationCockUp_DummyBusinessObjectCollection", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		DummyDependentBusinessObjectCollection fDummies_ForTestCancelEditDoesntModifyHasChanges;
		void OnHasChangesChanged_ForTestCancelEditDoesntModifyHasChanges(object sender, HasChangesChangedEventArgs e)
		{
			AssertEquals("No committed changes ever should be performed in this test", false, fDummies_ForTestCancelEditDoesntModifyHasChanges.HasChanges);
		}

		#endregion

		#region TestSuspendSettingHasChanges

		public void TestSuspendSettingHasChanges_OnAdd()
		{
			AssertEquals("Prerequsite: collection has no changes after save", false, Collection.HasChanges);

			using (Collection.SuspendSettingHasChanges())
			{
				Collection.AddNew();
			}

			AssertEquals("collection has no changes after adding new element", false, Collection.HasChanges);
		}

		public void TestSuspendSettingHasChanges_OnChangedElement()
		{
			var element = Collection.AddNew();
			Factory.Save();

			AssertEquals("Prerequsite: collection has no changes after save", false, Collection.HasChanges);

			using (Collection.SuspendSettingHasChanges())
			{
				element.Z0_Description = "zzz";
			}

			AssertEquals("collection has changes after an element has changed", true, Collection.HasChanges);
		}

		public void TestSuspendSettingHasChanges_OnDelete()
		{
			var element = Collection.AddNew();

			Factory.Save();

			AssertEquals("Prerequsite: collection has no changes after save", false, Collection.HasChanges);

			using (Collection.SuspendSettingHasChanges())
			{
				Collection.RemoveAndDelete(element);
			}

			AssertEquals("Prerequsite: collection has no changes after deletion of an element", false, Collection.HasChanges);
		}

		#endregion

		#region AddNewUncomittedWithRelatedElementDontSetHasChanges

		public void TestAddAndDeleteUncommittedElementDontSetHasChanges()
		{
			var collection = new DummyBusinessObjectCollection(Factory);

			Assert("Precondition", !collection.HasChanges);

			var uncommitted = ((IBindingList)collection).AddNew();
			Assert(((IBusinessObjectInternals)uncommitted).IsUnCommittedRow);
			Assert(!collection.HasChanges);

			((ICancelAddNew)collection).CancelNew(((IList)collection).IndexOf(uncommitted));
			Assert(!collection.HasChanges);
		}

		public void TestAddNewUncomittedWithRelatedElementDontSetHasChanges()
		{
			var collection = new DummyWithRelatedCollection(Factory);

			Assert("Precondition", !collection.HasChanges);

			var dummy = (DummyWithRelated)((IBindingList)collection).AddNew();
			Assert(((IBusinessObjectInternals)dummy).IsUnCommittedRow);
			Assert(!collection.HasChanges);
			AssertEquals(1, dummy.Z0_Number);
			AssertEquals(2, dummy.RelatedNumber);
			Assert(!dummy.IsSettingHasChangesSuspended);
			Assert(!dummy.Related.IsSettingHasChangesSuspended);

			((ICancelAddNew)collection).CancelNew(((IList)collection).IndexOf(dummy));
			Assert(!collection.HasChanges);
		}

		class DummyWithRelated : DummyBusinessObject
		{
			public DummyWithRelated(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public DummyBusinessObject Related
			{
				get
				{
					if (related == null)
					{
						if (Z0_Guid.IsValid)
						{
							related = Factory.Load<DummyBusinessObject>(Z0_Guid);
						}
						if (related == null)
						{
							related = Factory.NewWithValidTestData<DummyBusinessObject>();
							Z0_Guid = related.PK;
							RegisterEditableChildObject(related);
							RegisterListChangedCalledRefreshBinding(related);
						}
					}
					return related;
				}
			}
			DummyBusinessObject related;

			public ZInt RelatedNumber
			{
				get { return Related.Z0_Number; }
				set { Related.Z0_Number = value; }
			}
		}

		class DummyWithRelatedCollection : BusinessObjectCollection<DummyWithRelated>
		{
			public DummyWithRelatedCollection(BusinessObjectFactory factory) : base(factory) { }

			protected internal override void SetDefaultsForNewChild(BusinessObject child)
			{
				base.SetDefaultsForNewChild(child);

				var dummy = (DummyWithRelated)child;
				dummy.Z0_Number = 1;
				dummy.RelatedNumber = 2;
			}
		}

		#endregion

		#region Implementation

		DummyBusinessObjectCollection Collection;

		protected override void SetUp()
		{
			base.SetUp();
			Collection = new DummyBusinessObjectCollection(Factory);
		}

		public class DummyComparer : IComparer<BusinessObject>
		{
			public int Compare(BusinessObject x, BusinessObject y)
			{
				DummyBusinessObject dummyX = (DummyBusinessObject)x;
				DummyBusinessObject dummyY = (DummyBusinessObject)y;

				int result = dummyX.Z0_Date.CompareTo(dummyY.Z0_Date);

				if (result == 0)
				{
					result = dummyX.Z0_Code.CompareTo(dummyY.Z0_Code);
				}

				return result;
			}
		}

		#endregion
	}

	#region BusinessObjectCollectionElementTypeForCollectionTypeTest

	sealed class BusinessObjectCollectionElementTypeForCollectionTypeTest : TestCase
	{
		public void TestElementTypeForNonGenericCollectionType()
		{
			AssertEquals(typeof(string), BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DummyStringCollection)));
			AssertEquals(typeof(Int32), BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DummyInt32Collection)));
		}

		public void TestElementTypeForGenericCollectionType()
		{
			Type elementType = BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(IDummyGenericBusinessObjectCollection<DummyBaseBusinessObject>));
			AssertEquals(typeof(DummyBaseBusinessObject), elementType);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestNonBusinessObjectCollectionThrows()
		{
			BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(ArrayList));
		}

		[ExpectNoExceptions]
		public void TestDynamicBusinessObjectCollectionOK()
		{
			AssertEquals(typeof(DynamicBusinessObject), BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DynamicBusinessObjectCollection<DynamicBusinessObject>)));
			AssertEquals(typeof(DynamicBusinessObject), BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DynamicBusinessObjectCollection)));
			AssertEquals(typeof(TestDynamicBusinessObject), BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DynamicBusinessObjectCollection<TestDynamicBusinessObject>)));
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestBusinessObjectCollectionThrows()
		{
			BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(BusinessObjectCollection));
		}

		public void TestCollectionWithNewdIndexer()
		{
			Type elementType = BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DummyCollectionWithNewdIndexerBase));
			AssertEquals(typeof(DummyBaseBusinessObject), elementType);

			Type baseElementType = BusinessObjectCollection.GetElementTypeFromCollectionType(typeof(DummyCollectionWithNewdIndexer));
			AssertEquals(typeof(DummyBusinessObject), baseElementType);
		}

		class TestDynamicBusinessObject : DynamicBusinessObject
		{
			public TestDynamicBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class DummyStringCollection : BusinessObjectCollection
		{
			public DummyStringCollection()
				: base(null)
			{
			}

			public string this[int index]
			{
				get { return null; }
			}
		}

		class DummyInt32Collection : BusinessObjectCollection
		{
			public DummyInt32Collection()
				: base(null)
			{
			}

			public int this[int index]
			{
				get { return 0; }
			}
		}

		class DummyCollectionWithNewdIndexerBase : BusinessObjectCollection
		{
			public DummyCollectionWithNewdIndexerBase()
				: base(null)
			{
			}

			public DummyBaseBusinessObject this[int index]
			{
				get { return null; }
			}
		}

		class DummyCollectionWithNewdIndexer : DummyCollectionWithNewdIndexerBase
		{
			public new DummyBusinessObject this[int index]
			{
				get { return null; }
			}
		}

		interface IDummyGenericBusinessObjectCollection<T> : IBusinessObjectCollection<T>
		{
		}
	}

	#endregion

	#region BusinessObjectCollection_IFindBoxListProviderTest

	sealed class BusinessObjectCollection_IFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestDescriptionFromCode()
		{
			string result = "";
			SetupDummiesForCodeMatching();

			result = ListProvider.DescriptionFromCode("AA");
			AssertEquals("Description on BusinessObject that meets filter criteria should be returned first", "Dummy1", result);

			Dummy1.Z0_Code = "AB";

			result = ListProvider.DescriptionFromCode("AA");
			AssertEquals("Description on BusinessObject that doesn't meet filter criteria should be returned only if BusinessObject that meets filter criteria cannot be found", "Dummy2", result);
		}

		public void TestPrimaryKeyFromCode()
		{
			ZGuid result = ZGuid.Empty;
			SetupDummiesForCodeMatching();

			result = ListProvider.PrimaryKeyFromCode("AA");
			AssertEquals("PK on BusinessObject that meets filter criteria should be returned first", Dummy1.PK, result);

			Dummy1.Z0_Code = "AB";

			result = ListProvider.PrimaryKeyFromCode("AA");
			AssertEquals("ZGuid.Missing should be returned if BusinessObject that meets filter criteria cannot be found", ZGuid.Missing, result);
		}

		public void TestNearestMatch_WithoutListProvider()
		{
			const string DummyCode = "DummyCode";

			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory);
			AssertEquals("No match should return same code", DummyCode, ((IFindBoxListProvider)dummies).NearestMatch(DummyCode, true, -1).Item1);
		}

		public void TestNearestMatch_WithListProvider_Filtered()
		{
			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, "Z0"));

			DummyBusinessObject dummy1 = dummies.AddNew();
			dummy1.Z0_Code = "A";

			DummyBusinessObject dummy2 = dummies.AddNew();
			dummy2.Z0_Code = "AA";

			DummyBusinessObject dummy3 = dummies.AddNew();
			dummy3.Z0_Code = "Z0";

			AssertEquals("Missing code should return same code", "B", ((IFindBoxListProvider)dummies).NearestMatch("B", true, -1).Item1);
			AssertEquals("Missing code should return same code", "AB", ((IFindBoxListProvider)dummies).NearestMatch("AB", true, -1).Item1);
			AssertEquals("A", ((IFindBoxListProvider)dummies).NearestMatch("A", true, -1).Item1);
			AssertEquals("AA", ((IFindBoxListProvider)dummies).NearestMatch("AA", true, -1).Item1);
			AssertEquals("'", ((IFindBoxListProvider)dummies).NearestMatch("'", true, -1).Item1);
			AssertEquals("A", ((IFindBoxListProvider)dummies).NearestMatch("", true, -1).Item1);
			AssertEquals("Existing Object but not in List should return as missing", "Z", ((IFindBoxListProvider)dummies).NearestMatch("Z", true, -1).Item1);
		}

		public void TestNearestMatch_WithListProvider_Unfiltered()
		{
			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory);

			DummyBusinessObject dummy1 = dummies.AddNew();
			dummy1.Z0_Code = "A";

			DummyBusinessObject dummy2 = dummies.AddNew();
			dummy2.Z0_Code = "AA";

			DummyBusinessObject dummy3 = dummies.AddNew();
			dummy3.Z0_Code = "Z0";

			DummyBusinessObject dummy4 = dummies.AddNew();
			dummy4.Z0_Code = "X=Y";

			DummyBusinessObject dummy5 = dummies.AddNew();
			dummy5.Z0_Code = "XAA";

			AssertEquals("Missing code should return same code", "B", ((IFindBoxListProvider)dummies).NearestMatch("B", true, -1).Item1);
			AssertEquals("Missing code should return same code", "AB", ((IFindBoxListProvider)dummies).NearestMatch("AB", true, -1).Item1);
			AssertEquals("A", ((IFindBoxListProvider)dummies).NearestMatch("A", true, -1).Item1);
			AssertEquals("AA", ((IFindBoxListProvider)dummies).NearestMatch("AA", true, -1).Item1);
			AssertEquals("'", ((IFindBoxListProvider)dummies).NearestMatch("'", true, -1).Item1);
			AssertEquals("A", ((IFindBoxListProvider)dummies).NearestMatch("", true, -1).Item1);
			AssertEquals("Match even though not in collection", "Z0", ((IFindBoxListProvider)dummies).NearestMatch("Z", true, -1).Item1);
			AssertEquals("Test literal equals", "X=", ((IFindBoxListProvider)dummies).NearestMatch("X", true, 1).Item1);

			DummyBusinessObject dummy6 = dummies.AddNew();
			dummy6.Z0_Code = "=ZZ";

			AssertEquals("Test literal equals", "=", ((IFindBoxListProvider)dummies).NearestMatch("", true, 0).Item1);
		}

		public void TestAutoCompleteOnCommit()
		{
			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory);
			AssertEquals("False by default", false, ((IFindBoxListProvider)dummies).AutoCompleteOnCommit);

			DummyBusinessObjectCollectionWithAutoCompleteOnComit dummiesWithAutoComplete = new DummyBusinessObjectCollectionWithAutoCompleteOnComit(Factory);
			AssertEquals("True if listprovider says true", true, ((IFindBoxListProvider)dummiesWithAutoComplete).AutoCompleteOnCommit);
		}

		class DummyBusinessObjectCollectionWithAutoCompleteOnComit : DummyBusinessObjectCollection
		{
			public DummyBusinessObjectCollectionWithAutoCompleteOnComit(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override IFindBoxListProvider FindBoxListProvider
			{
				get { return new DummyBusinessObjectFindBoxListProviderWithAutoCompleteOnCommit(this); }
			}

			class DummyBusinessObjectFindBoxListProviderWithAutoCompleteOnCommit : FindBoxListProvider
			{
				public DummyBusinessObjectFindBoxListProviderWithAutoCompleteOnCommit(BusinessObjectCollection collection)
					: base(collection)
				{
				}

				public override bool AutoCompleteOnCommit
				{
					get { return true; }
				}
			}
		}

		public void TestIsManagedForDataRefresh()
		{
			DummyBusinessObjectCollection dummyCollection = new DummyBusinessObjectCollection(Factory);
			dummyCollection.Load();
			AssertEquals("IsManagedForDataRefresh", false, dummyCollection.IsManagedForDataRefresh);
			dummyCollection.IsManagedForDataRefresh = true;
			AssertEquals("IsManagedForDataRefresh", true, dummyCollection.IsManagedForDataRefresh);

			AssertEquals("DummyCollection.Count", 0, dummyCollection.Count);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy3 = (DummyBusinessObject)factory2.New(typeof(DummyBusinessObject));
			dummy3.Z0_Code = "123";
			factory2.Save();

			AssertEquals("DummyCollection.Count", 1, dummyCollection.Count);

			dummyCollection.IsManagedForDataRefresh = false;
			AssertEquals("IsManagedForDataRefresh", false, dummyCollection.IsManagedForDataRefresh);

			DummyBusinessObject dummy4 = (DummyBusinessObject)factory2.New(typeof(DummyBusinessObject));
			dummy4.Z0_Code = "333";
			factory2.Save();

			AssertEquals("DummyCollection.Count", 1, dummyCollection.Count);
		}

		public void TestDeletingBizObjectFromCollectionWillMakeCollectionChanged()
		{
			DummyBusinessObject dummy1 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			Factory.Save();

			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory);
			dummies.Load();

			AssertEquals("HasChanges", false, dummies.HasChanges);
			dummy1.Delete();
			AssertEquals("HasChanges", true, dummies.HasChanges);
		}

		public void TestDelete_BusinessObjectWithMaxCountNotificationErrorInCollection_Ok()
		{
			DummyBusinessObject dummyBizo = Factory.New<DummyBusinessObject>();
			var testCollection = new DummyViewCollection(dummyBizo.Collection);
			var testBO = dummyBizo.Collection.AddNew();
			testBO.Z0_Description = "INVIEW";
			testCollection.MaxCountValidationEnable(0);

			Assert("Business object should be in view", testCollection.Contains(testBO));
			Assert("Business object should be in filtered collection", dummyBizo.Collection.Contains(testBO));

			testBO.Delete();

			Assert("Business object should not be in view", !testCollection.Contains(testBO));
			Assert("Business object should not be in filtered collection", !dummyBizo.Collection.Contains(testBO));
		}

		public void TestRemoveAndDeleteWillMakeCollectionChanged()
		{
			DummyBusinessObject dummy1 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			Factory.Save();

			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory);
			((IBindingList)dummies).ListChanged += new ListChangedEventHandler(RemoveAndDeleteWillMakeCollectionChanged_ListChanged);
			dummies.Load();

			NewIndex = -1;
			AssertEquals("HasChanges", false, dummies.HasChanges);
			dummies.RemoveAndDelete(dummy1);
			AssertEquals("HasChanges", true, dummies.HasChanges);
			AssertEquals("NewIndex must be 0, otherwise exception will be thrown when deleting from Grid.", 0, NewIndex);
		}

		public void TestValidationIsNotSuspendedAfterAddingObjectToCollection()
		{
			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			AssertEquals("Precondition", false, dummy.IsValidationSuspended);
			dummies.Add(dummy);
			AssertEquals("IsValidationSuspended", false, dummy.IsValidationSuspended);
		}

		void RemoveAndDeleteWillMakeCollectionChanged_ListChanged(object sender, ListChangedEventArgs e)
		{
			NewIndex = e.NewIndex;
		}

		int NewIndex = -1;

		#region Implementation

		DummyBusinessObject Dummy1;
		DummyBusinessObject Dummy2;
		DummyBusinessObject Dummy3;
		IFindBoxListProvider ListProvider;

		void SetupDummiesForCodeMatching()
		{
			DummyBusinessObjectCollection dummies = new DummyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.Equal, 5));
			ListProvider = dummies;
			AssertNotNull("BusinessObjectCollection should implement IFindBoxListProvider", ListProvider);

			Dummy1 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			Dummy1.Z0_Code = "AA";
			Dummy1.Z0_Number = 5;
			Dummy1.Z0_Description = "Dummy1";

			Dummy2 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			Dummy2.Z0_Code = "AA";
			Dummy2.Z0_Number = 4;
			Dummy2.Z0_Description = "Dummy2";

			Dummy3 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			Dummy3.Z0_Code = "BB";
			Dummy3.Z0_Number = 5;
			Dummy3.Z0_Description = "Dummy3";
		}

		#endregion
	}

	#endregion
}
