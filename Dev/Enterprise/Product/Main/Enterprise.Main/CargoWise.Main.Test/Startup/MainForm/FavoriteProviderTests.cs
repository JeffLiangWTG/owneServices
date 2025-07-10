using System;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Navigation;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class FavoriteProviderTests : TestCaseWithFactory
	{
		[GuiTest]
		[RequiresSTA]
		public void TestUpdateFavoriteItem()
		{
			var actualMainForm = StartupOpenMainFormTask.MainFormInstance;
			DisposableLeakListener.Instance.StackTraceEnabled = true;
			try
			{
				using (StartupOpenMainFormTask.MainFormInstance = new TestMainForm())
				{
					StartupOpenMainFormTask.MainFormInstance.NavigationBar.LoadModuleTree(ModuleTree.Tree);
					var testedProvider = new FavoriteProvider();
					var wrapper1 = new LinkWrapper(ModuleIDs.Organisation.Name, Guid.NewGuid(), "recordUrl", "favorite 1");
					testedProvider.AddToFavorites(wrapper1);
					var itemKey = Guid.NewGuid();
					var wrapper2 = new LinkWrapper(ModuleIDs.Organisation.Name, itemKey, "recordUrl", "favorite 2");
					testedProvider.AddToFavorites(wrapper2);
					AssertEquals("should be 1 favorite item.", 2, RecentItemManager.Instance.FavoriteModules.Count);
					var favorite1 = RecentItemManager.Instance.FavoriteModules[0];
					var favorite2 = RecentItemManager.Instance.FavoriteModules[1];
					AssertEquals("favorite 1", favorite1.STL_ItemDescription);
					AssertEquals("favorite 2", favorite2.STL_ItemDescription);
					AssertEquals("fovorite item display.", "favorite 1", ((MenuSection)StartupOpenMainFormTask.MainFormInstance.NavigationBar.navigationViewModel.Categories[0].Buttons[0]).Items[0].MultilingualText);
					AssertEquals("fovorite item display.", "favorite 2", ((MenuSection)StartupOpenMainFormTask.MainFormInstance.NavigationBar.navigationViewModel.Categories[0].Buttons[0]).Items[1].MultilingualText);

					var wrapper3 = new LinkWrapper(ModuleIDs.Organisation.Name, itemKey, "recordUrl", "favorite 3");
					testedProvider.AddToRecentItems(wrapper3);

					AssertEquals("favorite 1", favorite1.STL_ItemDescription);
					AssertEquals("fovorite item with same Key should be updated.", "favorite 3", favorite2.STL_ItemDescription);
					AssertEquals("fovorite item display text.", "favorite 1", ((MenuSection)StartupOpenMainFormTask.MainFormInstance.NavigationBar.navigationViewModel.Categories[0].Buttons[0]).Items[0].MultilingualText);
					AssertEquals("fovorite item display text should be updated.", "favorite 3", ((MenuSection)StartupOpenMainFormTask.MainFormInstance.NavigationBar.navigationViewModel.Categories[0].Buttons[0]).Items[1].MultilingualText);
				}
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance = actualMainForm;
			}
		}

		class TestMainForm : MainForm
		{
			public TestMainForm()
			{
				NavigationBar = new TileNavigationBar();
			}

			protected override void Dispose(bool isNotFinalizing)
			{
				NavigationBar.Dispose();
				NavigationBar = null;
				base.Dispose(isNotFinalizing);
			}
		}

		[GuiTest, ExpectNoExceptions]
		public void TestAddToRecentItems_MultipleInputs()
		{
			FavoriteProvider testedProvider = new FavoriteProvider();

			// First run a test with a link that points to no particular module
			testedProvider.AddToRecentItems(new LinkWrapper("Dummy name"));

			// Now run with a valid module
			testedProvider.AddToRecentItems(new LinkWrapper(ModuleIDs.Organisation.Name));

			var actualMainForm = StartupOpenMainFormTask.MainFormInstance;
			// Now kill the navigation bar and try again
			try
			{
				StartupOpenMainFormTask.MainFormInstance = new MainForm();
				StartupOpenMainFormTask.MainFormInstance.NavigationBar = null;
				testedProvider.AddToRecentItems(new LinkWrapper(ModuleIDs.Organisation.Name));
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance.Dispose();
				StartupOpenMainFormTask.MainFormInstance = actualMainForm;
			}
		}

		public void TestAddToRecentItemsExceededSTL_ItemDescription()
		{
			var testedProvider = new FavoriteProvider();
			var stmLink = Factory.New<StmLink>();
			string recordDescription = CargoWise.Types.ZString.Replicate('a', stmLink.STL_ItemDescriptionInfo.MaxLength + 1);
			testedProvider.AddToRecentItems(new LinkWrapper(ModuleIDs.Organisation.Name, Guid.NewGuid(), "recordUrl", recordDescription));
			AssertEquals(string.Empty, CargoWise.Common.ErrorReporter.LastMessageReported);
		}

		public void TestUpdateRecentItemsExceededSTL_ItemDescription()
		{
			CargoWise.Common.ErrorReporter.Clear();
			var testedProvider = new FavoriteProvider();
			var shortcut = Factory.New<StmLink>();
			string recordDescription = CargoWise.Types.ZString.Replicate('a', shortcut.STL_ItemDescriptionInfo.MaxLength + 1);
			var wrapper = new LinkWrapper(ModuleIDs.Organisation.Name, Guid.NewGuid(), "recordUrl", recordDescription);
			testedProvider.AddToRecentItems(wrapper);
			testedProvider.AddToRecentItems(wrapper);
			AssertEquals(string.Empty, CargoWise.Common.ErrorReporter.LastMessageReported);
		}

		[GuiTest, ExpectNoExceptions]
		[RequiresSTA]
		public void TestRemoveFromRecentItems()
		{
			var wrapper = new LinkWrapper("test");
			var module = new LinkMainFormModule(wrapper);
			AssertNull(module.ModuleID);
			var testMainForm = new TestMainForm();
			try
			{
				StartupOpenMainFormTask.MainFormInstance = testMainForm;
				StartupOpenMainFormTask.MainFormInstance.NavigationBar.LoadModuleTree(ModuleTree.Tree);
				var testedProvider = new FavoriteProvider();
				testedProvider.DeleteFromFavorites(wrapper);
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance = null;
				testMainForm.Dispose();
			}
		}
	}
}
