using System.Collections.Generic;
using CargoWise.Common;
using IXmlCustomsResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business;

sealed class AmendmentResponseMessageSubProcessor : IResponseMessageSubProcessor
{
	public void ProcessMessage(IXmlCustomsLinkedObjectAdapter adapter, IXmlCustomsResponseMessage responseMessageWrapper)
	{
		Argument.NotNull(adapter, nameof(adapter));
		Argument.NotNull(responseMessageWrapper, nameof(responseMessageWrapper));
		if (responseMessageWrapper.IsRejectedAmendment)
		{
			adapter.SetStatusAsError();
			return;
		}

		adapter.SetStatusAsAcceptedBySystem();

		var messageHandlers = new List<IResponseMessageHandler>()
		{
			new ReleaseItemsHandler(adapter)
		};

		if (IsAmendmentConfirmed(responseMessageWrapper))
		{
			messageHandlers.Add(new A93NumberHandler(adapter, A93NumberHandleMode.InvalidateAndAdd));
			adapter.SetStatusAsAmended();
		}

		adapter.SetSentEntryLinesCount();
		messageHandlers.ForEach(h => h.Handle(responseMessageWrapper));
	}

	bool IsAmendmentConfirmed(IXmlCustomsResponseMessage responseMessage) => responseMessage.State == ConfirmedStatus;

	const int ConfirmedStatus = 4;
}
