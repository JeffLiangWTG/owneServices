using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class HtmlHelperTest : TestCase
	{
		public void TestGetHtmlTableCreator()
		{
			var htmlTableCreator = HtmlHelper.GetHtmlTableCreator("3", "75%");
			AssertEquals(@"<table border=""3"" cellpadding=""1"" cellspacing=""0"" width=""75%"" class=""table""></table>", htmlTableCreator.ToHtml());
		}

		public void TestToKeyValuePairSection()
		{
			var actualSection = HtmlHelper.ToKeyValuePairSection(new[] { ("Key 1", "Value 1"), ("Key 2", "Value 2") });
			AssertEquals("<p><strong>Key 1: </strong>Value 1<br><strong>Key 2: </strong>Value 2</p>", actualSection);
		}

		public void TestToStrongIfNotEmpty() => CombineAssertions(() =>
		{
			AssertEquals("Not empty", "<strong>Test</strong>", HtmlHelper.ToStrongIfNotEmpty("Test"));
			AssertEquals("Empty", string.Empty, HtmlHelper.ToStrongIfNotEmpty(string.Empty));
		});

		public void TestToH3IfNotEmpty() => CombineAssertions(() =>
		{
			AssertEquals("Not empty", "<H3>Test</H3>", HtmlHelper.ToH3IfNotEmpty("Test"));
			AssertEquals("Empty", string.Empty, HtmlHelper.ToH3IfNotEmpty(string.Empty));
		});

		public void TestToPIfNotEmpty() => CombineAssertions(() =>
		{
			AssertEquals("Not empty", "<p>Test</p>", HtmlHelper.ToPIfNotEmpty("Test"));
			AssertEquals("Empty", string.Empty, HtmlHelper.ToPIfNotEmpty(string.Empty));
		});
	}
}
