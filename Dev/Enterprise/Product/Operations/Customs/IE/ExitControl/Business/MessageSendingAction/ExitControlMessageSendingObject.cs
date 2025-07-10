using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class ExitControlMessageSendingObject : EU.ExitControl.Business.ExitControlMessageSendingObject, IExitControlMessageSendingAction
	{
		public ExitControlMessageSendingObject(CusExitReport messagingObject) : base(messagingObject)
		{
			GetMessageType(messagingObject.CER_Type);
		}

		public new CusExitReport MessagingObject => (CusExitReport)base.MessagingObject;

		[ResourceStringData("24A984F1-E0F5-4101-B3E9-E26236BC887A", Caption = "Exit/Arrival Date")]
		public override ZDateTime DateTime => base.DateTime;

		public ExitControlMessageSender CreateSender() => new ExitControlMessageSender(this);

		void GetMessageType(string reportType)
		{
			switch (reportType)
			{
				case ExitReportTypeList.Codes.ExitNotification:
					MessageType = AESOutgoingMessageTypeList.Codes.ExitNotification;
					break;
				case ExitReportTypeList.Codes.InformationOnNonExitedExport:
					MessageType = AESOutgoingMessageTypeList.Codes.InformationOnNonExitedExport;
					break;
				default:
					MessageType = AESOutgoingMessageTypeList.Codes.ArrivalAtExit;
					break;
			}
		}

		#region IMessageSendingAction Members
		IMessageAttachee IMessageSendingAction.MessageAttachee => MessagingObject;

		void IMessageSendingAction.AddMessage(OutboundEDIMessage message)
		{
			MessagingObject.Messages.Add(message);
		}
		#endregion
	}
}
