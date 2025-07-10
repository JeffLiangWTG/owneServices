using System;
using System.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZByteTypeConverter : TypeConverter
	{
		public static readonly ZByteTypeConverter Instance = new ZByteTypeConverter();

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			try
			{
				if (value is byte byteValue)
				{
					return new ZByte(byteValue);
				}
				else if (value is ZByte)
				{
					return value;
				}
				if (value is string stringValue)
				{
					return string.IsNullOrWhiteSpace(stringValue) ? ZByte.Zero : new ZByte(Convert.ToByte(stringValue));
				}
				else if (value is ZString zStringValue)
				{
					return string.IsNullOrWhiteSpace(zStringValue) ? ZByte.Zero : new ZByte(Convert.ToByte(zStringValue));
				}
				else if (value == DBNull.Value || value == null)
				{
					return ZByte.Zero;
				}
				else
				{
					return base.ConvertFrom(context, culture, value) ?? new FormatException("System class System.ComponentModel.TypeConverter returned null");
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
			return sourceType == typeof(byte)
			|| sourceType == typeof(ZByte)
			|| sourceType == typeof(DBNull)
			|| sourceType == typeof(string)
			|| sourceType == typeof(ZString)
			|| base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			try
			{
				if (value == null)
				{
					throw new FormatException("Casting null to a byte");
				}

				ZByte valueAsZByte = (ZByte)value;
				if (destinationType == typeof(byte))
				{
					return valueAsZByte.ToByte();
				}
				else if (destinationType == typeof(ZByte))
				{
					return valueAsZByte;
				}
				else if (destinationType == typeof(ZDecimal))
				{
					return new ZDecimal(valueAsZByte);
				}
				else if (destinationType == typeof(ZShort))
				{
					return new ZShort(Convert.ToInt16(valueAsZByte.ToByte()));
				}
				else if (destinationType == typeof(ZInt))
				{
					return new ZInt(valueAsZByte);
				}
				else if (destinationType == typeof(int))
				{
					return Convert.ToInt32(valueAsZByte.ToByte());
				}
				else if (destinationType == typeof(short))
				{
					return Convert.ToInt16(valueAsZByte.ToByte());
				}
				else if (destinationType == typeof(decimal))
				{
					return Convert.ToDecimal(valueAsZByte.ToByte());
				}
				else
				{
					return base.ConvertTo(context, culture, value, destinationType) ?? throw new FormatException("System class System.ComponentModel.TypeConverter returned null");
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
			return destinationType == typeof(byte)
				|| destinationType == typeof(ZByte)
				|| destinationType == typeof(decimal)
				|| destinationType == typeof(ZDecimal)
				|| destinationType == typeof(int)
				|| destinationType == typeof(ZInt)
				|| destinationType == typeof(short)
				|| destinationType == typeof(ZShort)
				|| base.CanConvertTo(context, destinationType);
		}
	}
}
