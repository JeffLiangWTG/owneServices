using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM432GoodsShipmentProvider : IIM432GoodsShipment
	{
		readonly EntryHeaderWrapper entryHeaderWrapper;

		public IM432GoodsShipmentProvider(EntryHeaderWrapper entryHeaderWrapper)
		{
			this.entryHeaderWrapper = entryHeaderWrapper;
		}

		public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocumentsCached ?? (previousDocumentsCached = entryHeaderWrapper.Instruction.PreviousDocuments.Select(x => new DocumentProvider(x)).ToArray());
		IReadOnlyCollection<IDocument> previousDocumentsCached;

		public IMConsignment02 Consignment => CachedValueHelper.GetValue(ref consignmentCached, () => new MConsignment02Provider(entryHeaderWrapper));
		CachedValue<IMConsignment02> consignmentCached;
	}
}
