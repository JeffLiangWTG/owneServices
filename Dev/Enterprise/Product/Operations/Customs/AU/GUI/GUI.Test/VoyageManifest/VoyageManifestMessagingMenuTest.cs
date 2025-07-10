using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class VoyageManifestMessagingMenuTest : TestCaseWithFactory
	{
		public void TestContingencyMenuItemExists()
		{
			bool found1 = false;
			bool found2 = false;
			bool found3 = false;
			typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Menu, new object[] { EventArgs.Empty });
			foreach (MenuItem item in Menu.MenuItems)
			{
				if (item.Text == "Create Cargo Report Contingency Data")
				{
					found1 = true;
				}
				else if (item.Text == "Create Impending Arrival Contingency Data")
				{
					found2 = true;
				}
				else if (item.Text == "Create Actual Arrival Contingency Data")
				{
					found3 = true;
				}
			}

			AssertEquals("Cargo Report Contingency Data menu should exist", true, found1);
			AssertEquals("Impending Arrival Contingency Data menu should exist", true, found2);
			AssertEquals("Actual Arrival Contingency Data menu should exist", true, found3);
		}

		public void TestConstructor()
		{
			AssertNotNull(Menu);
		}

		protected override void TearDown()
		{
			menu?.Dispose();
			base.TearDown();
		}

		VoyageManifestMessagingMenu menu;
		VoyageManifestMessagingMenu Menu
		{
			get
			{
				if (menu == null)
				{
					var transportHeader = Factory.New<CusSeaManTranHead>();
					var manager = new CusSeaManTranHeadMessageManager(transportHeader);
					menu = new VoyageManifestMessagingMenu(manager);
				}

				return menu;
			}
		}
	}
}
