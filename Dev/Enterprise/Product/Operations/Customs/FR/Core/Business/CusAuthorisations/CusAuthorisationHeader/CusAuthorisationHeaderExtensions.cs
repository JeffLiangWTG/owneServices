using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business
{
	public static class CusAuthorisationHeaderExtensions
	{
		public static IEnumerable<CusAuthorisationHeader> GetCusAuthorisationHeadersWithApplyingAddressAndType(this OrgAddress orgAddress, ZString authType, ZString country)
		{
			if (orgAddress == null)
			{
				return Enumerable.Empty<CusAuthorisationHeader>();
			}

			var query = new ZQuery(CusPermitHeaderSchema.CPH_OA_AppliesTo, orgAddress.PK);
			query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, country);
			query.AddToFilter(CusPermitHeaderSchema.CPH_Type, authType);
			return orgAddress.Factory.Load<CusAuthorisationHeader>(query);
		}

		public static IEnumerable<CusAuthorisationHeader> GetCusAuthorisationHeadersWithApplyingAddress(this OrgAddress orgAddress, ZString country)
		{
			if (orgAddress == null)
			{
				return Enumerable.Empty<CusAuthorisationHeader>();
			}

			var query = new ZQuery(CusPermitHeaderSchema.CPH_OA_AppliesTo, orgAddress.PK);
			query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, country);
			return orgAddress.Factory.Load<CusAuthorisationHeader>(query);
		}

		public static IEnumerable<ZString> GetCusAuthorisationHeadersNumberWithApplyingAddressForCustomsWarehouse(this OrgAddress orgAddress, ZString country)
		{
			return orgAddress.GetCusAuthorisationHeadersWithApplyingAddressForCustomsWarehouse(country).Select(x => x.CPH_Number);
		}

		public static IEnumerable<CusAuthorisationHeader> GetCusAuthorisationHeadersWithApplyingAddressForCustomsWarehouse(this OrgAddress orgAddress, ZString country)
		{
			if (orgAddress == null)
			{
				return Enumerable.Empty<CusAuthorisationHeader>();
			}

			var query = new ZQuery(CusPermitHeaderSchema.CPH_OA_AppliesTo, orgAddress.PK);
			query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, country);
			query.AddToFilter(CusPermitHeaderSchema.CPH_Type, new ZString[]
			{
				Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP,
				Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1,
				Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2
			});
			return orgAddress.Factory.Load<CusAuthorisationHeader>(query);
		}

		public static IEnumerable<ZString> GetCusAuthorisationHeadersNumberWithType(this OrgHeader orgHeader, ZString authType, ZString country)
		{
			return orgHeader.GetCusAuthorisationHeadersWithType(authType, country).Select(x => x.CPH_Number);
		}

		public static IEnumerable<CusAuthorisationHeader> GetCusAuthorisationHeadersWithType(this OrgHeader orgHeader, ZString authType, ZString country)
		{
			if (orgHeader == null)
			{
				return Enumerable.Empty<CusAuthorisationHeader>();
			}

			var query = new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, orgHeader.PK);
			query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, country);
			query.AddToFilter(CusPermitHeaderSchema.CPH_Type, authType);
			return orgHeader.Factory.Load<CusAuthorisationHeader>(query);
		}

		public static IEnumerable<ZString> GetAuthorisationRuleValueWithCode(this CusAuthorisationHeader auth, ZString ruleCode)
		{
			return auth.GetAuthorisationRuleWithCode(ruleCode).Select(x => x.CPR_ValueFrom);
		}

		public static IEnumerable<CusAuthorisationRule> GetAuthorisationRuleWithCode(this CusAuthorisationHeader auth, ZString ruleCode)
		{
			if (auth == null)
			{
				return Enumerable.Empty<CusAuthorisationRule>();
			}

			return auth.CusAuthorisationRules.Cast<CusAuthorisationRule>().Where(x => x.CPR_RuleCode == ruleCode);
		}

		public static ZBool IsSpecificRegime(this CusAuthorisationHeader auth) => auth.CPH_Type == Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP
																					|| auth.CPH_Type == Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing
																					|| auth.CPH_Type == Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing
																					|| auth.CPH_Type == CusAuthorizationHeaderTypeList.Codes.OtherThanOpo
																					|| auth.CPH_Type == Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission
																					|| auth.CPH_Type == CusAuthorizationHeaderTypeList.Codes.TemporaryExportation;

		public static List<ZString> TypeThatHaveAfreeTextLOCRuleValueFrom => new List<ZString>
		{
			Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit,
			Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit,
			Customs.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir,
			Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing,
			Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission,
			Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing,
			CusAuthorizationHeaderTypeList.Codes.OtherThanOpo,
			CusAuthorizationHeaderTypeList.Codes.TemporaryExportation,
		};
	}
}
