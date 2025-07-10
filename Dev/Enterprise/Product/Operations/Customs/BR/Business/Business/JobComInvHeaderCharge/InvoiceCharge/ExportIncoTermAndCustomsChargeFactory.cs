using Enterprise.Customs.Common;

namespace Enterprise.Customs.BR.Business
{
	public class ExportIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		public override CustomsChargeCode GetOverseasFreight() => ExportChargesProvider.OverseasFreight;

		public override CustomsChargeCode GetOverseasInsurance() => ExportChargesProvider.OverseasInsurance;
	}
}
