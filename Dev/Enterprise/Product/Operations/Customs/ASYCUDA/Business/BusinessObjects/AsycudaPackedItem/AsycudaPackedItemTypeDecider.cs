using System;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackedItemTypeDecider : ManifestBase.AsycudaPackedItemTypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(AsycudaPackedItem);
		}

		public override Type GetTypeForNew()
		{
			return typeof(AsycudaPackedItem);
		}
	}
}
