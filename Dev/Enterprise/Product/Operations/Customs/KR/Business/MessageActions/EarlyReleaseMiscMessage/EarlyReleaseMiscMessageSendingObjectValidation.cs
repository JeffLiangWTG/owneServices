using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class EarlyReleaseMiscMessageSendingObjectValidation : JobDeclarationMiscMessageSendingObjectCoreValidation
	{
		public EarlyReleaseMiscMessageSendingObjectValidation(EarlyReleaseMiscMessageSendingObject parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			ValidateAmendmentReason();
			ValidateSecurityType();
			ValidateSecurityStartDate();
			ValidateSecurityEndDate();
			ValidateSecurityAmount();
			ValidateOtherSecurityType();
			ValidateReasonForEarlyRemoval();
		}

		protected override void CheckAmendmentReason()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AmendmentReasonInfo);
		}

		public void ValidateSecurityType()
		{
			ValidateCalculatedProperty(Parent.SecurityTypeInfo);
		}
		protected void CheckSecurityType()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.SecurityTypeInfo);
		}

		public void ValidateSecurityStartDate()
		{
			ValidateCalculatedProperty(Parent.SecurityStartDateInfo);
		}
		protected void CheckSecurityStartDate()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SecurityStartDateInfo);
			CompareValidation.CheckDateIsNotAfterAnotherDate(Parent.SecurityStartDateInfo, Parent.SecurityEndDateInfo);
		}

		public void ValidateSecurityEndDate()
		{
			ValidateCalculatedProperty(Parent.SecurityEndDateInfo);
		}
		protected void CheckSecurityEndDate()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SecurityEndDateInfo);
		}

		public void ValidateSecurityAmount()
		{
			ValidateCalculatedProperty(Parent.SecurityAmountInfo);
		}
		protected void CheckSecurityAmount()
		{
			MandatoryValidation.MessageErrorIfIsZero(Parent.SecurityAmountInfo);
			MandatoryValidation.MessageErrorIfIsNegative(Parent.SecurityAmountInfo);
		}

		public void ValidateOtherSecurityType()
		{
			ValidateCalculatedProperty(Parent.OtherSecurityTypeInfo);
		}
		protected void CheckOtherSecurityType()
		{
			if (Parent.SecurityType == SecurityTypeCodeList.Codes._99)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OtherSecurityTypeInfo);
			}
		}

		public void ValidateReasonForEarlyRemoval()
		{
			ValidateCalculatedProperty(Parent.ReasonForEarlyRemovalInfo);
		}
		protected void CheckReasonForEarlyRemoval()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ReasonForEarlyRemovalInfo);
		}

		protected new EarlyReleaseMiscMessageSendingObject Parent => base.Parent as EarlyReleaseMiscMessageSendingObject;
	}
}
