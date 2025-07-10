using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Maintains a caches of element and property level notifications for a list.
	/// </summary>
	public sealed class ListNotificationsCache : IDisposable
	{
		public ListNotificationsCache(IBindingList list, PropertyDescriptor[] properties, Control boundControl, INotificationRenderContext context)
		{
			this.List = list;
			this.Properties = properties;
			this.BoundControl = boundControl;
			this.NotificationRenderContext = context;

			elementNotifications = new List<ListElementNotificationsCache>();
			List.ListChanged += new ListChangedEventHandler(OnListChanged);
			OnListChanged(List, new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		public IBindingList List { get; private set; }
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
		public PropertyDescriptor[] Properties { get; private set; }
		public Control BoundControl { get; private set; }
		public INotificationRenderContext NotificationRenderContext { get; private set; }

		/// <summary>
		/// Get or set whether to include notifications for properties not included in the Properties
		/// collection as element notifications.
		/// </summary>
		public bool IncludeOtherPropertiesInElementNotifications
		{
			get { return includeOtherPropertiesInElementNotifications; }
			set
			{
				if (includeOtherPropertiesInElementNotifications != value)
				{
					includeOtherPropertiesInElementNotifications = value;
					elementNotifications = null;
				}
			}
		}
		bool includeOtherPropertiesInElementNotifications;

		/// <summary>
		/// When the ListChanged event on the List property is fired.
		/// </summary>
		public event ListChangedEventHandler ListChanged;

		/// <summary>
		/// Get the notification cache for the element at the specified index.
		/// </summary>
		public ListElementNotificationsCache GetElementNotifications(int index)
		{
			if (elementNotifications == null)
			{
				elementNotifications = new List<ListElementNotificationsCache>();
			}
			if (elementNotifications.Count != List.Count)
			{
				elementNotifications.Clear();
				for (int i = elementNotifications.Count; i < List.Count; i++)
				{
					elementNotifications.Add(new ListElementNotificationsCache(this, List[i]));
				}
			}

			ListElementNotificationsCache result = elementNotifications[index];
			if (result == null || !object.Equals(List[index], result.Element))
			{
				result = new ListElementNotificationsCache(this, List[index]);
				if (List.Count == elementNotifications.Count)
				{
					elementNotifications[index] = result;
				}
			}
			return result;
		}

		/// <summary>
		/// Get the highest priority notification type that should be rendered for the whole list. For example,
		/// Error is higher than Warning. NotificationRenderContext is used to determine which list elements and
		/// properties should and should be used in this determination.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public INotificationType GetHighestPriorityNotificationTypeToRender()
		{
			bool hasNotifications = false;
			INotificationType highestPriorityNotificationType = null;
			for (int i = 0; i < List.Count; i++)
			{
				ListElementNotificationsCache rowElementNotifications = GetElementNotifications(i);
				NotificationCollection combinedNotifications = rowElementNotifications.GetCombinedNotificationsToRender();
				if (combinedNotifications != null)
				{
					INotificationType proposeNotificationType =
						combinedNotifications.GetHighestSeverityNotificationType();
					if (highestPriorityNotificationType == null ||
						proposeNotificationType.Severity < highestPriorityNotificationType.Severity)
					{
						hasNotifications = true;
						highestPriorityNotificationType = proposeNotificationType;
					}
				}
			}
			return hasNotifications ? highestPriorityNotificationType : null;
		}

		#region NotificationPropertyDescriptors / OtherNotificationPropertyDescriptors

		internal PropertyDescriptor[] NotificationPropertyDescriptors
		{
			get
			{
				if (this.notificationPropertyDescriptors == null)
				{
					List<PropertyDescriptor> list = new List<PropertyDescriptor>();
					foreach (PropertyDescriptor property in Properties)
					{
						PropertyDescriptor notificationProperty = (property == null) ? null : MetaData.GetMetaDataProperty(property.ComponentType, property, MetaDataTypes.Notifications);
						list.Add(notificationProperty);
					}
					this.notificationPropertyDescriptors = list.ToArray();
				}
				return this.notificationPropertyDescriptors;
			}
		}
		PropertyDescriptor[] notificationPropertyDescriptors;

		internal PropertyDescriptor[] OtherNotificationPropertyDescriptors
		{
			get
			{
				if (otherNotificationPropertyDescriptors == null)
				{
					IList<string> propertyNames = this.PropertyNames;
					Type elementType = ListUtil.GetListElementType(List);

					List<PropertyDescriptor> list = new List<PropertyDescriptor>();
					foreach (KPropertyDescriptor property in GetPrivateProperties())
					{
						if (!property.Name.Contains("+") && !propertyNames.Contains(property.Name))
						{
							KPropertyDescriptor notificationsProperty = MetaData.GetMetaDataProperty(elementType, property, MetaDataTypes.Notifications);
							if (notificationsProperty != null)
							{
								list.Add(notificationsProperty);
							}
						}
					}
					otherNotificationPropertyDescriptors = list.ToArray();
				}
				return otherNotificationPropertyDescriptors;
			}
		}
		PropertyDescriptor[] otherNotificationPropertyDescriptors;

		PropertyDescriptorCollection GetPrivateProperties()
		{
			PropertyDescriptorCollection properties = ((ITypedList)List).GetItemProperties(null);
			KPropertyDescriptorCollection kproperties = properties as KPropertyDescriptorCollection;
			return kproperties == null ? properties : kproperties.AllProperties;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{ List.ListChanged -= OnListChanged; }

		#endregion

		#region Implementation

		List<ListElementNotificationsCache> elementNotifications;

		string[] PropertyNames
		{
			get
			{
				if (propertyNames == null)
				{
					List<string> list = new List<string>();
					foreach (PropertyDescriptor prop in Properties)
					{
						list.Add(prop == null ? "" : prop.Name);
					}
					propertyNames = list.ToArray();
				}
				return propertyNames;
			}
		}
		string[] propertyNames;

		internal void OnListChanged(object sender, ListChangedEventArgs e)
		{
			if (elementNotifications != null)
			{
				switch (e.ListChangedType)
				{
					case ListChangedType.Reset:
						elementNotifications = null;
						break;
					case ListChangedType.ItemAdded:
						ListElementNotificationsCache added_element_notifications = new ListElementNotificationsCache(this, List[e.NewIndex]);
						if (List.Count == elementNotifications.Count)
						{
							elementNotifications[e.NewIndex] = added_element_notifications;
						}
						else
						{
							elementNotifications.Insert(e.NewIndex, added_element_notifications);
						}
						break;
					case ListChangedType.ItemChanged:
						elementNotifications[e.NewIndex] = new ListElementNotificationsCache(this, List[e.NewIndex]);
						break;
					case ListChangedType.ItemDeleted:
						if (e.NewIndex < elementNotifications.Count)
						{
							ListElementNotificationsCache deleted_row = elementNotifications[e.NewIndex];
							if (NotificationRenderContext != null)
							{
								NotificationRenderContext.NotifyDataItemDeleted(BoundControl, deleted_row.Element);
							}
							elementNotifications.RemoveAt(e.NewIndex);
						}
						break;
					case ListChangedType.ItemMoved:
						ListElementNotificationsCache tmp = elementNotifications[e.NewIndex];
						elementNotifications[e.NewIndex] = elementNotifications[e.OldIndex];
						elementNotifications[e.OldIndex] = tmp;
						break;
				}
			}
			switch (e.ListChangedType)
			{
				case ListChangedType.PropertyDescriptorAdded:
				case ListChangedType.PropertyDescriptorChanged:
				case ListChangedType.PropertyDescriptorDeleted:
					this.notificationPropertyDescriptors = null; // invalidate this cache
					break;
			}
			if (ListChanged != null)
			{
				ListChanged(this, e);
			}
		}

		#endregion
	}
}
