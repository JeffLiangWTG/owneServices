using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class CursorSwitcherTest : TestCase
	{
		public void TestCursorSwitcher()
		{
			Cursor.Current = Cursors.Default;
			using (new CursorSwitcher(Cursors.WaitCursor))
			{
				using (new CursorSwitcher(Cursors.WaitCursor))
				{
					AssertEquals(Cursors.WaitCursor, Cursor.Current);
				}
				AssertEquals(Cursors.WaitCursor, Cursor.Current);
			}
			AssertEquals(Cursors.Default, Cursor.Current);
		}
	}
}
