using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaPackedItemLookups : ASYCUDA.Business.AsycudaPackedItemLookups
	{
		public AsycudaPackedItemLookups(AsycudaPackedItem parent) : base(parent)
		{
		}

		public new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		public CodeDescriptionPairList PackStatusList => Factory.GetCachedValue<ILPackStatusList>();
	}
}
