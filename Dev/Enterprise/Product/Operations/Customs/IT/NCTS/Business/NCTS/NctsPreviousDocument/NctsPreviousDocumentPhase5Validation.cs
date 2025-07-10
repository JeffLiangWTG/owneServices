using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using ValidationCaptions = Enterprise.Customs.IT.Business.ValidationCaptions;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class NctsPreviousDocumentPhase5Validation : EU.NCTS.Business.NctsPreviousDocumentPhase5Validation
{
	public NctsPreviousDocumentPhase5Validation(EU.NCTS.Business.NctsPreviousDocument parent) : base(parent)
	{
	}

	protected override void CheckCSI_SubType()
	{
	}

	protected override void CheckCSI_ItemNumberWithRefCusCodeAttribute(EU.NCTS.Business.NctsPreviousDocument parent)
	{
		if (parent.CSI_Code.In(previousDocumentTypesDontNeedCusCodeAttributeValidation))
		{
			return;
		}
		base.CheckCSI_ItemNumberWithRefCusCodeAttribute(parent);
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();

		ValidateQuantityTotalNumberOfDigits();
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		var parent = Parent;
		var referenceNumber = parent.CSI_ReferenceNumber;
		var referenceNumberInfo = parent.CSI_ReferenceNumberInfo;

		if (ValidationRuleConfiguration is not null && parent.IsPhase5Departure)
		{
			new NctsPreviousDocumentPhase5RuleNR0047Validation((NctsPreviousDocument)parent).ValidateReferenceNumber(ValidationRuleConfiguration.Messages, referenceNumberInfo, referenceNumber);
		}
	}

	protected override int QuantityPrecision => NctsPreviousDocument.Schema.CSI_QuantityPrecisionPhase5;

	protected override int QuantityScale => NctsPreviousDocument.Schema.QuantityDecimalPlacesPhase5;

	void ValidateQuantityTotalNumberOfDigits()
	{
		const int maximumValidNumberOfDigits = 16;
		const int maxNumberOfDigits = 19;

		var parent = Parent;

		var quantityNumberOfDigits = parent.CSI_Quantity.GetNumberOfSignificantDigits();
		if (quantityNumberOfDigits > maximumValidNumberOfDigits && quantityNumberOfDigits <= maxNumberOfDigits)
		{
			parent.CSI_QuantityInfo.AddMessageError(ValidationCaptions.PreviousDocument.PreviousDocumentsQuantityAllowedNumberOfDigits(maximumValidNumberOfDigits));
		}
	}

	readonly ImmutableArray<ZString> previousDocumentTypesDontNeedCusCodeAttributeValidation = new ZString[]
	{
		NctsConstants.NctsTypeOfPreviousDocument.Codes.C651,
		NctsConstants.NctsTypeOfPreviousDocument.Codes.C658,
	}.ToImmutableArray();
}
