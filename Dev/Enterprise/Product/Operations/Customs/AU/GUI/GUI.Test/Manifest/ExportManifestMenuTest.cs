using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.ExportManifest.GUI.Testing
{
	sealed class ExportManifestMenuTest : TestCase
	{
		public void TestDefaultVisibility()
		{
			AssertEquals(2, VisibleMenuItems.Count); // should be 1, plus one for reset to original, 2
			AssertEquals("&Reset to original", VisibleMenuItems[0].Text);
			AssertEquals("&Import Data", VisibleMenuItems[1].Text);
		}

		public void TestShowManifestVisibility()
		{
			menu.ShowManifestMenuItems();
			CheckManifestVisibility();
			menu.ShowDepartureMenuItems();
			menu.ShowManifestMenuItems();
			CheckManifestVisibility();
		}

		public void TestShowDepartureVisibility()
		{
			menu.ShowDepartureMenuItems();
			CheckDepartureVisibility();
			menu.ShowManifestMenuItems();
			menu.ShowDepartureMenuItems();
			CheckDepartureVisibility();
		}

		public void TestShowOldVisibility()
		{
			menu.ShowOld();
			CheckOldVisibility();
			menu.ShowManifestMenuItems();
			menu.ShowDepartureMenuItems();
			menu.ShowOld();
			CheckOldVisibility();
		}

		void CheckManifestVisibility()
		{
			AssertEquals(6, VisibleMenuItems.Count);
			AssertEquals("&Declare Manifest", VisibleMenuItems[0].Text);
			AssertEquals("&Withdraw Manifest", VisibleMenuItems[1].Text);
			AssertEquals("&Reset to original", VisibleMenuItems[2].Text);
			AssertEquals(CMRMessage.MessagingHelpMenuCaption, VisibleMenuItems[3].Text);
			AssertEquals("-", VisibleMenuItems[4].Text);
			AssertEquals("&Import Data", VisibleMenuItems[5].Text);
		}

		void CheckDepartureVisibility()
		{
			AssertEquals(6, VisibleMenuItems.Count);
			AssertEquals("&Reset to original", VisibleMenuItems[0].Text);
			AssertEquals("&Declare Departure Report", VisibleMenuItems[1].Text);
			AssertEquals("&Withdraw Departure Report", VisibleMenuItems[2].Text);
			AssertEquals(CMRMessage.MessagingHelpMenuCaption, VisibleMenuItems[3].Text);
			AssertEquals("-", VisibleMenuItems[4].Text);
			AssertEquals("&Import Data", VisibleMenuItems[5].Text);
		}

		void CheckOldVisibility()
		{
			AssertEquals(7, VisibleMenuItems.Count);
			AssertEquals("&Declare Manifest", VisibleMenuItems[0].Text);
			AssertEquals("&Withdraw Manifest", VisibleMenuItems[1].Text);
			AssertEquals("&Reset to original", VisibleMenuItems[2].Text);
			AssertEquals("&Declare Departure Report", VisibleMenuItems[3].Text);
			AssertEquals("&Withdraw Departure Report", VisibleMenuItems[4].Text);
			AssertEquals("-", VisibleMenuItems[5].Text);
			AssertEquals("&Import Data", VisibleMenuItems[6].Text);
		}

		ExportManifestMenu menu;
		protected override void SetUp()
		{
			base.SetUp();
			menu = ExportManifestMenu.GetMenu();
		}

		protected override void TearDown()
		{
			menu.Dispose();
			base.TearDown();
		}

		List<MenuItem> VisibleMenuItems
		{
			get
			{
				List<MenuItem> result = new List<MenuItem>();
				foreach (MenuItem item in menu.MenuItems)
				{
					if (item.Visible)
					{
						result.Add(item);
					}
				}

				return result;
			}
		}
	}
}
