using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	public partial class NotificationView : ZUserControl, INotificationView
	{
		public NotificationView(string id, string message, Core.NotificationType notificationType, Action onClick)
		{
			InitializeComponent();
			SetStyle(ControlStyles.Selectable, false);

			this.id = id;

			linkLabel.Text = message;
			this.notificationType = notificationType;

			if (onClick != null)
			{
				linkLabel.Click += (s, e) => onClick();
			}
			else
			{
				linkLabel.ForeColor = SystemColors.ControlText;
				linkLabel.LinkBehavior = LinkBehavior.NeverUnderline;
			}

			UpdateBackColor();
		}

		readonly string id;

		public string Id => id;

		public string Message
		{
			get { return linkLabel.Text; }
			set { linkLabel.Text = value; }
		}

		public Core.NotificationType NotificationType
		{
			get { return notificationType; }
			set
			{
				if (value != notificationType)
				{
					notificationType = value;
					UpdateBackColor();
				}
			}
		}

		Core.NotificationType notificationType;

		void UpdateBackColor()
		{
			BackColor = GetColorForNotificationType(notificationType);
		}

		Color GetColorForNotificationType(Core.NotificationType type)
		{
			switch (type)
			{
				case Core.NotificationType.Error:
					return ColorSchema.ErrorBackgroundGradient.Item1;

				case Core.NotificationType.MessageError:
					return ColorSchema.MessageErrorBackgroundGradient.Item1;

				case Core.NotificationType.Warning:
					return ColorSchema.WarningBackgroundGradient.Item1;

				default:
					return ColorSchema.MessageErrorBackgroundGradient.Item2;
			}
		}
	}
}
