using System;
using System.Web;
using CargoWise.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public partial class MessagesTabUserControl : EU.ExitControl.GUI.MessagesTabUserControl
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();
		}

		const string HtmlPreElementTemplate = @"<html><head></head><body><pre>{0}<pre></body></html>";

		protected override void RefreshDocumentText()
		{
			try
			{
				var htmlDoc = "<html/>";
				if (!string.IsNullOrEmpty(InterpretedMessageTextBox.Text.Trim()))
				{
					var message = MessagesGrid.ListManager?.GetCurrent() as EDIMessage;

					if (message != null && message.IsTransmitMessage)
					{
						var encodedHtml = HttpUtility.HtmlEncode(InterpretedMessageTextBox.Text.Trim());
						htmlDoc = string.Format(HtmlPreElementTemplate, encodedHtml);
					}
					else
					{
						htmlDoc = InterpretedMessageTextBox.Text;
					}
				}

				if (InterpretedMessageTextWebBrowser.Document == null)
				{
					InterpretedMessageTextWebBrowser.Url = new Uri("about:blank", System.UriKind.Absolute);
				}
				var webPageDocument = InterpretedMessageTextWebBrowser.Document.OpenNew(true);
				webPageDocument.Write(htmlDoc);
				InterpretedMessageTextWebBrowser.Refresh();
			}
			catch (ArgumentException ex)
			{
				ErrorReporter.ReportOnce("IE-EXITCONTROL-GUI-RefreshDocumentText", "Could not refresh web browser text.  Text was: " + InterpretedMessageTextBox.Text, ex);
			}
		}
	}
}
