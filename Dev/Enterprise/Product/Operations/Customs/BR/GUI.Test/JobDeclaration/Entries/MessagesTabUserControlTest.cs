using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class MessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestGetHtmlFormattedText()
		{
			CombineAssertions(() =>
			{
				AssertEquals(GetExpectedHtml(), MessagesTabUserControl.GetHtmlFormattedText("<h1>Test</h1>"));
				AssertEquals("<html>TEST</html>", MessagesTabUserControl.GetHtmlFormattedText("<html>TEST</html>"));
				AssertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><test>TEST</test>", MessagesTabUserControl.GetHtmlFormattedText("<?xml version=\"1.0\" encoding=\"UTF-8\"?><test>TEST</test>"));
			});
		}

		string GetExpectedHtml() => @"<html>
<head>
	<style type='text/css'>
		<!--
			P {
		FONT-SIZE: 12px;
		COLOR: #666666;
		FONT-FAMILY: Arial, sans-serif;
		line-height: 17px;
}

body {
		background-color: #FFFFFF;
	font-family: Arial, sans-serif;
	font-size: 12px;
	color: #666666;
		margin: auto;
		width: 600px;
}

th {
	font-family: Arial, sans-serif;
	font-size: 12px;
	color: #FFFFFF;
	background-color: #005596;
}

td {
	font-family: Arial, sans-serif;
	font-size: 12px;
	color: #666666;
}

a, a:visited {
	color: #666666;
	text-decoration: underline;
}

a:hover {
	color: #00a4e4;
	text-decoration: underline;
}

.heading1 {
		font-family: Arial, sans-serif;
		font-size: 20px;
		font-weight: normal;
		color: #000000;
		line-height: 36px;
}

.subheading2 {
		font-family: Arial, sans-serif;
		font-size: 16px;
		line-height: 18px;
		font-weight: bold;
	color: #00a4e4;
}

.table {
		border-right: #666666 solid thin;
		border-top: #666666 solid thin;
		border-left: #666666 solid thin;
		border-bottom: #666666 solid thin;
}

.tableheadings td, th {
		border-bottom: #666666 solid thin;
		background-color: #005596;
}

.content {
		padding: 0em 1em;
}

.banner {
		padding-top: 1em;
}
		-->
	</style>
</head>
<body>
	<table border='0' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>
		<tr>
			<td class='content'>
				<pre><h1>Test</h1></pre>
			</td>
		</tr>
	</table>
</body>
</html>";
	}
}
