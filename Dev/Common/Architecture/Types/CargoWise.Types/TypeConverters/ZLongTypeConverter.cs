using System;
using System.ComponentModel;
using System.Globalization;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZLongTypeConverter : TypeConverter
	{
		public static readonly ZLongTypeConverter Instance = new ZLongTypeConverter();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Convert.ToInt64(System.String)")]
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			try
			{
				if (value is string stringValue)
				{
					return string.IsNullOrWhiteSpace(stringValue) ? ZLong.Zero : new ZLong(Convert.ToInt64(stringValue));
				}
				else if (value is ZString zStringValue)
				{
					return string.IsNullOrWhiteSpace(zStringValue) ? ZLong.Zero : new ZLong(Convert.ToInt64(zStringValue));
				}
				else if (value is byte byteValue)
				{
					return new ZLong(byteValue);
				}
				else if (value is short shortValue)
				{
					return new ZLong(shortValue);
				}
				else if (value is ZByte zByteValue)
				{
					return new ZLong(zByteValue);
				}
				else if (value is ZShort zShortValue)
				{
					return new ZLong(zShortValue);
				}
				else if (value is int intValue)
				{
					return new ZLong(intValue);
				}
				else if (value is ZInt zIntValue)
				{
					return new ZLong(zIntValue);
				}
				else if (value is long longValue)
				{
					return new ZLong(longValue);
				}
				else if (value is ZLong zLongValue)
				{
					return zLongValue;
				}
				else if (value == DBNull.Value || value == null)
				{
					return ZLong.Zero;
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
			return sourceType == typeof(byte)
			|| sourceType == typeof(ZByte)
			|| sourceType == typeof(DBNull)
			|| sourceType == typeof(short)
			|| sourceType == typeof(ZShort)
			|| sourceType == typeof(int)
			|| sourceType == typeof(ZInt)
			|| sourceType == typeof(long)
			|| sourceType == typeof(ZLong)
			|| sourceType == typeof(string)
			|| sourceType == typeof(ZString)
			|| base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			try
			{
				if (value == null)
				{
					throw new FormatException("Casting null to a ZLong");
				}

				ZLong zLongValue = (ZLong)value;
				if (destinationType == typeof(ZDecimal))
				{
					return new ZDecimal(zLongValue);
				}
				else if (destinationType == typeof(decimal))
				{
					return new decimal(zLongValue);
				}
				else if (destinationType == typeof(long))
				{
					return zLongValue.fValue;
				}
				else if (destinationType == typeof(ZLong))
				{
					return zLongValue;
				}
				else if (destinationType == typeof(int))
				{
					return (int)zLongValue.ToZInt();
				}
				else if (destinationType == typeof(ZInt))
				{
					return zLongValue.ToZInt();
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
			catch (InvalidCastException ex)
			{
				ex.ReThrowInvalidCastExceptionForConvertTo(value, typeof(ZLong));
				throw;
			}
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(decimal)
			|| destinationType == typeof(ZDecimal)
			|| destinationType == typeof(long)
			|| destinationType == typeof(ZLong)
			|| base.CanConvertTo(context, destinationType);
		}
	}
}
