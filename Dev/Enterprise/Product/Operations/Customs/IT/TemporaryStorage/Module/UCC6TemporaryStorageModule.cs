using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public class UCC6TemporaryStorageModule : EU.TemporaryStorage.Module.UCC6TemporaryStorageModule
{
	protected override FilterBusinessObject GetNewFilterBusinessObject() => new UCC6TemporaryStorageFilterStripBusinessObject();

	protected override IFilterControl GetNewFilterControl() => new UCC6TemporaryStorageFilterControl(GridCollection, FilterBusinessObject);
}
