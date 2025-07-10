using System;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.Bordereau;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public sealed class BordereauRequestDataProvider : IEdecBordereauRequest
{
	public BordereauRequestDataProvider(BordereauRequestSendingObject sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	}

	readonly BordereauRequestSendingObject sendingObject;

	public string RequestorTraderIdentificationNumber => requestorTraderIdentificationNumber ??= GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
	string requestorTraderIdentificationNumber;

	public string BordereauNumber => sendingObject.BordereauNumber;

	public string ProcessingCenterNumber => sendingObject.ProcessingCenterNumber;

	public DateTime CreationDate => sendingObject.CreationDate.IsValid ? sendingObject.CreationDate.ToDateTime() : DateTime.MinValue;
}
