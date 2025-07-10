using System.Collections.Generic;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ActiveBusinessObjectCollectionDataRefreshAdderTest : TestCaseWithFactory
	{
		public void TestAddingViaDataRefreshBus_ForNonDependentCollection()
		{
			var newFactory = new BusinessObjectFactory();
			var amount = 3;
			for (var i = 0; i < amount; ++i)
			{
				var businessObject1 = newFactory.New<DummyBusinessObject>();
			}

			AssertEquals("No items before the other factory saves", 0, Collection.Count);
			newFactory.Save();
			AssertEquals("All items in collection after factory save", amount, Collection.Count);
		}

		public void TestAddingViaDataRefreshBus_ForDependentCollection()
		{
			Master.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var master = newFactory.Load<DummyWithDependentsBusinessObject>(Master.PK);
			var amount = 3;
			for (var i = 0; i < amount; ++i)
			{
				var businessObject1 = newFactory.New<DummyDependantBusinessObject>();
				businessObject1.ZD1_Z0 = master.PK;
			}

			AssertEquals("No items before the other factory saves", 0, DependentCollection.Count);
			newFactory.Save();
			AssertEquals("All items in collection after factory save", amount, DependentCollection.Count);
		}

		public void TestAddingViaDataRefreshBus_ForDependentCollection_DBOnlyQuery()
		{
			Master.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var master = newFactory.Load<DummyWithDependentsBusinessObject>(Master.PK);
			var amount = 3;
			for (var i = 0; i < amount; ++i)
			{
				var businessObject1 = newFactory.New<DummyDependantBusinessObject>();
				businessObject1.ZD1_Z0 = master.PK;
			}

			var dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyDependantBusinessObject));
			dbOnlyQuery.AddFilterAndZSQLParameterCollection("1=1", new ZSqlParameterCollection());
			DependentCollection.AdditionalFilter.AddToFilter(dbOnlyQuery);
			AssertEquals("No items before the other factory saves", 0, DependentCollection.Count);
			newFactory.Save();
			AssertEquals("All items in collection after factory save", amount, DependentCollection.Count);
		}

		public void TestAddingViaDataRefreshBus_ForDependentCollection_DBOnlyQuery_TVP()
		{
			Master.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var master = newFactory.Load<DummyWithDependentsBusinessObject>(Master.PK);
			var amount = 6;
			for (var i = 0; i < amount; ++i)
			{
				var businessObject1 = newFactory.New<DummyDependantBusinessObject>();
				businessObject1.ZD1_Z0 = master.PK;
			}

			var dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyDependantBusinessObject));
			dbOnlyQuery.AddFilterAndZSQLParameterCollection("1=1", new ZSqlParameterCollection());
			DependentCollection.AdditionalFilter.AddToFilter(dbOnlyQuery);
			AssertEquals("No items before the other factory saves", 0, DependentCollection.Count);
			newFactory.Save();
			AssertEquals("All items in collection after factory save", amount, DependentCollection.Count);
		}

		#region TestActiveBizoIndexInWebDoesNotUsesDataRefreshBus

		public void TestActiveBizoIndexInWebDoesNotUsesDataRefreshBus()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.IsWeb = true;
				settings.DebugBusinessObjectType = "Fake";
				settings.RegistryRefreshFrequencyInSeconds = 600;
				settings.ReportCrossThreadFactoryAccess = false;

				var preValue = new List<BusinessObjectFactory>(DataRefreshManager.Factories).Count;

				var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(new BusinessObjectFactory());
				collection.AddNew();
				AssertEquals("Ensure ActiveBusinessCollectionIndex<T> instance is created.", 1, collection.Count);
				AssertEquals("No new factories!", preValue, new List<BusinessObjectFactory>(DataRefreshManager.Factories).Count);
			}
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

		ActiveBusinessObjectCollection<DummyDependantBusinessObject> DependentCollection
		{
			get
			{
				if (dependentCollection == null)
				{
					dependentCollection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(Master);
				}
				return dependentCollection;
			}
		}
		ActiveBusinessObjectCollection<DummyDependantBusinessObject> dependentCollection;

		#endregion
	}
}
