using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

class CusAuthorisationHeaderProvider : EU.Business.CusAuthorisationHeaderProvider
{
	public CusAuthorisationHeaderProvider(ZString countryCode) : base(countryCode)
	{
	}

	protected override Customs.Business.CusAuthorisationRuleValidation GetNewValidationCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleValidation(cusAuthorisationRule);

	protected override Customs.Business.CusAuthorisationRuleLookups GetNewLookupsCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleLookups(cusAuthorisationRule);

	protected override bool ShowCustomsCodeCore => true;
}
