using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.LPCO.Outgoing;

namespace Enterprise.Customs.BR.Business.LPCO
{
	public class CompatibilityRequestProvider : ICompatibilityRequest
	{
		public CompatibilityRequestProvider(LPCORequestObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}

		readonly LPCORequestObject sendingObject;

		public string DocumentNumber => sendingObject.DocumentNumber;

		public int DocumentItemNumber => sendingObject.DocumentItemNumber;

		public string Version => sendingObject.Version;

		public string Reason => sendingObject.Reason;
	}
}
