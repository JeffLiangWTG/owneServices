using System;
using System.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZIntTypeConverter : TypeConverter
	{
		public static readonly ZIntTypeConverter Instance = new ZIntTypeConverter();

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			try
			{
				if (value is string @string)
				{
					if (decimal.TryParse(@string, System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowThousands, culture, out var dec))
					{
						return new ZInt(decimal.ToInt32(dec));
					}

					return string.IsNullOrWhiteSpace(@string) ? ZInt.Zero : new ZInt(Convert.ToInt32(@string));
				}
				else if (value is ZString string1)
				{
					return string.IsNullOrWhiteSpace(string1) ? ZInt.Zero : new ZInt(Convert.ToInt32(string1));
				}
				else if (value is int || value is short || value is byte)
				{
					return new ZInt(Convert.ToInt32(value));
				}
				else if (value is ZInt)
				{
					return value;
				}
				else
				{
					if (value is INumericZType valueAsINumericZType)
					{
						return valueAsINumericZType.ToZInt();
					}
					else if (value == DBNull.Value || value == null)
					{
						return ZInt.Zero;
					}
					else
					{
						return base.ConvertFrom(context, culture, value) ?? throw new FormatException("System class System.ComponentModel.TypeConverter returned null");
					}
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
			return sourceType == typeof(int)
			|| sourceType == typeof(ZInt)
			|| sourceType == typeof(DBNull)
			|| sourceType == typeof(short)
			|| sourceType == typeof(byte)
			|| sourceType == typeof(ZShort)
			|| sourceType == typeof(ZByte)
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
					value = ZInt.Zero;
				}

				if (!(value is ZInt))
				{
					throw new InvalidCastException("Specified cast is not valid. Type: " + value.GetType() + ". Value: " + value.ToString());
				}

				var zIntValue = (ZInt)value;
				if (destinationType == typeof(ZDecimal))
				{
					return new ZDecimal(zIntValue);
				}
				else if (destinationType == typeof(decimal))
				{
					return new decimal(zIntValue);
				}
				else if (destinationType == typeof(int))
				{
					return zIntValue.fValue;
				}
				else if (destinationType == typeof(ZInt))
				{
					return zIntValue;
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
			return
				destinationType == typeof(decimal)
			|| destinationType == typeof(ZDecimal)
			|| destinationType == typeof(int)
			|| destinationType == typeof(ZInt)
			|| base.CanConvertTo(context, destinationType);
		}
	}
}

