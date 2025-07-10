using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Sailing.GUI;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class ArrivalPluginMenuTest : TestCaseWithFactory
	{
		public void TestItems()
		{
			bool found1 = false;
			bool found2 = false;
			typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menu, new object[] { EventArgs.Empty });
			foreach (MenuItem item in menu.MenuItems)
			{
				if (item.Text == "Create Impending Arrival Contingency Data")
				{
					found1 = true;
				}
				else if (item.Text == "Create Actual Arrival Contingency Data")
				{
					found2 = true;
				}
			}

			AssertEquals("Create Impending Arrival Contingency Data menu should exist", true, found1);
			AssertEquals("Create Actual Arrival Contingency Data menu should exist", true, found2);
		}

		ArrivalPluginMenu menu;
		protected override void SetUp()
		{
			base.SetUp();
			var voyageWrapper = new CustomsJobVoyageWrapper(Factory.New<JobVoyage>());
			var manager = new JobVoyageMessageManager(voyageWrapper);
			menu = new ArrivalPluginMenu(manager);
		}

		protected override void TearDown()
		{
			menu.Dispose();
			base.TearDown();
		}
	}
}
