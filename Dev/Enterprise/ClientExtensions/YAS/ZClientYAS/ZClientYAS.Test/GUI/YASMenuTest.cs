using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.YAS.GUI.Testing
{
	public class YASMenuTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			using (YASMenu menu = (YASMenu)YASMenu.New())
			{
				AssertEquals("Returns YAS Menu", typeof(YASMenu), menu.GetType());
			}
		}

		public void TestSetupTopLevelMenu()
		{
			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				using (YASMenuForTest menu = (YASMenuForTest)YASMenuForTest.New())
				{
					int menuItemCount = menu.DataMenuItemForTest.MenuItems.Count;
					menu.SetTopLevelMenuForTest();
					AssertEquals("Menu was added, Menu Item Count should be 1 extra", menuItemCount + 1, menu.DataMenuItemForTest.MenuItems.Count);
					AssertContainsMenuItem("Import Yamaha Invoices", menu);
				}

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Ukraine);
				using (YASMenuForTest menu = (YASMenuForTest)YASMenuForTest.New())
				{
					int menuItemCount = menu.DataMenuItemForTest.MenuItems.Count;
					menu.SetTopLevelMenuForTest();
					AssertEquals(menuItemCount, menu.DataMenuItemForTest.MenuItems.Count);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
		}

		void AssertContainsMenuItem(ZString menuItemText, YASMenuForTest menu)
		{
			bool isFound = false;
			foreach (MenuItem currentMenuItem in menu.DataMenuItemForTest.MenuItems)
			{
				if (currentMenuItem.Text == menuItemText)
				{
					isFound = true;
				}
			}

			Assert("Should have found " + menuItemText + ".", isFound);
		}
	}
}
