using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// A PropertyDescriptor that accesses a method with the signature:
	/// void ValidateProperty(INotifications notifications)
	/// and returns the notification collection.
	/// </summary>
	internal class NotificationsPropertyDescriptorWithAttributedNotificationLogic : KPropertyDescriptor
	{
		public NotificationsPropertyDescriptorWithAttributedNotificationLogic(PropertyDescriptorCollectionWithMetaData collection, PropertyDescriptor propertyBeingValidated, string metaDataPropertyName)
			: base(collection, metaDataPropertyName, new Attribute[] { new BrowsableAttribute(false) })
		{
			Argument.NotNull(collection, nameof(collection));
			Argument.NotNull(propertyBeingValidated, nameof(propertyBeingValidated));
			this.metaDataPropertyName = metaDataPropertyName;
			this.PropertyBeingValidated = propertyBeingValidated;
		}

		public NotificationsPropertyDescriptorWithAttributedNotificationLogic(PropertyDescriptorCollectionWithMetaData collection, PropertyDescriptor propertyBeingValidated, string metaDataPropertyName, PropertyDescriptor inner)
			: base(collection, inner)
		{
			Argument.NotNull(inner, nameof(inner));
			Argument.NotNull(propertyBeingValidated, nameof(propertyBeingValidated));
			this.metaDataPropertyName = metaDataPropertyName;
			this.PropertyBeingValidated = propertyBeingValidated;
		}

		public static bool RequiresThisMetaDataPropertyDescriptor(PropertyDescriptor propertyBeingValidated)
		{
			Argument.NotNull(propertyBeingValidated, nameof(propertyBeingValidated));
			return GetNotificationProvidingAttribute(propertyBeingValidated).GetEnumerator().MoveNext();
		}

		/// <summary>
		/// Get the property being validated.
		/// </summary>
		public PropertyDescriptor PropertyBeingValidated { get; private set; }

		public override string Name
		{
			get { return metaDataPropertyName; }
		}
		readonly string metaDataPropertyName;

		protected override object GetValueCore(object component)
		{
			return GetNotifications(component);
		}

		protected override void SetValueCore(object component, object value)
		{
		}

		IEnumerable<INotification> GetNotifications(object component)
		{
			if (PropertyBeingValidated != null)
			{
				var propertyNotifications = (IEnumerable<INotification>)BaseGetValueCore(component);
				if (propertyNotifications != null)
				{
					foreach (var notification in propertyNotifications)
					{
						yield return notification;
					}
				}
			}
			foreach (INotificationProvidingAttribute attribute in NotificationAttributes)
			{
				foreach (var notification in attribute.Validate(component, PropertyBeingValidated))
				{
					yield return notification;
				}
			}
		}

		object BaseGetValueCore(object component)
		{
			return base.GetValueCore(component);
		}

		public override Type PropertyType
		{
			get { return typeof(IEnumerable<INotification>); }
		}

		public override Type ComponentType
		{
			get { return PropertyBeingValidated.ComponentType; }
		}

		public override bool CanResetValue(object component)
		{
			return false;
		}

		public override bool IsReadOnly
		{
			get { return true; }
		}

		public override void ResetValue(object component)
		{
			throw new NotSupportedException();
		}

		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}

		Attribute[] NotificationAttributes
		{
			get
			{
				if (notificationAttributes == null)
				{
					notificationAttributes = new List<Attribute>(GetNotificationProvidingAttribute(PropertyBeingValidated)).ToArray();
				}
				return notificationAttributes;
			}
		}
		Attribute[] notificationAttributes;

		static IEnumerable<Attribute> GetNotificationProvidingAttribute(PropertyDescriptor propertyBeingValidated)
		{
			Argument.NotNull(propertyBeingValidated, nameof(propertyBeingValidated));
			return propertyBeingValidated.GetAttributesAllowMultiple(typeof(INotificationProvidingAttribute));
		}
	}
}
