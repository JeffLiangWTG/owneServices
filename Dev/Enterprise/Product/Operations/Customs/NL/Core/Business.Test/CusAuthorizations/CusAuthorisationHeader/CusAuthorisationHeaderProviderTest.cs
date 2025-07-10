using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CusAuthorisationHeaderProvider))]
class CusAuthorisationHeaderProviderTest : EU.Business.Testing.EUCusAuthorisationHeaderProviderAbstractTest<CusAuthorisationHeaderProvider>
{
	protected override ZString AuthorisationHeaderCountryCode => Core.Constants.CountryCodes.Netherlands;

	protected override Type ExpectedHeaderLookupsType => typeof(Customs.Business.CusAuthorisationHeaderLookups);

	protected override Type ExpectedRuleValidationType => typeof(CusAuthorisationRuleValidation);

	protected override CodeDescriptionPairList ExpectedAuthorisationTypeList
	{
		get
		{
			var authorizationTypes = new CodeDescriptionPairList(base.ExpectedAuthorisationTypeList);
			authorizationTypes.AddRangeOverwriteIfExists(new NLCusAuthorisationHeaderTypeList());
			authorizationTypes.Sort();
			return authorizationTypes;
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		authorisationHeader.CPH_RN_NKCountryCode = AuthorisationHeaderCountryCode;
	}
	protected new CusAuthorisationHeader authorisationHeader;
}
