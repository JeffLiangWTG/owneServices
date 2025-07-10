using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaManifestHeaderFetchStrategy : AsycudaFetchStrategy
	{
		public AsycudaManifestHeaderFetchStrategy(AsycudaManifestHeader manifestHeader)
			: base(manifestHeader)
		{
		}

		protected new AsycudaManifestHeader BusinessObject => (AsycudaManifestHeader)base.BusinessObject;

		protected override void AddCommonFetchHints()
		{
			base.AddCommonFetchHints();
			var clusterKey = BusinessObject.AMA_ClusterKey;
			Factory.AddFetchHint(typeof(AsycudaBill), AsycudaBillSchema.ABL_ClusterKey, clusterKey);
			Factory.AddFetchHint(typeof(AsycudaContainer), AsycudaContainerSchema.ACN_ClusterKey, clusterKey);
			Factory.AddFetchHint(typeof(AsycudaPack), AsycudaPackSchema.APA_ClusterKey, clusterKey);
			Factory.AddFetchHint(typeof(AsycudaBillScreening), AsycudaBillScreeningSchema.ASR_ClusterKey, clusterKey);
			Factory.AddFetchHint(typeof(AsycudaTax), AsycudaTaxSchema.AET_ClusterKey, clusterKey);
			Factory.AddFetchHint(typeof(AsycudaPackedItem), AsycudaPackedItemSchema.API_ClusterKey, clusterKey);
			Factory.AddFetchHint(typeof(AsycudaPackPackedItemPivot), AsycudaPackPackedItemPivotSchema.APP_ClusterKey, clusterKey);
			Factory.AddFetchHint(typeof(AsycudaContainerBillOrPackageLink), AsycudaContainerBillOrPackageLinkSchema.APC_ClusterKey, clusterKey);
		}

		protected override IEnumerable<BusinessObject> LoadChildren() => base.LoadChildren().Union(BusinessObject.Bills.Cast<BusinessObject>().Union(BusinessObject.Containers.Cast<BusinessObject>()));
	}
}
