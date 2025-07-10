using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class AdditionalTransitOperationValidation : CusSupportingInfoValidation
{
	public AdditionalTransitOperationValidation(AdditionalTransitOperation parent) : base(parent)
	{
	}

	protected override void CheckCSI_IssuerType()
	{
		base.CheckCSI_IssuerType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_IssuerTypeInfo);
	}
	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_QuantityInfo);
	}

	protected override void CheckCSI_PackQty()
	{
		base.CheckCSI_PackQty();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_PackQtyInfo);
	}

	protected override void CheckCSI_PackType()
	{
		base.CheckCSI_PackType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_PackTypeInfo);
	}

	protected override void CheckCSI_Status()
	{
		base.CheckCSI_Status();
		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_StatusInfo);
	}
}
