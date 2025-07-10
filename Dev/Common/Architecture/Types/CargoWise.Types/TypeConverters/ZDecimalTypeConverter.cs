using System;
using System.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZDecimalTypeConverter : TypeConverter
	{
		public static readonly ZDecimalTypeConverter Instance = new ZDecimalTypeConverter();

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			try
			{
				if (value is decimal @decimal)
				{
					return new ZDecimal(@decimal);
				}
				else if (value is ZDecimal)
				{
					return value;
				}
				else
				{
					if (value is INumericZType valueAsINumericZType)
					{
						return new ZDecimal(valueAsINumericZType.ToZInt());
					}
					else if (value == DBNull.Value || value == null)
					{
						return ZDecimal.Zero;
					}
					else if (value is ZString @string)
					{
						if (string.IsNullOrWhiteSpace(@string))
						{
							return ZDecimal.Zero;
						}
						else
						{
							return new ZDecimal(Convert.ToDecimal(value.ToString()));
						}
					}
					else
					{
						return new ZDecimal(Convert.ToDecimal(value));
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
			return sourceType == typeof(decimal)
				|| sourceType == typeof(ZDecimal)
				|| sourceType == typeof(DBNull)
				|| sourceType == typeof(int)
				|| sourceType == typeof(short)
				|| sourceType == typeof(long)
				|| sourceType == typeof(double)
				|| sourceType == typeof(float)
				|| sourceType == typeof(ZInt)
				|| sourceType == typeof(short)
				|| sourceType == typeof(ZShort)
				|| sourceType == typeof(long)
				|| sourceType == typeof(ZLong)
				|| sourceType == typeof(byte)
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
					value = ZDecimal.Zero;
				}

				var zDecimalValue = (ZDecimal)value;
				if (destinationType == typeof(decimal))
				{
					return zDecimalValue.ToDecimal();
				}

				if (destinationType == typeof(ZDecimal))
				{
					return zDecimalValue;
				}

				return base.ConvertTo(context, culture, value, destinationType) ?? throw new FormatException("System class System.ComponentModel.TypeConverter returned null");
			}
			catch (FormatException ex)
			{
				ex.RethrowForConvertTo(value, destinationType);
				throw;
			}
			catch (InvalidCastException ex)
			{
				ex.ReThrowInvalidCastExceptionForConvertTo(value, typeof(ZDecimal));
				throw;
			}
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return
				destinationType == typeof(decimal)
			|| destinationType == typeof(ZDecimal)
			|| base.CanConvertTo(context, destinationType);
		}
	}
}
