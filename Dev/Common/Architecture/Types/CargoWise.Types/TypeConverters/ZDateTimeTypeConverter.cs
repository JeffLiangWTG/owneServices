using System;
using System.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZDateTimeTypeConverter : TypeConverter
	{
		public static readonly ZDateTimeTypeConverter Instance = new ZDateTimeTypeConverter();

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			try
			{
				object result;
				if (value is DateTime time)
				{
					result = new ZDateTime(time);
				}
				else if (value is ZDateTime time1)
				{
					result = time1;
				}
				else if (value == DBNull.Value || value == null)
				{
					result = ZDateTime.Empty;
				}
				else if (value is ZDate date)
				{
					result = date.ToZDateTime();
				}
				else if (value is ZTime ztime)
				{
					if (ztime.IsEmpty)
					{
						result = ZDateTime.Empty;
					}
					else if (!ztime.IsValid)
					{
						result = ZDateTime.Invalid;
					}
					else
					{
						result = new ZDateTime(ztime.ToDateTime());
					}
				}
				else if (value is string || value is ZString)
				{
					string str = value.ToString();
					if (str.Length == 0)
					{
						result = ZDateTime.Empty;
					}
					else if (string.Compare(str, ZDateTime.InvalidLiteral, StringComparison.OrdinalIgnoreCase) == 0)
					{
						result = ZDateTime.Invalid;
					}
					else
					{
						try
						{
							result = new ZDateTime(dateTimeConverter.ConvertFrom(context, culture, str));
						}
						catch (FormatException)
						{
							throw new ZTypeValueException(typeof(ZDateTime), value);
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
				|| sourceType == typeof(ZDateTime)
				|| sourceType == typeof(DBNull)
				|| sourceType == typeof(ZDate)
				|| sourceType == typeof(ZTime)
				|| sourceType == typeof(ZString)
				|| sourceType == typeof(string)
				|| base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			try
			{
				if (destinationType == typeof(ZDateTime) && value != null)
				{
					return (ZDateTime)value;
				}
				else if (destinationType == typeof(ZDateTimeOffset) && value != null)
				{
					return new ZDateTimeOffset(value);
				}
				else if (destinationType == typeof(ZDate) && value != null)
				{
					return (ZDate)((ZDateTime)value);
				}
				else if (destinationType == typeof(ZTime) && value != null)
				{
					if (!((ZDateTime)value).IsValid)
					{
						if (((ZDateTime)value).IsEmpty)
						{
							return ZTime.Empty;
						}
						else
						{
							return ZTime.Invalid;
						}
					}
					return (ZTime)(((ZDateTime)value).ToTimeSpan());
				}
				else if (destinationType == typeof(DateTime) && value != null)
				{
					var zDateTimeValue = (ZDateTime)value;
					if (zDateTimeValue.IsValid)
					{
						return zDateTimeValue.ToDateTime();
					}
					else
					{
						throw new OperationOnInvalidZDateTimeException("Cannot convert an Empty or Invalid ZDateTime to a DateTime!");
					}
				}
				else if (value is ZDateTime time && destinationType == typeof(string))
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
				|| destinationType == typeof(ZDateTime)
				|| destinationType == typeof(ZDateTimeOffset)
				|| destinationType == typeof(ZDate)
				|| destinationType == typeof(ZTime)
				|| destinationType == typeof(string)
				|| base.CanConvertTo(context, destinationType);
		}

		readonly DateTimeConverter dateTimeConverter = new DateTimeConverter();
	}
}
