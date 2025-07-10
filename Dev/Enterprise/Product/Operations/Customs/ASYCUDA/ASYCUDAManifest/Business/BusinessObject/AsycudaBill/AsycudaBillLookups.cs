using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDAManifest.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList SADOfficeCodeList => Parent.Header is AsycudaManifestHeader header ? header.Lookups.CustomsOffices as CodeDescriptionPairList : new CodeDescriptionPairList();
	}
}
