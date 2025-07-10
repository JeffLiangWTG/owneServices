using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Module;

public abstract class FilterInflator : EU.TemporaryStorage.Module.FilterInflator
{
	protected FilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	#region FilterInflator Implementation

	protected new TemporaryStorageHeader TemporaryStorageHeader => Factory.GetNull<TemporaryStorageHeader>();

	#endregion
}
