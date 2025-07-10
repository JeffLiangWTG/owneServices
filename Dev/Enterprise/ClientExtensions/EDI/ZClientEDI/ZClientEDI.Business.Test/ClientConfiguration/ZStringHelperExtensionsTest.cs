using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test
{
	class ZStringHelperExtensionsTest : TestCase
	{
		public void CapitaliseFirstLettersOfWords()
		{
			ZString input = "MILK o'tea";
			AssertEquals("Milk O'Tea", input.CapitaliseFirstLettersOfWords());
		}

		public void TestEmptyString()
		{
			AssertEquals(ZString.Empty, ZString.Empty.CapitaliseFirstLettersOfWords());
		}

		public void TestDoubleLeadingAndTrailingSpaces()
		{
			AssertEquals(" Hello  Mum ", new ZString(" hello  mum ").CapitaliseFirstLettersOfWords());
		}

		public void TestCleanUpTextForHTMLWithoutTagEscaping()
		{
			ZString input =
@"Thank you for contacting CargoWise Customer Service.

This email is confirmation that we have received your request below.

	Your Incident Number is: CS0029402

Please quote this Incident Number in any future correspondence.

<<INVALID TAG>>";

			ZString expectedOutput = "Thank you for contacting CargoWise Customer Service.<br /><br />This email is confirmation that we have received your request below.<br /><br />&nbsp;&nbsp;&nbsp;&nbsp;Your Incident Number is: CS0029402<br /><br />Please quote this Incident Number in any future correspondence.<br /><br />&lt;<INVALID TAG>&gt;";

			AssertEquals(expectedOutput, input.CleanUpTextForHTMLWithoutTagEscaping());

			input = "<div> is better than <span>Line 1\nLine2\n\nLine3\r\nLine4";
			expectedOutput = "<div> is better than <span>Line 1<br />Line2<br /><br />Line3<br />Line4";
			AssertEquals(expectedOutput, input.CleanUpTextForHTMLWithoutTagEscaping());
		}

		public void TestCleanUpTextForHTML()
		{
			ZString input = "<div> is better than <span>Line 1\nLine2\n\nLine3\r\nLine4";
			ZString expectedOutput = "&lt;div&gt; is better than &lt;span&gt;Line 1<br />Line2<br /><br />Line3<br />Line4";
			AssertEquals(expectedOutput, input.CleanUpTextForHTML());
		}

		public void TestNormaliseWhitespaceCharactersForHtml()
		{
			ZString input = "Line one\n\tLine two\r\t\tLine three\r\nLine four";
			ZString expectedOutput = "Line one<br />&nbsp;&nbsp;&nbsp;&nbsp;Line two<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Line three<br />Line four";
			AssertEquals(expectedOutput, input.NormaliseWhitespaceCharactersForHtml());
		}

		public void TestNormaliseNewLine()
		{
			ZString input = "Line one\nLine two\rLine three\r\nLine four\r\n\r\nLine five\r\r\nLine six\r\n\nLine seven\r\n\rLine eight\n\r\nLine nine\n\n\r";
			ZString expectedOutput = "Line one\r\nLine two\r\nLine three\r\nLine four\r\n\r\nLine five\r\n\r\nLine six\r\n\r\nLine seven\r\n\r\nLine eight\r\n\r\nLine nine\r\n\r\n\r\n";
			AssertEquals(expectedOutput, input.NormaliseNewLine());
		}

		public void TestSplitCamelCase()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Hello World", new ZString("Hello World").SplitCamelCase());
				AssertEquals("cased Word HTTP Writer", new ZString("casedWordHTTPWriter").SplitCamelCase());
				AssertEquals("nothing", new ZString("nothing").SplitCamelCase());
				AssertEquals("ABBA", new ZString("ABBA").SplitCamelCase());
				AssertEquals("Computer", new ZString("Computer").SplitCamelCase());
				AssertEquals("a Computer", new ZString("aComputer").SplitCamelCase());
				AssertEquals("an Apple Computer", new ZString("anAppleComputer").SplitCamelCase());
				AssertEquals("an IBM", new ZString("anIBM").SplitCamelCase());
				AssertEquals("an IBM Computer", new ZString("anIBMComputer").SplitCamelCase());
				AssertEquals("Apple Computer", new ZString("AppleComputer").SplitCamelCase());
				AssertEquals("IBM Computer", new ZString("IBMComputer").SplitCamelCase());
				AssertEquals("One 123 Two 789", new ZString("One123Two789").SplitCamelCase());
				AssertEquals("HTTP Writer", new ZString("HTTPWriter").SplitCamelCase());
				AssertEquals("HTTP Writer 2", new ZString("HTTPWriter2").SplitCamelCase());
			});
		}
	}
}