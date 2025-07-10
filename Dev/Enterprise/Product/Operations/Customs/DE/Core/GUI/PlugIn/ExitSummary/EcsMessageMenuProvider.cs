using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	[CodeAlive("Unused class as using EU.ExitControl.GUI.PlugIn.ExitControlPlugIn instead of EU.GUI.PlugIn.ExitSummaryPlugIn")]
	public class EcsMessageMenuProvider : EU.GUI.PlugIn.EcsMessageMenuProvider
	{
		public EcsMessageMenuProvider(EU.Business.CusExitControlHeader exitHeader) : base(exitHeader)
		{
		}

		public override bool CreateArrivalMessages()
		{
			return SendECSMessageToCustoms(MessageType.Arrival);
		}

		public override bool CreateDepartureMessages()
		{
			return SendECSMessageToCustoms(MessageType.Departure);
		}

		bool SendECSMessageToCustoms(MessageType messageType)
		{
			var succeeded = false;
			if (!ExitHeader.CusExitDetails.Any())
			{
				Globals.Message.Show(Res.GetString("9383BA8A-3CE8-44E4-889D-5F592D5C53D5", "There is no movement to be sent."));
			}
			else
			{
				var continueWithSend = false;

				if (messageType == MessageType.Arrival)
				{
					var messageSendingActionParent = new ExitSummaryMessageSendingActionParent((CusExitControlHeader)ExitHeader);
					using (var form = new ExitSummaryMessageSendingForm(messageSendingActionParent))
					{
						continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
					}
					if (continueWithSend)
					{
						var errorMessage = new ECSMessageSender().Send();
						if (errorMessage.IsEmpty)
						{
							succeeded = true;
							Globals.Message.Show(MessageSentSuccessfully);
						}
						else
						{
							Globals.Message.Show(errorMessage);
						}
					}
				}
				else
				{
					var messageSendingActionParent = new ExitNotificationMessageSendingActionParent((CusExitControlHeader)ExitHeader);
					using (var form = new ExitNotificationMessageSendingForm(messageSendingActionParent))
					{
						continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
					}
					if (continueWithSend)
					{
						var errorMessage = new ExitNotificationMessageSender().Send();
						if (errorMessage.IsEmpty)
						{
							succeeded = true;
							Globals.Message.Show(MessageSentSuccessfully);
						}
						else
						{
							Globals.Message.Show(errorMessage);
						}
					}
				}
			}
			return succeeded;
		}

		static MultilingualString MessageSentSuccessfully => ResString.GetMultilingualString("E85F65DC-9AB1-4821-A177-7B057508D79D", "Message sent successfully");

		enum MessageType
		{
			Arrival,
			Departure
		}
	}
}
