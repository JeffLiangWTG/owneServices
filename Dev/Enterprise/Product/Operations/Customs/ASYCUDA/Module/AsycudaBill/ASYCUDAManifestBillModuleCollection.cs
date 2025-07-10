using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.ASYCUDA.Module;

public class ASYCUDAManifestBillModuleCollection : ASYCUDAManifestBillModuleCollection<AsycudaBill>
{
	public ASYCUDAManifestBillModuleCollection(BusinessObjectFactory factory)
		: base(factory, ZString.Empty)
	{
	}
}
