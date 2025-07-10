using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class CusTempStorageContainer : CusCodeData
{
	public CusTempStorageContainer(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CY_Type = CusCodeDataTypeList.Codes.TemporaryStorageContainer;
	}

	public new CusTempStorageRegLine Parent => (CusTempStorageRegLine)base.Parent;

	protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusTempStorageRegLine));
}
