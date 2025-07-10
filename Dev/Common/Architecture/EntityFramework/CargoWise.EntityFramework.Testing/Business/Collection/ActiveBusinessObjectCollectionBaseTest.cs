using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ActiveBusinessObjectCollectionBaseTest : TestCaseWithFactory
	{
		public void TestTrackIndexedCollections_ShouldTrackOnlyWhenIndexIsAccessed()
		{
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			var index = collection.IndexExposed;

			AssertNull(ActiveBusinessObjectCollection.IndexedCollections);
			AssertNull(ActiveBusinessObjectCollection.IndexedCollections_ForTest);

			using (ActiveBusinessObjectCollection.TrackIndexedCollectionCounts_ForTest())
			{
				AssertNotNull(ActiveBusinessObjectCollection.IndexedCollections);
				AssertNotNull(ActiveBusinessObjectCollection.IndexedCollections_ForTest);

				AssertEquals(0, ActiveBusinessObjectCollection.IndexedCollections.Count);

				collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);

				AssertEquals(0, ActiveBusinessObjectCollection.IndexedCollections.Count);

				index = collection.IndexExposed;

				AssertEquals(1, ActiveBusinessObjectCollection.IndexedCollections.Count);
				AssertEquals(new KeyValuePair<Type, int>(collection.GetType(), 1), ActiveBusinessObjectCollection.IndexedCollections_ForTest.Single());

				collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
				index = collection.IndexExposed;

				AssertEquals(1, ActiveBusinessObjectCollection.IndexedCollections.Count);
				AssertEquals(new KeyValuePair<Type, int>(collection.GetType(), 2), ActiveBusinessObjectCollection.IndexedCollections_ForTest.Single());

				var otherCollection = new ActiveBusinessObjectCollection<DummyBusinessObjectInList>(Factory);
				var otherIndex = otherCollection.IndexExposed;

				AssertContainsExactElementsInAnyOrder(new[]
				{
					new KeyValuePair<Type, int>(collection.GetType(), 2),
					new KeyValuePair<Type, int>(otherCollection.GetType(), 1),
				}, ActiveBusinessObjectCollection.IndexedCollections_ForTest);
			}

			AssertNull(ActiveBusinessObjectCollection.IndexedCollections);
			AssertNull(ActiveBusinessObjectCollection.IndexedCollections_ForTest);
		}

		public void TestTableName()
		{
			AssertEquals("((IBusiness)collection).TableName", DummyBusinessObject.Schema.TableName, ((IBusiness)Collection).TableName);
		}

		public void TestDeactivate()
		{
			IActiveBusinessObjectCollection collection = Collection;
			int countChanged = 0;
			collection.CountChanged += delegate
			{ countChanged++; };
			DummyBusinessObject businessObject1 = Factory.New<DummyBusinessObject>();
			AssertEquals(1, countChanged);
			AssertEquals(1, ActiveBusinessObjectCollectionIndexCache.GetInstance(Factory).All.Count());
			Collection.Deactivate();

			AssertEquals(0, ActiveBusinessObjectCollectionIndexCache.GetInstance(Factory).All.Count());

			DummyBusinessObject businessObject2 = Factory.New<DummyBusinessObject>();
			AssertEquals(1, countChanged);

			AssertEquals(0, ActiveBusinessObjectCollectionIndexCache.GetInstance(Factory).All.Count());

			AssertEquals(2, Collection.Count);  // This will 'liven' the collection again
			DummyBusinessObject businessObject3 = Factory.New<DummyBusinessObject>();
			AssertEquals("Collection should be live again", 2, countChanged);
			AssertEquals(3, Collection.Count);
		}

		public void TestDisposableServiceAutoDeactivatesNewCollections()
		{
			var destructorCalled = false;
			var factory = new BusinessObjectFactory();
			TestDependentActiveBusinessObjectCollection collection;
			using (factory.AddDisposableService())
			{
				collection = new TestDependentActiveBusinessObjectCollection(factory.New<DummyWithDependentsBusinessObject>());
				collection.destuctorCalled += delegate
				{ destructorCalled = true; };
				collection.AddNew();
				((IBusiness)collection).IncrementReadOnlyIncludingChildren();
				AssertEquals(true, collection.IsInDisposableManager);
				AssertEquals(true, collection.IsFinalizeSuppressed);
			}
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			AssertEquals(false, destructorCalled);
		}

		public void TestDeactivateSuppressesFinalize()
		{
			bool destructorCalled1 = false;
			bool destructorCalled2 = false;

			CreateUnreferencedCollection(delegate
			{ destructorCalled1 = true; }, deactivate: true);
			CreateUnreferencedCollection(delegate
			{ destructorCalled2 = true; }, deactivate: false);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			AssertEquals(false, destructorCalled1);
			AssertEquals(true, destructorCalled2);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		void CreateUnreferencedCollection(EventHandler destuctorCalled, bool deactivate)
		{
			var collection = new TestDependentActiveBusinessObjectCollection(Factory.New<DummyWithDependentsBusinessObject>());
			collection.destuctorCalled += destuctorCalled;
			collection.AddNew();
			((IBusiness)collection).IncrementReadOnlyIncludingChildren();
			if (deactivate)
			{
				collection.Deactivate();
			}
		}

		public void TestAddingViaDataRefreshBus_ForNonDependentCollection()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyBusinessObject businessObject1 = newFactory.New<DummyBusinessObject>();
			DummyBusinessObject businessObject2 = newFactory.New<DummyBusinessObject>();
			DummyBusinessObject businessObject3 = newFactory.New<DummyBusinessObject>();

			AssertEquals("No items before the other factory saves", 0, Collection.Count);
			((IBindingList)Collection).ListChanged += new ListChangedEventHandler(OnListChanged);
			newFactory.Save();
			AssertEquals("All items in collection after factory save", 3, Collection.Count);
			AssertListChanged(ListChangedType.Reset);
		}

		public void TestGetAllNotificationsWhenAdditionalFilterNotMet()
		{
			IActiveBusinessObjectCollection activeCollection = Collection;
			DummyBusinessObject bizObj = Factory.New<DummyBusinessObject>();
			AssertEquals("This DummyBizo cannot be chosen here. Please choose another DummyBizo.", activeCollection.GetAllNotificationsWhenAdditionalFilterNotMet(bizObj));

			Collection.AddNotificationWhenAdditionalFilterNotMetOverride = new ActiveBusinessObjectCollection<DummyBusinessObject>.AddNotificationWhenAdditionalFilterNotMetOverrideDelegate((x, y) => x.Add("HELLO WORLD"));
			AssertEquals("HELLO WORLD", activeCollection.GetAllNotificationsWhenAdditionalFilterNotMet(bizObj));
		}

		public void TestGetNotificationTyoeWhenAdditionalFilterNotMet()
		{
			IActiveBusinessObjectCollection activeCollection = Collection;
			DummyBusinessObject bizObj = Factory.New<DummyBusinessObject>();
			AssertEquals(NotificationType.Error, activeCollection.GetNotificationTyoeWhenAdditionalFilterNotMet());

			Collection.GetNotificationTypeWhenAdditionalFilterNotMetOverride = new ActiveBusinessObjectCollection<DummyBusinessObject>.GetNotificationTypeWhenAdditionalFilterNotMetOverrideDelegate(() => NotificationType.Warning);
			AssertEquals(NotificationType.Warning, activeCollection.GetNotificationTyoeWhenAdditionalFilterNotMet());
		}

		public void TestAddingViaDataRefreshBus_ForDependentCollection()
		{
			Master.Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyWithDependentsBusinessObject master = newFactory.Load<DummyWithDependentsBusinessObject>(Master.PK);
			DummyDependantBusinessObject businessObject1 = newFactory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject businessObject2 = newFactory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject businessObject3 = newFactory.New<DummyDependantBusinessObject>();
			businessObject1.ZD1_Z0 = master.PK;
			businessObject2.ZD1_Z0 = master.PK;
			businessObject3.ZD1_Z0 = master.PK;

			AssertEquals("No items before the other factory saves", 0, DependentCollection.Count);
			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			newFactory.Save();
			AssertEquals("All items in collection after factory save", 3, DependentCollection.Count);
			AssertListChanged(ListChangedType.Reset);
		}

		public void TestAddNew()
		{
			((IBindingList)DependentCollection).ListChanged += OnListChanged;
			DummyDependantBusinessObject element1 = DependentCollection.AddNew();
			AssertEquals("BusinessObject committed after AddNew()", DataRowState.Added, element1.Row.RowState);
			AssertListChanged(ListChangedType.ItemAdded);
		}

		public void TestAddNew_MasterIsITypeDeciderContext()
		{
			var master = Factory.New<DummyWithDependentsAndTypeDeciderContextBusinessObject>();
			var collection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(master);
			var dummyDependentBusinessObject = collection.AddNew();
			AssertEquals("Precondition", "XX", (master as ITypeDeciderContext).Country);
			AssertEquals(typeof(DummyDependantBusinessObject.DummyDependantBusinessObjectXXCountry), dummyDependentBusinessObject.GetType());
		}

		public void TestCollectionCountChange()
		{
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);

			bool countChangeFired = false;
			bool countChangeItemAdded = false;
			bool countChangeItemRemoved = false;
			int countChangeCounter = 0;
			CollectionCountChangedEventHandler countChangeHandler =
				(s, e) =>
				{
					countChangeFired = true;
					countChangeItemAdded = e.ItemAdded;
					countChangeItemRemoved = e.ItemRemoved;
					countChangeCounter++;
				};

			collection.CollectionCountChange += countChangeHandler;
			try
			{
				collection.AddNew();
				AssertEquals(true, countChangeFired);
				AssertEquals(true, countChangeItemAdded);
				AssertEquals(false, countChangeItemRemoved);
				AssertEquals(1, countChangeCounter);
				countChangeFired = false;
				countChangeItemAdded = false;
				countChangeItemRemoved = false;

				collection[0].Delete();
				AssertEquals(true, countChangeFired);
				AssertEquals(false, countChangeItemAdded);
				AssertEquals(true, countChangeItemRemoved);
				AssertEquals(2, countChangeCounter);
				countChangeFired = false;
				countChangeItemAdded = false;
				countChangeItemRemoved = false;

				Factory.New<DummyBusinessObject>();
				AssertEquals(true, countChangeFired);
				AssertEquals(true, countChangeItemAdded);
				AssertEquals(false, countChangeItemRemoved);
				AssertEquals(3, countChangeCounter);
			}
			finally
			{
				collection.CollectionCountChange -= countChangeHandler;
			}
		}

		public void TestIsMatchesFilterOverridden()
		{
			AssertEquals(false, ActiveBusinessObjectCollection<DummyBusinessObject>.IsMatchesFilterOverridden(typeof(ActiveBusinessObjectCollection<DummyBusinessObject>)));
			AssertEquals(false, ActiveBusinessObjectCollection<DummyBusinessObject>.IsMatchesFilterOverridden(typeof(ActiveBusinessObjectCollection<DummyBusinessObject>)));
			AssertEquals(true, TestDependentActiveBusinessObjectCollection.IsMatchesFilterOverridden(typeof(TestDependentActiveBusinessObjectCollection)));
			AssertEquals(true, TestDependentActiveBusinessObjectCollection.IsMatchesFilterOverridden(typeof(TestDependentActiveBusinessObjectCollection)));
			AssertEquals(true, TestDependentActiveBusinessObjectCollection.IsMatchesFilterOverridden(typeof(TestDeeperCollection)));
			AssertEquals(true, TestDependentActiveBusinessObjectCollection.IsMatchesFilterOverridden(typeof(TestDeeperCollection)));
			AssertEquals(false, TestActiveBusinessObjectCollection.IsMatchesFilterOverridden(typeof(TestActiveBusinessObjectCollection)));
			AssertEquals(false, TestActiveBusinessObjectCollection.IsMatchesFilterOverridden(typeof(TestActiveBusinessObjectCollection)));
		}

		public void TestNoMemoryLeakOnSwapFactory()
		{
			ActiveBusinessObjectCollection<DummyBusinessObjectWithActiveFilter> collection = new ActiveBusinessObjectCollection<DummyBusinessObjectWithActiveFilter>(Factory);
			var index1 = collection.IndexExposed;
			collection.Factory = new BusinessObjectFactory();
			var index2 = collection.IndexExposed;
			collection.AdditionalFilter = ZQuery.NoResultQuery;
			var index3 = collection.IndexExposed;
			collection.Factory = new BusinessObjectFactory();
			var index4 = collection.IndexExposed;
			collection.AdditionalFilter = ZQuery.NoResultQuery;
			var index5 = collection.IndexExposed;
			collection.AdditionalFilter = ZQuery.NoResultQuery;
			var index6 = collection.IndexExposed;
			AssertEquals("index1 has been disposed", true, index1.IsDisposed);
			AssertEquals("index2 has been disposed", true, index2.IsDisposed);
			AssertEquals("index3 has been disposed", true, index3.IsDisposed);
			AssertEquals("index4 (post-factory swap) has not been disposed", false, index4.IsDisposed);
			AssertEquals("index5 (post-factory swap) has not been disposed", false, index5.IsDisposed);
			AssertEquals("index6 (post-factory swap) has not been disposed", false, index6.IsDisposed);
		}

		public void TestAdditionalFilter_WithIgnoreActiveFilter()
		{
			ActiveBusinessObjectCollection<DummyBusinessObjectWithActiveFilter> collection = new ActiveBusinessObjectCollection<DummyBusinessObjectWithActiveFilter>(Factory);
			DummyBusinessObjectWithActiveFilter element1 = collection.AddNew();
			DummyBusinessObjectWithActiveFilter element2 = collection.AddNew();
			element1.Z0_Bool = true;
			element2.Z0_Bool = false;

			AssertEquals("1 active element returned from filter", 1, collection.Count);
			AssertEquals("Element1 active", true, collection.Contains(element1));
			AssertEquals("Element2 not active", false, collection.Contains(element2));

			ZQuery filter = new ZQuery();
			filter.IgnoreActiveFilter = true;
			collection.AdditionalFilter = filter;

			AssertEquals("All items returned when active filter ignored", 2, collection.Count);
			AssertEquals("All items returned when active filter ignored", true, collection.Contains(element1));
			AssertEquals("All items returned when active filter ignored", true, collection.Contains(element2));
		}

		public void TestRelationshipFilter_WithIgnoreActiveFilter()
		{
			ActiveBusinessObjectCollection<DummyBusinessObjectWithActiveFilter> collection = new ActiveBusinessObjectCollection<DummyBusinessObjectWithActiveFilter>(Factory);
			DummyBusinessObjectWithActiveFilter element1 = collection.AddNew();
			DummyBusinessObjectWithActiveFilter element2 = collection.AddNew();
			element1.Z0_Bool = true;
			element2.Z0_Bool = false;

			AssertEquals("1 active element returned from filter", 1, collection.Count);
			AssertEquals("Element1 active", true, collection.Contains(element1));
			AssertEquals("Element2 not active", false, collection.Contains(element2));

			ZQuery filter = new ZQuery();
			filter.IgnoreActiveFilter = true;
			collection = new ActiveBusinessObjectCollection<DummyBusinessObjectWithActiveFilter>(Factory, filter);

			AssertEquals("All items returned when active filter ignored", 2, collection.Count);
			AssertEquals("All items returned when active filter ignored", true, collection.Contains(element1));
			AssertEquals("All items returned when active filter ignored", true, collection.Contains(element2));
		}

		public void TestCollection_WithDBOnlyFilter()
		{
			DummyBusinessObject element1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject element2 = Factory.New<DummyBusinessObject>();
			element1.Z0_Bool = true;
			element2.Z0_Bool = true;
			Factory.Save();

			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			filter.AddFilterAndZSQLParameterCollection("Z0_Bool = @bool", new ZSqlParameterCollection(ZSqlParameter.New("@bool", "Y", DummyBizoSchema.Z0_Bool)));

			Factory.ResetDatabaseLoadCount();
			Collection.AdditionalFilter = filter;
			AssertEquals(2, Collection.Count);
			AssertEquals("Only 1 hit required to retrieve the DBOnlyQuery'd data", 1, Factory.DatabaseLoadCount);

			Factory.ResetDatabaseLoadCount();
			((IActiveBusinessObjectCollection)Collection).Refresh();
			AssertEquals(2, Collection.Count);
			AssertEquals("No additional database hits required", 0, Factory.DatabaseLoadCount);
		}

		public void TestGetEnumerator_ThrowsSilentEnumerationException()
		{
			DummyBusinessObject element = Collection.AddNew();
			foreach (DummyBusinessObject item in Collection)
			{
				Collection.AddNew();
			}
			AssertEquals("Collection ActiveBusinessObjectCollection`1 with elements of type CargoWise.EntityFramework.Testing.DummyBusinessObject was modified : enumeration will continue and the system will continue to function. Source = Add", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestEqualsAndGetHashCode()
		{
			ActiveBusinessObjectCollection<DummyBusinessObject> collection1 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			ActiveBusinessObjectCollection<DummyBusinessObject> collection2 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			AssertEquals("Same collection equals", collection1, collection1);
			AssertNotEquals(
				"Different collection instances are not equal, even if their indexes are equal. " +
				"This is important because if you put the collection into a hash table, the equals could change when the collection index changes.",
				collection1, collection2);
			AssertNotEquals("Collection not equal to an object that isn't a collection", collection1, "oranges");
		}

		public void TestLastChangeNumber()
		{
			AssertEquals(0, ((IBusinessObjectState)DependentCollection).LastChangeNumber);
			((IBusinessObjectCollectionInternals)DependentCollection).HasChangesFromDelete = true;
			Assert(((IBusinessObjectState)DependentCollection).LastChangeNumber > 0);
		}

		#region TestApplyGenericSort

		public void TestApplyGenericSort()
		{
			ActiveBusinessObjectCollection<DummyBusinessObject> collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			DummyBusinessObjectComparer comparer = new DummyBusinessObjectComparer();
			collection.ApplySort(comparer);
			AssertNotNull(((IActiveBusinessObjectCollection)collection).Index);
			AssertEquals(typeof(GenericComparerWrapper<DummyBusinessObject>), collection.SortComparer.GetType());
			AssertEquals(comparer, ((GenericComparerWrapper<DummyBusinessObject>)collection.SortComparer).comparer);
		}

		class DummyBusinessObjectComparer : IComparer<DummyBusinessObject>
		{
			public int Compare(DummyBusinessObject x, DummyBusinessObject y)
			{
				return 0;
			}
		}

		#endregion

		#region TestRemoveSortDisablesSort

		public void TestRemoveSortDisablesSort()
		{
			DummyDependantBusinessObject element1 = DependentCollection.AddNew();
			DummyDependantBusinessObject element2 = DependentCollection.AddNew();
			DummyDependantBusinessObject element3 = DependentCollection.AddNew();

			MyComparer comparer = new MyComparer();
			DependentCollection.ApplySort(comparer);
			BusinessObject o2 = DependentCollection[2];     // To force sort
			int sortCount = comparer.CompareCount;
			Assert(sortCount > 0);
			DependentCollection.RemoveSort();
			element3.ZD1_Number = 123;
			AssertEquals(sortCount, comparer.CompareCount);
		}

		class MyComparer : IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				BusinessObject xx = (BusinessObject)x;
				BusinessObject yy = (BusinessObject)y;
				CompareCount++;
				return xx.PK.CompareTo(yy.PK);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Override is required")]
			public override bool Equals(object obj)
			{
				return base.Equals(obj);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Override is required")]
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			#endregion

			public int CompareCount { get; private set; }
		}

		#endregion

		#region AddNew / DeleteAll

		public void TestDeleteAll()
		{
			DummyDependantBusinessObject element1 = DependentCollection.AddNew();
			DummyDependantBusinessObject element2 = DependentCollection.AddNew();
			DummyDependantBusinessObject element3 = DependentCollection.AddNew();
			DummyDependantBusinessObject element4 = DependentCollection.AddNew();
			DummyDependantBusinessObject element5 = DependentCollection.AddNew();

			int listChangedFiredCount = 0;
			((IBindingList)DependentCollection).ListChanged += delegate
			{ listChangedFiredCount++; };
			DependentCollection.DeleteAll();
			AssertEquals("Only 1 ListChanged event fired", 1, listChangedFiredCount);

			AssertEquals(true, element1.IsDeleted);
			AssertEquals(true, element2.IsDeleted);
			AssertEquals(true, element3.IsDeleted);
			AssertEquals(true, element4.IsDeleted);
			AssertEquals(true, element5.IsDeleted);
		}

		#endregion

		#region Find

		public void TestFind()
		{
			ActiveBusinessObjectCollection<DummyBusinessObject> testCollection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			testCollection.AddNew().Z0_Code = "AAA";
			testCollection.AddNew().Z0_Code = "BBB";
			testCollection.AddNew().Z0_Code = "AAA";

			List<DummyBusinessObject> foundList = new List<DummyBusinessObject>(testCollection.Find(new ZQuery(DummyBizoSchema.Z0_Code, "AAA")));
			AssertEquals(2, foundList.Count);
			AssertEquals("AAA", foundList[0].Z0_Code);
			AssertEquals("AAA", foundList[1].Z0_Code);
		}

		public void TestFindByPK()
		{
			DummyBusinessObject element1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject element2 = Factory.New<DummyBusinessObject>();
			element1.Z0_Bool = true;
			element2.Z0_Bool = true;
			Factory.Save();

			AssertEquals(element1, Collection.FindByPK(element1.PK));
			AssertEquals(element2, Collection.FindByPK(element2.PK));

			ActiveBusinessObjectCollection<DummyBusinessObject> collectionInNewFactory = new ActiveBusinessObjectCollection<DummyBusinessObject>(new BusinessObjectFactory());
			AssertEquals(element1.PK, collectionInNewFactory.FindByPK(element1.PK).PK);
			AssertEquals(element2.PK, collectionInNewFactory.FindByPK(element2.PK).PK);
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

			ActiveBusinessObjectCollection<DummyBusinessObject> collectionInNewFactory = new ActiveBusinessObjectCollection<DummyBusinessObject>(new BusinessObjectFactory());

			List<DummyBusinessObject> findResult = new List<DummyBusinessObject>(collectionInNewFactory.Find(element => element.Z0_Code == "BBB"));
			AssertEquals(1, findResult.Count);
			AssertEquals(element2.PK, findResult[0].PK);

			findResult = new List<DummyBusinessObject>(collectionInNewFactory.Find(element => element.Z0_Code == "AAA"));
			AssertEquals(2, findResult.Count);
			Assert(findResult[0].PK == element1.PK || findResult[1].PK == element1.PK);
			Assert(findResult[0].PK != element2.PK && findResult[1].PK != element2.PK);
			Assert(findResult[0].PK == element3.PK || findResult[1].PK == element3.PK);
		}

		#endregion

		#region RefreshAll

		public void TestRefreshAll_ForAllCollections()
		{
			ListChangedEventArgs lastListChanged = null;
			ListChangedEventArgs lastListChanged2 = null;
			ActiveBusinessObjectCollection<DummyBusinessObject> collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			ActiveBusinessObjectCollection<DummyDependantBusinessObject> collection2 = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(Factory);
			collection.AddNew();
			collection2.AddNew();

			((IBindingList)collection).ListChanged += (object sender, ListChangedEventArgs e) => { lastListChanged = e; };
			((IBindingList)collection2).ListChanged += (object sender, ListChangedEventArgs e) => { lastListChanged2 = e; };
			ActiveBusinessObjectCollection.RefreshAll(Factory);
			AssertEquals("All collections Reset", ListChangedType.Reset, lastListChanged.ListChangedType);
			AssertEquals("All collections Reset", ListChangedType.Reset, lastListChanged2.ListChangedType);
		}

		public void TestRefreshAll_ForSpecificElementType()
		{
			ListChangedEventArgs lastListChanged = null;
			ListChangedEventArgs lastListChanged2 = null;
			ActiveBusinessObjectCollection<DummyBusinessObject> collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			ActiveBusinessObjectCollection<DummyDependantBusinessObject> collection2 = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(Factory);
			collection.AddNew();
			collection2.AddNew();

			((IBindingList)collection).ListChanged += (object sender, ListChangedEventArgs e) => { lastListChanged = e; };
			((IBindingList)collection2).ListChanged += (object sender, ListChangedEventArgs e) => { lastListChanged2 = e; };
			ActiveBusinessObjectCollection<DummyBusinessObject>.RefreshAll(Factory);
			AssertEquals("Collection of the appropriate element type Reset", ListChangedType.Reset, lastListChanged.ListChangedType);
			AssertEquals("Collection with a different element type not reset", null, lastListChanged2);
		}

		[ExpectNoExceptions]
		public void TestRefreshAll_CreatesNewListIncaseModified()
		{
			ActiveBusinessObjectCollection<DummyBusinessObject> collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			collection.AddNew();

			((IBindingList)collection).ListChanged += (object sender, ListChangedEventArgs e) =>
			{
				ActiveBusinessObjectCollection<DummyDependantBusinessObject> collection2 = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(Factory);
				collection2.AddNew();
			};

			ActiveBusinessObjectCollection.RefreshAll(Factory);
		}

		#endregion

		#region TestFindAllActiveRelationships

		public void TestFindAllActiveRelationships()
		{
			var factory = new BusinessObjectFactory();
			var filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "AAA");
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(factory, filter);
			_ = collection.Count;

			var relationshipCollection = ActiveBusinessObjectCollection.FindAllActiveRelationships(typeof(DummyBusinessObject), factory);
			AssertEquals(1, relationshipCollection.Count());

			var relationship = relationshipCollection.Single();
			AssertEquals(nameof(relationship.ElementType), typeof(DummyBusinessObject), relationship.ElementType);
			AssertEquals(nameof(relationship.Relationship), collection.Relationship, relationship.Relationship);

			collection.Deactivate();

			var relationshipCollection2 = ActiveBusinessObjectCollection.FindAllActiveRelationships(typeof(DummyBusinessObject), factory);
			AssertEquals(0, relationshipCollection2.Count());
		}

		public void TestFindAllActiveRelationships_WithInheritedType()
		{
			var factory = new BusinessObjectFactory();
			var filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "AAA");
			var collection = new ActiveBusinessObjectCollection<TestDummy>(factory, filter);
			_ = collection.Count;

			var relationshipCollection = ActiveBusinessObjectCollection.FindAllActiveRelationships(typeof(DummyBusinessObject), factory);
			AssertEquals(1, relationshipCollection.Count());

			var relationship = relationshipCollection.Single();
			AssertEquals(nameof(relationship.ElementType), typeof(TestDummy), relationship.ElementType);
			AssertEquals(nameof(relationship.Relationship), collection.Relationship, relationship.Relationship);

			collection.Deactivate();

			var relationshipCollection2 = ActiveBusinessObjectCollection.FindAllActiveRelationships(typeof(DummyBusinessObject), factory);
			AssertEquals(0, relationshipCollection2.Count());
		}

		#endregion

		#region SetReadOnlyIncludingChildren

		public void TestSetReadOnlyIncludingChildren()
		{
			DummyBusinessObject element = Collection.AddNew();
			AssertEquals("Not ReadOnly initially", false, Collection.ReadOnly);
			int loaded = Collection.Count;

			Collection.SetReadOnlyIncludingChildren(true);
			AssertEquals("Collection is read only", true, Collection.ReadOnly);
			AssertEquals("Collection element is read only", true, element.ReadOnly);

			Collection.SetReadOnlyIncludingChildren(false);
			AssertEquals("Collection is not read only", false, Collection.ReadOnly);
			AssertEquals("Collection element is not read only", false, element.ReadOnly);
		}

		#endregion

		#region CreateRelationshipFilter

		public void TestCreateRelationshipFilter()
		{
			DummyDependantBusinessObject element1 = DependentCollection.AddNew();
			DummyDependantBusinessObject element2 = DependentCollection.AddNew();
			AssertEquals("2 elements initially", 2, DependentCollection.Count);
			element1.ZD1_Code = "EXL";
			AssertEquals("1 element when one filtered out by the relationship", 1, DependentCollection.Count);
		}

		#endregion

		#region ListChanged event

		public void TestListChanged_ItemChanged()
		{
			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);

			DummyDependantBusinessObject element = DependentCollection.AddNew();
			AssertListChanged(ListChangedType.ItemAdded);

			element.NonPersistentProperty = "NewValue";
			AssertListChanged(ListChangedType.ItemChanged);
		}

		public void TestListChanged_ItemMovedWhenElementSortedPositionChanges()
		{
			DummyDependantBusinessObject element1 = DependentCollection.AddNew();
			DummyDependantBusinessObject element2 = DependentCollection.AddNew();

			DependentCollection.ApplySort("NonPersistentProperty", ListSortDirection.Ascending);
			element1.NonPersistentProperty = "Value1";
			element2.NonPersistentProperty = "Value2";

			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			element1.NonPersistentProperty = "Value3";
			AssertListChanged(ListChangedType.ItemMoved);
		}

		public void TestListChanged_ChangeEventWhenElementSortedPositionChangesOnDataRefresh()
		{
			DummyDependantBusinessObject element1 = DependentCollection.AddNew();
			DummyDependantBusinessObject element2 = DependentCollection.AddNew();
			DependentCollection.ApplySort(DummyDependentBizoSchema.ZD1_Code.Name, ListSortDirection.Ascending);
			element1.ZD1_Code = "1";
			element2.ZD1_Code = "2";
			Factory.Save();

			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyDependantBusinessObject loadedElement1 = newFactory.Load<DummyDependantBusinessObject>(element1.PK);
			loadedElement1.ZD1_Code = "3";
			newFactory.Save();
			AssertListChanged(ListChangedType.Reset);
		}

		public void TestListChanged_ForAddNewThenCommit()
		{
			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);

			BusinessObject uncommittedElement = (BusinessObject)((IBindingList)DependentCollection).AddNew();
			AssertListChanged(ListChangedType.ItemAdded);

			uncommittedElement.HasChanges = true;
			((ICancelAddNew)DependentCollection).EndNew(DependentCollection.Count - 1);
			AssertListChanged(ListChangedType.ItemAdded, ListChangedType.ItemChanged);
		}

		public void TestListChanged_ForAddNewThenCancel()
		{
			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);

			BusinessObject uncommittedElement = (BusinessObject)((IBindingList)DependentCollection).AddNew();
			AssertListChanged(ListChangedType.ItemAdded);

			((ICancelAddNew)DependentCollection).CancelNew(DependentCollection.Count - 1);
			AssertListChanged(ListChangedType.ItemDeleted);
		}

		public void TestListChanged_ForAddNew()
		{
			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			BusinessObject uncommittedElement = DependentCollection.AddNew();
			AssertListChanged(ListChangedType.ItemAdded);
		}

		public void TestListChanged_ForDelete()
		{
			BusinessObject uncommittedElement = DependentCollection.AddNew();
			AssertEquals(1, DependentCollection.Count);

			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			uncommittedElement.Delete();
			AssertListChanged(ListChangedType.ItemDeleted);
		}

		public class DummyBusinessObjectWithExtraProperties : DummyBusinessObject
		{
			public DummyBusinessObjectWithExtraProperties(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ZString Code { get { return Z0_Code; } set { Z0_Code = value; } }
			public ZInt Number { get { return Z0_Number; } set { Z0_Number = value; } }
		}

		public void TestSubsequentDeletes_WhenSortedDifferentToDataView_DontCauseCorruption()
		{
			var element = Factory.NewWithValidTestData<DummyBusinessObjectWithExtraProperties>();
			element.Z0_Code = "A";
			element.Z0_Number = 7;
			element = Factory.NewWithValidTestData<DummyBusinessObjectWithExtraProperties>();
			element.Z0_Code = "B";
			element.Z0_Number = 3;
			element = Factory.NewWithValidTestData<DummyBusinessObjectWithExtraProperties>();
			element.Z0_Code = "C";
			element.Z0_Number = 6;
			element = Factory.NewWithValidTestData<DummyBusinessObjectWithExtraProperties>();
			element.Z0_Code = "D";
			element.Z0_Number = 2;
			element = Factory.NewWithValidTestData<DummyBusinessObjectWithExtraProperties>();
			element.Z0_Code = "E";
			element.Z0_Number = 5;
			element = Factory.NewWithValidTestData<DummyBusinessObjectWithExtraProperties>();
			element.Z0_Code = "F";
			element.Z0_Number = 1;
			element = Factory.NewWithValidTestData<DummyBusinessObjectWithExtraProperties>();
			element.Z0_Code = "G";
			element.Z0_Number = 4;
			element = Factory.NewWithValidTestData<DummyBusinessObjectWithExtraProperties>();
			element.Z0_Code = "H";
			element.Z0_Number = 0;

			Factory.Save();

			ActiveBusinessObjectCollection<DummyBusinessObjectWithExtraProperties> collectionInNewFactory
				= new ActiveBusinessObjectCollection<DummyBusinessObjectWithExtraProperties>(new BusinessObjectFactory());
			collectionInNewFactory.ApplySort(DummyBizoSchema.Z0_Code.Name, ListSortDirection.Ascending);
			var loaded = collectionInNewFactory.Count;
			collectionInNewFactory.ApplySort(nameof(DummyBusinessObjectWithExtraProperties.Number), ListSortDirection.Ascending);
			loaded = collectionInNewFactory.Count;
			//By sorting on a property of the business object that does not exist in the underlying table, DataView is unable to have the same sort, and DataViewBusinessObjectMapping mechanism is used.

			AssertEquals("H", collectionInNewFactory[0].Z0_Code);
			AssertEquals("F", collectionInNewFactory[1].Z0_Code);
			AssertEquals("D", collectionInNewFactory[2].Z0_Code);
			AssertEquals("B", collectionInNewFactory[3].Z0_Code);
			AssertEquals("G", collectionInNewFactory[4].Z0_Code);
			AssertEquals("E", collectionInNewFactory[5].Z0_Code);
			AssertEquals("C", collectionInNewFactory[6].Z0_Code);
			AssertEquals("A", collectionInNewFactory[7].Z0_Code);

			collectionInNewFactory[7].Delete();

			AssertEquals("H", collectionInNewFactory[0].Z0_Code);
			AssertEquals("F", collectionInNewFactory[1].Z0_Code);
			AssertEquals("D", collectionInNewFactory[2].Z0_Code);
			AssertEquals("B", collectionInNewFactory[3].Z0_Code);
			AssertEquals("G", collectionInNewFactory[4].Z0_Code);
			AssertEquals("E", collectionInNewFactory[5].Z0_Code);
			AssertEquals("C", collectionInNewFactory[6].Z0_Code);

			collectionInNewFactory[2].Delete();

			AssertEquals("H", collectionInNewFactory[0].Z0_Code);
			AssertEquals("F", collectionInNewFactory[1].Z0_Code);
			AssertEquals("B", collectionInNewFactory[2].Z0_Code); //Error occurs here with old code - "D" is here instead of "B".
			AssertEquals("G", collectionInNewFactory[3].Z0_Code);
			AssertEquals("E", collectionInNewFactory[4].Z0_Code);
			AssertEquals("C", collectionInNewFactory[5].Z0_Code);

			collectionInNewFactory[5].Delete();

			AssertEquals("H", collectionInNewFactory[0].Z0_Code);
			AssertEquals("F", collectionInNewFactory[1].Z0_Code);
			AssertEquals("B", collectionInNewFactory[2].Z0_Code);
			AssertEquals("G", collectionInNewFactory[3].Z0_Code);
			AssertEquals("E", collectionInNewFactory[4].Z0_Code);

			collectionInNewFactory[0].Delete();

			AssertEquals("F", collectionInNewFactory[0].Z0_Code);
			AssertEquals("B", collectionInNewFactory[1].Z0_Code);
			AssertEquals("G", collectionInNewFactory[2].Z0_Code);
			AssertEquals("E", collectionInNewFactory[3].Z0_Code);

			collectionInNewFactory[1].Delete();

			AssertEquals("F", collectionInNewFactory[0].Z0_Code);
			AssertEquals("G", collectionInNewFactory[1].Z0_Code);
			AssertEquals("E", collectionInNewFactory[2].Z0_Code);

			collectionInNewFactory[2].Delete();

			AssertEquals("F", collectionInNewFactory[0].Z0_Code);
			AssertEquals("G", collectionInNewFactory[1].Z0_Code);

			collectionInNewFactory[1].Delete();

			AssertEquals("F", collectionInNewFactory[0].Z0_Code);

			collectionInNewFactory[0].Delete();

			AssertEquals(0, collectionInNewFactory.Count);

			element = collectionInNewFactory.AddNew();
			element.Z0_Code = "Z";
			element.Z0_Number = 26;
			element = collectionInNewFactory.AddNew();
			element.Z0_Code = "Y";
			element.Z0_Number = 25;
			element = collectionInNewFactory.AddNew();
			element.Z0_Code = "X";
			element.Z0_Number = 24;
			element = collectionInNewFactory.AddNew();
			element = collectionInNewFactory.AddNew();

			AssertEquals(5, collectionInNewFactory.Count);
			AssertEquals("NCODE", collectionInNewFactory[0].Z0_Code);
			AssertEquals("NCODE", collectionInNewFactory[1].Z0_Code);
			AssertEquals("X", collectionInNewFactory[2].Z0_Code);
			AssertEquals("Y", collectionInNewFactory[3].Z0_Code);
			AssertEquals("Z", collectionInNewFactory[4].Z0_Code);

			element.Delete();

			AssertEquals(4, collectionInNewFactory.Count);
			AssertEquals("NCODE", collectionInNewFactory[0].Z0_Code);
			AssertEquals("X", collectionInNewFactory[1].Z0_Code);
			AssertEquals("Y", collectionInNewFactory[2].Z0_Code);
			AssertEquals("Z", collectionInNewFactory[3].Z0_Code);

			element = collectionInNewFactory.AddNew();
			element.Z0_Code = "A";
			element.Z0_Number = -3;
			element = collectionInNewFactory.AddNew();
			element.Z0_Code = "B";
			element.Z0_Number = -2;
			element = collectionInNewFactory.AddNew();
			element.Z0_Code = "C";
			element.Z0_Number = -1;

			AssertEquals(7, collectionInNewFactory.Count);
			AssertEquals("A", collectionInNewFactory[0].Z0_Code);
			AssertEquals("B", collectionInNewFactory[1].Z0_Code);
			AssertEquals("C", collectionInNewFactory[2].Z0_Code);
			AssertEquals("NCODE", collectionInNewFactory[3].Z0_Code);
			AssertEquals("X", collectionInNewFactory[4].Z0_Code);
			AssertEquals("Y", collectionInNewFactory[5].Z0_Code);
			AssertEquals("Z", collectionInNewFactory[6].Z0_Code);

			collectionInNewFactory[3].Delete();

			AssertEquals(6, collectionInNewFactory.Count);
			AssertEquals("A", collectionInNewFactory[0].Z0_Code);
			AssertEquals("B", collectionInNewFactory[1].Z0_Code);
			AssertEquals("C", collectionInNewFactory[2].Z0_Code);
			AssertEquals("X", collectionInNewFactory[3].Z0_Code);
			AssertEquals("Y", collectionInNewFactory[4].Z0_Code);
			AssertEquals("Z", collectionInNewFactory[5].Z0_Code);

			//next: have reverse sort
		}

		#endregion

		#region CountChanged event

		public void TestCountChanged()
		{
			bool countChanged = false;
			EventHandler handler = delegate
			{ countChanged = true; };
			Collection.CountChanged += handler;

			Collection.AddNew();
			AssertEquals("CountChanged fired for ItemAdded", true, countChanged);
			countChanged = false;

			Collection[0].Z0_AnotherDate = ZDateTime.Now;
			AssertEquals("CountChanged not fired for ItemChanged", false, countChanged);

			Collection.Delete(Collection[0]);
			AssertEquals("CountChanged fired for ItemDeleted", true, countChanged);
			countChanged = false;

			Collection.CountChanged -= handler;
			Collection.AddNew();
			AssertEquals("CountChanged not fired when no longer hooked", false, countChanged);
			countChanged = false;
		}

		#endregion

		#region HasChanges

		public void TestHasChanges_OnAddNew()
		{
			HasChangesChangedEventArgs hasChangesChangedEventArgs = null;
			((IBusinessObjectState)DependentCollection).HasChangesChanged += (s, e) => { hasChangesChangedEventArgs = e; };
			AssertEquals(0, DependentCollection.Count); // Load collection
			AssertHasChanges("HasChanges false initially", false, false, false, hasChangesChangedEventArgs);

			DependentCollection.AddNew();
			AssertHasChanges("HasChanges true after AddNew()", true, true, true, hasChangesChangedEventArgs);
		}

		public void TestHasChanges_OnAdd()
		{
			HasChangesChangedEventArgs hasChangesChangedEventArgs = null;
			((IBusinessObjectState)DependentCollection).HasChangesChanged += (s, e) => { hasChangesChangedEventArgs = e; };
			AssertHasChanges("HasChanges false initially", false, false, false, hasChangesChangedEventArgs);

			var dummy1 = Factory.New<DummyDependantBusinessObject>();
			DependentCollection.Add(dummy1);
			AssertHasChanges("HasChanges true after item Add(), HasChangesChanged not hit", true, false, true, hasChangesChangedEventArgs);

			AssertEquals(1, DependentCollection.Count); // Load collection
			var dummy2 = Factory.New<DummyDependantBusinessObject>();
			DependentCollection.Add(dummy2);
			AssertHasChanges("HasChanges true after item Add(), HasChangesChanged hit", true, true, true, hasChangesChangedEventArgs);

			hasChangesChangedEventArgs = null;
			dummy1.HasChanges = false;
			dummy2.HasChanges = false;
			AssertHasChanges("HasChanges false after new item HasChanges = false", false, true, false, hasChangesChangedEventArgs);
		}

		public void TestHasChanges_OnImplicitAdd()
		{
			HasChangesChangedEventArgs hasChangesChangedEventArgs = null;
			((IBusinessObjectState)DependentCollection).HasChangesChanged += (s, e) => { hasChangesChangedEventArgs = e; };
			((IActiveBusinessObjectCollection)DependentCollection).AdditionalFilter = new ZQuery(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.NotEqual, "EXL");
			AssertEquals(0, DependentCollection.Count); // Load collection
			AssertHasChanges("HasChanges false initially", false, false, false, hasChangesChangedEventArgs);

			var dummy = Factory.New<DummyDependantBusinessObject>();
			var excludedDummy = Factory.New<DummyDependantBusinessObject>();
			excludedDummy.ZD1_Code = "EXL";
			excludedDummy.ZD1_Z0 = Master.PK;
			AssertHasChanges("HasChanges false if added item not included in collection", false, false, false, hasChangesChangedEventArgs);

			hasChangesChangedEventArgs = null;
			dummy.ZD1_Z0 = Master.PK;
			AssertHasChanges("HasChanges true after item Add()", true, true, true, hasChangesChangedEventArgs);
		}

		public void TestHasChanges_OnRemove()
		{
			HasChangesChangedEventArgs hasChangesChangedEventArgs = null;
			((IBusinessObjectState)DependentCollection).HasChangesChanged += (s, e) => { hasChangesChangedEventArgs = e; };
			AssertEquals(0, DependentCollection.Count); // Load collection
			AssertHasChanges("HasChanges false initially", false, false, false, hasChangesChangedEventArgs);

			var dummy = Factory.New<DummyDependantBusinessObject>();
			dummy.ZD1_Z0 = Master.PK;
			AssertHasChanges("HasChanges false initially", true, true, true, hasChangesChangedEventArgs);

			hasChangesChangedEventArgs = null;
			DependentCollection.RemoveFromRelationship(dummy);
			AssertHasChanges("HasChanges true after item Remove() called on a changed bizo", false, true, false, hasChangesChangedEventArgs);
		}

		public void TestHasChanges_OnDelete()
		{
			HasChangesChangedEventArgs hasChangesChangedEventArgs = null;
			((IBusinessObjectState)DependentCollection).HasChangesChanged += (s, e) => { hasChangesChangedEventArgs = e; };
			AssertEquals(0, DependentCollection.Count); // Load collection
			AssertHasChanges("HasChanges false initially", false, false, false, hasChangesChangedEventArgs);

			hasChangesChangedEventArgs = null;
			var dummy = Factory.New<DummyDependantBusinessObject>();
			dummy.ZD1_Z0 = Master.PK;
			AssertHasChanges("HasChanges true after item is added", true, true, true, hasChangesChangedEventArgs);

			var dummyNotInCollection = Factory.New<DummyDependantBusinessObject>();
			dummyNotInCollection.ZD1_Code = "EXC";

			hasChangesChangedEventArgs = null;
			Factory.Save();
			DependentCollection.AdditionalFilter = new ZQuery(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.NotEqual, "EXC");
			AssertHasChanges("HasChanges false after saving", false, true, false, hasChangesChangedEventArgs);

			hasChangesChangedEventArgs = null;
			dummyNotInCollection.Delete();
			AssertHasChanges("HasChanges false after item Delete() of unrelated item", false, false, false, hasChangesChangedEventArgs);

			hasChangesChangedEventArgs = null;
			dummy.Delete();
			AssertHasChanges("HasChanges true after item Delete()", true, true, true, hasChangesChangedEventArgs);
		}

		public void TestHasChanges_AfterSave()
		{
			HasChangesChangedEventArgs hasChangesChangedEventArgs = null;
			((IBusinessObjectState)DependentCollection).HasChangesChanged += (s, e) => { hasChangesChangedEventArgs = e; };
			AssertHasChanges("HasChanges false initially", false, false, false, hasChangesChangedEventArgs);

			Factory.New<DummyDependantBusinessObject>().ZD1_Z0 = Master.PK;
			AssertHasChanges("HasChanges true initially, HasChangesChanged not hit", true, false, true, hasChangesChangedEventArgs);

			AssertEquals(1, DependentCollection.Count); // Load collection
			Factory.New<DummyDependantBusinessObject>().ZD1_Z0 = Master.PK;
			AssertHasChanges("HasChanges true initially, HasChangesChanged hit", true, true, true, hasChangesChangedEventArgs);

			hasChangesChangedEventArgs = null;
			Factory.Save();
			AssertHasChanges("HasChanges false after Factory.Save()", false, true, false, hasChangesChangedEventArgs);
		}

		public void TestHasChangesFromDelete_AfterSave()
		{
			HasChangesChangedEventArgs hasChangesChangedEventArgs = null;
			((IBusinessObjectState)DependentCollection).HasChangesChanged += (s, e) => { hasChangesChangedEventArgs = e; };
			AssertHasChanges("HasChanges false initially", false, false, false, hasChangesChangedEventArgs);

			var dummy = Factory.New<DummyDependantBusinessObject>();
			dummy.ZD1_Z0 = Master.PK;
			Factory.Save();
			AssertHasChanges("HasChanges false initially", false, true, false, hasChangesChangedEventArgs);

			hasChangesChangedEventArgs = null;
			dummy.Delete();
			AssertHasChanges("HasChanges true after Delete()", true, true, true, hasChangesChangedEventArgs);

			hasChangesChangedEventArgs = null;
			Factory.Save();
			AssertHasChanges("HasChanges false after Factory.Save()", false, true, false, hasChangesChangedEventArgs);
		}

		public void TestHasChanges_ForNonCommittedItem()
		{
			HasChangesChangedEventArgs hasChangesChangedEventArgs = null;
			((IBusinessObjectState)DependentCollection).HasChangesChanged += (s, e) => { hasChangesChangedEventArgs = e; };
			AssertEquals(0, DependentCollection.Count); // Load collection
			AssertHasChanges("HasChanges false initially", false, false, false, hasChangesChangedEventArgs);

			var dummy = (DummyDependantBusinessObject)((IBindingList)DependentCollection).AddNew();
			AssertHasChanges("HasChanges true after IBindingList.AddNew(), but HasChangesChanged wasn't called", true, false, false, hasChangesChangedEventArgs);

			dummy.ZD1_Code = "CHG";
			AssertHasChanges("HasChanges true after IBindingList.AddNew and property changed", true, true, true, hasChangesChangedEventArgs);

			hasChangesChangedEventArgs = null;
			((ICancelAddNew)DependentCollection).EndNew(DependentCollection.Count - 1);
			AssertHasChanges("HasChanges after EndNew", true, false, false, hasChangesChangedEventArgs);
		}

		public void TestHasChangesChanged_EventContinuesToWorkWhenChangingIndex()
		{
			HasChangesChangedEventArgs hasChangesChangedEventArgs = null;
			((IBusinessObjectState)DependentCollection).HasChangesChanged += (s, e) => { hasChangesChangedEventArgs = e; };
			AssertHasChanges("HasChanges false initially", false, false, false, hasChangesChangedEventArgs);

			var dummy = Factory.New<DummyDependantBusinessObject>();
			DependentCollection.AdditionalFilter = new ZQuery(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.NotEqual, "QQQ"); // change the index
			DependentCollection.Add(dummy);

			AssertHasChanges("HasChanges true after item Add()", true, false, true, hasChangesChangedEventArgs);
		}

		void AssertHasChanges(string message, bool expectedValue, bool expectHasChangesChangedEvent, bool expectObjectChange, HasChangesChangedEventArgs hasChangesChangedEventArgs)
		{
			AssertEquals(message, expectedValue, ((IBusinessObjectState)DependentCollection).HasChanges);
			AssertEquals("HasChangesChanged is fired", expectHasChangesChangedEvent, hasChangesChangedEventArgs != null);

			if (expectHasChangesChangedEvent)
			{
				AssertEquals("HasChangesChanged has expected argument values", expectObjectChange, hasChangesChangedEventArgs.ObjectJustWasChanged);

				var actualHasChanges = CalculateHasChanges(DependentCollection);
				if (actualHasChanges)
				{
					AssertEquals(message, true, ((IBusinessObjectState)DependentCollection).HasChanges);
				}
			}
		}

		static bool CalculateHasChanges(IActiveBusinessObjectCollection collection)
		{
			bool result = collection.HasChangesFromDelete;
			if (!result)
			{
				foreach (BusinessObject item in collection)
				{
					if (collection.Relationship.HasChangesIncludingRelationship(item))
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		#endregion

		#region OnListChanged / AssertListChanged

		readonly List<ListChangedEventArgs> listChangedEvents = new List<ListChangedEventArgs>();

		void OnListChanged(object sender, ListChangedEventArgs e)
		{
			listChangedEvents.Add(e);
		}

		void AssertListChanged(params ListChangedType[] expectedEventTypes)
		{
			AssertEquals("Expected number of events", expectedEventTypes.Length, listChangedEvents.Count);
			for (int i = 0; i < expectedEventTypes.Length; i++)
			{
				AssertEquals("Change event " + i, expectedEventTypes[i], listChangedEvents[i].ListChangedType);
			}
			listChangedEvents.Clear();
		}

		void AssertNoListChanged()
		{
			AssertListChanged(Array.Empty<ListChangedType>());
		}

		#endregion

		#region OnAdded

		public void TestOnAdded_ForAdd()
		{
			DummyDependantBusinessObject dummy = Factory.New<DummyDependantBusinessObject>();
			AssertEquals(null, DependentCollection.OnAddedLastCalledFor);
			DependentCollection.Add(dummy);
			AssertEquals(dummy, DependentCollection.OnAddedLastCalledFor);
		}

		public void TestOnAdded_ForAddNew()
		{
			AssertEquals(null, DependentCollection.OnAddedLastCalledFor);
			DummyDependantBusinessObject dummy = DependentCollection.AddNew();
			AssertEquals(dummy, DependentCollection.OnAddedLastCalledFor);
		}

		public void TestOnAdded_ForIBindingListAddNew()
		{
			AssertEquals(null, DependentCollection.OnAddedLastCalledFor);
			DummyDependantBusinessObject dummy = (DummyDependantBusinessObject)((IBindingList)DependentCollection).AddNew();
			AssertEquals(dummy, DependentCollection.OnAddedLastCalledFor);
		}

		#endregion

		#region Refresh data in collection after it is changed somewhere else

		public void TestReloadExistingRowsRefreshesCollections()
		{
			DummyBusinessObject dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Code = "D1";
			dummy1.Z0_VarCharMax = "T1";
			DummyBusinessObject dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Code = "D2";
			dummy2.Z0_VarCharMax = "T2";
			Factory.Save();

			BusinessObjectFactory workFactory = new BusinessObjectFactory();

			ActiveBusinessObjectCollection<DummyBusinessObject> dummies = new ActiveBusinessObjectCollection<DummyBusinessObject>(
				workFactory, new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "D"));
			AssertEquals(2, dummies.Count);

			using (DbCommand command = ((IDbConnected)Factory).Connection.Command("insert into dbo.DummyBizo (Z0_PK, Z0_Code, Z0_VarCharMax) values ('" + Guid.NewGuid().ToString() + "', 'D3', 'T3')"))
			{
				command.ExecuteNonQuery();
			}
			using (DbCommand command = ((IDbConnected)Factory).Connection.Command("update dbo.DummyBizo set Z0_VarCharMax = @string where Z0_Code = @code"))
			{
				command.AddParameterBasedOnDbColumn("@string", "TT", DummyBizoSchema.Z0_VarCharMax);
				command.AddParameterBasedOnDbColumn("@code", "D2", DummyBizoSchema.Z0_Code);
				command.ExecuteNonQuery();
			}

			AssertEquals(2, dummies.Count);

			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "D") { ReLoadExistingRows = true };
			AssertEquals(3, workFactory.Load<DummyBusinessObject>(query).Length);

			AssertEquals(3, dummies.Count);

			foreach (DummyBusinessObject dummy in dummies)
			{
				if (dummy.Z0_Code == "D2")
				{
					AssertEquals("TT", dummy.Z0_VarCharMax);
				}
			}
		}

		public void TestRefreshFromDB()
		{
			DummyBusinessObject dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Code = "D1";
			dummy1.Z0_VarCharMax = "T1";
			DummyBusinessObject dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Code = "D2";
			dummy2.Z0_VarCharMax = "T2";
			Factory.Save();

			BusinessObjectFactory workFactory = new BusinessObjectFactory();

			ActiveBusinessObjectCollection<DummyBusinessObject> dummies = new ActiveBusinessObjectCollection<DummyBusinessObject>(
				workFactory, new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "D"));
			AssertEquals(2, dummies.Count);

			using (DbCommand command = ((IDbConnected)Factory).Connection.Command("insert into dbo.DummyBizo (Z0_PK, Z0_Code, Z0_VarCharMax) values ('" + Guid.NewGuid().ToString() + "', 'D3', 'T3')"))
			{
				command.ExecuteNonQuery();
			}
			using (DbCommand command = ((IDbConnected)Factory).Connection.Command("update dbo.DummyBizo set Z0_VarCharMax = @string where Z0_Code = @code"))
			{
				command.AddParameterBasedOnDbColumn("@string", "TT", DummyBizoSchema.Z0_VarCharMax);
				command.AddParameterBasedOnDbColumn("@code", "D2", DummyBizoSchema.Z0_Code);
				command.ExecuteNonQuery();
			}

			AssertEquals(2, dummies.Count);

			dummies.RefreshFromDb();

			AssertEquals(3, dummies.Count);

			foreach (DummyBusinessObject dummy in dummies)
			{
				if (dummy.Z0_Code == "D2")
				{
					AssertEquals("TT", dummy.Z0_VarCharMax);
				}
			}
		}

		public void TestRefreshCollectionWithNewMatches()
		{
			DummyBusinessObject dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Code = "D1";
			dummy1.Z0_VarCharMax = "T1";
			Factory.Save();

			ActiveBusinessObjectCollection<DummyBusinessObject> dummies = new ActiveBusinessObjectCollection<DummyBusinessObject>(
				Factory, new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "D"));
			AssertEquals(1, dummies.Count);

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();

			DummyBusinessObject dummy2 = otherFactory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Code = "D2";
			dummy2.Z0_VarCharMax = "T2";
			otherFactory.Save();

			AssertEquals(2, dummies.Count);

			DummyBusinessObject dummy3 = otherFactory.NewWithValidTestData<DummyBusinessObject>();
			dummy3.Z0_Code = "Z3";
			dummy3.Z0_VarCharMax = "T3";
			otherFactory.Save();

			AssertEquals(2, dummies.Count);

			dummy3.Z0_Code = "D3";
			otherFactory.Save();

			AssertEquals(3, dummies.Count);
		}

		#endregion

		#region IBusiness

		public void TestSuspendResumeValidation()
		{
			DummyDependantBusinessObject element1 = DependentCollection.AddNew();
			DummyDependantBusinessObject element2 = DependentCollection.AddNew();

			int loaded = DependentCollection.Count;
			((IBusiness)DependentCollection).SuspendValidation();
			AssertEquals("Validation of all elements suspended", true, element1.IsValidationSuspended);
			AssertEquals("Validation of all elements suspended", true, element2.IsValidationSuspended);
			DummyDependantBusinessObject element3 = DependentCollection.AddNew();
			AssertEquals("Validation of all elements suspended", true, element3.IsValidationSuspended);

			DependentCollection.RemoveFromRelationship(element1);
			AssertEquals("Element no longer suspended when out of the collection", false, element1.IsValidationSuspended);

			((IBusiness)DependentCollection).ResumeValidation();
			AssertEquals("Validation of all elements resumed", false, element1.IsValidationSuspended);
			AssertEquals("Validation of all elements resumed", false, element2.IsValidationSuspended);
			AssertEquals("Validation of all elements resumed", false, element3.IsValidationSuspended);
		}

		public void TestSuspendResumeValidation_WhenChangingIndex()
		{
			DummyDependantBusinessObject element = DependentCollection.AddNew();
			int loaded = DependentCollection.Count;
			((IBusiness)DependentCollection).SuspendValidation();
			AssertEquals("Validation of elements suspended", true, element.IsValidationSuspended);

			DependentCollection.AdditionalFilter = new ZQuery(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.NotEqual, "x");
			loaded = DependentCollection.Count;
			AssertEquals("Validation of elements suspended", true, element.IsValidationSuspended);

			((IBusiness)DependentCollection).ResumeValidation();
			AssertEquals("Validation of elements resumed", false, element.IsValidationSuspended);
			DependentCollection.AdditionalFilter = new ZQuery();
			loaded = DependentCollection.Count;
			AssertEquals("Validation of elements resumed", false, element.IsValidationSuspended);
		}

		public void TestDelete()
		{
			DummyDependantBusinessObject element1 = DependentCollection.AddNew();
			DummyDependantBusinessObject element2 = DependentCollection.AddNew();
			((IBusiness)DependentCollection).Delete();
			AssertEquals(true, element1.IsDeleted);
			AssertEquals(true, element2.IsDeleted);
		}

		public void TestDelete_StillFiresListChangedIfUncommitted()
		{
			DummyDependantBusinessObject element = (DummyDependantBusinessObject)((IBindingList)DependentCollection).AddNew();
			((IBindingList)DependentCollection).ListChanged += OnListChanged;
			DependentCollection.Delete(element);
			AssertEquals("ItemDeleted event raised", ListChangedType.ItemDeleted, this.listChangedEvents[0].ListChangedType);
			AssertEquals("BusinessObject deleted", true, element.IsDeleted);
		}

		#endregion

		#region IBusinessObjectState

		public void TestNotificationsChanged()
		{
			bool notificationsChanged = false;
			((IBusinessObjectState)DependentCollection).NotificationsChanged += delegate
			{ notificationsChanged = true; };
			DummyDependantBusinessObject dummy = DependentCollection.AddNew();
			object populated = DependentCollection.Count; // NotificationsChanged in many cases will only work if the collection has been populated

			notificationsChanged = false;
			dummy.ZD1_Code = "error";
			AssertEquals("When notifications change", true, notificationsChanged);

			notificationsChanged = false;
			dummy.ZD1_Code = "";
			AssertEquals("When notifications change", true, notificationsChanged);

			notificationsChanged = false;
			dummy.ZD1_Code = "error";
			AssertEquals("When notifications change", true, notificationsChanged);

			notificationsChanged = false;
			DependentCollection.RemoveFromRelationship(dummy);
			AssertEquals("When a business object that had notifications is removed", true, notificationsChanged);

			notificationsChanged = false;
			DependentCollection.Add(dummy);
			AssertEquals("When a business object that has notifications is added", true, notificationsChanged);
		}

		public void TestSuspendNotificationChanged()
		{
			var notificationsChanged = false;
			var master = Factory.New<DummyWithDependentsBusinessObject>();
			var collection = new TestDependentActiveBusinessObjectCollection(master, new ZQuery(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.NotEqual, "EXL"));

			((IBusinessObjectState)collection).NotificationsChanged += delegate
			{ notificationsChanged = true; };

			var dummy = collection.AddNew();
			object populated = collection.Count; // NotificationsChanged in many cases will only work if the collection has been populated

			notificationsChanged = false;
			using (collection.SuspendNotificationsChangedExposed())
			{
				dummy.ZD1_Code = "error";
				AssertEquals("When notifications change", false, notificationsChanged);

				dummy.ZD1_Code = "";
				AssertEquals("When notifications change", false, notificationsChanged);

				collection.RemoveFromRelationship(dummy);
				AssertEquals("When a business object that had notifications is removed", false, notificationsChanged);

				collection.Add(dummy);
				AssertEquals("When a business object that has notifications is added", false, notificationsChanged);
			}
		}

		public void TestIncrementReadOnlyIncludingChildren()
		{
			DummyDependantBusinessObject element1 = DependentCollection.AddNew();
			AssertEquals("ReadOnly=false initially", false, element1.ReadOnly);

			int loaded = DependentCollection.Count;
			((IBusiness)DependentCollection).IncrementReadOnlyIncludingChildren();
			AssertEquals("ReadOnly=true after IncrementReadOnlyIncludingChildren called", true, element1.ReadOnly);

			DummyDependantBusinessObject element2 = DependentCollection.AddNew();
			AssertEquals("ReadOnly=true after IncrementReadOnlyIncludingChildren and new element added", true, element2.ReadOnly);

			DependentCollection.RemoveFromRelationship(element2);
			AssertEquals("ReadOnly=false when removed from the collection", false, element2.ReadOnly);

			((IBusiness)DependentCollection).IncrementReadOnlyIncludingChildren();
			((IBusiness)DependentCollection).DecrementReadOnlyIncludingChildren();
			AssertEquals("ReadOnly=true when increment called twice and decrement called once", true, element1.ReadOnly);
			((IBusiness)DependentCollection).DecrementReadOnlyIncludingChildren();
			AssertEquals("ReadOnly=false when everything decremented", false, element1.ReadOnly);
		}

		public void TestIncrementReadOnlyIncludingChildren_WhenIndexesChange()
		{
			DummyDependantBusinessObject element = DependentCollection.AddNew();
			int loaded = DependentCollection.Count;

			AssertEquals("ReadOnly=false initially", false, element.ReadOnly);
			((IBusiness)DependentCollection).IncrementReadOnlyIncludingChildren();
			AssertEquals("ReadOnly=true after IncrementReadOnlyIncludingChildren called", true, element.ReadOnly);
			DependentCollection.ApplySort(DummyDependentBizoSchema.ZD1_Code.Name, ListSortDirection.Ascending);
			loaded = DependentCollection.Count;
			AssertEquals("ReadOnly=true after index changed", true, element.ReadOnly);

			((IBusiness)DependentCollection).DecrementReadOnlyIncludingChildren();
			AssertEquals("ReadOnly=false after IncrementReadOnlyIncludingChildren called", false, element.ReadOnly);
			DependentCollection.RemoveSort();
			AssertEquals("ReadOnly=false after index changed", false, element.ReadOnly);
		}

		public void TestIncrementReadOnlyIncludingChildren_BusinessObjectReadOnlyResetOnCollect()
		{
			var collection = new TestDependentActiveBusinessObjectCollection(Master);
			var element = AddNewToUnreferencedCollectionAndIncrementReadOnlyIncludingChildren();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			AssertEquals("Count of other collection that has the same index", 1, collection.Count);
			AssertEquals("Elements not read only when collection is collected", false, element.ReadOnly);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		DummyDependantBusinessObject AddNewToUnreferencedCollectionAndIncrementReadOnlyIncludingChildren()
		{
			var collection = new TestDependentActiveBusinessObjectCollection(Master);
			var element = collection.AddNew();
			AssertEquals("Count", 1, collection.Count);
			((IBusiness)collection).IncrementReadOnlyIncludingChildren();
			AssertEquals("Elements read only when collection read only", true, element.ReadOnly);
			return element;
		}

		#endregion

		#region IBusinessObjectCollection

		public void TestAdditionalFilter_WithMaximumRows_BeforeLoad()
		{
			Master.Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyWithDependentsBusinessObject master = newFactory.Load<DummyWithDependentsBusinessObject>(Master.PK);
			DummyDependantBusinessObject businessObject1 = newFactory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject businessObject2 = newFactory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject businessObject3 = newFactory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject businessObject4 = newFactory.New<DummyDependantBusinessObject>();
			businessObject1.ZD1_Z0 = master.PK;
			businessObject2.ZD1_Z0 = master.PK;
			businessObject3.ZD1_Z0 = master.PK;
			businessObject4.ZD1_Z0 = master.PK;
			newFactory.Save();

			ZQuery filter = new ZQuery();
			filter.MaximumRows = 2;
			DependentCollection.AdditionalFilter = filter;
			AssertEquals("Maximum number of records loaded", 2, DependentCollection.Count);
		}

		public void TestAdditionalFilter_WithMaximumRows_AfterLoad()
		{
			ZQuery filter = new ZQuery();
			filter.MaximumRows = 2;
			DependentCollection.AdditionalFilter = filter;
			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);

			AssertEquals(0, DependentCollection.Count);
			DummyDependantBusinessObject businessObject1 = DependentCollection.AddNew();
			AssertListChanged(ListChangedType.ItemAdded);
			DummyDependantBusinessObject businessObject2 = DependentCollection.AddNew();
			AssertListChanged(ListChangedType.ItemAdded);
			AssertEquals(2, DependentCollection.Count);

			DummyDependantBusinessObject businessObject3 = DependentCollection.AddNew();
			AssertNoListChanged();
			DummyDependantBusinessObject businessObject4 = DependentCollection.AddNew();
			AssertNoListChanged();
			AssertEquals(2, DependentCollection.Count);

			businessObject1.Delete();
			AssertListChanged(ListChangedType.Reset);
			AssertEquals(2, DependentCollection.Count);
			AssertEquals(false, new List<BusinessObject>(DependentCollection.Cast()).Contains(businessObject1));
			AssertEquals(true, new List<BusinessObject>(DependentCollection.Cast()).Contains(businessObject2));
			AssertEquals(true, new List<BusinessObject>(DependentCollection.Cast()).Contains(businessObject3));
			AssertEquals(false, new List<BusinessObject>(DependentCollection.Cast()).Contains(businessObject4));
		}

		public void TestRefresh()
		{
			DummyDependantBusinessObject dummy1 = DependentCollection.AddNew();
			DummyDependantBusinessObject dummy2 = DependentCollection.AddNew();
			dummy1.ZD1_Code = "X";
			dummy2.ZD1_Code = "Y";

			((IBindingList)DependentCollection).ListChanged += new ListChangedEventHandler(OnListChanged);
			AssertEquals("Both elements in the collection initially", 2, DependentCollection.Count);
			DependentCollection.ExcludedElement = dummy1;
			((IActiveBusinessObjectCollection)DependentCollection).Refresh();

			AssertListChanged(ListChangedType.Reset);
			AssertEquals("1 element excluded from collection", 1, DependentCollection.Count);
		}

		#endregion

		#region IBindingList

		public void TestAddNewUncommitted()
		{
			DummyDependantBusinessObject element = (DummyDependantBusinessObject)((IBindingList)DependentCollection).AddNew();
			AssertEquals("IBindingList.AddNew'd item not committed", DataRowState.Detached, element.Row.RowState);

			element.ZD1_Code = "x";
			((ICancelAddNew)DependentCollection).EndNew(DependentCollection.Count - 1);
			AssertEquals("ICancelAddNew.EndNew'd item committed", DataRowState.Added, element.Row.RowState);
		}

		public void TestContains_WithMaximumRows()
		{
			Master.Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyWithDependentsBusinessObject master = newFactory.Load<DummyWithDependentsBusinessObject>(Master.PK);
			DummyDependantBusinessObject businessObject1 = newFactory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject businessObject2 = newFactory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject businessObject3 = newFactory.New<DummyDependantBusinessObject>();
			DummyDependantBusinessObject businessObject4 = newFactory.New<DummyDependantBusinessObject>();
			businessObject1.ZD1_Z0 = master.PK;
			businessObject2.ZD1_Z0 = master.PK;
			businessObject3.ZD1_Z0 = master.PK;
			businessObject4.ZD1_Z0 = master.PK;
			newFactory.Save();

			ZQuery filter = new ZQuery();
			filter.MaximumRows = 2;
			DependentCollection.AdditionalFilter = filter;

			int containsCount = 0;

			if (DependentCollection.Contains(Factory.Load<DummyDependantBusinessObject>(businessObject1.PK)))
			{
				containsCount++;
			}

			if (DependentCollection.Contains(Factory.Load<DummyDependantBusinessObject>(businessObject2.PK)))
			{
				containsCount++;
			}

			if (DependentCollection.Contains(Factory.Load<DummyDependantBusinessObject>(businessObject3.PK)))
			{
				containsCount++;
			}

			if (DependentCollection.Contains(Factory.Load<DummyDependantBusinessObject>(businessObject4.PK)))
			{
				containsCount++;
			}

			AssertEquals("Contains returns true for 2 elements due to MaximumRows", 2, containsCount);
		}

		public void TestSortInformation()
		{
			DummyDependantBusinessObject element1 = DependentCollection.AddNew();
			DummyDependantBusinessObject element2 = DependentCollection.AddNew();
			DummyDependantBusinessObject element3 = DependentCollection.AddNew();

			AssertNull(((IBusinessObjectCollection)DependentCollection).SortInformation);

			DependentCollection.ApplySort(DummyDependantBusinessObject.Schema.ZD1_Number, ListSortDirection.Descending);
			AssertEquals(DummyDependantBusinessObject.Schema.ZD1_Number, ((IBusinessObjectCollection)DependentCollection).SortInformation.PropertyName);
			AssertEquals(ListSortDirection.Descending, ((IBusinessObjectCollection)DependentCollection).SortInformation.Direction);
		}

		public void TestIndexOf()
		{
			DependentCollection.AddNew();
			DependentCollection.AddNew();
			DependentCollection.AddNew();
			DependentCollection.AddNew();

			var element1 = DependentCollection[3];
			var element2 = DependentCollection[2];
			var element3 = DependentCollection[1];
			var element4 = DependentCollection[0];
			element4.ZD1_Code = "1";
			element3.ZD1_Code = "2";
			element2.ZD1_Code = "3";
			element1.ZD1_Code = "4";

			AssertContainsExactElementsInAnyOrder(Enumerable.Range(0, 4),
				new[]
				{
					DependentCollection.IndexOf(element1),
					DependentCollection.IndexOf(element2),
					DependentCollection.IndexOf(element3),
					DependentCollection.IndexOf(element4),
				});

			element2.Delete();
			AssertEquals(-1, DependentCollection.IndexOf(element2));
			AssertContainsExactElementsInAnyOrder(Enumerable.Range(0, 3),
				new[]
				{
					DependentCollection.IndexOf(element1),
					DependentCollection.IndexOf(element3),
					DependentCollection.IndexOf(element4),
				});

			DependentCollection.ApplySort(DummyDependentBizoSchema.ZD1_Code.Name, ListSortDirection.Ascending);
			AssertEquals(2, DependentCollection.IndexOf(element1));
			AssertEquals(1, DependentCollection.IndexOf(element3));
			AssertEquals(0, DependentCollection.IndexOf(element4));

			element3.Delete();
			AssertEquals(1, DependentCollection.IndexOf(element1));
			AssertEquals(0, DependentCollection.IndexOf(element4));
		}

		#endregion

		#region IFindBoxListProvider

		public void TestDescriptionFromPrimaryKey()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "AAA";
			dummy1.Z0_Description = "BBBBBBB";

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "CCC";
			dummy2.Z0_Description = "DDDDDD";

			AssertEquals("Matches PK", dummy2.Z0_Description, ((IFindBoxListProvider)Collection).DescriptionFromPrimaryKey(dummy2.PK));
			AssertNull("Non-existent item", ((IFindBoxListProvider)Collection).DescriptionFromPrimaryKey(ZGuid.NewZGuid()));
			AssertNull("Invalid PK", ((IFindBoxListProvider)Collection).DescriptionFromPrimaryKey(ZGuid.Invalid));
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

			ActiveBusinessObjectCollection<CalculableSortableObject> collection = new ActiveBusinessObjectCollection<CalculableSortableObject>(factory1,
				new ZQuery(DummyBizoSchema.Z0_Code, "SRT"));
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

		class DummyFetch : BusinessObjectFetchStrategy
		{
			public DummyFetch(CalculableSortableObject businessObject)
				: base(businessObject)
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

		#region StackOverflowOnPopulation

		[ExpectNoExceptions]
		public void TestStackOverflowOnPopulation()
		{
			PrepareTestData();

			using (MockForm form = new MockForm(Factory.New<Super>()))
			{
				AssertEquals(1, form.StartBinding());
			}
		}

		#region Test Form

		class MockForm : IDisposable
		{
			public MockForm(Super super)
			{
				this.super = super;
				((IBusinessObjectState)super).IncrementReadOnlyIncludingChildren();
				super.Dummies.ApplySort("Z0_Code", ListSortDirection.Ascending);
				((IBusinessObjectState)super.Dummies).NotificationsChanged += Parents_NotificationsChanged;
			}

			readonly Super super;

			void Parents_NotificationsChanged(object sender, NotificationsChangedEventArgs e)
			{
				Assert("Try to do a dead loop", super.Dummies.Count >= 0);
			}

			public int StartBinding()
			{
				return super.Dummies.Count;
			}

			public void Dispose()
			{
				((IBusinessObjectState)super.Dummies).NotificationsChanged -= Parents_NotificationsChanged;
			}
		}

		#endregion

		#region Test Data

		void PrepareTestData()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			TestDummy dummy1 = factory.New<TestDummy>();
			dummy1.Z0_Code = "PRN";

			factory.New<TestDummy>().Z0_Guid = dummy1.PK;

			factory.Save();
		}

		class TestDummy : DummyBusinessObject
		{
			public TestDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				Assert("Pre-populate children collection", Children.Count >= 0);
			}

			Dummies Children
			{
				get
				{
					if (children == null)
					{
						children = new Dummies(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, PK));
						RegisterEditableChildObject(children);
					}
					return children;
				}
			}
			Dummies children;
		}

		class Dummies : ActiveBusinessObjectCollection<TestDummy>
		{
			public Dummies(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter) { }
		}

		class Super : DummyBusinessObject
		{
			public Super(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public Dummies Dummies
			{
				get
				{
					if (dummies == null)
					{
						dummies = new Dummies(Factory, new ZQuery(DummyBizoSchema.Z0_Code, "PRN"));
						RegisterEditableChildObject(dummies);
					}
					return dummies;
				}
			}
			Dummies dummies;
		}

		#endregion

		#endregion

		#region TestGetEnumerator

		public void TestGetEnumerator()
		{
			Factory.NewWithValidTestData<DummyBusinessObject>().Z0_Code = "XX1";
			Factory.NewWithValidTestData<DummyBusinessObject>().Z0_Code = "XX2";
			Factory.NewWithValidTestData<DummyBusinessObject>().Z0_Code = "YY1";

			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);

			var enumerator = collection.GetEnumerator();
			AssertSame(typeof(ActiveBusinessObjectCollection<DummyBusinessObject>.IndexEnumeratorWrapper), enumerator.GetType());

			var wrapper = (ActiveBusinessObjectCollection<DummyBusinessObject>.IndexEnumeratorWrapper)enumerator;
			AssertSame(collection, wrapper.Collection);

			var count = 0;
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Z0_Code.StartsWith("XX"))
				{
					count++;
				}
			}
			AssertEquals(2, count);

			enumerator.Dispose();
			AssertNull(wrapper.Collection);
		}

		#endregion

		#region Test Classes

		public class TestActiveBusinessObjectCollection : ActiveBusinessObjectCollection<DummyDependantBusinessObject>
		{
			public TestActiveBusinessObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public void SetHookOnIndexDisposed(Action action)
			{
				IndexExposed.Disposed += (s, e) => action();
			}
		}

		class TestDependentActiveBusinessObjectCollection : ActiveBusinessObjectCollection<DummyDependantBusinessObject>
		{
			public TestDependentActiveBusinessObjectCollection(BusinessObject master)
				: base(master)
			{
			}

			public TestDependentActiveBusinessObjectCollection(BusinessObject master, ZQuery relationshipFilter)
				: base(master)
			{
				this.relationshipFilter = relationshipFilter;
			}

			public TestDependentActiveBusinessObjectCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
				: base(factory, relationship)
			{
			}

			public DummyDependantBusinessObject ExcludedElement;

			protected override bool MatchesFilterCore(DummyDependantBusinessObject element, bool fetchOnlyFromLocalCache)
			{
				return base.MatchesFilterCore(element, fetchOnlyFromLocalCache) && element != ExcludedElement;
			}

			protected override ZQuery CreateRelationshipFilter()
			{
				return relationshipFilter;
			}
			readonly ZQuery relationshipFilter;

			public DummyDependantBusinessObject OnAddedLastCalledFor;

			protected override void OnAdded(DummyDependantBusinessObject businessObject)
			{
				base.OnAdded(businessObject);
				OnAddedLastCalledFor = businessObject;
			}

			public IDisposable SuspendNotificationsChangedExposed() => SuspendNotificationsChanged();
		}

		class TestDeeperCollection : TestDependentActiveBusinessObjectCollection
		{
			public TestDeeperCollection(BusinessObject master) : base(master) { }
		}

		#endregion

		#region Implementation

		DummyWithDependentsBusinessObject Master
		{
			get
			{
				if (master == null)
				{
					master = Factory.New<DummyWithDependentsBusinessObject>();
				}
				return master;
			}
		}
		DummyWithDependentsBusinessObject master;

		ActiveBusinessObjectCollection<DummyBusinessObject> Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
				}
				return collection;
			}
		}
		ActiveBusinessObjectCollection<DummyBusinessObject> collection;

		TestDependentActiveBusinessObjectCollection DependentCollection
		{
			get
			{
				if (dependentCollection == null)
				{
					dependentCollection = new TestDependentActiveBusinessObjectCollection(Master, new ZQuery(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.NotEqual, "EXL"));
				}
				return dependentCollection;
			}
		}
		TestDependentActiveBusinessObjectCollection dependentCollection;

		#endregion
	}
}
