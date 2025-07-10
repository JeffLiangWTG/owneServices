namespace Enterprise.Customs.DE.Module;

public abstract class SumAFilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	public SumAFilterInflator(SumAFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	protected new SumAFilterStripBusinessObject BizObj => base.BizObj as SumAFilterStripBusinessObject;
	protected SumAFilterStripBusinessObjectLookups Lookups => BizObj.Lookups;
}
