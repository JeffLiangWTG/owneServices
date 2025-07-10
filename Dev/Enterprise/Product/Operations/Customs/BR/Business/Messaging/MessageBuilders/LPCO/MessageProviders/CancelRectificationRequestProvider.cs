using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.LPCO.Outgoing;

namespace Enterprise.Customs.BR.Business.LPCO
{
	public class CancelRectificationRequestProvider : ICancelRectificationRequest
	{
		public CancelRectificationRequestProvider(LPCORequestObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}

		readonly LPCORequestObject sendingObject;

		public string Reason => sendingObject.Reason;
	}
}
