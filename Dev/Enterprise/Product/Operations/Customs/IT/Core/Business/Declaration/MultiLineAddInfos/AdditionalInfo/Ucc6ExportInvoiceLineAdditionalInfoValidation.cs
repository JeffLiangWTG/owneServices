using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business.Declaration;

public class Ucc6ExportInvoiceLineAdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
{
	public Ucc6ExportInvoiceLineAdditionalInfoValidation(AdditionalInfo parent)
		: base(parent)
	{
	}

	protected override void CheckCSI_SubType()
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		var parent = Parent;
		if (parent.IsReferenceNumberReadOnly)
		{
			return;
		}

		CheckMandatoryFieldBasedOnAttributes(parent, RefCusCodeListAttributeName.ReferenceNumber);
		CheckCSI_ReferenceNumberExceedsMaxLengthForTransportDocumentInTransitionPeriod(parent);
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();
		CheckCSI_DescriptionExceedsMaxLengthForAdditionalInformationInTransitionPeriod(Parent);
	}

	protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;

	protected override bool IsOtherFieldsEnabled => false;

	void CheckCSI_ReferenceNumberExceedsMaxLengthForTransportDocumentInTransitionPeriod(AdditionalInfo parent)
	{
		var declaration = Declaration;
		if (declaration is null)
		{
			return;
		}

		if (declaration.IsTransitionPeriodAES30
			&& parent.IsATransportDocument
			&& parent.CSI_ReferenceNumber.Length > Ucc6XmlConstants.CustomsFieldMaxLength.Ucc6ExportInvoiceLineAdditionalInfo.ReferenceNumber)
		{
			parent.CSI_ReferenceNumberInfo.AddMessageError(ValidationCaptions.Ucc6ExportInvoiceLineAdditionalInfo.ReferenceCannotBeLongerThanCustomsFieldMaxLengthInTransitionPeriodE1104);
		}
	}

	void CheckCSI_DescriptionExceedsMaxLengthForAdditionalInformationInTransitionPeriod(AdditionalInfo parent)
	{
		var declaration = Declaration;
		if (declaration is null)
		{
			return;
		}

		if (declaration.IsTransitionPeriodAES30
			&& parent.IsAnAdditionalInformation
			&& parent.CSI_Description.Length > Ucc6XmlConstants.CustomsFieldMaxLength.Ucc6ExportInvoiceLineAdditionalInfo.Description)
		{
			parent.CSI_DescriptionInfo.AddMessageError(ValidationCaptions.Ucc6ExportInvoiceLineAdditionalInfo.DescriptionCannotBeLongerThanCustomsFieldMaxLengthInTransitionPeriodE1106);
		}
	}

	void CheckMandatoryFieldBasedOnAttributes(AdditionalInfo parent, ZString attributeName)
	{
		const string attributeValue = RefCusCodeListAttributeValues.Yes;

		if (parent.IsATransportDocument
			|| (parent.IsAnAdditionalReference && (parent.RefCusCode?.HasAttribute(attributeName, attributeValue) ?? false)))
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ReferenceNumberInfo);
		}
	}

	JobDeclaration Declaration => InvoiceLine?.Declaration;
	JobComInvoiceLine InvoiceLine => Parent?.Parent as JobComInvoiceLine;
}
