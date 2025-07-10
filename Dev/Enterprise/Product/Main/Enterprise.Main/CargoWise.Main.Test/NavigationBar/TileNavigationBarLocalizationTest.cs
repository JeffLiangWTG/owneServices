using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Navigation;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
#if !WINZOR
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
#endif
using Application = System.Windows.Forms.Application;
using MainFormForTest = Enterprise.Startup.Testing.MainFormTestCase.TestMainForm;
using MenuItem = System.Windows.Forms.MenuItem;
using Res = CargoWise.Main.Res;
using ResString = CargoWise.Main.ResString;

namespace Enterprise.Startup.Testing
{
	sealed class TileNavigationBarLocalizationTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestAddOrUpdateModule()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();
				var tree = TreeWithFavoritesAndRecents();
				var jump = tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name];
				var favoriteItems = jump.Sections[ModuleTreeLoaderConstant.Section.Favorites.Name];
				var bar = form.Bar;
				bar.LoadModuleTree(tree);
				AssertEquals(3, favoriteItems.Modules.Count);
				AssertEquals("description", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0]).Items[0].MultilingualText);

				var updateItem = new LinkWrapper(dummyItemShortcut.ModuleName, dummyItemShortcut.RecordKey, dummyItemShortcut.RecordUrl, "new description");
				bar.AddOrUpdateModule(jump, favoriteItems, new LinkMainFormModule(updateItem), true);
				Application.DoEvents();
				AssertEquals("new description", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0]).Items[0].MultilingualText);
			}
		}

		[RequiresSTA]
		public void TestAddOrUpdateModuleWorksWithOtherLanuage()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				using var form = new TestForm();
				form.Show();
				Application.DoEvents();
				var tree = TreeWithFavoritesAndRecents();
				var jump = tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name];
				var favoriteItems = jump.Sections[ModuleTreeLoaderConstant.Section.Favorites.Name];
				var bar = form.Bar;
				bar.LoadModuleTree(tree);

				var mainViewModel = bar.navigationViewModel.MainViewModel;
				AssertNotNull("MainViewModel should exists based on Jump", mainViewModel);
				AssertEquals("The Key for MainViewModel is Jump", ModuleTreeLoaderConstant.Category.Jump.Name, mainViewModel.Name);
				AssertEquals("MainViewModel is translated", "快捷", mainViewModel.MultilingualText);
			}
		}

		[RequiresSTA]
		public void TestAddOrUpdateModule_NoExceptionThrownOut_WhenSectionOrModuleIsNull()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();
				var tree = TreeWithFavoritesAndRecents();
				var jump = tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name];
				var favoriteItems = jump.Sections[ModuleTreeLoaderConstant.Section.Favorites.Name];
				var bar = form.Bar;
				var updateItem = new LinkWrapper(dummyItemShortcut.ModuleName, dummyItemShortcut.RecordKey, dummyItemShortcut.RecordUrl, "new description");

				bar.LoadModuleTree(tree);

				AssertNoExceptionThrown("No NRE exception thrown out", () => bar.AddOrUpdateModule(jump, null, new LinkMainFormModule(updateItem), true));
				AssertNoExceptionThrown("No NRE exception thrown out", () => bar.AddOrUpdateModule(jump, favoriteItems, null, true));
			}
		}

		[RequiresSTA]
		public void TestRemoveModulesWithSameDescription_NoExceptionThrownOut_WhenSectionOrModuleIsNull()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();
				var tree = TreeWithFavoritesAndRecents();
				var jump = tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name];
				var favoriteItems = jump.Sections[ModuleTreeLoaderConstant.Section.Favorites.Name];
				var bar = form.Bar;
				var updateItem = new LinkWrapper(dummyItemShortcut.ModuleName, dummyItemShortcut.RecordKey, dummyItemShortcut.RecordUrl, "new description");

				bar.LoadModuleTree(tree);

				AssertNoExceptionThrown("No NRE exception thrown out", () => bar.RemoveModulesWithSameDescription(jump, null, new LinkMainFormModule(updateItem)));
				AssertNoExceptionThrown("No NRE exception thrown out", () => bar.RemoveModulesWithSameDescription(jump, favoriteItems, null));
			}
		}

		[RequiresSTA]
		public void TestRemoveModule_NoExceptionThrownOut_WhenSectionOrModuleIsNull()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();
				var tree = TreeWithFavoritesAndRecents();
				var jump = tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name];
				var favoriteItems = jump.Sections[ModuleTreeLoaderConstant.Section.Favorites.Name];
				var bar = form.Bar;
				var updateItem = new LinkWrapper(dummyItemShortcut.ModuleName, dummyItemShortcut.RecordKey, dummyItemShortcut.RecordUrl, "new description");

				bar.LoadModuleTree(tree);

				AssertNoExceptionThrown("No NRE exception thrown out", () => bar.RemoveModule(jump, null, new LinkMainFormModule(updateItem)));
				AssertNoExceptionThrown("No NRE exception thrown out", () => bar.RemoveModule(jump, favoriteItems, null));
			}
		}

		[RequiresSTA]
		public void TestAddModule_NoExceptionThrownOut_WhenSectionOrModuleIsNull()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();
				var tree = TreeWithFavoritesAndRecents();
				var jump = tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name];
				var favoriteItems = jump.Sections[ModuleTreeLoaderConstant.Section.Favorites.Name];
				var bar = form.Bar;
				var updateItem = new LinkWrapper(dummyItemShortcut.ModuleName, dummyItemShortcut.RecordKey, dummyItemShortcut.RecordUrl, "new description");

				bar.LoadModuleTree(tree);

				AssertNoExceptionThrown("No NRE exception thrown out", () => bar.AddModule(jump, null, new LinkMainFormModule(updateItem)));
				AssertNoExceptionThrown("No NRE exception thrown out", () => bar.AddModule(jump, favoriteItems, null));
			}
		}

		[RequiresSTA]
		public void TestLayoutItemsAreSorted()
		{
			using (var disposables = new DisposableList(10))
			using (var bar = new TileNavigationBar())
			{
				var formWithFilterStripLayouts = new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz")));
				var layouts = new StmModuleFilterCollection(Factory, LayoutsTestDataHelper.TestModuleID, new FilterStripLayoutsHelper());
				layouts.AddNew().S9_FilterName = "Z";
				layouts.AddNew().S9_FilterName = "B";
				layouts.AddNew().S9_FilterName = "A";
				layouts.AddNew().S9_FilterName = "M";
				layouts.AddNew().S9_FilterName = "F";

				Factory.Save();

				var items = bar.GetNewOpenModuleWithFilterLayoutMenuItems(formWithFilterStripLayouts, layouts, false);
				disposables.AddRange(items);

				AssertEquals("PRE: Needs to be enough items to confirm it is sorted", 5, items.Count);
				var itemsTexts = string.Join(System.Environment.NewLine, items.Select(item => item.Text));
				var expectedTexts = string.Join(System.Environment.NewLine, "ABFMZ".Select(c => c));

				AssertEquals("Items should be in order", expectedTexts, itemsTexts);
			}
		}

		[GuiTest]
		[RequiresSTA]
		public void TestCanOpenModulesInNewWindow()
		{
			TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);

			using (var bar = new TileNavigationBar())
			{
				bar.LoadModuleTree(ModuleTree.Tree);

				foreach (var info in bar.ModuleLookup.Values)
				{
					if (!TileNavigationBar.ModuleAlwaysOpensInNewWindow(info.module))
					{
						Assert(FormattableString.Invariant($"Could not open {info.category.DisplayText} > {info.section.DisplayText} > {info.module.Description}"), CanOpenInNewWindow(bar, info.module));
					}
				}
			}
		}

		bool CanOpenInNewWindow(TileNavigationBar bar, MainFormModule module)
		{
			try
			{
				using (bar.OpenModuleInNewWindowCore(module, null))
				{
					Application.DoEvents();
				}

				return true;
			}
			catch
			{
				return false;
			}
		}

		[RequiresSTA]
		public void TestLayoutItemsAreWellOrganized()
		{
			using (var disposables = new DisposableList(10))
			using (var bar = new TileNavigationBar())
			{
				var formWithFilterStripLayouts = new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz")));
				var layouts = new StmModuleFilterCollection(Factory, LayoutsTestDataHelper.TestModuleID, new FilterStripLayoutsHelper());
				layouts.AddNew().S9_FilterName = "A/ B/C";
				layouts.AddNew().S9_FilterName = "A";
				layouts.AddNew().S9_FilterName = "A/D";
				layouts.AddNew().S9_FilterName = "E";
				layouts.Last().S9_IsPublished = true;
				layouts.AddNew().S9_FilterName = "F /G";

				Factory.Save();

				var items = bar.GetNewOpenModuleWithFilterLayoutMenuItems(formWithFilterStripLayouts, layouts, false).ToArray();
				disposables.AddRange(items);

				AssertEquals("There should be 3 items at first level", 3, items.Length);

				var levelA = items[0];
				var levelF = items[1];
				var levelE = items[2];
				AssertEquals("First item text should be 'A'", "A", levelA.Text);
				AssertEquals("Second item text should be 'F'", "F", levelF.Text);
				AssertEquals("Third item text should be 'E'", "E", levelE.Text);

				disposables.AddRange(levelA.MenuItems.Cast<MenuItem>());
				AssertEquals("First item should have 2 sub-menu items", 2, levelA.MenuItems.Count);
				AssertEquals("Third item should have 1 sub-menu item1", 1, levelF.MenuItems.Count);

				var levelB = (ModuleMenuItem)levelA.MenuItems[0];
				disposables.AddRange(levelB.MenuItems.Cast<MenuItem>());
				AssertEquals("First sub-item should have 1 sub-menu item itself", 1, levelB.MenuItems.Count);
				AssertEquals("First sub-item text should be 'B'", "B", levelB.Text);
				AssertEquals("The sub-sub-item text should be 'C'", "C", levelB.MenuItems[0].Text);

				var levelD = (ModuleMenuItem)levelA.MenuItems[1];
				AssertEquals("Second sub-item should have 0 sub-menu item", 0, levelD.MenuItems.Count);
				AssertEquals("Second sub-item text should be 'D'", "D", levelD.Text);
			}
		}

		[RequiresSTA]
		public void TestFavoritesLayoutItemsAreShowing()
		{
			using (var bar = new TileNavigationBar())
			{
				var module = DummyModuleIDs.Dummy.Name;
				var formWithFilterStripLayouts = new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz")));
				var layouts = new StmModuleFilterCollection(Factory, LayoutsTestDataHelper.TestModuleID, new FilterStripLayoutsHelper());
				layouts.AddNew().S9_FilterName = "A";
				layouts.AddNew().S9_FilterName = "B";
				layouts.AddNew().S9_FilterName = "C";
				layouts.AddNew().S9_FilterName = "D";
				layouts.Last().S9_IsPublished = true;
				layouts.AddNew().S9_FilterName = "E";

				Factory.Save();

				var itemsPrior = bar.GetNewOpenModuleWithFilterLayoutMenuItems(formWithFilterStripLayouts, layouts);
				CombineAssertions(() =>
				{
					AssertEquals("There should be 9 items at first level", 9, itemsPrior.Length);
					AssertEquals("First Category should be 'My Filter Layouts'", "My Filter Layouts", itemsPrior[1].Text);
					AssertEquals("First element should be 'A'", "A", itemsPrior[2].Text);
					AssertEquals("Second element should be 'B'", "B", itemsPrior[3].Text);
					AssertEquals("Second Category should be 'Shared Filter Layouts'", "Shared Filter Layouts", itemsPrior[7].Text);
				});

				var favoriteLayoutsHander = new FavoriteLayoutsHandler(Factory, module);
				favoriteLayoutsHander.AddOrRemoveFavoriteFilter(new LinkWrapper(module, layouts.ToArray()[0].PK.ToGuid(), null, "A"));
				favoriteLayoutsHander.AddOrRemoveFavoriteFilter(new LinkWrapper(module, layouts.ToArray()[4].PK.ToGuid(), null, "C"));
				favoriteLayoutsHander.AddOrRemoveFavoriteFilter(new LinkWrapper(module, layouts.ToArray()[2].PK.ToGuid(), null, "E"));

				var items = bar.GetNewOpenModuleWithFilterLayoutMenuItems(formWithFilterStripLayouts, layouts);
				CombineAssertions(() =>
				{
					AssertEquals("There should be 11 items at first level", 11, items.Length);
					AssertEquals("First Category should be 'Favorites'", "Favorites", items[1].Text);
					AssertEquals("First element should be 'A'", "A", items[2].Text);
					AssertEquals("Second element should be 'C'", "C", items[3].Text);
					AssertEquals("Third element should be 'D'", "E", items[4].Text);
					AssertEquals("Second Category should be 'My Filter Layouts'", "My Filter Layouts", items[6].Text);
					AssertEquals("Third Category should be 'Shared Filter Layouts'", "Shared Filter Layouts", items[9].Text);
				});
			}
		}

		[RequiresSTA]
		public void TestUserDefinedLayoutsAreNotShowing()
		{
			using (var bar = new TileNavigationBar())
			{
				var formWithFilterStripLayouts = new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz")));
				var layouts = new StmModuleFilterCollection(Factory, LayoutsTestDataHelper.TestModuleID, new FilterStripLayoutsHelper());

				var filter1 = layouts.AddNew();
				filter1.S9_IsPublished = false;
				filter1.S9_FilterName = "filter1";

				var filter2 = layouts.AddNew();
				filter2.S9_IsPublished = true;
				filter2.S9_FilterName = "filter2";

				var filter3 = layouts.AddNew();
				filter3.S9_IsPublished = false;
				filter3.S9_FilterName = "userdefined1";
				filter3.S9_FilterType = StmModuleFilterTypes.Codes.UserDefined;

				var filter4 = layouts.AddNew();
				filter4.S9_IsPublished = true;
				filter4.S9_FilterName = "userdefined2";
				filter4.S9_FilterType = StmModuleFilterTypes.Codes.UserDefined;

				var filter5 = layouts.AddNew();
				filter5.S9_IsPublished = true;
				filter5.S9_FilterName = "filter5";

				Factory.Save();

				var items = bar.GetNewOpenModuleWithFilterLayoutMenuItems(formWithFilterStripLayouts, layouts).Where(x => !string.IsNullOrEmpty(x.Text.Trim()));

				AssertEquals("There should be 5 items (2 titles and 1 private and 2 public)", 5, items.Count());

				var itemsTexts = string.Join(System.Environment.NewLine, items.Select(item => item.Text));
				AssertContains("Private filter should not be showing", filter1.S9_FilterName, itemsTexts);
				AssertContains("Public filter defined items should not be showing", filter2.S9_FilterName, itemsTexts);
				AssertContains("Public filter defined items should not be showing", filter5.S9_FilterName, itemsTexts);

				AssertNotContains("Private User defined items should not be showing", filter3.S9_FilterName, itemsTexts);
				AssertNotContains("Public User defined items should not be showing", filter4.S9_FilterName, itemsTexts);
			}
		}

#if !WINZOR

		[RequiresSTA]
		public void TestAddOrUpdateModuleCrossthread()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();
				var tree = TreeWithFavoritesAndRecents();
				var jump = tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name];
				var recentItems = jump.Sections[ModuleTreeLoaderConstant.Section.RecentItems.Name];

				var bar = form.Bar;
				bar.LoadModuleTree(tree);

				var duplicate = new LinkWrapper(dummyItemShortcut.ModuleName, dummyItemShortcut.RecordKey, dummyItemShortcut.RecordUrl, "nolsy");

				//simulate CollectionView attached to ObservableCollection as in issue 02135645
				var menuSection = (MenuSection)bar.navigationViewModel.Categories[0].Buttons[1];
				var collection = menuSection.Items;
				var collectionView = new System.Windows.Data.CollectionView(collection);

				var thread = new Thread(() => { bar.AddOrUpdateModule(jump, recentItems, new LinkMainFormModule(duplicate)); });
				thread.Start();
				thread.Join();
				Application.DoEvents();

				var numberOfRecentItems = ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[1]).Items.Count;
				AssertEquals("The first item should be replaced instead of duplicated", 1, numberOfRecentItems);
			}
		}

		[RequiresSTA]
		public void TestFavorite_HyperLinkAction()
		{
			var moduleTree = new ModuleTree();
			dummyModuleWithExtendedDescription = new LinkMainFormModule(dummyModuleWithExtendedDescriptionShortcut);
			var jumpCategory = new ModuleCategory(ModuleTreeLoaderConstant.Category.Jump, null);
			var favoriteSection = new ModuleSection(ModuleTreeLoaderConstant.Section.Favorites, null, null, IconTypes.Blank, IconTypes.Blank);
			favoriteSection.Modules.Add(new LinkMainFormModule(dummyItemShortcut));
			favoriteSection.Modules.Add(new LinkMainFormModule(dummyModuleShortcut));
			favoriteSection.Modules.Add(dummyModuleWithExtendedDescription);
			jumpCategory.Sections.Add(favoriteSection);
			moduleTree.Categories.Add(jumpCategory);

			using (var form = new TestForm())
			{
				form.Size = ControlDpiScalingHelper.NewScaledSize(2000, 1500);
				form.Show();
				Application.DoEvents();

				var bar = form.Bar;
				bar.LoadModuleTree(moduleTree);
				bar.SelectedCategory = jumpCategory;
				Application.DoEvents();
				var tilebarControl = bar.TileControl;

				var categoryDetail = tilebarControl.FindLogicalChild<ScrollViewer>();
				var textBlocks = categoryDetail.FindVisualChildren<TextBlock>();
				var hyperlinks = textBlocks
							.Select(tb => tb.Inlines.OfType<System.Windows.Documents.Hyperlink>().FirstOrDefault())
							.Where(h => h != null)
							.ToList();
				var link = hyperlinks.LastOrDefault();

				var peer = new HyperlinkAutomationPeer(link);
				var invokeProvider = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
				invokeProvider?.Invoke();

				AssertNotNull(bar.lastTileClickedModule);
			}
		}
#endif

		[RequiresSTA]
		public void TestDuplicatingRecentItems()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();
				var tree = TreeWithFavoritesAndRecents();
				var jump = tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name];
				var recentItems = jump.Sections[ModuleTreeLoaderConstant.Section.RecentItems.Name];

				var bar = form.Bar;
				bar.LoadModuleTree(tree);

				var duplicate = new LinkWrapper(dummyItemShortcut.ModuleName, dummyItemShortcut.RecordKey, dummyItemShortcut.RecordUrl, "nolsy");

				bar.AddOrUpdateModule(jump, recentItems, new LinkMainFormModule(duplicate));

				var numberOfRecentItems = ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[1]).Items.Count;
				AssertEquals("The first item should be replaced instead of duplicated", 1, numberOfRecentItems);
			}
		}

		[RequiresSTA]
		public void TestSorting()
		{
			var tree = new ModuleTree();

			var categoryZ = new ModuleCategory("Zayden", ResString.GetMultilingualString("Zayden", "Zayden"), null);
			var sectionL = new ModuleSection("Lola", ResString.GetMultilingualString("Lola", "Lola"), null, null, IconTypes.ActionsButtonActive, IconTypes.ActionsButtonActive, ModuleTreeLoaderConstant.Subcategory.CashBook);
			sectionL.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz"))));
			sectionL.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy2, "rrr", "rrr", "AU", ResString.GetMultilingualString("rrr", "rrr"))));
			categoryZ.Sections.Add(sectionL);
			var sectionB = new ModuleSection("Bub", ResString.GetMultilingualString("Bub", "Bub"), null, null, IconTypes.ActionsButtonActive, IconTypes.ActionsButtonActive, ModuleTreeLoaderConstant.Subcategory.ArchiveManager);
			sectionB.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz"))));
			sectionB.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy2, "rrr", "rrr", "AU", ResString.GetMultilingualString("rrr", "rrr"))));
			categoryZ.Sections.Add(sectionB);
			var sectionA = new ModuleSection("Apple", ResString.GetMultilingualString("Apple", "Apple"), null, null, IconTypes.ActionsButtonActive, IconTypes.ActionsButtonActive, ModuleTreeLoaderConstant.Subcategory.ArchiveManager);
			sectionA.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz"))));
			sectionA.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy2, "rrr", "rrr", "AU", ResString.GetMultilingualString("rrr", "rrr"))));
			categoryZ.Sections.Add(sectionA);
			tree.Categories.Add(categoryZ);

			var categoryR = new ModuleCategory("Rylan", ResString.GetMultilingualString("Rylan", "Rylan"), null);
			var sectionS = new ModuleSection("S", ResString.GetMultilingualString("S", "S"), null, null, IconTypes.ActionsButtonActive, IconTypes.ActionsButtonActive, ModuleTreeLoaderConstant.Subcategory.Warehouse);
			sectionS.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz"))));
			sectionS.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy2, "rrr", "rrr", "AU", ResString.GetMultilingualString("rrr", "rrr"))));
			categoryR.Sections.Add(sectionS);
			var sectionP = new ModuleSection("P", ResString.GetMultilingualString("P", "P"), null, null, IconTypes.ActionsButtonActive, IconTypes.ActionsButtonActive, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			sectionP.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz"))));
			sectionP.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy2, "rrr", "rrr", "AU", ResString.GetMultilingualString("rrr", "rrr"))));
			categoryR.Sections.Add(sectionP);
			tree.Categories.Add(categoryR);

			var categoryJump = new ModuleCategory(ModuleTreeLoaderConstant.Category.Jump.Name, ResString.GetMultilingualString(ModuleTreeLoaderConstant.Category.Jump.Name, ModuleTreeLoaderConstant.Category.Jump.Name), null);
			var sectionJump = new ModuleSection("Jump", ResString.GetMultilingualString("Jump", "Jump"), null, null, IconTypes.ActionsButtonActive, IconTypes.ActionsButtonActive);
			sectionJump.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy2, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz"))));
			sectionJump.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "rrr", "rrr", "AU", ResString.GetMultilingualString("rrr", "rrr"))));
			categoryJump.Sections.Add(sectionJump);
			tree.Categories.Add(categoryJump);

			using (var bar = new TileNavigationBar())
			{
				bar.LoadModuleTree(tree);

				AssertEquals("Zayden", bar.navigationViewModel.Categories[0].Name);

				AssertEquals(ModuleTreeLoaderConstant.Subcategory.CashBook.DisplayText, bar.navigationViewModel.Categories[0].Buttons[0].DisplayName);
				AssertEquals("Lola", bar.navigationViewModel.Categories[0].Buttons[0].Subsections[0].DisplayName);
				AssertEquals("rrr", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0].Subsections[0]).Items[0].Name);
				AssertEquals("zzz", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0].Subsections[0]).Items[1].Name);

				AssertEquals(ModuleTreeLoaderConstant.Subcategory.ArchiveManager.DisplayText, bar.navigationViewModel.Categories[0].Buttons[1].DisplayName);
				AssertEquals("Apple", bar.navigationViewModel.Categories[0].Buttons[1].Subsections[0].DisplayName);
				AssertEquals("rrr", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[1].Subsections[0]).Items[0].Name);
				AssertEquals("zzz", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[1].Subsections[0]).Items[1].Name);

				AssertEquals("Bub", bar.navigationViewModel.Categories[0].Buttons[1].Subsections[1].DisplayName);
				AssertEquals("rrr", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[1].Subsections[1]).Items[0].Name);
				AssertEquals("zzz", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[1].Subsections[1]).Items[1].Name);

				AssertEquals("Rylan", bar.navigationViewModel.Categories[1].Name);

				AssertEquals(ModuleTreeLoaderConstant.Subcategory.Warehouse.DisplayText, bar.navigationViewModel.Categories[1].Buttons[0].DisplayName);
				AssertEquals("S", bar.navigationViewModel.Categories[1].Buttons[0].Subsections[0].DisplayName);
				AssertEquals("rrr", ((MenuSection)bar.navigationViewModel.Categories[1].Buttons[0].Subsections[0]).Items[0].Name);
				AssertEquals("zzz", ((MenuSection)bar.navigationViewModel.Categories[1].Buttons[0].Subsections[0]).Items[1].Name);

				AssertEquals(ModuleTreeLoaderConstant.Subcategory.Forwarding.DisplayText, bar.navigationViewModel.Categories[1].Buttons[1].DisplayName);
				AssertEquals("P", bar.navigationViewModel.Categories[1].Buttons[1].Subsections[0].DisplayName);
				AssertEquals("rrr", ((MenuSection)bar.navigationViewModel.Categories[1].Buttons[1].Subsections[0]).Items[0].Name);
				AssertEquals("zzz", ((MenuSection)bar.navigationViewModel.Categories[1].Buttons[1].Subsections[0]).Items[1].Name);

				AssertEquals("Jump", bar.navigationViewModel.Categories[2].Name);
				AssertEquals("Test Module2", ((MenuSection)bar.navigationViewModel.Categories[2].Buttons[0]).Items[0].Name);
				AssertEquals("Test Module", ((MenuSection)bar.navigationViewModel.Categories[2].Buttons[0]).Items[1].Name);
			}
		}

		[RequiresSTA]
		public void TestSectionOpensAutomatically_WhenOnlyOneSection()
		{
			var tree = new ModuleTree();

			var category1 = new ModuleCategory("Operate", ResString.GetMultilingualString("Operate", "Operate"), null);
			var sectionA = new ModuleSection("Singular", ResString.GetMultilingualString("Singular", "Singular"), null, null, IconTypes.ActionsButtonActive, IconTypes.ActionsButtonActive, ModuleTreeLoaderConstant.Subcategory.CashBook);
			sectionA.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz"))));
			sectionA.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy2, "rrr", "rrr", "AU", ResString.GetMultilingualString("rrr", "rrr"))));
			category1.Sections.Add(sectionA);
			tree.Categories.Add(category1);

			var category2 = new ModuleCategory("Maintain", ResString.GetMultilingualString("Maintain", "Maintain"), null);
			var sectionB = new ModuleSection("Double1", ResString.GetMultilingualString("Double1", "Double1"), null, null, IconTypes.ActionsButtonActive, IconTypes.ActionsButtonActive, ModuleTreeLoaderConstant.Subcategory.Warehouse);
			sectionB.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz"))));
			sectionB.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy2, "rrr", "rrr", "AU", ResString.GetMultilingualString("rrr", "rrr"))));
			category2.Sections.Add(sectionB);
			var sectionC = new ModuleSection("Double2", ResString.GetMultilingualString("Double2", "Double2"), null, null, IconTypes.ActionsButtonActive, IconTypes.ActionsButtonActive, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			sectionC.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz"))));
			sectionC.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy2, "rrr", "rrr", "AU", ResString.GetMultilingualString("rrr", "rrr"))));
			category2.Sections.Add(sectionC);
			tree.Categories.Add(category2);

			using (var bar = new TileNavigationBar())
			{
				bar.LoadModuleTree(tree);

				bar.SelectedCategory = category1;
				AssertEquals("Precondition", bar.SelectedCategory, category1);
				AssertEquals("The first section should be selected", true, bar.navigationViewModel.SelectedCategory.Buttons[0].IsSelected);
				AssertEquals("The first section should be open", bar.navigationViewModel.SelectedCategory.SelectedItem, bar.navigationViewModel.Categories[0].Buttons[0]);

				bar.SelectedCategory = category2;
				AssertEquals("Precondition", bar.SelectedCategory, category2);
				AssertEquals("The first section should not be selected", false, bar.navigationViewModel.Categories[1].Buttons[0].IsSelected);
				AssertEquals("The second section should not be selected", false, bar.navigationViewModel.Categories[1].Buttons[1].IsSelected);

				bar.navigationViewModel.SelectedCategory.SelectedItem = bar.navigationViewModel.Categories[1].Buttons[0];
				bar.navigationViewModel.Categories[1].Buttons[0].IsSelected = true;
				AssertEquals("The first section should now be selected", true, bar.navigationViewModel.Categories[1].Buttons[0].IsSelected);
				AssertEquals("The first section should now be open", bar.navigationViewModel.SelectedCategory.SelectedItem, bar.navigationViewModel.Categories[1].Buttons[0]);

				bar.SelectedCategory = category1;
				AssertEquals("Precondition", bar.SelectedCategory, category1);
				AssertEquals("The first section should be selected", bar.navigationViewModel.SelectedCategory.SelectedItem, bar.navigationViewModel.Categories[0].Buttons[0]);
				AssertEquals("The first section should be open", true, bar.navigationViewModel.Categories[0].Buttons[0].IsSelected);

				bar.navigationViewModel?.SearchViewModel.WaitingForSearchToComplete();
			}
		}

		[RequiresSTA]
		public void TestOpenOrganisationModuleAddsLinksAndHotkeys()
		{
			TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);

			using (var form = new TestMainFormWithoutUnreadItems())
			{
				form.Show();
				Application.DoEvents();

				const string sql = "select COUNT(*) from dbo.StmLink where STL_LinkType = 'RUM' and STL_ModuleID = 'Organisation'";
				Func<int> doCount = () => (int)Db.Connection.ExecuteScalar(sql);
				AssertEquals("There should be no module links", 0, doCount());

				var navBar = form.Bar;
				navBar.LoadModuleTree(ModuleTree.Tree);

				using (navBar.OpenModuleInNewWindowCore(new MainFormModule(ModuleIDs.Organisation), null))
				{
					AssertEquals("Pressing 'F7' key anywhere in Enterprise should popup Organisations module in new window", "Organization", Application.OpenForms[Application.OpenForms.Count - 1].Text);
				}

				AssertEquals("Exactly one recent modules link created", 1, doCount());
			}
		}

		[RequiresSTA]
		public void TestOpenShipmentModuleInNewWindowShowsTemplateRecords()
		{
			using (var form = new TestMainFormWithoutUnreadItems())
			{
				form.Show();
				Application.DoEvents();

				using (var modulePopup = (EmbeddedModulePopup)form.Bar.OpenModuleInNewWindowCore(new MainFormModule(ModuleIDs.JobShipment), null))
				{
					var module1 = modulePopup.Module;
					if (module1 is ZFilterGridModule gridmodule)
					{
						Assert("Job Shipment module should allow for Template Records", gridmodule.AllowLoadTemplateRecords);
						Assert("'Template Records' should be in the list of filters", gridmodule.FilterBusinessObject.ModuleFilters.Filter_List.ContainsCode("Template Records"));
					}
				}
				Assert(true);
			}
		}

		[RequiresSTA]
		public void TestModuleSortingInCustomizedLanguageEnvironment()
		{
			var sectionCustoms = ResString.GetMultilingualString("SectionCustoms", "Customs");
			var moduleCustomsDeclarations = ResString.GetMultilingualString("ModuleCustomsDeclarations", "Customs Declarations");
			var moduleReports = ResString.GetMultilingualString("ModuleReports", "Reports");

			var sectionCustomsGlobal = ResString.GetMultilingualString("SectionCustomsGlobal", "Customs Global");
			var moduleCAeManifestForwarder = ResString.GetMultilingualString("ModuleCAeManifestForwarder", "CA eManifest Forwarder");
			var moduleGlobalManifest = ResString.GetMultilingualString("ModuleGlobalManifest", "Global Manifest");

			using (Res.TemporarilySwitchLanguage("ZH-CN"))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(sectionCustoms.ResourceKey, new ResourceStringData(sectionCustoms.ResourceKey, "海关"));
				mockRes.Put(moduleCustomsDeclarations.ResourceKey, new ResourceStringData(moduleCustomsDeclarations.ResourceKey, "海关申报"));
				mockRes.Put(moduleReports.ResourceKey, new ResourceStringData(moduleReports.ResourceKey, "报表"));

				mockRes.Put(sectionCustomsGlobal.ResourceKey, new ResourceStringData(sectionCustomsGlobal.ResourceKey, "全球海关"));
				mockRes.Put(moduleCAeManifestForwarder.ResourceKey, new ResourceStringData(moduleCAeManifestForwarder.ResourceKey, "加拿大电子舱单货代"));
				mockRes.Put(moduleGlobalManifest.ResourceKey, new ResourceStringData(moduleGlobalManifest.ResourceKey, "全球舱单"));

				var tree = new ModuleTree();

				var cat = new ModuleCategory("Category", ResString.GetMultilingualString("Category", "Category"), null);
				var sectionCustoms1 = new ModuleSection("Customs", sectionCustoms, null, null, IconTypes.ActionsButtonActive, IconTypes.ActionsButtonActive, ModuleTreeLoaderConstant.Subcategory.Customs);
				sectionCustoms1.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "aaa", "aaa", "CN", moduleCustomsDeclarations)));
				sectionCustoms1.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy2, "bbb", "bbb", "CN", moduleReports)));

				var sectionCustoms2 = new ModuleSection("Customs Global", sectionCustomsGlobal, null, null, IconTypes.ActionsButtonActive, IconTypes.ActionsButtonActive, ModuleTreeLoaderConstant.Subcategory.Customs);
				sectionCustoms2.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy3, "ccc", "ccc", "CN", moduleCAeManifestForwarder)));
				sectionCustoms2.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.DummyDependent, "ddd", "ddd", "CN", moduleGlobalManifest)));

				cat.Sections.Add(sectionCustoms1);
				cat.Sections.Add(sectionCustoms2);
				tree.Categories.Add(cat);

				using (var bar = new TileNavigationBar())
				{
					bar.LoadModuleTree(tree);

					AssertEquals("海关", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0].Subsections[0]).DisplayName);
					AssertEquals("海关申报", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0].Subsections[0]).Items[0].Name);
					AssertEquals("报表", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0].Subsections[0]).Items[1].Name);

					AssertEquals("全球海关", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0].Subsections[1]).DisplayName);
					AssertEquals("加拿大电子舱单货代", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0].Subsections[1]).Items[0].Name);
					AssertEquals("全球舱单", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0].Subsections[1]).Items[1].Name);
				}
			}
		}

		[RequiresSTA]
		public void TestOpenModuleInNewWindow_NullCheck()
		{
			TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
			var testModuleId = DummyModuleIDs.Dummy;

			using (var form = new TestMainFormWithUnreadItems())
			{
				form.Show();
				Application.DoEvents();
				var navBar = form.Bar;

				navBar.LoadModuleTree(ModuleTree.Tree);
				var mainFormModule = new MainFormModule(testModuleId);
				mainFormModule.LicenceCheckpoint = Env.Licence.AlwaysAllow;

				AssertNull("Precondition: should be null.", navBar.OpenModuleInNewWindowCore(mainFormModule, null));
				AssertNoExceptionThrown("Null should have been handled.", () => navBar.OpenModuleInNewWindow(mainFormModule));
			}
		}

		[RequiresSTA]
		public void TestOpenModuleInNewWindowCore_NoPopup()
		{
			var testModuleId = DummyModuleIDs.DummyNoPopup;
			using (var dummyModule = ZModuleFactory.Instance.Create(testModuleId))
			{
				AssertNull("Precondition - test module should return null in ShowPopup()", dummyModule.ShowPopup());
			}

			TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
			using (var form = new TestMainFormWithoutUnreadItems())
			{
				form.Show();
				Application.DoEvents();
				var navBar = form.Bar;

				navBar.LoadModuleTree(ModuleTree.Tree);
				var mainFormModule = new MainFormModule(testModuleId);
				mainFormModule.LicenceCheckpoint = Env.Licence.AlwaysAllow;

				AssertNoExceptionThrown(() => navBar.OpenModuleInNewWindowCore(mainFormModule, null));
			}
		}

		[RequiresSTA]
		public void TestCategoryChangedEvent()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();

				var categoryChangedFired = false;
				var bar = form.Bar;
				bar.CategoryChanged += delegate
				{ categoryChangedFired = true; };

				bar.navigationViewModel.SelectedCategory = new NavigationMenuViewModel(ModuleTreeLoaderConstant.Category.Admin.DisplayText, ModuleTreeLoaderConstant.Category.Admin.Name, 0, false);
				Assert(categoryChangedFired);

				categoryChangedFired = false;
				bar.navigationViewModel.SelectedCategory = new NavigationMenuViewModel(ModuleTreeLoaderConstant.Category.Jump.DisplayText, ModuleTreeLoaderConstant.Category.Jump.Name, 0, false);
				Assert(categoryChangedFired);
			}
		}

		[RequiresSTA]
		public void TestLocalizationWhenNoResourcesCanBeFound()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();

				var bar = form.Bar;
				bar.LoadModuleTree(Tree());

				AssertEquals(category.Name, bar.navigationViewModel.Categories[0].Name);
				AssertEquals(ModuleTreeLoaderConstant.Subcategory.Forwarding.DisplayText, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0]).DisplayName);
				AssertEquals(ModuleTreeLoaderConstant.Subcategory.Forwarding.Letter, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0]).Letter);
				AssertEquals(section.DisplayTextWithoutAmpersand, bar.navigationViewModel.Categories[0].Buttons[0].Subsections[0].DisplayName);
				AssertEquals(module.Description, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0].Subsections[0]).Items[0].Name);
			}
		}

		[RequiresSTA]
		public void TestLocalizationWhenResourcesExists()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();

				using (IMockResourceStringCache cache = Res.UseMockData())
				{
					var data = new ResourceStringData("", "TEST");

					cache.Put("TestSection", data);
					cache.Put("TestDummy", data);

					var bar = form.Bar;
					bar.LoadModuleTree(Tree());

					AssertEquals(data.Caption, bar.navigationViewModel.Categories[0].Buttons[0].Subsections[0].DisplayName);
					AssertEquals(data.Caption, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0].Subsections[0]).Items[0].Name);
				}
			}
		}

		[RequiresSTA]
		public void TestFavoriteAndRecentSections()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();

				var bar = form.Bar;
				bar.LoadModuleTree(TreeWithFavoritesAndRecents());

				AssertEquals(ModuleTreeLoaderConstant.Category.Jump.Name, bar.navigationViewModel.Categories[0].Name);
				AssertEquals(3, bar.navigationViewModel.Categories[0].Buttons.Count);

				AssertEquals(SectionType.Favorite, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0]).Type);
				AssertEquals(SectionType.RecentItem, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[1]).Type);
				AssertEquals(SectionType.RecentModule, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[2]).Type);

				AssertEquals(RecentItemManager.Instance.MaximumNumberOfFavorites, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0]).MaxNumberOfItems);
				AssertEquals(RecentItemManager.Instance.MaximumNumberOfRecentItems, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[1]).MaxNumberOfItems);
				AssertEquals(RecentItemManager.Instance.MaximumNumberOfRecentModules, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[2]).MaxNumberOfItems);

				AssertEquals(dummyItemShortcut.UniqueKey, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0]).Items[0].Key);
				AssertEquals(dummyModuleShortcut.UniqueKey, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0]).Items[1].Key);
				AssertEquals(dummyModuleWithExtendedDescriptionShortcut.UniqueKey, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0]).Items[2].Key);
				AssertEquals(dummyItemShortcut.UniqueKey, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[1]).Items[0].Key);
				AssertEquals(dummyModuleShortcut.UniqueKey, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[2]).Items[0].Key);
				AssertEquals(dummyModuleWithExtendedDescriptionShortcut.UniqueKey, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[2]).Items[1].Key);

				AssertEquals("Jump category uses extended description", dummyModuleWithExtendedDescription.ExtendedDescription, ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0]).Items[2].Name);
				AssertEquals("Non-Jump category uses description", dummyModuleWithExtendedDescription.Description, ((MenuSection)((MenuSection)bar.navigationViewModel.Categories[1].Buttons[0]).Subsections[0]).Items[1].Name);

				foreach (var item in ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[0]).Items)
				{
					AssertNotNull("Move Action should be specified for Favorites", item.MoveAction);
				}

				AssertNull("Move Action should not be specified for recent items", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[1]).Items[0].MoveAction);
				AssertNull("Move Action should not be specified for recent items", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[2]).Items[0].MoveAction);
				AssertNull("Move Action should not be specified for recent items", ((MenuSection)bar.navigationViewModel.Categories[0].Buttons[2]).Items[1].MoveAction);
			}
		}

		[RequiresSTA]
		public void TestSelectFindbox()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();

				var bar = form.Bar;
				bar.LoadModuleTree(TreeWithJump());

				AssertEquals(ModuleTreeLoaderConstant.Category.Jump.Name, bar.navigationViewModel.Categories[0].Name);
				AssertNull("No category selected", bar.navigationViewModel.SelectedCategory);
				Assert("Not in search mode", !bar.navigationViewModel.IsInSearchMode);

				bar.SelectFindbox();
				Assert("Not in search mode", !bar.navigationViewModel.IsInSearchMode);

				bar.SwitchCategory(ModuleTree.Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name]);
				bar.SelectFindbox("D");
				Assert("In search mode", bar.navigationViewModel.IsInSearchMode);
				AssertEquals("SearchValue should be \"D\"", "D", bar.navigationViewModel.SearchViewModel.SearchValue);
				bar.navigationViewModel.SearchViewModel.WaitingForSearchToComplete();
				AssertEquals("Caret in SelectFindBox should be at the end of the initial text", 1, bar.TileControl.GetCaretPosition());
			}
		}

		[RequiresSTA]
		public void TestNavigationBarUsesThemeColors()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();

				var bar = form.Bar;
				var colorTheme = SystemDataRegistry.Instance.ColorTheme;

				AssertEquals("Tile Background Color", colorTheme.NavBarButtonColor1, bar.TileControl.TileBackgroundColor);
				AssertEquals("Tile Selected Color", colorTheme.NavBarGroupSelected1, bar.TileControl.TileSelectedColor);
				AssertEquals("Tile Text Color", colorTheme.NavBarTextColor, bar.TileControl.TileTextColor);
				AssertEquals("Group Background Color", colorTheme.NavBarGroupBackground1, bar.TileControl.GroupBackgroundColor);
				AssertEquals("Group Header Background Color", colorTheme.NavBarGroupHeaderBackground, bar.TileControl.GroupHeaderBackgroundColor);
				AssertEquals("Recent Background Color", colorTheme.NavBarRecentPanelBackground, bar.TileControl.RecentPanelBackgroundColor);
			}
		}

		[RequiresSTA]
		public void TestAddModuleLink()
		{
			using (var form = new TestForm())
			{
				form.Show();
				Application.DoEvents();
				var bar = form.Bar;

				var bizoGuid = Guid.NewGuid();
				var moduleIdString = ModuleIDs.JobShipment.Name + bizoGuid.ToString();
				Assert("Module lookup shouldn't contain this module info yet", !bar.ModuleLookup.ContainsKey(moduleIdString));

				var result = CreateMenuItemFromPK(bar, ModuleIDs.JobShipment, bizoGuid.ToString());
				AssertEquals("Key(Bizo PK) is different", moduleIdString, result.Key);
				AssertEquals("MultilingualText is different", "Shipments", result.MultilingualText);
				AssertEquals("Name is different", "Shipments", result.Name);
				AssertEquals("IsInFavourites is different", false, result.IsInFavorites);
				AssertEquals("IsTranslatable is different", true, result.IsTranslatable);
				AssertNotNull("LinkAction_PrimaryClick should not be null", result.LinkAction);
				AssertNotNull("LinkAction_SecondaryClick should not be null", result.LinkRightClickAction);
				AssertNotNull("FavoriteAction should not be null", result.FavoriteAction);
				AssertNull("MoveAction should be null", result.MoveAction);
				Assert("Module lookup should contain this module info now", bar.ModuleLookup.ContainsKey(moduleIdString));
			}
		}

		[RequiresSTA]
		public void TestOnTileLink_Click_OpenModule()
		{
			using (var form = new TestMainFormWithoutUnreadItems() { Text = "MainForm" })
			{
				form.Show();
				Application.DoEvents();

				var bar = form.Bar;
				bar.ModuleOpener = form;

				var bizoGuid = Guid.NewGuid();
				var result = CreateMenuItemFromPK(bar, ModuleIDs.Organisation, bizoGuid.ToString());

				result.LinkAction.Execute(null);
				AssertEquals("Open Organisations module in main form", "MainForm", Application.OpenForms[Application.OpenForms.Count - 1].Text);
			}
		}

		[RequiresSTA]
		public void TestOnTileLink_Click_OpenModuleInANewWindow()
		{
			using (var form = new TestMainFormWithoutUnreadItems())
			{
				form.Show();
				Application.DoEvents();

				var bar = form.Bar;

				var module = new MainFormModule(ModuleIDs.Organisation, System.Guid.NewGuid(), null, null);

				var menuItem = new CargoWise.Main.Navigation.MenuItem
				(
					module.ID,
					module.Description,
					module.ModuleID.ExtendedDescription,
					() => bar.OnTileLink_Click(this, module),
					() => bar.OnTileLink_RightClick(this, module),
					() => bar.OnFavorite_Click(this, module),
					null
				);

				using (RawDataRegistry.Instance.OpenModuleInANewWindow.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					menuItem.LinkAction.Execute(null);
					AssertEquals("Should popup Organisations module in a new window", "Organization", Application.OpenForms[Application.OpenForms.Count - 1].Text);
					Application.OpenForms[Application.OpenForms.Count - 1].Close();
				}
			}
		}

		CargoWise.Main.Navigation.MenuItem CreateMenuItemFromPK(TileNavigationBar bar, ModuleIdentifier moduleIdentifier, string bizoPK)
		{
			var openerInfo = new ModuleOpenerInfo(null, moduleIdentifier, bizoPK);
			var module = bar.GetModuleInfoForOpener(openerInfo).module;
			var item = new CargoWise.Main.Navigation.MenuItem
			(
				module.ID,
				module.Description,
				module.ModuleID.ExtendedDescription,
				() => bar.OnTileLink_Click(this, module),
				() => bar.OnTileLink_RightClick(this, module),
				() => bar.OnFavorite_Click(this, module),
				null
			);

			return item;
		}

		[RequiresSTA]
		public void TestGetModuleInfoForOpener()
		{
			using (TileNavigationBar bar = new TileNavigationBar())
			{
				var bizoGuid = Guid.NewGuid();
				var openerInfo = new ModuleOpenerInfo(null, ModuleIDs.JobShipment, bizoGuid.ToString());

				var result = bar.GetModuleInfoForOpener(openerInfo);

				AssertEquals(ModuleIDs.JobShipment, result.module.ModuleID);
				AssertEquals(bizoGuid, result.module.RecordKey);
			}
		}

		[RequiresSTA]
		public void TestGetModuleInfoForOpener_NullValues()
		{
			using (TileNavigationBar bar = new TileNavigationBar())
			{
				AssertNull(bar.GetModuleInfoForOpener(new ModuleOpenerInfo(null, null, Guid.NewGuid().ToString())));
				AssertNull(bar.GetModuleInfoForOpener(new ModuleOpenerInfo(null, null, "")));
				AssertNull(bar.GetModuleInfoForOpener(new ModuleOpenerInfo(null, ModuleIDs.JobShipment, null)));
				AssertNull(bar.GetModuleInfoForOpener(new ModuleOpenerInfo(null, null, null)));
			}
		}

		[RequiresSTA]
		public void TestGetModuleInfoForOpener_Controller()
		{
			using (var bar = new TileNavigationBar())
			{
				var openerInfo = new ModuleOpenerInfo(ControllerIDs.Customs.JobDeclaration, ModuleIDs.Customs.JobDeclaration, Guid.NewGuid().ToString());
				AssertNotEquals("Precondition: ControllerID not equal with ModuleID", ControllerIDs.Customs.JobDeclaration.Name, ModuleIDs.Customs.JobDeclaration.Name);

				var moduleInfo = bar.GetModuleInfoForOpener(openerInfo);

				AssertNotNull(moduleInfo);
				AssertContains("ControllerID=JobDeclaration", moduleInfo.module.RecordUrl);
			}
		}

		[RequiresSTA]
		public void TestSelectedCategory_NullReferenceException()
		{
			var moduleTree = new ModuleTree();
			var moduleCategory = new ModuleCategory("Operate", ResString.GetMultilingualString("Operate", "Operate"), null);
			var moduleSection = new ModuleSection("Singular", ResString.GetMultilingualString("Singular", "Singular"), null, null, IconTypes.ActionsButtonActive, IconTypes.ActionsButtonActive, ModuleTreeLoaderConstant.Subcategory.CashBook);

			moduleSection.Modules.Add(new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "zzz", "zzz", "AU", ResString.GetMultilingualString("zzz", "zzz"))));
			moduleCategory.Sections.Add(moduleSection);
			moduleTree.Categories.Add(moduleCategory);

			using (var tileNavigationBar = new TileNavigationBarForTest())
			{
				tileNavigationBar.LoadModuleTree(moduleTree);
				tileNavigationBar.SelectedCategory = moduleCategory;

				AssertNull(tileNavigationBar.SelectedCategory);
			}
		}

#region Implementation

		ModuleCategory category;
		ModuleSection section;
		MainFormModule module;
		MainFormModule dummyModuleWithExtendedDescription;
		LinkWrapper dummyModuleShortcut;
		LinkWrapper dummyItemShortcut;
		LinkWrapper dummyModuleWithExtendedDescriptionShortcut;

		protected override void SetUp()
		{
			base.SetUp();

			RecentItemManager.Reset();

			category = new ModuleCategory("TestCat", ResString.GetMultilingualString("TestCategory", "Test C&ategory"), null);
			section = new ModuleSection("TestSect", ResString.GetMultilingualString("TestSection", "Test Section"), null, null, IconTypes.Blank, IconTypes.Blank, ModuleTreeLoaderConstant.Subcategory.Forwarding);
			module = new MainFormModule(new ModuleInfo(DummyModuleIDs.Dummy, "whatever", "whatever", "AU", ResString.GetMultilingualString("TestDummy", "Dummy")));

			dummyItemShortcut = new LinkWrapper("Dummy", System.Guid.NewGuid(), "http://", "description");
			dummyModuleShortcut = new LinkWrapper("Dummy", Guid.Empty, string.Empty, string.Empty);
			dummyModuleWithExtendedDescriptionShortcut = new LinkWrapper("DummyWithExtendedDescription", Guid.Empty, string.Empty, string.Empty);
		}

		protected override void TearDown()
		{
			RecentItemManager.Reset();

			base.TearDown();
		}

		ModuleTree Tree()
		{
			var result = new ModuleTree();

			category.Sections.Add(section);
			section.Modules.Add(module);
			result.Categories.Add(category);

			return result;
		}

		ModuleTree TreeWithJump()
		{
			var result = new ModuleTree();

			result.Categories.Add(ModuleTree.Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name]);

			return result;
		}

		ModuleTree TreeWithFavoritesAndRecents()
		{
			var result = new ModuleTree();
			dummyModuleWithExtendedDescription = new LinkMainFormModule(dummyModuleWithExtendedDescriptionShortcut);

			var jumpCategory = new ModuleCategory(ModuleTreeLoaderConstant.Category.Jump, null);
			var favoriteSection = new ModuleSection(ModuleTreeLoaderConstant.Section.Favorites, null, null, IconTypes.Blank, IconTypes.Blank);
			favoriteSection.Modules.Add(new LinkMainFormModule(dummyItemShortcut));
			favoriteSection.Modules.Add(new LinkMainFormModule(dummyModuleShortcut));
			favoriteSection.Modules.Add(dummyModuleWithExtendedDescription);
			jumpCategory.Sections.Add(favoriteSection);

			var recentSection = new ModuleSection(ModuleTreeLoaderConstant.Section.RecentItems, null, null, IconTypes.Blank, IconTypes.Blank);
			recentSection.Modules.Add(new LinkMainFormModule(dummyItemShortcut));
			jumpCategory.Sections.Add(recentSection);

			var recentModuleSection = new ModuleSection(ModuleTreeLoaderConstant.Section.RecentModules, null, null, IconTypes.Blank, IconTypes.Blank);
			recentModuleSection.Modules.Add(new LinkMainFormModule(dummyModuleShortcut));
			recentModuleSection.Modules.Add(dummyModuleWithExtendedDescription);
			jumpCategory.Sections.Add(recentModuleSection);

			result.Categories.Add(jumpCategory);

			category.Sections.Add(section);
			section.Modules.Add(module);
			section.Modules.Add(dummyModuleWithExtendedDescription);
			result.Categories.Add(category);

			return result;
		}

		class TestForm : Form
		{
			public readonly TileNavigationBar Bar = new TileNavigationBar();

			public TestForm()
			{
				Controls.Add(Bar);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					Bar.Dispose();
				}
				base.Dispose(disposing);

				Bar.navigationViewModel?.SearchViewModel.WaitingForSearchToComplete();
			}
		}

		class TestMainFormWithUnreadItems : MainFormForTest
		{
			public readonly TileNavigationBar Bar = new TileNavigationBar();

			public TestMainFormWithUnreadItems()
			{
				Controls.Add(Bar);
			}

			internal override bool HasUnReadItemsMandatoryToRead() => true;

			internal override bool HasLicenceAndSecurityPermissions(MainFormModule module) => true;
		}

		class TestMainFormWithoutUnreadItems : MainFormForTest
		{
			public readonly TileNavigationBar Bar = new TileNavigationBar();

			public TestMainFormWithoutUnreadItems()
			{
				Controls.Add(Bar);
			}

			internal override bool HasUnReadItemsMandatoryToRead() => false;
		}

		class TileNavigationBarForTest : TileNavigationBar
		{
			protected override ModuleCategory GetCurrentCategory(NavigationMenuViewModel currentCategoryModel) => null;

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					this.navigationViewModel?.SearchViewModel.WaitingForSearchToComplete();
				}
				base.Dispose(disposing);
			}
		}

#endregion
	}
}
