using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class REXDISCusTempStorageDecProvider : CusTempStorageDecProvider, IREXDISTempStorageDec
	{
		public REXDISCusTempStorageDecProvider(CusTempStorageDec storageDec) : base(storageDec)
		{
		}

		public string DeclarationSubType => storageDec.STH_DeclarationSubType;

		IReadOnlyCollection<ITempStorageLine> ITempStorageDec.StorageLines => storageLines ?? (storageLines = storageDec.CusTempStorageLines.Cast<CusTempStorageLine>().Select(line => new REXDISCusTempStorageReExportLineProvider(line)).ToArray());

		ITempStorageLine ITempStorageDec.FirstTempStorageLineDetails => firstTempStorageLineDetails ?? (firstTempStorageLineDetails = new REXDISCusTempStorageReExportLineProvider(storageDec.CusTempStorageLines.Cast<CusTempStorageLine>().OrderBy(x => x.TSL_SystemCreateTimeUtc).First()));

		ITempStorageHeader ITempStorageDec.Header => header ?? (header = new REXDISCusTempStorageJobHeaderProvider(storageDec.StorageHeader));
	}
}
