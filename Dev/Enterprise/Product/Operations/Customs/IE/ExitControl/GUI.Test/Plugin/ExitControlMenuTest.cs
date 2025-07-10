using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.IE.ExitControl.Business;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing.Plugin
{
	class ExitControlMenuItemTest : TestCaseWithFactory
	{
		public void TestMenuItems()
		{
			var exitHeader = Factory.New<CusExitHeader>();

			var menu = new ExitControlMenuItem();
			AssertNull("ExitHeader", menu.ExitHeader);
			AssertEquals("MenuItems.Count", 0, menu.MenuItems.Count);

			menu.ExitHeader = exitHeader;
			AssertEquals("ExitHeader", exitHeader, menu.ExitHeader);
			AssertEquals("MenuItems.Count", 5, menu.MenuItems.Count);
			AssertSequencesEqual("All menu items", new[] { "&Create Exit Report", "&Select/Edit Report Items", "-", "Send to Customs", "Upload Supporting Documents" }, menu.MenuItems.Cast<MenuItem>().Select(x => x.Text));
		}
	}
}
