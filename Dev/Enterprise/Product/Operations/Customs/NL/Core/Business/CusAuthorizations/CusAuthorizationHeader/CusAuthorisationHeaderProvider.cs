using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public class CusAuthorisationHeaderProvider : EU.Business.CusAuthorisationHeaderProvider
{
	public CusAuthorisationHeaderProvider(ZString countryCode) : base(countryCode)
	{
	}

	protected override CodeDescriptionPairList GetAuthorisationTypeListCore(BusinessObjectFactory factory)
	{
		return factory.GetCachedValue("Enterprise.Customs.NL.Business.CusAuthorisationHeaderProvider.GetAuthorisationTypeListCore", () =>
		{
			var authorizationTypes = new CodeDescriptionPairList(base.GetAuthorisationTypeListCore(factory));
			authorizationTypes.AddRangeOverwriteIfExists(new NLCusAuthorisationHeaderTypeList());
			authorizationTypes.Sort();
			return authorizationTypes;
		});
	}

	protected override Customs.Business.CusAuthorisationRuleValidation GetNewValidationCore(Customs.Business.CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleValidation(cusAuthorisationRule);
}
