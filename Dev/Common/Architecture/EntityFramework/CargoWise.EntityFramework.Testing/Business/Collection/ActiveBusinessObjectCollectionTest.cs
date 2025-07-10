using System;
using System.ComponentModel;
using System.Linq;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestedType(typeof(ActiveBusinessObjectCollection<DummyBusinessObject>))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1134:Non-abstract Test Classes Should Be Sealed", Justification = "This test class is both instanciated and inherited so unable to make abstract or sealed")]
	public class ActiveBusinessObjectCollectionTest : ActiveBusinessObjectCollectionTestCase<ActiveBusinessObjectCollection<DummyBusinessObject>>
	{
		protected override ActiveBusinessObjectCollection<DummyBusinessObject> GetCollectionToTest()
		{
			return new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<DummyBusinessObject>();
		}

		public void TestIBindingListApplySort()
		{
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo.Z0_Code = "XX1";
			Factory.NewWithValidTestData<DummyBusinessObject>().Z0_Code = "XX2";
			Factory.NewWithValidTestData<DummyBusinessObject>().Z0_Code = "YY1";
			Factory.Save();

			var factory1 = new BusinessObjectFactory();

			using (ActiveBusinessObjectCollection.TrackIndexedCollectionCounts_ForTest())
			{
				AssertEquals(0, ActiveBusinessObjectCollection.IndexedCollections.Count);

				var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(factory1);

				((IBindingList)collection).ApplySort(bizo.GetProperties()["Z0_Code"], ListSortDirection.Ascending);
				_ = collection.Count;

				AssertEquals("Should only be 1 collection indexed", 1, ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.GetInstance(factory1).All.Count());
			}
		}

		public void TestIBindingListViewApplySort()
		{
			var bizo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo1.Z0_Code = "XX1";
			bizo1.Z0_Decimal = 1m;
			var bizo2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo2.Z0_Code = "XX2";
			bizo2.Z0_Decimal = 2m;
			var bizo3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo3.Z0_Code = "XX3";
			bizo3.Z0_Decimal = 3m;

			Factory.Save();

			var factory1 = new BusinessObjectFactory();

			using (ActiveBusinessObjectCollection.TrackIndexedCollectionCounts_ForTest())
			{
				AssertEquals(0, ActiveBusinessObjectCollection.IndexedCollections.Count);

				var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(factory1);

				var properties = bizo1.GetProperties();
				var sortCode = new ListSortDescription(properties["Z0_Code"], ListSortDirection.Ascending);
				var sortDecimal = new ListSortDescription(properties["Z0_Decimal"], ListSortDirection.Ascending);
				var sortCollection = new ListSortDescriptionCollection(new[] { sortCode, sortDecimal });

				((IBindingListView)collection).ApplySort(sortCollection);
				_ = collection.Count;

				AssertEquals("Should only be 1 collection indexed", 1, ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.GetInstance(factory1).All.Count());
			}
		}

		public void TestNoResultsQueryCache()
		{
			var parent = Factory.NewWithValidTestData<DummyBusinessObject>();
			var collection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(Factory, parent, new ZQuery(DummyDependentBizoSchema.ZD1_Number, SQLComparisonOperator.GreaterThan, 0), DummyDependentBizoSchema.ZD1_Z0);

			collection.AddNew().ZD1_Number = 1;
			collection.AddNew().ZD1_Number = 2;
			collection.AddNew().ZD1_Number = 3;

			Factory.Save();

			AssertEquals(3, collection.Count); // Force an initial load
			AssertEquals("One db hit after initial load", 1, Factory.GetTableHitCount(DummyDependentBizoSchema.Constants.TableName));

			collection.AdditionalFilter = ZQuery.NoResultQuery;
			var newFactory = new BusinessObjectFactory();
			((IActiveBusinessObjectCollection)collection).SetFactory(newFactory);
			AssertEquals("Our AdditionalFilter should now stop the collection from loading anything", 0, collection.Count);

			collection.AdditionalFilter = new ZQuery();
			AssertEquals("Now we should be able to load the collection", 3, collection.Count);
			AssertEquals("One new db hit after second load", 1, newFactory.GetTableHitCount(DummyDependentBizoSchema.Constants.TableName));
		}

		public void TestHasChangesFromDatabase()
		{
			var d1 = Collection.AddNew();
			var d2 = Collection.AddNew();
			var d3 = Collection.AddNew();
			var d4 = Collection.AddNew();
			Factory.Save();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var collectionInFactory1 = new ActiveBusinessObjectCollection<DummyBusinessObject>(factory1);
			var collectionInFactory2 = new ActiveBusinessObjectCollection<DummyBusinessObject>(factory2);

			//1) Test that if there are no changes in the database, it returns false.

			AssertEquals(false, ((IBusinessObjectCollectionInternals)collectionInFactory1).HasChangesFromDatabase());

			//2) Test that adding/deleting/modifying rows locally still returns false, and that modifying a row in the database returns false.

			collectionInFactory1[0].Z0_Description = "azx";
			collectionInFactory1.AddNew();
			collectionInFactory1[2].Delete();
			AssertEquals(false, ((IBusinessObjectCollectionInternals)collectionInFactory1).HasChangesFromDatabase());

			collectionInFactory2[0].Z0_Description = "dfu";
			factory2.Save();
			AssertEquals(false, ((IBusinessObjectCollectionInternals)collectionInFactory1).HasChangesFromDatabase());

			//3) Test that deleting a row in the database returns true.
			collectionInFactory2[0].Delete();
			factory2.Save();
			AssertEquals(true, ((IBusinessObjectCollectionInternals)collectionInFactory1).HasChangesFromDatabase());

			//4) Test that adding a row in the database returns true.
			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			var collectionInFactory3 = new ActiveBusinessObjectCollection<DummyBusinessObject>(factory3);
			AssertEquals(false, ((IBusinessObjectCollectionInternals)collectionInFactory3).HasChangesFromDatabase());

			collectionInFactory2.AddNew();
			factory2.Save();
			AssertEquals(true, ((IBusinessObjectCollectionInternals)collectionInFactory3).HasChangesFromDatabase());
		}

		public void TestSuspendListChanged()
		{
			IBusinessObjectCollection iCollection = Collection;
			bool fired = false;
			ListChangedEventHandler handler = (o, e) => { fired = true; };
			iCollection.ListChanged += handler;
			try
			{
				Assert("Precondition", !fired);
				using (iCollection.SuspendListChanged())
				{
					iCollection.AddNew();
					Assert("Should delay list events", !fired);
				}
				Assert("Should fire delayed list events now", fired);
			}
			finally
			{
				iCollection.ListChanged -= handler;
			}
		}

		public void TestCollectionCountChange_DataRefreshBusDelete()
		{
			var newDummyBiZo = Collection.AddNew();
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			var collectionInFactory1 = new ActiveBusinessObjectCollection<DummyBusinessObject>(factory1);
			var collectionInFactory2 = new ActiveBusinessObjectCollection<DummyBusinessObject>(factory2);
			AssertEquals(newDummyBiZo.PK, collectionInFactory1.Single().PK);
			AssertEquals(newDummyBiZo.PK, collectionInFactory2.Single().PK);

			var countChangedCalled = false;
			collectionInFactory2.CollectionCountChange += (s, e) => countChangedCalled = true;
			collectionInFactory1.Single().Delete();
			AssertEquals(false, countChangedCalled);
			AssertEquals(1, collectionInFactory2.Count);
			factory1.Save();
			AssertEquals(0, collectionInFactory2.Count);
			AssertEquals(true, countChangedCalled);
		}

		public class DummyActiveBusinessObjectCollection : ActiveBusinessObjectCollection<DummyDependantBusinessObject>
		{
			public DummyActiveBusinessObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public void SetHookOnIndexDisposed(Action action)
			{
				IndexExposed.Disposed += (s, e) => action();
			}
		}
	}
}
