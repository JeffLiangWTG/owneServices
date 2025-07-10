using System;
using System.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZBoolTypeConverter : TypeConverter
	{
		public static readonly ZBoolTypeConverter Instance = new ZBoolTypeConverter();

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			try
			{
				if (value is ZString || value is string)
				{
					if (ZBool.TryParse(value.ToString(), out var result))
					{
						return result;
					}

					throw new ZTypeValueException(typeof(ZBool), value);
				}
				else if (value is char charValue)
				{
					return new ZBool(charValue);
				}
				else if (value is bool boolValue)
				{
					return new ZBool(boolValue);
				}
				else if (value is ZBool zBoolValue)
				{
					return zBoolValue;
				}
				else if (value is int intValue && (intValue == 0 || intValue == 1))
				{
					return new ZBool(Convert.ToBoolean(intValue));
				}
				else if (value == DBNull.Value || value == null)
				{
					return ZBool.False;
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
			return sourceType == typeof(bool) ||
				sourceType == typeof(DBNull) ||
				sourceType == typeof(ZBool) ||
				sourceType == typeof(char) ||
				sourceType == typeof(string) ||
				sourceType == typeof(ZString) ||
				sourceType == typeof(int) ||
				base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			try
			{
				if (value == null)
				{
					throw new FormatException("Casting null to Zbool");
				}

				ZBool boolValue = (ZBool)value;
				if (destinationType == typeof(bool))
				{
					return (bool)boolValue;
				}
				else if (destinationType == typeof(ZBool))
				{
					return boolValue;
				}
				else if (destinationType == typeof(string))
				{
					return boolValue.ToString();
				}
				else if (destinationType == typeof(ZString))
				{
					return new ZString(boolValue.ToString());
				}
				else if (destinationType == typeof(char))
				{
					var boolString = boolValue.ToString();
					return boolString[0];
				}
				else if (destinationType == typeof(int) && boolValue)
				{
					return 1;
				}
				else if (destinationType == typeof(int) && !boolValue)
				{
					return 0;
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
				throw new ArgumentException("Expected object of type Zbool, recieved: " + value.GetType() + " with value: " + value, ex);
			}
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(bool)
				|| destinationType == typeof(ZBool)
				|| destinationType == typeof(string)
				|| destinationType == typeof(ZString)
				|| destinationType == typeof(char)
				|| destinationType == typeof(int);
		}
	}
}
