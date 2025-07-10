using System;
using System.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZDateTypeConverter : TypeConverter
	{
		public static readonly ZDateTypeConverter Instance = new ZDateTypeConverter();

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			try
			{
				if (value is ZDate zDateValue)
				{
					return zDateValue;
				}
				else if (value == DBNull.Value || value == null)
				{
					return ZDate.Empty;
				}
				else if (value is DateTime dateTimeValue)
				{
					return new ZDate(new ZDateTime(dateTimeValue));
				}
				else if (value is ZDateTime zDateTimeValue)
				{
					return new ZDate(zDateTimeValue);
				}
				else if (value is ZString || value is string)
				{
					string str = value.ToString();
					if (str.Length == 0)
					{
						return ZDate.Empty;
					}
					else if (str == ZDate.Invalid.ToString())
					{
						return ZDate.Invalid;
					}
					else
					{
						try
						{
							return new ZDateTime(dateTimeConverter.ConvertFrom(context, culture, str)).Date;
						}
						catch (FormatException e)
						{
							throw new ZTypeValueException(typeof(ZDateTime), value, e);
						}
					}
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
			return
				sourceType == typeof(DBNull)
				|| sourceType == typeof(DateTime)
				|| sourceType == typeof(ZDate)
				|| sourceType == typeof(ZDateTime)
				|| sourceType == typeof(ZString)
				|| sourceType == typeof(string)
				|| base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			try
			{
				if (value == null)
				{
					throw new FormatException("Casting null to a zdate");
				}

				ZDate zDateValue = (ZDate)value;
				if (destinationType == typeof(DateTime) && zDateValue.IsValid)
				{
					return zDateValue.ToDateTime();
				}
				else if (destinationType == typeof(ZDate))
				{
					return zDateValue;
				}
				else if (destinationType == typeof(ZDateTime))
				{
					return new ZDateTime(zDateValue);
				}
				else if (destinationType == typeof(string) || destinationType == typeof(ZString))
				{
					var str = (zDateValue.IsValid
						? (string)dateTimeConverter.ConvertTo(context, culture, zDateValue.ToDateTime(), typeof(string))
						: zDateValue.ToString())
						?? throw new FormatException("System class System.ComponentModel.TypeConverter returned null");

					if (destinationType == typeof(string))
					{
						return str;
					}
					else
					{
						return new ZString(str);
					}
				}
				else
				{
					return base.ConvertTo(context, culture, zDateValue, destinationType) ?? throw new FormatException("System class System.ComponentModel.TypeConverter returned null");
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
				destinationType == typeof(DateTime)
				|| destinationType == typeof(ZDate)
				|| destinationType == typeof(string)
				|| destinationType == typeof(ZString)
				|| destinationType == typeof(ZDateTime);
		}

		readonly DateTimeConverter dateTimeConverter = new DateTimeConverter();
	}
}
