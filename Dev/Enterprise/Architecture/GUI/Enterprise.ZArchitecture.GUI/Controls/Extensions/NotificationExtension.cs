using System;
using System.Collections.Generic;
using System.ComponentModel;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions
{
	#region Interface

	public interface INotificationExtension : IControlExtension, IBindableControlExtension
	{
		/// <summary>
		/// Gets or sets the current notifications.
		/// </summary>
		/// <value>An instance of <see cref="NotificationCollection" />.</value>
		IEnumerable<INotification> Notifications { get; set; }

		/// <summary>
		/// Gets the notification presenter in use.
		/// </summary>
		/// <value>The notification presenter.</value>
		INotificationPresenter Presenter { get; }

		/// <summary>
		/// Redraws this instance.
		/// </summary>
		void Redraw();
	}

	#endregion

	/// <summary>
	/// Notification extension for control. 
	/// <list type="Responsibilities">
	///		<item>Hold current notifications state</item>
	///		<item>Interact with notification presenter in order to display notifications indication</item>
	///		<item>Notify parent controls when notifications changes</item>
	///		<item></item>
	/// </list>
	/// </summary>
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	[ToolboxItem(false)]
	public class NotificationExtension : ControlExtension, INotificationExtension, IBindableComponent
	{
		#region Fields

		INotificationPresenter presenter;
		readonly NotificationBroadcaster notifier;

		IEnumerable<INotification> notifications = NotificationCollection.Empty;
		INotificationType lastState;

		#endregion

		#region Constructors

		public NotificationExtension()
			: this(NotificationBroadcaster.Instance)
		{
		}

		internal NotificationExtension(NotificationBroadcaster notifier)
		{
			this.notifier = notifier;
		}

		#endregion

		#region Presenter / Redraw / Notifications

		public INotificationPresenter Presenter
		{
			get { return presenter; }
		}

		public void Redraw()
		{
			Notifications = notifications;
		}

		public IEnumerable<INotification> Notifications
		{
			get { return notifications; }
			set
			{
				var newState = value.GetHighestSeverityNotificationType();

				if (newState == lastState && lastState == null)
				{
					return;
				}

				notifications = value;

				if (IsValidating())
				{
					return;
				}

				presenter.Notifications = value;

				if (newState != lastState)
				{
					lastState = newState;
					notifier.BroadcastNotificationsChange(newState, Owner.Host);
				}
			}
		}

		#endregion

		#region Mounting

		public override void Initialize(IExtendedControl control)
		{
			Initialize(control, NotificationPresenter.Create());
		}

		internal void Initialize(IExtendedControl control, INotificationPresenter presenter)
		{
			try
			{
				base.Initialize(control);
				this.presenter = presenter;
				presenter.Initialize(Owner.Host);
			}
			catch
			{
				presenter.Dispose();
				throw;
			}
		}

		bool isDisposed;
		public override void Dispose()
		{
			SetDataBinding(null, "");
			if (presenter != null)
			{
				presenter.Dispose();
			}
			OnDisposed(EventArgs.Empty);
			base.Dispose();
			isDisposed = true;
		}

		#endregion

		#region Exceptions

		[Serializable]
		internal class DuplicatePropertyMemberException : Exception
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception text")]
			public DuplicatePropertyMemberException(Control control, string dataMember)
				: base("Property members has duplicate entries. " + string.Format("Control: {0}-{1}, BindTo: {2}", control.GetType(), control.Name, dataMember))
			{
			}

#if NETFRAMEWORK
			protected DuplicatePropertyMemberException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

#endregion

		#region Binding

		public void SetDataBinding(object dataSource, string dataMember)
		{
			if (isDisposed)
			{
				return;
			}

			if (dataBindings != null)
			{
				dataBindings.Clear();
			}
			if (dataSource != null && Owner != null)
			{
				var host = Owner.Host;
				var bindingSource = KBindingSource.GetBindingSource(host);
				var boundControl = host as INotificationDataMembers;
				if ((boundControl != null || bindingSource != null) &&
					(boundControl != null || !string.IsNullOrEmpty(dataMember)))
				{
					if (boundControl != null && HasDuplicates(boundControl.NotificationDataMembers))
					{
						throw new DuplicatePropertyMemberException(host, dataMember);
					}
					if (boundControl != null && boundControl.NotificationDataMembers != null && boundControl.NotificationDataMembers.Length > SupportedNumberOfBoundProperties)
					{
						throw new NotImplementedException("Please add more bindable properties to support " + boundControl.NotificationDataMembers.Length + " property members");
					}
					AddBindings(
						dataSource,
						boundControl != null ? boundControl.NotificationDataMembers : new string[] { dataMember });
				}
			}
		}

		static readonly string[] NotificationsLiterals = new[] { "Notifications1", "Notifications2", "Notifications3", "Notifications4", "Notifications5" };

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "literals for performance")]
		void AddBindings(object dataSource, string[] propertyMembers)
		{
			if (propertyMembers == null)
			{
				return;
			}

			for (var i = 0; i < propertyMembers.Length; i++)
			{
				var propertyMember = propertyMembers[i];

				if (string.IsNullOrEmpty(propertyMember))
				{
					continue;
				}
				if (!IsPropertyExists(Owner.Host.BindingContext, dataSource, propertyMember + BindingHelper.InfoSuffix))
				{
					continue;
				}

				// this is simply to reduce the number of duplicate strings
				var propertyName = i < 5 ? NotificationsLiterals[i] : "Notifications" + (i + 1);

				var binding = new KBinding(propertyName, dataSource,
												propertyMember + BindingHelper.InfoSuffix + "+Notifications", false, DataSourceUpdateMode.Never);
				binding.FormattingEnabled = true;
				DataBindings.Add(binding);
			}

			foreach (KBinding binding in DataBindings)
			{
				ForceBinding(binding, Owner.Host);
			}
		}

		protected virtual bool IsPropertyExists(BindingContext bindingContext, object dataSource, string dataMember)
		{
			var member = new KBindingMemberInfo(dataMember);
			var bindingManager = Owner.Host.BindingContext[dataSource, member.BindingPath];
			return bindingManager.GetItemProperties()[member.BindingField] != null;
		}

		/// <summary>
		/// Add this Binding to a control and force the binding to commence even if the control is
		/// not visible. This is used for binding to Notifications for example when a tab isn't shown
		/// but the tab's notification icon needs to be updated based on the state of the control.
		/// </summary>
		protected virtual void ForceBinding(Binding binding, Control control)
		{
			binding.ForceBinding(control);
		}

		static bool HasDuplicates(string[] array)
		{
			for (var i = 0; i < array.Length; i++)
			{
				var entry = array[i];

				for (var j = i + 1; j < array.Length; j++)
				{
					var candidate = array[j];

					if (candidate == entry)
					{
						return true;
					}
				}
			}

			return false;
		}

		#endregion

		#region Bindable Properties

		internal const int SupportedNumberOfBoundProperties = 2;

		IEnumerable<INotification> notifications1 = NotificationCollection.Empty;
		IEnumerable<INotification> notifications2 = NotificationCollection.Empty;

		public IEnumerable<INotification> Notifications1
		{
			get { return notifications1; }
			set
			{
				notifications1 = value ?? NotificationCollection.Empty;
				UpdateNotifications();
			}
		}

		public IEnumerable<INotification> Notifications2
		{
			get { return notifications2; }
			set
			{
				notifications2 = value ?? NotificationCollection.Empty;
				UpdateNotifications();
			}
		}

		void UpdateNotifications()
		{
			var result = new NotificationCollection();

			result.AddRange(notifications1);
			result.AddRange(notifications2);

			Notifications = result;
		}

		#region Memory Leak Prevention

		public event EventHandler Notifications1Changed { add { } remove { } }
		public event EventHandler Notifications2Changed { add { } remove { } }

		#endregion

		#endregion

		#region Metadata

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			var builder = new ControlPropertyDescriptorBuilder<NotificationExtension>();
			for (var i = 1; i <= SupportedNumberOfBoundProperties; i++)
			{
				builder.Property("Notifications" + i, NotificationCollection.Empty, true);
			}
			return builder.Result;
		}

		#endregion

		#region IBindableComponent Members

		ControlBindingsCollection IBindableComponent.DataBindings
		{
			get { return DataBindings; }
		}
		ControlBindingsCollection DataBindings
		{
			get { return dataBindings ?? (dataBindings = new ControlBindingsCollection(this)); }
		}
		ControlBindingsCollection dataBindings;

		BindingContext IBindableComponent.BindingContext
		{
			get { return Owner.Host.BindingContext; }
			set { Owner.Host.BindingContext = value; }
		}

		#endregion

		#region IComponent Members

		void OnDisposed(EventArgs eventArgs)
		{
			if (Disposed != null)
			{
				Disposed(this, EventArgs.Empty);
			}
		}

		public event EventHandler Disposed;

		ISite IComponent.Site { get; set; }

		#endregion

		#region Implementation

		protected virtual bool IsValidating()
		{
			if (Owner.Extensions.Supports<IValidationExtension>())
			{
				return Owner.Extensions.Get<IValidationExtension>().IsValidating;
			}

			return false;
		}

		#endregion
	}
}
