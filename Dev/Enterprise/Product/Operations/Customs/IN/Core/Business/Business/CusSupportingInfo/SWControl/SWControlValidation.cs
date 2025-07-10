using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public class SWControlValidation : AutoINCusSupportingInfoValidation
{
	public SWControlValidation(SWControl parent) : base(parent)
	{
	}

	public new SWControl Parent => (SWControl)base.Parent;

	protected override void CheckCSI_ReferenceNumber2()
	{
		base.CheckCSI_ReferenceNumber2();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_ReferenceNumber2Info);
	}

	protected override void CheckCSI_ControlLocation()
	{
		base.CheckCSI_ControlLocation();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ControlLocationInfo);
	}

	protected override void CheckCSI_DateOfIssue()
	{
		base.CheckCSI_DateOfIssue();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DateOfIssueInfo);
	}

	protected override void CheckCSI_DateOfExpiry()
	{
		base.CheckCSI_DateOfExpiry();
		var parent = Parent;
		var propertyInfo = parent.CSI_DateOfExpiryInfo;

		MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);

		var dateOfExpiry = parent.CSI_DateOfExpiry;
		var dateOfIssue = parent.CSI_DateOfIssue;
		if (dateOfExpiry.IsValid && dateOfIssue.IsValid && dateOfExpiry < dateOfIssue)
		{
			propertyInfo.AddMessageError(Res.GetString("e3c9a2f7-1b5d-4c8e-8e2a-3f7b6d9c2a54", "The Control End Date cannot be earlier than the Control Start Date."));
		}
	}
}

