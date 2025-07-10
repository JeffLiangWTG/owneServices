using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.EntityFramework
{
	[TypeConverter(typeof(SchemaFieldTypeConverter))]
	public struct SchemaField
	{
		public SchemaField(string value)
		{
			if (value == null)
			{
				throw new Exception("SchemaField must receive a non-null string in its constructor");
			}

			fValue = value;
		}

		public static implicit operator SchemaField(string value)
		{
			return new SchemaField(value);
		}
		[SuppressMessage("Maintainability", "IDE0052: Remove unread private member.", Justification = "It is used for implicit conversion")]
		readonly string fValue;
	}
	class SchemaFieldTypeConverter : TypeConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			return new SchemaField(value.ToString());
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			bool canConvert;

			if (sourceType == typeof(string))
			{
				canConvert = true;
			}
			else
			{
				canConvert = base.CanConvertFrom(context, sourceType);
			}

			return canConvert;
		}
	}
}
