using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class MessagesTabUserControl : ZUserControl
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();
#if !WINZOR
			MessageInterpretationWebBrowser.AllowOverlap(MessageInterpretationTextBox);
			MessageInterpretationWebBrowser.AllowOverlap(MessageInterpretationRichTextBox);
			MessageInterpretationRichTextBox.Visible = false;
#else
			MessageInterpretationRichTextBox.AllowOverlap(MessageInterpretationTextBox);
			MessageInterpretationRichTextBox.AllowOverlap(MessageInterpretationWebBrowser);
			MessageInterpretationWebBrowser.Visible = false;
#endif
		}

		void MessageInterpretationTextBox_TextChanged(object sender, System.EventArgs e)
		{
			RefreshMessageInterpretationWebBrowser();
		}

		void RefreshMessageInterpretationWebBrowser()
		{
#if !WINZOR
			var webPageDocument = MessageInterpretationWebBrowser?.Document?.OpenNew(true);
			webPageDocument?.Write(GetFormattedDocumentText());
			MessageInterpretationWebBrowser?.Refresh();
#else
			var colorizedHtml = ColorizerHelper.ColorizeHtmlText(GetFormattedDocumentText());
			MessageInterpretationRichTextBox.Html = colorizedHtml;
#endif
		}

		string GetFormattedDocumentText() => GetFormattedDocumentTextCore();

		protected virtual string GetFormattedDocumentTextCore()
		{
			return MessageInterpretationTextBox.Text;
		}
	}
}
