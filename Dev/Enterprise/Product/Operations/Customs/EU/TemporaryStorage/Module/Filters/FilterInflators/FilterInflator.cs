using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public abstract class FilterInflator : IFilterInflator
{
	protected FilterInflator(FilterStripBusinessObject bizObj)
	{
		BizObj = Argument.NotNull(bizObj, nameof(bizObj));
	}

	#region FilterInflator Implementation

	protected FilterStripBusinessObject BizObj { get; }
	protected BusinessObjectFactory Factory => BizObj.Factory;
	protected TemporaryStorageHeader TemporaryStorageHeader => Factory.GetNull<TemporaryStorageHeader>();
	protected TemporaryStoragePack TemporaryStoragePack => Factory.GetNull<TemporaryStoragePack>();

	#endregion

	#region IFilterInflator Implementation

	abstract public void InflateFilter(ModuleFilterCollection filterCollection);

	#endregion
}
