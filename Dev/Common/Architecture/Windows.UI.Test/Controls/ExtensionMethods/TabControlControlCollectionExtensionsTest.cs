using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
	sealed class TabControlControlCollectionExtensionsTest : TestCase
	{
		public void TestCorrectTabSelectedAfterwards()
		{
			using (TabControl tabControl = new TabControl())
			using (TabPage tabPage1 = new TabPage())
			using (TabPage tabPage2 = new TabPage())
			using (TabPage tabPage3 = new TabPage())
			using (TabPage tabPage4 = new TabPage())
			using (TabPage tabPage5 = new TabPage())
			using (TabPage tabPage6 = new TabPage())
			{
				tabControl.TabPages.InsertPage(tabPage1, 0);
				AssertEquals(-1, tabControl.SelectedIndex);
				tabControl.SelectedIndex = 0;
				tabControl.TabPages.InsertPage(tabPage2, 1);
				AssertEquals(0, tabControl.SelectedIndex);
				tabControl.TabPages.InsertPage(tabPage3, 0); //inserting while position is 0 or -1 doesn't cause selected tab to change - was causing problems with unit tests, form initialization, etc
				AssertEquals(0, tabControl.SelectedIndex);
				tabControl.SelectedIndex = 1;
				tabControl.TabPages.InsertPage(tabPage4, 0);
				AssertEquals(2, tabControl.SelectedIndex);
				tabControl.TabPages.InsertPage(tabPage5, 2);
				AssertEquals(3, tabControl.SelectedIndex);
				tabControl.TabPages.InsertPage(tabPage6, 4);
				AssertEquals(3, tabControl.SelectedIndex);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
		public void TestInsertPageWithInvalidParameter()
		{
			using (TabControl tabControl = new TabControl())
			using (TabPage tabPage = new TabPage())
			{
				AssertNoExceptionThrown(delegate
				{ tabControl.TabPages.InsertPage(tabPage, -1); });
			}
		}
	}
}
