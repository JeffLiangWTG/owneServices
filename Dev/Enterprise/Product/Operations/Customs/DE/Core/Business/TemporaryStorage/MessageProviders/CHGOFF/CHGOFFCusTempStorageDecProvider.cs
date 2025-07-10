using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGOFFCusTempStorageDecProvider : CusTempStorageDecProvider, ICHGOFFTempStorageDec
	{
		public CHGOFFCusTempStorageDecProvider(CusTempStorageDec storageDec)
			: base(storageDec)
		{
		}

		public string OwnerReferenceNumber => storageDec.STH_OwnerReferenceNumber;
	}
}
