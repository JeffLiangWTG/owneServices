using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class TestMenuItemToToolStripItemConverter : TestCase
	{
		public void TestConvertMenuItemToToolStripItem()
		{
			var item = new ZMenuItem();
			item.MenuItems.Add(new ZMenuItem("You && Me"));
			item.MenuItems.Add(new ZMenuItem("&Test1"));
			item.MenuItems.Add(new ZMenuItem("&You && Me"));
			item.MenuItems.Add(new ZMenuItem("Test3(&T)"));
			var toolStripItem = (ToolStripDropDownItem)MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(item);
			AssertEquals("You && Me", toolStripItem.DropDown.Items[0].Text);
			AssertEquals("Test1", toolStripItem.DropDown.Items[1].Text);
			AssertEquals("You && Me", toolStripItem.DropDown.Items[2].Text);
			AssertEquals("Test3(T)", toolStripItem.DropDown.Items[3].Text);
		}
	}
}
