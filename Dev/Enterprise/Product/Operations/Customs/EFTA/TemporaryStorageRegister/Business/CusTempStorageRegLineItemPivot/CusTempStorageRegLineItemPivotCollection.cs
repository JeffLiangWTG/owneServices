using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegLineItemPivotCollection<TCusTempStorageRegLineItemPivot> : CusTempStorageRegLineItemPivotCollection
	where TCusTempStorageRegLineItemPivot : CusTempStorageRegLineItemPivot
{
	public CusTempStorageRegLineItemPivotCollection(BusinessObject master, bool includeChildren = true, bool includeParents = true) : base(master, includeChildren, includeParents)
	{
	}

	public new TCusTempStorageRegLineItemPivot this[int i] => (TCusTempStorageRegLineItemPivot)base[i];

	public new TCusTempStorageRegLineItemPivot AddNew() => (TCusTempStorageRegLineItemPivot)base.AddNew();
}

public abstract class CusTempStorageRegLineItemPivotCollection : PivotBusinessObjectCollection<CusTempStorageRegLineItemPivot>
{
	protected CusTempStorageRegLineItemPivotCollection(BusinessObject master, bool includeChildren = true, bool includeParents = true)
		: base(master, new CusTempStorageRegLineItemPivotCollectionRelationship(master), includeChildren, includeParents)
	{
	}
}
