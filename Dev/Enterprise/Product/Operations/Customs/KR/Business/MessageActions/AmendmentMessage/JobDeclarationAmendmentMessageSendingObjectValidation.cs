using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationAmendmentMessageSendingObjectValidation : JobDeclarationMiscMessageSendingObjectCoreValidation
	{
		public JobDeclarationAmendmentMessageSendingObjectValidation(JobDeclarationAmendmentMessageSendingObject parent) : base(parent)
		{
		}
		protected new JobDeclarationAmendmentMessageSendingObject Parent => base.Parent as JobDeclarationAmendmentMessageSendingObject;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTaxPenaltyCause();
			ValidateDutyPenaltyCause();
			ValidateApplyDutyPenaltyReduction();
			ValidatePenaltyExemptionIndicator();
			ValidatePenaltyExemptionReasonCode();
			ValidatePenaltyExemptionReason();
		}
		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();
			if (Parent.ShouldSend)
			{
				if (Parent.AmendedItems.Count == 0)
				{
					Parent.ShouldSendInfo.AddError(Res.GetString("CC761A39-9513-4F06-865A-CB2EDCF64CB8", "There are no changes to send."));
				}

				switch (Parent.MessageType)
				{
					case ElectronicDocumentTypeList.Codes._5BB:
						if (Parent.AmendmentType == _5BBAmendmentType.Codes.Mix)
						{
							Parent.ShouldSendInfo.AddError(Res.GetString("50511963-0429-42A5-BA07-94190BC8FF8A", "You can add, delete or update entry lines but this should be done separately."));
						}
						break;

					case ElectronicDocumentTypeList.Codes._5FE:
						if (Parent.Header != null && Parent.IsIncluding5UAIn5FE)
						{
							var last5FEMessage = Parent.Header.Messages.GetLastMessageMatching(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5FE && !string.IsNullOrEmpty(x.EM_MessageOwner));
							if (last5FEMessage != null && !CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(last5FEMessage.MessageOrEntryStatus))
							{
								var last5UBMessage = Parent.Header.Messages.GetLastMessageMatching(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UB && x.EM_MessageSubType == last5FEMessage.EM_MessageType && x.EM_ApplicationReference == last5FEMessage.EM_MessageNum);
								if (last5UBMessage == null)
								{
									Parent.ShouldSendInfo.AddError(Res.GetString("BC2D6631-0D29-49CB-AEA2-427DA4B15E69", "You have indicated to include 5UA in 5FE. The previous 5UA request included in the last 5FE has not been reviewed by Customs. Please wait until its 5UB arrives."));
								}
							}

							var last5UAStandAloneMessage = Parent.Header.Messages.GetLastMessageMatching(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UA);
							if (last5UAStandAloneMessage != null)
							{
								var last5UBMessage = Parent.Header.Messages.GetLastMessageMatching(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UB && x.EM_ApplicationReference == last5UAStandAloneMessage.EM_MessageNum);
								if (last5UBMessage == null)
								{
									Parent.ShouldSendInfo.AddError(Res.GetString("21549673-31D5-43B1-96C2-9AD70D36DF04", "You have indicated to include 5UA in 5FE. The previous 5UA request has not been reviewed by Customs. Please wait until its 5UB arrives."));
								}
							}
						}
						break;
				}
			}
		}

		protected override void CheckFaultParty()
		{
			if (Parent.ShouldSend)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.FaultPartyInfo);
			}
		}

		public void ValidateTaxPenaltyCause()
		{
			ValidateCalculatedProperty(Parent.TaxPenaltyCauseInfo);
		}

		protected void CheckTaxPenaltyCause()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.TaxPenaltyCauseInfo);
		}

		public void ValidateDutyPenaltyCause()
		{
			ValidateCalculatedProperty(Parent.DutyPenaltyCauseInfo);
		}

		protected void CheckDutyPenaltyCause()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.DutyPenaltyCauseInfo);
		}

		public void ValidateApplyDutyPenaltyReduction()
		{
			ValidateCalculatedProperty(Parent.ApplyDutyPenaltyReductionInfo);
		}

		protected void CheckApplyDutyPenaltyReduction()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ApplyDutyPenaltyReductionInfo);
		}

		public void ValidatePenaltyExemptionIndicator()
		{
			ValidateCalculatedProperty(Parent.PenaltyExemptionIndicatorInfo);
		}
		protected void CheckPenaltyExemptionIndicator()
		{
			if (!Parent.IsPenaltyExemptionIrrelevant)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PenaltyExemptionIndicatorInfo);
			}
		}

		public void ValidatePenaltyExemptionReasonCode()
		{
			ValidateCalculatedProperty(Parent.PenaltyExemptionReasonCodeInfo);
		}
		protected void CheckPenaltyExemptionReasonCode()
		{
			if (Parent.IsPenaltyExemptionRequested)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PenaltyExemptionReasonCodeInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.PenaltyExemptionReasonCodeInfo);
			}
		}

		public void ValidatePenaltyExemptionReason()
		{
			ValidateCalculatedProperty(Parent.PenaltyExemptionReasonInfo);
		}
		protected void CheckPenaltyExemptionReason()
		{
			if (Parent.IsPenaltyExemptionRequested)
			{
				if (PenaltyExemptionReasonCodeList.IsLegalReasonCode(Parent.PenaltyExemptionReasonCode))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.PenaltyExemptionReasonInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.PenaltyExemptionReasonInfo);
				}
			}
		}

		public void ValidateRefundRequestSubmissionYN()
		{
			ValidateCalculatedProperty(Parent.RefundRequestSubmissionYNInfo);
		}
		protected void CheckRefundRequestSubmissionYN()
		{
			if (Parent.AmendmentType.SubstringSafe(0, 1) == DutyTaxCorrectionCodeList.Codes.C)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.RefundRequestSubmissionYNInfo);
			}
		}
	}
}
