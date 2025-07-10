using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsPreviousDocumentPhase5RuleNR0046Validation
	{
		internal NctsPreviousDocumentPhase5RuleNR0046Validation(NctsPreviousDocument parent)
		{
			this.parent = Argument.NotNull(parent, nameof(parent));
		}

		internal void ValidateReferenceNumber(ValidationRuleMessages validationRuleMessages, bool isRuleActive, ZPropertyInfo referenceNumberInfo, ZString referenceNumber)
		{
			if (!isRuleActive
				|| referenceNumber.IsEmpty
				|| !UniversalValidationHelper.IsDocumentTypeIsMRN(parent.CSI_Code))
			{
				return;
			}

			var validationMessage = NctsPreviousDocumentPhase5RuleNRWithMrnValidationHelper.GetMrnValidationMessage(referenceNumber, validationRuleMessages.NR0046RuleCode);
			if (!validationMessage.IsEmpty)
			{
				referenceNumberInfo.AddMessageError(validationMessage);
			}
		}

		readonly NctsPreviousDocument parent;
	}
}
