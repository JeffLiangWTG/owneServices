using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectCollectionViewTest : SubsetBusinessObjectCollectionTestCase<DummyBaseBusinessObject>
	{
		#region Hook and Unhook collection

		DummyBusinessObject CreateDummyForView(DummyChildBusinessObjectCollection collection, int id)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "INVIEW";
			dummy.Z0_Number = id;
			collection.Add(dummy);
			return dummy;
		}

		public void TestAllowUnhookToAvoidMemoryLeak()
		{
			var testCollection = GetNewDummyViewCollection(Dummy.Collection);
			CreateDummyForView(Dummy.Collection, 1);
			CreateDummyForView(Dummy.Collection, 2);
			CreateDummyForView(Dummy.Collection, 3);
			var rebuilds = 0;
			testCollection.OnRebuild += (s, e) => rebuilds++;

			Dummy.Collection.FireListResetEventInternal();
			AssertEquals(1, rebuilds);

			Dummy.Collection.FireListResetEventInternal();
			AssertEquals(2, rebuilds);
			Assert(testCollection.IsHooked);

			testCollection.UnhookCollection();
			Dummy.Collection.FireListResetEventInternal();
			Assert(!testCollection.IsHooked);
			AssertEquals("We don't need to rebuild this collection any more.", 2, rebuilds);

			testCollection.HookCollection();
			Dummy.Collection.FireListResetEventInternal();
			AssertEquals("Test that it can be re-hooked.", 3, rebuilds);
		}

		[ExpectNoExceptions]
		public void TestUnHook_Idempotent()
		{
			var testCollection = GetNewDummyViewCollection(Dummy.Collection);
			testCollection.UnhookCollection();
			testCollection.UnhookCollection();
			testCollection.UnhookCollection();
			testCollection.UnhookCollection();
		}

		public void TestHook_Idempotent()
		{
			var testCollection = GetNewDummyViewCollection(Dummy.Collection);
			CreateDummyForView(Dummy.Collection, 1);
			CreateDummyForView(Dummy.Collection, 2);
			CreateDummyForView(Dummy.Collection, 3);
			var rebuilds = 0;
			testCollection.OnRebuild += (s, e) => rebuilds++;
			testCollection.HookCollection();
			testCollection.HookCollection();
			testCollection.HookCollection();
			Dummy.Collection.FireListResetEventInternal();
			AssertEquals(1, rebuilds);
		}

		public void TestUnhook_IsFaultTolerant()
		{
			var testCollection = GetNewDummyViewCollection(Dummy.Collection);
			testCollection.UnhookCollection();

			CreateDummyForView(Dummy.Collection, 1);
			CreateDummyForView(Dummy.Collection, 2);
			CreateDummyForView(Dummy.Collection, 3);

			var count = 0;
			testCollection.ForEach(i => count++); // We enumerate the collection.
			AssertEquals("I anticipate that someone will do this some day, so here is our protection.", 3, count);
			AssertStartsWith("We should not write code unless we know what to expect from it.", "Should not enumerate a deliberately unhooked collection of type", ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
		}

		#endregion

		public void TestAllowNew()
		{
			DummyViewCollection testCollection = GetNewDummyViewCollection(Dummy.Collection);
			Dummy.Collection.SetAllowNew(false);
			AssertEquals("AllowNew proxied", Dummy.Collection.AllowNew, testCollection.AllowNew);

			Dummy.Collection.SetAllowNew(true);
			AssertEquals("AllowNew proxied", Dummy.Collection.AllowNew, testCollection.AllowNew);
		}

		public void TestSortingParentCollectionDoesNotRebuildView()
		{
			var collection = Dummy.Collection;
			var viewCollection = GetNewDummyViewCollection(collection);

			var rebuilds = 0;
			viewCollection.OnRebuild += (s, e) => rebuilds++;
			collection.Sort("Z0_Code");

			AssertEquals(0, rebuilds);
		}

		public void TestReadOnly()
		{
			DummyChildBusinessObjectCollection collection = Dummy.Collection;
			DummyViewCollection viewCollection = GetNewDummyViewCollection(collection);

			AssertEquals(false, collection.ReadOnly);
			AssertEquals(false, viewCollection.ReadOnly);

			collection.SetReadOnly(true);
			AssertEquals(true, collection.ReadOnly);
			AssertEquals(true, viewCollection.ReadOnly);

			collection.SetReadOnly(false);
			AssertEquals(false, collection.ReadOnly);
			AssertEquals(false, viewCollection.ReadOnly);

			viewCollection.SetReadOnlyIncludingChildren(true);
			AssertEquals(false, collection.ReadOnly);
			AssertEquals(true, viewCollection.ReadOnly);

			viewCollection.SetReadOnlyIncludingChildren(false);
			AssertEquals(false, collection.ReadOnly);
			AssertEquals(false, viewCollection.ReadOnly);
		}

		public void TestMastersAreInDatabase()
		{
			DummyViewCollection testCollection = GetNewDummyViewCollection(Dummy.Collection);
			Dummy.SetIsInDatabase(false);
			AssertEquals("Dummy master not in DB", false, ((IBusinessObjectCollectionInternals)testCollection).MastersAreInDatabase);
			Dummy.SetIsInDatabase(true);
			AssertEquals("Dummy master in DB", true, ((IBusinessObjectCollectionInternals)testCollection).MastersAreInDatabase);
		}

		public void TestAddToFilteredCollection()
		{
			DummyViewCollection testCollection = GetNewDummyViewCollection(Dummy.Collection);
			DummyBusinessObject filteredBO = Factory.New<DummyBusinessObject>();
			Dummy.Collection.Add(filteredBO);
			Assert("Business object should not be in view", !testCollection.Contains(filteredBO));
			AssertEquals("No objects should be in the list", 0, testCollection.Count);

			DummyBusinessObject testBO = Factory.New<DummyBusinessObject>();
			testBO.Z0_Description = "INVIEW";
			Dummy.Collection.Add(testBO);
			Assert("Business object should be in view", testCollection.Contains(testBO));
			AssertEquals("1 object should be in the list", 1, testCollection.Count);
		}

		public void TestViewSuspendRemovingWhenOnlyOneElementLeftForListChanged()
		{
			var testCollection = GetNewDummyViewCollection(Dummy.Collection);
			var filteredBO = Factory.New<DummyBusinessObject>();

			testCollection.ElementsForTest.Add(filteredBO);
			filteredBO.Z0_Description = "Inview";
			Dummy.Collection.Add(filteredBO);

			using (testCollection.SuspendRemovingWhenOnlyOneElementLeftForListChanged())
			{
				filteredBO.Z0_Description = "NotInview";

				Assert("Business object should be in view", testCollection.Contains(filteredBO));
				AssertEquals("1 objects should be in the list", 1, testCollection.Count);
			}
		}

		public void TestViewNotSuspendRemovingWhenOnlyOneElementLeftForListChanged()
		{
			var testCollection = GetNewDummyViewCollection(Dummy.Collection);
			var filteredBO = Factory.New<DummyBusinessObject>();

			testCollection.ElementsForTest.Add(filteredBO);
			filteredBO.Z0_Description = "Inview";
			Dummy.Collection.Add(filteredBO);
			filteredBO.Z0_Description = "NotInview";

			Assert("Business object should not be in view", !testCollection.Contains(filteredBO));
			AssertEquals("no objects should be in the list", 0, testCollection.Count);
		}

		public void TestWhenAccessingDeletedBusinessObject()
		{
			DummyViewCollection testCollection = GetNewDummyViewCollection(Dummy.Collection);
			DummyBusinessObject testBO = Factory.New<DummyBusinessObject>();
			testBO.Delete();
			Dummy.Collection.Add(testBO);
			Assert("Deleted TestBO has been cached and has attempted to be accessed", testBO.IsDeleted);
		}

		public void TestTimeComparerForSort()
		{
			var collection = new DummyBusinessObjectInListList(Factory);
			var testCollectionView = GetNewDummyViewCollection(collection);
			var bizO1 = Factory.NewWithValidTestData<DummyBusinessObjectInList>();
			var bizO2 = Factory.NewWithValidTestData<DummyBusinessObjectInList>();
			var bizO3 = Factory.NewWithValidTestData<DummyBusinessObjectInList>();
			bizO1.Z0_SmallDateTime = new ZDateTime(2022, 1, 1, 0, 1, 0);
			bizO1.Z0_Description = "INVIEW";
			bizO2.Z0_SmallDateTime = new ZDateTime(2020, 1, 1, 0, 2, 0);
			bizO2.Z0_Description = "INVIEW";
			bizO3.Z0_SmallDateTime = new ZDateTime(2021, 1, 1, 0, 3, 0);
			bizO3.Z0_Description = "INVIEW";
			testCollectionView.Add(bizO1);
			testCollectionView.Add(bizO2);
			testCollectionView.Add(bizO3);

			Factory.Save();

			testCollectionView.AddIsTime(DummyBizoSchema.Z0_SmallDateTime.Name);
			testCollectionView.Sort(DummyBizoSchema.Z0_SmallDateTime.Name, ListSortDirection.Ascending);

			AssertEquals(bizO1.PK, testCollectionView[0].PK);
			AssertEquals(bizO2.PK, testCollectionView[1].PK);
			AssertEquals(bizO3.PK, testCollectionView[2].PK);

			testCollectionView.Sort(DummyBizoSchema.Z0_SmallDateTime.Name, ListSortDirection.Descending);

			AssertEquals(bizO3.PK, testCollectionView[0].PK);
			AssertEquals(bizO2.PK, testCollectionView[1].PK);
			AssertEquals(bizO1.PK, testCollectionView[2].PK);
		}

		public void TestBusinessObjectCollectionViewApplySortOnGuidColumnCorrectly()
		{
			var collection = new DummyBusinessObjectInListList(Factory);
			var testCollectionView = GetNewDummyViewCollection(collection);

			for (var i = 0; i < 10; i++)
			{
				var dummy = Factory.NewWithValidTestData<DummyBusinessObjectInList>();
				dummy.Z0_Code = (9 - i).ToString();
				dummy.Z0_Description = "INVIEW";
				dummy.Z0_Guid = dummy.PK;
				dummy.LookupList.Add(dummy);
				testCollectionView.Add(dummy);
			}

			Factory.Save();

			testCollectionView.AddGuidListMapping(DummyBizoSchema.Z0_Guid.Name, "LookupList");
			testCollectionView.Sort(DummyBizoSchema.Z0_Guid.Name, ListSortDirection.Ascending);

			for (var i = 0; i < 10; i++)
			{
				AssertEquals(i.ToString(), testCollectionView[i].Z0_Code);
			}
		}

		public void TestCollectionAndViewSortTogether()
		{
			DummyViewCollection testCollection = GetNewDummyViewCollection(Dummy.Collection);
			Dummy.Collection.RemoveAndDeleteAll();
			AssertEquals(0, Dummy.Collection.Count);
			DummyBusinessObject bO1 = Factory.New<DummyBusinessObject>();
			Dummy.Collection.Add(bO1);
			bO1.Z0_Description = "INVIEW";
			bO1.Z0_Number = 10;
			bO1.Z0_DateTimeOffset = new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.Zero);
			AssertEquals(1, Dummy.Collection.Count);

			DummyBusinessObject bO2 = Factory.New<DummyBusinessObject>();
			Dummy.Collection.Add(bO2);
			bO2.Z0_Description = "INVIEW";
			bO2.Z0_Number = 5;
			bO2.Z0_DateTimeOffset = new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.FromHours(1));

			DummyBusinessObject bO3 = Factory.New<DummyBusinessObject>();
			Dummy.Collection.Add(bO3);
			bO3.Z0_Description = "INVIEW";
			bO3.Z0_Number = 1;
			bO3.Z0_DateTimeOffset = new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.FromHours(2));
			AssertEquals(3, Dummy.Collection.Count);

			Factory.Save();
			AssertEquals(3, Dummy.Collection.Count);

			testCollection.ApplySortCore(new ListSortDescriptionCollection(new ListSortDescription[] { new ListSortDescription(bO1.GetPropertyDescriptor("Z0_Number"), ListSortDirection.Ascending) }));

			Dummy.Collection.Load();
			AssertEquals(4, Dummy.Collection.Count);
			AssertEquals(3, testCollection.Count);

			AssertEquals(0, Dummy.Collection[0].Z0_Number);
			AssertEquals(1, Dummy.Collection[1].Z0_Number);
			AssertEquals(5, Dummy.Collection[2].Z0_Number);
			AssertEquals(10, Dummy.Collection[3].Z0_Number);
			AssertEquals(1, testCollection[0].Z0_Number);
			AssertEquals(5, testCollection[1].Z0_Number);
			AssertEquals(10, testCollection[2].Z0_Number);

			Dummy.Collection[0].Z0_DateTimeOffset = new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.FromHours(3));

			testCollection.ApplySortCore(new ListSortDescriptionCollection(new ListSortDescription[] { new ListSortDescription(bO1.GetPropertyDescriptor("Z0_Number"), ListSortDirection.Descending) }));

			Dummy.Collection.Load();
			AssertEquals(4, Dummy.Collection.Count);

			AssertEquals(10, Dummy.Collection[0].Z0_Number);
			AssertEquals(5, Dummy.Collection[1].Z0_Number);
			AssertEquals(1, Dummy.Collection[2].Z0_Number);
			AssertEquals(0, Dummy.Collection[3].Z0_Number);
			AssertEquals(10, testCollection[0].Z0_Number);
			AssertEquals(5, testCollection[1].Z0_Number);
			AssertEquals(1, testCollection[2].Z0_Number);

			testCollection.ApplySortCore(new ListSortDescriptionCollection(new ListSortDescription[] { new ListSortDescription(bO1.GetPropertyDescriptor("Z0_DateTimeOffset"), ListSortDirection.Ascending) }));

			Dummy.Collection.Load();
			AssertEquals(4, Dummy.Collection.Count);
			AssertEquals(3, testCollection.Count);

			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.FromHours(3)), Dummy.Collection[0].Z0_DateTimeOffset);
			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.FromHours(2)), Dummy.Collection[1].Z0_DateTimeOffset);
			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.FromHours(1)), Dummy.Collection[2].Z0_DateTimeOffset);
			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.Zero), Dummy.Collection[3].Z0_DateTimeOffset);
			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.FromHours(2)), testCollection[0].Z0_DateTimeOffset);
			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.FromHours(1)), testCollection[1].Z0_DateTimeOffset);
			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.Zero), testCollection[2].Z0_DateTimeOffset);

			testCollection.ApplySortCore(new ListSortDescriptionCollection(new ListSortDescription[] { new ListSortDescription(bO1.GetPropertyDescriptor("Z0_DateTimeOffset"), ListSortDirection.Descending) }));

			Dummy.Collection.Load();
			AssertEquals(4, Dummy.Collection.Count);
			AssertEquals(3, testCollection.Count);

			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.Zero), Dummy.Collection[0].Z0_DateTimeOffset);
			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.FromHours(1)), Dummy.Collection[1].Z0_DateTimeOffset);
			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.FromHours(2)), Dummy.Collection[2].Z0_DateTimeOffset);
			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.FromHours(3)), Dummy.Collection[3].Z0_DateTimeOffset);
			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.Zero), testCollection[0].Z0_DateTimeOffset);
			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.FromHours(1)), testCollection[1].Z0_DateTimeOffset);
			AssertEquals(new DateTimeOffset(2000, 02, 02, 02, 30, 40, TimeSpan.FromHours(2)), testCollection[2].Z0_DateTimeOffset);
		}

		public void TestSwapCollectionCallsRebuild()
		{
			DummyBusinessObjectCollection collection1 = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObjectCollection collection2 = new DummyBusinessObjectCollection(Factory);

			DummyBusinessObject child1 = collection1.AddNew();
			DummyBusinessObject child2 = collection2.AddNew();

			child1.Z0_Description = "INVIEW";
			child2.Z0_Description = "INVIEW";

			DummyViewCollection testCollection = GetNewDummyViewCollection(collection1);
			AssertEquals(1, testCollection.Count);
			AssertEquals(child1, testCollection[0]);

			testCollection.SwapCollectionToFilter(collection2);
			AssertEquals(1, testCollection.Count);
			AssertEquals(child2, testCollection[0]);
		}

		public void TestElementChangedInFilteredCollection()
		{
			DummyViewCollection testCollection = GetNewDummyViewCollection(Dummy.Collection);
			DummyBaseBusinessObject filteredBO = Dummy.Collection.AddNew();
			Assert("Business object should not be in view", !testCollection.Contains(filteredBO));
			AssertEquals("No objects should be in the list", 0, testCollection.Count);

			filteredBO.Z0_Description = "INVIEW";
			Assert("Business object should now be in view", testCollection.Contains(filteredBO));
			AssertEquals("1 object should be in the list", 1, testCollection.Count);

			filteredBO.Z0_Description = "NOTINVIEW";
			Assert("Business object should not be in view", !testCollection.Contains(filteredBO));
			AssertEquals("No objects should be in the list", 0, testCollection.Count);
		}

		public void TestAddToViewCollection()
		{
			DummyViewCollection testView = GetNewDummyViewCollection(Dummy.Collection);
			DummyBusinessObject testBO = Factory.New<DummyBusinessObject>();
			testBO.Z0_Description = "INVIEW";

			testView.Add(testBO);
			Assert("Business object should be in view collection", testView.Contains(testBO));
			Assert("Business object should be in filtered collection", Dummy.Collection.Contains(testBO));

			DummyBusinessObject filteredBO = Factory.New<DummyBusinessObject>();
			filteredBO.Z0_Description = "NOTINVIEW";
			testView.Add(filteredBO);
			Assert("Business object should not be in view collection", !testView.Contains(filteredBO));
			Assert("Business object should be in filtered collection", Dummy.Collection.Contains(filteredBO));
		}

		public void TestRemoveFromViewCollection()
		{
			var testCollection = GetNewDummyViewCollection(Dummy.Collection);
			var testBO = Dummy.Collection.AddNew();
			testBO.Z0_Description = "INVIEW";
			Assert("Business object should be in view", testCollection.Contains(testBO));
			Assert("Business object should be in filtered collection", Dummy.Collection.Contains(testBO));

			testCollection.Remove(testBO);
			Assert("Business object should not be in view", !testCollection.Contains(testBO));
			Assert("Business object should not be in filtered collection", !Dummy.Collection.Contains(testBO));
		}

		public void TestRemoveFromFilteredCollection()
		{
			var testCollection = GetNewCollection(Dummy.Collection);
			var testBO = Dummy.Collection.AddNew();
			testBO.Z0_Description = "INVIEW";
			Assert("Business object should be in view", testCollection.Contains(testBO));
			Assert("Business object should be in filtered collection", Dummy.Collection.Contains(testBO));

			Dummy.Collection.Remove(testBO);
			Assert("Business object should not be in view", !testCollection.Contains(testBO));
			Assert("Business object should not be in filtered collection", !Dummy.Collection.Contains(testBO));

			var testFilteredBO = (DummyBaseBusinessObject)Dummy.Collection.AddNew();
			testFilteredBO.Z0_Description = "NOTINVIEW";
			Assert("Business object should not be in view", !testCollection.Contains(testFilteredBO));
			Assert("Business object should be in filtered collection", Dummy.Collection.Contains(testFilteredBO));

			Dummy.Collection.Remove(testFilteredBO);
			Assert("Business object should not be in view", !testCollection.Contains(testFilteredBO));
			Assert("Business object should not be in filtered collection", !Dummy.Collection.Contains(testFilteredBO));
		}

		public void TestListResetEventIsPassedUp()
		{
			var collectionView = GetNewDummyViewCollection(Dummy.Collection);

			ListChangedEventArgs lastListChanged = null;
			((IBindingList)collectionView).ListChanged += (sender, e) => lastListChanged = e;

			((IBusinessObjectCollectionInternals)Dummy.Collection).FireListResetEvent();

			AssertNotNull("Event fired on collection view", lastListChanged);
			AssertEquals("ListChangedType.Reset fired", ListChangedType.Reset, lastListChanged.ListChangedType);
		}

		public void TestLoadOnInnerCollectionFiresListChangedEventOnceOnly()
		{
			Dummy.Collection.AddNew().Z0_Description = "INVIEW";
			var collectionView = GetNewDummyViewCollection(Dummy.Collection);
			AssertEquals("Precondition", 1, collectionView.Count);

			IBindingList bindingList = collectionView;

			var listChangedEventFiredCount = 0;
			bindingList.ListChanged += (sender, e) =>
				{
					listChangedEventFiredCount++;
				};

			Dummy.Collection.Load();

			AssertEquals("Should have fired only once on dispose", 1, listChangedEventFiredCount);
		}

		#region TestSuspendListChagedIsCalledForCollectionToFilter

		public void TestSuspendListChagedIsCalledForCollectionToFilterAndNoStackOverflowOnViewRebuild()
		{
			var collection = new DummyCollectionWithAdditionalListChangedSuspenders(Factory);
			var collectionView = GetNewDummyViewCollection(collection);
			Assert("AreAdditionalListChangedSuspendersActivated: Initially", !collection.AreAdditionalListChangedSuspendersActivated);
			var bizo = Factory.New<DummyBusinessObject>();
			using (collectionView.SuspendListChanged())
			{
				Assert("AreAdditionalListChangedSuspendersActivated: : when collection view list changed is suspended.", collection.AreAdditionalListChangedSuspendersActivated);
				collectionView.Add(bizo);
				bizo.Z0_Description = "INVIEW";
			}
			Assert("AreAdditionalListChangedSuspendersActivated: after suspender disposing", !collection.AreAdditionalListChangedSuspendersActivated);
			AssertEquals("SuspendListChangedCallCounter", 1, collection.SuspendListChangedCallCounter);

			collection.SuspendListChangedCallCounter = 0;
			collectionView.Rebuild();
			AssertEquals("Collection view rebuild should not cause collection suspension because as it doesn't change it. Also it helps to avoid stack overflow in some tricky scenarios.",
				0, collection.SuspendListChangedCallCounter);
		}

		class DummyCollectionWithAdditionalListChangedSuspenders : DummyBusinessObjectCollection
		{
			public DummyCollectionWithAdditionalListChangedSuspenders(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override DisposableList GetAdditionalListChangedSuspenders()
			{
				SuspendListChangedCallCounter++;
				return new DisposableList(new[] { new DisposableAction(() => AdditionalListChangedSuspenderCounter++, () => AdditionalListChangedSuspenderCounter--) });
			}

			int AdditionalListChangedSuspenderCounter;

			public bool AreAdditionalListChangedSuspendersActivated => AdditionalListChangedSuspenderCounter > 0;

			public int SuspendListChangedCallCounter;
		}

		#endregion

		DummyViewCollection GetNewDummyViewCollection(BusinessObjectCollection collectionToFilter)
		{
			return (DummyViewCollection)GetNewCollection(collectionToFilter);
		}

		protected override ISubsetBusinessObjectCollection GetNewCollection(BusinessObjectCollection collectionToFilter)
		{
			return new DummyViewCollection(collectionToFilter);
		}
	}
}
