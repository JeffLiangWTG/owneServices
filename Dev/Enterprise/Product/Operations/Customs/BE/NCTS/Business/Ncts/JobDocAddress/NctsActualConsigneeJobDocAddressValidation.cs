using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public sealed class NctsActualConsigneeJobDocAddressValidation : EU.NCTS.Business.NctsActualConsigneeJobDocAddressValidation
{
	public NctsActualConsigneeJobDocAddressValidation(AutoJobDocAddress parent, MessageSendingAction messageSendingAction)
		: base(parent, messageSendingAction)
	{
	}

	MessageSendingAction MessageSendingAction => (MessageSendingAction)NctsHeaderMessageSendingObject;

	protected override void CheckOrganisationPK()
	{
		if (MessageSendingAction.EntryType == NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement)
		{
			CheckRuleTR0021(Parent.OrganisationPK, Parent.OrganisationPKInfo);
		}
		else
		{
			base.CheckOrganisationPK();
		}
	}

	void CheckRuleTR0021(ZGuid orgPk, ZPropertyInfo orgPkInfo)
	{
		if (orgPk.IsEmpty
			&& MessageSendingAction.ActualOfficeOfDestination.IsEmpty
			&& !MessageSendingAction.QueryInformation.IsEmpty)
		{
			orgPkInfo.AddError(NctsHeaderValidationHelper.TR0021ValidationMessage);
		}
	}
}
