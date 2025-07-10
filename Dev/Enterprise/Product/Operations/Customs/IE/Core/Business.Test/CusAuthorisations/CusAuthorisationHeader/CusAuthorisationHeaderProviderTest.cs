using System;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CusAuthorisationHeaderProvider))]
	class CusAuthorisationHeaderProviderTest : EU.Business.Testing.EUCusAuthorisationHeaderProviderAbstractTest<CusAuthorisationHeaderProvider>
	{
		protected override CusAuthorisationHeaderProvider AuthorisationHeaderProvider => (CusAuthorisationHeaderProvider)authorisationHeader.Provider;

		protected override ZString AuthorisationHeaderCountryCode => Core.Constants.CountryCodes.Ireland;

		protected override bool ExpectedShowCustomsCode => true;

		protected override Type ExpectedRuleLookupsType => typeof(CusAuthorisationRuleLookups);

		protected override CodeDescriptionPairList ExpectedAuthorisationTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("OTH", "Other IE");
				result.AddPair("ACE", "Authorised Consignee Transit IE");
				result.Sort();
				return result;
			}
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, null, grouping);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, "AUTH", "ACE", "Authorised Consignee Transit IE", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, "AUTH", "OTH", "Other IE", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();
			base.SetUp();
		}
	}
}
