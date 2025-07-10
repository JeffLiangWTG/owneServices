using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.KR.GUI
{
	public partial class MessagesTabUserControl : BaseMessagesTabUserControl
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();

			MessagesGrid.OnResendInterchange = ResendInterchange;
		}

		void ResendInterchange()
		{
			if (!Env.Security.ResendInterchange.IsAllowed)
			{
				Globals.Message.ShowError(Env.Security.ResendInterchange.ErrorMessageForNotAllowed);
			}
			else if (MessagesGrid.SelectedElements.Length == 1)
			{
				var message = (EDIMessage)MessagesGrid.SelectedElements[0];
				var entry = (CusEntryHeader)message.EM_LinkedObject;

				if (message.EM_ReceiveTransmit != EDIMessage.Direction.Transmit)
				{
					Globals.Message.ShowError((NoResString)"Only outgoing interchanges can be resent.");
				}
				else if (entry.Messages.LastOutgoingMessage != message)
				{
					Globals.Message.ShowError((NoResString)"Interchange could not be resent. Please select the last outgoing message.");
				}
				else if (message.Interchange == null)
				{
					Globals.Message.ShowError((NoResString)"This message has no interchange, thus it cannot be resent.");
				}
				else if (message.Interchange.EI_Status == EDIInterchange.Status.Queued || message.Interchange.EI_Status == EDIInterchange.Status.eHubQueued || message.Interchange.EI_Status == EDIInterchange.Status.eHubPending)
				{
					Globals.Message.Show((NoResString)"This interchange is already queued for sending.");
				}
				else
				{
					var outcome = new KRInterchangeResender(message.Interchange).Resend(true) ? (NoResString)"successfully queued to be" : (NoResString)"could not be";
					Globals.Message.Show((NoResString)"Interchange " + outcome + (NoResString)" resent.");
				}
			}
			else
			{
				Globals.Message.ShowInformation((NoResString)"Select one message before trying to resend the interchange.");
			}
		}
	}
}
