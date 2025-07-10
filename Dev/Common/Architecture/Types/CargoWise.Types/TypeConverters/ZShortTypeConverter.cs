using System;
using System.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZShortTypeConverter : TypeConverter
	{
		public static readonly ZShortTypeConverter Instance = new ZShortTypeConverter();

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			try
			{
				if (value is string stringValue)
				{
					return string.IsNullOrWhiteSpace(stringValue) ? ZShort.Zero : new ZShort(Convert.ToInt16(stringValue));
				}
				else if (value is ZString zStringValue)
				{
					return string.IsNullOrWhiteSpace(zStringValue) ? ZShort.Zero : new ZShort(Convert.ToInt16(zStringValue));
				}
				else if (value is byte byteValue)
				{
					return new ZShort(byteValue);
				}
				else if (value is short shortValue)
				{
					return new ZShort(shortValue);
				}
				else if (value is ZByte zByteValue)
				{
					return new ZShort(zByteValue);
				}
				else if (value is ZShort zShortValue)
				{
					return zShortValue;
				}
				else if (value == DBNull.Value || value == null)
				{
					return ZShort.Zero;
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
					throw new FormatException("Casting null to a zshort");
				}

				ZShort zShortValue = (ZShort)value;
				if (destinationType == typeof(short))
				{
					return zShortValue.fValue;
				}
				else if (destinationType == typeof(ZShort))
				{
					return zShortValue;
				}
				else if (destinationType == typeof(int))
				{
					return (int)zShortValue.ToZInt();
				}
				else if (destinationType == typeof(ZInt))
				{
					return zShortValue.ToZInt();
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
				ex.ReThrowInvalidCastExceptionForConvertTo(value, typeof(ZShort));
				throw;
			}
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(short)
			|| destinationType == typeof(ZShort)
			|| destinationType == typeof(ZInt)
			|| destinationType == typeof(int)
			|| base.CanConvertTo(context, destinationType);
		}
	}
}
