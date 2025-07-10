using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class HtmlNotificationEmailSenderTest : TestCaseWithFactory
	{
		public void TestCreateEmail()
		{
			var email = new HtmlNotificationEmailSender().CreateEmail("Email Subject", "This text will go into the body.\r\n");
			CombineAssertions(() =>
			{
				AssertEquals("Subject", "Email Subject", email.Subject);
				AssertMultilineASCIIEquals("Body", ExpectedEmailHtmlBody, email.Body);
				AssertEquals("ContentType", EmailContentTypes.HTML, email.ContentType);
				AssertEquals("Attachments.Count", 2, email.Attachments.Count);
				AssertEquals("Attachments[0].DisplayName", "Banner.jpg", email.Attachments[0].DisplayName);
				AssertEquals("Attachments[1].DisplayName", "Footer.jpg", email.Attachments[1].DisplayName);
			});
		}

		string ExpectedEmailHtmlBody
		{
			get
			{
				const string htmlResult = @"<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"">
<head>
  <title>Email Subject</title>
  <style type=""text/css"">
  <!--
  (*StyleSheet*)
  -->
</style>
</head>
<body>
  <table border=""0"" cellpadding=""0"" cellspacing=""0"" bgcolor=""#FFFFFF"">
    <tr>
      <td class=""banner""><img src=""cid:Banner.jpg"" alt=""Banner Image"" /></td>
    </tr>
    <tr>
      <td class=""content"">
(*Body*)
      </td>
    </tr>
    <tr>
      <td><img src=""cid:Footer.jpg"" alt=""Footer Image"" /></td>
    </tr>
  </table>
</body>
</html>";
				var defaultCss = @"
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

".Trim();
				return htmlResult
					.Replace("(*StyleSheet*)", defaultCss)
					.Replace("(*Body*)", "This text will go into the body.");
			}
		}
	}
}
