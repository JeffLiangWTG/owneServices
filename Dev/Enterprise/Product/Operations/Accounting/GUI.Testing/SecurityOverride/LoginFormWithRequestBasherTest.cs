using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.GUI.Testing;

namespace Enterprise.Accounting.GUI.Testing
{
	public class LoginFormWithRequestBasherTest : LoginForm_Test
	{
		public void TestApprovalRequestButton()
		{
			using (LoginFormWithRequest form = new LoginFormWithRequest())
			{
				form.Shown += new EventHandler((sender, e) => form.ApprovalRequestButton_ForTestOnly.PerformClick());
				DialogResult result = form.ShowDialog();
				AssertEquals(DialogResult.Ignore, result);

				AssertEquals("Approval Request button displays correct text", "Approval Request", form.ApprovalRequestButton_ForTestOnly.Text);
			}
		}
	}
}
