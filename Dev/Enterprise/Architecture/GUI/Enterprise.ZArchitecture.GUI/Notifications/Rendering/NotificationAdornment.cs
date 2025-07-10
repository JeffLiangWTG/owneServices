using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	/// <summary>
	/// Represents notification adornment for control
	/// </summary>
	public abstract class NotificationAdornment
	{
		Control control;
		INotificationPresenter presenter;
		IEnumerable<INotification> notifications = NotificationCollection.Empty;

		/// <summary>
		/// Gets the control.
		/// </summary>
		/// <value>The control.</value>
		public virtual Control Control
		{
			get { return control; }
		}

		/// <summary>
		/// Gets the current notifications for control.
		/// </summary>
		/// <value>The notifications.</value>
		public virtual IEnumerable<INotification> Notifications
		{
			get { return notifications; }
			set
			{
				Argument.NotNull(value, "notifications");
				notifications = value;
			}
		}

		/// <summary>
		/// Gets the presenter.
		/// </summary>
		/// <value>The presenter.</value>
		public virtual INotificationPresenter Presenter
		{
			get { return presenter; }
		}

		/// <summary>
		/// Gets the current state of notifications.
		/// </summary>
		/// <value>The notification state.</value>
		public INotificationType State
		{
			get { return notifications.GetHighestSeverityNotificationType(); }
		}

		/// <summary>
		/// Attaches adornment to given control
		/// </summary>
		/// <param name="control">Instance of <see cref="Control"/> class</param>
		/// <param name="presenter">Implementor of <see cref="INotificationPresenter"/> interface</param>
		public void Initialize(Control control, INotificationPresenter presenter)
		{
			SetPresenter(presenter);
			SetControl(control);
		}

		/// <summary>
		/// Sets the component for this adornment.
		/// </summary>
		/// <param name="control">The control.</param>
		protected internal virtual void SetControl(Control control)
		{
			Argument.NotNull(control, "control");

			if (this.control != null)
			{
				throw new ArgumentException("Adornment is already initialized with control");
			}

			this.control = control;
		}

		/// <summary>
		/// Sets the notification presenter for this adornment.
		/// </summary>
		/// <param name="presenter">The presenter.</param>
		protected internal virtual void SetPresenter(INotificationPresenter presenter)
		{
			Argument.NotNull(presenter, "presenter");

			if (this.presenter != null)
			{
				throw new ArgumentException("Adornment is already initialized with presenter");
			}

			this.presenter = presenter;
		}

		public virtual void Attach()
		{
		}

		public virtual void Detach()
		{
		}
	}
}
