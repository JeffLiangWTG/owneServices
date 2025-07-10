using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.AE.Business;

sealed class DocInterchangeUnpacker
{
	public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, EDIMessage outgoingMessage)
	{
		return outgoingMessage != null
			? new EDIInterchangeUnpackerResult(CreateReceivedEDIMessage(interchange))
			: new EDIInterchangeUnpackerResult("[ERROR] Outbound Document Submission message not found.");
	}

	EDIMessage[] CreateReceivedEDIMessage(EDIInterchange interchange)
	{
		var message = EDIInterchangeUnPackerUtils.CreateReceivedEDIMessage(interchange, interchange.EI_BodyText);
		message.EM_MessageSubType = Common.Shared.MessageSubTypeCodes.Codes.Undefined;
		return [message];
	}
}
