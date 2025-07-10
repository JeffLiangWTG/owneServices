using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.AWB.Testing
{
	sealed class ElementListTest : TestCase
	{
		public void TestAdd()
		{
			ElementList elements = new ElementList();
			AssertEquals(0, elements.Count);

			elements.Add(new Element(StatusType.Mandatory));
			AssertEquals(1, elements.Count);
			elements.Add(new Element(StatusType.Mandatory));
			AssertEquals(2, elements.Count);
		}

		public void TestLineIdentifier()
		{
			ElementList elements = new ElementList();
			elements.AddLineIdentifier("XYZ");
			AssertEquals("XYZ", ((ValueElement)elements[0]).Value);
			AssertEquals(StatusType.Mandatory, elements[0].Status);
			AssertEquals(ValueType.LineIdentifier, ((ValueElement)elements[0]).ValueType);
		}

		public void TestColumnIdentifier()
		{
			ElementList elements = new ElementList();
			elements.AddColumnIdentifier("X");
			AssertEquals("X", ((ValueElement)elements[0]).Value);
			AssertEquals(StatusType.Mandatory, elements[0].Status);
			AssertEquals(ValueType.ColumnIdentifier, ((ValueElement)elements[0]).ValueType);
		}

		public void TestAddSlant()
		{
			ElementList elements = new ElementList();
			elements.AddSlant();
			AssertEquals("/", ((ValueElement)elements[0]).Value);
			AssertEquals(StatusType.Mandatory, elements[0].Status);
			AssertEquals(ValueType.Special, ((ValueElement)elements[0]).ValueType);
		}

		public void TestAddSlantStatus()
		{
			ElementList elements = new ElementList();
			elements.AddSlant(StatusType.Conditional);
			AssertEquals("/", ((ValueElement)elements[0]).Value);
			AssertEquals(StatusType.Conditional, elements[0].Status);
			AssertEquals(ValueType.Special, ((ValueElement)elements[0]).ValueType);
		}

		public void TestAddHyphen()
		{
			ElementList elements = new ElementList();
			elements.AddHyphen();
			AssertEquals("-", ((ValueElement)elements[0]).Value);
			AssertEquals(StatusType.Mandatory, elements[0].Status);
			AssertEquals(ValueType.Special, ((ValueElement)elements[0]).ValueType);
		}

		public void TestAddCRLF()
		{
			ElementList elements = new ElementList();
			elements.AddCRLF();
			AssertEquals("\r\n", ((ValueElement)elements[0]).Value);
			AssertEquals(StatusType.Mandatory, elements[0].Status);
			AssertEquals(ValueType.Special, ((ValueElement)elements[0]).ValueType);
		}

		public void TestAddValue()
		{
			ElementList elements = new ElementList();
			elements.AddValue(new Format(3, CharType.Alpha), "VAL");
			AssertEquals("VAL", ((ValueElement)elements[0]).Value);
			AssertEquals(StatusType.Mandatory, elements[0].Status);
			AssertEquals(ValueType.Value, ((ValueElement)elements[0]).ValueType);

			elements = new ElementList();
			elements.AddValue(new Format(2, CharType.Alpha), (ZInt)10);
			AssertEquals("10", ((ValueElement)elements[0]).Value);
			AssertEquals(StatusType.Mandatory, elements[0].Status);
			AssertEquals(ValueType.Value, ((ValueElement)elements[0]).ValueType);

			elements = new ElementList();
			elements.AddValue(new Format(12, 0, CharType.NumericWithDecimal), 10.45M);
			AssertEquals("10.45", ((ValueElement)elements[0]).Value);
			AssertEquals(StatusType.Mandatory, elements[0].Status);
			AssertEquals(ValueType.Value, ((ValueElement)elements[0]).ValueType);

			elements = new ElementList();
			elements.AddValue(new Format(12, 0, CharType.NumericWithDecimal), 10.40M);
			AssertEquals("10.4", ((ValueElement)elements[0]).Value);
			AssertEquals(StatusType.Mandatory, elements[0].Status);
			AssertEquals(ValueType.Value, ((ValueElement)elements[0]).ValueType);

			elements = new ElementList();
			elements.AddValue(new Format(12, 0, CharType.NumericWithDecimal), 10M);
			AssertEquals("10", ((ValueElement)elements[0]).Value);
			AssertEquals(StatusType.Mandatory, elements[0].Status);
			AssertEquals(ValueType.Value, ((ValueElement)elements[0]).ValueType);

			elements = new ElementList();
			elements.AddOptionalValue(new Format(3, CharType.Alpha), "VAL");
			AssertEquals("VAL", ((ValueElement)elements[0]).Value);
			AssertEquals(StatusType.Optional, elements[0].Status);
			AssertEquals(ValueType.Value, ((ValueElement)elements[0]).ValueType);

			elements = new ElementList();
			elements.AddConditionalValue(new Format(3, CharType.Alpha), "VAL");
			AssertEquals("VAL", ((ValueElement)elements[0]).Value);
			AssertEquals(StatusType.Conditional, elements[0].Status);
			AssertEquals(ValueType.Value, ((ValueElement)elements[0]).ValueType);
		}

		public void TestAddHeader()
		{
			ElementList elements = new ElementList();
			ElementList headerElements = new ElementList();
			headerElements.AddSlant();
			elements.AddHeader(headerElements);
			AssertEquals(StatusType.Mandatory, elements[0].Status);
			AssertEquals(1, ((HeaderElement)elements[0]).Elements.Count);
		}

		public void TestAddOptionalHeader()
		{
			ElementList elements = new ElementList();
			ElementList headerElements = new ElementList();
			headerElements.AddSlant();
			elements.AddOptionalHeader(headerElements);
			AssertEquals(StatusType.Optional, elements[0].Status);
			AssertEquals(1, ((HeaderElement)elements[0]).Elements.Count);
		}

		public void TestAddConditionalHeader()
		{
			ElementList elements = new ElementList();
			ElementList headerElements = new ElementList();
			headerElements.AddSlant();
			elements.AddConditionalHeader(headerElements);
			AssertEquals(StatusType.Conditional, elements[0].Status);
			AssertEquals(1, ((HeaderElement)elements[0]).Elements.Count);
		}

		public void TestToString()
		{
			var elements = new ElementList();
			elements.AddSlant();
			elements.AddValue(new Format(3, CharType.Alpha), "VAL");
			elements.AddSlant(StatusType.Mandatory);
			elements.AddOptionalValue(new Format(4, CharType.Alpha), "VALA");
			AssertEquals("/VAL/VALA", elements.ToString());

			elements.AddSlant(StatusType.Mandatory);
			elements.AddOptionalValue(new Format(3, CharType.Alpha), "");
			AssertEquals("/VAL/VALA/", elements.ToString());

			elements.AddSlant(StatusType.Conditional);
			elements.AddConditionalValue(new Format(3, CharType.Alpha), "");
			AssertEquals("/VAL/VALA/", elements.ToString());

			elements.AddSlant(StatusType.Conditional);
			elements.AddConditionalValue(new Format(3, CharType.Alpha), "XYZ");
			AssertEquals("/VAL/VALA///XYZ", elements.ToString());

			elements.AddSlant(StatusType.Conditional);
			elements.AddConditionalValue(new Format(3, CharType.Alpha), "ABC");
			elements.AddConditionalValue(new Format(3, CharType.Alpha), "DEF");
			elements.AddHyphen();
			AssertEquals("/VAL/VALA///XYZ/ABCDEF-", elements.ToString());

			elements.AddValue(new Format(3, CharType.Alpha), "XXX");
			AssertEquals("/VAL/VALA///XYZ/ABCDEF-XXX", elements.ToString());

			elements.AddConditionalValue(new Format(3, CharType.Alpha), "YYY");
			elements.AddSlant(StatusType.Mandatory);
			AssertEquals("Conditional value YYY after a mandatory value XXX, before a mandatory slant", "/VAL/VALA///XYZ/ABCDEF-XXXYYY/", elements.ToString());
			elements.AddValue(new Format(3, CharType.Alpha), "ZZZ");

			elements.AddSlant(StatusType.Conditional);
			elements.AddSlant(StatusType.Conditional);
			elements.AddConditionalValue(new Format(3, CharType.Alpha), "");
			elements.AddConditionalValue(new Format(4, CharType.Alpha), "GHI");
			AssertEquals("conditional slants should be added", "/VAL/VALA///XYZ/ABCDEF-XXXYYY/ZZZ//GHI", elements.ToString());
		}

		public void TestMandatoryValue()
		{
			var elements = new ElementList();
			elements.AddSlant();
			elements.AddColumnIdentifier("T");
			ZInt totalNoOfPieces = 6;
			elements.AddValue(new Format(4, 0, CharType.Numeric), totalNoOfPieces);
			elements.AddValue(new Format(1, CharType.Alpha), "K");
			ZDecimal totalGrossWeight = 0.0m;
			elements.AddValue(new Format(7, 0, CharType.NumericWithDecimal), totalGrossWeight);

			AssertEquals("/T6K0", elements.ToString());
		}

		public void TestOptionalMessageParts()
		{
			var allElements = new ElementList();
			allElements.AddValue(new Format(3, CharType.Alpha), "SHA");
			allElements.AddHyphen();
			allElements.AddValue(new Format(8, CharType.AlphaNumeric), "12345678");
			allElements.AddHyphen(StatusType.Optional);
			allElements.AddOptionalValue(new Format(12, CharType.AlphaNumeric), "HB1");
			allElements.AddHyphen(StatusType.Optional);
			allElements.AddOptionalValue(new Format(1, CharType.Alpha), "A");
			allElements.AddCRLF();

			var expected = @"SHA-12345678-HB1-A
";
			AssertEquals("Elements marked as Optional must appear in the message when followed by a value", expected, allElements.ToString());

			var minimumElements = new ElementList();
			minimumElements.AddValue(new Format(3, CharType.Alpha), "SHA");
			minimumElements.AddHyphen();
			minimumElements.AddValue(new Format(8, CharType.AlphaNumeric), "12345678");
			minimumElements.AddHyphen(StatusType.Optional);
			minimumElements.AddOptionalValue(new Format(12, CharType.AlphaNumeric), "");
			minimumElements.AddHyphen(StatusType.Optional);
			minimumElements.AddOptionalValue(new Format(1, CharType.Alpha), "");
			minimumElements.AddCRLF();

			expected = @"SHA-12345678
";
			AssertEquals(expected, minimumElements.ToString());

			var missingElement = new ElementList();
			missingElement.AddValue(new Format(3, CharType.Alpha), "SHA");
			missingElement.AddHyphen();
			missingElement.AddValue(new Format(8, CharType.AlphaNumeric), "12345678");
			missingElement.AddHyphen(StatusType.Optional);
			missingElement.AddOptionalValue(new Format(12, CharType.AlphaNumeric), "");
			missingElement.AddHyphen(StatusType.Optional);
			missingElement.AddOptionalValue(new Format(1, CharType.Alpha), "A");
			missingElement.AddCRLF();

			expected = @"SHA-12345678-A
";
			AssertEquals("Elements marked as Optional are excluded from the message when empty", expected, missingElement.ToString());
		}

		public void TestConditionalMessageParts()
		{
			var allElements = new ElementList();
			allElements.AddLineIdentifier("TRN");
			allElements.AddSlant(StatusType.Mandatory);
			allElements.AddValue(new Format(3, CharType.AlphaNumeric), "LAX");
			allElements.AddHyphen(StatusType.Conditional);
			allElements.AddConditionalValue(new Format(1, CharType.Alpha), "D");
			allElements.AddSlant(StatusType.Conditional);
			allElements.AddConditionalValue(new Format(12, CharType.Text), "13-150279800");
			allElements.AddSlant(StatusType.Conditional);
			allElements.AddConditionalValue(new Format(9, CharType.AlphaNumeric), "1234");
			allElements.AddCRLF();

			var expected = @"TRN/LAX-D/13-150279800/1234
";
			AssertEquals("Elements marked as Conditional must appear in the message when followed by a value", expected, allElements.ToString());

			var minimumElements = new ElementList();
			minimumElements.AddLineIdentifier("TRN");
			minimumElements.AddSlant(StatusType.Mandatory);
			minimumElements.AddValue(new Format(3, CharType.AlphaNumeric), "000");
			minimumElements.AddHyphen(StatusType.Conditional);
			minimumElements.AddConditionalValue(new Format(1, CharType.Alpha), "");
			minimumElements.AddSlant(StatusType.Conditional);
			minimumElements.AddConditionalValue(new Format(12, CharType.Text), "");
			minimumElements.AddSlant(StatusType.Conditional);
			minimumElements.AddConditionalValue(new Format(9, CharType.AlphaNumeric), "");
			minimumElements.AddCRLF();

			expected = @"TRN/000
";
			AssertEquals("Elements marked as Conditional are excluded from the message when all are empty", expected, minimumElements.ToString());

			var missingElement = new ElementList();
			missingElement.AddLineIdentifier("TRN");
			missingElement.AddSlant(StatusType.Mandatory);
			missingElement.AddValue(new Format(3, CharType.AlphaNumeric), "LAX");
			missingElement.AddHyphen(StatusType.Conditional);
			missingElement.AddConditionalValue(new Format(1, CharType.Alpha), "D");
			missingElement.AddSlant(StatusType.Conditional);
			missingElement.AddConditionalValue(new Format(12, CharType.Text), "");
			missingElement.AddSlant(StatusType.Conditional);
			missingElement.AddConditionalValue(new Format(9, CharType.AlphaNumeric), "1234");
			missingElement.AddCRLF();

			expected = @"TRN/LAX-D//1234
";
			AssertEquals("Elements marked as Conditional are included in the message when following conditional elements have a value", expected, missingElement.ToString());
		}

		public void TestProcessMixedList()
		{
			var elementsList1 = new ElementList();
			elementsList1.AddValue(new Format(3, CharType.Alpha), "SHA");
			elementsList1.AddHyphen();
			elementsList1.AddValue(new Format(8, CharType.AlphaNumeric), "12345678");
			elementsList1.AddHyphen(StatusType.Optional);
			elementsList1.AddOptionalValue(new Format(12, CharType.AlphaNumeric), "HB1");
			elementsList1.AddHyphen(StatusType.Conditional);
			elementsList1.AddConditionalValue(new Format(1, CharType.Alpha), "A");
			elementsList1.AddCRLF();

			var elementsList2 = new ElementList();
			elementsList2.AddLineIdentifier("TRN");
			elementsList2.AddSlant(StatusType.Mandatory);
			elementsList2.AddValue(new Format(3, CharType.AlphaNumeric), "LAX");
			elementsList2.AddHyphen(StatusType.Conditional);
			elementsList2.AddConditionalValue(new Format(1, CharType.Alpha), "D");
			elementsList2.AddSlant(StatusType.Conditional);
			elementsList2.AddConditionalValue(new Format(12, CharType.Text), "13-150279800");
			elementsList2.AddSlant(StatusType.Conditional);
			elementsList2.AddConditionalValue(new Format(9, CharType.AlphaNumeric), "1234");
			elementsList2.AddCRLF();

			var headerElementsList = new ElementList();
			headerElementsList.AddValue(new Format(8, CharType.AlphaNumeric), "FRC");
			headerElementsList.AddCRLF();
			headerElementsList.AddHeader(elementsList1);
			headerElementsList.AddHeader(elementsList2);
			headerElementsList.AddLineIdentifier("FDA");
			headerElementsList.AddCRLF();

			var expected = @"FRC
SHA-12345678-HB1-A
TRN/LAX-D/13-150279800/1234
FDA
";
			AssertEquals(expected, headerElementsList.ToString());
		}

		public void TestToStringValueTypes()
		{
			ElementList elements = new ElementList();
			elements.AddSlant();
			elements.AddValue(new Format(3, CharType.Alpha), "VAL");
			elements.AddSlant();
			elements.AddOptionalValue(new Format(4, CharType.Alpha), "VALA");
			AssertEquals("VALVALA", elements.ToStringValueTypes());

			elements.AddSlant(StatusType.Optional);
			elements.AddOptionalValue(new Format(3, CharType.Alpha), "");
			AssertEquals("VALVALA", elements.ToStringValueTypes());
			AssertEquals("/VAL/VALA", elements.ToString());
		}
	}
}
