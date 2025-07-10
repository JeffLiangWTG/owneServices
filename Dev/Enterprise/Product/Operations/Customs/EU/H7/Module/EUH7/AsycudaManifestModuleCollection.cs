using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Module
{
	public class AsycudaManifestModuleCollection : ASYCUDA.Module.AsycudaManifestModuleCollection
	{
		public AsycudaManifestModuleCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
			branchSubQuery.AddToFilter(GlbBranchSchema.GB_GC, Env.CurrentCompanyPK);

			var headerQuery = new ZDBOnlyQuery(typeof(Business.AsycudaManifestHeader));
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, new[] { ApplicationCodeTypeList.Codes.EuH7, ApplicationCodeTypeList.Codes.EuH7V1, ApplicationCodeTypeList.Codes.EuH7V2 });
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestType, EUH7ManifestTypes.Codes.EH7);
			headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.AMA_GB, branchSubQuery, JoinCondition.And);

			return headerQuery;
		}
	}
}
