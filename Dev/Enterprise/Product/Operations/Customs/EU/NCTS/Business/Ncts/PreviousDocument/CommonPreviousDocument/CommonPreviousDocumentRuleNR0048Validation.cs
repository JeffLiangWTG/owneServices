using CargoWise.Common;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class CommonPreviousDocumentRuleNR0048Validation
	{
		internal CommonPreviousDocumentRuleNR0048Validation(CommonPreviousDocument parent)
		{
			this.parent = Argument.NotNull(parent, nameof(parent));
		}

		internal void ValidateReferenceNumber(ValidationRuleConfiguration validationRuleConfiguration)
		{
			var referenceNumber = parent.CSI_ReferenceNumber;

			if (!(validationRuleConfiguration?.IsRuleNR0048Active ?? false)
				|| !(parent.Header?.IsPhase5Departure ?? false)
				|| referenceNumber.IsEmpty
				|| parent.CSI_Code != NctsConstants.NctsTypeOfPreviousDocument.Codes.N830)
			{
				return;
			}

			var validationMessage = NctsPreviousDocumentPhase5RuleNRWithMrnValidationHelper.GetMrnValidationMessage(referenceNumber, validationRuleConfiguration.Messages.NR0048RuleCode);
			if (!validationMessage.IsEmpty)
			{
				parent.CSI_ReferenceNumberInfo.AddMessageError(validationMessage);
			}
		}

		readonly CommonPreviousDocument parent;
	}
}
