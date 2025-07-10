using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Favorites.Testing
{
	[TestedType(typeof(StmLinkCollection))]
	sealed class StmLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<StmLinkCollection>
	{
		protected override StmLinkCollection GetCollectionToTest()
		{
			return new StmLinkCollection(Factory, new ZQuery());
		}

		[TestDate(2016, 10, 17)]
		public void TestAddNewWithShortcut()
		{
			TestCaseHelper.ClearTable("StmLink");
			var collection = new StmLinkCollection(Factory, new ZQuery(), "FAV", 2, StmLinkSchema.Constants.STL_ShortcutIndex, ListSortDirection.Descending);

			var item1 = collection.AddNew(new LinkWrapper("MD1", new Guid("1678BD45-0311-4939-84A8-26606D693F11"), "Url1", "Description1"));
			AssertEquals(1, collection.Count);
			AssertEquals(item1, collection[0]);
			AssertCollectionItem(item1, "MD1", "Url1", "Description1", new Guid("1678BD45-0311-4939-84A8-26606D693F11"), "FAV", 1);

			var item2 = collection.AddNew(new LinkWrapper("MD2", new Guid("7981C104-BB27-4C0C-ABD3-ED1AE2B1CC3F"), "Url2", "Description2"));
			AssertEquals(2, collection.Count);
			AssertEquals(item2, collection[0]);
			AssertEquals(item1, collection[1]);
			AssertCollectionItem(item2, "MD2", "Url2", "Description2", new Guid("7981C104-BB27-4C0C-ABD3-ED1AE2B1CC3F"), "FAV", 2);

			var item3 = collection.AddNew(new LinkWrapper("MD3", new Guid("989CB689-5E04-4446-84F2-4A5F46680AA6"), "Url3", "Description3"));
			AssertEquals(2, collection.Count);
			AssertEquals(item3, collection[0]);
			AssertEquals(item2, collection[1]);
			AssertCollectionItem(item3, "MD3", "Url3", "Description3", new Guid("989CB689-5E04-4446-84F2-4A5F46680AA6"), "FAV", 2);
		}

		void AssertCollectionItem(StmLink item, string expectedModuleID, string expectedUrl, string expectedDescription, Guid expectedItemPK, string expectedLinkType, int expectedIndex)
		{
			AssertEquals(expectedModuleID, item.STL_ModuleID);
			AssertEquals(expectedUrl, item.STL_ItemUrl);
			AssertEquals(expectedDescription, item.STL_ItemDescription);
			AssertEquals(expectedItemPK, item.STL_ItemPK);
			AssertEquals(EnvProxy.Instance.CurrentCompany.PK, item.STL_GC_LogonCompany);
			AssertEquals(EnvProxy.Instance.CurrentUser.Initials, item.STL_GS_NKUser);
			AssertEquals(ZDateTime.UtcNow, item.STL_LastUsedDateTimeUtc);
			AssertEquals(expectedLinkType, item.STL_LinkType);
			AssertEquals(expectedIndex, item.STL_ShortcutIndex);
		}

		public void TestCleanUp()
		{
			TestCaseHelper.ClearTable("StmLink");
			AddFavorites();

			var collection = new StmLinkCollection(Factory, new ZQuery(), "FAV", 3, StmLinkSchema.Constants.STL_ShortcutIndex, ListSortDirection.Ascending);
			AssertEquals("4 items", 4, collection.Count);

			collection.CleanUp();
			AssertEquals("3 items", 3, collection.Count);
			AssertArrayEqualsByElements(new ZByte[] { 1, 2, 3 }, collection.OfType<StmLink>().Select(t => t.STL_ShortcutIndex).ToArray());
		}

		public void TestCleanUpDuplicates()
		{
			TestCaseHelper.ClearTable("StmLink");
			AddFavoritesWithDuplicates();

			var collection = new StmLinkCollection(Factory, new ZQuery(), "FAV", 4, StmLinkSchema.Constants.STL_ShortcutIndex, ListSortDirection.Ascending);
			AssertEquals("4 items", 4, collection.Count);

			collection.CleanUp();
			AssertEquals("3 items", 3, collection.Count);
			AssertArrayEqualsByElements(new ZByte[] { 1, 2, 3 }, collection.OfType<StmLink>().Select(t => t.STL_ShortcutIndex).ToArray());
		}

		public void TestSort()
		{
			TestCaseHelper.ClearTable("StmLink");
			AddFavorites();

			var collection = new StmLinkCollection(Factory, new ZQuery(), "FAV", 3, StmLinkSchema.Constants.STL_ShortcutIndex, ListSortDirection.Ascending);

			AssertEquals("4 items", 4, collection.Count);
			AssertArrayEqualsByElements(new ZByte[] { 1, 2, 3, 4 }, collection.OfType<StmLink>().Select(t => t.STL_ShortcutIndex).ToArray());

			var link = collection.AddNew();
			link.STL_ModuleID = "Dummy3";
			link.STL_LinkType = "FAV";
			link.STL_ItemPK = ZGuid.Empty;
			link.STL_ShortcutIndex = 0;
			AssertArrayEqualsByElements(new ZByte[] { 0, 1, 2, 3, 4 }, collection.OfType<StmLink>().Select(t => t.STL_ShortcutIndex).ToArray());
		}

		void AddFavorites()
		{
			GetLink("Dummy", 3, ZGuid.Empty);
			GetLink("Dummy2", 1, ZGuid.Empty);
			GetLink("Dummy", 2, ZGuid.NewZGuid());
			GetLink("Dummy", 4, ZGuid.NewZGuid());

			Factory.Save();
		}

		void AddFavoritesWithDuplicates()
		{
			GetLink("Dummy", 1, ZGuid.Empty);
			GetLink("Dummy2", 2, ZGuid.Empty);
			GetLink("Dummy", 3, ZGuid.NewZGuid());
			GetLink("Dummy", 1, ZGuid.Empty);

			Factory.Save();
		}

		StmLink GetLink(string moduleID, byte shortcutIndex, ZGuid itemPK)
		{
			var link = Factory.New<StmLink>();
			link.STL_ModuleID = moduleID;
			link.STL_ShortcutIndex = shortcutIndex;
			link.STL_GC_LogonCompany = EnvProxy.Instance.CurrentCompany.PK;
			link.STL_GS_NKUser = EnvProxy.Instance.CurrentUser.Initials;
			link.STL_LastUsedDateTimeUtc = ZDateTime.UtcNow;
			link.STL_LinkType = StmLinkConstants.Favorites;
			link.STL_ItemPK = itemPK;

			return link;
		}
	}
}
