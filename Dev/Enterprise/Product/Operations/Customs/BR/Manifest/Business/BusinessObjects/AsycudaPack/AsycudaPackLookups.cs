using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaPackLookups : ASYCUDA.Business.AsycudaPackLookups
	{
		public AsycudaPackLookups(AsycudaPack parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList BulkTypes => Factory.GetCachedValue<BRBulkTypeList>();
	}
}
