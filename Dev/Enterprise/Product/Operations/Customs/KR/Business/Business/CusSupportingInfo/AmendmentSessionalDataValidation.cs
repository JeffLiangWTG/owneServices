using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public class AmendmentSessionalDataValidation : Customs.Business.CusSupportingInfoValidation
	{
		public AmendmentSessionalDataValidation(AmendmentSessionalData parent) : base(parent)
		{
		}

		public new AmendmentSessionalData Parent
		{
			get { return (AmendmentSessionalData)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDutyPenaltyCause();
			ValidateTaxPenaltyCause();
		}

		public void ValidateDutyPenaltyCause()
		{
			ValidateCalculatedProperty(Parent.DutyPenaltyCauseInfo);
		}
		public void ValidateTaxPenaltyCause()
		{
			ValidateCalculatedProperty(Parent.TaxPenaltyCauseInfo);
		}

		protected void CheckDutyPenaltyCause()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.DutyPenaltyCauseInfo);
		}
		protected void CheckTaxPenaltyCause()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.TaxPenaltyCauseInfo);
		}
	}
}
