using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business;

public class AdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
{
	public AdditionalInfoValidation(AdditionalInfo parent) : base(parent)
	{
	}

	protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
	}

	protected override void CheckCSI_SubType()
	{
		base.CheckCSI_SubType();

		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		if (!Parent.CSI_ReferenceNumberInfo.ReadOnly)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		}
	}
}
