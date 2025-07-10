using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.ZArchitecture.GUI.StripControl;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class FilterLayoutsHelperTest : TestCaseWithFactory
	{
		public void TestGetNewItemInsertIndex_PrivateFilters()
		{
			var list = new[] { "b/test1", "b/test2", "c ", "   d", "e [+]" }.Select(x => new ZFilterToolStripMenuItem(x)).ToList();

			var filter = Factory.NewWithValidTestData<StmModuleFilter>();
			filter.S9_FilterName = "f";

			AssertEquals(5, FilterLayoutsHelper.GetNewItemInsertIndex(list, filter, filter.S9_FilterName));

			var filter2 = Factory.NewWithValidTestData<StmModuleFilter>();
			filter2.S9_FilterName = "cf";

			AssertEquals(3, FilterLayoutsHelper.GetNewItemInsertIndex(list, filter2, filter2.S9_FilterName));

			var filter3 = Factory.NewWithValidTestData<StmModuleFilter>();
			filter3.S9_FilterName = "a";

			AssertEquals(0, FilterLayoutsHelper.GetNewItemInsertIndex(list, filter3, "a"));

			var filter4 = Factory.NewWithValidTestData<StmModuleFilter>();
			filter4.S9_FilterName = "a/name";

			AssertEquals(0, FilterLayoutsHelper.GetNewItemInsertIndex(list, filter4, "a"));
		}

		public void TestGetNewItemInsertIndex_MixedPublicityFilters()
		{
			var list = new[] { "a", "b", "c ", "   d", "e [+]" }.Select(x => new ZFilterToolStripMenuItem(x)).ToList();

			var filter = Factory.NewWithValidTestData<StmModuleFilter>();
			filter.S9_FilterName = "aaa";
			filter.S9_IsPublished = true;

			AssertEquals("Should insert public filters after private filters", 5, FilterLayoutsHelper.GetNewItemInsertIndex(list, filter, filter.S9_FilterName));

			list.Add(new ZFilterToolStripMenuItem("b", true));
			list.Add(new ZFilterToolStripMenuItem("c", true));

			AssertEquals("Should insert public filters at the right place", 5, FilterLayoutsHelper.GetNewItemInsertIndex(list, filter, filter.S9_FilterName));

			var filter2 = Factory.NewWithValidTestData<StmModuleFilter>();
			filter2.S9_FilterName = "ca";
			filter2.S9_IsPublished = false;

			AssertEquals("Should insert private filters at the right place", 3, FilterLayoutsHelper.GetNewItemInsertIndex(list, filter2, filter2.S9_FilterName));
		}

		public void TestCreateMenuItemFromFilter_WithPrivateAction()
		{
			var filter = Factory.NewWithValidTestData<StmModuleFilter>();
			filter.S9_FilterName = "test";

			var menuItem = FilterLayoutsHelper.CreateMenuItemFromFilter(filter, "blah", true, (sender, e) => { throw new Exception("This is my event !"); }, false);

			AssertEquals("Should have used the given display text", "blah", menuItem.Text);
			AssertEquals("Should have attached the given filter", filter, menuItem.Tag);
			AssertEquals("Should have attached the appropriate Tooltip text", $"This is your private filter layout '{filter.S9_FilterName}'", menuItem.ToolTipText);
			AssertExceptionThrown<Exception>("Should have attached the event", "This is my event !", () => menuItem.PerformClick());
		}

		public void TestCreateMenuItemFromFilter_WithPublishedAction()
		{
			var filter = Factory.NewWithValidTestData<StmModuleFilter>();
			filter.S9_FilterName = "test";
			filter.S9_IsPublished = true;

			var menuItem = FilterLayoutsHelper.CreateMenuItemFromFilter(filter, "blah", true, (sender, e) => { throw new Exception("This is my event !"); }, false);

			AssertEquals("Should have used the given display text", "blah", menuItem.Text);
			AssertEquals("Should have attached the given filter", filter, menuItem.Tag);
			AssertEquals("Should have attached the appropriate Tooltip text", $"The filter layout 'blah' is published system-wide.", menuItem.ToolTipText);
			AssertExceptionThrown<Exception>("Should have attached the event", "This is my event !", () => menuItem.PerformClick());
		}

		public void TestCreateMenuItemFromFilter_WithSystemAction()
		{
			var filter = Factory.NewWithValidTestData<StmModuleFilter>();
			filter.S9_FilterName = "test";
			filter.S9_IsSystem = true;

			var menuItem = FilterLayoutsHelper.CreateMenuItemFromFilter(filter, "blah", true, (sender, e) => { throw new Exception("This is my event !"); }, false);

			AssertEquals("Should have used the given display text", "blah", menuItem.Text);
			AssertEquals("Should have attached the given filter", filter, menuItem.Tag);
			AssertEquals("Should have attached the appropriate Tooltip text", $"The filter layout blah ships with {Constants.ProductName} and is published system-wide.", menuItem.ToolTipText);
			AssertExceptionThrown<Exception>("Should have attached the event", "This is my event !", () => menuItem.PerformClick());
		}

		public void TestCreateMenuItemFromFilter_WithNoAction()
		{
			var filter = Factory.NewWithValidTestData<StmModuleFilter>();
			filter.S9_FilterName = "test";

			var menuItem = FilterLayoutsHelper.CreateMenuItemFromFilter(filter, "blah", false, (sender, e) => { throw new Exception("This is my event !"); }, false);

			AssertEquals("Should have used the given display text", menuItem.Text, "blah");
			AssertNull("Should not have attached the given filter", menuItem.Tag);
			AssertNull("Should not have attached the appropriate Tooltip text", menuItem.ToolTipText);
			AssertNoExceptionThrown("Should not have attached the event", () => menuItem.PerformClick());
		}

		public void TestCalculateTree()
		{
			var filter1 = Factory.NewWithValidTestData<StmModuleFilter>();
			filter1.S9_FilterName = "a";

			var filter2 = Factory.NewWithValidTestData<StmModuleFilter>();
			filter2.S9_FilterName = "a/b";

			var filter3 = Factory.NewWithValidTestData<StmModuleFilter>();
			filter3.S9_FilterName = "a/c";

			var filter4 = Factory.NewWithValidTestData<StmModuleFilter>();
			filter4.S9_FilterName = "a/b/d";

			var layoutsRenamed = new Dictionary<ZGuid, string>() { { filter3.PK, "a/z" } };
			var list = new List<StmModuleFilter>() { filter1, filter2, filter3, filter4 };
			var tree = FilterLayoutsHelper.CalculateTree(list, false, null, layoutsRenamed);

			CombineAssertions(() =>
			{
				AssertEquals("Should be 1 top level menu item", 1, tree.Count);
				AssertEquals("Should have inserted the first menu text", "a", tree[0].Text);
				AssertEquals("Should have 2 drop down items", 2, tree[0].DropDownItems.Count);
				AssertEquals("The first drop down item should also have 1 drop down item", 1, ((ZFilterToolStripMenuItem)tree[0].DropDownItems[0]).DropDownItems.Count);
				AssertEquals("The first drop down item should also have 1 drop down item with correct text", "d", ((ZFilterToolStripMenuItem)tree[0].DropDownItems[0]).DropDownItems[0].Text);
				AssertEquals("Should have renamed the 2nd drop down item", "z", tree[0].DropDownItems[1].Text);
			});
		}

		public void TestCalculateTree_Multilingual()
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
			using (var mockChs = Res.UseMockData())
			{
				var filter1 = Factory.NewWithValidTestData<StmModuleFilter>();
				filter1.S9_FilterName = "a";

				var filter2 = Factory.NewWithValidTestData<StmModuleFilter>();
				filter2.S9_FilterName = "a/b";

				var filter3 = Factory.NewWithValidTestData<StmModuleFilter>();
				filter3.S9_FilterName = "a/c";

				var filter4 = Factory.NewWithValidTestData<StmModuleFilter>();
				filter4.S9_FilterName = "a/b/d";

				var key1 = ((ResourceString)filter1.S9_FilterNameMultilingual).ResourceKey;
				var key2 = ((ResourceString)filter2.S9_FilterNameMultilingual).ResourceKey;
				var key3 = ((ResourceString)filter3.S9_FilterNameMultilingual).ResourceKey;
				var key4 = ((ResourceString)filter4.S9_FilterNameMultilingual).ResourceKey;

				mockChs.Put(key1, new ResourceStringData(key1, "一"));
				mockChs.Put(key2, new ResourceStringData(key2, "一/二"));
				mockChs.Put(key3, new ResourceStringData(key3, "一/三"));
				mockChs.Put(key4, new ResourceStringData(key4, "一/二/四"));

				var layoutsRenamed = new Dictionary<ZGuid, string>() { { filter3.PK, "一/五" } };
				var list = new List<StmModuleFilter>() { filter1, filter2, filter3, filter4 };
				var tree = FilterLayoutsHelper.CalculateTree(list, false, null, layoutsRenamed);

				CombineAssertions(() =>
				{
					AssertEquals("Should be 1 top level menu item", 1, tree.Count);
					AssertEquals("Should have inserted the first menu text", "一", tree[0].Text);
					AssertEquals("Should have 2 drop down items", 2, tree[0].DropDownItems.Count);
					AssertEquals("The first drop down item should also have 1 drop down item", 1, ((ZFilterToolStripMenuItem)tree[0].DropDownItems[0]).DropDownItems.Count);
					AssertEquals("The first drop down item should also have 1 drop down item with correct text", "四", ((ZFilterToolStripMenuItem)tree[0].DropDownItems[0]).DropDownItems[0].Text);
					AssertEquals("Should have renamed the 2nd drop down item", "五", tree[0].DropDownItems[1].Text);
				});
			}
		}
	}
}
