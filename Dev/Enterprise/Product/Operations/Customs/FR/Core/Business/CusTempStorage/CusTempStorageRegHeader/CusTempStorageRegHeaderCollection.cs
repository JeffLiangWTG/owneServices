using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageRegHeaderCollection : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>
	{
		public CusTempStorageRegHeaderCollection(BusinessObjectFactory factory) : base(factory, FRConstants.TemporaryStorage.AppCodeIST, FRConstants.TemporaryStorage.AppCodeSTO)
		{
		}
	}
}
