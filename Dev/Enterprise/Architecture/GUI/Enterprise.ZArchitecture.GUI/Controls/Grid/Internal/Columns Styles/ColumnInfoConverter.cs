using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace Enterprise.Core.Forms
{
	/// <summary>
	/// Provides an InstanceDescriptor of the ColumnInfo for ZGrid Serialisation.
	/// </summary>
	internal class ColumnInfoConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				return true;
			}
			else
			{
				return base.CanConvertTo(context, destinationType);
			}
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
			{
				var constructor = value.GetType().GetConstructor(Array.Empty<Type>());
				return new InstanceDescriptor(constructor, Array.Empty<object>(), false);
			}
			else
			{
				return base.ConvertTo(context, culture, value, destinationType);
			}
		}
	}
}