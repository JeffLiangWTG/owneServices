using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	public class CusTempStorageLineCollection : CusTempStorageLineCollection<CusTempStorageLine, CusTempStorageDec>
	{
		public CusTempStorageLineCollection(CusTempStorageDec parentStorageDec) : base(parentStorageDec)
		{
		}
	}
}
