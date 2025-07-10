using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public sealed class SupportingDocumentValidation : AutoINCusSupportingInfoValidation
{
	public SupportingDocumentValidation(AutoINCusSupportingInfo parent) : base(parent)
	{
	}

	protected override void CheckCSI_ReferenceNumber2()
	{
		base.CheckCSI_ReferenceNumber2();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumber2Info);
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		var codeInfo = Parent.CSI_CodeInfo;
		MandatoryValidation.WarnIfNotEntered(codeInfo);
		ListValidation.MessageErrorIfInvalidCode(codeInfo);
	}

	protected override void CheckCSI_IssuerType()
	{
		base.CheckCSI_IssuerType();
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_IssuerTypeInfo);
	}
}
