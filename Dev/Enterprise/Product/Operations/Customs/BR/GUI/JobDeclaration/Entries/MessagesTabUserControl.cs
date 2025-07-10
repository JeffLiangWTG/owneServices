using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class MessagesTabUserControl : BaseMessagesTabUserControl
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();
			InterpretedMessageTextBox.TextChanged += new EventHandler(InterpretedMessageTextBox_TextChanged);
			if (!DesignModeFinder.IsDesigning)
			{
				UserIdleWorker.QueueWorkItem(this, new MethodInvoker(RefreshDocumentText), null); // updates the web browser control's HTML with the text in the hidden text box, but only when the computer first becomes idle.
			}

			InterpretedMessageTextWebBrowser.AllowOverlap(InterpretedMessageTextBox);
		}

		void InterpretedMessageTextBox_TextChanged(object sender, EventArgs e)
		{
			RefreshDocumentText();
		}

		void RefreshDocumentText()
		{
			try
			{
				if (!string.IsNullOrEmpty(InterpretedMessageTextBox.Text.Trim()))
				{
					InterpretedMessageTextWebBrowser.DocumentText = GetHtmlFormattedText(InterpretedMessageTextBox.Text);
				}
				else
				{
					InterpretedMessageTextWebBrowser.DocumentText = "<html/>";
				}
			}
			catch (ArgumentException ex)
			{
				ErrorReporter.ReportOnce("BR-GUI-MessageTextRefresh", "Could not refresh web browser text.  Text was: " + InterpretedMessageTextBox.Text, ex);
			}
		}

		#region GetHtmlFormattedText

		internal static string GetHtmlFormattedText(string textToFormat)
		{
			if (textToFormat != null && !textToFormat.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase) && !textToFormat.TrimStart().StartsWith((NoResString)"<?xml", StringComparison.OrdinalIgnoreCase))
			{
				var isPlainText = !Regex.IsMatch(textToFormat, @"<[^>]+>");
				textToFormat = HtmlTamplate.Replace("(*HtmlStyleSheet*)", GetHtmlStyleSheet(isPlainText))
					.Replace("(*HtmlBody*)", textToFormat);
			}
			return textToFormat;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Style sheet")]
		static string GetHtmlStyleSheet(bool isPlainText)
		{
			const string plainTextStyleSheet = @"
body, p, td
{
	font-family: Microsoft Sans Serif;
	font-size: 8.25pt;
	margin:4pt;
}";
			return isPlainText ? plainTextStyleSheet : SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value;
		}

		const string HtmlTamplate = @"<html>
<head>
	<style type='text/css'>
		<!--
			(*HtmlStyleSheet*)
		-->
	</style>
</head>
<body>
	<table border='0' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>
		<tr>
			<td class='content'>
				<pre>(*HtmlBody*)</pre>
			</td>
		</tr>
	</table>
</body>
</html>";

		#endregion
	}
}
