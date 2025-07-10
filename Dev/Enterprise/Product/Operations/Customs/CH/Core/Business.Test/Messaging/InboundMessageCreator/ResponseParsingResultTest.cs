using System;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

internal class ResponseParsingResultTest : TestCase
{
	public void TestIsXml()
	{
		CombineAssertions(() =>
		{
			AssertIsXml("text/xml", true);
			AssertIsXml("text/xml;charset=utf-8", true);
			AssertIsXml("application/xml", true);
			AssertIsXml("application/xml;charset=utf-8", true);
			AssertIsXml("application/xop+xml", true);
			AssertIsXml("application/xop+xml;charset=utf-8", true);
			AssertIsXml("application/pdf", false);
			AssertIsXml("text/json", false);
		});
		void AssertIsXml(string type, bool expectedResult)
		{
			AssertEquals(type, expectedResult, new ResponseParsingResult() { Type = type }.IsXml);
		}
	}

	public void TestIsPdf()
	{
		CombineAssertions(() =>
		{
			AssertIsPdf("text/xml", false);
			AssertIsPdf("text/xml;charset=utf-8", false);
			AssertIsPdf("application/xml", false);
			AssertIsPdf("application/xml;charset=utf-8", false);
			AssertIsPdf("application/xop+xml", false);
			AssertIsPdf("application/xop+xml;charset=utf-8", false);
			AssertIsPdf("application/pdf", true);
			AssertIsPdf("application/pdf;charset=utf-8", true);
			AssertIsPdf("application/excel", false);
		});
		void AssertIsPdf(string type, bool expectedResult)
		{
			AssertEquals(type, expectedResult, new ResponseParsingResult() { Type = type }.IsPdf);
		}
	}

	public void TestIsJson()
	{
		CombineAssertions(() =>
		{
			AssertIsJson("text/xml", false);
			AssertIsJson("text/xml;charset=utf-8", false);
			AssertIsJson("application/xml", false);
			AssertIsJson("application/xml;charset=utf-8", false);
			AssertIsJson("application/xop+xml", false);
			AssertIsJson("application/xop+xml;charset=utf-8", false);
			AssertIsJson("application/pdf", false);
			AssertIsJson("application/pdf;charset=utf-8", false);
			AssertIsJson("application/excel", false);
			AssertIsJson("application/json", true);
		});
		void AssertIsJson(string type, bool expectedResult)
		{
			AssertEquals(type, expectedResult, new ResponseParsingResult() { Type = type }.IsJson);
		}
	}

	public void TestIsEmpty()
	{
		CombineAssertions(() =>
		{
			AssertEquals("with non-empty text", false, new ResponseParsingResult() { BodyText = "test" }.IsEmpty);
			AssertEquals("with empty text", true, new ResponseParsingResult() { BodyText = string.Empty }.IsEmpty);
			AssertEquals("with data", false, new ResponseParsingResult() { BodyData = Array.Empty<byte>() }.IsEmpty);
			AssertEquals("with nothing", true, new ResponseParsingResult().IsEmpty);
		});
	}
}
