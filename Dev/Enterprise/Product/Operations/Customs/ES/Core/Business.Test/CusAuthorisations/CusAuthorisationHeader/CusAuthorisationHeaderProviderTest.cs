using System;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CusAuthorisationHeaderProvider))]
	class CusAuthorisationHeaderProviderTest : EU.Business.Testing.EUCusAuthorisationHeaderProviderAbstractTest<CusAuthorisationHeaderProvider>
	{
		protected override Type ExpectedHeaderLookupsType => typeof(CusAuthorisationHeaderLookups);

		protected override CusAuthorisationHeaderProvider AuthorisationHeaderProvider => (CusAuthorisationHeaderProvider)authorisationHeader.Provider;

		protected override ZString AuthorisationHeaderCountryCode => Core.Constants.CountryCodes.Spain;

		protected override CodeDescriptionPairList ExpectedAuthorisationTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("OTH", "Other ES");
				result.AddPair("SAS", "Self-Assessment");
				result.AddPair("DPO", "Deferred");
				result.Sort();
				return result;
			}
		}

		protected override bool ExpectedShowCustomsCode => true; protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, null, grouping);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Spain, "AUTH", "OTH", "Other ES", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();
			base.SetUp();
		}
	}
}
