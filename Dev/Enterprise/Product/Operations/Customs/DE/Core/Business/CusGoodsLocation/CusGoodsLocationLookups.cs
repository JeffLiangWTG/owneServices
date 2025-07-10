using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class CusGoodsLocationLookups : EU.Business.CusGoodsLocationLookups
	{
		public CusGoodsLocationLookups(CusGoodsLocation cusGoodsLocation)
			: base(cusGoodsLocation)
		{
		}

		new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

		public override CodeDescriptionPairList QualifierList
		{
			get
			{
				return Factory.GetCachedValue("DE.CusGoodsLocationLookups.QualifierList", () =>
				{
					var result = new CusGoodsLocationQualifierList();
					result.RemoveCode(CusGoodsLocationQualifierList.Codes.PostcodeAddress);
					result.RemoveCode(CusGoodsLocationQualifierList.Codes.EoriNumber);
					return result;
				});
			}
		}

		public override ICollection UnlocodeList
		{
			get
			{
				return Factory.GetCachedValue("DE.CusGoodsLocationLookups.UnlocodeList", () =>
				{
					var result = new RefUNLOCOCollection(Factory);
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Economic Group", "Property", new ZString(EconomicGroupList.Codes.EuropeanUnion), true));
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Economic Group", "ComparisonOperator", (ZString)ModuleTextFilter.ComparisonConstants.Exact, true));
					return result;
				});
			}
		}

		public CodeDescriptionPairList AdditionalIdentifierList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var entryInstruction = Parent.Parent as Declaration.CusEntryInstruction;
				var declaration = entryInstruction?.JobDeclaration;
				var isExportDeclaration = declaration?.IsExport ?? false;
				if (isExportDeclaration)
				{
					var style = entryInstruction.CEI_Style;
					var partyConstellation = entryInstruction.ZG_PartyConstellation;
					var customsOfficeCode = declaration.JE_CustomsOffice;
					var declarantPK = declaration.Declarant?.Header.PK ?? ZGuid.Empty;
					var representativePK = declaration.Representative?.Header.PK ?? ZGuid.Empty;
					if (style.Length == 6 && partyConstellation.Length == 4 && !customsOfficeCode.IsEmpty && !(declarantPK.IsEmpty && representativePK.IsEmpty))
					{
						result = Factory.GetCachedValue(string.Join("|", "DE.CusGoodsLocationLookups.LoadingPlaceCodeList", style, partyConstellation, customsOfficeCode, declarantPK, representativePK),
							() =>
							{
								var list = new CodeDescriptionPairList();
								var filterRuleValue = GetFilterRuleValue(style);
								if (!filterRuleValue.IsEmpty)
								{
									var secondAndThirdNumInPartyConstellation = partyConstellation.Substring(1, 2);
									var thirdAndFourthNumInPartyConstellation = partyConstellation.Substring(2, 2);
									var onlyFromDeclarant = secondAndThirdNumInPartyConstellation == "00" || secondAndThirdNumInPartyConstellation == "10" || thirdAndFourthNumInPartyConstellation == "00";
									var fromDeclarantThenRepresentative = secondAndThirdNumInPartyConstellation == "01" || secondAndThirdNumInPartyConstellation == "11" || thirdAndFourthNumInPartyConstellation == "10";
									if (onlyFromDeclarant || fromDeclarantThenRepresentative)
									{
										if (!declarantPK.IsEmpty)
										{
											list = GetLocationRulesMatchingCustomsOffice(entryInstruction.Factory, declarantPK, filterRuleValue, customsOfficeCode);
										}
										if (fromDeclarantThenRepresentative && !representativePK.IsEmpty && list.Count == 0)
										{
											list = GetLocationRulesMatchingCustomsOffice(entryInstruction.Factory, representativePK, filterRuleValue, customsOfficeCode);
										}
									}
								}
								return list;
							});
					}
				}
				return result;
			}
		}

		ZString GetFilterRuleValue(ZString style)
		{
			var filterRuleValue = ZString.Empty;
			if (style.StartsWith("111"))
			{
				filterRuleValue = CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure;
			}
			else if (style.SubstringSafe(1, 2) == "01")
			{
				filterRuleValue = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
			}
			return filterRuleValue;
		}

		CodeDescriptionPairList GetLocationRulesMatchingCustomsOffice(BusinessObjectFactory factory, ZGuid authorizationHolderPk, ZString filterRuleValue, ZString customsOfficeCode)
		{
			return CusAuthorizationHelper.GetCachedRuleValuesFilteredByLinkedRules(factory
				, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration
				, new[] { authorizationHolderPk }
				, Customs.Business.CusAuthorisationRuleTypeList.Codes.Location
				, CusAuthorisationRuleTypeList.Codes.Usage
				, filterRuleValue
				, LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice
				, customsOfficeCode);
		}
	}
}
