using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Favorites.Testing
{
	sealed class RecentItemManagerTest : TestCaseWithFactory
	{
		#region SetUp

		Guid GetTempBranch()
		{
			var company = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			var branch = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch[GlbBranchSchema.GB_GC] = company.PK;
			Factory.Save();

			return branch.PK.ToGuid();
		}

		#endregion

		public void TestUpdateFavoriteModules()
		{
			var itemKey = Guid.NewGuid();
			var favorite1 = new LinkWrapper("BlahModule", itemKey, "http://favorite", "favorite description");
			Assert("Favorite added", RecentItemManager.Instance.AddToFavoriteModules(favorite1));

			var favorite = RecentItemManager.Instance.FavoriteModules.First(f => f.STL_ItemPK == itemKey);
			var firstDate = favorite.STL_LastUsedDateTimeUtc;

			var favorite2 = new LinkWrapper("BlahModule", itemKey, "http://favorite", "favorite description");
			RecentItemManager.Instance.UpdateFavoriteModules(favorite2);
			var updatedFavorite = RecentItemManager.Instance.FavoriteModules.First(f => f.STL_ItemPK == itemKey);
			AssertNotEquals("item STL_LastUsedDateTimeUtc should change.", firstDate, updatedFavorite.STL_LastUsedDateTimeUtc);

			var favorite3 = new LinkWrapper("BlahModule", itemKey, "http://favorite", "favorite new description");
			RecentItemManager.Instance.UpdateFavoriteModules(favorite3);

			updatedFavorite = RecentItemManager.Instance.FavoriteModules.First(f => f.STL_ItemPK == itemKey);
			AssertEquals("item description is updated.", "favorite new description", updatedFavorite.STL_ItemDescription);
			AssertNotEquals("item STL_LastUsedDateTimeUtc should update.", firstDate, updatedFavorite.STL_LastUsedDateTimeUtc);

			var favorite4 = new LinkWrapper("BlahModule", itemKey, "http://favoriteUpdated", "favorite description");
			RecentItemManager.Instance.UpdateFavoriteModules(favorite4);

			updatedFavorite = RecentItemManager.Instance.FavoriteModules.First(f => f.STL_ItemPK == itemKey);
			AssertEquals("url is updated.", "http://favoriteUpdated", updatedFavorite.STL_ItemUrl);
			AssertNotEquals("item STL_LastUsedDateTimeUtc should update.", firstDate, updatedFavorite.STL_LastUsedDateTimeUtc);
		}

		public void TestMaximumNumberOfFavoriteItems()
		{
			AssertEquals("Maximum Favorite Items", RawDataRegistry.Instance.MaximumNumberOfFavorites.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty), RecentItemManager.Instance.MaximumNumberOfFavorites);
		}

		public void TestFavorites()
		{
			AssertEquals("Favorites list is empty", 0, RecentItemManager.Instance.FavoriteModules.Count);

			var favorite = new LinkWrapper("BlahModule", Guid.NewGuid(), "http://favorite", "favorite shipment");
			Assert("Favorite added", RecentItemManager.Instance.AddToFavoriteModules(favorite));
			AssertEquals("Favorite added", 1, RecentItemManager.Instance.FavoriteModules.Count);
			Assert("Is in favorites", RecentItemManager.Instance.IsInFavoriteModules(favorite));
			Assert("Duplicate not added", !RecentItemManager.Instance.AddToFavoriteModules(favorite));

			RecentItemManager.Instance.RemoveFromFavoriteModules(favorite);
			AssertEquals("Favorite removed", 0, RecentItemManager.Instance.FavoriteModules.Count);

			var favorite2 = new LinkWrapper("PenModule", Guid.NewGuid(), "http://pen", "I have a pen");
			var favorite3 = new LinkWrapper("PineappleModule", Guid.NewGuid(), "http://pineapple", "I have a pineapple");
			Assert("Favorite 1 added", RecentItemManager.Instance.AddToFavoriteModules(favorite));
			Assert("Favorite 2 added", RecentItemManager.Instance.AddToFavoriteModules(favorite2));
			Assert("Favorite 3 added", RecentItemManager.Instance.AddToFavoriteModules(favorite3));
			AssertEquals("Favorites added", 3, RecentItemManager.Instance.FavoriteModules.Count);

			RecentItemManager.Instance.RemoveAllFavoriteModules();
			AssertEquals("Favorite removed", 0, RecentItemManager.Instance.FavoriteModules.Count);
		}

		public void TestFavoritesOrderIsCorrect()
		{
			AssertEquals("Favorites list is empty", 0, RecentItemManager.Instance.FavoriteModules.Count);

			var favorite1 = new LinkWrapper("BlahModule", Guid.NewGuid(), "http://favorite", "favorite shipment");
			Assert("Favorite added", RecentItemManager.Instance.AddToFavoriteModules(favorite1));
			AssertEquals("Favorite added", 1, RecentItemManager.Instance.FavoriteModules.Count);
			Assert("Is in favorites", RecentItemManager.Instance.IsInFavoriteModules(favorite1));
			AssertEquals("Shortcut Index is 1", (byte)1, RecentItemManager.Instance.FavoriteModules[0].STL_ShortcutIndex);

			var favorite2 = new LinkWrapper("AnotherModule", Guid.NewGuid(), "http://favorite", "favorite consol");
			Assert("Favorite added", RecentItemManager.Instance.AddToFavoriteModules(favorite2));
			AssertEquals("Favorite added", 2, RecentItemManager.Instance.FavoriteModules.Count);
			Assert("Is in favorites", RecentItemManager.Instance.IsInFavoriteModules(favorite2));
			AssertEquals("Shortcut Index is 2", (byte)2, RecentItemManager.Instance.FavoriteModules[1].STL_ShortcutIndex);
		}

		public void TestMoveFavorites()
		{
			AssertEquals("Favorites list is empty", 0, RecentItemManager.Instance.FavoriteModules.Count);

			var favorite1 = new LinkWrapper("BlahModule", Guid.NewGuid(), "http://favorite", "favorite shipment");
			Assert("Favorite 1 added", RecentItemManager.Instance.AddToFavoriteModules(favorite1));

			var favorite2 = new LinkWrapper("AnotherModule", Guid.NewGuid(), "http://favorite", "favorite consol");
			Assert("Favorite 2 added", RecentItemManager.Instance.AddToFavoriteModules(favorite2));

			var favorite3 = new LinkWrapper("BlahModule");
			Assert("Favorite 3 added", RecentItemManager.Instance.AddToFavoriteModules(favorite3));

			AssertOrderOfFavorites(favorite1, favorite2, favorite3);

			Assert("Favorite 1 moved", RecentItemManager.Instance.MoveFavorite(favorite1, 3));
			AssertOrderOfFavorites(favorite2, favorite3, favorite1);

			Assert("Cannot move favorite to invalid position", !RecentItemManager.Instance.MoveFavorite(favorite1, 5));
			Assert("Cannot move favorite to its own position", !RecentItemManager.Instance.MoveFavorite(favorite1, 3));
			AssertOrderOfFavorites(favorite2, favorite3, favorite1);

			Assert("Favorite 3 moved", RecentItemManager.Instance.MoveFavorite(favorite3, 1));
			AssertOrderOfFavorites(favorite3, favorite2, favorite1);
		}

		void AssertOrderOfFavorites(params LinkWrapper[] orderedLinks)
		{
			for (var i = 0; i < orderedLinks.Length; i++)
			{
				Assert("Favorite is in expected position", LinkWrapper.AreShortcutsEqual(orderedLinks[i], RecentItemManager.Instance.FavoriteModules[i]));
			}
		}

		public void TestFavoritesForMultipleCompanies()
		{
			ZGuid[] company1Favorites;
			ZGuid[] company1FavoritesAfterDeletion;
			ZGuid[] company2Favorites;

			var branch1PK = GetTempBranch();
			var branch2PK = GetTempBranch();

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch1PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Favorites list is empty", 0, RecentItemManager.Instance.FavoriteModules.Count);
				var favorite = new LinkWrapper("BlahModule", Guid.NewGuid(), "http://favorite", "favorite shipment");
				Assert("Favorite added", RecentItemManager.Instance.AddToFavoriteModules(favorite));
				Assert("Is in favorites", RecentItemManager.Instance.IsInFavoriteModules(favorite));

				company1Favorites = RecentItemManager.Instance.FavoriteModules.Select(t => t.PK).ToArray();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch2PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Favorites list is empty", 0, RecentItemManager.Instance.FavoriteModules.Count);
				var favorite = new LinkWrapper("BlahModule2", Guid.NewGuid(), "http://favorite", "favorite shipment for another company");
				Assert("Favorite added", RecentItemManager.Instance.AddToFavoriteModules(favorite));
				Assert("Is in favorites", RecentItemManager.Instance.IsInFavoriteModules(favorite));

				company2Favorites = RecentItemManager.Instance.FavoriteModules.Select(t => t.PK).ToArray();

				RecentItemManager.Instance.RemoveFromFavoriteModules(favorite);
				AssertEquals("Favorite removed", 0, RecentItemManager.Instance.FavoriteModules.Count);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch1PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				company1FavoritesAfterDeletion = RecentItemManager.Instance.FavoriteModules.Select(t => t.PK).ToArray();
			}

			AssertNotEquals("Favorites are different", company1Favorites[0], company2Favorites[0]);
			AssertArrayEqualsByElements("Company 1 favorites not affected by company 2 removing favorites", company1Favorites, company1FavoritesAfterDeletion);
		}

		public void TestMaximumNumberOfRecentItems()
		{
			AssertEquals("Maximum number of recent items same as in the registry", RawDataRegistry.Instance.MaximumNumberOfRecentItems.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty), RecentItemManager.Instance.MaximumNumberOfRecentItems);
		}

		public void TestRecentModules()
		{
			AssertEquals("Recent module list is empty", 0, RecentItemManager.Instance.RecentModules.Count);

			var module1 = new LinkWrapper("BlahModule", Guid.Empty, string.Empty, string.Empty);
			var module2 = new LinkWrapper("PenModule", Guid.Empty, string.Empty, string.Empty);
			var module3 = new LinkWrapper("PineappleModule", Guid.Empty, string.Empty, string.Empty);

			RecentItemManager.Instance.AddOrUpdateRecentModules(module1);
			AssertEquals("Recent module added", 1, RecentItemManager.Instance.RecentModules.Count);
			Assert("Is in recent modules", RecentItemManager.Instance.IsInRecentModules(module1));
			RecentItemManager.Instance.AddOrUpdateRecentModules(module1);
			AssertEquals("Recent module not added", 1, RecentItemManager.Instance.RecentModules.Count);

			RecentItemManager.Instance.AddOrUpdateRecentModules(module2);
			AssertEquals("Recent module added", 2, RecentItemManager.Instance.RecentModules.Count);
			Assert("Is in recent modules", RecentItemManager.Instance.IsInRecentModules(module2));

			RecentItemManager.Instance.AddOrUpdateRecentModules(module3);
			AssertEquals("Recent module added", 3, RecentItemManager.Instance.RecentModules.Count);
			Assert("Is in recent modules", RecentItemManager.Instance.IsInRecentModules(module3));

			RecentItemManager.Instance.RemoveFromRecentModules(module1);
			AssertEquals("Recent module 1 is removed", 2, RecentItemManager.Instance.RecentModules.Count);

			RecentItemManager.Instance.RemoveAllRecentModules();
			AssertEquals("All Recent modules are removed", 0, RecentItemManager.Instance.RecentModules.Count);
		}

		public void TestRecentModulesForMultipleCompanies()
		{
			ZGuid[] company1RecentModules;
			ZGuid[] company1RecentModulesAfterDeletion;
			ZGuid[] company2RecentModules;

			var branch1PK = GetTempBranch();
			var branch2PK = GetTempBranch();

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch1PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Recent module list is empty", 0, RecentItemManager.Instance.RecentModules.Count);

				var recent = new LinkWrapper("BlahModule", Guid.Empty, string.Empty, string.Empty);
				RecentItemManager.Instance.AddOrUpdateRecentModules(recent);
				AssertEquals("Recent module added", 1, RecentItemManager.Instance.RecentModules.Count);
				Assert("Is in recent modules", RecentItemManager.Instance.IsInRecentModules(recent));

				company1RecentModules = RecentItemManager.Instance.RecentModules.Select(t => t.PK).ToArray();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch2PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Recent module list is empty", 0, RecentItemManager.Instance.RecentModules.Count);

				var recent = new LinkWrapper("BlahModule2", Guid.Empty, string.Empty, string.Empty);
				RecentItemManager.Instance.AddOrUpdateRecentModules(recent);
				AssertEquals("Recent module added", 1, RecentItemManager.Instance.RecentModules.Count);
				Assert("Is in recent modules", RecentItemManager.Instance.IsInRecentModules(recent));

				company2RecentModules = RecentItemManager.Instance.RecentModules.Select(t => t.PK).ToArray();

				RecentItemManager.Instance.RemoveFromRecentModules(recent);
				AssertEquals("Recent removed", 0, RecentItemManager.Instance.RecentModules.Count);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch1PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				company1RecentModulesAfterDeletion = RecentItemManager.Instance.RecentModules.Select(t => t.PK).ToArray();
			}

			AssertNotEquals("Recent Modules are different", company1RecentModules[0], company2RecentModules[0]);
			AssertArrayEqualsByElements("Company 1 recent modules not affected by company 2 removing recent modules", company1RecentModules, company1RecentModulesAfterDeletion);
		}

		public void TestRecentItemsForMultipleCompanies()
		{
			RecentItemsForMultipleCompaniesCore(string.Empty);
		}

		public void TestModuleSpecificRecentItemsForMultipleCompanies()
		{
			RecentItemsForMultipleCompaniesCore("Dummy");
		}

		void RecentItemsForMultipleCompaniesCore(string module)
		{
			ZGuid[] company1RecentItems;
			ZGuid[] company1RecentItemsAfterDeletion;
			ZGuid[] company2RecentItems;

			var branch1PK = GetTempBranch();
			var branch2PK = GetTempBranch();

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch1PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Recent module list is empty", 0, RecentItemManager.Instance.GetRecentItems(module).Count);

				var recent = new LinkWrapper(module, Guid.NewGuid(), "http://favorite", "favorite shipment");
				RecentItemManager.Instance.AddOrUpdateRecentItems(module, recent);
				AssertEquals("Recent item added", 1, RecentItemManager.Instance.GetRecentItems(module).Count);
				Assert("Is in recent modules", RecentItemManager.Instance.IsInRecentItems(module, recent));

				company1RecentItems = RecentItemManager.Instance.GetRecentItems(module).Select(t => t.PK).ToArray();
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch2PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Recent module list is empty", 0, RecentItemManager.Instance.GetRecentItems(module).Count);

				var recent = new LinkWrapper(module, Guid.NewGuid(), "http://favorite", "favorite shipment 2");
				RecentItemManager.Instance.AddOrUpdateRecentItems(module, recent);
				AssertEquals("Recent module added", 1, RecentItemManager.Instance.GetRecentItems(module).Count);
				Assert("Is in recent modules", RecentItemManager.Instance.IsInRecentItems(module, recent));

				company2RecentItems = RecentItemManager.Instance.GetRecentItems(module).Select(t => t.PK).ToArray();

				RecentItemManager.Instance.RemoveFromRecentItems(module, recent);
				AssertEquals("Recent removed", 0, RecentItemManager.Instance.GetRecentItems(module).Count);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch1PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				company1RecentItemsAfterDeletion = RecentItemManager.Instance.GetRecentItems(module).Select(t => t.PK).ToArray();
			}

			AssertNotEquals("Recent Items are different", company1RecentItems[0], company2RecentItems[0]);
			AssertArrayEqualsByElements("Company 1 recent items not affected by company 2 removing recent items", company1RecentItems, company1RecentItemsAfterDeletion);
		}

		public void TestMaximumNumberOfRecentModules()
		{
			AssertEquals("Maximum number of recent modules same as in the registry", RawDataRegistry.Instance.MaximumNumberOfRecentModules.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty), RecentItemManager.Instance.MaximumNumberOfRecentModules);
		}

		public void TestRecentItems()
		{
			RecentItemsCore(string.Empty);
		}

		public void TestModuleSpecificRecentItems()
		{
			RecentItemsCore("BlahModule");
		}

		void RecentItemsCore(string module)
		{
			var changed = false;
			var changedModule = string.Empty;
			LinkWrapper changedItem = null;
			var changedAdded = false;

			RecentItemManager.RecentItemsChanged += (s, e) =>
			{
				changed = true;
				changedModule = e.Module;
				changedItem = e.Item;
				changedAdded = e.Added;
			};

			AssertEquals("Recent items list is empty", 0, RecentItemManager.Instance.GetRecentItems(module).Count);

			var recent = new LinkWrapper("BlahModule", Guid.NewGuid(), "http://favorite", "favorite shipment");
			RecentItemManager.Instance.AddOrUpdateRecentItems(module, recent);
			AssertEquals("Recent item added", 1, RecentItemManager.Instance.GetRecentItems(module).Count);
			Assert("Is in recent items", RecentItemManager.Instance.IsInRecentItems(module, recent));
			RecentItemManager.Instance.AddOrUpdateRecentItems(module, recent);
			AssertEquals("Recent item not added", 1, RecentItemManager.Instance.GetRecentItems(module).Count);

			if (!string.IsNullOrEmpty(module))
			{
				Assert("Changed event fired", changed);
				AssertEquals("Changed module matches", module, changedModule);
				AssertEquals("Changed item matches", recent.UniqueKey, changedItem.UniqueKey);
				Assert("Item was added", changedAdded);

				changed = false;
				changedAdded = false;
				changedItem = null;
				changedModule = string.Empty;
			}

			RecentItemManager.Instance.RemoveFromRecentItems(module, recent);
			AssertEquals("Recent modules removed", 0, RecentItemManager.Instance.GetRecentItems(module).Count);

			if (!string.IsNullOrEmpty(module))
			{
				Assert("Changed event fired", changed);
				AssertEquals("Changed module matches", module, changedModule);
				AssertEquals("Changed item matches", recent.UniqueKey, changedItem.UniqueKey);
				Assert("Item was removed", !changedAdded);
			}

			var recent2 = new LinkWrapper("BlahModule", Guid.NewGuid(), "http://favorite", "favorite shipment");
			RecentItemManager.Instance.AddOrUpdateRecentItems(module, recent);
			RecentItemManager.Instance.AddOrUpdateRecentItems(module, recent2);
			AssertEquals("recent2 boots out recent due to having same description", 1, RecentItemManager.Instance.GetRecentItems(module).Count);
			Assert("Isn't in recent items", !RecentItemManager.Instance.IsInRecentItems(module, recent));
			Assert("Is in recent items", RecentItemManager.Instance.IsInRecentItems(module, recent2));

			RecentItemManager.Instance.AddOrUpdateRecentItems(module, recent);
			AssertEquals("recent boots out recent2 due to having same description", 1, RecentItemManager.Instance.GetRecentItems(module).Count);
			Assert("Is in recent items", RecentItemManager.Instance.IsInRecentItems(module, recent));
			Assert("Isn't in recent items", !RecentItemManager.Instance.IsInRecentItems(module, recent2));

			var recent3 = new LinkWrapper("BlahModule", Guid.NewGuid(), "http://pen", "I have a pen");
			var recent4 = new LinkWrapper("BlahModule", Guid.NewGuid(), "http://pineapple", "I have a pineapple");

			RecentItemManager.Instance.AddOrUpdateRecentItems(module, recent);
			RecentItemManager.Instance.AddOrUpdateRecentItems(module, recent3);
			RecentItemManager.Instance.AddOrUpdateRecentItems(module, recent4);
			AssertEquals("All items are added", 3, RecentItemManager.Instance.GetRecentItems(module).Count);

			changed = false;
			changedAdded = false;
			changedItem = null;
			changedModule = string.Empty;

			RecentItemManager.Instance.RemoveAllRecentItems(module);
			AssertEquals("All recent items are removed", 0, RecentItemManager.Instance.GetRecentItems(module).Count);

			if (!string.IsNullOrEmpty(module))
			{
				Assert("Changed event fired", changed);
				AssertEquals("Changed module matches", module, changedModule);
				AssertEquals("Changed item matches", null, changedItem);
				Assert("Items was removed", !changedAdded);
			}

			var recent5 = new LinkWrapper("BlahModule", recent.RecordKey, "http://favoriteUpdated", "favorite shipment");
			RecentItemManager.Instance.AddOrUpdateRecentItems(module, recent);
			RecentItemManager.Instance.AddOrUpdateRecentItems(module, recent5);

			var recentItems = RecentItemManager.Instance.GetRecentItems(module);

			AssertEquals("recent is updated by recent5", 1, recentItems.Count);
			AssertEquals("Is in recent items", true, RecentItemManager.Instance.IsInRecentItems(module, recent));
			AssertEquals("Is in recent items", true, RecentItemManager.Instance.IsInRecentItems(module, recent5));

			var addedItem = RecentItemManager.Instance.GetRecentItems(module)[0];

			AssertEquals("Record url should be updated", "http://favoriteUpdated", addedItem.STL_ItemUrl);

			RecentItemManager.Instance.RemoveAllRecentItems(module);
			AssertEquals("All recent items are removed", 0, RecentItemManager.Instance.GetRecentItems(module).Count);

			if (!string.IsNullOrEmpty(module))
			{
				Assert("Changed event fired", changed);
				AssertEquals("Changed module matches", module, changedModule);
				AssertEquals("Changed item matches", null, changedItem);
				Assert("Items was removed", !changedAdded);
			}
		}

		public void TestConcurrencyMerges()
		{
			var factory = RecentItemManager.Instance.FactoryExposedForTest;

			var link = factory.New<StmLink>();
			link.STL_LinkType = StmLinkConstants.RecentModules;
			link.STL_GC_LogonCompany = EnvProxy.Instance.CurrentCompany.PK;
			link.STL_GS_NKUser = EnvProxy.Instance.CurrentUser.Initials;
			link.STL_LastUsedDateTimeUtc = new ZDateTime(2001, 1, 1);
			link.STL_ItemDescription = "Initial Description";
			factory.Save();

			var recentWrapper = new LinkWrapper(link);
			RecentItemManager.Instance.AddOrUpdateRecentModules(recentWrapper);

			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var linkReloaded = anotherFactory.Load<StmLink>(link.PK);
			linkReloaded.Delete();
			anotherFactory.Save();

			link.STL_ItemDescription = "Modified Value";

			AssertEquals("Precondition - RecentItemManager still thinks link hasn't been deleted", 1, RecentItemManager.Instance.RecentModules.Count);
			AssertNoExceptionThrown("Any operation that tries to save", () => RecentItemManager.Instance.RemoveAllRecentModules());
			AssertEquals("Changes from DB were merged. Link is deleted.", 0, RecentItemManager.Instance.RecentModules.Count);
		}
	}
}
