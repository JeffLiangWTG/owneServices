using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public partial class StatementMessagesUserControl : ZUserControl
	{
		public StatementMessagesUserControl()
		{
			InitializeComponent();
			MessageInterpretationWebBrowser.AllowOverlap(MessageInterpretationTextBox);
		}

		void MessageInterpretationTextBox_TextChanged(object sender, System.EventArgs e)
		{
			RefreshMessageInterpretationWebBrowser();
		}

		void RefreshMessageInterpretationWebBrowser()
		{
			var webPageDocument = MessageInterpretationWebBrowser.Document.OpenNew(true);
			webPageDocument.Write(MessageInterpretationTextBox.Text);
			MessageInterpretationWebBrowser.Refresh();
		}
	}
}
