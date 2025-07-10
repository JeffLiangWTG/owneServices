using System.ComponentModel;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public partial class ConfirmationNotificationUserControl : ZUserControl
	{
#if DEBUG
		public ConfirmationNotificationUserControl()
		{
			InitializeComponent();
		}
#endif

		internal ConfirmationNotificationUserControl(ConfirmationNotification notification)
		{
			this.notification = notification;

			InitializeComponent();
			SetDataBinding(notification, string.Empty);
			SetupBackColor();

			if (notification.NotificationType == NotificationTypes.Error)
			{
				IgnoredCheckBox.Visible = false;
				HintTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(3, 3);
			}
			else
			{
				IgnoredCheckBox.CheckedChanged += (s, e) => SetupBackColor();
			}
		}

		readonly ConfirmationNotification notification;

		void SetupBackColor()
		{
			var colour = GetBackColour();

			BackColor = colour;
			HintTextBox.ColorChanger.ForceBackColor(colour);
		}

		Color GetBackColour()
		{
			if (!IgnoredCheckBox.Checked)
			{
				switch (notification.NotificationType)
				{
					case NotificationTypes.Error:
						return Color.Pink;

					case NotificationTypes.Warning:
						return Color.LightYellow;

					case NotificationTypes.MessageError:
						return Color.LightBlue;
				}
			}

			return SystemColors.Control;
		}
	}
}
