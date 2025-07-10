using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaBillFetchStrategy : AsycudaFetchStrategy
	{
		public AsycudaBillFetchStrategy(AsycudaBill bill)
			: base(bill)
		{
		}

		protected new AsycudaBill BusinessObject => (AsycudaBill)base.BusinessObject;

		protected override void AddCommonFetchHints()
		{
			Factory.AddFetchHintWithClusterKey(BusinessObject.ABL_ClusterKey, BusinessObject.PK, typeof(AsycudaPack), AsycudaPackSchema.APA_ClusterKey, AsycudaPackSchema.APA_ABL_Bill, !BusinessObject.IsInDatabase);
			Factory.AddFetchHintWithClusterKey(BusinessObject.ABL_ClusterKey, BusinessObject.PK, typeof(AsycudaPackedItem), AsycudaPackedItemSchema.API_ClusterKey, AsycudaPackedItemSchema.API_ABL_Bill, !BusinessObject.IsInDatabase);
			if (BusinessObject is IAsycudaTaxTypeSupporter taxTypeSupporter)
			{
				Factory.AddFetchHintWithClusterKey(BusinessObject.ABL_ClusterKey, BusinessObject.PK, taxTypeSupporter.GetAsycudaTaxType(), AsycudaTaxSchema.AET_ClusterKey, AsycudaTaxSchema.AET_ABL, !BusinessObject.IsInDatabase);
			}
			Factory.AddFetchHintWithClusterKey(BusinessObject.ABL_ClusterKey, BusinessObject.PK, typeof(AsycudaContainerBillOrPackageLink), AsycudaContainerBillOrPackageLinkSchema.APC_ClusterKey, AsycudaContainerBillOrPackageLinkSchema.APC_ABL_Bill, !BusinessObject.IsInDatabase);
			if (BusinessObject is IAsycudaBillScreeningTypeSupporter billScreeningTypeSupporter)
			{
				Factory.AddFetchHintWithClusterKey(BusinessObject.ABL_ClusterKey, BusinessObject.PK, billScreeningTypeSupporter.GetAsycudaBillScreeningType(), AsycudaBillScreeningSchema.ASR_ClusterKey, AsycudaBillScreeningSchema.ASR_ABL, !BusinessObject.IsInDatabase);
			}
		}

		protected override IEnumerable<BusinessObject> LoadChildren() => base.LoadChildren().Union(BusinessObject.Packs.Cast<AsycudaPack>()); //.Union(BusinessObject.AsycudaTaxes).Union(BusinessObject.BillScreenings);
	}
}

