using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	public partial class FilterFieldNotification : ZUserControl
	{
		INotificationType notificationType;
		FilterField filterField;

		public FilterFieldNotification()
		{
			InitializeComponent();
		}

		public FilterFieldNotification(FilterField filterField)
			: this()
		{
			FilterField = filterField;
			Visible = false;

			this.Disposed += new EventHandler(NotificationUserControl_Disposed);
		}

		public FilterField FilterField
		{
			get { return filterField; }
			set
			{
				if (value != filterField)
				{
					if (filterField != null)
					{
						cleanUpFilterField();
					}

					filterField = value;

					if (filterField != null)
					{
						filterField.NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(FilterField_NotificationsChanged);
					}
				}
			}
		}

		void cleanUpFilterField()
		{
			if (filterField != null)
			{
				filterField.NotificationsChanged -= new EventHandler<NotificationsChangedEventArgs>(FilterField_NotificationsChanged);
				filterField = null;
			}
		}

		void FilterField_NotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
			Visible = e.SourceOfNotificationChange.HasNotifications();
			if (Visible)
			{
				if (FilterField != null)
				{
					notificationType = FilterField.GetHighestSeverityNotificationType();
				}
			}

			Refresh();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (Parent != null)
			{
				using (var brush = new SolidBrush(Parent.BackColor))
				{
					e.Graphics.FillRectangle(brush, ClientRectangle);
				}
			}
			else
			{
				e.Graphics.FillRectangle(SystemBrushes.Control, ClientRectangle);
			}

			if (notificationType != null)
			{
				var image = NotificationIconScheme.Instance.GetMiniImage(notificationType);
				if (image != null)
				{
					e.Graphics.DrawImageUnscaled(image, 0, 0);
				}
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);

			if (FilterField != null && notificationType != null)
			{
				Balloon.Instance.Show(FilterField.Notifications, this, ClientRectangle, false);
			}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			Balloon.Instance.Hide();
		}

		void NotificationUserControl_Disposed(object sender, EventArgs e)
		{
			cleanUpFilterField();
		}

#if DEBUG

		internal void PerformMouseEnterForTesting()
		{
			OnMouseEnter(EventArgs.Empty);
		}

		internal void PerformMouseLeaveForTesting()
		{
			OnMouseLeave(EventArgs.Empty);
		}

#endif
	}
}
