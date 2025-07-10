using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZCodeFindBoxControlBasher : ZAutoCompleteFindBoxBasher
	{
		public override void Bash(Control control, INotifications notifications)
		{
			base.Bash(control, notifications);
			var findBox = (ZCodeFindBox)control;
			if (findBox.ShowDescriptionBox && findBox.DescriptionBox.Width < ControlDpiScalingHelper.ScaleToCurrentDpiX(30) && findBox.DescriptionBox.Width != 0)   // If you're seeing zero, you're doing something wrong, Mr Test
			{
				notifications.AddError(
					ControlDescription.GetControlPath(control) +
					" - ShowDescriptionBox is true but DescriptionBox.Width is " +
					findBox.DescriptionBox.Width + ". Set ShowDescriptionBox to false.");
			}
		}
	}
}
