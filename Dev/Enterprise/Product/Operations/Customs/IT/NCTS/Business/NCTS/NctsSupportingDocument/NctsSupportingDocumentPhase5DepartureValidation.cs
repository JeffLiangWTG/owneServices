using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class NctsSupportingDocumentPhase5DepartureValidation : EU.NCTS.Business.NctsSupportingDocumentPhase5DepartureValidation
{
	public NctsSupportingDocumentPhase5DepartureValidation(NctsSupportingDocument parent)
		: base(parent)
	{
	}

	#region ValidationFunctions

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		CheckMandatoryFieldBasedOnAttributes(Parent.CSI_ReferenceNumberInfo, RefCusCodeListAttributeName.ReferenceNumber);
	}

	protected override void CheckCSI_ReferenceNumber2()
	{
		base.CheckCSI_ReferenceNumber2();
		CheckMandatoryFieldBasedOnAttributes(Parent.CSI_ReferenceNumber2Info, RefCusCodeListAttributeName.Complement);
	}

	protected override void CheckCSI_RN_NKCountryCode()
	{
		base.CheckCSI_RN_NKCountryCode();
		CheckMandatoryFieldBasedOnAttributes(Parent.CSI_RN_NKCountryCodeInfo, RefCusCodeListAttributeName.Country);
	}

	protected override void CheckCSI_DateOfIssue()
	{
		base.CheckCSI_DateOfIssue();
		CheckMandatoryFieldBasedOnAttributes(Parent.CSI_YearOfIssueInfo, RefCusCodeListAttributeName.Year);
	}

	#endregion

	#region Implementation

	void CheckMandatoryFieldBasedOnAttributes(ZPropertyInfo targetPropertyInfo, ZString attributeName)
	{
		if (Parent.RefCusCode.HasAttributeForMandatoryValidation(attributeName))
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
		}
	}

	#endregion

	new NctsSupportingDocument Parent => (NctsSupportingDocument)base.Parent;
}
