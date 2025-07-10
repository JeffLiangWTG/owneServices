using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public class JobWorkValidation : AutoINCusSupportingInfoValidation
{
	public JobWorkValidation(JobWork parent) : base(parent)
	{
	}

	protected override void CheckCSI_LineNo()
	{
		base.CheckCSI_LineNo();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_LineNoInfo);
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

	protected override void CheckCSI_CustomsOffice()
	{
		base.CheckCSI_CustomsOffice();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_CustomsOfficeInfo);
	}

	protected override void CheckCSI_ReferenceNumber2()
	{
		base.CheckCSI_ReferenceNumber2();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumber2Info);
	}

	protected override void CheckCSI_ItemNumber()
	{
		base.CheckCSI_ItemNumber();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ItemNumberInfo);
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_QuantityInfo);
	}

	protected override void CheckCSI_UnitOfQuantity()
	{
		base.CheckCSI_UnitOfQuantity();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_UnitOfQuantityInfo);
	}
}
