using CargoWise.EntityFramework;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business.Declaration;

public class Ucc6ExportInvoiceHeaderAdditionalInfoValidation : InvoiceHeaderAdditionalInfoValidation
{
	public Ucc6ExportInvoiceHeaderAdditionalInfoValidation(InvoiceHeaderAdditionalInfo parent) : base(parent)
	{
	}

	protected new InvoiceHeaderAdditionalInfo Parent => (InvoiceHeaderAdditionalInfo)base.Parent;

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		CheckCSI_ReferenceNumberExceedsMaxLengthForTransportDocumentInTransitionPeriod(Parent);
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();
		CheckCSI_DescriptionExceedsMaxLengthForAdditionalInformationInTransitionPeriod(Parent);
	}

	#region Implementation

	protected override void CheckReferenceNumberIsEnteredIfNeeded(ZPropertyInfo targetPropertyInfo)
	{
		var parent = Parent;
		var attributeName = RefCusCodeListAttributeName.ReferenceNumber;
		const string attributeValue = RefCusCodeListAttributeValues.Yes;

		if (parent.IsATransportDocument
			|| (parent.IsAnAdditionalReference && (parent.RefCusCode?.HasAttribute(attributeName, attributeValue) ?? false)))
		{
			MandatoryValidation.MessageErrorIfNotEntered(targetPropertyInfo);
		}
	}

	void CheckCSI_ReferenceNumberExceedsMaxLengthForTransportDocumentInTransitionPeriod(InvoiceHeaderAdditionalInfo parent)
	{
		var declaration = Declaration;
		if (declaration is null)
		{
			return;
		}

		if (declaration.IsTransitionPeriodAES30
			&& parent.IsATransportDocument
			&& parent.CSI_ReferenceNumber.Length > Ucc6XmlConstants.CustomsFieldMaxLength.Ucc6ExportInvoiceHeaderAdditionalInfo.ReferenceNumber)
		{
			parent.CSI_ReferenceNumberInfo.AddMessageError(ValidationCaptions.Ucc6ExportInvoiceHeaderAdditionalInfo.ReferenceCannotBeLongerThanCustomsFieldMaxLengthInTransitionPeriodE1104);
		}
	}

	void CheckCSI_DescriptionExceedsMaxLengthForAdditionalInformationInTransitionPeriod(InvoiceHeaderAdditionalInfo parent)
	{
		var declaration = Declaration;
		if (declaration is null)
		{
			return;
		}

		if (declaration.IsTransitionPeriodAES30
			&& parent.IsAnAdditionalInformation
			&& parent.CSI_Description.Length > Ucc6XmlConstants.CustomsFieldMaxLength.Ucc6ExportInvoiceHeaderAdditionalInfo.Description)
		{
			parent.CSI_DescriptionInfo.AddMessageError(ValidationCaptions.Ucc6ExportInvoiceHeaderAdditionalInfo.DescriptionCannotBeLongerThanCustomsFieldMaxLengthInTransitionPeriodE1106);
		}
	}

	#endregion

	JobDeclaration Declaration => InvoiceHeader?.JobDeclaration;
	JobComInvoiceHeader InvoiceHeader => Parent?.Parent as JobComInvoiceHeader;
}
