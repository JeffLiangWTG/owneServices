using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public sealed class DfiaExportItemDetailValidation : AutoINCusSupportingInfoValidation
{
	public DfiaExportItemDetailValidation(DfiaExportItemDetail parent) : base(parent)
	{
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
	}

	protected override void CheckCSI_DateOfIssue()
	{
		base.CheckCSI_DateOfIssue();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DateOfIssueInfo);
	}

	protected override void CheckCSI_ReferenceNumber2()
	{
		base.CheckCSI_ReferenceNumber2();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumber2Info);
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_QuantityInfo);
	}
}
