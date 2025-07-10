using CargoWise.Common;
using Enterprise.Messaging.Business;
using IXmlCustomsResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business;

sealed class NewResponseMessageSubProcessor : IResponseMessageSubProcessor
{
	public NewResponseMessageSubProcessor(EDIMessage originalSentMessage)
	{
		this.originalSentMessage = Argument.NotNull(originalSentMessage, nameof(originalSentMessage));
	}

	public void ProcessMessage(IXmlCustomsLinkedObjectAdapter adapter, IXmlCustomsResponseMessage responseMessageWrapper)
	{
		Argument.NotNull(adapter, nameof(adapter));
		Argument.NotNull(responseMessageWrapper, nameof(responseMessageWrapper));

		var handlers = new IResponseMessageHandler[]
		{
			new MrnFieldHandler(adapter),
			new ReleaseItemsHandler(adapter),
			new A93NumberHandler(adapter, A93NumberHandleMode.Add),
			new DepositedDeclarationMessageHandler(adapter),
			new UnderControlDeclarationMessageHandler(adapter),
			new GuaranteeTransactionHandler(adapter, originalSentMessage),
		};
		handlers.ForEach(h => h.Handle(responseMessageWrapper));
	}

	readonly EDIMessage originalSentMessage;
}
