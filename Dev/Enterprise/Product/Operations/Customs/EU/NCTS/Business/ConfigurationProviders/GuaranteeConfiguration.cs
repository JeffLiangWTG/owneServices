namespace Enterprise.Customs.EU.NCTS.Business
{
	public class GuaranteeConfiguration
	{
		public bool OverrideSupport(NctsHeader header) => OverrideSupportCore(header);
		protected virtual bool OverrideSupportCore(NctsHeader header) => true;

		public bool ApplySecurityToPW_Override(NctsHeader header) => ApplySecurityToPW_OverrideCore(header);
		protected virtual bool ApplySecurityToPW_OverrideCore(NctsHeader header) => false;

		public INctsGuaranteeValidationDecider GetValidationDecider(NctsHeader header) => GetValidationDeciderCore(header);

		protected virtual INctsGuaranteeValidationDecider GetValidationDeciderCore(NctsHeader header)
		{
			if (header?.IsPhase5Departure ?? false)
			{
				return GetGuaranteeeDeparturePhase5ValidationDecider();
			}

			return null;
		}

		public int DefaultPercentageForLiabilityAmountCalculation => DefaultPercentageForLiabilityAmountCalculationCore;
		protected virtual int DefaultPercentageForLiabilityAmountCalculationCore => 25;

		public bool AllowDefaultLiabilityAmount => AllowDefaultLiabilityAmountCore;
		protected virtual bool AllowDefaultLiabilityAmountCore => false;

		public decimal DefaultLiabilityAmount => DefaultLiabilityAmountCore;
		protected virtual decimal DefaultLiabilityAmountCore => 10000m;

		public bool UseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods => UseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethodsCore;
		protected virtual bool UseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethodsCore => true;

		protected virtual INctsGuaranteePhase5ValidationDecider GetGuaranteeeDeparturePhase5ValidationDecider() => new NctsGuaranteeDeparturePhase5ValidationDecider();
	}
}
