using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	class ZCodeMappedZStringTest : TestCase
	{
		public void TestConstructor()
		{
			var mappedString = new ZCodeMappedZString();
			Assert("SourceValue", mappedString.SourceValue.IsEmpty);
			Assert("MappedValue", !mappedString.MappedValue.HasValue);

			var expectedValue = "some code";
			mappedString = new ZCodeMappedZString(expectedValue);
			AssertEquals("SourceValue", expectedValue, mappedString.SourceValue);
			Assert("MappedValue", !mappedString.MappedValue.HasValue);
		}

		public void TestIsMapped()
		{
			var mappedString = new ZCodeMappedZString();
			Assert(!mappedString.IsMapped);

			mappedString = new ZCodeMappedZString("code");
			Assert(!mappedString.IsMapped);

			mappedString.MappedValue = "something";
			Assert(mappedString.IsMapped);
		}

		public void TestImplicitCastToZString()
		{
			var mappedString = new ZCodeMappedZString();
			ZString mappedStringAsZString = mappedString;
			AssertEquals("", mappedStringAsZString);

			ZString expectedValue = "some code";
			mappedString = new ZCodeMappedZString(expectedValue);
			mappedStringAsZString = mappedString;
			AssertEquals(expectedValue, mappedStringAsZString);

			expectedValue = "something";
			mappedString.MappedValue = "something";
			AssertNotEquals("Precondition", mappedString.MappedValue, mappedString.SourceValue);
			mappedStringAsZString = mappedString;
			AssertEquals(expectedValue, mappedStringAsZString);
		}

		public void TestImplicitCastToString()
		{
			var mappedString = new ZCodeMappedZString();
			string mappedStringAsZString = mappedString;
			AssertEquals("", mappedStringAsZString);

			string expectedValue = "some code";
			mappedString = new ZCodeMappedZString(expectedValue);
			mappedStringAsZString = mappedString;
			AssertEquals(expectedValue, mappedStringAsZString);

			expectedValue = "something";
			AssertNotEquals("Precondition", expectedValue, mappedString.SourceValue);
			mappedString.MappedValue = expectedValue;
			mappedStringAsZString = mappedString;
			AssertEquals(expectedValue, mappedStringAsZString);
		}

		public void TestImplicitCastFromZString()
		{
			ZString expectedValue = "";
			ZCodeMappedZString mappedString = expectedValue;
			AssertEquals("SourceValue", expectedValue, mappedString.SourceValue);
			Assert("MappedValue", !mappedString.MappedValue.HasValue);

			expectedValue = "some code";
			mappedString = expectedValue;
			AssertEquals("SourceValue", expectedValue, mappedString.SourceValue);
			Assert("MappedValue", !mappedString.MappedValue.HasValue);
		}

		public void TestImplicitCastFromString()
		{
			string expectedValue = null;
			ZCodeMappedZString mappedString = expectedValue;
			AssertEquals("SourceValue", "", mappedString.SourceValue);
			Assert("MappedValue", !mappedString.MappedValue.HasValue);

			expectedValue = "";
			mappedString = expectedValue;
			AssertEquals("SourceValue", expectedValue, mappedString.SourceValue);
			Assert("MappedValue", !mappedString.MappedValue.HasValue);

			expectedValue = "some code";
			mappedString = expectedValue;
			AssertEquals("SourceValue", expectedValue, mappedString.SourceValue);
			Assert("MappedValue", !mappedString.MappedValue.HasValue);
		}

		public void TestToString()
		{
			var mappedString = new ZCodeMappedZString();
			AssertEquals("", mappedString.ToString());

			string expectedValue = "some code";
			mappedString = new ZCodeMappedZString(expectedValue);
			AssertEquals(expectedValue, mappedString.ToString());

			expectedValue = "something";
			AssertNotEquals("Precondition", expectedValue, mappedString.SourceValue);
			mappedString.MappedValue = expectedValue;
			AssertEquals(expectedValue, mappedString.ToString());
		}

		public void TestEqualsWithTheSameObjectType()
		{
			var mappedString1 = new ZCodeMappedZString();
			var mappedString2 = new ZCodeMappedZString();
			Assert(mappedString1.Equals(mappedString2));
			Assert(mappedString2.Equals(mappedString1));
			AssertEquals(mappedString1, mappedString2);
			AssertEquals(mappedString2, mappedString1);
			Assert(mappedString1 == mappedString2);
			Assert(mappedString2 == mappedString1);
			Assert(!(mappedString1 != mappedString2));
			Assert(!(mappedString2 != mappedString1));

			string expectedValue = "some code";
			mappedString1 = new ZCodeMappedZString(expectedValue);
			mappedString2 = new ZCodeMappedZString(expectedValue);
			Assert(mappedString1.Equals(mappedString2));
			Assert(mappedString2.Equals(mappedString1));
			AssertEquals(mappedString1, mappedString2);
			AssertEquals(mappedString2, mappedString1);
			Assert(mappedString1 == mappedString2);
			Assert(mappedString2 == mappedString1);
			Assert(!(mappedString1 != mappedString2));
			Assert(!(mappedString2 != mappedString1));

			expectedValue = "something";
			AssertNotEquals("Precondition", expectedValue, mappedString1.SourceValue);
			mappedString1.MappedValue = expectedValue;
			Assert(!mappedString1.Equals(mappedString2));
			Assert(!mappedString2.Equals(mappedString1));
			AssertNotEquals(mappedString1, mappedString2);
			AssertNotEquals(mappedString2, mappedString1);
			Assert(!(mappedString1 == mappedString2));
			Assert(!(mappedString2 == mappedString1));
			Assert(mappedString1 != mappedString2);
			Assert(mappedString2 != mappedString1);

			mappedString2.SourceValue = expectedValue;
			Assert(mappedString1.Equals(mappedString2));
			Assert(mappedString2.Equals(mappedString1));
			AssertEquals(mappedString1, mappedString2);
			AssertEquals(mappedString2, mappedString1);
			Assert(mappedString1 == mappedString2);
			Assert(mappedString2 == mappedString1);
			Assert(!(mappedString1 != mappedString2));
			Assert(!(mappedString2 != mappedString1));

			expectedValue = "another something";
			AssertNotEquals("Precondition", expectedValue, mappedString1.SourceValue);
			AssertNotEquals("Precondition", expectedValue, mappedString2.SourceValue);
			mappedString1.MappedValue = expectedValue;
			mappedString2.MappedValue = expectedValue;
			Assert(mappedString1.Equals(mappedString2));
			Assert(mappedString2.Equals(mappedString1));
			AssertEquals(mappedString1, mappedString2);
			AssertEquals(mappedString2, mappedString1);
			Assert(mappedString1 == mappedString2);
			Assert(mappedString2 == mappedString1);
			Assert(!(mappedString1 != mappedString2));
			Assert(!(mappedString2 != mappedString1));

			mappedString1.MappedValue = null;
			mappedString2.MappedValue = null;
			mappedString1.SourceValue = "1";
			mappedString2.SourceValue = "2";
			Assert(!mappedString1.Equals(mappedString2));
			Assert(!mappedString2.Equals(mappedString1));
			AssertNotEquals(mappedString1, mappedString2);
			AssertNotEquals(mappedString2, mappedString1);
			Assert(!(mappedString1 == mappedString2));
			Assert(!(mappedString2 == mappedString1));
			Assert(mappedString1 != mappedString2);
			Assert(mappedString2 != mappedString1);
		}

		public void TestEqualsWithZString()
		{
			var mappedString = new ZCodeMappedZString();
			var zStringValue = new ZString();
			Assert(mappedString.Equals(zStringValue));
			AssertEquals(mappedString, zStringValue);
			AssertEquals(zStringValue, mappedString);
			Assert(mappedString == zStringValue);
			Assert(zStringValue == mappedString);
			Assert(!(mappedString != zStringValue));
			Assert(!(zStringValue != mappedString));

			ZString expectedValue = "some code";
			mappedString = new ZCodeMappedZString(expectedValue);
			zStringValue = expectedValue;
			Assert(mappedString.Equals(zStringValue));
			AssertEquals(mappedString, zStringValue);
			AssertEquals(zStringValue, mappedString);
			Assert(mappedString == zStringValue);
			Assert(zStringValue == mappedString);
			Assert(!(mappedString != zStringValue));
			Assert(!(zStringValue != mappedString));

			expectedValue = "something";
			AssertNotEquals("Precondition", expectedValue, mappedString.SourceValue);
			mappedString.MappedValue = expectedValue;
			Assert(!mappedString.Equals(zStringValue));
			AssertNotEquals(mappedString, zStringValue);
			AssertNotEquals(zStringValue, mappedString);
			Assert(!(mappedString == zStringValue));
			Assert(!(zStringValue == mappedString));
			Assert(mappedString != zStringValue);
			Assert(zStringValue != mappedString);

			zStringValue = expectedValue;
			Assert(mappedString.Equals(zStringValue));
			AssertEquals(mappedString, zStringValue);
			AssertEquals(zStringValue, mappedString);
			Assert(mappedString == zStringValue);
			Assert(zStringValue == mappedString);
			Assert(!(mappedString != zStringValue));
			Assert(!(zStringValue != mappedString));

			mappedString.MappedValue = null;
			mappedString.SourceValue = "1";
			zStringValue = "2";
			Assert(!mappedString.Equals(zStringValue));
			AssertNotEquals(mappedString, zStringValue);
			AssertNotEquals(zStringValue, mappedString);
			Assert(!(mappedString == zStringValue));
			Assert(!(zStringValue == mappedString));
			Assert(mappedString != zStringValue);
			Assert(zStringValue != mappedString);
		}

		public void TestEqualsWithString()
		{
			var mappedString = new ZCodeMappedZString();
			string stringValue = null;
			Assert(!mappedString.Equals(stringValue));
			AssertNotEquals(mappedString, stringValue);
			AssertNotEquals(stringValue, mappedString);
			Assert(!(mappedString == stringValue));
			Assert(!(stringValue == mappedString));
			Assert(mappedString != stringValue);
			Assert(stringValue != mappedString);

			stringValue = "";
			Assert(mappedString.Equals(stringValue));
			AssertEquals(mappedString, stringValue);
			AssertEquals(stringValue, mappedString);
			Assert(mappedString == stringValue);
			Assert(stringValue == mappedString);
			Assert(!(mappedString != stringValue));
			Assert(!(stringValue != mappedString));

			string expectedValue = "some code";
			mappedString = new ZCodeMappedZString(expectedValue);
			stringValue = expectedValue;
			Assert(mappedString.Equals(stringValue));
			AssertEquals(mappedString, stringValue);
			AssertEquals(stringValue, mappedString);
			Assert(mappedString == stringValue);
			Assert(stringValue == mappedString);
			Assert(!(mappedString != stringValue));
			Assert(!(stringValue != mappedString));

			expectedValue = "something";
			AssertNotEquals("Precondition", expectedValue, mappedString.SourceValue);
			mappedString.MappedValue = expectedValue;
			Assert(!mappedString.Equals(stringValue));
			AssertNotEquals(mappedString, stringValue);
			AssertNotEquals(stringValue, mappedString);
			Assert(!(mappedString == stringValue));
			Assert(!(stringValue == mappedString));
			Assert(mappedString != stringValue);
			Assert(stringValue != mappedString);

			stringValue = expectedValue;
			Assert(mappedString.Equals(stringValue));
			AssertEquals(mappedString, stringValue);
			AssertEquals(stringValue, mappedString);
			Assert(mappedString == stringValue);
			Assert(stringValue == mappedString);
			Assert(!(mappedString != stringValue));
			Assert(!(stringValue != mappedString));

			mappedString.MappedValue = null;
			mappedString.SourceValue = "1";
			stringValue = "2";
			Assert(!mappedString.Equals(stringValue));
			AssertNotEquals(mappedString, stringValue);
			AssertNotEquals(stringValue, mappedString);
			Assert(!(mappedString == stringValue));
			Assert(!(stringValue == mappedString));
			Assert(mappedString != stringValue);
			Assert(stringValue != mappedString);
		}

		public void TestGetHashCode()
		{
			var mappedString1 = new ZCodeMappedZString();
			var mappedString2 = new ZCodeMappedZString();
			AssertEquals(mappedString1.GetHashCode(), mappedString2.GetHashCode());

			mappedString1.SourceValue = "something";
			mappedString2.SourceValue = mappedString1.SourceValue;
			AssertEquals(mappedString1.GetHashCode(), mappedString2.GetHashCode());

			mappedString2.SourceValue += 1;
			AssertNotEquals(mappedString1.GetHashCode(), mappedString2.GetHashCode());

			mappedString2.MappedValue = mappedString1.SourceValue;
			AssertEquals(mappedString1.GetHashCode(), mappedString2.GetHashCode());

			mappedString1.MappedValue = "another something";
			mappedString2.MappedValue = mappedString1.MappedValue;
			AssertEquals(mappedString1.GetHashCode(), mappedString2.GetHashCode());

			mappedString1.SourceValue = "something";
			mappedString2.SourceValue = mappedString1.SourceValue;
			mappedString1.MappedValue = "another something";
			mappedString2.MappedValue = "other something";
			AssertNotEquals(mappedString1.GetHashCode(), mappedString2.GetHashCode());
		}
	}
}
