namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusSupplyChainActorReferenceValidation : EU.Business.Declaration.CusSupplyChainActorReferenceValidation
	{
		public CusSupplyChainActorReferenceValidation(CusSupplyChainActorReference parent)
			: base(parent)
		{
		}

		protected new CusSupplyChainActorReference Parent => (CusSupplyChainActorReference)base.Parent;

		protected override void CheckCFR_Reference()
		{
			base.CheckCFR_Reference();

			const int lengthForCountry = 15;

			var cfrReference = Parent.CFR_Reference;
			var prefixCountry = cfrReference.SubstringSafe(0, NctsHelper.StandardCountryCodeLength).ToUpper();
			var suffixNumber = cfrReference.SubstringSafe(NctsHelper.StandardCountryCodeLength);

			if (Parent.ValidationDecider is { IsRuleR0840Active: true })
			{
				if (!NctsHelper.IsValidCountry(prefixCountry, Parent.Factory) || IsSuffixNumberInvalid(lengthForCountry))
				{
					Parent.CFR_ReferenceInfo.AddMessageError(Parent.NctsHeader.Configuration.ValidationRuleConfiguration.Messages.R0840Message);
				}
			}

			bool IsSuffixNumberInvalid(int maxLength)
			{
				return suffixNumber.IsEmpty || !suffixNumber.IsLettersAndNumbersOnlyOrEmpty || suffixNumber.Length > maxLength;
			}
		}
	}
}
