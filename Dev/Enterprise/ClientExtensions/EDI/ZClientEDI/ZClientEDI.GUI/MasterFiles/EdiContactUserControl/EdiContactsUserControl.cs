using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class EdiContactsUserControl : ContactsUserControl
	{
		protected override ContactSendEmailSetResetPassword GetContactSendEmailSetResetPassword() =>
			new EdiContactSendEmailSetResetPassword();
	}
}