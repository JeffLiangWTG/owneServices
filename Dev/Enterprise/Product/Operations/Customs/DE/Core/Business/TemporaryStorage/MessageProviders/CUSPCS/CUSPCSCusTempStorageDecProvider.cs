using System.Linq;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPCSCusTempStorageDecProvider : CusTempStorageDecProvider, ITempStorageDec
	{
		public CUSPCSCusTempStorageDecProvider(CusTempStorageDec storageDec)
			: base(storageDec)
		{
		}

		ITempStorageLine ITempStorageDec.FirstTempStorageLineDetails => firstTempStorageLineDetails ?? (firstTempStorageLineDetails = new CUSPCSConsolidatedCusTempStorageLineProvider(storageDec.CusTempStorageLines.Cast<CusTempStorageLine>().OrderBy(x => x.TSL_SystemCreateTimeUtc).First()));
	}
}
