using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaPackFetchStrategy : ASYCUDA.Business.AsycudaPackFetchStrategy
	{
		public AsycudaPackFetchStrategy(ASYCUDA.Business.AsycudaPack pack) : base(pack)
		{
		}

		protected override void AddCommonFetchHints()
		{
			var clusterKey = BusinessObject.APA_ClusterKey;
			var businessObjectPK = BusinessObject.PK;
			var fetchOnlyFromLocalCache = !BusinessObject.IsInDatabase;
			if (!BusinessObject.IsNonePackedItemRelationship)
			{
				Factory.AddFetchHintWithClusterKey(clusterKey, businessObjectPK, typeof(AsycudaPackPackedItemPivot), AsycudaPackPackedItemPivotSchema.APP_ClusterKey, AsycudaPackPackedItemPivotSchema.APP_APA_Pack, fetchOnlyFromLocalCache);
			}
			Factory.AddFetchHintWithClusterKey(clusterKey, businessObjectPK, typeof(AsycudaContainerBillOrPackageLink), AsycudaContainerBillOrPackageLinkSchema.APC_ClusterKey, AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack, fetchOnlyFromLocalCache);
		}
	}
}
