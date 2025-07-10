using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Favorites.Testing
{
	sealed class StmLinkExtensionsTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable("StmLink");
		}

		public void TestAddToCollection()
		{
			var collection = new StmLinkCollection(Factory, new ZQuery(), StmLinkConstants.Favorites, 2, StmLinkSchema.Constants.STL_ShortcutIndex, System.ComponentModel.ListSortDirection.Descending);
			AssertEquals("Collection is empty", 0, collection.Count);

			var wrapper1 = new LinkWrapper("Dummy1", Guid.Empty, "http://url1", "description1");
			collection.AddToCollection(wrapper1);
			AssertEquals("Item added to collection", 1, collection.Count);
			AssertEquals("description1", collection[0].STL_ItemDescription);

			var wrapper2 = new LinkWrapper("Dummy2", Guid.Empty, "http://url2", "description2");
			collection.AddToCollection(wrapper2);
			AssertEquals("Item added to collection", 2, collection.Count);
			AssertEquals("description2", collection[0].STL_ItemDescription);
			AssertEquals("description1", collection[1].STL_ItemDescription);

			var item1 = collection[0];
			var item2 = collection[1];
			var wrapper3 = new LinkWrapper("Dummy3", Guid.Empty, "http://url3", "description3");
			collection.AddToCollection(wrapper3);
			AssertEquals("Item added to collection", 2, collection.Count);
			AssertEquals("description3", collection[0].STL_ItemDescription);
			AssertEquals("description2", collection[1].STL_ItemDescription);
			AssertEquals(item2, collection[0]);
			AssertEquals(item1, collection[1]);
		}

		public void TestRemoveFromCollection()
		{
			var collection = new StmLinkCollection(Factory, new ZQuery(), StmLinkConstants.Favorites, 3, StmLinkSchema.Constants.STL_ShortcutIndex, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("Collection is empty", 0, collection.Count);

			var wrapper = new LinkWrapper("Dummy", Guid.Empty, "http://url", "description");
			collection.AddToCollection(wrapper);
			AssertEquals("Item added to collection", 1, collection.Count);

			collection.RemoveFromCollection(wrapper);
			AssertEquals("Item removed from collection", 0, collection.Count);
		}

		public void TestUpdateCollection()
		{
			var collection = new StmLinkCollection(Factory, new ZQuery(), StmLinkConstants.Favorites, 3, StmLinkSchema.Constants.STL_ShortcutIndex, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("Collection is empty", 0, collection.Count);

			var wrapper = new LinkWrapper("Dummy", Guid.Empty, "http://url", "description");
			collection.AddToCollection(wrapper);
			AssertEquals("Item added to collection", 1, collection.Count);
			AssertEquals("Old item description", "description", collection.OfType<StmLink>().First().STL_ItemDescription);

			collection.UpdateCollection(wrapper, (l, c) => l.STL_ItemDescription = "new description");
			AssertEquals("Count unchanged", 1, collection.Count);
			AssertEquals("New item description", "new description", collection.OfType<StmLink>().First().STL_ItemDescription);
		}

		public void TestInitializeLink()
		{
			var link = Factory.New<StmLink>();
			AssertEquals(ZString.Empty, link.STL_ModuleID);
			AssertEquals(ZString.Empty, link.STL_ItemDescription);
			AssertEquals(ZString.Empty, link.STL_ItemUrl);
			AssertEquals(ZGuid.Empty, link.STL_ItemPK);

			var pk = Guid.NewGuid();
			var wrapper = new LinkWrapper("Dummy", pk, "http://url", "description");
			link.InitializeLink(wrapper);
			AssertEquals("Dummy", link.STL_ModuleID);
			AssertEquals("description", link.STL_ItemDescription);
			AssertEquals("http://url", link.STL_ItemUrl);
			AssertEquals(pk, link.STL_ItemPK.ToGuid());
		}
	}
}
