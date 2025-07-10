using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

class ExitControlMenuItemTest : TestCaseWithFactory
{
	public void TestAddMenuItems()
	{
		var header = Factory.New<CusExitHeader>();

		using var menu = new ExitControlMenuItem() { ExitHeader = header };
		AssertContainsExactElementsInExactOrder(new[] { "Send to Customs" }, menu.MenuItems.Cast<MenuItem>().Select(x => x.Text));
	}
}
