using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Business
{
	sealed class FeedbackMessageUnpacker<T>
		: IFeedbackMessageUnpacker
		where T : EDIMessage
	{
		public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, ZString bodyText, ZString responseHeaderText, ZString correlationId, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, Integration.BatchProcessor.ILoggingInformation logger)
		{
			var message = interchange.Factory.New<T>();
			interchange.ContainedMessages.Add(message);

			message.EM_ApplicationCode = ILEDIInterchange.ApplicationCodes.ILCustoms;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageData = interchange.EI_BodyData;
			message.EM_MessageNum = interchange.EI_InterchangeNum.SubstringSafe(0, EDIMessageSchema.EM_MessageNum.MaxLength);
			message.EM_MessageText = bodyText;
			message.EM_ApplicationReference = correlationId;

			return new EDIInterchangeUnpackerResult(new[] { message });
		}
	}
}
