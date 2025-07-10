using CargoWise.Common;

namespace Enterprise.Customs.IT.Business;

sealed class UnderControlDeclarationMessageHandler : IResponseMessageHandler
{
	public UnderControlDeclarationMessageHandler(IXmlCustomsLinkedObjectAdapter adapter)
	{
		this.adapter = Argument.NotNull(adapter, nameof(adapter));
	}

	readonly IXmlCustomsLinkedObjectAdapter adapter;

	void IResponseMessageHandler.Handle(CargoWise.Customs.IT.MessageDefinitions.IResponseMessage responseMessage)
	{
		if (responseMessage.State == MessageProcessorConstants.AidaXmlResponseStatusNumbers.UnderControl)
		{
			adapter.SetStatusAsUnderControl();
		}
	}
}
