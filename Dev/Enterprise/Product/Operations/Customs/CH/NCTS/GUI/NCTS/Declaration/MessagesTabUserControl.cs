using Enterprise.Customs.CH.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public partial class MessagesTabUserControl : EU.NCTS.GUI.MessagesTabUserControl
{
	public MessagesTabUserControl()
	{
		InitializeComponent();
	}

	protected override string GetFormattedDocumentTextCore()
	{
		var formattedText = "<html/>";
		if (!string.IsNullOrEmpty(MessageInterpretationTextBox.Text.Trim()))
		{
			formattedText = MessageTabUserControlHelper.GetHtmlFormattedText(MessageInterpretationTextBox.Text);
		}

		return formattedText;
	}
}
