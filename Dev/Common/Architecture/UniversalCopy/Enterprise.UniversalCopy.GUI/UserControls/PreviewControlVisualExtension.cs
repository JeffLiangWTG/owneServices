using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.UniversalCopy.GUI.Resources;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Notifications;

namespace Enterprise.UniversalCopy.GUI.UserControls
{
	class PreviewControlVisualExtension : IControlExtension
	{
		static PreviewControlVisualExtension()
		{
			NotificationIconScheme.Instance.SetMiniImage(PreviewControlNotification.Copy, UniversalCopyResources.Copy);
			NotificationIconScheme.Instance.SetMiniImage(PreviewControlNotification.Property, UniversalCopyResources.Reference);
			NotificationIconScheme.Instance.SetMiniImage(PreviewControlNotification.Empty, UniversalCopyResources.New);
			NotificationIconScheme.Instance.SetMiniImage(PreviewControlNotification.Default, UniversalCopyResources.New);
			NotificationIconScheme.Instance.SetMiniImage(PreviewControlNotification.Edit, UniversalCopyResources.Macro);
			NotificationIconScheme.Instance.SetMiniImage(PreviewControlNotification.Macro, UniversalCopyResources.Macro);
		}

		Control OwnerControl
		{
			get { return ownerControl; }
			set
			{
				if (ownerControl != value)
				{
					if (ownerControl != null)
					{
						ownerControl.Paint -= OwnerControl_Paint;
						if (ownerControl.Parent != null)
						{
							ownerControl.Parent.Paint -= Parent_Paint;
						}
					}

					ownerControl = value;

					if (ownerControl != null)
					{
						ownerControl.Paint += OwnerControl_Paint;
						if (ownerControl.Parent != null)
						{
							ownerControl.Parent.Paint += Parent_Paint;
						}
					}
				}
			}
		}
		Control ownerControl;

		public IExtendedControl Owner
		{
			get { return (IExtendedControl)OwnerControl; }
		}

		public void Initialize(IExtendedControl owner)
		{
			OwnerControl = owner as Control;
		}

		public void Dispose()
		{
			OwnerControl = null;
		}

		#region Visualization

		Color originalColor = Color.White;
		Color workColor = Color.White;
		readonly Color selectedControlColor = Color.LightSkyBlue;
		[SuppressMessage("Maintainability", "IDE0052: Remove unread private member.", Justification = "The field is only invoked in not Winzor Condition like '#if !WINZOR'.")]
		readonly Color selectedControlBorderColor = Color.Orange;

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "ControlDpiScalingHelper is already used")]
		public bool IsSelected
		{
			get { return isSelected; }
			set
			{
				if (value != isSelected)
				{
					isSelected = value;

					if (ownerControl.Parent != null)
					{
						var rectangle = OwnerControl.Bounds;
						rectangle.X -= ControlDpiScalingHelper.ScaleToCurrentDpiX(2);
						rectangle.Y -= ControlDpiScalingHelper.ScaleToCurrentDpiY(2);
						rectangle.Width += ControlDpiScalingHelper.ScaleToCurrentDpiX(4);
						rectangle.Height += ControlDpiScalingHelper.ScaleToCurrentDpiY(4);
						ownerControl.Parent.Invalidate(rectangle, false);
					}

					SetBackColor(IsSelected ? selectedControlColor : workColor);
					UpdateNotification();
				}
			}
		}
		bool isSelected;

		void SetBackColor(Color backColor)
		{
			var colorChanger = OwnerControl as IBackColorMutable;
			if (colorChanger != null)
			{
				if (originalColor == Color.White)
				{
					originalColor = OwnerControl.BackColor;
				}
				if (workColor == Color.White)
				{
					workColor = OwnerControl.BackColor;
				}
				colorChanger.ColorChanger.ForceBackColor(backColor);
			}
		}

		void OwnerControl_Paint(object sender, PaintEventArgs e)
		{
			UpdateNotification();
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "ControlDpiScalingHelper is already used")]
		void Parent_Paint(object sender, PaintEventArgs e)
		{
#if !WINZOR
			if (IsSelected)
			{
				using (var pen = new Pen(selectedControlBorderColor, 2))
				{
					var rectangle = OwnerControl.Bounds;
					rectangle.X -= ControlDpiScalingHelper.ScaleToCurrentDpiX(1);
					rectangle.Y -= ControlDpiScalingHelper.ScaleToCurrentDpiY(1);
					rectangle.Width += ControlDpiScalingHelper.ScaleToCurrentDpiX(2);
					rectangle.Height += ControlDpiScalingHelper.ScaleToCurrentDpiY(2);
					e.Graphics.DrawRectangle(pen, rectangle);
				}
			}
#endif
		}

		void UpdateNotification()
		{
			if (notificationType != null)
			{
				SetNotification(notificationType, argument);
			}
		}

		public void SetNotification(PreviewControlNotification notificationType, object argument = null)
		{
			var notificationExtension = Owner.Extensions.Get<INotificationExtension>();
			if (notificationExtension != null)
			{
				var message = notificationType.GetMessage(argument);

				if (notificationExtension.Notifications.GetNotifications(notificationType).Any(notification => notification.Message.Equals(message, StringComparison.Ordinal)))
				{
					return;
				}

				var notifications = new NotificationCollection();
				notifications.Add(notificationType, message);
				notificationExtension.Notifications = notifications;
			}

			workColor = notificationType.GetBackColor();
			if (!IsSelected)
			{
				SetBackColor(notificationType.GetBackColor());
			}

			this.notificationType = notificationType;
			this.argument = argument;
		}

		public void ClearNotifications()
		{
			var notificationExtension = Owner.Extensions.Get<INotificationExtension>();
			if (notificationExtension != null && notificationExtension.Notifications != null && notificationExtension.Notifications.Count() > 0)
			{
				notificationExtension.Notifications = new NotificationCollection();
				SetBackColor(originalColor);
			}

			notificationType = null;
			argument = null;
		}

		PreviewControlNotification notificationType;
		object argument;

		#endregion

		#region PreviewControlNotification

		[WTG.StaticAnalysis.Annotation.Immutable]
		public class PreviewControlNotification : INotificationType, INotificationColorProvider
		{
			public PreviewControlNotification(string messageFormat, Color backColor)
			{
				this.messageFormat = messageFormat;
				this.backColor = backColor;
			}

			readonly string messageFormat;
			readonly Color backColor;

			public string GetMessage(object argument)
			{
				return string.Format(messageFormat, argument);
			}

			public Color GetBackColor()
			{
				return backColor;
			}

			#region INotificationType members

			bool INotificationType.IsFatal { get { return false; } }

			int INotificationType.Severity { get { return 10; } }

			string INotificationType.EnumValueName { get { return Res.GetString("7163e74c-2d1c-4806-9c93-a62200050acb", "Universal Copy"); } }

			#endregion

			#region INotificationColorProvider members

			Color INotificationColorProvider.GetColor()
			{
				return SystemColors.Info;
			}

			Color INotificationColorProvider.GetFontColor()
			{
				return SystemColors.InfoText;
			}

			#endregion

			#region Notification types

			public static PreviewControlNotification Copy
			{
				get { return copy ?? (copy = new PreviewControlNotification(Res.GetString("09a4ffd6-a65f-4607-ab00-6f929a250e8d", "UC: Copy source value."), Color.Turquoise)); }
			}
			static PreviewControlNotification copy;

			public static PreviewControlNotification Property
			{
				get { return property ?? (property = new PreviewControlNotification(Res.GetString("0961c45d-2a8f-4169-97e6-28b0cc167f59", "UC: Copy value from property '{0}'.", "{0}"), Color.SkyBlue)); }
			}
			static PreviewControlNotification property;

			public static PreviewControlNotification Empty
			{
				get { return empty ?? (empty = new PreviewControlNotification(Res.GetString("bbd52c17-5536-4f15-b501-1941145352c5", "UC: Set empty value."), Color.Wheat)); }
			}
			static PreviewControlNotification empty;

			public static PreviewControlNotification Default
			{
				get { return _default ?? (_default = new PreviewControlNotification(Res.GetString("88aa2ff6-09b8-47ea-bfef-dacb141d1404", "UC: Set value default for type '{0}'.", "{0}"), Color.Wheat)); }
			}
			static PreviewControlNotification _default;

			public static PreviewControlNotification Edit
			{
				get { return edit ?? (edit = new PreviewControlNotification(Res.GetString("38faf5ab-5e7d-4c97-a6a5-36d4d8cbe59d", "UC: Set value '{0}'.", "{0}"), Color.LightPink)); }
			}
			static PreviewControlNotification edit;

			public static PreviewControlNotification Macro
			{
				get { return macro ?? (macro = new PreviewControlNotification(Res.GetString("bbd76e47-94a0-4325-be1b-e03a74e9ae04", "UC: Calculate value of macro '{0}'.", "{0}"), Color.LightPink)); }
			}
			static PreviewControlNotification macro;

			#endregion
		}

		#endregion
	}
}
