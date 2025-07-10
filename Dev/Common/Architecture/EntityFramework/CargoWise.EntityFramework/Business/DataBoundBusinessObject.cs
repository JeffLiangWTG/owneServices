using System;
using CargoWise.Common;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	public sealed class DataBoundBusinessObject : IDataBoundBusinessObject
	{
		public DataBoundBusinessObject(BusinessObject businessObject)
		{
			this.businessObject = Argument.NotNull(businessObject, nameof(businessObject));
		}
		readonly BusinessObject businessObject;

		#region IBusinessObjectValuesProvider

		bool IDataBoundBusinessObject.HasProperty(string propertyName) => HasProperty(propertyName);

		bool IDataBoundBusinessObject.TryGetValue<TValueType>(string propertyName, out TValueType value) => TryGetValue(propertyName, out value);

		#endregion

		bool HasProperty(string propertyName) => Properties[propertyName] != null;

		bool TryGetValue<TValueType>(string propertyName, out TValueType value)
		{
			value = default(TValueType);

			if (!HasProperty(propertyName))
			{
				return false;
			}

			var propertyInfo = Properties[propertyName];
			var propValue = propertyInfo.GetValue(businessObject);
			try
			{
				value = (TValueType)propValue;
				return true;
			}
			catch (InvalidCastException)
			{
				return false;
			}
		}

		KPropertyDescriptorCollection Properties => properties ?? (properties = GetProperties());
		KPropertyDescriptorCollection properties;

		KPropertyDescriptorCollection GetProperties() => businessObject.GetProperties();
	}
}
