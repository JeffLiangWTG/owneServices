using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms
{
	internal class ZWaitCursorChangerTest : TestCase
	{
		public void TestCursorSetUnset()
		{
			Cursor.Current = Cursors.Arrow;
			using (var waitCursorChanger = new ZWaitCursorChanger())
			{
				AssertEquals("Current cursor should switch to Cursors.WaitCursor", Cursors.WaitCursor, Cursor.Current);
			}
			AssertEquals("Current cursor should switch back to it's original cursor", Cursors.Arrow, Cursor.Current);
		}

		public void TestCursorSetUnsetWithForm()
		{
			Cursor.Current = Cursors.Arrow;
			var formToForceWaitCursor = new Form();
			formToForceWaitCursor.UseWaitCursor = false;

			using (var waitCursorChanger = new ZWaitCursorChanger(formToForceWaitCursor))
			{
				AssertEquals("Current cursor should switch to Cursors.WaitCursor", Cursors.WaitCursor, Cursor.Current);
				AssertEquals("Form should be forced to use wait cursor", true, formToForceWaitCursor.UseWaitCursor);
			}

			AssertEquals("Current cursor should switch back to it's original cursor", Cursors.Arrow, Cursor.Current);
			AssertEquals("UseWaitCursor flag should be switched back", false, formToForceWaitCursor.UseWaitCursor);

			formToForceWaitCursor.UseWaitCursor = true;
			using (var waitCursorChanger = new ZWaitCursorChanger(formToForceWaitCursor))
			{
				AssertEquals("Current cursor should switch to Cursors.WaitCursor", Cursors.WaitCursor, Cursor.Current);
				AssertEquals("Form should be forced to use wait cursor", true, formToForceWaitCursor.UseWaitCursor);
			}

			AssertEquals("Current cursor should switch back to it's original cursor", Cursors.Arrow, Cursor.Current);
			AssertEquals("UseWaitCursor flag should be switched back", true, formToForceWaitCursor.UseWaitCursor);
		}
	}
}
