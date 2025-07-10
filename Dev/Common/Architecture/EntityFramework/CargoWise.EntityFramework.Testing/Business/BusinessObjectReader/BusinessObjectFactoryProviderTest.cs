using System;
using CargoWise.Data;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectFactoryProviderTest : TestCaseWithFactory
	{
		public void TestConstructorWithConnection()
		{
			using (DbConnection connection = Db.NewExtraConnectionToMainDb())
			{
				BusinessObjectFactoryProvider provider = new BusinessObjectFactoryProvider(connection);
				AssertEquals("Connection is used in the Current Factory", connection, ((IDbConnected)provider.Current).Connection);

				provider.SaveCurrentAndCreateNew();
				AssertEquals("Connection is used in the new Current Factory", connection,
					((IDbConnected)provider.Current).Connection);
			}
		}

		public void TestUsingSubclassedFactory()
		{
			BusinessObjectFactoryProvider provider = new BusinessObjectFactoryProvider(new BusinessObjectFactory2());
			AssertEquals("Subclassed factory in use", "BusinessObjectFactory2", provider.Current.GetType().Name);

			provider.CreateNewAndReclaimMemoryWithoutSave();
			AssertEquals("Subclassed factory in use after CreateNew", "BusinessObjectFactory2", provider.Current.GetType().Name);
		}

		public void TestRemoveCurrentFactory()
		{
			BusinessObjectFactoryProvider provider = new BusinessObjectFactoryProvider(new BusinessObjectFactory());
			Assert("Should have current factory", provider.HasCurrentFactory);
			provider.RemoveCurrent();
			Assert("Current factory should be removed", !provider.HasCurrentFactory);
		}

		public void TestSaveCurrentAndUpdateRecordCounts()
		{
			Factory.NewWithValidTestData(typeof(DummyBusinessObject));
			Factory.Save();

			BusinessObjectFactory initialFactory = new BusinessObjectFactory();
			DummyBusinessObject modified = initialFactory.LoadTop1<DummyBusinessObject>(new ZQuery());
			modified.Z0_Code = "ZAZZZ";
			DummyBusinessObject added = initialFactory.NewWithValidTestData<DummyBusinessObject>();

			BusinessObjectFactoryProvider provider = new BusinessObjectFactoryProvider(initialFactory);
			AssertEquals("Precondition: 0 record added", 0, provider.RecordsAdded);
			AssertEquals("Precondition: 0 record modified", 0, provider.RecordsModified);

			provider.SaveCurrentAndUpdateRecordCounts();
			AssertEquals("Should have not created a new factory", initialFactory, provider.Current);
			AssertEquals("1 record added", 1, provider.RecordsAdded);
			AssertEquals("1 record modified", 1, provider.RecordsModified);

			provider.SaveCurrentAndUpdateRecordCounts();
			AssertEquals("Should still be the same factory", initialFactory, provider.Current);
			AssertEquals("1 record added", 1, provider.RecordsAdded);
			AssertEquals("1 record modified", 1, provider.RecordsModified);

			modified = provider.Current.Load<DummyBusinessObject>(modified.PK);
			modified.Z0_Code = "ZAXXX";
			provider.SaveCurrentAndUpdateRecordCounts();
			AssertEquals("Should still be the same factory", initialFactory, provider.Current);
			AssertEquals("1 record added", 1, provider.RecordsAdded);
			AssertEquals("2 records modified", 2, provider.RecordsModified);

			added = provider.Current.NewWithValidTestData<DummyBusinessObject>();
			provider.SaveCurrentAndUpdateRecordCounts();
			AssertEquals("Should still be the same factory", initialFactory, provider.Current);
			AssertEquals("2 record added", 2, provider.RecordsAdded);
			AssertEquals("2 records modified", 2, provider.RecordsModified);
		}

		public void TestSaveCurrentAndCreateNew()
		{
			Factory.NewWithValidTestData(typeof(DummyBusinessObject));
			Factory.Save();

			BusinessObjectFactory initialFactory = new BusinessObjectFactory();
			DummyBusinessObject modified = initialFactory.LoadTop1<DummyBusinessObject>(new ZQuery());
			modified.Z0_Code = "ZAZZZ";
			DummyBusinessObject added = initialFactory.NewWithValidTestData<DummyBusinessObject>();

			BusinessObjectFactoryProvider provider = new BusinessObjectFactoryProvider(initialFactory);
			provider.SaveCurrentAndCreateNew();
			AssertEquals("Should have created a new factory", true, initialFactory != provider.Current);
			AssertEquals("1 record added", 1, provider.RecordsAdded);
			AssertEquals("1 record modified", 1, provider.RecordsModified);

			provider.SaveCurrentAndCreateNew();
			AssertEquals("1 record added", 1, provider.RecordsAdded);
			AssertEquals("1 record modified", 1, provider.RecordsModified);

			modified = provider.Current.Load<DummyBusinessObject>(modified.PK);
			modified.Z0_Code = "ZAXXX";
			provider.SaveCurrentAndCreateNew();
			AssertEquals("1 record added", 1, provider.RecordsAdded);
			AssertEquals("2 records modified", 2, provider.RecordsModified);
		}

		public void TestSaveCurrentAndCreateNew_RetainsValidationSuspended()
		{
			BusinessObjectFactoryProvider provider = new BusinessObjectFactoryProvider();

			provider.Current.SuspendValidation();
			provider.SaveCurrentReclaimMemoryAndCreateNew();
			AssertEquals("Should retain RefreshEnabled", true, provider.Current.IsValidationSuspended);
			provider.Current.ResumeValidation();
			provider.SaveCurrentReclaimMemoryAndCreateNew();
			AssertEquals("Should retain RefreshEnabled", false, provider.Current.IsValidationSuspended);
		}

		public void TestSaveCurrentAndCreateNew_RetainsRefreshEnabled()
		{
			BusinessObjectFactoryProvider provider = new BusinessObjectFactoryProvider();

			provider.Current.RefreshEnabled = false;
			provider.SaveCurrentReclaimMemoryAndCreateNew();
			AssertEquals("Should retain RefreshEnabled", false, provider.Current.RefreshEnabled);
			provider.Current.RefreshEnabled = true;
			provider.SaveCurrentReclaimMemoryAndCreateNew();
			AssertEquals("Should retain RefreshEnabled", true, provider.Current.RefreshEnabled);
		}

		public void TestSaveCurrentAndCreateNew_RetainsShouldPerformFullQueryCacheClean()
		{
			BusinessObjectFactoryProvider provider = new BusinessObjectFactoryProvider();

			provider.Current.ShouldPerformFullQueryCacheCleanOnSave = false;
			provider.SaveCurrentReclaimMemoryAndCreateNew();
			AssertEquals("Should retain ShouldPerformFullQueryCacheCleanOnSave", false, provider.Current.ShouldPerformFullQueryCacheCleanOnSave);
			provider.Current.ShouldPerformFullQueryCacheCleanOnSave = true;
			provider.SaveCurrentReclaimMemoryAndCreateNew();
			AssertEquals("Should retain ShouldPerformFullQueryCacheCleanOnSave", true, provider.Current.ShouldPerformFullQueryCacheCleanOnSave);
		}

		public void TestSaveCurrentAndCreateNew_RetainsConnection()
		{
			BusinessObjectFactory initialFactory = new BusinessObjectFactory(Db.DatabaseName);
			BusinessObjectFactoryProvider provider = new BusinessObjectFactoryProvider();
			DbConnection extraConnection = ((IDbConnected)initialFactory).Connection;

			provider.SaveCurrentReclaimMemoryAndCreateNew();
			AssertEquals("Should retain extra connection", extraConnection, ((IDbConnected)provider.Current).Connection);
		}

		public void TestIFactoryProvider()
		{
			BusinessObjectFactoryProvider provider = new BusinessObjectFactoryProvider();
			AssertEquals("Should return the current factory", provider.Current, ((IFactoryProvider)provider).Factory);
		}

		int handlerCount;
		void Provider_CurrentFactoryChanged(object sender, EventArgs e)
		{
			handlerCount++;
		}

		public void TestFireCurrentFactoryChangedEvent()
		{
			BusinessObjectFactoryProvider provider = new BusinessObjectFactoryProvider(new BusinessObjectFactory());
			provider.CurrentFactoryChanged += Provider_CurrentFactoryChanged;

			provider.SaveCurrentAndCreateNew();
			AssertEquals("Should fire current factory changed event.", 1, handlerCount);

			provider.SaveCurrentAndUpdateRecordCounts();
			provider.RemoveCurrent();
			AssertEquals("Should not fire Current factory changed event.", 1, handlerCount);
		}

		#region Implementation
		class BusinessObjectFactory2 : BusinessObjectFactory
		{
			public override BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
			{
				return new BusinessObjectFactory2();
			}

			public override BusinessObject Load(Type bizOType, Types.ZGuid pK)
			{
				throw new NotSupportedException();
			}

			public override BusinessObject[] Load(Type bizOType, ZQuery sqlFilter)
			{
				throw new NotSupportedException();
			}
		}
		#endregion
	}
}
