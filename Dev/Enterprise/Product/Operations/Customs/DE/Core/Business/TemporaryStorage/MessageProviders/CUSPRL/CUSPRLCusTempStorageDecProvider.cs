using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPRLCusTempStorageDecProvider : CusTempStorageDecProvider, ICUSPRLTempStorageDec
	{
		public CUSPRLCusTempStorageDecProvider(CusTempStorageDec storageDec)
			: base(storageDec)
		{
		}

		public CUSPRLCusTempStorageDecProvider(CusTempStorageDec storageDec, Func<CusTempStorageLine, bool> lineFilter)
			: base(storageDec)
		{
			this.lineFilter = lineFilter;
		}
		readonly Func<CusTempStorageLine, bool> lineFilter;

		public string ATBNumber => storageDec.CusEntryNumber.CE_EntryNum;

		IReadOnlyCollection<ITempStorageLine> ITempStorageDec.StorageLines
		{
			get
			{
				if (storageLines == null)
				{
					var linesToSend = storageDec.CusTempStorageLines.Cast<CusTempStorageLine>().Where(l => l.TSL_CustomsStatus != CustomsStatusList.Codes.TST);
					if (lineFilter != null)
					{
						linesToSend = linesToSend.Where(lineFilter);
					}
					storageLines = linesToSend.Select(line => new CUSPRLCusTempStorageLineProvider(line)).ToArray();
				}
				return storageLines;
			}
		}

		ITempStorageLine ITempStorageDec.FirstTempStorageLineDetails => firstTempStorageLineDetails ?? (firstTempStorageLineDetails = new CUSPRLCusTempStorageLineProvider(storageDec.CusTempStorageLines.Cast<CusTempStorageLine>().OrderBy(x => x.TSL_SystemCreateTimeUtc).First()));

		ITempStorageHeader ITempStorageDec.Header => header ?? (header = new CUSPRLCusTempStorageJobHeaderProvider(storageDec.StorageHeader));
	}
}
