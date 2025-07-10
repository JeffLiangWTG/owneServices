using System;
using System.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZTimeTypeConverter : TypeConverter
	{
		public static readonly ZTimeTypeConverter Instance = new ZTimeTypeConverter();

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			try
			{
				object result;
				if (value is DateTime time)
				{
					result = new ZTime(time);
				}
				else if (value is TimeSpan timeSpan)
				{
					result = new ZTime(timeSpan);
				}
				else if (value is ZTime time1)
				{
					result = time1;
				}
				else if (value == DBNull.Value || value == null)
				{
					result = ZTime.Empty;
				}
				else if (value is ZLong longValue)
				{
					result = new ZTime(longValue.fValue);
				}
				else if (value is string || value is ZString)
				{
					string str = value.ToString();
					if (str.Length == 0)
					{
						result = ZTime.Empty;
					}
					else if (string.Compare(str, ZTime.InvalidLiteral, StringComparison.OrdinalIgnoreCase) == 0)
					{
						result = ZTime.Invalid;
					}
					else
					{
						try
						{
							result = new ZTime(dateTimeConverter.ConvertFrom(context, culture, str));
						}
						catch (FormatException)
						{
							throw new ZTypeValueException(typeof(ZTime), value);
						}
					}
				}
				else
				{
					result = base.ConvertFrom(context, culture, value);
					if (result == null)
					{
						throw new FormatException("System class System.ComponentModel.TypeConverter returned null");
					}
				}
				return result;
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
				sourceType == typeof(DateTime)
				|| sourceType == typeof(TimeSpan)
				|| sourceType == typeof(ZTime)
				|| sourceType == typeof(ZDateTime)
				|| sourceType == typeof(DBNull)
				|| sourceType == typeof(ZLong)
				|| sourceType == typeof(ZString)
				|| sourceType == typeof(string)
				|| base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			try
			{
				if (destinationType == typeof(ZTime) && value != null)
				{
					return (ZTime)value;
				}
				else if (destinationType == typeof(ZDateTime) && value != null)
				{
					if (value is ZTime zt)
					{
						return new ZDateTime(zt.Ticks + ZDateTime.Today.Ticks);
					}
					return null;
				}
				else if (destinationType == typeof(ZLong) && value != null)
				{
					return (ZLong)((ZTime)value).Ticks;
				}
				else if (destinationType == typeof(TimeSpan) && value != null)
				{
					return ((ZTime)value).ToTimeSpan();
				}
				else if (destinationType == typeof(DateTime) && value != null)
				{
					var zDateTimeValue = (ZTime)value;
					if (zDateTimeValue.IsValid)
					{
						return zDateTimeValue.ToDateTime();
					}
					else
					{
						throw new OperationOnInvalidZTimeException("Cannot convert an Empty or Invalid ZTime to a DateTime!");
					}
				}
				else if (value is ZTime time && destinationType == typeof(string))
				{
					var zDateTimeValue = time;
					if (zDateTimeValue.IsValid)
					{
						return dateTimeConverter.ConvertTo(context, culture, zDateTimeValue.ToDateTime(), destinationType)
							?? throw new FormatException("System class System.ComponentModel.TypeConverter returned null");
					}
					else
					{
						return zDateTimeValue.ToString();
					}
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
				destinationType == typeof(DateTime)
				|| destinationType == typeof(TimeSpan)
				|| destinationType == typeof(ZTime)
				|| destinationType == typeof(ZLong)
				|| destinationType == typeof(ZDateTime)
				|| destinationType == typeof(string)
				|| base.CanConvertTo(context, destinationType);
		}

		readonly DateTimeConverter dateTimeConverter = new DateTimeConverter();
	}
}
