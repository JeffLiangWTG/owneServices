using CargoWise.Common;
using CancellationStatus = CargoWise.Customs.IT.MessageDefinitions.CancellationStatus;
using lXmlCustomsResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business;

sealed class CancellationResponseMessageSubProcessor : IResponseMessageSubProcessor
{
	public void ProcessMessage(IXmlCustomsLinkedObjectAdapter adapter, lXmlCustomsResponseMessage responseMessageWrapper)
	{
		Argument.NotNull(adapter, nameof(adapter));
		Argument.NotNull(responseMessageWrapper, nameof(responseMessageWrapper));

		var cancellationStatus = responseMessageWrapper.CancellationStatus;
		if (cancellationStatus == CancellationStatus.Rejected)
		{
			adapter.SetStatusAsError();
			return;
		}

		adapter.SetStatusAsAcceptedBySystem();

		if (cancellationStatus == CancellationStatus.Confirmed)
		{
			adapter.SetStatusAsCancelled();
			new A93NumberHandler(adapter, A93NumberHandleMode.Invalidate).Handle(responseMessageWrapper);
		}
	}
}
