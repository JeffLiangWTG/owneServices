using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Module
{
	public class EUH7BillModuleCollection<T> : BusinessObjectCollection<T> where T : Business.AsycudaBill
	{
		public EUH7BillModuleCollection(BusinessObjectFactory factory)
			: base(factory, DefaultFilter)
		{
		}

		static ZQuery DefaultFilter
		{
			get
			{
				var branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
				branchSubQuery.AddToFilter(GlbBranchSchema.GB_GC, Env.CurrentCompanyPK);

				var headerSubQuery = new ZDBOnlySubQuery(typeof(Business.AsycudaManifestHeader), AsycudaManifestHeaderSchema.AMA_ClusterKey);
				headerSubQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, new[] { ApplicationCodeTypeList.Codes.EuH7, ApplicationCodeTypeList.Codes.EuH7V1, ApplicationCodeTypeList.Codes.EuH7V2 });
				headerSubQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestType, EUH7ManifestTypes.Codes.EH7);
				headerSubQuery.AddSubQuery(AsycudaManifestHeaderSchema.AMA_GB, branchSubQuery, JoinCondition.And);

				var result = new ZDBOnlyQuery(typeof(Business.AsycudaBill));
				result.AddSubQuery(AsycudaBillSchema.ABL_ClusterKey, headerSubQuery, JoinCondition.And);
				result.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, ASYCUDA.Business.AsycudaBill.ChildBolCode);
				return result;
			}
		}
	}
}
