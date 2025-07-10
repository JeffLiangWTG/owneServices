namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsHeaderValidation : EU.NCTS.Business.NctsHeaderPhase5Validation
	{
		public NctsHeaderValidation(NctsHeader parent) : base(parent)
		{
		}

		public new NctsHeader Parent => (NctsHeader)base.Parent;

		protected override bool EitherDispatchCountryOnGoodsItemOrOnHeaderMustBeFilled => false;

		protected override void CheckBH_RL_NKImportLoadPort()
		{
			//Intentionally Kept blank
		}
	}
}
