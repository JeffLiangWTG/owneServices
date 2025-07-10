using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class CheckBoxControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			if (!control.GetReadOnly())
			{
				(control as CheckBox).Checked = true;
			}
		}
	}
}
