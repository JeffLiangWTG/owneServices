using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.FlexCelInterface.Testing
{
	sealed class ExcelHyperlinkTest : TestCase
	{
		public void TestConstructor()
		{
			ExcelHyperlink hyperlink = new ExcelHyperlink(THyperLinkType.URL, "HomePage", "http://www.edi.com.au", "AY7", "blah", "meh meh");
			AssertEquals(THyperLinkType.URL, hyperlink.Type);
			AssertEquals("HomePage", hyperlink.TextToShow);
			AssertEquals("http://www.edi.com.au", hyperlink.LinkLocation);
			AssertEquals("AY7", hyperlink.TargetFrame);
			AssertEquals("blah", hyperlink.TextMark);
			AssertEquals("meh meh", hyperlink.Tooltip);
		}

		public void TestSimpleConstructor()
		{
			ExcelHyperlink hyperlink = new ExcelHyperlink(THyperLinkType.URL, "http://www.edi.com.au");
			AssertEquals(THyperLinkType.URL, hyperlink.Type);
			AssertEquals("http://www.edi.com.au", hyperlink.TextToShow);
			AssertEquals("http://www.edi.com.au", hyperlink.LinkLocation);
			AssertEquals("", hyperlink.TargetFrame);
			AssertEquals("", hyperlink.TextMark);
			AssertEquals("", hyperlink.Tooltip);
		}

		public void TestToString()
		{
			var hyperlink = new ExcelHyperlink(THyperLinkType.URL, "HomePage", "http://www.edi.com.au", "AY7", "blah", "meh meh");
			AssertEquals("ToString() Value of the HyperLink should be the URL.", "http://www.edi.com.au", hyperlink.ToString());
		}
	}
}
