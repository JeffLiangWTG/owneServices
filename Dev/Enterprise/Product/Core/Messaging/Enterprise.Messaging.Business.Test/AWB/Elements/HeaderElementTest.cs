using NUnit.Framework;

namespace Enterprise.Messaging.Business.AWB.Testing
{
	sealed class HeaderElementTest : TestCase
	{
		public void TestConstructor()
		{
			HeaderElement element = new HeaderElement(StatusType.Conditional, new ElementList());
			AssertEquals(StatusType.Conditional, element.Status);
			AssertNotNull(element.Elements);
		}

		public void TestToString()
		{
			ElementList elements = new ElementList();
			elements.AddValue(new Format(3, CharType.Alpha), "VAL");
			elements.AddSlant();
			elements.AddValue(new Format(4, CharType.Alpha), "VALA");

			HeaderElement element = new HeaderElement(StatusType.Mandatory, elements);
			AssertEquals("VAL/VALA", element.ToString());

			elements = new ElementList();
			elements.AddValue(new Format(3, CharType.Alpha), "VAL");
			elements.AddSlant();
			elements.AddValue(new Format(4, CharType.AlphaNumeric), "VAL2");

			element = new HeaderElement(StatusType.Optional, elements);
			AssertEquals("VAL/VAL2", element.ToString());

			elements = new ElementList();
			elements.AddValue(new Format(3, CharType.Alpha), "");
			elements.AddSlant();
			elements.AddValue(new Format(4, CharType.Alpha), "");

			element = new HeaderElement(StatusType.Optional, elements);
			AssertEquals("", element.ToString());
		}
	}
}
