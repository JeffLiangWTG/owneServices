using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class AECharacteSetTest : TestCase
{
	public void TestDelimiters()
	{
		var characterSet = AECharacterSet.New();
		CombineAssertions(() =>
		{
			AssertEquals("SubElementDelimiterChar", ':', characterSet.SubElementDelimiterChar);
			AssertEquals("ElementDelimiterChar", '+', characterSet.ElementDelimiterChar);
			AssertEquals("EscapeCharacterChar", '?', characterSet.EscapeCharacterChar);
			AssertEquals("SegmentDelimiterChar", '\'', characterSet.SegmentDelimiterChar);
		});
	}

	public void TestReplaceEscapedCharactersWithSpace()
	{
		AssertEquals(expected: true, AECharacterSet.New().ReplaceEscapedCharactersWithSpace);
	}
}
