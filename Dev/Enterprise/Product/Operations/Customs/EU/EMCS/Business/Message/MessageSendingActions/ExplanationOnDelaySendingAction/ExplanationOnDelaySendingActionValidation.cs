using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class ExplanationOnDelaySendingActionValidation : ZValidation
	{
		public ExplanationOnDelaySendingActionValidation(ExplanationOnDelaySendingAction parent) : base(parent)
		{
		}

		protected ExplanationOnDelaySendingAction Parent => (ExplanationOnDelaySendingAction)ParentFilter;

		public override Type AutoValidationType => typeof(ExplanationOnDelaySendingActionValidation);

		public override void ValidateAll()
		{
			ValidateExplanationCode();
			ValidateInformation();
			ValidateMessageRole();
		}

		public void ValidateExplanationCode()
		{
			ValidateCalculatedProperty(Parent.ExplanationCodeInfo);
		}

		protected void CheckExplanationCode()
		{
			MandatoryValidation.CheckEntered(Parent.ExplanationCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ExplanationCodeInfo);
		}

		public void ValidateInformation()
		{
			ValidateCalculatedProperty(Parent.InformationInfo);
		}

		protected void CheckInformation()
		{
			if (Parent.ExplanationCode == EMCSExplanationOnDelayCodeList.Codes.Other)
			{
				MandatoryValidation.CheckEntered(Parent.InformationInfo);
			}
		}

		public void ValidateMessageRole()
		{
			ValidateCalculatedProperty(Parent.MessageRoleInfo);
		}

		protected void CheckMessageRole()
		{
			MandatoryValidation.CheckEntered(Parent.MessageRoleInfo);
			ListValidation.ErrorIfInvalidCode(Parent.MessageRoleInfo);
		}
	}
}
