using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AE.Module;

public class JobDeclarationFilterStripBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject, Integration.Customs.AE.IJobDeclarationFilterBusinessObject
{
	#region Lookups

	public new JobDeclarationFilterLookups Lookups
	{
		get { return (JobDeclarationFilterLookups)base.Lookups; }
	}

	protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
	{
		return new JobDeclarationFilterLookups(this);
	}

	protected override void AddShipmentSubTypeFilter(ModuleFilterCollection filters)
	{
	}

	#endregion
}
