using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.LPCO.Outgoing;

namespace Enterprise.Customs.BR.Business.LPCO
{
	public class ConsentingBodyRequestProvider : IConsentingBodyRequest
	{
		public ConsentingBodyRequestProvider(LPCORequestObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}

		readonly LPCORequestObject sendingObject;

		public string Message => sendingObject.Message;
	}
}
