using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI.Tools;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class CancelEmailSendConfirmationForm : BaseIncidentPopupForm
	{
		public CancelEmailSendConfirmationForm()
		{
			SpellChecker.InitialiseSpellcheck(zTextBox1, "CancelEmailSendConfirmationForm_TextBox1");
		}

		public CancelEmailSendConfirmationForm(SupportIncidentCancelEmailNotificationAction action)
			: base(action)
		{
			SpellChecker.InitialiseSpellcheck(zTextBox1, "CancelEmailSendConfirmationForm_TextBox1");
		}
	}
}
