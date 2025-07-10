using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CDS
{
	public class CusAuthorisationHeaderProvider : EU.Business.CusAuthorisationHeaderProvider
	{
		protected CusAuthorisationHeaderProvider(ZString countryCode) : base(countryCode)
		{
		}

		protected override Customs.Business.CusAuthorisationHeaderLookups GetNewLookupsCore(CusAuthorisationHeader cusAuthorisationHeader) => new CusAuthorisationHeaderLookups(cusAuthorisationHeader);

		protected override Customs.Business.CusAuthorisationRuleLookups GetNewLookupsCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleLookups(cusAuthorisationRule);

		protected override CodeDescriptionPairList GetAuthorisationTypeListCore(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.GB.CDS.CusAuthorisationHeaderProvider.GetAuthorisationTypeListCore", () =>
			{
				var authorizationTypes = new CDSAuthorisationHeaderTypeList();
				authorizationTypes.Sort();
				return authorizationTypes;
			});
		}

		protected override ZBool AllowMixedCaseAuthorisationNumbersCore(CusAuthorisationHeader header) =>
			header.CPH_Type == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit || header.CPH_Type == CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;

		protected override CodeDescriptionPairList GetRuleCodeListForModuleCore(BusinessObjectFactory factory) => factory.GetCachedValue<GBCusAuthorisationRuleTypeList>();

		protected override Dictionary<ZString, FieldType> GetRuleValueFieldTypesCore()
		{
			var result = base.GetRuleValueFieldTypesCore();
			result.Add(GBCusAuthorisationRuleTypeList.Codes.ORG, FieldType.TextCodeFindBox);
			result.Add(GBCusAuthorisationRuleTypeList.Codes.CTY, FieldType.TextDropEdit);
			return result;
		}

		protected override void SetupRuleDescriptionFunctions()
		{
			base.SetupRuleDescriptionFunctions();
			AddRuleDescriptionFunction(GBCusAuthorisationRuleTypeList.Codes.ORG, rule => GetOrgDescription(rule));
			AddRuleDescriptionFunction(GBCusAuthorisationRuleTypeList.Codes.CTY, rule => GetCTYDescription(rule));
		}

		protected override string GetRuleValueFromFieldTypeCore(CusAuthorisationRule cusAuthorisationRule)
		{
			var result = base.GetRuleValueFromFieldTypeCore(cusAuthorisationRule);

			if (cusAuthorisationRule != null)
			{
				var header = cusAuthorisationRule.AuthorisationHeader;
				var authTypes = new ZString[]
				{
					CDSAuthorisationHeaderTypeList.Codes.AuthorizedConsigneeTransit,
					CDSAuthorisationHeaderTypeList.Codes.AuthorizedConsignorTransit
				};
				if (header != null && cusAuthorisationRule.CPR_RuleCode == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location && header.CPH_Type.In(authTypes))
				{
					result = nameof(FieldType.Text);
				}
			}
			return result;
		}

		ZString GetOrgDescription(CommonCusPermitRule rule)
		{
			var result = ZString.Empty;
			var orgHeader = rule.Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, rule.CPR_ValueFrom));
			if (orgHeader != null)
			{
				result = orgHeader.OH_FullName;
			}
			return result;
		}

		ZString GetCTYDescription(CommonCusPermitRule rule)
		{
			return (string)rule.CPR_ValueFrom switch
			{
				GBCusAuthorisationCountryCodePrefixList.Codes.GB => (ZString)GBCusAuthorisationCountryCodePrefixList.Descriptions.GB,
				GBCusAuthorisationCountryCodePrefixList.Codes.XI => (ZString)GBCusAuthorisationCountryCodePrefixList.Descriptions.XI,
				_ => ZString.Empty,
			};
		}

		protected override bool ShowRelatedAuthorisationWithoutReferenceCore => true;

		protected override bool IsAgcNumberFieldALookupCore => false;
	}
}
