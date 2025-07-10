using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class PenaltyExemptionSessionalDataValidation : Customs.Business.CusSupportingInfoValidation
	{
		public PenaltyExemptionSessionalDataValidation(PenaltyExemptionSessionalData parent) : base(parent)
		{
		}

		public new PenaltyExemptionSessionalData Parent
		{
			get { return (PenaltyExemptionSessionalData)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateApplyDutyPenaltyReduction();
			ValidatePenaltyExemptionCode();
		}

		public void ValidateApplyDutyPenaltyReduction()
		{
			ValidateCalculatedProperty(Parent.ApplyDutyPenaltyReductionInfo);
		}

		protected void CheckApplyDutyPenaltyReduction()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ApplyDutyPenaltyReductionInfo);
		}

		public void ValidatePenaltyExemptionCode()
		{
			ValidateCalculatedProperty(Parent.PenaltyExemptionCodeInfo);
		}

		protected void CheckPenaltyExemptionCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.PenaltyExemptionCodeInfo);
		}
	}
}
