using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;

namespace CargoWise.Windows.UI
{
	internal class ControlNotificationProvider : IDisposable
	{
		public ControlNotificationProvider(KNotificationProvider owner, Control control, object dataSource, string dataMember)
		{
			this.Owner = owner;
			this.Control = control;
			this.DataSource = dataSource;
			this.DataMember = dataMember;
			BindingMember = new KBindingMemberInfo(DataMember);

			Control.Disposed += new EventHandler(Control_Disposed);
			BindingReadyNotifier.ReadyChanged += delegate
			{ UpdateIsBinding(); };
			UpdateIsBinding();
		}

		~ControlNotificationProvider()
		{ Dispose(false); }

		public Control Control { get; private set; }
		public object DataSource { get; private set; }
		public string DataMember { get; private set; }

		#region Enabled / IsBinding / UpdateIsBinding

		public bool Enabled
		{
			get { return enabled; }
			set
			{
				CheckNotDisposed();
				if (enabled != value)
				{
					enabled = value;
					UpdateIsBinding();
				}
			}
		}
		bool enabled;

		bool IsBinding
		{
			get { return isBinding; }
			set
			{
				CheckNotDisposed();
				if (IsBinding != value)
				{
					if (IsBinding)
					{
						StopBinding();
					}
					this.isBinding = value;
					if (IsBinding)
					{
						StartBinding();
					}
				}
			}
		}
		bool isBinding;

		void UpdateIsBinding()
		{ IsBinding = Enabled && BindingReadyNotifier.Ready && Owner.BindingContext != null && !Owner.IsDesignMode; }

		#endregion

		#region StartBinding / StopBinding

		void StartBinding()
		{
			Control.Validated += new EventHandler(Control_ValidatedForUpdatingNotifications);

			if (IsNotSelfNotificationRendering)
			{
				boundObjectBindingManagerForNotifications = Owner.BindingContext[DataSource, BindingMember];
				if (boundObjectBindingManagerForNotifications != null)
				{
					boundObjectBindingManagerForNotifications.CurrentChanged += new EventHandler(BindingManagerForNotifications_CurrentChanged);
				}
				UpdateNotifications();
				UpdateNotificationsAccessorPropertyChangeHook();
			}
			ISelfNotificationRendering selfNotificationRendering = Control as ISelfNotificationRendering;
			if (selfNotificationRendering != null)
			{
				selfNotificationRendering.SetNotificationRenderContext(Owner);
			}
		}

		void StopBinding()
		{
			Control.Validated -= Control_ValidatedForUpdatingNotifications;

			if (IsNotSelfNotificationRendering)
			{
				if (boundObjectBindingManagerForNotifications != null)
				{
					boundObjectBindingManagerForNotifications.CurrentChanged -= new EventHandler(BindingManagerForNotifications_CurrentChanged);
				}
				UpdateNotificationsAccessorPropertyChangeHook();
			}

			boundObjectBindingManagerForNotifications = null;
			accessorProperty = null;
			notificationsAccessorProperty = null;
			parentBindingManagerBase = null;
			NotificationsOnObjectWithEventsHooked = null;
		}

		#endregion

		#region NotificationTarget

		Control notificationTarget;
		public Control NotificationTarget
		{
			get
			{
				if (notificationTarget == null)
				{
					this.notificationTarget = this.Control;
					while (this.notificationTarget is INotificationRenderTarget)
					{
						this.notificationTarget = ((INotificationRenderTarget)this.notificationTarget).ControlToRenderNotificationOn;
					}
				}
				return this.notificationTarget;
			}
		}

		#endregion

		#region Binding to Notifications

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule")]
		public void UpdateNotifications()
		{
			CheckNotDisposed();
			if (IsBinding)
			{
				BindingManagerBase parentBindingManager = string.IsNullOrEmpty(BindingMember.BindingField) ? Owner.BindingContext[DataSource] : ParentBindingManagerBase;
				NotificationCollection notifications = new NotificationCollection();
				if (Owner.ShouldRenderNotification(Control, parentBindingManager, BindingMember.BindingField))
				{
					IEnumerable<INotification> metadataNotifications = null;
					IEnumerable<INotification> objectNotifications = null;

					if (ParentBindingManagerBase != null && NotificationsAccessorProperty != null)
					{
						metadataNotifications = (IEnumerable<INotification>)NotificationsAccessorProperty.GetValue(ParentBindingManagerBase.Current);
					}

					INotificationSource source = null;
					if (boundObjectBindingManagerForNotifications.Position != -1)
					{
						CurrencyManager cm = boundObjectBindingManagerForNotifications as CurrencyManager;
						source = (cm != null ? cm.List : boundObjectBindingManagerForNotifications.Current) as INotificationSource;
					}
					NotificationsOnObjectWithEventsHooked = source;
					if (source != null)
					{
						objectNotifications = source.Notifications;
					}
					if (metadataNotifications != null)
					{
						notifications.AddRange(metadataNotifications);
					}

					if (objectNotifications != null)
					{
						notifications.AddRange(objectNotifications);
					}
				}
				Owner.SetControlNotifications(this, notifications);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		INotificationSource NotificationsOnObjectWithEventsHooked
		{
			set
			{
				if (value != notificationsOnObjectWithEventsHooked)
				{
					if (notificationsOnObjectWithEventsHooked != null && notificationsOnObjectProperty != null)
					{
						notificationsOnObjectProperty.RemoveValueChanged(notificationsOnObjectWithEventsHooked, new EventHandler(NotificationsOnNotificationsProperty_ValueChanged));
					}
					notificationsOnObjectWithEventsHooked = value;
					if (value != null)
					{
						notificationsOnObjectProperty = TypeDescriptor.GetProperties(notificationsOnObjectWithEventsHooked)["Notifications"];
					}
					if (notificationsOnObjectWithEventsHooked != null && notificationsOnObjectProperty != null)
					{
						notificationsOnObjectProperty.AddValueChanged(notificationsOnObjectWithEventsHooked, new EventHandler(NotificationsOnNotificationsProperty_ValueChanged));
					}
				}
			}
		}
		PropertyDescriptor notificationsOnObjectProperty;
		INotificationSource notificationsOnObjectWithEventsHooked;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule")]
		void UpdateNotificationsAccessorPropertyChangeHook()
		{
			object oldCurrent = currentParentBindingManagerCurrent.Target;
			if (oldCurrent != null && currentNotificationsAccessorProperty != null)
			{
				currentNotificationsAccessorProperty.RemoveValueChanged(oldCurrent, NotificationsAccessorProperty_ValueChanged);
			}

			if (NotificationsAccessorProperty != null &&
					ParentBindingManagerBase != null &&
					ParentBindingManagerBase.Position != -1 &&
					ParentBindingManagerBase.Current != null)
			{
				NotificationsAccessorProperty.AddValueChanged(ParentBindingManagerBase.Current, NotificationsAccessorProperty_ValueChanged);
				currentParentBindingManagerCurrent.Target = ParentBindingManagerBase.Current;
				currentNotificationsAccessorProperty = NotificationsAccessorProperty;
			}
		}
		readonly WeakReference currentParentBindingManagerCurrent = new WeakReference(null);
		PropertyDescriptor currentNotificationsAccessorProperty;

		PropertyDescriptor AccessorProperty
		{
			get
			{
				if (accessorProperty == null && ParentBindingManagerBase != null)
				{
					accessorProperty = ParentBindingManagerBase.GetItemProperties()[BindingMember.BindingField];
				}
				return accessorProperty;
			}
		}
		PropertyDescriptor accessorProperty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule")]
		PropertyDescriptor NotificationsAccessorProperty
		{
			get
			{
				if (notificationsAccessorProperty == null &&
						AccessorProperty != null &&
						ParentBindingManagerBase != null &&
						ParentBindingManagerBase.Position != -1 &&
						ParentBindingManagerBase.Current != null)
				{
					notificationsAccessorProperty = MetaData.GetMetaDataProperty(ParentBindingManagerBase.Current.GetType(), AccessorProperty, MetaDataTypes.Notifications);
				}
				return notificationsAccessorProperty;
			}
		}
		PropertyDescriptor notificationsAccessorProperty;

		BindingManagerBase ParentBindingManagerBase
		{
			get
			{
				if (parentBindingManagerBase == null && DataSource != null && !string.IsNullOrEmpty(DataMember))
				{
					parentBindingManagerBase = Owner.BindingContext[DataSource, BindingMember.BindingPath];
				}
				return parentBindingManagerBase;
			}
		}
		BindingManagerBase parentBindingManagerBase;

		void BindingManagerForNotifications_CurrentChanged(object sender, EventArgs e)
		{
			UpdateNotifications();
			UpdateNotificationsAccessorPropertyChangeHook();
		}

		void NotificationsOnNotificationsProperty_ValueChanged(object sender, EventArgs e)
		{ UpdateNotifications(); }

		void NotificationsAccessorProperty_ValueChanged(object sender, EventArgs e)
		{ UpdateNotifications(); }

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !IsDisposed)
			{
				Control.Disposed -= Control_Disposed;
				BindingReadyNotifier.Dispose();
				IsBinding = false;
				IsDisposed = true;
			}
		}

		public bool IsDisposed { get; private set; }

		void CheckNotDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("ControlNotificationProvider");
			}
		}

		#endregion

		#region Implementation

		BindingManagerBase boundObjectBindingManagerForNotifications;
		readonly KNotificationProvider Owner;
		readonly KBindingMemberInfo BindingMember;

		ReadyForBindingNotifier BindingReadyNotifier
		{ get { return bindingReadyNotifier ?? (bindingReadyNotifier = new ReadyForBindingNotifier(Control)); } }
		ReadyForBindingNotifier bindingReadyNotifier;

		bool IsNotSelfNotificationRendering
		{
			get
			{
				return
						!(Control is ISelfNotificationRendering) ||
						((ISelfNotificationRendering)Control).DefaultRenderingEnabled;
			}
		}

		void Control_Disposed(object sender, EventArgs e)
		{ Dispose(); }

		void Control_ValidatedForUpdatingNotifications(object sender, EventArgs e)
		{
			BindingManagerBase parentBindingManagerBase = this.ParentBindingManagerBase;
			if (parentBindingManagerBase != null)
			{
				Owner.NotifyDataControlFocused((Control)sender, parentBindingManagerBase, BindingMember.BindingField);
				UpdateNotifications();
			}
		}

		#endregion
	}
}
