using NUnit.Framework;

namespace Enterprise.Messaging.Business.AWB.Testing
{
	sealed class ValueElementTest : TestCase
	{
		public void TestConstructors()
		{
			ValueElement element = new ValueElement(StatusType.Optional, new Format(3, CharType.Alpha), "VAL", ValueType.Special);
			AssertEquals(StatusType.Optional, element.Status);
			AssertEquals("VAL", element.Value);
			AssertEquals(ValueType.Special, element.ValueType);

			element = new ValueElement(new Format(3, CharType.Alpha), "VAL");
			AssertEquals(StatusType.Mandatory, element.Status);
			AssertEquals("VAL", element.Value);
			AssertEquals(ValueType.Value, element.ValueType);

			element = new ValueElement(new Format(3, CharType.Alpha), "VAL", ValueType.ColumnIdentifier);
			AssertEquals(StatusType.Mandatory, element.Status);
			AssertEquals("VAL", element.Value);
			AssertEquals(ValueType.ColumnIdentifier, element.ValueType);
		}

		public void TestToString()
		{
			ValueElement element = new ValueElement(new Format(3, CharType.Alpha), "val");
			AssertEquals("VAL", element.ToString());
		}

		public void ToStringValueTypes()
		{
			ValueElement element = new ValueElement(new Format(3, CharType.Alpha), "val");
			AssertEquals("VAL", element.ToStringValueTypes());
			element = new ValueElement(new Format(1, CharType.Special), SpecialChars.Slant, ValueType.Special);
			AssertEquals("", element.ToStringValueTypes());

			element = new ValueElement(new Format(1, 0, CharType.Numeric), "0", ValueType.Value);
			AssertEquals("", element.ToStringValueTypes());
		}
	}
}
