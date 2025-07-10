using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// A collection of INotification objects.
	/// </summary>
	[DebuggerDisplay("Count={Count},HasWarnings={this.HasWarnings()},HasErrors={this.HasErrors()}")]
	[DebuggerTypeProxy(typeof(NotificationCollection_DebuggerTypeProxy))]
	[Serializable]
	public class NotificationCollection : Collection<INotification>, INotifications
	{
		[DebuggerStepThrough]
		public NotificationCollection()
		{
		}

		public NotificationCollection(params INotification[] notifications)
		{
			this.AddRange(notifications);
		}

		public NotificationCollection(IEnumerable<INotification> notifications)
		{
			this.AddRange(notifications);
		}

		/// <summary>
		/// Get a zero-length collection.
		/// </summary>
		public static IEnumerable<INotification> Empty
		{
			get
			{
				return Enumerable.Empty<INotification>();
			}
		}

		/// <summary>
		/// Either cast the given IEnumerable to a NotificationCollection if it is of that type,
		/// or create a new NotificationCollection populated with the items in the IEnumerable.
		/// </summary>
		public static NotificationCollection Cast(IEnumerable<INotification> notifications)
		{
			NotificationCollection result = notifications as NotificationCollection;
			return result ?? new NotificationCollection(notifications);
		}

		public virtual NotificationCollection Clone()
		{
			NotificationCollection result = (NotificationCollection)Activator.CreateInstance(GetType());
			result.AddRange(this);
			return result;
		}

		public void Remove(INotificationType type, string message, bool containing = false)
		{
			var indexToRemove = new List<int>();
			for (int i = 0; i < Count; i++)
			{
				INotification notification = this[i];

				bool messageMatches(string notificationMessage) => containing ? notificationMessage.Contains(message) : notificationMessage == message;

				if (notification != null && notification.Type == type && messageMatches(notification.Message))
				{
					indexToRemove.Add(i);
				}
			}

			indexToRemove.Reverse();
			indexToRemove.ForEach(RemoveAt);
		}

		#region Changed event

		public event ListChangedEventHandler ListChanged;

		protected void OnListChanged(ListChangedEventArgs e)
		{
			if (ListChanged != null)
			{
				ListChanged(this, e);
			}
		}

		#endregion

		#region Immutibility

		protected override void ClearItems()
		{
			base.ClearItems();
			containingNotificationTypes = null;
			highestSeverityNotificationType = null;
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		protected override void InsertItem(int index, INotification item)
		{
			base.InsertItem(index, item);
			UpdateContainedNotificationsAndHighestSeverity(item);
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
		}

		protected override void RemoveItem(int index)
		{
			base.RemoveItem(index);
			containingNotificationTypes = null;
			highestSeverityNotificationType = null;
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
		}

		protected override void SetItem(int index, INotification item)
		{
			base.SetItem(index, item);
			containingNotificationTypes = null;
			highestSeverityNotificationType = null;
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
		}

		#endregion

		#region INotifications Members

		public bool HasNotifications(INotificationType notificationType)
		{
			return ContainingNotificationTypes.Contains(notificationType);
		}

		public INotificationType GetHighestSeverityNotificationType()
		{
			_ = ContainingNotificationTypes;
			return highestSeverityNotificationType;
		}

		#endregion

		#region Implementation

		List<INotificationType> ContainingNotificationTypes
		{
			get
			{
				if (containingNotificationTypes == null)
				{
					containingNotificationTypes = new List<INotificationType>(1);
					foreach (var notification in this)
					{
						UpdateContainedNotificationsAndHighestSeverity(notification);
					}
				}
				return containingNotificationTypes;
			}
		}
		List<INotificationType> containingNotificationTypes;
		INotificationType highestSeverityNotificationType;

		void UpdateContainedNotificationsAndHighestSeverity(INotification notification)
		{
			var notificationType = notification?.Type;
			if (notificationType != null && !ContainingNotificationTypes.Contains(notificationType))
			{
				ContainingNotificationTypes.Add(notificationType);

				if (highestSeverityNotificationType == null || notificationType.Severity < highestSeverityNotificationType.Severity)
				{
					highestSeverityNotificationType = notificationType;
				}
			}
		}

		#endregion
	}
}
