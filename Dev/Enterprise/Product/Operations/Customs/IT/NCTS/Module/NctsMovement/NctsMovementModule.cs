using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.Module;

public class NctsMovementModule : EU.NCTS.Module.NctsMovementModule
{
	protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
	{
		return new NctsMovementFilterStripBusinessObject();
	}

	protected override IFilterControl GetNewFilterControl()
	{
		return new NctsMovementFilterControl(GridCollection, FilterBusinessObject);
	}
}
