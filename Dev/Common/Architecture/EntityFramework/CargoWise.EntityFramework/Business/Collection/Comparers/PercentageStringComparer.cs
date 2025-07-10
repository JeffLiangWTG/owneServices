using System;
using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class PercentageStringComparer : DecimalStringComparer
	{
		public PercentageStringComparer(Type businessObjectType, string propertyName, ListSortDirection direction)
			: base(businessObjectType, propertyName, direction)
		{
		}

		public PercentageStringComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
			: base(propertyDescriptor, direction)
		{
		}

		protected override ZString GetValueForConversion(object value)
		{
			var stringValue = base.GetValueForConversion(value);
			return stringValue.EndsWith("%")
				? stringValue.TrimEndIncludingWhiteSpace('%')
				: stringValue;
		}
	}
}
