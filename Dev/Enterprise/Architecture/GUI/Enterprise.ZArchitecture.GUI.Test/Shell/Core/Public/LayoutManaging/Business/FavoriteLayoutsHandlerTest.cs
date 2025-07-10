using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class FavoriteLayoutsHandlerTest : TestCaseWithFactory
	{
		Guid GetTempBranch()
		{
			var company = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
			var branch = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
			branch[GlbBranchSchema.GB_GC] = company.PK;
			Factory.Save();

			return branch.PK.ToGuid();
		}

		public void TestFavoriteFilters()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				var module = "BlahFilter";
				var handler = new FavoriteLayoutsHandler(Factory, module);
				var favoriteFilters = handler.FavoriteFilters as StmLinkCollection;
				AssertEquals("Favorites list is empty", 0, favoriteFilters.Count);

				var filter = new LinkWrapper(module, Guid.NewGuid(), "http://favoriteFilter", "favorite filter");
				handler.AddOrRemoveFavoriteFilter(filter);
				AssertEquals("Favorite added", 1, favoriteFilters.Count);
				Assert("Is in favorites", handler.IsInFavoriteFilters(filter));
				handler.AddOrRemoveFavoriteFilter(filter);
				AssertEquals("Duplicate got Removed", 0, favoriteFilters.Count);

				var filter2 = new LinkWrapper(module, Guid.NewGuid(), "http://pen", "I have a pen");
				var filter3 = new LinkWrapper(module, Guid.NewGuid(), "http://pineapple", "I have a pineapple");
				handler.AddOrRemoveFavoriteFilter(filter);
				handler.AddOrRemoveFavoriteFilter(filter2);
				handler.AddOrRemoveFavoriteFilter(filter3);
				handler.AddOrRemoveFavoriteFilter(filter2);
				AssertEquals("Filters added", 2, favoriteFilters.Count);

				handler.RemoveAllFavoriteFilters();
				AssertEquals("Filter removed", 0, favoriteFilters.Count);
			}
		}

		public void TestAddOrRemoveFavoriteFilter_ZSaveConcurrencyException()
		{
			var module = "BlahFilter";
			var handler = new FavoriteLayoutsHandler(Factory, module);
			var filter = new LinkWrapper(module, Guid.NewGuid(), "http://favoriteFilter", "favorite filter");
			handler.AddOrRemoveFavoriteFilter(filter);

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var handler2 = new FavoriteLayoutsHandler(factory2, module);
			_ = handler2.FavoriteFilters;

			handler.AddOrRemoveFavoriteFilter(filter);
			AssertNoExceptionThrown(() => handler2.AddOrRemoveFavoriteFilter(filter));
			AssertEquals("Filter removed", 0, (handler2.FavoriteFilters as StmLinkCollection).Count);
		}

		public void TestFiltersForMultipleUsers()
		{
			ZGuid[] company1FavoriteFilters;
			ZGuid[] company1FavoriteFiltersAfterDeletion;
			ZGuid[] company2FavoriteFilters;

			var branch1PK = GetTempBranch();
			var branch2PK = GetTempBranch();

			var module = "Blah";

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch1PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				var handler = new FavoriteLayoutsHandler(Factory, module);
				var favoriteFilters = handler.FavoriteFilters as StmLinkCollection;
				AssertEquals("Favorite filters list is empty", 0, favoriteFilters.Count);
				var filter = new LinkWrapper(module, Guid.NewGuid(), "http://filter", "filter shipment");
				handler.AddOrRemoveFavoriteFilter(filter);
				Assert("Is in favorites", handler.IsInFavoriteFilters(filter));

				company1FavoriteFilters = favoriteFilters.Select(t => t.PK).ToArray();
			}

			var staff = Factory.New<IGlbStaff>();
			staff.GS_LoginName = "TST";

			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, branch2PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				var handler = new FavoriteLayoutsHandler(Factory, module);
				var favoriteFilters = handler.FavoriteFilters as StmLinkCollection;
				AssertEquals("Favorite filters list is empty", 0, favoriteFilters.Count);
				var filter = new LinkWrapper(module, Guid.NewGuid(), "http://filter", "filter shipment for another company");
				handler.AddOrRemoveFavoriteFilter(filter);
				Assert("Is in filters", handler.IsInFavoriteFilters(filter));
				AssertEquals("Just 1 Fav filter for user 2", 1, favoriteFilters.Count);

				company2FavoriteFilters = favoriteFilters.Select(t => t.PK).ToArray();

				handler.AddOrRemoveFavoriteFilter(filter);
				AssertEquals("Filter removed", 0, favoriteFilters.Count);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch1PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				var handler = new FavoriteLayoutsHandler(Factory, module);
				company1FavoriteFiltersAfterDeletion = (handler.FavoriteFilters as StmLinkCollection).Select(t => t.PK).ToArray();
			}

			AssertNotEquals("Favorites are different", company1FavoriteFilters[0], company2FavoriteFilters[0]);
			AssertArrayEqualsByElements("User 1 favorites not affected by company 2 removing favorites", company1FavoriteFilters, company1FavoriteFiltersAfterDeletion);
		}

		public void TestAddOrRemoveThrowsInvalidOperationException()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, GetTempBranch(), EnvProxy.Instance.CurrentDepartment.PK))
			{
				var module = "Blah";
				var handler = new FavoriteLayoutsHandler(Factory, module);
				var favoriteFilters = handler.FavoriteFilters as StmLinkCollection;
				AssertEquals("Favorite filters list is empty", 0, favoriteFilters.Count);
				for (var i = 1; i < 11; i++)
				{
					var filter = new LinkWrapper(module, Guid.NewGuid(), "http://filter" + i, "filter shipment" + i);
					handler.AddOrRemoveFavoriteFilter(filter);
					Assert("Filter should be added by now", handler.IsInFavoriteFilters(filter));
					AssertEquals("Amount of filters should be equal to i", i, favoriteFilters.Count);
				}

				var expectedMessage = "Limit of 10 favorites reached." + System.Environment.NewLine + "You first need to remove a favorite if you want to add '" + new LinkWrapper("Invalid").RecordDescription + "'";
				AssertExceptionThrown<InvalidOperationException>("", expectedMessage, () => handler.AddOrRemoveFavoriteFilter(new LinkWrapper("Invalid")));
			}
		}
	}
}
