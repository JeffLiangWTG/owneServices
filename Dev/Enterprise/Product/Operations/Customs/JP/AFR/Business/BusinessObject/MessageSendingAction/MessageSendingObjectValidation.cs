using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class MessageSendingObjectValidation : AutoMessageSendingObjectValidation
	{
		public MessageSendingObjectValidation(AutoMessageSendingObject parent)
			: base(parent) { }

		#region Implementation

		public new MessageSendingObject Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (MessageSendingObject)base.Parent; }
		}

		protected override void CheckJPM_BillOfLadingNumber()
		{
			base.CheckJPM_BillOfLadingNumber();
			if (Parent.JPM_Send)
			{
				var billNumber = Parent.JPM_BillOfLadingNumber;
				var targetInfo = Parent.JPM_BillOfLadingNumberInfo;
				if (billNumber.IsEmpty)
				{
					targetInfo.AddError(ValidationConstants.MessageSending.BOLIsEmpty(targetInfo.HumanReadableName));
				}
				else if (billNumber.ExcludeChars(ValidationConstants.Constants.ValidNACCSCharactersForBillNumber).Length != 0)
				{
					targetInfo.AddError(ValidationConstants.Shared.InvalidNACCSCharInBOLNumberMessageForSending(billNumber));
				}
			}
		}

		protected override void CheckJPM_Send()
		{
			base.CheckJPM_Send();
			ValidateJPM_ActionCode();
			ValidateJPM_ReleaseStatus();
		}

		protected override void CheckJPM_ActionCode()
		{
			base.CheckJPM_ActionCode();
			if (Parent.JPM_Send)
			{
				var targetInfo = Parent.JPM_ActionCodeInfo;
				var actionPurpose = Parent.ActionCode;
				var actionCode = Parent.JPM_ActionCode;
				var isBillAlreadyRegistered = Parent.IsBillAlreadyRegistered;
				var hasATDBeenSent = Parent.HasATDBeenSent;
				var isMasterBillMarkedCompleted = Parent.IsMasterBillMarkedAsCompleted;

				if (actionCode == AFRSendingActionCodeList.Codes.Add && isBillAlreadyRegistered && !hasATDBeenSent)
				{
					Parent.JPM_ActionCodeInfo.AddMessageError(ValidationConstants.MessageSending.BillIsAlreadyRegisteredUseUpdateInstead);
				}
				else if (actionPurpose == ActionCode.NewBill && actionCode == AFRSendingActionCodeList.Codes.Register && (hasATDBeenSent || isMasterBillMarkedCompleted))
				{
					targetInfo.AddMessageError(ValidationConstants.MessageSending.UnordinaryActionCode(actionCode, AFRSendingActionCodeList.Codes.Add));
				}
				else if (actionPurpose == ActionCode.NewBill && actionCode == AFRSendingActionCodeList.Codes.Add && !hasATDBeenSent && !isMasterBillMarkedCompleted)
				{
					targetInfo.AddMessageError(ValidationConstants.MessageSending.UnordinaryActionCode(actionCode, AFRSendingActionCodeList.Codes.Register));
				}
				else if ((actionCode == AFRSendingActionCodeList.Codes.Update || actionCode == AFRSendingActionCodeList.Codes.Delete) &&
					Parent.HasATDBeenSent && !Parent.IsRiskAssessmentReceivedForBill)
				{
					targetInfo.AddMessageError(ValidationConstants.MessageSending.UpdateDeleteAreNotAllowForMasterWithATD);
				}
				else if (!targetInfo.ReadOnly)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
				}
			}
			ValidateJPM_DeleteReasonCode();
			ValidateJPM_DeleteReasonText();
		}

		protected override void CheckJPM_ReleaseStatus()
		{
			base.CheckJPM_ReleaseStatus();
			if (Parent.JPM_Send && Parent.IsRegisteringActionAndBillAlreadyOnFile)
			{
				Parent.JPM_ReleaseStatusInfo.AddMessageError(ValidationConstants.MessageSending.BillIsAlreadyRegisteredUseUpdateInsteadOfAddAction);
			}
		}

		protected override void CheckJPM_Calc_ESDT()
		{
			base.CheckJPM_Calc_ESDT();
			if (Parent.JPM_Send)
			{
				var targetValue = Parent.JPM_Calc_ESDT;
				if (targetValue.IsValid && targetValue < ValidationUtils.GetCurrentJPDate)
				{
					Parent.JPM_Calc_ESDTInfo.AddMessageError(ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);
				}
			}
		}

		protected override void CheckJPM_Calc_EFDT()
		{
			base.CheckJPM_Calc_EFDT();
			if (Parent.JPM_Send)
			{
				var targetValue = Parent.JPM_Calc_EFDT;
				if (targetValue.IsValid && targetValue < ValidationUtils.GetCurrentJPDate)
				{
					Parent.JPM_Calc_EFDTInfo.AddMessageError(ValidationConstants.InbondDetails.EstimatesDateShouldBeAfterSystemDate);
				}
			}
		}

		protected override void CheckJPM_DeleteReasonCode()
		{
			base.CheckJPM_DeleteReasonCode();
			if (Parent.IsBillSendAndDelete)
			{
				if (Parent.JPM_DeleteReasonCode.IsEmpty)
				{
					Parent.JPM_DeleteReasonCodeInfo.AddMessageError(ValidationConstants.MessageSending.DeleteReasonCodeIsRequired);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.JPM_DeleteReasonCodeInfo);
				}
			}
		}

		protected override void CheckJPM_DeleteReasonText()
		{
			base.CheckJPM_DeleteReasonText();
			if (Parent.IsBillSendAndDelete)
			{
				if (Parent.JPM_DeleteReasonText.IsEmpty)
				{
					Parent.JPM_DeleteReasonTextInfo.AddMessageError(ValidationConstants.MessageSending.ADeleteReasonMustBeSupplied);
				}
				else if (Parent.IsDeleteReasonCodeFreeTextRequired && Parent.JPM_DeleteReasonText == Parent.DeleteReasonCode.ZZD_Description)
				{
					Parent.JPM_DeleteReasonTextInfo.AddMessageError(ValidationConstants.MessageSending.ADeleteReasonMustBeSpecific);
				}
			}
		}

		protected override void CheckJPM_DeleteReasonTextIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.JPM_DeleteReasonTextInfo);
		}

		#endregion
	}
}
