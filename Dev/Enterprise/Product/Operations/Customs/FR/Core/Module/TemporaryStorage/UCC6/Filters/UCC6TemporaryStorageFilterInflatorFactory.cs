using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Module;

public sealed class UCC6TemporaryStorageFilterInflatorFactory : EU.TemporaryStorage.Module.UCC6TemporaryStorageFilterInflatorFactory
{
	#region FR.UCC6TemporaryStorageFilterInflatorFactory Factory Methods

	public IFilterInflator CreateDeclarationTypeFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj)
		=> new DeclarationTypeFilterInflator(bizObj);

	#endregion
}
