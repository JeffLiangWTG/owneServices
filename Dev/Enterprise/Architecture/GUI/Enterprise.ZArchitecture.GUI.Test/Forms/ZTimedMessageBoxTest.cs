using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZTimedMessageBoxTest : ZMessageBoxTest
	{
		protected override ZMessageBox NewMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, string button1Text)
		{
			return new ZTimedMessageBox(message, caption, buttons, icon, button1Text, 3); // These are the unit tests for ZMessageBox
		}

		protected override ZMessageBox NewMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			return new ZTimedMessageBox(message, caption, buttons, icon, defaultButton, 3); // These are the unit tests for ZMessageBox
		}

		public void TestDialogResult_WhenTimeIsOut()
		{
			using (var messagebox = new ZTimedMessageBox("This is message", "This is caption", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, 3))
			{
				var result = messagebox.ShowDialog();

				AssertEquals("No should be the returned value", DialogResult.Cancel, result);
			}

			using (var messagebox = new ZTimedMessageBox("This is message", "This is caption", MessageBoxButtons.YesNo, MessageBoxIcon.Information, 3))
			{
				var result = messagebox.ShowDialog();

				AssertEquals("No should be the returned value", DialogResult.No, result);
			}

			using (var messagebox = new ZTimedMessageBox("This is message", "This is caption", MessageBoxButtons.OK, MessageBoxIcon.Information, 3))
			{
				var result = messagebox.ShowDialog();

				AssertEquals("OK should be the returned value", DialogResult.OK, result);
			}
		}
	}
}
