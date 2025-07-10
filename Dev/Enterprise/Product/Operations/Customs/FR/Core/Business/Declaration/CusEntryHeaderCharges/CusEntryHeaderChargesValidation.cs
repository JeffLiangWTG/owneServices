using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryHeaderChargesValidation : Customs.Business.CusEntryHeaderChargesValidation
	{
		public CusEntryHeaderChargesValidation(Customs.Business.AutoCusEntryHeaderCharges parent) : base(parent)
		{
		}

		protected new CusEntryHeaderCharges Parent => (CusEntryHeaderCharges)base.Parent;

		protected override void CheckC1_RateOverrideReasonCode()
		{
			base.CheckC1_RateOverrideReasonCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.C1_RateOverrideReasonCodeInfo);
		}
	}
}
