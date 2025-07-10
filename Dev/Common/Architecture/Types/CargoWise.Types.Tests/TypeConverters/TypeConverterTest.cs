using System;
using System.ComponentModel;
using System.Globalization;
using NUnit.Framework;

namespace CargoWise.Types.Tests
{
	public abstract class TypeConverterTest : TestCase
	{
		public virtual void TestTypeCanBeConstructedUsingItself()
		{
			Type type = GetZTypeImplementingTypeConverter();
			IZType obj = (IZType)Activator.CreateInstance(type, Array.Empty<object>());
			IZType obj2 = (IZType)Activator.CreateInstance(type, new object[] { obj });
			AssertEquals(obj, obj2);
		}

		[ExpectNoExceptions]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestConvertFrom()
		{
			TypeConverter converter = GetTypeConverter();
			foreach (ValueMapping valueMapping in GetConvertibleFromValues())
			{
				AssertEquals(GetZTypeImplementingTypeConverter(), valueMapping.ToType);
				if (converter.CanConvertFrom(valueMapping.FromType))
				{
					AssertEquals(valueMapping.To, converter.ConvertFrom(null, CultureInfo.InvariantCulture, valueMapping.From));
				}
				else
				{
					Fail("CanConvertFrom returned false on an acceptable value");
				}
			}
		}

		[ExpectNoExceptions]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestConvertTo()
		{
			TypeConverter converter = GetTypeConverter();
			foreach (ValueMapping valueMapping in GetConvertibleToValues())
			{
				AssertEquals(GetZTypeImplementingTypeConverter(), valueMapping.FromType);
				if (converter.CanConvertTo(valueMapping.ToType))
				{
					object result = converter.ConvertTo(null, CultureInfo.InvariantCulture, valueMapping.From, valueMapping.ToType);
					if (valueMapping.ToType == typeof(byte[]))
					{
						AssertEquals((byte[])valueMapping.To, (byte[])result);
					}
					else
					{
						AssertEquals(valueMapping.To, result);
					}
				}
				else
				{
					Fail("CanConvertTo returned false on an acceptable value");
				}
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestCanConvertFrom()
		{
			foreach (Type type in GetNonConvertibleFromTypes())
			{
				AssertEquals(false, GetTypeConverter().CanConvertFrom(type));
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestCanConvertTo()
		{
			foreach (Type type in GetNonConvertibleToTypes())
			{
				AssertEquals(false, GetTypeConverter().CanConvertTo(type));
			}
		}

		TypeConverter GetTypeConverter()
		{
			TypeConverter converter = TypeDescriptor.GetConverter(GetZTypeImplementingTypeConverter());
			if (converter.GetType() == typeof(TypeConverter))
			{
				throw new Exception(GetZTypeImplementingTypeConverter().FullName + " does not implement a custom type converter");
			}
			return converter;
		}

		protected abstract Type GetZTypeImplementingTypeConverter();
		protected abstract ValueMapping[] GetConvertibleFromValues();
		protected abstract ValueMapping[] GetConvertibleToValues();
		protected abstract Type[] GetNonConvertibleFromTypes();
		protected abstract Type[] GetNonConvertibleToTypes();
	}
}
