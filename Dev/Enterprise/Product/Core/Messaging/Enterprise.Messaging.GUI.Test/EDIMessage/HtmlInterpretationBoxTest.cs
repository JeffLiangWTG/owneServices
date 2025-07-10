using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Messaging.GUI.Testing
{
	sealed class HtmlInterpretationBoxTest : ZControlBaseTestCase<HtmlInterpretationBox>
	{
		[UseSnapshotProtection]
		public void TestDocumentTextIsLoadedWhenDifferentOnly()
		{
			using (SystemDataRegistry.Instance.UserIdleWorkerEnabled.SetTemporaryValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, false))
			using (var form = new Form())
			using (var box = new HtmlInterpretationBoxForTesting())
			{
				form.Controls.Add(box);
				form.Show();
				Application.DoEvents();
				var plainText = "Plain Text";

				box.DocumentText = plainText;
				UserIdleWorker.Flush();

				AssertEquals(1, box.TotalCallsToUpdateDocumentTextFromHtmlText);

				box.DocumentText = plainText;
				UserIdleWorker.Flush();

				AssertEquals("Should not load new document if it is the same DocumentText", 1, box.TotalCallsToUpdateDocumentTextFromHtmlText);
			}
		}

		[UseSnapshotProtection]
		public void TestDocumentText()
		{
			using (SystemDataRegistry.Instance.UserIdleWorkerEnabled.SetTemporaryValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, false))
			using (var form = new Form())
			using (var box = new HtmlInterpretationBoxForTesting())
			{
				form.Controls.Add(box);
				form.Show();
				Application.DoEvents();

				var plainText = "OneWordToAvoidHtmlCharacters";
				box.DocumentText = plainText;
				UserIdleWorker.Flush();

				AssertEquals("User-control should not manipulate DocumentText for binding", plainText, box.DocumentText);
			}
		}

		public void TestGetHtmlFormattedText()
		{
			using (var box = new HtmlInterpretationBoxForTesting())
			{
				#region Expected Html

				const string expectedHtmlFromPlainText = @"<html>
<head>
	<style type='text/css'>
		<!--
			
body, p, td 
{ 
	font-family: Microsoft Sans Serif; 
	font-size: 8.25pt;
	margin:4pt;
}
		-->
	</style>
</head>
<body>
	<table border='0' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>
		<tr>
			<td class='content'>
				<pre>&lt;Plain&amp;&nbsp;Text<br/></pre>
			</td>
		</tr>
	</table>
</body>
</html>";

				#endregion

				box.UseFixedWidthForPlainText = true;
				AssertEquals("Plain Text should be converted to html", expectedHtmlFromPlainText, box.GetHtmlFormattedText_Exposed("<Plain& Text\r\n"));

				box.UseFixedWidthForPlainText = false;
				var actual = box.GetHtmlFormattedText_Exposed("<Plain& Text\r\n");
				AssertEquals("Plain Text should not have fixed-width tag", false, actual.Contains("pre>"));

				const string textWithHtmlTags = "bla bla <p>\r\nHtml Text\r\n</p>";
				actual = box.GetHtmlFormattedText_Exposed(textWithHtmlTags);
				AssertEquals("Text with html tags should be shown with email style sheet", true,actual.Contains(SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value));
				AssertEquals("Text with html tags should not be changed", true, actual.Contains(textWithHtmlTags));

				const string fullHtml = "<html>\r\n<body>\r\n\tHtml Body\r\n</body>\r\n</html>";
				actual = box.GetHtmlFormattedText_Exposed(fullHtml);
				AssertEquals("Full html text should be shown as it is", fullHtml, actual);
			}
		}

		#region Implementation

		class HtmlInterpretationBoxForTesting : HtmlInterpretationBox
		{
			public int TotalCallsToUpdateDocumentTextFromHtmlText;
			protected override void UpdateDocumentTextFromHtmlText(string htmlText)
			{
				TotalCallsToUpdateDocumentTextFromHtmlText++;
				base.UpdateDocumentTextFromHtmlText(htmlText);
				while (ReadyState != WebBrowserReadyState.Complete)
				{
					System.Windows.Forms.Application.DoEvents();
				}
			}

			public string GetHtmlFormattedText_Exposed(string text) => base.GetHtmlFormattedText(text);
		}

		protected override void BindControl()
		{
			base.BindControl();
			Control.BindTo = "Z0_Description";
			DataBoundControl.Get(Control).SetDataBinding(Dummy, Control.BindTo);
		}

		#endregion
	}
}
