using System.Windows.Forms;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Messaging.GUI
{
	public static class MessageZGridExtension
	{
		public static void DoResendInterchange(this MessageZGrid messagesGrid)
		{
			if (messagesGrid.SelectedElements.Length > 0)
			{
				foreach (EDIMessage aMessage in messagesGrid.SelectedElements)
				{
					if (aMessage.IsInDatabase && aMessage.Interchange != null && aMessage.Interchange.IsInDatabase)
					{
						aMessage.Reload();
						aMessage.Interchange.Reload();
						if (aMessage.EM_ReceiveTransmit == EDIMessage.Direction.Transmit)
						{
							if (aMessage.Interchange.EI_Status == EDIInterchange.Status.Acknowledged)
							{
								if (Globals.Message.ShowConfirmation(GetConfirmationMessageWhenTryingResendAcknowledgedMessage(aMessage.EM_MessageNum, aMessage.Interchange.EI_InterchangeNum), ConfirmResendQuestion, ConfirmResendText, MessageBoxIcon.Hand) == DialogResult.OK)
								{
									InterchangeResender.GetInstance(aMessage.Interchange).Resend(true);
								}
							}
							else if (aMessage.Interchange.EI_Status == EDIInterchange.Status.Queued)
							{
								Globals.Message.ShowError(GetErrorMessageWhenTryingResendQueuedMessage(aMessage.EM_MessageNum, aMessage.Interchange.EI_InterchangeNum), ErrorCaptionWhenTryingResendQueuedMessage);
							}
							else
							{
								if (Globals.Message.Show(GetMessageWhenTryingResendOtherMessage(aMessage.EM_MessageNum, aMessage.Interchange.EI_InterchangeNum), ResendMessageQuestion, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
								{
									InterchangeResender.GetInstance(aMessage.Interchange).Resend();
								}
							}
							aMessage.Reload();
							aMessage.Interchange.Reload();
						}
						else
						{
							Globals.Message.ShowError(GetErrorMessageWhenTryingResendOfInboundMessage(aMessage.EM_MessageNum, aMessage.Interchange.EI_InterchangeNum), ErrorCaptionWhenTryingResendOfInboundMessage);
						}
					}
					else
					{
						Globals.Message.ShowError(GetErrorMessageWhenTryingResendOfNotSentMessage(aMessage.EM_MessageNum), ErrorCaptionWhenTryingResendOfNotSentMessage);
					}
				}
			}
			else
			{
				Globals.Message.ShowInformation(InfoMessageWhenTryingResendNotSelectedMessages);
			}
		}

		public static string GetConfirmationMessageWhenTryingResendAcknowledgedMessage(string messageNum, string interchangeNum) => Res.GetString("8df533ae-c409-4a0b-93eb-c49e5e6a313c", "Message {0} in Interchange {1} has been acknowledged.  Are you sure you want to resend it?", messageNum, interchangeNum);
		public static string ConfirmResendQuestion => Res.GetString("bfd036c7-6269-484b-b23a-7501c61df815", "Confirm Resend?");
		public static string ConfirmResendText => "RESEND";
		public static string GetErrorMessageWhenTryingResendQueuedMessage(string messageNum, string interchangeNum) => Res.GetString("fba468d9-3040-4e3e-a23a-80b2e127f2fd", "Message {0} in Interchange {1} is waiting to be sent or has just been sent.  Please verify the service tasks are running", messageNum, interchangeNum);
		public static string ErrorCaptionWhenTryingResendQueuedMessage => Res.GetString("6201b9a7-ccf1-4a53-a16c-c509d96aa978", "Message Queued");
		public static string GetMessageWhenTryingResendOtherMessage(string messageNum, string interchangeNum) => Res.GetString("91caa9a6-ab90-4635-8f16-df4efb684e75", "Message {0} in Interchange {1} has not been acknowledged.  Do you wish to resend it?", messageNum, interchangeNum);
		public static string ResendMessageQuestion => Res.GetString("d8a2edb9-8697-49c6-9c79-4bf61e756471", "Resend Interchange?");
		public static string GetErrorMessageWhenTryingResendOfInboundMessage(string messageNum, string interchangeNum) => Res.GetString("df1d10cc-3a40-41c5-915c-f4bfdb01316b", "Message {0} in Interchange {1} is not an outbound message.  You may only resend outbound messages", messageNum, interchangeNum);
		public static string ErrorCaptionWhenTryingResendOfInboundMessage => Res.GetString("bdfa25e0-524f-48d9-9de5-5af2caebecad", "Inbound Message");
		public static string GetErrorMessageWhenTryingResendOfNotSentMessage(string messageNum) => Res.GetString("1a59f200-8118-49c2-94e7-adfaebfd52cc", "Message {0} has never been sent.  Please verify the service tasks are running", messageNum);
		public static string ErrorCaptionWhenTryingResendOfNotSentMessage => Res.GetString("4b668641-70c3-4164-b9be-9aff778518c8", "Message not sent");
		public static string InfoMessageWhenTryingResendNotSelectedMessages => Res.GetString("00228197-48d5-4239-89a1-62db15b05c9e", "Select an interchange to resend");
	}
}
