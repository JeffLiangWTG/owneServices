using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public sealed class DfiaImportItemDetailValidation : AutoINCusSupportingInfoValidation
{
	public DfiaImportItemDetailValidation(DfiaImportItemDetail parent) : base(parent)
	{
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_QuantityInfo);
	}

	protected override void CheckCSI_IssuerType()
	{
		base.CheckCSI_IssuerType();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_IssuerTypeInfo);
	}
}
