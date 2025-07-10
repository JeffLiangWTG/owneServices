using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AE.Manifest.Business;

[ModuleID(ModuleId.ManifestBill)]
public class AsycudaBillFindBoxCollection : ActiveBusinessObjectCollection<AsycudaBill>
{
	public AsycudaBillFindBoxCollection(BusinessObjectFactory factory) : base(factory)
	{
	}
}

