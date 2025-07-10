using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace CargoWise.EntityFramework
{
	public class SQLComparisonOperatorTypeConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			bool canConvert;
			if (destinationType == typeof(InstanceDescriptor) || destinationType == typeof(String))
			{
				canConvert = true;
			}
			else
			{
				canConvert = base.CanConvertTo(context, destinationType);
			}
			return canConvert;
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			bool canConvert;

			if (sourceType == typeof(string))
			{
				canConvert = true;
			}
			else
			{
				canConvert = base.CanConvertFrom(context, sourceType);
			}
			return canConvert;
		}

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			PropertyInfo selectedItemInfo = typeof(SQLComparisonOperator).GetProperty((string)value);
			return selectedItemInfo.GetValue(null, null);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			object result = null;
			SQLComparisonOperator comparisonOperator = (SQLComparisonOperator)value;
			if (destinationType == typeof(InstanceDescriptor))
			{
				result = comparisonOperator.InstanceDescriptor;
			}
			else if (destinationType == typeof(string))
			{
				result = comparisonOperator.ToString();
			}
			else
			{
				result = base.ConvertTo(context, culture, value, destinationType);
			}
			return result;
		}
	}
}
