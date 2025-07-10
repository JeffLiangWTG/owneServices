using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.GUI.TileBar;
using CargoWise.Main.Navigation;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.GUI;
using Enterprise.BufferManagement.GUI.Test;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMBoardModule))]
	class BMBoardModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.BMBoard;
		}

		protected override ZFilterModule GetModule()
		{
			var module = base.GetModule() as BMBoardModule;
			// We are going to perform all standard check against the view board configuration mode.
			// Otherwise, some tests like TestShowViewForm_WhenLoadingInCorrectThreadReturnsNull_ShouldShowErrorAndNotThrowException would not make sense for the default action - opening visual bord - as we don't need to reload board on the main thread.
			// We could omit these checks but it is better to have them for the view board configuration mode.
			module.SetBoardViewMode_ForTest(BMBoardModule.BoardViewMode.Config);
			return module;
		}

		#region View Menu Items

		public void TestViewMenuItems_WhenPaveOnTheWebEnabled()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var module = new BMBoardModuleForTest())
			{
				var viewMenuItems = module.GetViewMenuItems.ToArray();
				AssertEquals(3, viewMenuItems.Length);
				AssertEquals("Visual Board", viewMenuItems[0].Text);
				AssertEquals("Visual Board on the Web", viewMenuItems[1].Text);
				AssertEquals("Visual Board Configuration", viewMenuItems[2].Text);
			}
		}

		public void TestViewMenuItems_WhenPaveOnTheWebDisabled()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var module = new BMBoardModuleForTest())
			{
				var viewMenuItems = module.GetViewMenuItems.ToArray();
				AssertEquals(2, viewMenuItems.Length);
				AssertEquals("Visual Board", viewMenuItems[0].Text);
				AssertEquals("Visual Board Configuration", viewMenuItems[1].Text);
			}
		}

		#endregion
	}

	class BMBoardModuleNonTransactionedTest : NonTransactionedTestCase
	{
		#region Nothing Is Selected in the Grid

		public void TestViewVisualBoard_ShouldShowErrorMessage_WhenNothingIsSelectedInTheGrid()
		{
			using (var module = new BMBoardModule())
			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			{
				var grid = (ZDisplayGrid)module.DisplayGrid;
				var viewContextMenuItem = grid.ContextMenu.MenuItems.FindByText("&View");
				AssertNotNull(viewContextMenuItem);
				var viewBoardContextMenuItem = viewContextMenuItem.MenuItems.FindByText("Visual Board");
				AssertNotNull(viewBoardContextMenuItem);
				viewBoardContextMenuItem.PerformClick();

				AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestViewVisualBoardOnTheWeb_ShouldShowErrorMessage_WhenNothingIsSelectedInTheGrid()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var module = new BMBoardModule())
			{
				var grid = (ZDisplayGrid)module.DisplayGrid;
				var viewContextMenuItem = grid.ContextMenu.MenuItems.FindByText("&View");
				AssertNotNull(viewContextMenuItem);
				var viewBoardContextMenuItem = viewContextMenuItem.MenuItems.FindByText("Visual Board on the Web");
				AssertNotNull(viewBoardContextMenuItem);
				viewBoardContextMenuItem.PerformClick();

				AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestViewVisualBoardConfiguration_ShouldShowErrorMessage_WhenNothingIsSelectedInTheGrid()
		{
			using (var module = new BMBoardModule())
			{
				var grid = (ZDisplayGrid)module.DisplayGrid;
				var viewContextMenuItem = grid.ContextMenu.MenuItems.FindByText("&View");
				AssertNotNull(viewContextMenuItem);
				var viewBoardContextMenuItem = viewContextMenuItem.MenuItems.FindByText("Visual Board Configuration");
				AssertNotNull(viewBoardContextMenuItem);
				viewBoardContextMenuItem.PerformClick();

				AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Default Action

		public void TestDefaultAction()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			AssertNotNull(config.BufferBoard);

			Factory.Save();

			using (var module = new BMBoardModuleForTest())
			using (var popup = module.ShowPopup() as EmbeddedModulePopup)
			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			{
				Application.DoEvents();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				AssertEquals(0, grid.VisibleRowCount);

				var filter = (BMBoardFilterControl)module.EmbeddedControl;
				filter.FirePerformSearch();
				Application.DoEvents();
				AssertEquals(1, grid.VisibleRowCount);

				module.HandleEnterOrDoubleClick_ExposedforTest();

				var visualBoardForm = ZApplication.GetOpenForms().OfType<VisualBoardForm>().SingleOrDefault();
				visualBoardForm.AwaitAll();
				AssertNotNull("Should open visual board on default action", visualBoardForm);

				visualBoardForm.Close();
				visualBoardForm.AwaitAll();
			}

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region View Board

		public void TestViewVisualBoard()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			AssertNotNull(config.BufferBoard);

			Factory.Save();

			using (var module = new BMBoardModuleForTest())
			using (var popup = module.ShowPopup() as EmbeddedModulePopup)
			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			{
				Application.DoEvents();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				AssertEquals(0, grid.VisibleRowCount);

				var filter = (BMBoardFilterControl)module.EmbeddedControl;
				filter.FirePerformSearch();
				Application.DoEvents();
				AssertEquals(1, grid.VisibleRowCount);

				var viewContextMenuItem = grid.ContextMenu.MenuItems.FindByText("&View");
				AssertNotNull(viewContextMenuItem);
				var viewBoardContextMenuItem = viewContextMenuItem.MenuItems.FindByText("Visual Board");
				AssertNotNull(viewBoardContextMenuItem);
				viewBoardContextMenuItem.PerformClick();

				var visualBoardForm = ZApplication.GetOpenForms().OfType<VisualBoardForm>().SingleOrDefault();
				AssertNotNull("Should open visual board", visualBoardForm);
				visualBoardForm.AwaitAll();

				visualBoardForm.Close();
				visualBoardForm.AwaitAll();
			}

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			var link = RecentItemManager.Instance.GetRecentItems(ControllerIDs.VisualBoard.Name).First();
			AssertEquals("Buffer Board - This Board", link.STL_ItemDescription);
		}

		public void TestViewVisualBoard_ForMultipleSelection()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			AssertNotNull(config.BufferBoard);
			AssertNotNull(config.BucketBoard);

			Factory.Save();

			using (var module = new BMBoardModuleForTest())
			using (var popup = module.ShowPopup() as EmbeddedModulePopup)
			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			{
				Application.DoEvents();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				AssertEquals(0, grid.VisibleRowCount);

				var filter = (BMBoardFilterControl)module.EmbeddedControl;
				filter.FirePerformSearch();
				Application.DoEvents();
				AssertEquals(2, grid.VisibleRowCount);

				grid.SelectAllElements();

				var viewContextMenuItem = grid.ContextMenu.MenuItems.FindByText("&View");
				AssertNotNull(viewContextMenuItem);
				var viewBoardContextMenuItem = viewContextMenuItem.MenuItems.FindByText("Visual Board");
				AssertNotNull(viewBoardContextMenuItem);
				viewBoardContextMenuItem.PerformClick();

				var visualBoardForms = ZApplication.GetOpenForms().OfType<VisualBoardForm>().ToArray();
				AssertEquals("Should open two visual boards", 2, visualBoardForms.Length);
				Application.DoEvents(); // necessary to guarantee that we run VisualBoardForm.SaveToRecentItems, the event on the show can happen after WaitUntilSet
				visualBoardForms[0].Close();
				visualBoardForms[1].Close();
			}

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			var links = RecentItemManager.Instance.GetRecentItems(ControllerIDs.VisualBoard.Name);
			AssertContainsExactElementsInAnyOrder(new string[] { "Buffer Board - This Board", "Bucket Board - This Board" }, links.Select(l => l.STL_ItemDescription.ToString()));
		}

		#endregion

		#region View Board on the Web

		public void TestViewVisualBoardOnTheWeb()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			AssertNotNull(config.BufferBoard);

			Factory.Save();

			using (var module = new BMBoardModuleForTest())
			using (var popup = module.ShowPopup() as EmbeddedModulePopup)
			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			{
				Application.DoEvents();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				AssertEquals(0, grid.VisibleRowCount);

				var filter = (BMBoardFilterControl)module.EmbeddedControl;
				filter.FirePerformSearch();
				Application.DoEvents();
				AssertEquals(1, grid.VisibleRowCount);

				var viewContextMenuItem = grid.ContextMenu.MenuItems.FindByText("&View");
				AssertNotNull(viewContextMenuItem);
				var viewBoardContextMenuItem = viewContextMenuItem.MenuItems.FindByText("Visual Board on the Web");
				AssertNotNull(viewBoardContextMenuItem);
				viewBoardContextMenuItem.PerformClick();

				Application.DoEvents();

				VisualBoardFormTest.AssertBoardOpenedOnTheWeb(config.BufferBoard, "address");

				AssertNull(ZApplication.GetOpenForms().OfType<BMBoardForm>().SingleOrDefault());
				AssertNull(ZApplication.GetOpenForms().OfType<VisualBoardForm>().SingleOrDefault());
			}

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			var link = RecentItemManager.Instance.GetRecentItems(ControllerIDs.VisualBoard.Name).First();
			AssertEquals("Buffer Board - This Board (Web)", link.STL_ItemDescription);
		}

		#endregion

		#region View Board Configuration

		public void TestViewVisualBoardConfiguration()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			AssertNotNull(config.BufferBoard);

			Factory.Save();

			using (var module = new BMBoardModuleForTest())
			using (var popup = module.ShowPopup() as EmbeddedModulePopup)
			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			{
				Application.DoEvents();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				AssertEquals(0, grid.VisibleRowCount);

				var filter = (BMBoardFilterControl)module.EmbeddedControl;
				filter.FirePerformSearch();
				Application.DoEvents();
				AssertEquals(1, grid.VisibleRowCount);

				var viewContextMenuItem = grid.ContextMenu.MenuItems.FindByText("&View");
				AssertNotNull(viewContextMenuItem);
				var viewBoardContextMenuItem = viewContextMenuItem.MenuItems.FindByText("Visual Board Configuration");
				AssertNotNull(viewBoardContextMenuItem);
				viewBoardContextMenuItem.PerformClick();

				Application.DoEvents();

				var configForm = ZApplication.GetOpenForms().OfType<BMBoardForm>().SingleOrDefault();
				AssertNotNull("Should open visual board configuration", configForm);

				configForm.Close();
			}

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			var link = RecentItemManager.Instance.GetRecentItems(ControllerIDs.BMBoard.Name).First();
			AssertEquals("Buffer Board - This Board (configuration)", link.STL_ItemDescription);
		}

		public void TestViewVisualBoardConfiguration_ForMultipleSelection()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			AssertNotNull(config.BufferBoard);
			AssertNotNull(config.BucketBoard);

			Factory.Save();

			using (var module = new BMBoardModuleForTest())
			using (var popup = module.ShowPopup() as EmbeddedModulePopup)
			{
				Application.DoEvents();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				AssertEquals(0, grid.VisibleRowCount);

				var filter = (BMBoardFilterControl)module.EmbeddedControl;
				filter.FirePerformSearch();
				Application.DoEvents();
				AssertEquals(2, grid.VisibleRowCount);

				grid.SelectAllElements();

				var viewContextMenuItem = grid.ContextMenu.MenuItems.FindByText("&View");
				AssertNotNull(viewContextMenuItem);
				var viewBoardContextMenuItem = viewContextMenuItem.MenuItems.FindByText("Visual Board Configuration");
				AssertNotNull(viewBoardContextMenuItem);
				viewBoardContextMenuItem.PerformClick();

				Application.DoEvents();

				var configForms = ZApplication.GetOpenForms().OfType<BMBoardForm>().ToArray();
				AssertNotNull("Should open two visual board configurations", configForms.Length);

				configForms[0].Close();
				configForms[1].Close();
			}

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			var links = RecentItemManager.Instance.GetRecentItems(ControllerIDs.BMBoard.Name);
			AssertContainsExactElementsInAnyOrder(new string[] { "Buffer Board - This Board (configuration)", "Bucket Board - This Board (configuration)" }, links.Select(l => l.STL_ItemDescription.ToString()));
		}

		public void TestShouldOpenBoardConfigurationFromRecentItems()
		{
			using (var module = new BMBoardModuleForTestingRecentItems())
			using (var filterControl = (BMBoardFilterControlForTestingRecentItems)module.GetNewFilterControlForGrid())
			{
				var board = Factory.NewWithValidTestData<BMBoard>();
				Factory.Save();

				var recentItem = new LinkWrapper(module.ID.Name, board.PK.ToGuid(), "http://", "description");
				RecentItemManager.Instance.AddOrUpdateRecentItems(module.ID.Name, recentItem);

				filterControl.Grid.SetParentFilterGridModule(module);
				filterControl.Show();
				filterControl.LoadRecentItems();

				var recentItemsControl = ((IFilterStripBaseControlForTest)filterControl).RecentItemsControlExposed;
				AssertNotNull(recentItemsControl);
				var viewModel = recentItemsControl.DataContext as MenuSection;

				var item = viewModel.Items.SingleOrDefault();
				AssertNotNull(item);

				item.LinkAction.Execute(null);

				Application.DoEvents();

				var configForm = ZApplication.GetOpenForms().OfType<BMBoardForm>().SingleOrDefault();
				AssertNotNull("Should open visual board configuration", configForm);

				configForm.Close();
			}
		}

		#endregion
	}

	#region Test Classes

	class BMBoardModuleForTest : BMBoardModule
	{
		public IEnumerable<System.Windows.Forms.MenuItem> GetViewMenuItems => GetNewStandardMenuItems().FindByText("&View").MenuItems.Cast<System.Windows.Forms.MenuItem>().ToList();
		public void HandleEnterOrDoubleClick_ExposedforTest() => HandleEnterOrDoubleClick();
	}

	class BMBoardModuleForTestingRecentItems : BMBoardModule
	{
		protected override IFilterControl GetNewFilterControl() => new BMBoardFilterControlForTestingRecentItems(GridCollection, (BMBoardFilterBusinessObject)FilterBusinessObject);
	}

	class BMBoardFilterControlForTestingRecentItems : BMBoardFilterControl, IFilterStripBaseControlForTest
	{
		public BMBoardFilterControlForTestingRecentItems(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
		}

		RecentItemsControl IFilterStripBaseControlForTest.RecentItemsControlExposed => RecentItemsControl;

		#region Other Staff

		ZLabel IFilterStripBaseControlForTest.AutoRefreshWarningLabelExposed => throw new NotImplementedException();

		ToolStripSplitButton IFilterStripBaseControlForTest.ToolStripFindDropButtonExposed => throw new NotImplementedException();

		ToolStripSplitButton IFilterStripBaseControlForTest.FindButtonExposed => throw new NotImplementedException();

		ToolStripButton IFilterStripBaseControlForTest.ToolStripSaveLayoutButtonExposed => throw new NotImplementedException();

		ToolStripMenuItem IFilterStripBaseControlForTest.ToolStripManageLayoutsButtonExposed => throw new NotImplementedException();

		bool IFilterStripBaseControlForTest.CanSaveColumnLayoutsExposed => throw new NotImplementedException();

		bool IFilterStripBaseControlForTest.CanSaveGridColoursExposed => throw new NotImplementedException();

		KPanel IFilterStripBaseControlForTest.FilterStripsPanelExposed => throw new NotImplementedException();

		ZToolStrip IFilterStripBaseControlForTest.ToolStripHelpExposed => throw new NotImplementedException();

		ToolStripButton IFilterStripBaseControlForTest.ToolStripClearButtonExposed => throw new NotImplementedException();

		void IFilterStripBaseControlForTest.AddOrUpdateExistingFindDropListItemExposed(StmModuleFilter filter)
		{
			throw new NotImplementedException();
		}

		void IFilterStripBaseControlForTest.HandleFindButtonDropDownItemClickExposed(ToolStripItem item)
		{
			throw new NotImplementedException();
		}

		bool IFilterStripBaseControlForTest.ProcessDialogKeyExposed(Keys keyData)
		{
			throw new NotImplementedException();
		}

		void IFilterStripBaseControlForTest.SetCanSaveColumnLayouts(bool canSaveColumnLayouts)
		{
			throw new NotImplementedException();
		}

		void IFilterStripBaseControlForTest.SetCanSaveGridColours(bool canSaveGridColours)
		{
			throw new NotImplementedException();
		}

		void IFilterStripBaseControlForTest.SetShouldPerformSearch(bool shouldPerformSearch)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	#endregion
}
