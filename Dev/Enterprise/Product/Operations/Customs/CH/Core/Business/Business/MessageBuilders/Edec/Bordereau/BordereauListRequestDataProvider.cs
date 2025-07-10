using System;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.Bordereau;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public sealed class BordereauListRequestDataProvider : IEdecBordereauListRequest
{
	public BordereauListRequestDataProvider(BordereauListRequestSendingObject sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	}

	readonly BordereauListRequestSendingObject sendingObject;

	public string RequestorTraderIdentificationNumber => requestorTraderIdentificationNumber ??= GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;
	string requestorTraderIdentificationNumber;

	public string AccountNumber => accountNumber ??= GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.SwissCodeTypes.CAD);
	string accountNumber;

	public DateTime StartDate => sendingObject.StartDate.ToDateTime();

	public DateTime EndDate => sendingObject.EndDate.ToDateTime();
}
