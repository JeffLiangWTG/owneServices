using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework.Extensions;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectFactoryExtensionMethodsTest : TestCaseWithFactory
	{
		#region TestSynchroniseCachedBusinessObjectsWithDB

		public void TestSynchroniseCachedBusinessObjectsWithDB()
		{
			var query = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "AA");
			query.OrderBy = DummyBizoSchema.Constants.Z0_Code;

			// create and save some Dummy objects

			var dummy0 = Factory.New<DummyBusinessObject>();
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy3 = Factory.New<DummyBusinessObject>();
			var dummy4 = Factory.New<DummyBusinessObject>();
			dummy0.Z0_Code = "AA0";
			dummy1.Z0_Code = "AA1";
			dummy2.Z0_Code = "AA2";
			dummy3.Z0_Code = "AA3";
			dummy4.Z0_Code = "AA4";

			dummy0.Z0_Number = 100; // modify BizO in DB before cache.
			dummy1.Z0_Number = 100; // modify BizO in DB before cache.

			Factory.Save();

			// Cache BizO's
			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummiesInOtherFactory = otherFactory.Load<DummyBusinessObject>(query);

			AssertEquals("Precondition - Dummies should be loaded.", 5, dummiesInOtherFactory.Length);
			AssertEquals("Precondition - Dummy should be loaded.", dummy0.PK, dummiesInOtherFactory[0].PK);
			AssertEquals("Precondition - Dummy should be loaded.", dummy1.PK, dummiesInOtherFactory[1].PK);
			AssertEquals("Precondition - Dummy should be loaded.", dummy2.PK, dummiesInOtherFactory[2].PK);
			AssertEquals("Precondition - Dummy should be loaded.", dummy3.PK, dummiesInOtherFactory[3].PK);
			AssertEquals("Precondition - Dummy should be loaded.", dummy4.PK, dummiesInOtherFactory[4].PK);

			dummy0.Z0_Number = 50; // modify in DB cached BizO (100 in cache, 50 in DB).
			dummy1.Z0_Number = 50; // modify in DB cached BizO (100 in cache, 50 in DB).
			dummy2.Delete(); // delete from DB cached BizO
			dummy3.Delete(); // delete from DB cached BizO

			Factory.Save();

			var oldDummyInOtherFactory2 = dummiesInOtherFactory[2]; // save link to bizO deleted in DB.
			var oldDummyInOtherFactory4 = dummiesInOtherFactory[4]; // save link to bizO deleted in cache.

			dummiesInOtherFactory[1].Z0_Number = 200; // modify bizO in cache that was modified in DB.
			dummiesInOtherFactory[3].Z0_Number = 200; // modify bizO in cache that was deleted in DB.
			dummiesInOtherFactory[4].Delete(); // delete bizO from cache.

			var newDummyInOtherFactory = otherFactory.New<DummyBusinessObject>(); // Add new BizO into cache.
			newDummyInOtherFactory.Z0_Code = "AA6";

			// ensure that dummies cached in this factory are not data-refreshed from other factories
			dummiesInOtherFactory = otherFactory.Load<DummyBusinessObject>(query);
			AssertEquals("No changes from other factories should be passed into current Factory.", 5, dummiesInOtherFactory.Length);
			AssertEquals("Should load cached BizO.", dummy0.PK, dummiesInOtherFactory[0].PK);
			AssertEquals("Should load cached BizO.", dummy1.PK, dummiesInOtherFactory[1].PK);
			AssertEquals("Should load cached BizO, even if it is deleted in another factory.", dummy2.PK, dummiesInOtherFactory[2].PK);
			AssertEquals("Should load cached BizO, even if it is deleted in another factory.", dummy3.PK, dummiesInOtherFactory[3].PK);
			AssertEquals("Should load new BizO that was added into cache.", newDummyInOtherFactory.PK, dummiesInOtherFactory[4].PK);

			// call Synchronise, then ensure we have the most current data (local changes are preferred over DB changes).
			// note that new business objects should *not* be brought into the factory.
			otherFactory.SynchroniseCachedBusinessObjectsWithDB<DummyBusinessObject>(false, false);
			dummiesInOtherFactory = otherFactory.Load<DummyBusinessObject>(query);

			AssertEquals("Cached BizO's should match most current data (local changes are preferred over DB changes).", 4, dummiesInOtherFactory.Length);

			AssertEquals(dummy0.PK, dummiesInOtherFactory[0].PK);
			AssertEquals("This BizO was updated in DB after caching but not updated in a cache, so new data should be reloaded from DB.", 50, dummiesInOtherFactory[0].Z0_Number);

			AssertEquals(dummy1.PK, dummiesInOtherFactory[1].PK);
			AssertEquals("This BizO was updated in both DB and local cache, so local changes should be preferred and *not* reloaded from DB.", 200, dummiesInOtherFactory[1].Z0_Number);

			AssertEquals(dummy3.PK, dummiesInOtherFactory[2].PK);
			AssertEquals("This BizO was deleted from DB, but also has changes in cache, because of that it should *not* be deleted.", false, dummiesInOtherFactory[2].IsDeleted);
			AssertEquals("This BizO was deleted from DB, but also has changes in cache, because of that it should *not* be updated.", 200, dummiesInOtherFactory[2].Z0_Number);

			AssertEquals(newDummyInOtherFactory.PK, dummiesInOtherFactory[3].PK);
			AssertEquals("New BizO added in cache, should not be deleted during Synchronise.", false, dummiesInOtherFactory[3].IsDeleted);

			// Deleted bizO's
			Assert("This BizO was deleted from DB and has no changes in cache, should be deleted from cache during Synchronise.", oldDummyInOtherFactory2.IsDeleted);
			Assert("This BizO was deleted from DB and has no changes in cache, should *not* have changes in cache so it will not be attended to be removed from DB again.", !oldDummyInOtherFactory2.HasChanges);

			Assert("This BizO was deleted from cache, should stay deleted during Synchronise.", oldDummyInOtherFactory4.IsDeleted);
			Assert("This BizO was deleted from cache, so should have changes and so be deleted from DB during Save().", oldDummyInOtherFactory4.HasChanges);
		}

		#endregion

		#region TestSynchroniseCachedBusinessObjectsWithDB_IncludeBusinessObjectsWithChanges

		public void TestSynchroniseCachedBusinessObjectsWithDB_IncludeBusinessObjectsWithChanges()
		{
			var query = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "AA");
			query.OrderBy = DummyBizoSchema.Constants.Z0_Code;

			// create and save some Dummy objects

			var dummy0 = Factory.New<DummyBusinessObject>();
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy3 = Factory.New<DummyBusinessObject>();
			var dummy4 = Factory.New<DummyBusinessObject>();
			dummy0.Z0_Code = "AA0";
			dummy1.Z0_Code = "AA1";
			dummy2.Z0_Code = "AA2";
			dummy3.Z0_Code = "AA3";
			dummy4.Z0_Code = "AA4";

			dummy0.Z0_Number = 100; // modify BizO in DB before cache.
			dummy1.Z0_Number = 100; // modify BizO in DB before cache.

			Factory.Save();

			// Cache BizO's
			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummiesInOtherFactory = otherFactory.Load<DummyBusinessObject>(query);

			AssertEquals("Precondition - Dummies should be loaded.", 5, dummiesInOtherFactory.Length);
			AssertEquals("Precondition - Dummy should be loaded.", dummy0.PK, dummiesInOtherFactory[0].PK);
			AssertEquals("Precondition - Dummy should be loaded.", dummy1.PK, dummiesInOtherFactory[1].PK);
			AssertEquals("Precondition - Dummy should be loaded.", dummy2.PK, dummiesInOtherFactory[2].PK);
			AssertEquals("Precondition - Dummy should be loaded.", dummy3.PK, dummiesInOtherFactory[3].PK);
			AssertEquals("Precondition - Dummy should be loaded.", dummy4.PK, dummiesInOtherFactory[4].PK);

			dummy0.Z0_Number = 50; // modify in DB cached BizO (100 in cache, 50 in DB).
			dummy1.Z0_Number = 50; // modify in DB cached BizO (100 in cache, 50 in DB).
			dummy2.Delete(); // delete from DB cached BizO
			dummy3.Delete(); // delete from DB cached BizO

			Factory.Save();

			var oldDummy2InOtherFactory = dummiesInOtherFactory[2]; // save link to bizO deleted in DB.
			var oldDummy3InOtherFactory = dummiesInOtherFactory[3]; // save link to bizO deleted in DB.
			var oldDummy4InOtherFactory = dummiesInOtherFactory[4]; // save link to bizO deleted in cache.

			dummiesInOtherFactory[1].Z0_Number = 200; // modify bizO in cache that was modified in DB.
			dummiesInOtherFactory[3].Z0_Number = 200; // modify bizO in cache that was deleted in DB.
			dummiesInOtherFactory[4].Delete(); // delete bizO from cache.

			var newDummyInOtherFactory = otherFactory.New<DummyBusinessObject>(); // Add new BizO into cache.
			newDummyInOtherFactory.Z0_Code = "AA6";

			// ensure that dummies cached in this factory are not data-refreshed from other factories
			dummiesInOtherFactory = otherFactory.Load<DummyBusinessObject>(query);
			AssertEquals("No changes from other factories should be passed into current Factory.", 5, dummiesInOtherFactory.Length);
			AssertEquals("Should load cached BizO.", dummy0.PK, dummiesInOtherFactory[0].PK);
			AssertEquals("Should load cached BizO.", dummy1.PK, dummiesInOtherFactory[1].PK);
			AssertEquals("Should load cached BizO, even if it is deleted in another factory.", dummy2.PK, dummiesInOtherFactory[2].PK);
			AssertEquals("Should load cached BizO, even if it is deleted in another factory.", dummy3.PK, dummiesInOtherFactory[3].PK);
			AssertEquals("Should load new BizO that was added into cache.", newDummyInOtherFactory.PK, dummiesInOtherFactory[4].PK);

			// call Synchronise with the option to include business objects that have local changes, then ensure we have the most current data (DB changes should override local changes).
			// note that new business objects should *not* be brought into the factory.
			otherFactory.SynchroniseCachedBusinessObjectsWithDB<DummyBusinessObject>(true, false);
			dummiesInOtherFactory = otherFactory.Load<DummyBusinessObject>(query);

			AssertEquals("Cached BizO's should match most current data (DB changes should override any local changes).", 3, dummiesInOtherFactory.Length);

			AssertEquals(dummy0.PK, dummiesInOtherFactory[0].PK);
			AssertEquals("This BizO was updated in DB after caching but not updated in a cache, so new data should be reloaded from DB.", 50, dummiesInOtherFactory[0].Z0_Number);

			AssertEquals(dummy1.PK, dummiesInOtherFactory[1].PK);
			AssertEquals("This BizO was updated in both DB and local cache, so changes from DB should be loaded and override local changes", 50, dummiesInOtherFactory[1].Z0_Number);

			AssertEquals(newDummyInOtherFactory.PK, dummiesInOtherFactory[2].PK);
			AssertEquals("New BizO added in cache, should not be deleted during Synchronise.", false, dummiesInOtherFactory[2].IsDeleted);

			// Deleted bizO's
			Assert("This BizO was deleted from DB so should be deleted from cache during Synchronise.", oldDummy2InOtherFactory.IsDeleted);
			Assert("This BizO was deleted from DB so should *not* have changes in cache so there will be no attempted to remove from DB again.", !oldDummy2InOtherFactory.HasChanges);

			Assert("This BizO was deleted from DB so should be deleted from cache during Synchronise.", oldDummy3InOtherFactory.IsDeleted);
			Assert("This BizO was deleted from DB so should *not* have changes in cache so there will be no attempt to be remove from DB again.", !oldDummy3InOtherFactory.HasChanges);

			Assert("This BizO was deleted from cache, should stay deleted during Synchronise.", oldDummy4InOtherFactory.IsDeleted);
			Assert("This BizO was deleted from cache, so should have changes and so be deleted from DB during Save().", oldDummy4InOtherFactory.HasChanges);
		}

		#endregion

		#region TestSynchroniseCachedBusinessObjectsWithDB_DeleteAtDatabaseRowLevel

		public void TestSynchroniseCachedBusinessObjectsWithDB_DeleteAtDatabaseRowLevel()
		{
			var query = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "AA");
			query.OrderBy = DummyBizoSchema.Constants.Z0_Code;

			// create and save some Dummy objects

			var dummy0 = Factory.New<DummyWontDelete>();
			var dummy1 = Factory.New<DummyWontDelete>();
			var dummy2 = Factory.New<DummyWontDelete>();
			var dummy3 = Factory.New<DummyWontDelete>();
			var dummy4 = Factory.New<DummyWontDelete>();
			dummy0.Z0_Code = "AA0";
			dummy1.Z0_Code = "AA1";
			dummy2.Z0_Code = "AA2";
			dummy3.Z0_Code = "AA3";
			dummy4.Z0_Code = "AA4";

			dummy0.Z0_Number = 100; // modify BizO in DB before cache.
			dummy1.Z0_Number = 100; // modify BizO in DB before cache.

			Factory.Save();

			// Cache BizO's
			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummiesInOtherFactory = otherFactory.Load<DummyWontDelete>(query);

			AssertEquals("Precondition - Dummies should be loaded.", 5, dummiesInOtherFactory.Length);
			AssertEquals("Precondition - Dummy should be loaded.", dummy0.PK, dummiesInOtherFactory[0].PK);
			AssertEquals("Precondition - Dummy should be loaded.", dummy1.PK, dummiesInOtherFactory[1].PK);
			AssertEquals("Precondition - Dummy should be loaded.", dummy2.PK, dummiesInOtherFactory[2].PK);
			AssertEquals("Precondition - Dummy should be loaded.", dummy3.PK, dummiesInOtherFactory[3].PK);
			AssertEquals("Precondition - Dummy should be loaded.", dummy4.PK, dummiesInOtherFactory[4].PK);

			dummy0.Z0_Number = 50; // modify in DB cached BizO (100 in cache, 50 in DB).
			dummy1.Z0_Number = 50; // modify in DB cached BizO (100 in cache, 50 in DB).
			dummy2.ReallyDelete(); // delete from DB cached BizO
			dummy3.ReallyDelete(); // delete from DB cached BizO

			Factory.Save();

			var oldDummyInOtherFactory2 = dummiesInOtherFactory[2]; // save link to bizO deleted in DB.
			var oldDummyInOtherFactory4 = dummiesInOtherFactory[4]; // save link to bizO deleted in cache.

			dummiesInOtherFactory[1].Z0_Number = 200; // modify bizO in cache that was modified in DB.
			dummiesInOtherFactory[3].Z0_Number = 200; // modify bizO in cache that was deleted in DB.
			dummiesInOtherFactory[4].ReallyDelete(); // delete bizO from cache.

			var newDummyInOtherFactory = otherFactory.New<DummyWontDelete>(); // Add new BizO into cache.
			newDummyInOtherFactory.Z0_Code = "AA6";

			// ensure that dummies cached in this factory are not data-refreshed from other factories
			dummiesInOtherFactory = otherFactory.Load<DummyWontDelete>(query);
			AssertEquals("No changes from other factories should be passed into current Factory.", 5, dummiesInOtherFactory.Length);
			AssertEquals("Should load cached BizO.", dummy0.PK, dummiesInOtherFactory[0].PK);
			AssertEquals("Should load cached BizO.", dummy1.PK, dummiesInOtherFactory[1].PK);
			AssertEquals("Should load cached BizO, even if it is deleted in another factory.", dummy2.PK, dummiesInOtherFactory[2].PK);
			AssertEquals("Should load cached BizO, even if it is deleted in another factory.", dummy3.PK, dummiesInOtherFactory[3].PK);
			AssertEquals("Should load new BizO that was added into cache.", newDummyInOtherFactory.PK, dummiesInOtherFactory[4].PK);

			// call Synchronise, then ensure we have the most current data (local changes are preferred over DB changes).
			// note that new business objects should *not* be brought into the factory.
			otherFactory.SynchroniseCachedBusinessObjectsWithDB<DummyWontDelete>(false, true);
			dummiesInOtherFactory = otherFactory.Load<DummyWontDelete>(query);

			CombineAssertions("Cached BizO's should match most current data (local changes are preferred over DB changes).", () =>
			{
				AssertCollectionNotContains("This BizO was deleted from DB and has no changes in cache, should be deleted from cache during Synchronise.", dummy2.PK, dummiesInOtherFactory.Select(d => d.PK));
				AssertCollectionNotContains("This BizO was deleted from cache, should stay deleted during Synchronise.", dummy4.PK, dummiesInOtherFactory.Select(d => d.PK));
				AssertEquals("Wrong number of BizO's", 4, dummiesInOtherFactory.Length);
			});

			CombineAssertions(() =>
			{
				AssertEquals(dummy0.PK, dummiesInOtherFactory[0].PK);
				AssertEquals("This BizO was updated in DB after caching but not updated in a cache, so new data should be reloaded from DB.", 50, dummiesInOtherFactory[0].Z0_Number);

				AssertEquals(dummy1.PK, dummiesInOtherFactory[1].PK);
				AssertEquals("This BizO was updated in both DB and local cache, so local changes should be preferred and *not* reloaded from DB.", 200, dummiesInOtherFactory[1].Z0_Number);

				AssertEquals(dummy3.PK, dummiesInOtherFactory[2].PK);
				AssertEquals("This BizO was deleted from DB, but also has changes in cache, because of that it should *not* be deleted.", false, dummiesInOtherFactory[2].IsDeleted);
				AssertEquals("This BizO was deleted from DB, but also has changes in cache, because of that it should *not* be updated.", 200, dummiesInOtherFactory[2].Z0_Number);

				AssertEquals(newDummyInOtherFactory.PK, dummiesInOtherFactory[3].PK);
				AssertEquals("New BizO added in cache, should not be deleted during Synchronise.", false, dummiesInOtherFactory[3].IsDeleted);

				// Deleted bizO's
				Assert("This BizO was deleted from DB and has no changes in cache, should be deleted from cache during Synchronise.", oldDummyInOtherFactory2.IsDeleted);
				Assert("This BizO was deleted from DB and has no changes in cache, should *not* have changes in cache so it will not be attended to be removed from DB again.", !oldDummyInOtherFactory2.HasChanges);

				Assert("This BizO was deleted from cache, should stay deleted during Synchronise.", oldDummyInOtherFactory4.IsDeleted);
				Assert("This BizO was deleted from cache, so should have changes and so be deleted from DB during Save().", oldDummyInOtherFactory4.HasChanges);
			});
		}

		#endregion

		#region TestSynchroniseCachedBusinessObjectsWithDB_IncludeBusinessObjectsWithChanges_DeleteAtDatabaseRowLevel

		public void TestSynchroniseCachedBusinessObjectsWithDB_IncludeBusinessObjectsWithChanges_DeleteAtDatabaseRowLevel()
		{
			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			AssertExceptionThrown(typeof(ArgumentException), () =>
			{
				otherFactory.SynchroniseCachedBusinessObjectsWithDB<DummyWontDelete>(true, true);
			});
		}

		#endregion
	}

	class DummyWontDelete : DummyBusinessObject
	{
		public DummyWontDelete(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		public override void Delete()
		{
			// Don't do anything
		}

		public void ReallyDelete()
		{
			base.Delete();
		}
	}
}
