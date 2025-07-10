using CargoWise.Common;
using IXmlResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business;

public sealed class DepositedDeclarationMessageHandler : IResponseMessageHandler
{
	public DepositedDeclarationMessageHandler(IXmlCustomsLinkedObjectAdapter adapter)
	{
		this.adapter = Argument.NotNull(adapter, nameof(adapter));
	}

	readonly IXmlCustomsLinkedObjectAdapter adapter;

	void IResponseMessageHandler.Handle(IXmlResponseMessage responseMessage)
	{
		if (responseMessage.State == MessageProcessorConstants.AidaXmlResponseStatusNumbers.Deposited)
		{
			adapter.SetStatusAsDeposited();
			adapter.SetLocalReferenceNumber(responseMessage.Lrn);
		}
	}
}
