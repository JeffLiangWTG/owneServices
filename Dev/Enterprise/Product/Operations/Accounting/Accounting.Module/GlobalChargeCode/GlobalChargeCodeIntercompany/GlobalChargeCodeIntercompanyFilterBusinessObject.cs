using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class GlobalChargeCodeIntercompanyFilterBusinessObject : GlobalChargeCodeFilterBusinessObject
	{
		public GlobalChargeCodeIntercompanyFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			filters.AddGuidFilter("Job Local Client Override", ModuleIDs.Organisation, JobLocalClientOverrideFilter, OrganizationList).MultilingualDescription = ResString.GetMultilingualString("Accounting|GlobalChargeCodeFilter|JobLocalClientOverride", "Job Local Client Override");

			return filters;
		}

		ZQuery JobLocalClientOverrideFilter(ZGuid orgHeaderPK)
		{
			ZDBOnlyQuery globalChargeCodeQuery = new ZDBOnlyQuery(typeof(GlobalChargeCodeMap));

			ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(GlobalChargeCodeMapPivot), AccGlobalChargeCodeMapPivotSchema.YP_YG);
			pivotSubQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride, orgHeaderPK);
			globalChargeCodeQuery.AddSubQuery(AccGlobalChargeCodeMapSchema.PK, pivotSubQuery, JoinCondition.And);

			return globalChargeCodeQuery;
		}

		OrgHeaderCollection OrganizationList
		{
			get { return FindboxLookupCollections.GetOrgHeaderCollection(Factory);  }
		}
	}
}
