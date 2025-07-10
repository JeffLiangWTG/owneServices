using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCTOMenuTest : TestCaseWithFactory
	{
		public void TestContingencyMenuItemExists()
		{
			using (var menu = new TestAirCTOMenu(new CTOCusMAWBMessageManager(() => Factory.New<CTOCusMAWB>())))
			{
				menu.OnPopup(EventArgs.Empty);
				AssertEquals(true, ContainsMenuItem(menu, "Create Contingency Data"));
			}
		}

		bool ContainsMenuItem(Menu menu, string text)
		{
			foreach (MenuItem menuItem in menu.MenuItems)
			{
				if (menuItem.Text == text)
				{
					return true;
				}
			}
			return false;
		}

		sealed class TestAirCTOMenu : AirCTOMenu
		{
			public TestAirCTOMenu(CTOCusMAWBMessageManager manager) : base(manager)
			{
			}

			public new void OnPopup(EventArgs e) => base.OnPopup(e);
		}
	}
}
