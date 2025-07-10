using CargoWise.EntityFramework;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.Customs.ASYCUDA.Module;

namespace Enterprise.Customs.AE.Manifest.Module;

public class AEManifestBillModuleCollection(BusinessObjectFactory factory) : ASYCUDAManifestBillModuleCollection<AsycudaBill>(factory, Core.Constants.CountryCodes.UnitedArabEmirates)
{
	protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
	{
		return new AEManifestBillModuleCollectionFetchStrategy(this);
	}
}
