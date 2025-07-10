using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusGuaranteeRuleTypedeciderTest : CountrySpecificTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var cusGuaranteeRule = bizO as CusGuaranteeRule;
			if (cusGuaranteeRule != null)
			{
				cusGuaranteeRule.GuaranteeHeader.CPH_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.MainAccessCode = "MAC1";
			var rule = (CusGuaranteeRule)guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = "TAR";
			rule.CPR_ValueFrom = "1";
			rule.CPR_ValueTo = "2";
			return rule;
		}

		protected override Type BaseTypeDecidedType => typeof(CusGuaranteeRule);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Constants.CountryGuids.Latvia, BaseTypeDecidedType },
				{ Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusGuaranteeRule>() },
				{ Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusGuaranteeRule>() },
				{ Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeRule>() },
				{ Constants.CountryGuids.FrenchGuiana, ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeRule>() },
				{ Constants.CountryGuids.Guadeloupe, ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeRule>() },
				{ Constants.CountryGuids.Martinique, ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeRule>() },
				{ Constants.CountryGuids.Mayotte, ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeRule>() },
				{ Constants.CountryGuids.Reunion, ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeRule>() },
				{ Constants.CountryGuids.SaintMartin, ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeRule>() },
				{ Constants.CountryGuids.SaintBarthelemy, ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeRule>() },
				{ Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusGuaranteeRule>() },
				{ Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusGuaranteeRule>() },
				{ Constants.CountryGuids._TemplateCountryName_, BaseTypeDecidedType }
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Constants.CountryCodes.Latvia, BaseTypeDecidedType },
				{ Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.ICusGuaranteeRule>() },
				{ Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.ICusGuaranteeRule>() },
				{ Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeRule>() },
				{ Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusGuaranteeRule>() },
				{ Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusGuaranteeRule>() },
				{ Constants.CountryCodes._TemplateCountryName_, BaseTypeDecidedType }
			};
		}
	}
}

