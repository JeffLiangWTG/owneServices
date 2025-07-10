using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class REXDISCusTempStorageReExportLineProvider : CusTempStorageLineProvider, IREXDISTempStorageLine
	{
		public REXDISCusTempStorageReExportLineProvider(CusTempStorageLine storageLine)
			: base(storageLine)
		{
		}

		public ITempStorageLine SumALine => sumALine ?? (sumALine = new CusTempStorageLineProvider(((REXDISCusTempStorageReExportLine)storageLine).SumALine));
		ITempStorageLine sumALine;
	}
}
