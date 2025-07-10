using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(TextGroup))]
	sealed class TextGroupTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			// Arrange
			// Act
			// Assert
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("TextGroup", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<FormatPhoneNumber meh>", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<TextGroup(\"8615601131981\", \"blah\", \"-\")>", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<TextGroup(\"8615601131981\", 3, \"-\", \"e\")>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<TextGroup(\"0001000200\",4)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<TextGroup(\"0001000200\",4,L)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<TextGroup(\"0001000200\",4,R)>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			// Arrange
			// Act
			var validEmptyText = ValueProviderToTest.GetReplacement("<TextGroup(\"\",4)>", Report);
			var validTextCode = ValueProviderToTest.GetReplacement("<TextGroup ( \"RF18539007547034\", 4 )>", Report);
			var validTextCodeWithDefaultDelimiterLDirection = ValueProviderToTest.GetReplacement("<TextGroup(\"0001000200\",4)>", Report);
			var validTextCodeWithDefaultDelimiterLDirection2 = ValueProviderToTest.GetReplacement("<TextGroup(\"0001\",4)>", Report);
			var validTextCodeWithDefaultDelimiterLDirection3 = ValueProviderToTest.GetReplacement("<TextGroup(\"00\",4)>", Report);
			var validTextCodeWithDefaultDelimiterLDirection4 = ValueProviderToTest.GetReplacement("<TextGroup(\"000100020\",3)>", Report);
			var validTextCodeWithDefaultMinusLDirection = ValueProviderToTest.GetReplacement("<TextGroup(\"0001000200\",3,\"-\")>", Report);
			var validTextCodeWithDefaultMinusLDirection2 = ValueProviderToTest.GetReplacement("<TextGroup(\"0001000200\",4,\"-\",L)>", Report);
			var validTextCodeWithRDirection = ValueProviderToTest.GetReplacement("<TextGroup(\"299999000000000000100000012\",5,\" \",R)>", Report);
			var validTextCodeWithDefaultMinusRDirection = ValueProviderToTest.GetReplacement("<TextGroup(\"9001000200\",3,\"-\",R)>", Report);
			var validTextCodeWithDefaultMinusRDirection2 = ValueProviderToTest.GetReplacement("<TextGroup(\"901000200\",3,\"-\",R)>", Report);
			var validTextCodeWithDefaultMinusRDirection3 = ValueProviderToTest.GetReplacement("<TextGroup(\"9001\",4,\"-\",R)>", Report);
			// Assert
			AssertEquals("Valid empty text", String.Empty, validEmptyText);
			AssertEquals("Valid text group space separated L direction", "RF18 5390 0754 7034", validTextCode);
			AssertEquals("Valid text group space separated L direction", "0001 0002 00", validTextCodeWithDefaultDelimiterLDirection);
			AssertEquals("Valid text group space separated L direction", "0001", validTextCodeWithDefaultDelimiterLDirection2);
			AssertEquals("Valid text group space separated L direction", "00", validTextCodeWithDefaultDelimiterLDirection3);
			AssertEquals("Valid text group space separated L direction", "000 100 020", validTextCodeWithDefaultDelimiterLDirection4);
			AssertEquals("Valid text group minus separated L direction", "000-100-020-0", validTextCodeWithDefaultMinusLDirection);
			AssertEquals("Valid text group minus separated L direction", "0001-0002-00", validTextCodeWithDefaultMinusLDirection2);
			AssertEquals("Valid text group R direction", "29 99990 00000 00000 01000 00012", validTextCodeWithRDirection);
			AssertEquals("Valid text group minus separated R direction 1", "9-001-000-200", validTextCodeWithDefaultMinusRDirection);
			AssertEquals("Valid text group minus separated R direction 2", "901-000-200", validTextCodeWithDefaultMinusRDirection2);
			AssertEquals("Valid text group minus separated R direction 3", "9001", validTextCodeWithDefaultMinusRDirection3);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new TextGroup();
		}
	}
}
