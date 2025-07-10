using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDAManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class AsycudaPreBoardingNotificationCollection : AsycudaManifestModuleCollection
	{
		public AsycudaPreBoardingNotificationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn) : base(factory)
		{
			this.currentCompanyPK = companyPkToFilterOn;
		}
		readonly ZGuid currentCompanyPK;

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			result.AddToFilter(AsycudaManifestHeaderSchema.AMA_ManifestType, ASYCUDAManifestTypes.Codes.PBN);

			if (currentCompanyPK.IsValid)
			{
				var query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
				var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), AsycudaManifestHeaderSchema.AMA_GB);
				branchQuery.AddToFilter(GlbBranchSchema.GB_GC, currentCompanyPK);
				query.AddSubQuery(branchQuery, JoinCondition.And);
				result.AddToFilter(query);
			}

			return result;
		}
	}
}
