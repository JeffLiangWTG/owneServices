using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	public abstract class NotificationPresenter : INotificationPresenter
	{
		public NotificationPresenter()
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		public static INotificationPresenter Create()
		{
			return new NotificationAdornmentPresenter();
		}

		#region Fields

		Control control;
		IEnumerable<INotification> notifications = NotificationCollection.Empty;

		#endregion

		#region Mounting

		public virtual void Initialize(Control control)
		{
			this.control = control;
		}

		public virtual void Dispose()
		{
			control = null;
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		#endregion

		#region Implementation

		public virtual IEnumerable<INotification> Notifications
		{
			get { return notifications; }
			set
			{
				notifications = value;

				if (notifications.GetHighestSeverityNotificationType() != null)
				{
					Display();
				}
				else
				{
					Clear();
				}
			}
		}

		protected Control Control
		{
			get { return control; }
		}

		#endregion

		#region Abstract

		protected abstract void Display();
		protected abstract void Clear();

		#endregion
	}
}
