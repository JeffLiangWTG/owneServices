using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.Module;

public class CusPermitModule : Customs.Module.CusPermitModule
{
	public CusPermitModule()
	{
	}

	protected override IFilterControl GetNewFilterControl() => new CusPermitFilterControl(GridCollection, FilterBusinessObject);

	protected override Customs.Module.CusPermitFilterStripBusinessObject GetFilterStripBusinessObject() => new CusPermitFilterStripBusinessObject();
}
