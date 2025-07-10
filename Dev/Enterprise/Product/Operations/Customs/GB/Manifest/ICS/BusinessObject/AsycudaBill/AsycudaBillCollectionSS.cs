namespace Enterprise.Customs.GB.ICS.Business;

public class AsycudaBillCollectionSS : ASYCUDA.Business.AsycudaBillCollection<EU.Manifest.Business.AsycudaBill, AsycudaManifestHeaderBase>
{
	public AsycudaBillCollectionSS(AsycudaManifestHeaderSS master) : base(master)
	{
	}
}
