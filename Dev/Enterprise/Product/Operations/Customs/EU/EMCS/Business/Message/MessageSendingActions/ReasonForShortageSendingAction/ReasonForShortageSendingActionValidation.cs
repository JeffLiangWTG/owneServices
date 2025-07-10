using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class ReasonForShortageSendingActionValidation : ZValidation
	{
		public ReasonForShortageSendingActionValidation(ReasonForShortageSendingAction parent) : base(parent)
		{
		}

		ReasonForShortageSendingAction Parent => (ReasonForShortageSendingAction)ParentFilter;
		public override Type AutoValidationType => typeof(ReasonForShortageSendingActionValidation);

		public void ValidateGeneralExplanation()
		{
			ValidateCalculatedProperty(Parent.GeneralExplanationInfo);
		}

		protected void CheckGeneralExplanation()
		{
			var info = Parent.GeneralExplanationInfo;
			MandatoryValidation.CheckEntered(info);
		}

		public override void ValidateAll()
		{
			ValidateGeneralExplanation();
		}
	}
}
