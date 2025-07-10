using System;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class NotDefaultingPropertyValueAttribute : Attribute
	{
		public NotDefaultingPropertyValueAttribute(string filterValue)
		{
			FilterValue = filterValue;
		}

		public NotDefaultingPropertyValueAttribute()
			: this("")
		{ }

		public string FilterValue
		{
			get; private set;
		}

		public bool MatchFilterValue(string propertyValue)
		{
			return (string.IsNullOrEmpty(FilterValue)) || (propertyValue == FilterValue);
		}
	}
}
