using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTimeZoneFindBoxControlBasher : ZAutoCompleteFindBoxBasher
	{
		public override void Bash(Control control, INotifications notifications)
		{
			base.Bash(control, notifications);
			var timeZoneFindBox = (ZTimeZoneFindBox)control;

			if (timeZoneFindBox.TimeZoneComboBox.Width < ControlDpiScalingHelper.ScaleToCurrentDpiX(50) && timeZoneFindBox.TimeZoneComboBox.Width != 0)
			{
				notifications.AddError(
					ControlDescription.GetControlPath(control) +
					" - ZTimeZoneFindBox TimeZoneComboBox is " +
					timeZoneFindBox.Width + " and is too small.");
			}
		}
	}
}
