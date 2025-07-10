using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.Manifest.Module;

public class AEManifestBillModule : ASYCUDAManifestBillModule
{
	protected override IFilterControl GetNewFilterControl() => new AEManifestBillFilterStripControl(GridCollection, (AEManifestBillFilterStrip)FilterBusinessObject);

	protected override FilterBusinessObject GetNewFilterBusinessObject() => new AEManifestBillFilterStrip();

	protected override IBusinessObjectCollection GetNewGridCollection() => new AEManifestBillModuleCollection(Factory);
}
