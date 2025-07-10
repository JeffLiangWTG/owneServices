using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI.Controls;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TabPageControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			var tabPage = (TabPage)control;
			if (string.IsNullOrEmpty(tabPage.Text))
			{
				notifications.AddError(ControlDescription.GetControlPath(control) + " - TabPage.Text must always have a value");
			}
		}
	}
}
