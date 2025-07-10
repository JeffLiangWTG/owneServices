using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageDecProvider : ITempStorageDec
	{
		public CusTempStorageDecProvider(CusTempStorageDec storageDec)
		{
			this.storageDec = Argument.NotNull(storageDec, nameof(storageDec));
		}
		protected readonly CusTempStorageDec storageDec;

		public string IdentificationIndicator => storageDec.STH_IdentificationIndicator;

		public string AdditionalInformation => storageDec.STH_AdditionalInformation;

		public string InterchangeControlReference => EDIInterchange.InterchangeNumberPlaceHolder;

		public string MessageIdentifier => EDIMessage.SendersReferencePlaceHolder;

		public IReadOnlyCollection<ITempStorageLine> StorageLines => storageLines ?? (storageLines = storageDec.CusTempStorageLines.Cast<CusTempStorageLine>().Select(line => new CusTempStorageLineProvider(line)).ToArray());
		protected IReadOnlyCollection<ITempStorageLine> storageLines;

		public ITempStorageLine FirstTempStorageLineDetails => firstTempStorageLineDetails ?? (firstTempStorageLineDetails = new CusTempStorageLineProvider(storageDec.CusTempStorageLines.Cast<CusTempStorageLine>().OrderBy(x => x.TSL_SystemCreateTimeUtc).First()));
		protected ITempStorageLine firstTempStorageLineDetails;

		public ITempStorageHeader Header => header ?? (header = new SumACusTempStorageJobHeaderProvider(storageDec.StorageHeader));
		protected ITempStorageHeader header;
	}
}
