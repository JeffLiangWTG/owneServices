using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class CusTempStorageRegLineItemPivot : EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot, Integration.Customs.ES.ICusTempStorageRegLineItemPivot
{
	public CusTempStorageRegLineItemPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public override void Delete()
	{
		var line = RegLineItem;
		if (!IsDeleted
			&& line != null
			&& !line.IsDeleted)
		{
			line.Delete();
		}

		base.Delete();
	}
}
