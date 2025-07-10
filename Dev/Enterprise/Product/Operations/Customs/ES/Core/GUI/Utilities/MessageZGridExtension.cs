using System.Windows.Forms;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture.Environment;
using GUIMessageZGridExtension = Enterprise.Messaging.GUI.MessageZGridExtension;

namespace Enterprise.Customs.ES.GUI
{
	public static class MessageZGridExtension
	{
		public static void DoResendInterchange(this MessageZGrid messagesGrid)
		{
			if (messagesGrid.SelectedElements.Length > 0)
			{
				foreach (EDIMessage aMessage in messagesGrid.SelectedElements)
				{
					var messageNum = aMessage.EM_MessageNum;
					if (aMessage.IsInDatabase && aMessage.Interchange != null && aMessage.Interchange.IsInDatabase)
					{
						aMessage.Reload();
						aMessage.Interchange.Reload();

						var interchangeNum = aMessage.Interchange.EI_InterchangeNum;
						if (aMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
						{
							var interchangeStatus = aMessage.Interchange.EI_Status;
							if (interchangeStatus == EDIInterchange.Status.eHubPending || interchangeStatus == EDIInterchange.Status.Failed || interchangeStatus == EDIInterchange.Status.Sent)
							{
								AskConfirmationAndResend(interchangeStatus, messageNum, interchangeNum, aMessage.Interchange);
							}
							else if (interchangeStatus == EDIInterchange.Status.eHubQueued || interchangeStatus == EDIInterchange.Status.Queued)
							{
								Globals.Message.ShowError(GUIMessageZGridExtension.GetErrorMessageWhenTryingResendQueuedMessage(messageNum, interchangeNum), GUIMessageZGridExtension.ErrorCaptionWhenTryingResendQueuedMessage);
							}
							else
							{
								Globals.Message.ShowInformation(Res.GetString("5FE976FD-3F7F-4A1B-8231-A846E28F473E", "Message {0} in Interchange {1} is in status {2} and is not possible to resend it", messageNum, interchangeNum, interchangeStatus));
							}
							aMessage.Reload();
							aMessage.Interchange.Reload();
						}
						else
						{
							Globals.Message.ShowError(GUIMessageZGridExtension.GetErrorMessageWhenTryingResendOfInboundMessage(messageNum, interchangeNum), GUIMessageZGridExtension.ErrorCaptionWhenTryingResendOfInboundMessage);
						}
					}
					else
					{
						Globals.Message.ShowError(GUIMessageZGridExtension.GetErrorMessageWhenTryingResendOfNotSentMessage(messageNum), GUIMessageZGridExtension.ErrorCaptionWhenTryingResendOfNotSentMessage);
					}
				}
			}
			else
			{
				Globals.Message.ShowInformation(GUIMessageZGridExtension.InfoMessageWhenTryingResendNotSelectedMessages);
			}

			void AskConfirmationAndResend(string interchangeStatus, string messageNum, string interchangeNum, EDIInterchange interchange)
			{
				var confirmationMessage = string.Empty;
				if (interchangeStatus == EDIInterchange.Status.eHubPending)
				{
					confirmationMessage = Res.GetString("9407D99F-80D5-45B0-BFEF-70FA3343EA13", "Message {0} in Interchange {1} has been acknowledged and is waiting for a response.  Are you sure you want to resend it?", messageNum, interchangeNum);
				}
				else if (interchangeStatus == EDIInterchange.Status.Failed)
				{
					confirmationMessage = Res.GetString("13AEB0D7-8B19-449B-BACB-FBDB8B55D42E", "Message {0} in Interchange {1} has been acknowledged but something failed. Before resending it, please check the logs or error message.  Are you sure you want to resend it?", messageNum, interchangeNum);
				}
				else if (interchangeStatus == EDIInterchange.Status.Sent)
				{
					confirmationMessage = Res.GetString("71D9B630-2D81-4968-9077-5C7E4A637FCC", "Message {0} in Interchange {1} has been acknowledged and sent correctly.  Are you sure you want to resend it?", messageNum, interchangeNum);
				}

				if (Globals.Message.ShowConfirmation(confirmationMessage, GUIMessageZGridExtension.ConfirmResendQuestion, GUIMessageZGridExtension.ConfirmResendText, MessageBoxIcon.Hand) == DialogResult.OK)
				{
					InterchangeResender.GetInstance(interchange).Resend(true);
				}
			}
		}
	}
}
