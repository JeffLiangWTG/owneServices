using System.Windows.Forms;

namespace Enterprise.Registry.GUI
{
	sealed class AWBCustomTextControlTest : NUnit.Framework.TestCase
	{
		public void TestReadOnly()
		{
			using (AWBCustomTextControl control = new AWBCustomTextControl())
			{
				control.ReadOnly = true;
				AssertEquals("ReadOnly", true, control.ReadOnly);
				AssertEquals("PasswordTextBox.ReadOnly", true, control.TextBox.ReadOnly);
				AssertEquals("ViewButton.Enabled", false, control.ViewButton.Enabled);

				control.ReadOnly = false;
				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("PasswordTextBox.ReadOnly", false, control.TextBox.ReadOnly);
				AssertEquals("ViewButton.Enabled", true, control.ViewButton.Enabled);

				control.ReadOnly = true;
				AssertEquals("ReadOnly", true, control.ReadOnly);
				AssertEquals("PasswordTextBox.ReadOnly", true, control.TextBox.ReadOnly);
				AssertEquals("ViewButton.Enabled", false, control.ViewButton.Enabled);
			}
		}

		public void TestMultiline()
		{
			using (AWBCustomTextControl testCtrl = new AWBCustomTextControl())
			{
				Assert(!testCtrl.TextBox.Multiline);
				AssertEquals(ScrollBars.None, testCtrl.TextBox.ScrollBars);
				Assert(!testCtrl.TextBox.AcceptsReturn);
			}

			using (AWBCustomTextControl testCtrl = new AWBCustomTextControl())
			{
				testCtrl.Multiline = true;
				Assert(testCtrl.TextBox.Multiline);
				AssertEquals(ScrollBars.Vertical, testCtrl.TextBox.ScrollBars);
				Assert(testCtrl.TextBox.AcceptsReturn);
			}
		}
	}
}
