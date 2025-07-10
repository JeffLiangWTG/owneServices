using System.Collections;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
{
	public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent)
		: base(parent)
	{
	}

	public ICollection CustomsOriginPortList => new RefUNLOCOCollection(Factory);
}
