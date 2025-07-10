using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class TabControlExtensionTest : TestCaseWithFactory
{
	public void TestOrderTabPages()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when TabControl parameter is null", () => (null as ZTabControl).OrderTabPages(new Dictionary<ZInt, ZTabPage>()));
		using (var tabControl = new ZTabControl())
		{
			AssertExceptionThrown<ArgumentNullException>("Expected exception when expectedTabPagesInOrder parameter is null", () => tabControl.OrderTabPages(null));
		}

		using (var tabControl = new ZTabControl())
		{
			var tabPage1 = new ZTabPage();
			var tabPage2 = new ZTabPage();
			var tabPage3 = new ZTabPage();
			var unknownTabPage1 = new ZTabPage();
			var unknownTabPage2 = new ZTabPage();
			var missedTabPage = new ZTabPage();

			tabControl.TabPages.Add(tabPage3);
			tabControl.TabPages.Add(unknownTabPage1);
			tabControl.TabPages.Add(tabPage2);
			tabControl.TabPages.Add(tabPage1);
			tabControl.TabPages.Add(unknownTabPage2);

			tabPage3.TabVisible = false;
			unknownTabPage2.TabVisible = false;

			var expectedTabPagesInOrder = new Dictionary<ZInt, ZTabPage>()
			{
				{ 0, tabPage1 },
				{ 1, tabPage2 },
				{ 2, tabPage3 },
				{ 3, missedTabPage },
			}.ToImmutableDictionary();

			tabControl.OrderTabPages(expectedTabPagesInOrder);
			AssertArrayEqualsByElements("Expected ordered tab pages", new[] { tabPage1, tabPage2, tabPage3, unknownTabPage1, unknownTabPage2 }, tabControl.AllTabPages.ToArray());

			CombineAssertions("Check TabVisibility", () =>
			{
				AssertEquals(nameof(tabPage1), true, tabPage1.TabVisible);
				AssertEquals(nameof(tabPage2), true, tabPage2.TabVisible);
				AssertEquals(nameof(tabPage3), false, tabPage3.TabVisible);
				AssertEquals(nameof(unknownTabPage1), true, unknownTabPage1.TabVisible);
				AssertEquals(nameof(unknownTabPage2), false, unknownTabPage2.TabVisible);
			});

			missedTabPage.Dispose();
		}
	}
}
