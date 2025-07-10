using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business
{
	public class CusTempStorageRegLineSelCollection : ActiveBusinessObjectCollection<CusTempStorageRegLine>
	{
		public CusTempStorageRegLineSelCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}
	}
}
