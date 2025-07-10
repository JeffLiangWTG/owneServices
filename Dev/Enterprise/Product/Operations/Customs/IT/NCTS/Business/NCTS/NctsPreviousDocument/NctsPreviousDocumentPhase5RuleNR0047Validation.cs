using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class NctsPreviousDocumentPhase5RuleNR0047Validation
{
	internal NctsPreviousDocumentPhase5RuleNR0047Validation(NctsPreviousDocument parent)
	{
		this.parent = Argument.NotNull(parent, nameof(parent));
	}

	internal void ValidateReferenceNumber(EU.NCTS.Business.ValidationRuleMessages validationRuleMessages, ZPropertyInfo referenceNumberInfo, ZString referenceNumber)
	{
		if (referenceNumber.IsEmpty
			|| parent.CSI_Code != NctsConstants.NctsTypeOfPreviousDocument.Codes.NMRN)
		{
			return;
		}

		var nmrnFormatValidationMessage = ITPreviousDocumentValidationHelper.ValidateReferenceNumberFormatForNmrn(parent.Factory, referenceNumber);
		if (!string.IsNullOrEmpty(nmrnFormatValidationMessage))
		{
			referenceNumberInfo.AddMessageError(ValidationRuleMessages.FormatMessage(validationRuleMessages.NR0047RuleCode, nmrnFormatValidationMessage));
		}
	}

	readonly NctsPreviousDocument parent;
}
