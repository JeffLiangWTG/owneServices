using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Database.Abstractions;

namespace CargoWise.Types
{
	public sealed class ZDbValueConversion : IDbValueConversion
	{
		static readonly object boxedBooleanTrue = true;
		static readonly object boxedBooleanFalse = false;

		public bool TryUnwrapSimpleValue(object value, out object unwrapped)
		{
			if (value is ZBool booleanValue)
			{
				unwrapped = booleanValue ? boxedBooleanTrue : boxedBooleanFalse;
				return true;
			}

			unwrapped = default;
			return false;
		}

		public TypeConverter TryGetConverterForValues(IEnumerable values)
		{
			var zValue = values.OfType<IZType>().FirstOrDefault();
			if (zValue is null)
			{
				return null;
			}

			var converter = TypeDescriptor.GetConverter(zValue);
			if (zValue is ZGuid)
			{
				converter = new InvalidZGuidTypeConverter(converter);
			}

			return converter;
		}

		sealed class InvalidZGuidTypeConverter : TypeConverter
		{
			public InvalidZGuidTypeConverter(TypeConverter inner)
			{
				this.inner = inner ?? throw new ArgumentNullException(nameof(inner));
			}

			readonly TypeConverter inner;

			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => inner.CanConvertFrom(context, sourceType);

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) => inner.ConvertFrom(context, culture, value);

			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => inner.CanConvertTo(context, destinationType);

			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (destinationType == typeof(Guid) && value is ZGuid guidValue && !guidValue.IsValid)
				{
					return new Guid(value.ToString());
				}

				return inner.ConvertTo(context, culture, value, destinationType);
			}
		}
	}
}
