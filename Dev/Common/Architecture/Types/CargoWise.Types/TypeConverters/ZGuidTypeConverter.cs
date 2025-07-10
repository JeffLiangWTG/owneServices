using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZGuidTypeConverter : TypeConverter
	{
		public static readonly ZGuidTypeConverter Instance = new ZGuidTypeConverter();

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			try
			{
				if (value is Guid guidValue)
				{
					return new ZGuid(guidValue);
				}
				else if (value is DBNull || value == null)
				{
					return ZGuid.Empty;
				}
				else if (value is string || value is ZString)
				{
					var str = value.ToString();
					return str.Length == 0 ? ZGuid.Empty : new ZGuid(new Guid(str));
				}
				else if (value is ZGuid zGuidValue)
				{
					return zGuidValue;
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
			return sourceType == typeof(Guid)
				|| sourceType == typeof(DBNull)
				|| sourceType == typeof(ZGuid)
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
					throw new FormatException("Casting null to a zGuid");
				}

				ZGuid zGuidValue = ZGuid.Empty;
				if (value is ZGuid)
				{
					zGuidValue = (ZGuid)value;
				}
				else
				{
					if (destinationType == typeof(Guid) ||
							destinationType == typeof(ZGuid) ||
							destinationType == typeof(InstanceDescriptor))
					{
						throw new FormatException("Casting " + value.GetType().ToString() + " to a ZGuid. DestinationType is " + destinationType + ".");
					}
				}

				if (destinationType == typeof(Guid))
				{
					if (zGuidValue.IsValid)
					{
						return zGuidValue.ToGuid();
					}
					else if (zGuidValue.IsEmpty)
					{
						return Guid.Empty;
					}
					else
					{
						throw new FormatException("Cannot convert ZGuid.Invalid to Guid.");
					}
				}

				if (destinationType == typeof(ZGuid))
				{
					return value;
				}

				else if (destinationType == typeof(InstanceDescriptor))
				{
					return new InstanceDescriptor(typeof(ZGuid).GetConstructor(new Type[] { typeof(object) }), new object[] { zGuidValue.ToString() });
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
			return destinationType == typeof(Guid)
				|| destinationType == typeof(ZGuid)
				|| destinationType == typeof(InstanceDescriptor)
				|| base.CanConvertTo(context, destinationType);
		}
	}
}
