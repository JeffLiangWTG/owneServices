using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZButtonControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			CheckButton((ZButton)control, notifications);
		}

		#region Implementation

		void CheckButton(ZButton button, INotifications notifications)
		{
			if (!button.ClickHasBeenHooked && button.DialogResult == DialogResult.None)
			{
				throw new ApplicationException(button.Name + " does not have a click event");
			}
			CheckButtonCaptionNotCutOff(button, notifications);
			CheckHasText(button, notifications);
		}

		void CheckButtonCaptionNotCutOff(Button button, INotifications notifications)
		{
			using (var g = button.CreateGraphics())
			{
				g.PageUnit = GraphicsUnit.Pixel;

#if WINZOR
				var buttonTextAreaSize = ControlDpiScalingHelper.NewScaledSize(button.ClientSize.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(6), button.ClientSize.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(6), false);
#else
				var buttonTextAreaSize = ControlDpiScalingHelper.NewScaledSize(button.ClientSize.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(18), button.ClientSize.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(11), false);
#endif
				var textSize = g.MeasureString(button.Text, button.Font, buttonTextAreaSize);

				if (buttonTextAreaSize.Width >= 0 &&
					(textSize.Width > buttonTextAreaSize.Width || textSize.Height > buttonTextAreaSize.Height))
				{
					throw new ApplicationException(ControlDescription.GetControlPathAndLocation(button) + " - doesn't fit the display area (maximum display area is " + buttonTextAreaSize + ", actual caption area is " + textSize + ")");
				}
			}
		}

		void CheckHasText(ZButton button, INotifications notifications)
		{
			var hasNoText = string.IsNullOrEmpty(button.Text);
			var hasNoImage = (button.Image ?? button.BackgroundImage) == null;
			var suppressed = TypeDescriptor.GetAttributes(button)[typeof(SuppressControlRequiresTextBasherAttribute)] != null;
			var excluded = MissingResourceStringChecker.IsExcludedFromTest(button);

			if (hasNoText && hasNoImage && !suppressed && !excluded)
			{
				notifications.AddError(ControlDescription.GetControlPath(button) + " - Button.Text must always have a value");
			}
		}

		#endregion
	}
}
