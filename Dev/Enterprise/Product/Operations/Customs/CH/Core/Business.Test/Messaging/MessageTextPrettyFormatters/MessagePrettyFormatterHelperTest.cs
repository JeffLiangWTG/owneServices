using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class MessagePrettyFormatterHelperTest : TestCase
{
	public void TestGetTextByLanguageCode()
	{
		GlbStaff.CurrentUser.GS_WorkingLanguage = "EN";
		AssertEquals(null, MessagePrettyFormatterHelper.GetTextByLanguageCode(new List<(string, string)> { }));
		AssertEquals("Text", MessagePrettyFormatterHelper.GetTextByLanguageCode(new List<(string, string)> { (null, "Text") }));
		AssertEquals("DE Text", MessagePrettyFormatterHelper.GetTextByLanguageCode(new List<(string, string)> { ("de", "DE Text"), ("fr", "FR Text") }));
		AssertEquals("EN Text", MessagePrettyFormatterHelper.GetTextByLanguageCode(new List<(string, string)> { ("DE", "DE Text"), ("en", "EN Text") }));
		AssertEquals("IT Text", MessagePrettyFormatterHelper.GetTextByLanguageCode(new List<(string, string)> { ("it", "IT Text"), ("fr", "FR Text") }));
	}
}
