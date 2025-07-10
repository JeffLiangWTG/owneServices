namespace Enterprise.Customs.EU.TemporaryStorage.Module;

public abstract class UCC6TemporaryStorageFilterInflator : FilterInflator
{
	protected UCC6TemporaryStorageFilterInflator(UCC6TemporaryStorageFilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	#region UCC6TemporaryStorageFilterInflator Implementation

	protected new UCC6TemporaryStorageFilterStripBusinessObject BizObj => base.BizObj as UCC6TemporaryStorageFilterStripBusinessObject;

	#endregion
}
