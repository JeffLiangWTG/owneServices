using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common.US
{
	public static class HelperExtensions
	{
		public static ZString[] GetOrgProxySCACs(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("OrgProxySCACs", () =>
			{
				var query = new ZDBOnlyQuery(typeof(OrgCusCode));
				query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CarrierCode);
				query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
				query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, SQLComparisonOperator.NotEqual, ZString.Empty);
				var subQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.GC_OH_OrgProxy);
				subQuery.AddToFilter(GlbCompanySchema.GC_IsActive, ZBool.True);
				query.AddSubQuery(OrgCusCodeSchema.OK_OH, subQuery, JoinCondition.And);
				return factory.Load<OrgCusCode>(query).Select(x => x.OK_CustomsRegNo).Distinct().OrderBy(x => x).ToArray();
			});
		}
	}
}
