using System;
using System.ComponentModel;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Core
{
	public class ZMultilingualTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string)
				|| sourceType == typeof(ZString)
				|| sourceType == typeof(DBNull)
				|| sourceType == typeof(char)
				|| typeof(ZMultilingual).IsAssignableFrom(sourceType)
				|| base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is ZMultilingual)
			{
				return value;
			}
			else if (value == DBNull.Value || value == null)
			{
				return (NoResString)((string)null);
			}
			else if (value is string)
			{
				return (NoResString)((string)value);
			}
			else if (value is ZString)
			{
				return (NoResString)((ZString)value);
			}
			else if (value is char)
			{
				return (NoResString)((char)value).ToString();
			}

			throw new NotSupportedException();
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string)
				|| destinationType == typeof(ZString)
				|| typeof(ZMultilingual).IsAssignableFrom(destinationType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value == null || destinationType == null)
			{
				return null;
			}

			if (destinationType == typeof(ZString))
			{
				return (ZString)value.ToString();
			}
			else if (destinationType == typeof(string))
			{
				return value.ToString();
			}
			else if (typeof(ZMultilingual).IsAssignableFrom(destinationType))
			{
				return value;
			}
			else
			{
				return base.ConvertTo(context, culture, value, destinationType);
			}
		}
	}
}
