using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	static class ZMessageBoxExtensionsForTest
	{
#if DEBUG
		public static DialogResult ShowDialogForTest(this ZMessageBox messageBox)
		{
			return UnitTestUserNotification.Instance.Show(messageBox.Message, messageBox.Text, messageBox.Buttons, messageBox.DefaultButton);
		}
#endif
	}
}
