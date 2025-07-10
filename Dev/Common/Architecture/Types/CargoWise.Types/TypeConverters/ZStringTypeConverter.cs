using System;
using System.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZStringTypeConverter : StringConverter
	{
		public static readonly ZStringTypeConverter Instance = new ZStringTypeConverter();

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			try
			{
				if (value is string valueAsString)
				{
					return new ZString(valueAsString);
				}
				else if (value is ZString)
				{
					return value;
				}
				else if (value == DBNull.Value || value == null)
				{
					return ZString.Empty;
				}
				else if (value is char @char)
				{
					return new ZString(@char);
				}
				else if (value is ZGuid || value is Guid)
				{
					return new ZString(value.ToString());
				}
				else
				{
					return base.ConvertFrom(context, culture, value) ?? throw new FormatException("System class System.ComponentModel.TypeConverter returned null");
				}
			}
			catch (FormatException ex)
			{
				ex.RethrowForConvertFrom(value);
				throw;
			}
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string)
				|| sourceType == typeof(ZString)
				|| sourceType == typeof(DBNull)
				|| sourceType == typeof(char)
				|| sourceType == typeof(ZGuid)
				|| sourceType == typeof(Guid)
				|| base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (value == null)
			{
				return null;
			}

			try
			{
				if (destinationType == typeof(ZString))
				{
					return (ZString)value;
				}
				else
				{
					return base.ConvertTo(context, culture, value, destinationType);
				}
			}
			catch (FormatException ex)
			{
				ex.RethrowForConvertTo(value, destinationType);
				throw;
			}
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(ZString)
			|| base.CanConvertTo(context, destinationType);
		}
	}
}
