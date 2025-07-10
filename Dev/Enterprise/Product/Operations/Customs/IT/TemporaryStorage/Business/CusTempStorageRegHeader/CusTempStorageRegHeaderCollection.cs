using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class CusTempStorageRegHeaderCollection : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>
{
	public CusTempStorageRegHeaderCollection(BusinessObjectFactory factory) : base(factory, ITConstants.TemporaryStorage.AppCodeTSR)
	{
	}
}
