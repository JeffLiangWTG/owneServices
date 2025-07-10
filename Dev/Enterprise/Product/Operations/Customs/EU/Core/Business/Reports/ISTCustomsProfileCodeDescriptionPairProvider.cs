using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Reports
{
	public class ISTCustomsProfileCodeDescriptionPairProvider : CodeDescriptionPairList, Integration.Customs.EU.IISTCustomsProfileCodeDescriptionPairProvider, DocumentEngine.RuntimeOptions.IDependenceCodeDescriptionPairListProvider
	{
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList(ZGuid orgPK)
		{
			return Factory.GetCachedValue($"ISTCustomsProfileCodeDescriptionPairProvider{orgPK}", () =>
			{
				var today = ZDate.Today;
				var query = new ZDBOnlyQuery(typeof(CusAuthorisationHeader));
				query.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Authorisation);
				query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Code);
				query.AddToFilter(CusPermitHeaderSchema.CPH_IsActive, true);
				query.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, today);
				var endDateQuery = new ZQuery(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);
				endDateQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, null);
				query.AddToFilter(endDateQuery);
				query.AddToFilter(CusPermitHeaderSchema.CPH_Type, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage);
				var ruleSubQuery = new ZDBOnlySubQuery(typeof(CusAuthorisationRule), CusPermitRuleSchema.CPR_CPH_PermitHeader);
				ruleSubQuery.AddToFilter(CusPermitRuleSchema.CPR_RuleCode, CusAuthorisationRuleTypeList.Codes.USE);
				ruleSubQuery.AddToFilter(CusPermitRuleSchema.CPR_ValueFrom, TemporaryStorageApplicationCodesList.Codes.IST);
				query.AddSubQuery(CusPermitHeaderSchema.PK, ruleSubQuery, JoinCondition.And);
				if (!orgPK.IsEmpty)
				{
					query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, orgPK);
				}
				return Factory.Load<CusAuthorisationHeader>(query).Aggregate(new CodeDescriptionPairList(), (list, auth) =>
				{
					list.AddPair(auth.CPH_Number, auth.CPH_PermitDescription);
					return list;
				});
			});
		}

		public ReadOnlyCodeDescriptionPairList GetDependenceCodeDescriptionPairList(string value)
		{
			ZGuid.TryParse(value, out ZGuid zguid);
			return GetCodeDescriptionPairList(zguid);
		}

		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			//return values must include result of GetDependenceCodeDescriptionPairList, otherwise report will fail to run.
			return GetCodeDescriptionPairList(ZGuid.Empty);
		}
	}
}
