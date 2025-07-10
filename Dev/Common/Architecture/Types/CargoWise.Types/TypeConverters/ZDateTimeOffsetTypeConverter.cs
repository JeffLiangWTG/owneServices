using System;
using System.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZDateTimeOffsetTypeConverter : TypeConverter
	{
		public static readonly ZDateTimeOffsetTypeConverter Instance = new ZDateTimeOffsetTypeConverter();

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			try
			{
				object result;
				if (value is DateTime dateTimeValue)
				{
					result = new ZDateTimeOffset(dateTimeValue);
				}
				else if (value is ZDate date)
				{
					result = new ZDateTimeOffset(date.ToZDateTime());
				}
				else if (value is ZDateTime zDateTimeValue)
				{
					result = new ZDateTimeOffset(zDateTimeValue);
				}
				else if (value is DateTimeOffset dateTimeOffsetValue)
				{
					result = new ZDateTimeOffset(dateTimeOffsetValue);
				}
				else if (value is ZDateTimeOffset offset)
				{
					result = offset;
				}
				else if (value == DBNull.Value || value == null)
				{
					result = ZDateTimeOffset.Empty;
				}
				else if (value is string || value is ZString)
				{
					var str = value.ToString().Trim();
					if (str.Length == 0)
					{
						result = ZDateTimeOffset.Empty;
					}
					else
					{
						try
						{
							if (!ZDateTimeOffset.TryParseForUpdate(ZDateTimeOffset.Invalid, str, out var resultAsOffset, culture))
							{
								throw new ZTypeValueException(typeof(ZDateTimeOffset), value);
							}
							result = resultAsOffset;
						}
						catch (FormatException)
						{
							throw new ZTypeValueException(typeof(ZDateTimeOffset), value);
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
				|| sourceType == typeof(DateTimeOffset)
				|| sourceType == typeof(ZDateTimeOffset)
				|| sourceType == typeof(DBNull)
				|| sourceType == typeof(ZDate)
				|| sourceType == typeof(ZString)
				|| sourceType == typeof(string)
				|| base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			try
			{
				if (destinationType == typeof(ZDateTimeOffset) && value != null)
				{
					return (ZDateTimeOffset)value;
				}
				else if (destinationType == typeof(ZDateTime) && value != null)
				{
					var zDateTimeOffsetValue = (ZDateTimeOffset)value;
					if (zDateTimeOffsetValue.IsValid)
					{
						return new ZDateTime(zDateTimeOffsetValue.ToDateTime());
					}
					else if (zDateTimeOffsetValue.IsEmpty)
					{
						return ZDateTime.Empty;
					}
					return ZDateTime.Invalid;
				}
				else if (destinationType == typeof(ZDate) && value != null)
				{
					var zDateTimeOffsetValue = (ZDateTimeOffset)value;
					if (zDateTimeOffsetValue.IsValid)
					{
						return new ZDate(zDateTimeOffsetValue.ToDateTime());
					}
					else if (zDateTimeOffsetValue.IsEmpty)
					{
						return ZDate.Empty;
					}
					return ZDate.Invalid;
				}
				else if (destinationType == typeof(DateTimeOffset) && value != null)
				{
					var zDateTimeOffsetValue = (ZDateTimeOffset)value;
					if (zDateTimeOffsetValue.IsValid)
					{
						return zDateTimeOffsetValue.ToDateTimeOffset();
					}
					else
					{
						throw new OperationOnInvalidZDateTimeException("Cannot convert an Empty or Invalid ZDateTime to a DateTime!");
					}
				}
				else if (destinationType == typeof(DateTime) && value != null)
				{
					var zDateTimeOffsetValue = (ZDateTimeOffset)value;
					if (zDateTimeOffsetValue.IsValid)
					{
						return zDateTimeOffsetValue.ToDateTime();
					}
					else
					{
						throw new OperationOnInvalidZDateTimeException("Cannot convert an Empty or Invalid ZDateTime to a DateTime!");
					}
				}
				else if (value is ZDateTimeOffset offset && destinationType == typeof(string))
				{
					var zDateTimeOffsetValue = offset;
					if (zDateTimeOffsetValue.IsValid)
					{
						return dateTimeOffsetConverter.ConvertTo(context, culture, zDateTimeOffsetValue.ToDateTimeOffset(), destinationType)
							?? throw new FormatException("System class System.ComponentModel.TypeConverter returned null");
					}
					else
					{
						return zDateTimeOffsetValue.ToString();
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
				|| destinationType == typeof(ZDate)
				|| destinationType == typeof(DateTimeOffset)
				|| destinationType == typeof(ZDateTimeOffset)
				|| destinationType == typeof(string)
				|| base.CanConvertTo(context, destinationType);
		}

		readonly DateTimeOffsetConverter dateTimeOffsetConverter = new DateTimeOffsetConverter();
	}
}
