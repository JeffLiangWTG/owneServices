using System;
using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class AlphanumericStringPropertyComparer : PropertyComparer
	{
		public AlphanumericStringPropertyComparer(Type businessObjectType, string propertyName, ListSortDirection direction)
			: base(businessObjectType, propertyName, direction)
		{
		}

		public AlphanumericStringPropertyComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
			: base(propertyDescriptor, direction)
		{
		}

		public override int Compare(BusinessObject x, BusinessObject y)
		{
			ZString firstString;
			ZString secondString;
			var xString = (ZString)GetPropertyValueFromObject(x);
			var yString = (ZString)GetPropertyValueFromObject(y);

			if (Direction == ListSortDirection.Ascending)
			{
				firstString = xString;
				secondString = yString;
			}
			else
			{
				firstString = yString;
				secondString = xString;
			}

			return new AlphanumericStringComparer().Compare(firstString, secondString);
		}

		protected internal override IComparable GetPropertyValueFromObject(BusinessObject businessObject)
		{
			return GetValueForConversion(base.GetPropertyValueFromObject(businessObject));
		}

		protected virtual ZString GetValueForConversion(object value)
		{
			return value is ZString ? (ZString)value : ZString.Empty;
		}
	}
}
