using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Caches element and property level notifications for a list.
	/// </summary>
	public class ListElementNotificationsCache
	{
		public ListElementNotificationsCache(ListNotificationsCache listNotifications, object element)
		{
			this.ListNotifications = listNotifications;
			this.Element = element;
		}

		/// <summary>
		/// Get the element notifications are taken for this instance.
		/// </summary>
		public object Element { get; private set; }

		/// <summary>
		/// Refresh the cached notifications.
		/// </summary>
		public void Refresh()
		{
			this.elementPropertyNotifications = null;
			this.elementNotifications = null;
			this.elementNotificationsPopulated = false;
			this.combinedNotifications = null;
			this.combinedNotificationsPopulated = false;
		}

		#region GetCombinedNotificationsToRender

		/// <summary>
		/// Gets the combined element and property level notifications that should be rendered for this element.
		/// NotificationRenderContext is used to determine which notifications should be rendered (the controls
		/// that have been given focus in the past may be used in this determination).
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public NotificationCollection GetCombinedNotificationsToRender()
		{
			NotificationCollection result = null;
			if (NotificationRenderContext == null || NotificationRenderContext.ShouldAlwaysRenderAllNotifications())
			{
				result = CombinedNotifications;
			}
			else
			{
				if (NotificationRenderContext.ShouldRenderAnyNotificationsFrom(ListNotifications.BoundControl, Element))
				{
					if (NotificationRenderContext.ShouldRenderNotification(ListNotifications.BoundControl, Element, ""))
					{
						if (result == null)
						{
							result = new NotificationCollection();
						}
						result.AddRange(ElementNotifications);
					}
					NotificationCollection[] propertyNotifications = ElementPropertyNotifications;
					for (int i = 0; i < ListNotifications.Properties.Length; i++)
					{
						PropertyDescriptor property = ListNotifications.Properties[i];
						if (property != null &&
							propertyNotifications[i] != null &&
							NotificationRenderContext.ShouldRenderNotification(ListNotifications.BoundControl, Element, property.Name))
						{
							if (result == null)
							{
								result = new NotificationCollection();
							}
							result.AddRange(propertyNotifications[i]);
						}
					}
				}
			}
			return (result == null || result.Count == 0) ? null : result;
		}

		#endregion

		#region ElementNotifications

		/// <summary>
		/// If INotificationSource is implemented on the element, the notifications from that
		/// are cached and returned.
		/// </summary>
		public NotificationCollection ElementNotifications
		{
			get
			{
				if (!elementNotificationsPopulated)
				{
					INotificationSource source = Element as INotificationSource;
					NotificationCollection newElementNotifications = new NotificationCollection();
					if (source != null)
					{
						newElementNotifications.AddRange(source.Notifications);
					}
					if (ListNotifications.IncludeOtherPropertiesInElementNotifications)
					{
						foreach (PropertyDescriptor property in ListNotifications.OtherNotificationPropertyDescriptors)
						{
							IEnumerable<INotification> propertyNotifications = (IEnumerable<INotification>)property.GetValue(Element);
							newElementNotifications.AddRange(propertyNotifications);
						}
					}
					if (newElementNotifications != null && newElementNotifications.Count == 0)
					{
						newElementNotifications = null;
					}
					elementNotificationsPopulated = true;

					// NOTE: set last because Refresh() may be called during this method which sets elementNotifications to null
					elementNotifications = newElementNotifications;
				}
				return elementNotifications;
			}
		}
		NotificationCollection elementNotifications;
		bool elementNotificationsPopulated;

		#endregion

		#region ElementPropertyNotifications

		/// <summary>
		/// Get the notifications on the element for the individual properties. The properties are returned in
		/// the order of the PropertyDescriptor accessors passed into the constructor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public NotificationCollection[] ElementPropertyNotifications
		{
			get
			{
				if (elementPropertyNotifications == null)
				{
					elementPropertyNotifications = new NotificationCollection[NotificationAccessorProperties.Length];
					for (int i = 0; i < NotificationAccessorProperties.Length; i++)
					{
						if (NotificationAccessorProperties[i] != null)
						{
							elementPropertyNotifications[i] = NotificationCollection.Cast((IEnumerable<INotification>)NotificationAccessorProperties[i].GetValue(this.Element));
						}
					}
				}
				return elementPropertyNotifications;
			}
		}
		NotificationCollection[] elementPropertyNotifications;

		#endregion

		#region CombinedNotifications

		/// <summary>
		/// Combine the element and property level notifications to make one big collection. Null is
		/// returned if there are zero notifications after combining.
		/// </summary>
		public NotificationCollection CombinedNotifications
		{
			get
			{
				if (!this.combinedNotificationsPopulated)
				{
					this.combinedNotifications = new NotificationCollection();
					if (ElementNotifications != null)
					{
						this.combinedNotifications.AddRange(ElementNotifications);
					}
					foreach (NotificationCollection next in ElementPropertyNotifications)
					{
						if (next != null)
						{
							this.combinedNotifications.AddRange(next);
						}
					}
					if (this.combinedNotifications.Count == 0)
					{
						this.combinedNotifications = null;
					}
					this.combinedNotificationsPopulated = true;
				}
				return this.combinedNotifications;
			}
		}
		NotificationCollection combinedNotifications;
		bool combinedNotificationsPopulated;

		#endregion

		#region Implementation

		internal readonly ListNotificationsCache ListNotifications;

		PropertyDescriptor[] NotificationAccessorProperties
		{ get { return ListNotifications.NotificationPropertyDescriptors; } }

		INotificationRenderContext NotificationRenderContext
		{ get { return ListNotifications.NotificationRenderContext; } }

		#endregion
	}
}
