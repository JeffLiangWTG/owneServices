using System;
using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class DecimalStringComparer : PropertyComparer
	{
		public DecimalStringComparer(Type businessObjectType, string propertyName, ListSortDirection direction)
			: base(businessObjectType, propertyName, direction)
		{
		}

		public DecimalStringComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
			: base(propertyDescriptor, direction)
		{
		}

		protected internal override IComparable GetPropertyValueFromObject(BusinessObject businessObject)
		{
			var stringValue = GetValueForConversion(base.GetPropertyValueFromObject(businessObject));
			if (!string.IsNullOrEmpty(stringValue))
			{
				return ZDataType.ObjectToZType(typeof(ZDecimal), stringValue);
			}

			return ZDecimal.Zero;
		}

		protected virtual ZString GetValueForConversion(object value)
		{
			return value is ZString ? (ZString)value : ZString.Empty;
		}
	}
}
