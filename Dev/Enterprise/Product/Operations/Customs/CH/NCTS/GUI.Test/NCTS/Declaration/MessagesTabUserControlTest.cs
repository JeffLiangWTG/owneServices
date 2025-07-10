using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

public class MessagesTabUserControlTest : TestCaseWithFactory
{
	public void TestGetFormattedDocumentTextCore()
	{
		using (var form = new ZForm())
		using (var control = new MessageTabUserControlForTest())
		{
			form.Controls.Add(control);
			form.Show();

			CombineAssertions(() =>
			{
				control.MessageInterpretationTextBoxExposed.Text = "<h1>Test</h1>";
				AssertEquals(GetExpectedHtml(), control.GetFormattedDocumentTextCoreExposed());

				control.MessageInterpretationTextBoxExposed.Text =
					"<?xml version=\"1.0\" encoding=\"UTF-8\"?><test>TEST</test>";
				AssertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><test>TEST</test>",
					control.GetFormattedDocumentTextCoreExposed());
			});
		}
	}

	class MessageTabUserControlForTest : MessagesTabUserControl
	{
		public string GetFormattedDocumentTextCoreExposed() => GetFormattedDocumentTextCore();

		public ZTextBox MessageInterpretationTextBoxExposed => MessageInterpretationTextBox;
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
