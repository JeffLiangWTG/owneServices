using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing;

[TestsSubclassesOf(typeof(CusAuthorisationHeaderProvider))]
public abstract class EUCusAuthorisationHeaderProviderAbstractTest<T> : CusAuthorisationHeaderProviderAbstractTest<T> where T : CusAuthorisationHeaderProvider
{
	protected override T AuthorisationHeaderProvider => (T)authorisationHeader.Provider;
	protected override ZString AuthorisationHeaderCountryCode => Core.Constants.CountryCodes.EuropeanUnion;
	protected override Type ExpectedLinkedRuleLookupsType => typeof(LinkedCusAuthorisationRuleLookups);
	protected override Type ExpectedRuleLookupsType => typeof(CusAuthorisationRuleLookups);

	protected override CodeDescriptionPairList ExpectedAuthorisationTypeList
	{
		get
		{
			var result = new CodeDescriptionPairList();
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
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, parent: grouping);
		helper.CreateCusCodeType("AUTH", "Authorisation");
		helper.CreateCusCodeList("EUN", "AUTH", "SAS", "Self-Assessment", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateCusCodeList("EUN", "AUTH", "DPO", "Deferred", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		Factory.Save();

		base.SetUp();
		authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
		linkedAuthorisationRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
	}
	protected CusAuthorisationRule authorisationRule;
	protected LinkedCusAuthorisationRule linkedAuthorisationRule;
}
