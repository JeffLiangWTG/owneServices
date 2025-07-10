using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Balloons
{
	public class BalloonDescriptor
	{
		internal const int NotificationsCap = 20;

		public BalloonDescriptor(Control anchorControl, Rectangle anchorRectOnControl, string caption, string description, IEnumerable<INotification> notifications)
		{
			AnchorControl = anchorControl;
			Caption = caption;
			Description = description;
			Notifications = notifications;
			ParentForm = AnchorControl.FindForm();
			AnchorRectOnControl = anchorRectOnControl;
		}

		public BalloonDescriptor(Control anchorControl, string caption, string description, IEnumerable<INotification> notifications)
			: this(anchorControl, ControlDpiScalingHelper.NewScaledRectangle(0, 0, anchorControl.Width, anchorControl.Height, false), caption, description, notifications)
		{
		}

		public bool IsEmpty
		{
			get { return String.IsNullOrEmpty(Caption) && String.IsNullOrEmpty(Description) && (Notifications == null || !Notifications.HasNotifications()); }
		}

		public bool HideWhenMouseOverBalloon = true;

		public readonly Control AnchorControl;
		public readonly Form ParentForm;

		public readonly string Caption;
		public readonly string Description;

		public readonly IEnumerable<INotification> Notifications;

		public IEnumerable<INotification> AllUniqueNotifications
		{
			get { return Notifications == null ? null : AllUniqueNotificationsCore; }
		}

		IEnumerable<INotification> AllUniqueNotificationsCore
		{
			get
			{
				var sofar = new List<INotification>();
				foreach (var notification in Notifications)
				{
					if (!sofar.Contains(notification))
					{
						sofar.Add(notification);
						yield return notification;
					}
				}
			}
		}

		public IEnumerable<INotification> UniqueNotifications(int offset)
		{
			return Notifications == null ? null : UniqueNotificationsCore(offset);
		}

		IEnumerable<INotification> UniqueNotificationsCore(int offset)
		{
			var originalOffset = offset;
			var i = 0;
			foreach (var notification in AllUniqueNotifications)
			{
				if (offset < 1)
				{
					++i;
				}
				if (i <= NotificationsCap)
				{
					if (offset > 0)
					{
						--offset;
					}
					else
					{
						yield return notification;
					}
				}
			}
			if (originalOffset > 0)
			{
				yield return new Notification(NotificationType.Warning, Res.GetString("7c8f997b-9f07-436a-9d57-a1c603a790fd", "... ({0} more, scroll up)", originalOffset));
			}
			if (i > NotificationsCap)
			{
				yield return new Notification(NotificationType.Warning, Res.GetString("b873dcfb-5b03-42c5-8e98-3b07e4f8f268", "... ({0} more, scroll down)", i - NotificationsCap));
			}
		}

		public readonly Rectangle AnchorRectOnControl;
	}
}
