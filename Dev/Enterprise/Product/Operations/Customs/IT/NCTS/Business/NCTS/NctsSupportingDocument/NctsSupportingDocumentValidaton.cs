using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSupportingDocumentValidation : EU.NCTS.Business.NctsSupportingDocumentPhase4Validation
{
	public NctsSupportingDocumentValidation(NctsSupportingDocument parent) : base(parent)
	{
	}

	protected new NctsSupportingDocument Parent => (NctsSupportingDocument)base.Parent;

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		CheckAeoCertificateOnGoodItem();
		CheckMandatoryFieldBasedOnAttributes(Parent.CSI_ReferenceNumberInfo, RefCusCodeListAttributeName.ReferenceNumber);
	}

	protected override void CheckCSI_RN_NKCountryCode()
	{
		base.CheckCSI_RN_NKCountryCode();
		var countryCodeInfo = Parent.CSI_RN_NKCountryCodeInfo;
		ListValidation.MessageErrorIfInvalidCode(countryCodeInfo);
		CheckMandatoryFieldBasedOnAttributes(countryCodeInfo, RefCusCodeListAttributeName.Country);
	}

	protected override void CheckCSI_DateOfIssue()
	{
		base.CheckCSI_DateOfIssue();
		CheckMandatoryFieldBasedOnAttributes(Parent.CSI_DateOfIssueInfo, RefCusCodeListAttributeName.Year);
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		CheckMandatoryFieldBasedOnAttributes(Parent.CSI_QuantityInfo, RefCusCodeListAttributeName.Quantity);
	}

	protected override void CheckCSI_UnitOfQuantity()
	{
		base.CheckCSI_UnitOfQuantity();
		CheckMandatoryFieldBasedOnAttributes(Parent.CSI_UnitOfQuantityInfo, RefCusCodeListAttributeName.UnitOfQuantity);
	}

	#region Implementation

	void CheckMandatoryFieldBasedOnAttributes(ZPropertyInfo targetPropertyInfo, ZString attributeName)
	{
		const string yes = "Y";
		if (Parent.RefCusCode?.HasAttribute(attributeName, yes) ?? false)
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
		}
	}

	#endregion

	#region AeoCertificateValidation

	void CheckAeoCertificateOnGoodItem()
	{
		if (Parent.Parent is NctsDepartureCargoDesc goodItem && goodItem.Header != null)
		{
			var aeoCertificateValidator = new AeoCertificateValidator(goodItem.Header, new NctsAeoCertificateOrganisationCaptionProvider());
			aeoCertificateValidator.CheckAEOCertificate(Parent);
		}
	}

	protected override void CheckCSI_DateOfIssueIsValidZDateTimeRange()
	{
		TypeValidation.CheckValidZDateTimeRange(Parent.CSI_DateOfIssueInfo, new TypeValidationLimits() { PastYearsBeforeError = DateRangeValidation.MaximumPastYears });
	}

	class NctsAeoCertificateOrganisationCaptionProvider : IAeoCertificateOrganisationCaptionProvider
	{
		public string SupplierCaption => Res.GetString("6585128F-17AD-4FA6-9919-E9B4F3100455", "Consignor");

		public string ImporterCaption => Res.GetString("19086184-AA91-4A90-94D5-88680CEEE20D", "Consignee");

		public string DeclarantCaption => Res.GetString("954DCD3E-6F7E-4C52-B0EC-B2CDB3EAD971", "Declarant");
	}

	#endregion
}
