using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[HttpContextEnabledTest]
	public abstract class ZFilterGridModuleWithCachingTest : TestCaseWithFactory
	{
		public void TestCollectionReloadsWhenSearching()
		{
			using (var module = GetNewFilterGridModule())
			{
				module.SessionCollectionIndexer = "CollectionIndexer";
				var filterBO = module.CreateNewFilterBusinessObject();
				module.LoadCollection(filterBO);
				var collection = module.GridCollection;
				AssertGreaterThan(collection.Count, 1);
				var cachedCollection = TestPage.GetSessionTimeSensitiveData(module.SessionCollectionIndexer);
				AssertNotNull(cachedCollection);
				AssertEquals(collection, cachedCollection);

				var initialCount = collection.Count;
				collection.RemoveAt(0);
				AssertEquals(initialCount - 1, collection.Count);
				module.LoadCollection(filterBO);
				var reloadedCollection = module.GridCollection;
				cachedCollection = TestPage.GetSessionTimeSensitiveData(module.SessionCollectionIndexer);
				AssertNotNull(cachedCollection);
				AssertEquals(reloadedCollection, cachedCollection);
				AssertEquals(initialCount - 1, reloadedCollection.Count);

				TestPage.Session[SearchControl.SearchControlIsSearchingIndexer] = true;
				module.LoadCollection(filterBO);
				reloadedCollection = module.GridCollection;
				cachedCollection = TestPage.GetSessionTimeSensitiveData(module.SessionCollectionIndexer);
				AssertNotNull(cachedCollection);
				AssertEquals(reloadedCollection, cachedCollection);
				AssertEquals(initialCount, reloadedCollection.Count);
			}
		}

		public void TestCollectionGetsStoredInSessionAndShared()
		{
			using (var module1 = GetNewFilterGridModule())
			using (var module2 = GetNewFilterGridModule())
			using (var module3 = GetNewFilterGridModule())
			{
				module1.SessionCollectionIndexer = "CollectionIndexer";
				var filterBO1 = module1.CreateNewFilterBusinessObject();
				module1.LoadCollection(filterBO1);
				var collection1 = module1.GridCollection;
				AssertGreaterThan(collection1.Count, 1);
				var cachedCollection = module1.Page.GetSessionTimeSensitiveData(module1.SessionCollectionIndexer);
				AssertNotNull(cachedCollection);
				AssertEquals(collection1, cachedCollection);

				module2.SessionCollectionIndexer = "CollectionIndexer";
				var filterBO2 = module2.CreateNewFilterBusinessObject();
				module2.LoadCollection(filterBO2);
				var collection2 = module2.GridCollection;
				AssertNotNull(collection2);
				AssertEquals(collection1, collection2);

				module3.SessionCollectionIndexer = "DifferentIndexer";
				var filterBO3 = module3.CreateNewFilterBusinessObject();
				module3.LoadCollection(filterBO3);
				var collection3 = module3.GridCollection;
				AssertNotNull(collection3);
				AssertNotEquals(collection1, collection3);
			}
		}

		public void TestCollectionOrderByQuery()
		{
			using (var module = GetNewFilterGridModule())
			{
				var query = new ZQuery();
				module.SessionCollectionIndexer = "CollectionIndexer";
				var filterBO = module.CreateNewFilterBusinessObject();
				module.LoadCollection(filterBO, query);
				Assert("Query OrderBy should not be appended with the PK column", !query.OrderBy.Contains($", {module.GetBusinessObjectPKColumn(filterBO).Name}"));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new ZWebTestHelper(Factory);
			Factory.Save();
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			TestPage = new ZTestPage();
			TestPage.SetSiteUser(helper.TestSiteUser);

			SetupData();
		}

		protected abstract void SetupData();

		protected abstract ZFilterGridModule GetNewFilterGridModule();

		protected ZTestPage TestPage;
	}
}
