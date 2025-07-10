using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusAuthorisationHeaderProvider))]
class CusAuthorisationHeaderProviderTest : CusAuthorisationHeaderProviderAbstractTest<CusAuthorisationHeaderProvider>
{
	protected override CusAuthorisationHeaderProvider AuthorisationHeaderProvider => (CusAuthorisationHeaderProvider)authorisationHeader.Provider;

	protected override ZString AuthorisationHeaderCountryCode => Core.Constants.CountryCodes.Switzerland;

	protected override Type ExpectedHeaderValidationType => typeof(CusAuthorisationHeaderValidation);

	protected override Type ExpectedRuleValidationType => typeof(CusAuthorisationRuleValidation);

	protected override CodeDescriptionPairList ExpectedAuthorisationTypeList
	{
		get
		{
			var authorizationTypes = new CusAuthorizationHeaderTypeList();
			return authorizationTypes;
		}
	}

	public void TestGetRuleValueFieldTypes()
	{
		var authorizationRule = authorisationHeader.CusAuthorisationRules.AddNew();
		authorizationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		AssertEquals(nameof(FieldType.Text), AuthorisationHeaderProvider.GetRuleValueFromFieldType(authorizationRule));
	}
}
