using System.Windows.Forms;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDateEditControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			var dateEdit = (ZDateEdit)control;
			TestKeyStrokeHelper.SendKeyToControl(dateEdit.DateTextBox, Keys.F5);
		}
	}
}
