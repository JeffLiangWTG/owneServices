using System;
using System.ComponentModel;
using Microsoft.SqlServer.Types;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Types
{
	[ThreadSafe]
	public sealed class ZGeographyTypeConverter : TypeConverter
	{
		public static readonly ZGeographyTypeConverter Instance = new ZGeographyTypeConverter();

		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			try
			{
				if (value is ZGeography)
				{
					return value;
				}
				else if (value is string stringValue)
				{
					return new ZGeography(stringValue);
				}
				else if (value is ZString zStringValue)
				{
					return new ZGeography(zStringValue);
				}
				else if (value == null || value is DBNull)
				{
					return ZGeography.Empty;
				}
				else if (value is SqlGeography sqlGeographyValue)
				{
					return new ZGeography(sqlGeographyValue);
				}
				else if (value is ZBlob zBlobValue)
				{
					return new ZGeography(zBlobValue);
				}
				else if (value is byte[] byteArrayValue)
				{
					return new ZGeography(byteArrayValue);
				}
				else if (value is SqlBytes sqlBytesValue)
				{
					return new ZGeography(sqlBytesValue);
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
			return sourceType == typeof(ZGeography) ||
				sourceType == typeof(SqlGeography) ||
				sourceType == typeof(string) ||
				sourceType == typeof(ZString) ||
				sourceType == typeof(DBNull) ||
				sourceType == typeof(ZBlob) ||
				sourceType == typeof(byte[]) ||
				sourceType == typeof(SqlBytes) ||
				base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			try
			{
				if (value == null || value is DBNull)
				{
					throw new FormatException("Casting null to a ZGeography");
				}

				ZGeography geoValue = (ZGeography)value;

				if (destinationType == typeof(ZGeography))
				{
					return geoValue;
				}
				else if (destinationType == typeof(SqlGeography) && geoValue.IsValid)
				{
					var result = (SqlGeography)geoValue;
					return result;
				}
				else if (destinationType == typeof(string))
				{
					return geoValue.ToString();
				}
				else if (destinationType == typeof(ZString))
				{
					return (ZString)geoValue.ToString();
				}
				else if (destinationType == typeof(byte[]) && geoValue.IsValid)
				{
					return geoValue.AsBinary();
				}
				else if (destinationType == typeof(SqlBytes) && geoValue.IsValid)
				{
					return new SqlBytes(geoValue.AsBinary());
				}
				else if (destinationType == typeof(ZBlob) && geoValue.IsValid)
				{
					return new ZBlob(geoValue.AsBinary());
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
			return destinationType == typeof(SqlGeography)
				|| destinationType == typeof(ZGeography)
				|| destinationType == typeof(string)
				|| destinationType == typeof(ZString)
				|| destinationType == typeof(byte[])
				|| destinationType == typeof(SqlBytes)
				|| destinationType == typeof(ZBlob)
				|| base.CanConvertTo(context, destinationType);
		}
	}
}
