using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	sealed class NormalAcknowledgementUnpacker
	{
		public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange)
		{
			var message = interchange.Factory.New<EDIMessage>();
			message.EM_ApplicationCode = interchange.EI_ApplicationCode;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageType = interchange.EI_InterchangeType;
			message.EM_MessageNum = interchange.EI_InterchangeNum.SubstringSafe(0, EDIMessageSchema.EM_MessageNum.MaxLength);
			message.EM_GB = interchange.EI_GB;
			interchange.ContainedMessages.Add(message);
			message.EM_MessageText = interchange.EI_BodyText;
			return new EDIInterchangeUnpackerResult(new[] { message });
		}
	}
}
