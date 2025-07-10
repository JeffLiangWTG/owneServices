using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegLineItemPivotCollectionRelationship : CollectionRelationship
{
	public CusTempStorageRegLineItemPivotCollectionRelationship(BusinessObject master)
		: base(typeof(CusTempStorageRegLineItemPivot), GetQueryFilter(master))
	{
	}

	public static ZQuery GetQueryFilter(BusinessObject master) => new(CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line, master.PK);
}
