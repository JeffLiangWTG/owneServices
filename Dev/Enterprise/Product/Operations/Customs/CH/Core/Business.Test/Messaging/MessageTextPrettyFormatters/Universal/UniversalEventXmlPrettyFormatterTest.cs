using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class UniversalEventXmlPrettyFormatterTest : TestCaseWithFactory
{
	public void TestGetFormattedText() => CombineAssertions(() =>
	{
		const string message = "<?xml?>\r\n<message />";
		AssertContains("Formatted text", expectedXmlParsingErrorMessage.ReplaceNonBreakingSpaceWithNormalSpace(), GetFormatter(message).GetFormattedText());
	});

	public void TestGetFormattedText_null() => CombineAssertions(() =>
	{
		var formatter = new UniversalEventXmlPrettyFormatter(null);
		var formattedText = formatter.GetFormattedText();
		AssertEquals(ZString.Empty, formattedText);
	});

	public void TestGetFormattedText_hasXmlDeclCRLF() => CombineAssertions(() =>
	{
		AssertEquals("No XML declaration", $"{XmlDecl}\r\n<ntxxx />", GetFormatter("<ntxxx />").GetFormattedText());
		AssertEquals("XML declaration not ending with CRLF", $"{XmlDecl}\r\n<ntxxx />", GetFormatter($"{XmlDecl}<ntxxx />").GetFormattedText());
		AssertEquals("XML declaration ending with CR only", $"{XmlDecl}\r\n<ntxxx />", GetFormatter($"{XmlDecl}\n<ntxxx />").GetFormattedText());
		AssertEquals("XML declaration ending with LF only", $"{XmlDecl}\r\n<ntxxx />", GetFormatter($"{XmlDecl}\r<ntxxx />").GetFormattedText());
		AssertEquals("XML declaration ending with CRLF only", $"{XmlDecl}\r\n<ntxxx />", GetFormatter($"{XmlDecl}\r\n<ntxxx />").GetFormattedText());

		AssertContains("Incomplete message", expectedXmlParsingErrorMessage.ReplaceNonBreakingSpaceWithNormalSpace(), GetFormatter($"{XmlDecl}").GetFormattedText().ToString());
		var malformedMessage = $"{XmlDecl.Replace("?>", "")}<ntxxx />";

		AssertContains("Malformed message", expectedXmlParsingErrorMessage.ReplaceNonBreakingSpaceWithNormalSpace(), GetFormatter(malformedMessage).GetFormattedText().ToString());
	});

	public void TestGetFormattedText_HasNoContextCollection()
	{
		var formatter = new UniversalEventXmlPrettyFormatter(new UniversalEventWrapper(UniversalEventTestDataHelper.CreateUniversalEventXmlWithoutContextCollection()));
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown("Expected no exception in case of no ContextCollection", () => formatter.GetFormattedText());
			AssertEquals("Expected empty string in case of no ContextCollection ", ZString.Empty, formatter.GetFormattedText());
		});
	}

	UniversalEventWrapper GetUniversalEventData(string message) => new UniversalEventWrapper(UniversalEventTestDataHelper.CreateUniversalEventXml(messageType: "", responseType: "", reason: "", responseMessage: message));

	UniversalEventXmlPrettyFormatter GetFormatter(string message) => new UniversalEventXmlPrettyFormatter(GetUniversalEventData(message));

	const string XmlDecl = @"<?xml version=""1.0"" encoding=""UTF-8""?>";

	readonly ZString expectedXmlParsingErrorMessage = "<h2>The message content cannot be shown</h2><p>Please look at Text tab for the raw message response.</p><p>Reason:";
}
