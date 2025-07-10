using System;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	public static class MessageDialog
	{
		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
		{
			return OverridableMessageBox.Value.Show(text, caption, buttons);
		}

		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return OverridableMessageBox.Value.Show(text, caption, buttons, icon);
		}

		static readonly Overridable<IMessageBox> OverridableMessageBox = new Overridable<IMessageBox>(new MessageBoxWrapper());

#if DEBUG
		public static IDisposable OverrideMessageBox_ForTest(IMessageBox messageBox)
		{
			var savedValue = OverridableMessageBox.Value;
			OverridableMessageBox.Value = messageBox;
			return new DisposableAction(() => OverridableMessageBox.Value = savedValue);
		}
#endif
	}
}
