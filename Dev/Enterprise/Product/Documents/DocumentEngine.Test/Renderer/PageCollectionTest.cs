using NUnit.Framework;

namespace Enterprise.DocumentEngine.Renderer.Testing
{
	sealed class PageCollectionTest : TestCase
	{
		public void TestFindIndex()
		{
			var pages = new PageCollection();

			var page1 = new Page();
			var page2 = new Page();

			AssertEquals("pages.FindIndex", -1, pages.FindIndex(0, page => page == page1));
			AssertEquals("pages.FindIndex", -1, pages.FindIndex(0, page => page == page2));

			pages.Add(page1);
			AssertEquals("pages.FindIndex", 0, pages.FindIndex(0, page => page == page1));
			AssertEquals("pages.FindIndex", -1, pages.FindIndex(0, page => page == page2));

			pages.Add(page2);
			AssertEquals("pages.FindIndex", 0, pages.FindIndex(0, page => page == page1));
			AssertEquals("pages.FindIndex", 1, pages.FindIndex(0, page => page == page2));
			AssertEquals("pages.FindIndex", -1, pages.FindIndex(1, page => page == page1));

			pages.Remove(page1);
			AssertEquals("pages.FindIndex", -1, pages.FindIndex(0, page => page == page1));
			AssertEquals("pages.FindIndex", 0, pages.FindIndex(0, page => page == page2));

			pages.Clear();
			AssertEquals("pages.FindIndex", -1, pages.FindIndex(0, page => page == page1));
			AssertEquals("pages.FindIndex", -1, pages.FindIndex(0, page => page == page2));
		}

		public void TestAddNew()
		{
			var pages = new PageCollection();

			var page1 = pages.AddNew();
			AssertEquals("page1.Height", 0, page1.Height);

			pages.DefaultPageHeight = 100;
			var page2 = pages.AddNew();
			AssertEquals("page2.Height", 100, page2.Height);
		}

		public void TestLastPage()
		{
			var pages = new PageCollection();

			AssertNull("pages.LastPage", pages.LastPage);

			var page1 = pages.AddNew();
			AssertEquals("pages.LastPage", page1, pages.LastPage);

			var page2 = pages.AddNew();
			AssertEquals("pages.LastPage", page2, pages.LastPage);

			pages.Remove(page2);
			AssertEquals("pages.LastPage", page1, pages.LastPage);

			pages.Remove(page1);
			AssertNull("pages.LastPage", pages.LastPage);
		}
	}
}
