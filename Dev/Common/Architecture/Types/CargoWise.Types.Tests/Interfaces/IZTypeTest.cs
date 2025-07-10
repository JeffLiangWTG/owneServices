using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace CargoWise.Types.Tests
{
	using NUnit.Framework;

	public abstract class IZTypeTest : TestCase
	{
		public void TestZTypeValueException()
		{
			foreach (object value in UnsupportedValues)
			{
				bool wasThrown = false;

				try
				{
					NewZ(value);
				}
				catch (ZTypeValueException)
				{
					wasThrown = true;
				}

				Assert(MessageForValue(value), wasThrown);
			}
		}

		public void TestIsEmpty()
		{
			foreach (object value in AllValues)
			{
				AssertEquals(MessageForValue(value), IsValueThatWillCreateEmptyZType(value), NewZ(value).IsEmpty);
			}
		}

		public void TestIsValid()
		{
			foreach (object value in AllValues)
			{
				AssertEquals(MessageForValue(value), ValueIsValid(value), NewZ(value).IsValid);
			}
		}

		public void TestIsDefault()
		{
			foreach (object value in AllValues)
			{
				IZType z = NewZ(value);
				AssertEquals(MessageForValue(value), AnyZ.Default.Equals(z), z.IsDefault);
			}
		}

		public void TestDefault()
		{
			foreach (object value in AllValues)
			{
				IZType z = NewZ(value);
				AssertEquals(MessageForValue(value), AnyZ.Default, z.Default);
				AssertEquals(MessageForValue(value), true, z.Default.IsDefault);
			}
		}

		public void TestDataType()
		{
			ZDataType expected = AnyZ.DataType;

			foreach (object value in AllValues)
			{
				AssertEquals(MessageForValue(value), expected, NewZ(value).DataType);
			}
		}

		public void TestDataTypeIsNumeric()
		{
			bool expected = AnyZ.DataType.IsNumeric;

			foreach (object value in AllValues)
			{
				AssertEquals(MessageForValue(value), expected, NewZ(value).DataType.IsNumeric);
			}
		}

		public void TestDataTypeIsInteger()
		{
			bool expected = AnyZ.DataType.IsInteger;

			foreach (object value in AllValues)
			{
				AssertEquals(MessageForValue(value), expected, NewZ(value).DataType.IsInteger);
			}
		}

		public void TestIsIntegerImpliesIsNumeric()
		{
			foreach (object value in AllValues)
			{
				IZType z = NewZ(value);
				Assert(MessageForValue(value), !z.DataType.IsInteger || z.DataType.IsNumeric);
			}
		}

		public virtual void TestToString()
		{
			foreach (object value in ValidValues)
			{
				if (value != null)
				{
					AssertEquals(MessageForValue(value), value.ToString(), NewZ(value).ToString());
				}
			}
		}

		public virtual void TestEquals()
		{
			foreach (object value in AllValues)
			{
				string message = MessageForValue(value);
				IZType z = NewZ(value);

				AssertEquals(message, false, z.Equals(null));
				AssertEquals(message, false, z.Equals(new object()));
				AssertEquals(message, true, z.Equals(NewZ(value)));
				AssertEquals(message, ValueIsUsualType(value), z.Equals(value));
			}
		}

		public void TestEqualsUnfortunatelyIsntCommutative()
		{
			foreach (object value in ValidValues)
			{
				if (value != null)
				{
					IZType z = NewZ(value);
					AssertEquals(MessageForValue(value), value.GetType() == z.GetType(), value.Equals(z));
				}
			}
		}

		public void TestGetHashCode()
		{
			foreach (object value in AllValues)
			{
				AssertEquals(MessageForValue(value), NewZ(value).GetHashCode(), NewZ(value).GetHashCode());
			}
		}

		public virtual void TestCompareTo()
		{
			foreach (object value in AllValues)
			{
				string message = MessageForValue(value) + " compare to ";
				IZType z = NewZ(value);

				Assert(message + "null should always be greater than zero", z.CompareTo(null) > 0);
				AssertEquals(message + "same value", 0, z.CompareTo(NewZ(value)));

				object otherValue = z.IsEmpty ? DBNull.Value : value;

				try
				{
					AssertEquals(message + MessageForValue(otherValue), 0, z.CompareTo(otherValue));
				}
				catch (ArgumentException)
				{
					if (ValueIsUsualType(otherValue))
					{
						throw;
					}
				}
			}
		}

		public virtual void TestGetValue()
		{
			foreach (object value in AllValues)
			{
				string message = MessageForValue(value) + " ";
				IZType z = NewZ(value);

				object actualFalse = ((IZTypeInternals)z).GetValueForLogicalDataLayer(false);
				object actualTrue = ((IZTypeInternals)z).GetValueForLogicalDataLayer(true);

				if (z.IsEmpty)
				{
					AssertEquals(message + "GetValue(false) type when empty", UsualValueType, actualFalse.GetType());
					AssertEquals(message + "GetValue(true) when empty", DBNull.Value, actualTrue);
				}
				else if (ValueIsUsualType(value))
				{
					AssertEquals(message + "GetValue(false) when not empty", value, actualFalse);
					AssertEquals(message + "GetValue(true) when not empty", value, actualTrue);
				}
				else
				{
					AssertEquals(message + " when not empty", actualFalse, actualTrue);
				}
			}
		}

		#region Xml Support

		public void TestFieldsAndPropertiesHaveXmlIgnoreAttribute()
		{
			ArrayList members = new ArrayList();
			members.AddRange(AnyZ.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance));
			members.AddRange(AnyZ.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance));

			string propertiesWithoutXmlIgnore = string.Empty;
			foreach (MemberInfo fieldOrProperty in members)
			{
				if (fieldOrProperty.Name != "Item")
				{
					XmlIgnoreAttribute[] xmlIgnoreAttr = (XmlIgnoreAttribute[])fieldOrProperty.GetCustomAttributes(typeof(XmlIgnoreAttribute), true);
					XmlTextAttribute[] xmlTextAttr = (XmlTextAttribute[])fieldOrProperty.GetCustomAttributes(typeof(XmlTextAttribute), true);
					if (xmlIgnoreAttr.Length == 0 && xmlTextAttr.Length == 0)
					{
						propertiesWithoutXmlIgnore += fieldOrProperty.Name + "\r\n";
					}
				}
			}

			if (!string.IsNullOrEmpty(propertiesWithoutXmlIgnore))
			{
				string message =
					"The following Field/properties didn't have the XmlIgnoreAttribute applied to them and will be serialized " +
					"as elements which is probably not what you want on a Z type:\r\n\r\n" +
					propertiesWithoutXmlIgnore;
				Fail(message);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestXmlTextAttributeAppliedToOnly1Property()
		{
			bool foundOne = false;
			foreach (PropertyInfo property in AnyZ.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				XmlTextAttribute[] xmlTextAttr = (XmlTextAttribute[])property.GetCustomAttributes(typeof(XmlTextAttribute), true);
				if (xmlTextAttr.Length > 0)
				{
					if (foundOne)
					{
						Fail("2 properties with XmlTextAttribute were found. Only 1 can exist.");
					}

					foundOne = true;
				}
			}

			Assert(true);
		}

		public void TestXmlTextAttributePropertyReturnsText()
		{
			PropertyInfo xmlTextProperty = GetXmlTextProperty();
			object primitiveValueForXml = xmlTextProperty.GetValue(AnyZ, null);
			AssertNotNull(primitiveValueForXml);
		}

		public void TestGetSetXmlPrimitiveProperty()
		{
			PropertyInfo xmlTextProperty = GetXmlTextProperty();
			foreach (object value in AllValues)
			{
				IZType valueAsZType = NewZ(value);
				try
				{
					try
					{
						object primitiveValueForXml = xmlTextProperty.GetValue(valueAsZType, null);
						IZType constructedZType = GetValueFromDefaultConstructor();
						xmlTextProperty.SetValue(constructedZType, primitiveValueForXml, null);
						AssertEquals("IZType values should be the same after getting as xml text then setting from xml text", valueAsZType, constructedZType);
					}
					catch (TargetInvocationException ex)
					{
						throw ex.InnerException;
					}
				}
				catch (OperationOnInvalidZTypeException)
				{
					// it is acceptable to throw this exception on ZValues that are invalid
					if (!valueAsZType.IsEmpty && valueAsZType.IsValid)
					{
						throw;
					}
				}
			}
		}

		protected IZType GetValueFromDefaultConstructor()
		{
			Array emptyZType = Array.CreateInstance(AnyZ.GetType(), 1);
			return (IZType)emptyZType.GetValue(0);
		}

		PropertyInfo GetXmlTextProperty()
		{
			PropertyInfo result = null;
			foreach (PropertyInfo property in AnyZ.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (property.Name != "Item")
				{
					XmlTextAttribute[] xmlTextAttr = (XmlTextAttribute[])property.GetCustomAttributes(typeof(XmlTextAttribute), true);
					if (xmlTextAttr.Length > 0)
					{
						result = property;
						break;
					}
				}
			}

			return result;
		}

		public void TestIEnumerableShouldNotBeImplementedForXmlSerialization()
		{
			AssertEquals(
				AnyZ.GetType().Name + " cannot implement IEnumerable otherwise it can't be serialized to/from xml",
				false, (AnyZ is IEnumerable));
		}

		protected void AssertZTypeSerializesToXml(string expectedXml, IZType type)
		{
			XmlSerializer serializer = new XmlSerializer(type.GetType(), new XmlRootAttribute("a"));

			StringWriter writer = new StringWriter();
			serializer.Serialize(
				writer, type,
				new XmlSerializerNamespaces(new XmlQualifiedName[] { new XmlQualifiedName() }));
			string actualXml = writer.GetStringBuilder().ToString();
			actualXml = actualXml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", string.Empty).Trim();

			AssertEquals(
				"Xml should be generated correctly by System.Xml.Serialization.XmlSerializer for " + type.GetType().Name,
				expectedXml, actualXml);
		}

		#endregion

		#region Implementation

		protected abstract IZType NewZ(object value);
		protected abstract object[] UnsupportedValues { get; }
		protected abstract object[] ValidValues { get; }

		protected IZType AnyZ
		{
			get { return NewZ(ValidValues[0]); }
		}

		protected virtual object[] InvalidValues
		{
			get { return Array.Empty<object>(); }
		}

		protected virtual object[] EmptyValues
		{
			get { return Array.Empty<object>(); }
		}

		protected virtual object[] AllValues
		{
			get
			{
				ArrayList result = new ArrayList();
				result.Add(AnyZ.Default);
				result.AddRange(EmptyValues);
				result.AddRange(InvalidValues);
				result.AddRange(ValidValues);
				foreach (object value in ValidValues)
				{
					result.Add(NewZ(value));
				}

				return result.ToArray();
			}
		}

		protected Type UsualValueType
		{
			get { return ValidValues[0].GetType(); }
		}

		protected virtual bool ValueIsUsualType(object value)
		{
			Type valueType = value == null ? null : value.GetType();
			return UsualValueType == valueType || AnyZ.Default.GetType() == valueType;
		}

		protected virtual bool IsValueThatWillCreateEmptyZType(object value)
		{
			if (value is IZType && value.GetType() == AnyZ.GetType())
			{
				return ((IZType)value).IsEmpty;
			}
			else
			{
				return ArrayContainsValue(EmptyValues, value);
			}
		}

		protected virtual bool ArrayContainsValue(Array values, object value)
		{
			foreach (object emptyValue in values)
			{
				if (value == null)
				{
					if (emptyValue == null)
					{
						return true;
					}
				}
				else if (value.Equals(emptyValue))
				{
					return true;
				}
			}

			return false;
		}

		protected virtual bool ValueIsValid(object value)
		{
			return true;
		}

		protected string MessageForValue(object value)
		{
			string message = "<null>";
			if (value != null)
			{
				message = "<" + value + "> of type (" + value.GetType() + ")";
			}

			return "With value " + message;
		}

		#endregion
	}
}
