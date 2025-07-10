using System;
using System.ComponentModel;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZBlobTypeConverter : TypeConverter
	{
		public static readonly ZBlobTypeConverter Instance = new ZBlobTypeConverter();

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			try
			{
				if (value is byte[] valueAsByteArray)
				{
					return new ZBlob(valueAsByteArray);
				}
				else if (value is ZBlob blobValue)
				{
					return blobValue;
				}
				else if (value == DBNull.Value || value == null)
				{
					return ZBlob.Empty;
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
			return sourceType == typeof(byte[]) ||
				sourceType == typeof(ZBlob) ||
				sourceType == typeof(DBNull) ||
				base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			try
			{
				if (value == null)
				{
					throw new FormatException("Casting null to a blob");
				}

				ZBlob blobValue = (ZBlob)value;
				if (destinationType == typeof(byte[]))
				{
					return (byte[])blobValue;
				}
				else if (destinationType == typeof(ZBlob))
				{
					return blobValue;
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
			return destinationType == typeof(byte[])
				|| destinationType == typeof(ZBlob)
				|| base.CanConvertTo(context, destinationType);
		}
	}
}
