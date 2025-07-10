using System;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.LPCO.Outgoing;

namespace Enterprise.Customs.BR.Business.LPCO
{
	public class ApplyExtensionRequestProvider : IApplyExtensionRequest
	{
		public ApplyExtensionRequestProvider(LPCORequestObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		}

		readonly LPCORequestObject sendingObject;

		public string Reason => sendingObject.Reason;

		public DateTime? NewEffectiveDate => sendingObject.NewEffectiveDate.IsValid ? sendingObject.NewEffectiveDate.ToDateTime() : null;
	}
}
