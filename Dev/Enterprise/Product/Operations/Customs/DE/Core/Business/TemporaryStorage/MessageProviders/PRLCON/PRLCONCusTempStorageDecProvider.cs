using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class PRLCONCusTempStorageDecProvider : CusTempStorageDecProvider, IPRLCONTempStorageDec
	{
		public PRLCONCusTempStorageDecProvider(CusTempStorageDec storageDec)
			: base(storageDec)
		{
		}

		public IPRLCONConsolidatedTempStorageLine ConsolidatedTempStorageLineDetails => consolidatedTempStorageLineDetails ?? (consolidatedTempStorageLineDetails = new PRLCONConsolidatedCusTempStorageLineProvider(((PRLCONCusTempStorageDec)storageDec).ConsolidatedLine));
		IPRLCONConsolidatedTempStorageLine consolidatedTempStorageLineDetails;
	}
}
