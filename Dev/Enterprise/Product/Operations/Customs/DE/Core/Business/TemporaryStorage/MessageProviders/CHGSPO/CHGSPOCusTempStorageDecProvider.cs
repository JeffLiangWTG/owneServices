using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGSPOCusTempStorageDecProvider : CusTempStorageDecProvider, ICHGSPOTempStorageDec
	{
		public CHGSPOCusTempStorageDecProvider(CusTempStorageDec storageDec)
			: base(storageDec)
		{
		}

		public string OwnerReferenceNumber => storageDec.STH_OwnerReferenceNumber;
	}
}
