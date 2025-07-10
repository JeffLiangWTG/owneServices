using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class DisableStaffCommissionAgreementsActionValidation : AutoDisableStaffCommissionAgreementsActionValidation
	{
		public DisableStaffCommissionAgreementsActionValidation(AutoDisableStaffCommissionAgreementsAction parent)
			: base(parent)
		{
		}

		protected override void CheckDate()
		{
			base.CheckDate();
			MandatoryValidation.CheckEntered(Parent.DateInfo);
		}
	}
}
