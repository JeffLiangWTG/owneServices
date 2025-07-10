using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CustomsWare.Business
{
	class MessageCreator : IInboundMessageCreator
	{
		void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
		{
			var ediMessage = interchange.ContainedMessages.AddNew(typeof(EDIMessage));
			ediMessage.EM_ApplicationCode = interchange.EI_ApplicationCode;
			ediMessage.EM_MessageType = ApplicationCodeList.Codes.CustomsWare;
			ediMessage.EM_Status = EDIMessage.Status.Queued;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_MessageNum = "1";
			ediMessage.SetEM_MessageTextSource(new TextReaderSource(LargeMessageHelper.CopyAndDispose(interchange.GetEI_BodyTextReader())));
		}
	}
}
