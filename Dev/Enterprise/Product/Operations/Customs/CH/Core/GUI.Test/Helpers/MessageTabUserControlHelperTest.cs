using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

public class MessageTabUserControlHelperTest : TestCase
{
	public void TestGetHtmlFormattedText()
	{
		CombineAssertions(() =>
		{
			AssertEquals(GetExpectedHtml(), MessageTabUserControlHelper.GetHtmlFormattedText("<h1>Test</h1>"));
			AssertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><test>TEST</test>", MessageTabUserControlHelper.GetHtmlFormattedText("<?xml version=\"1.0\" encoding=\"UTF-8\"?><test>TEST</test>"));
		});
	}

	string GetExpectedHtml() => @"<html>
<head>
	<style type='text/css'>
		<!--
			
		*
		{
			font-family: Verdana, Arial, Helvetica, sans-serif;
		}

		body,
		table,
		th,
		td {
			font-size: 9pt;
			border-collapse: collapse;
			padding: 3px 5px 3px 5px;
			vertical-align: top;
			text-align: left;
		}
		-->
	</style>
</head>
<body>
	<h1>Test</h1>
</body>
</html>";
}
