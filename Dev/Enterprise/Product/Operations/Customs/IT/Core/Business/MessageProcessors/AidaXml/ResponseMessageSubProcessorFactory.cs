using CargoWise.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

static class ResponseMessageSubProcessorFactory
{
	public static IResponseMessageSubProcessor GetSubProcessor(EDIMessage originalSentMessage)
	{
		Argument.NotNull(originalSentMessage, nameof(originalSentMessage));

		var sentMessageType = originalSentMessage.EM_MessageType;

		switch (sentMessageType)
		{
			case MessageProcessorConstants.InterchangeTypes.Ucc6CancellationType:
				return new CancellationResponseMessageSubProcessor();
			case MessageProcessorConstants.InterchangeTypes.Ucc6AmendmentType:
				return new AmendmentResponseMessageSubProcessor();
			default:
				return new NewResponseMessageSubProcessor(originalSentMessage);
		}
	}
}
