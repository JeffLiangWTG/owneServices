using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class MovementReferenceNumberSupportingInfoValidation : CusSupportingInfoValidation
{
	public MovementReferenceNumberSupportingInfoValidation(MovementReferenceNumberSupportingInfo parent) : base(parent)
	{
	}

	protected new MovementReferenceNumberSupportingInfo Parent => (MovementReferenceNumberSupportingInfo)base.Parent;

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		CHNCTSValidationHelper.CheckGDRNorMRN(Parent.Factory, Parent.CSI_ReferenceNumberInfo);

		Parent.RemoveRowError(nameof(PassarValidationMessages.NZ50023), true);
		PassarValidation.CheckNZ50023(Parent.CSI_ReferenceNumberInfo, Parent);
	}

	protected override void CheckCSI_Status()
	{
		base.CheckCSI_Status();
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_StatusInfo);
	}
}
