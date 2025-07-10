using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	class NotificationsCacheTestCase : TestCase
	{
		#region Test Classes

		public class TestEntityCollection : ComponentModel.Testing.KBindingList<TestEntity>
		{
		}

		public class TestEntity : ComponentModel.Testing.KComponentWithPropertyChange, INotificationSource
		{
			#region Schema

			public static class Properties
			{
				public static readonly KPropertyDescriptor PropertyWithNotifications = (KPropertyDescriptor)PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(TestEntity))["PropertyWithNotifications"];
				public static readonly KPropertyDescriptor OtherPropertyWithNotifications = (KPropertyDescriptor)PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(TestEntity))["OtherPropertyWithNotifications"];
			}

			#endregion

			#region Property Validation

			[NotificationsMember("ValidatePropertyWithNotifications")]
			public string PropertyWithNotifications
			{
				get { return propertyWithNotifications; }
				set
				{
					if (propertyWithNotifications != value)
					{
						propertyWithNotifications = value;
						FirePropertyChanged(nameof(PropertyWithNotifications));
						FirePropertyChanged(nameof(ValidatePropertyWithNotifications));
					}
				}
			}
			string propertyWithNotifications;

			protected IEnumerable<INotification> ValidatePropertyWithNotifications
			{ get { return CreateNotifications(PropertyWithNotifications); } }

			[NotificationsMember("ValidateOtherPropertyWithNotifications")]
			public string OtherPropertyWithNotifications
			{
				get { return otherPropertyWithNotifications; }
				set
				{
					if (otherPropertyWithNotifications != value)
					{
						otherPropertyWithNotifications = value;
						FirePropertyChanged(nameof(OtherPropertyWithNotifications));
						FirePropertyChanged(nameof(ValidateOtherPropertyWithNotifications));
					}
				}
			}
			string otherPropertyWithNotifications;

			protected IEnumerable<INotification> ValidateOtherPropertyWithNotifications
			{ get { return CreateNotifications(OtherPropertyWithNotifications); } }

			#endregion

			#region Validation on Entity

			public string EntityNotifications
			{
				get { return entityNotifications; }
				set
				{
					if (entityNotifications != value)
					{
						entityNotifications = value;
						FirePropertyChanged(nameof(EntityNotifications));
						FirePropertyChanged(nameof(Notifications));
					}
				}
			}
			string entityNotifications;

			public IEnumerable<INotification> Notifications
			{ get { return CreateNotifications(EntityNotifications); } }

			#endregion

			#region Implementation

			IEnumerable<INotification> CreateNotifications(string value)
			{
				NotificationCollection notifications = new NotificationCollection();
				if (value == "error")
				{
					notifications.AddError("error");
				}
				if (value == "warning")
				{
					notifications.AddWarning("warning");
				}
				return notifications;
			}

			#endregion
		}

		#endregion

		#region Implementation

		internal ListNotificationsCache NotificationsCache
		{
			get
			{
				if (notificationsCache == null)
				{
					notificationsCache = new ListNotificationsCache(
						Collection,
						new PropertyDescriptor[] { TestEntity.Properties.PropertyWithNotifications },
						BoundControl,
						NotificationRenderContext);
				}
				return notificationsCache;
			}
		}
		ListNotificationsCache notificationsCache;

		internal TestEntityCollection Collection
		{
			get
			{
				if (collection == null)
				{
					collection = new TestEntityCollection();
				}
				return collection;
			}
		}
		TestEntityCollection collection;

		internal TestEntity Entity
		{
			get
			{
				if (Collection.Count == 0)
				{
					Collection.AddNew();
				}
				return Collection[0];
			}
		}

		internal Control BoundControl
		{
			get
			{
				if (boundControl == null)
				{
					boundControl = new Control();
				}
				return boundControl;
			}
		}
		Control boundControl;

		internal MockNotificationRenderContext NotificationRenderContext
		{
			get
			{
				if (notificationRenderContext == null)
				{
					notificationRenderContext = new MockNotificationRenderContext();
				}
				return notificationRenderContext;
			}
		}
		MockNotificationRenderContext notificationRenderContext;

		#endregion
	}
}
