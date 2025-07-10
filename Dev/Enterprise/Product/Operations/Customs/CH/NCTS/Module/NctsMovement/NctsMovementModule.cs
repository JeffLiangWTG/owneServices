using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.Module;

public class NctsMovementModule : EU.NCTS.Module.NctsMovementModule
{
	protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject() => new NctsMovementFilterStripBusinessObject();

	protected override IFilterControl GetNewFilterControl() => new NctsMovementFilterControl(GridCollection, FilterBusinessObject);
}
