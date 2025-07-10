using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class ActualConsigneeJobDocAddressRequirement : JobDocAddressRequirement
{
	public ActualConsigneeJobDocAddressRequirement(NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		this.sendingObject = sendingObject;
		ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = ValidateE2_OA_Address;
	}
	readonly NctsHeaderDepartureMessageSendingObject sendingObject;

	void ValidateE2_OA_Address(JobDocAddressValidation validation)
	{
		PassarValidation.CheckNS30034(validation.Parent.E2_OA_AddressInfo, sendingObject);
	}
}
