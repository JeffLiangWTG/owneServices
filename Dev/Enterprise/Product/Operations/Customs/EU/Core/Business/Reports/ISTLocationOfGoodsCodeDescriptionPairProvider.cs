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
	public class ISTLocationOfGoodsCodeDescriptionPairProvider : CodeDescriptionPairList, Integration.Customs.EU.IISTLocationOfGoodsCodeDescriptionPairProvider, DocumentEngine.RuntimeOptions.IDependenceCodeDescriptionPairListProvider
	{
		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList(ZGuid addressPK)
		{
			return Factory.GetCachedValue($"ISTLocationOfGoodsCodeDescriptionPairProvider{addressPK}", () =>
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
				var authorisationHeaderProvider = Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(GlbCompany.CurrentCompany.Country.Code) as CusAuthorisationHeaderProvider;
				if (authorisationHeaderProvider != null && !authorisationHeaderProvider.AuthorizedLocationCode.IsEmpty)
				{
					query.AddToFilter(CusPermitHeaderSchema.CPH_Type, authorisationHeaderProvider.AuthorizedLocationCode);
				}
				if (!addressPK.IsEmpty)
				{
					query.AddToFilter(CusPermitHeaderSchema.CPH_OA_AppliesTo, addressPK);
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
