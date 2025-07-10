using CargoWise.Common;
using Enterprise.Messaging.Business;
using IXmlCustomsResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business;

sealed class GuaranteeTransactionHandler : IResponseMessageHandler
{
	public GuaranteeTransactionHandler(IXmlCustomsLinkedObjectAdapter customsLinkedObjectAdapter, EDIMessage originalSentMessage)
	{
		this.originalSentMessage = Argument.NotNull(originalSentMessage, nameof(originalSentMessage));
		this.customsLinkedObjectAdapter = Argument.NotNull(customsLinkedObjectAdapter, nameof(customsLinkedObjectAdapter));
	}

	public void Handle(IXmlCustomsResponseMessage responseMessage)
	{
		if (customsLinkedObjectAdapter is IGuaranteeTransactionSupporter guaranteeTransactionSupporter)
		{
			guaranteeTransactionSupporter.ConfirmPendingTransactions(originalSentMessage.EM_MessageNum);
		}
	}

	readonly IXmlCustomsLinkedObjectAdapter customsLinkedObjectAdapter;
	readonly EDIMessage originalSentMessage;
}
