using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceHeaderAdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
{
	public InvoiceHeaderAdditionalInfoValidation(InvoiceHeaderAdditionalInfo parent) : base(parent)
	{
	}

	protected override void CheckCSI_SubType()
	{
		base.CheckCSI_SubType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		CheckReferenceNumberIsEnteredIfNeeded(Parent.CSI_ReferenceNumberInfo);
	}

	#region Implementation

	protected virtual void CheckReferenceNumberIsEnteredIfNeeded(ZPropertyInfo targetPropertyInfo)
	{
		if (!targetPropertyInfo.ReadOnly)
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
		}
	}

	#endregion
}
