using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class CusAuthorisationHeaderProvider : Customs.Business.CusAuthorisationHeaderProvider
	{
		public CusAuthorisationHeaderProvider(ZString countryCode) : base(countryCode)
		{
		}

		#region Authorisation Header
		protected override CodeDescriptionPairList GetAuthorisationTypeListCore(BusinessObjectFactory factory) => GetEuropeanUnionAuthorisationCusCodeList(factory);

		CodeDescriptionPairList GetEuropeanUnionAuthorisationCusCodeList(BusinessObjectFactory factory) =>
		RefCusCodeListTypes.GetCachedList(
				factory,
				CountryCode,
				UniversalReferenceConstants.RefCusCodeListType.Code.Code_AUTH,
				ZDateTime.Today
				);

		public CodeDescriptionPairList GetAuthorisationTypeListWithEuropeanUnionCustomsCodeInDescription(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue($"ZZRefCusCodeList_EUN_{CountryCode}_AUTH_WithDescription_{ZDateTime.Today}", () =>
			{
				var pairList = GetEuropeanUnionAuthorisationCusCodeList(factory);
				var maps = EUUniversalLookupsHelper.GetEuropeanUnionAuthorizationCustomsCodeMaps(factory, CountryCode).GroupBy(x => x.ZZM_CW1orCommercialValue, (key, g) => g.OrderBy(e => e.ZZM_ZZZ_NKDataGrouping != CountryCode).ThenBy(e => e.ZZM_StartDate).First());
				var codeDescriptions = from pair in pairList.Cast<ICodeDescription>()
									   join map in maps.Select(x => (Code: x.ZZM_CW1orCommercialValue.ToString(), Description: x.ZZM_CustomsValue.ToString())) on pair.Code equals map.Code into gj
									   from subMap in gj.DefaultIfEmpty()
									   select new CodeDescriptionPair(pair.Code, $"{subMap.Description} - {pair.Description}");
				var result = new CodeDescriptionPairList();
				foreach (var codeDescription in codeDescriptions)
				{
					result.Add(codeDescription);
				}
				return result;
			});
		}

		public ZString AuthorizedLocationCode => AuthorizedLocationCodeCore;

		protected virtual ZString AuthorizedLocationCodeCore => ZString.Empty;

		#endregion

		#region Authorisation Rules
		protected override Customs.Business.CusAuthorisationRuleLookups GetNewLookupsCore(CusAuthorisationRule cusAuthorisationRule) => new CusAuthorisationRuleLookups(cusAuthorisationRule);

		protected override string GetRuleValueFromFieldTypeCore(CusAuthorisationRule cusAuthorisationRule)
		{
			var result = base.GetRuleValueFromFieldTypeCore(cusAuthorisationRule);

			if (cusAuthorisationRule != null)
			{
				var header = cusAuthorisationRule.AuthorisationHeader;

				if (header != null && cusAuthorisationRule.CPR_RuleCode == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location && CusAuthorisationHeaderExtensions.TypeThatHaveAGuidLOCRuleValueFrom.Contains(header.CPH_Type))
				{
					result = nameof(FieldType.Guid);
				}
			}
			return result;
		}

		protected override string GetRuleDescriptionFieldTypeCore(CusAuthorisationRule cusAuthorisationRule)
		{
			var result = base.GetRuleDescriptionFieldTypeCore(cusAuthorisationRule);

			if (cusAuthorisationRule != null)
			{
				var header = cusAuthorisationRule.AuthorisationHeader;

				if (header != null && cusAuthorisationRule.CPR_RuleCode == Customs.Business.CusAuthorisationRuleTypeList.Codes.Location && CusAuthorisationHeaderExtensions.TypeThatHaveAGuidLOCRuleValueFrom.Contains(header.CPH_Type))
				{
					result = nameof(FieldType.GuidDropEdit);
				}
			}
			return result;
		}
		#endregion

		#region Linked Authorisation Rules
		protected override Customs.Business.LinkedCusAuthorisationRuleLookups GetNewLookupsCore(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => new LinkedCusAuthorisationRuleLookups(linkedCusAuthorisationRule);

		protected override void SetupLinkedRuleDescriptionFunctions()
		{
			base.SetupLinkedRuleDescriptionFunctions();
			AddLinkedRuleDescriptionFunction(LinkedCusAuthorisationRuleTypeList.Codes.Active, x => GetLinkedRuleDescriptionForActive(x));
			AddLinkedRuleDescriptionFunction(LinkedCusAuthorisationRuleTypeList.Codes.IEB, x => GetLinkedRuleDescriptionForIEB(x));
			AddLinkedRuleDescriptionFunction(LinkedCusAuthorisationRuleTypeList.Codes.OfficeType, x => GetLinkedRuleDescriptionForOfficeType(x));
		}

		ZString GetLinkedRuleDescriptionForActive(LinkedCusAuthorisationRule linkedCusAuthorisationRule)
		{
			var result = ZString.Empty;
			if (linkedCusAuthorisationRule != null)
			{
				var list = (CodeDescriptionPairList)linkedCusAuthorisationRule.Lookups.ValueList;
				result = list.GetDescriptionFromCode(linkedCusAuthorisationRule.CPR_ValueFrom);
			}
			return result;
		}

		ZString GetLinkedRuleDescriptionForIEB(LinkedCusAuthorisationRule linkedCusAuthorisationRule)
		{
			var result = ZString.Empty;
			if (linkedCusAuthorisationRule != null)
			{
				var list = (CodeDescriptionPairList)linkedCusAuthorisationRule.Lookups.ValueList;
				result = list.GetDescriptionFromCode(linkedCusAuthorisationRule.CPR_ValueFrom);
			}
			return result;
		}

		ZString GetLinkedRuleDescriptionForOfficeType(LinkedCusAuthorisationRule linkedCusAuthorisationRule)
		{
			var result = ZString.Empty;
			if (linkedCusAuthorisationRule != null)
			{
				var list = (CodeDescriptionPairList)linkedCusAuthorisationRule.Lookups.ValueList;
				result = list.GetDescriptionFromCode(linkedCusAuthorisationRule.CPR_ValueFrom);
			}
			return result;
		}

		protected override ZBool IsLinkedRuleCodeReadOnlyCore(LinkedCusAuthorisationRule linkedCusAuthorisationRule) => false;

		protected override string GetLinkedRuleValueFromFieldTypeCore(LinkedCusAuthorisationRule linkedCusAuthorisationRule)
		{
			var result = base.GetLinkedRuleValueFromFieldTypeCore(linkedCusAuthorisationRule);

			if (linkedCusAuthorisationRule != null)
			{
				var header = linkedCusAuthorisationRule.AuthorisationHeader;

				if (header != null && (linkedCusAuthorisationRule.CPR_RuleCode == LinkedCusAuthorisationRuleTypeList.Codes.Active || linkedCusAuthorisationRule.CPR_RuleCode == LinkedCusAuthorisationRuleTypeList.Codes.IEB || linkedCusAuthorisationRule.CPR_RuleCode == LinkedCusAuthorisationRuleTypeList.Codes.OfficeType))
				{
					result = nameof(FieldType.TextDropEdit);
				}
			}
			return result;
		}

		protected override Dictionary<ZString, FieldType> LinkedRuleValueFieldTypes
		{
			get
			{
				var result = base.LinkedRuleValueFieldTypes;
				result[LinkedCusAuthorisationRuleTypeList.Codes.Active] = FieldType.TextDropEdit;
				result[LinkedCusAuthorisationRuleTypeList.Codes.IEB] = FieldType.TextDropEdit;
				result[LinkedCusAuthorisationRuleTypeList.Codes.OfficeType] = FieldType.TextDropEdit;
				return result;
			}
		}
		#endregion
	}
}
