using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class MenuItemCollectionExtensionsTest : TestCase
	{
		public void TestSetAllVisible()
		{
			List<MenuItem> list = new List<MenuItem>();
			list.Add(new MenuItem("MenuItem1"));
			list.Add(new MenuItem("MenuItem2"));

			list.SetAllVisible(true);
			AssertEquals(true, list[0].Visible);
			AssertEquals(true, list[1].Visible);

			list.SetAllVisible(false);
			AssertEquals(false, list[0].Visible);
			AssertEquals(false, list[1].Visible);
		}
	}
}
