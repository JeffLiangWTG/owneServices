using System.Collections.Generic;
using System.ComponentModel;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Implemented on an attribute class that provides additional validation notifications
	/// for a property.
	/// </summary>\
	public interface INotificationProvidingAttribute
	{
		IEnumerable<INotification> Validate(object component, PropertyDescriptor propertyBeingValidated);
		bool ProvidesNotifications(PropertyDescriptor propertyBeingValidated);
	}
}
