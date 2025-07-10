using System.Collections;
using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.ComponentModel.NotificationExtensions
{
	public static class NotificationsExtensions
	{
		#region Mandatory Validation

		/// <summary>
		/// Add an error notification if the given property on the given component is empty.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
		public static void ErrorIfEmpty(this INotifications notifications, object component, PropertyDescriptor property)
		{
			Argument.NotNull(notifications, nameof(notifications));
			Argument.NotNull(property, nameof(property));
			if (MetaData.IsEmptyValue(component, property))
			{
				var description = GetPropertyDescription(component, property) ?? property.Name;
				var message = "You must enter a value for " + description;
				notifications.AddError(message);
			}
		}

		#endregion

		#region List Validation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
		public static void ErrorIfNotInList(this INotifications notifications, object component, PropertyDescriptor propertyToValidate, PropertyDescriptor listValueProperty, IEnumerable list)
		{
			Argument.NotNull(notifications, nameof(notifications));
			Argument.NotNull(propertyToValidate, nameof(propertyToValidate));
			Argument.NotNull(list, nameof(list));
			if (!MetaData.IsEmptyValue(component, propertyToValidate) && !ExistsInList(component, propertyToValidate, listValueProperty, list))
			{
				var message = "Enter a valid selection for " + GetPropertyDescription(component, propertyToValidate);
				notifications.AddError(message);
			}
		}

		static bool ExistsInList(object component, PropertyDescriptor propertyToValidate, PropertyDescriptor listValueProperty, IEnumerable list)
		{
			Argument.NotNull(propertyToValidate, nameof(propertyToValidate)); // Suggested By ReviewBot 
			Argument.NotNull(list, nameof(list));
			var componentValue = propertyToValidate.GetValue(component);
			var bindingList = list as IBindingList;
			if (bindingList != null && bindingList.SupportsSearching && listValueProperty != null)
			{
				if (bindingList.Find(listValueProperty, componentValue) != -1)
				{
					return true;
				}
			}
			else
			{
				foreach (var item in list)
				{
					var itemValue = (listValueProperty == null) ? item : listValueProperty.GetValue(item);
					if (object.Equals(itemValue, componentValue))
					{
						return true;
					}
				}
			}
			return false;
		}

		#endregion

		#region Implementation

		static string GetPropertyDescription(object component, PropertyDescriptor property)
		{
			return MetaData.GetDescriptionOfMaxLength(component, property, 25);
		}

		#endregion
	}
}
