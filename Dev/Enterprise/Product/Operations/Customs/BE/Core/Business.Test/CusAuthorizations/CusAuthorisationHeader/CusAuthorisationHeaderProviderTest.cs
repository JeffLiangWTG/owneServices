using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CusAuthorisationHeaderProvider))]
sealed class CusAuthorisationHeaderProviderTest : EUCusAuthorisationHeaderProviderAbstractTest<CusAuthorisationHeaderProvider>
{
	protected override CusAuthorisationHeaderProvider AuthorisationHeaderProvider => new CusAuthorisationHeaderProvider(Core.Constants.CountryCodes.Belgium);

	protected override ZString AuthorisationHeaderCountryCode => Core.Constants.CountryCodes.Belgium;

	protected override Type ExpectedRuleValidationType => typeof(CusAuthorisationRuleValidation);

	protected override Type ExpectedRuleLookupsType => typeof(CusAuthorisationRuleLookups);

	protected override bool ExpectedShowCustomsCode => true;

	protected override CodeDescriptionPairList ExpectedAuthorisationTypeList
	{
		get
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("OTH", "Other BE");
			result.AddPair("SAS", "Self-Assessment");
			result.AddPair("DPO", "Deferred");
			result.Sort();
			return result;
		}
	}

	protected override void SetUp()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, null, grouping);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, "AUTH", "OTH", "Other BE", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		Factory.Save();
		base.SetUp();
	}
}
