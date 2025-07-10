using System;
using System.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class TimeComparer : PropertyComparer
	{
		public TimeComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction) : base(propertyDescriptor, direction)
		{
		}

		internal protected override IComparable GetPropertyValueFromObject(BusinessObject businessObject)
		{
			var result = base.GetPropertyValueFromObject(businessObject);
			if (result is ZDateTime time)
			{
				result = time.GetMinutesFromDateTimeSpan();
			}
			return result;
		}
	}
}
