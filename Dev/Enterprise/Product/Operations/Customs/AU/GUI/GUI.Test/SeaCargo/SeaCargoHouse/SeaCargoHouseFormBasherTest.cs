using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	[TestedType(typeof(SeaCargoHouseForm))]
	sealed class SeaCargoHouseFormBasherTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.FilteredHouseBills.AddNew();
			houseBill.CA_HouseBill = "ABC123";
			using (var testForm = new SeaCargoHouseForm(houseBill))
			{
				AssertContains("Sea Cargo House ABC123", testForm.FormCaption);
			}
		}

		public void TestTabOrder()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.FilteredHouseBills.AddNew();
			houseBill.CA_HouseBill = "ABC123";
			using (var testForm = new SeaCargoHouseForm(houseBill))
			{
				testForm.Show();
				var mainTabControl = testForm.FindSingle<ZTemplateTabControl>("MainTabControl");
				var allPages = mainTabControl.AllTabPages;
				var detailsTabPage = testForm.FindSingle<ZTabPage>("MainTabPage");
				AssertEquals("Details", detailsTabPage.CaptionResourceString.Caption);
				var detailsTabPageIdx = Array.IndexOf(allPages, detailsTabPage);
				var messagesTabPage = testForm.FindSingle<ZTabPage>("MessagesTabPage");
				AssertEquals("Messages", messagesTabPage.CaptionResourceString.Caption);
				var messagesTabPageIdx = Array.IndexOf(allPages, messagesTabPage);
				AssertGreaterThan(messagesTabPageIdx, detailsTabPageIdx);
				var workflowTabPage = testForm.FindSingle<ZWorkflowTabPage>("WorkflowTabPage");
				AssertEquals("Workflow & Tracking", workflowTabPage.CaptionResourceString.Caption);
				var workflowTabPageIdx = Array.IndexOf(allPages, workflowTabPage);
				AssertGreaterThan(workflowTabPageIdx, messagesTabPageIdx);
				var eDocsTabPage = testForm.FindSingle<ZTabPage>("eDocsTabPage");
				AssertEquals("eDocs", eDocsTabPage.CaptionResourceString.Caption);
				var eDocsTabPageIdx = Array.IndexOf(allPages, eDocsTabPage);
				AssertGreaterThan(eDocsTabPageIdx, workflowTabPageIdx);
				var notesTabPage = testForm.FindSingle<ZTabPage>("NotesTabPage");
				AssertEquals("Notes", notesTabPage.CaptionResourceString.Caption);
				var notesTabPageIdx = Array.IndexOf(allPages, notesTabPage);
				AssertGreaterThan(notesTabPageIdx, eDocsTabPageIdx);
				var logsTabPage = testForm.FindSingle<ZTabPage>("LogsTabPage");
				AssertEquals("Logs", logsTabPage.CaptionResourceString.Caption);
				var logsTabPageIdx = Array.IndexOf(allPages, logsTabPage);
				AssertGreaterThan(logsTabPageIdx, notesTabPageIdx);
				AssertEquals("Tab Count", 6, allPages.Length);
			}
		}

		public void TestMessagesTab()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill1 = oceanBill.FilteredHouseBills.AddNew();
			houseBill1.CA_HouseBill = "ABC123";
			var messageHB1 = houseBill1.Messages.AddNew();
			var houseBill2 = oceanBill.FilteredHouseBills.AddNew();
			houseBill2.CA_HouseBill = "ABC123";
			var messageHB2 = houseBill2.Messages.AddNew();
			using (var testForm = new SeaCargoHouseForm(houseBill1))
			{
				testForm.Show();
				var messagesTabPage = testForm.FindSingle<ZTabPage>("MessagesTabPage");
				AssertEquals("Messages", messagesTabPage.Text);
				var mainTabControl = testForm.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectTab(messagesTabPage);
				var messagesGrid = messagesTabPage.FindSingle<ZArchitecture.ZGrid>("MessagesGrid");
				messagesGrid.SelectAllElements();
				AssertEquals("Displays only the messages on the loaded House Bill", 1, messagesGrid.SelectedRowCount);
				AssertSame(messageHB1, messagesGrid.SelectedElements[0]);
			}
		}

		public void TestMessagingMenu()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.FilteredHouseBills.AddNew();
			using (var testForm = new SeaCargoHouseForm(houseBill))
			{
				testForm.Show();
				var menu = testForm.Menu.MenuItems.OfType<SeaCargoHouseMenu>().First();
				menu.ShowPopupMenu();
				CombineAssertions(() =>
				{
					AssertEquals("Menu.Text", "Sea Cargo", menu.Text);
					AssertEquals("MenuItems[0].Text", "Send &Underbond Requests", menu.MenuItems[0].Text);
					AssertEquals("MenuItems[1].Text", "&Send Message(s)", menu.MenuItems[1].Text);
					AssertEquals("MenuItems[2].Text", "&Amend Message(s)", menu.MenuItems[2].Text);
					AssertEquals("MenuItems[3].Text", "&Withdraw Message(s)", menu.MenuItems[3].Text);
					AssertEquals("MenuItems[4].Text", "&Reset to Original", menu.MenuItems[4].Text);
					AssertEquals("MenuItems[5].Text", "Messaging Problems? Click for HELP.", menu.MenuItems[5].Text);
				});
				AssertEquals("MenuItems.Count", 6, menu.MenuItems.Count);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.FilteredHouseBills.AddNew();
			return new SeaCargoHouseForm(houseBill)
			{ ControllerID = ControllerIDs.Customs.AU.SeaCargoHouseController };
		}
	}
}
