using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsCargoDescFeeValidation : EU.NCTS.Business.NctsCargoDescFeeValidation
	{
		public NctsCargoDescFeeValidation(NctsCargoDescFee parent) : base(parent)
		{
		}

		protected new NctsCargoDescFee Parent => (NctsCargoDescFee)base.Parent;

		protected override void CheckBFE_ChargeType()
		{
			base.CheckBFE_ChargeType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BFE_ChargeTypeInfo);
		}

		protected override void CheckBFE_RateOverrideReasonCode()
		{
			base.CheckBFE_RateOverrideReasonCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BFE_RateOverrideReasonCodeInfo);
		}
	}
}
