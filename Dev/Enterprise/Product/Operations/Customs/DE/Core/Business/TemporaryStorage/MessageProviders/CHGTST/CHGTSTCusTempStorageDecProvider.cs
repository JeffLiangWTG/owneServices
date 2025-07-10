using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGTSTCusTempStorageDecProvider : CusTempStorageDecProvider, ICHGTSTTempStorageDec
	{
		public CHGTSTCusTempStorageDecProvider(CusTempStorageDec storageDec)
			: base(storageDec)
		{
		}

		public string NewCustodianBranch => ((CHGTSTCusTempStorageDec)storageDec).NewCustodianBranch;

		public string NewCustodianEoriNumber => EU.Business.Extensions.GetEoriNumberWithFallback();

		public string OwnerReferenceNumber => storageDec.STH_OwnerReferenceNumber;

		IReadOnlyCollection<ITempStorageLine> ITempStorageDec.StorageLines => storageLines ?? (storageLines = storageDec.CusTempStorageLines.Cast<CusTempStorageLine>().Select(line => new CHGTSTCusTempStorageLineProvider(line)).ToArray());
	}
}
