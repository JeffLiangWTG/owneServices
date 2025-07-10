using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class PreviousDocumentValidation : Customs.Business.CusSupportingInfoValidation
{
	public PreviousDocumentValidation(PreviousDocument parent) : base(parent) { }

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
	}
}
