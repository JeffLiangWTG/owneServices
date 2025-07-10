using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPCSConsolidatedCusTempStorageLineCollection : DecCusTempStorageLineCollectionFrom<CUSPCSConsolidatedCusTempStorageLine, CUSPCSCusTempStorageDec>
	{
		public CUSPCSConsolidatedCusTempStorageLineCollection(CUSPCSCusTempStorageDec parentStorageDec)
			: base(parentStorageDec)
		{
		}
	}
}
