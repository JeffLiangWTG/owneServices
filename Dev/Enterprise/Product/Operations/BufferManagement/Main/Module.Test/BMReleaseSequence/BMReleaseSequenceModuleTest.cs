using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMReleaseSequenceModule))]
	class BMReleaseSequenceModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.BMReleaseSequence;
		}
	}

	class BMReleaseSequenceModuleSpecificsTest : TestCaseWithFactory
	{
		public void TestContextMenuView_ShouldOpenMainPortalPage_WhenThereIsNoSelectedItem()
		{
			ShouldOpenPage_WhenUsingContextMenu("View", "https://glow.portal.url/CSQ/Desktop#/", false);
		}

		public void TestContextMenuEdit_ShouldOpenMainPortalPage_WhenThereIsNoSelectedItem()
		{
			ShouldOpenPage_WhenUsingContextMenu("Edit", "https://glow.portal.url/CSQ/Desktop#/", false);
		}

		public void TestContextMenuDelete_ShouldOpenMainPortalPage_WhenThereIsNoSelectedItem()
		{
			ShouldOpenPage_WhenUsingContextMenu("Delete", "https://glow.portal.url/CSQ/Desktop#/", false);
		}

		public void TestContextMenuView_ShouldOpenSpecificSequencePage_WhenItemSelected()
		{
			ShouldOpenPage_WhenUsingContextMenu("View", "https://glow.portal.url/CSQ/Desktop#/formFlow/0d1a7285-c5af-4173-91c5-770b93c5cf83/" + seq2.PK, true);
		}

		public void TestContextMenuEdit_ShouldOpenSpecificSequencePage_WhenItemSelected()
		{
			ShouldOpenPage_WhenUsingContextMenu("Edit", "https://glow.portal.url/CSQ/Desktop#/formFlow/0d1a7285-c5af-4173-91c5-770b93c5cf83/" + seq2.PK, true);
		}

		public void TestContextMenuDelete_ShouldOpenSpecificSequencePage_WhenItemSelected()
		{
			ShouldOpenPage_WhenUsingContextMenu("Delete", "https://glow.portal.url/CSQ/Desktop#/formFlow/0d1a7285-c5af-4173-91c5-770b93c5cf83/" + seq2.PK, true);
		}

		public void TestContextMenuNew_ShouldOpenNewSequencePortalPage()
		{
			ShouldOpenPage_WhenUsingContextMenu("New", "https://glow.portal.url/CSQ/Desktop#/formFlow/2d32cde1-8d3b-4972-b040-0c714b2d2b67", false);
		}

		public void TestContextMenu_ShouldOpenMultiplePages_WhenUsingContextMenu()
		{
			using (var module = new BMReleaseSequenceModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var viewContextMenuItem = grid.ContextMenu.MenuItems.FindByText("&View");
				viewContextMenuItem.PerformClick();

				CheckRecentItems(module.ID.Name);
			}
		}

		public void TestMainMenuView_ShouldOpenMainPortalPage_WhenThereIsNoSelectedItem()
		{
			ShouldOpenPage_WhenUsingMainMenu("View", "https://glow.portal.url/CSQ/Desktop#/", false);
		}

		public void TestMainMenuEdit_ShouldOpenMainPortalPage_WhenThereIsNoSelectedItem()
		{
			ShouldOpenPage_WhenUsingMainMenu("Edit", "https://glow.portal.url/CSQ/Desktop#/", false);
		}

		public void TestMainMenuDelete_ShouldOpenMainPortalPage_WhenThereIsNoSelectedItem()
		{
			ShouldOpenPage_WhenUsingMainMenu("Delete", "https://glow.portal.url/CSQ/Desktop#/", false);
		}

		public void TestMainMenuView_ShouldOpenSpecificSequencePage_WhenItemSelected()
		{
			ShouldOpenPage_WhenUsingMainMenu("View", "https://glow.portal.url/CSQ/Desktop#/formFlow/0d1a7285-c5af-4173-91c5-770b93c5cf83/" + seq2.PK, true);
		}

		public void TestMainMenuEdit_ShouldOpenSpecificSequencePage_WhenItemSelected()
		{
			ShouldOpenPage_WhenUsingMainMenu("Edit", "https://glow.portal.url/CSQ/Desktop#/formFlow/0d1a7285-c5af-4173-91c5-770b93c5cf83/" + seq2.PK, true);
		}

		public void TestMainMenuDelete_ShouldOpenSpecificSequencePage_WhenItemSelected()
		{
			ShouldOpenPage_WhenUsingMainMenu("Delete", "https://glow.portal.url/CSQ/Desktop#/formFlow/0d1a7285-c5af-4173-91c5-770b93c5cf83/" + seq2.PK, true);
		}

		public void TestMainMenu_ShouldOpenMultiplePages_WhenUsingContextMenu()
		{
			using (var module = new BMReleaseSequenceModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				module.EditMenuItem.PerformClick();

				CheckRecentItems(module.ID.Name);
			}
		}

		public void TestDoubleClickBMReleaseSequence_ShouldOpenTheNewSequencePortalPage()
		{
			using (var module = new BMReleaseSequenceModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.PerformMouseDownForTest(0, 2);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("https://glow.portal.url/CSQ/Desktop#/formFlow/0d1a7285-c5af-4173-91c5-770b93c5cf83/" + seq3.PK, WebUrlLauncher.LastUrlLaunched);

				CheckRecentItems(module.ID.Name, true, seq3.PK);
			}
		}

		void ShouldOpenPage_WhenUsingContextMenu(string menuItem, string url, bool select = false)
		{
			using (var module = new BMReleaseSequenceModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				if (select)
				{
					grid.SelectSingleElement(seq2);
				}

				var viewContextMenuItem = grid.ContextMenu.MenuItems.FindByText("&" + menuItem);
				AssertNotNull(viewContextMenuItem);
				viewContextMenuItem.PerformClick();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(WebUrlLauncher.LastUrlLaunched, url);

				CheckRecentItems(module.ID.Name, select, seq2.PK);
			}
		}

		void ShouldOpenPage_WhenUsingMainMenu(string menuItem, string url, bool select = false)
		{
			using (var module = new BMReleaseSequenceModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				if (select)
				{
					grid.SelectSingleElement(seq2);
				}

				switch (menuItem)
				{
					case "View":
						module.ViewMenuItem.PerformClick();
						break;
					case "Edit":
						module.EditMenuItem.PerformClick();
						break;
					case "Delete":
						module.DeleteMenuItem.PerformClick();
						break;
				}

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(WebUrlLauncher.LastUrlLaunched, url);

				CheckRecentItems(module.ID.Name, select, seq2.PK);
			}
		}

		void CheckRecentItems(string module, bool select, ZGuid pk)
		{
			var moduleRecentItems = RecentItemManager.Instance.GetRecentItems(module);
			var commonRecentItems = RecentItemManager.Instance.GetRecentItems(string.Empty);
			if (select)
			{
				AssertStartsWith("item not found in module recent items", "edient:Command=ShowEditForm&ControllerID=BMReleaseSequence&BusinessEntityPK=" + pk, moduleRecentItems.First().STL_ItemUrl);
				AssertStartsWith("item not found in common recent items", "edient:Command=ShowEditForm&ControllerID=BMReleaseSequence&BusinessEntityPK=" + pk, commonRecentItems.First().STL_ItemUrl);
			}
			else
			{
				AssertEquals(0, moduleRecentItems.Count);
				AssertEquals(0, commonRecentItems.Count);
			}
		}

		void CheckRecentItems(string module)
		{
			var moduleRecentItems = RecentItemManager.Instance.GetRecentItems(module);
			AssertEquals(3, moduleRecentItems.Count);
			var str = string.Join(",", moduleRecentItems.Select(i => i.STL_ItemUrl));
			AssertContains(seq1.PK.ToString(), str);
			AssertContains(seq2.PK.ToString(), str);
			AssertContains(seq3.PK.ToString(), str);

			var commonRecentItems = RecentItemManager.Instance.GetRecentItems(string.Empty);
			AssertEquals(3, commonRecentItems.Count);
			str = string.Join(",", commonRecentItems.Select(i => i.STL_ItemUrl));
			AssertContains(seq1.PK.ToString(), str);
			AssertContains(seq2.PK.ToString(), str);
			AssertContains(seq3.PK.ToString(), str);
		}

		BMReleaseSequence seq1, seq2, seq3;

		protected override void SetUp()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glow.portal.url");

			seq1 = Factory.NewWithValidTestData<BMReleaseSequence>();
			seq2 = Factory.NewWithValidTestData<BMReleaseSequence>();
			seq3 = Factory.NewWithValidTestData<BMReleaseSequence>();
			Factory.Save();
		}
	}
}
