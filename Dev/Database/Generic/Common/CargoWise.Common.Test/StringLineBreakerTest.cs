using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class StringLineBreakerTest : TestCase
	{
		public void TestNoLineBreakOnCR()
		{
			StringLineBreaker stringLineBreaker = new StringLineBreaker("123456789-12::3456789-\r\n123456789-\r\n1234++56789 12345``6789 123456789 ");
			AssertEquals("Line 1", "123456789-", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 2", "123456789-", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 3", "123456789-", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 4", "123456789", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 5", "12345", stringLineBreaker.GetNextLine(5));
			AssertEquals("Line 6", "6789", stringLineBreaker.GetNextLine(5));
			AssertEquals("Line 7", "123456789", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 8", "", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 9", "", stringLineBreaker.GetNextLine(10));
		}

		public void TestIsEmpty()
		{
			StringLineBreaker stringLineBreaker1 = new StringLineBreaker("");
			AssertEquals(stringLineBreaker1.IsEmpty(), true);
			StringLineBreaker stringLineBreaker2 = new StringLineBreaker("HASVALUE");
			AssertEquals(stringLineBreaker2.IsEmpty(), false);
		}

		public void TestAddressBreakup()
		{
			StringLineBreaker breaker = new StringLineBreaker("COMPANYNAME\r\nADDRESS1\r\n\r\nCITY\r\n", true);
			AssertEquals("Line 1", "COMPANYNAME", breaker.GetNextLine(35));
			AssertEquals("Line 2", "ADDRESS1", breaker.GetNextLine(35));
			AssertEquals("Line 3", "CITY", breaker.GetNextLine(35));
			AssertEquals("Line 4", "", breaker.GetNextLine(35));
		}

		public void TestLineBreakOnCR()
		{
			StringLineBreaker stringLineBreaker = new StringLineBreaker("123456789-12::345\r\n6789-\r\n123456789-\r\n1234++56789 12345``6789 1234\r\n56789 ", true);
			AssertEquals("Line 1", "123456789-", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 2", "12345", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 3", "6789-", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 4", "123456789-", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 5", "123456789", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 6", "123456789", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 7", "1234", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 8", "56789", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 9", "", stringLineBreaker.GetNextLine(10));
		}

		public void TestAllowedCharacters()
		{
			StringLineBreaker stringLineBreaker = new StringLineBreaker("123456789-12::345\r\n6789-\r\n123456789-\r\n1234++56789 12345``6789 1234\r\n56789 ", true, "12345");
			AssertEquals("Line 1", "1234512345", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 2", "12345", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 3", "12345", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 4", "12345 1234", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 5", "5", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 6", "", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 7", "", stringLineBreaker.GetNextLine(10));
			AssertEquals("Line 8", "", stringLineBreaker.GetNextLine(10));
		}
	}
}