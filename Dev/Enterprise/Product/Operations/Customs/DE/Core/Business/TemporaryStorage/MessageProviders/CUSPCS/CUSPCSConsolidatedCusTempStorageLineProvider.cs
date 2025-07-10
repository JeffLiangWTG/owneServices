using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPCSConsolidatedCusTempStorageLineProvider : CusTempStorageLineProvider, ICUSPCSTempStorageLine
	{
		public CUSPCSConsolidatedCusTempStorageLineProvider(CusTempStorageLine storageLine)
			: base(storageLine)
		{
		}

		public IReadOnlyCollection<ICUSPCSSplitTempStorageLine> SplitLines => splitLines ?? (splitLines = ((CUSPCSConsolidatedCusTempStorageLine)storageLine).CusTempStorageLinesTo.Cast<CUSPCSSplitCusTempStorageLine>().Select(splitLine => new CUSPCSSplitCusTempStorageLineProvider(splitLine)).ToArray());
		IReadOnlyCollection<ICUSPCSSplitTempStorageLine> splitLines;
	}
}
