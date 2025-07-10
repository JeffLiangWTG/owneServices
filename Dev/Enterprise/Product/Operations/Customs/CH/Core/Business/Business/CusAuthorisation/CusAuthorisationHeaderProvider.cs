using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class CusAuthorisationHeaderProvider : Customs.Business.CusAuthorisationHeaderProvider
{
	protected CusAuthorisationHeaderProvider(ZString countryCode) : base(countryCode)
	{
	}

	protected override Customs.Business.CusAuthorisationHeaderValidation GetNewValidationCore(CusAuthorisationHeader cusAuthorisationHeader) => new CusAuthorisationHeaderValidation(cusAuthorisationHeader);

	protected override Customs.Business.CusAuthorisationRuleValidation GetNewValidationCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleValidation(cusAuthorisationRule);

	protected override CodeDescriptionPairList GetAuthorisationTypeListCore(BusinessObjectFactory factory)
	{
		return factory.GetCachedValue("Enterprise.Customs.CH.Business.CusAuthorisationHeaderProvider.GetAuthorisationTypeListCore", () =>
		{
			var authorizationTypes = new CusAuthorizationHeaderTypeList();
			return authorizationTypes;
		});
	}

	protected override Dictionary<ZString, FieldType> GetRuleValueFieldTypesCore()
	{
		var result = base.GetRuleValueFieldTypesCore();
		result[CusAuthorisationRuleTypeList.Codes.Location] = FieldType.Text;
		return result;
	}
}
