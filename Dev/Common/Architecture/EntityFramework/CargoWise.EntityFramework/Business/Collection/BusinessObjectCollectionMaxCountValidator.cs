using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	public sealed class BusinessObjectCollectionMaxCountValidator
	{
		public BusinessObjectCollectionMaxCountValidator(IBindingList collection)
		{
			this.collection = collection;
		}

		public Func<int> GetMaxCountReduction { get; set; } = () => 0;

		public int MaxCount
		{
			get => absoluteMaxCount == -1 ? absoluteMaxCount : Math.Max(absoluteMaxCount - GetMaxCountReduction(), 0);
			set
			{
				if (absoluteMaxCount != value)
				{
					if (absoluteMaxCount != -1)
					{
						collection.ListChanged -= new ListChangedEventHandler(Collection_ListChanged);
						if (ActiveCollection != null)
						{
							ActiveCollection.Rebuilt -= new EventHandler(ActiveCollection_Rebuilt);
						}
					}
					RemoveCurrentNotifications();

					absoluteMaxCount = value;

					if (absoluteMaxCount != -1)
					{
						collection.ListChanged += new ListChangedEventHandler(Collection_ListChanged);
						if (ActiveCollection != null)
						{
							ActiveCollection.Rebuilt += new EventHandler(ActiveCollection_Rebuilt);
						}
					}
					Refresh();
				}
			}
		}

		int absoluteMaxCount = -1;

		int HalfMaxCount { get { return MaxCount / 2; } }

		public INotification Notification
		{
			get { return notification; }
			set
			{
				RemoveCurrentNotifications();
				notification = new DefaultNotification(value.Message, value.Type);
				Refresh();
			}
		}

		INotification notification;

		public bool WarnAtHalfway { get; set; }

		bool inRefresh;

		public void Refresh()
		{
			if (!inRefresh)
			{
				try
				{
					inRefresh = true;
					BusinessObjectsToClear.Clear();

					var businessObjectsThatNeedToRemoveNotifications = new HashSet<BusinessObject>();
					var businessObjectsThatNeedToAddWarningNotifications = new List<BusinessObject>();
					var businessObjectsThatNeedToAddErrorNotifications = new List<BusinessObject>();

					if (absoluteMaxCount != -1 && (ActiveCollection == null || !ActiveCollection.IsRebuildPending))
					{
						for (int i = 0; i < collection.Count; i++)
						{
							BusinessObject businessObject = (BusinessObject)collection[i];

							foreach (var notification in businessObject.RowNotifications)
							{
								if (notification is DefaultNotification)
								{
									businessObjectsThatNeedToRemoveNotifications.Add(businessObject);
									break;
								}
							}

							if (i >= MaxCount)
							{
								businessObjectsThatNeedToAddErrorNotifications.Add(businessObject);
							}
							else if (WarnAtHalfway && i >= HalfMaxCount)
							{
								businessObjectsThatNeedToAddWarningNotifications.Add(businessObject);
							}
						}

						RefreshRowNotifications(businessObjectsThatNeedToRemoveNotifications, businessObjectsThatNeedToAddWarningNotifications, ComponentModel.NotificationType.Warning);
						RefreshRowNotifications(businessObjectsThatNeedToRemoveNotifications, businessObjectsThatNeedToAddErrorNotifications, ComponentModel.NotificationType.Error);

						DoRemoveCurrentNotifications(businessObjectsThatNeedToRemoveNotifications.ToList());
					}
				}
				finally
				{
					inRefresh = false;
				}
			}
		}

		void RefreshRowNotifications(HashSet<BusinessObject> listToRemove, List<BusinessObject> listToAdd, INotificationType type)
		{
			foreach (var bizo in listToAdd)
			{
				if (!listToRemove.Contains(bizo))
				{
					bizo.AddRowNotification(GetNotificationFallbackToDefault(bizo));
					BusinessObjectsToClear.Add(bizo);
				}
				else
				{
					foreach (var notification in bizo.RowNotifications)
					{
						if (notification is DefaultNotification)
						{
							BusinessObjectsToClear.Add(bizo);
							listToRemove.Remove(bizo);

							if (notification.Type != type)
							{
								bizo.RemoveRowNotification(notification);
								if (type == ComponentModel.NotificationType.Warning)
								{
									bizo.AddRowNotification(GetHalfwayWarningNotification(bizo));
								}

								if (type == ComponentModel.NotificationType.Error)
								{
									bizo.AddRowNotification(GetNotificationFallbackToDefault(bizo));
								}
							}
							break;
						}
					}
				}
			}
		}

		#region Implementation

		readonly IBindingList collection;

		INotification GetNotificationFallbackToDefault(BusinessObject businessObject)
		{
			return Notification ?? GetDefaultNotification(businessObject);
		}

		INotification GetDefaultNotification(BusinessObject businessObject)
		{
			var maximumCount = MaxCount;
			var hrn = (maximumCount != 1 ? businessObject.HumanReadableNameForPlural : businessObject.HumanReadableName);
			return new DefaultNotification(Res.GetString("a8c2df3c-0db0-4850-8632-7123a3081993", "You are only allowed a maximum of {0} {1} here.", maximumCount, hrn));
		}

		INotification GetHalfwayWarningNotification(BusinessObject businessObject)
		{
			var baseNotification = GetNotificationFallbackToDefault(businessObject);
			return baseNotification != null ? new DefaultNotification(baseNotification.Message, NotificationType.Warning) : null;
		}

		[Serializable]
		class DefaultNotification : Notification
		{
			public DefaultNotification(string message, INotificationType notificationType = null)
				: base(notificationType ?? NotificationType.Error, message)
			{
			}
		}

		IActiveBusinessObjectCollection ActiveCollection
		{
			get { return collection as IActiveBusinessObjectCollection; }
		}

		HashSet<BusinessObject> BusinessObjectsToClear
		{
			get { return businessObjectsToClear ?? (businessObjectsToClear = new HashSet<BusinessObject>()); }
		}

		HashSet<BusinessObject> businessObjectsToClear;

		void Collection_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (ActiveCollection == null || !ActiveCollection.IsRebuildPending)
			{
				switch (e.ListChangedType)
				{
					case ListChangedType.ItemAdded:
						Func<BusinessObject, INotification> getNotificationToAdd = null;

						if (e.NewIndex >= MaxCount)
						{
							getNotificationToAdd = GetNotificationFallbackToDefault;
						}
						else if (WarnAtHalfway && e.NewIndex >= HalfMaxCount)
						{
							getNotificationToAdd = GetHalfwayWarningNotification;
						}

						if (getNotificationToAdd != null)
						{
							var businessObject = (BusinessObject)collection[e.NewIndex];
							businessObject.AddRowNotification(getNotificationToAdd(businessObject));
							if (!BusinessObjectsToClear.Contains(businessObject))
							{
								BusinessObjectsToClear.Add(businessObject);
							}
						}
						break;
					case ListChangedType.ItemChanged:
					case ListChangedType.PropertyDescriptorAdded:
					case ListChangedType.PropertyDescriptorChanged:
					case ListChangedType.PropertyDescriptorDeleted:
						break;
					default:
						Refresh();
						break;
				}
			}
		}

		void ActiveCollection_Rebuilt(object sender, EventArgs e)
		{
			Refresh();
		}

		void RemoveCurrentNotifications()
		{
			var localBusinessObjectsToClear = BusinessObjectsToClear.ToList();
			BusinessObjectsToClear.Clear();
			DoRemoveCurrentNotifications(localBusinessObjectsToClear);
		}

		void DoRemoveCurrentNotifications(List<BusinessObject> businessObjectsToClear)
		{
			foreach (var businessObject in businessObjectsToClear)
			{
				foreach (INotification notification in businessObject.RowNotifications)
				{
					if (notification is DefaultNotification)
					{
						businessObject.RemoveRowNotification(notification);
						break;
					}
				}
			}
		}

		#endregion
	}
}
