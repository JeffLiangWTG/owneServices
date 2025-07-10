using System;
using System.Net;
using CargoWise.Common;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public partial class MessagesTabUserControl : EU.GUI.MessagesTabUserControl
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();
			SetupMessagesGrid();
		}

		void SetupMessagesGrid()
		{
			MessagesGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("{E51F3F80-550D-46F0-9FE5-1B53ADE7F72E}", "Message Type With Description"),
				ColumnName = nameof(EDIMessage.MessageTypeWithDescription),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(272),
				IsVisible = false
			});
		}

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
						var encodedHtml = WebUtility.HtmlEncode(InterpretedMessageTextBox.Text.Trim());

						const string template = @"
						<html>
							</head>
							<body>
								<pre>{0}<pre>
							</body>
						</html>";

						htmlDoc = string.Format(template, encodedHtml);
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
				ErrorReporter.ReportOnce("IE-GUI-MessageTextRefresh", "Could not refresh web browser text.  Text was: " + InterpretedMessageTextBox.Text, ex);
			}
		}
	}
}
