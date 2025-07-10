namespace Enterprise.Customs.BE.NCTS.Business;

public class CommonPreviousDocumentValidation : EU.NCTS.Business.CommonPreviousDocumentValidation
{
	public CommonPreviousDocumentValidation(CommonPreviousDocument parent) : base(parent)
	{
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		PreviousDocumentValidationHelper.CheckCSI_ReferenceNumberN785(Parent.CSI_Code, Parent.CSI_ReferenceNumber, Parent.CSI_ReferenceNumberInfo);
	}

	protected override void CheckCSI_ReferenceNumber2()
	{
		base.CheckCSI_ReferenceNumber2();

		PreviousDocumentValidationHelper.CheckCSI_ReferenceNumber2N785(Parent.CSI_Code, Parent.CSI_ReferenceNumber2, Parent.CSI_ReferenceNumber2Info);
	}
}
