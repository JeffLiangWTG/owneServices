using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZMessageBoxWithCheckboxTest : ZMessageBoxTest
	{
		protected override ZMessageBox NewMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return new ZMessageBoxWithCheckbox(message, caption, buttons, icon, MessageBoxDefaultButton.Button1, "www.google.com", null);
		}

		protected override ZMessageBox NewMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultbutton)
		{
			return new ZMessageBoxWithCheckbox(message, caption, buttons, icon, defaultbutton, "http://msdn.microsoft.com/en-us/library/system.windows.forms.datagrid.hittesttype(v=VS.85).aspx", null);
		}

		public void TestDontAskMeAgain()
		{
			var registryItem = RawDataRegistry.Instance.ShowZGridDragAndDropInformation;
			registryItem.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, true);

			Assert("Pre-condition: value of registry item is true", (bool)registryItem.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty));
			using (var msgBox = new ZMessageBoxWithCheckbox("message", "test", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, null, registryItem))
			{
				msgBox.Show();
				AssertEquals("checkbox is not ticked", false, msgBox.DontAskMeAgainCheckBox.Checked);

				msgBox.DontAskMeAgainCheckBox.Checked = true;
				Assert("checkbox is ticked", msgBox.DontAskMeAgainCheckBox.Checked);
				msgBox.AcceptButton.PerformClick();
			}
			AssertEquals("registry item is set to false.", false, (bool)registryItem.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty));
		}

		public void TestSetDontAskMeAgainCheckBoxVisibility()
		{
			using (var msgBox = new ZMessageBoxWithCheckbox("message", "test", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, "www.google.com", null))
			{
				msgBox.Show();
				AssertEquals("Pre-condition: checkbox is visible.", true, msgBox.DontAskMeAgainCheckBox.Visible);

				msgBox.SetDontAskMeAgainCheckBoxVisibility(false);
				AssertEquals("Checkbox is invisible.", false, msgBox.DontAskMeAgainCheckBox.Visible);
			}
		}
	}
}
