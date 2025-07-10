using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Module;

public sealed class UCC6TemporaryStorageFilterInflatorFactory : EU.TemporaryStorage.Module.UCC6TemporaryStorageFilterInflatorFactory
{
	#region IE.UCC6TemporaryStorageFilterInflatorFactory Factory Methods

	public IFilterInflator CreateMessageVersionFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new MessageVersionFilterInflator(bizObj);

	#endregion
}
