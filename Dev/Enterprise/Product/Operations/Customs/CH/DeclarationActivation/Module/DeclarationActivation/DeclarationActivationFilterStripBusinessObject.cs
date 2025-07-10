using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.DeclarationActivation.Module;

public sealed class DeclarationActivationFilterStripBusinessObject : FilterStripBusinessObject
{
	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var result = new ModuleFilterCollection();
		// To implemented by next WF of WI00894375
		return result;
	}

	protected override IBusinessObjectFetchStrategy GetFetchStrategy()
	{
		return base.GetFetchStrategy();
	}
}
