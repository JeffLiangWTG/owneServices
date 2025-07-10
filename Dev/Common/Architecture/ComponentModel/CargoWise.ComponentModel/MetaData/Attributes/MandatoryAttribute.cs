using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel.NotificationExtensions;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Indicates that an error notification should automatically be added to the
	/// MetaDataTypes.Notifications meta-data type if no value is entered for the property. Apply this to
	/// properties that are known to always require a value before they are saved.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class MandatoryAttribute : Attribute, INotificationProvidingAttribute
	{
		public MandatoryAttribute()
			: this(true)
		{
		}

		public MandatoryAttribute(bool isMandatory)
		{
			this.isMandatory = isMandatory;
		}

		public bool IsMandatory
		{
			get { return isMandatory; }
		}
		readonly bool isMandatory;

		#region INotificationProvidingAttribute Members

		IEnumerable<INotification> INotificationProvidingAttribute.Validate(object component, PropertyDescriptor propertyBeingValidated)
		{
			var notifications = new NotificationCollection();
			if (IsMandatory)
			{
				notifications.ErrorIfEmpty(component, propertyBeingValidated);
			}
			return notifications;
		}

		bool INotificationProvidingAttribute.ProvidesNotifications(PropertyDescriptor propertyBeingValidated)
		{
			return IsMandatory;
		}

		#endregion
	}
}
