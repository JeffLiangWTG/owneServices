using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Business
{
	public class CusEntryHeaderChargesValidation : Customs.Business.CusEntryHeaderChargesValidation
	{
		public CusEntryHeaderChargesValidation(CusEntryHeaderCharges parent)
			: base(parent)
		{
		}

		protected override void CheckC1_RateOverrideReasonCode()
		{
			base.CheckC1_RateOverrideReasonCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.C1_RateOverrideReasonCodeInfo);
		}
	}
}
