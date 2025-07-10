using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaPackFetchStrategy : AsycudaFetchStrategy
	{
		public AsycudaPackFetchStrategy(AsycudaPack pack)
			: base(pack)
		{
		}

		protected new AsycudaPack BusinessObject => (AsycudaPack)base.BusinessObject;

		protected override void AddCommonFetchHints()
		{
			var clusterKey = BusinessObject.APA_ClusterKey;
			var businessObjectPK = BusinessObject.PK;
			var fetchOnlyFromLocalCache = !BusinessObject.IsInDatabase;
			if (!BusinessObject.IsNonePackedItemRelationship)
			{
				Factory.AddFetchHintWithClusterKey(clusterKey, businessObjectPK, typeof(AsycudaPackPackedItemPivot), AsycudaPackPackedItemPivotSchema.APP_ClusterKey, AsycudaPackPackedItemPivotSchema.APP_APA_Pack, fetchOnlyFromLocalCache);
				Factory.AddFetchHint(typeof(AsycudaPackedItem), BusinessObject.GetPackedItemsFromPivotQuery());
			}
			Factory.AddFetchHintWithClusterKey(clusterKey, businessObjectPK, typeof(AsycudaContainerBillOrPackageLink), AsycudaContainerBillOrPackageLinkSchema.APC_ClusterKey, AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack, fetchOnlyFromLocalCache);
		}

		protected override void AddFetchHintsForDeleteCore()
		{
			var clusterKey = BusinessObject.APA_ClusterKey;
			var fetchOnlyFromLocalCache = !BusinessObject.IsInDatabase;
			var businessObjectPK = BusinessObject.PK;
			if (!BusinessObject.IsNonePackedItemRelationship)
			{
				Factory.AddFetchHintWithClusterKey(clusterKey, businessObjectPK, typeof(AsycudaPackPackedItemPivot), AsycudaPackPackedItemPivotSchema.APP_ClusterKey, AsycudaPackPackedItemPivotSchema.APP_APA_Pack, fetchOnlyFromLocalCache);
				Factory.AddFetchHint(typeof(AsycudaPackedItem), BusinessObject.GetPackedItemsFromPivotQuery());
			}
			Factory.AddFetchHintWithClusterKey(clusterKey, businessObjectPK, typeof(AsycudaContainerBillOrPackageLink), AsycudaContainerBillOrPackageLinkSchema.APC_ClusterKey, AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack, fetchOnlyFromLocalCache);
		}

		protected override IEnumerable<BusinessObject> LoadChildren() => base.LoadChildren().Union(BusinessObject.PackedItems).Union(new[] { BusinessObject.Pivot });

		protected override IEnumerable<BusinessObject> LoadChildrenForDelete() => base.LoadChildrenForDelete().Union(BusinessObject.GetAllPackedItems());
	}
}

