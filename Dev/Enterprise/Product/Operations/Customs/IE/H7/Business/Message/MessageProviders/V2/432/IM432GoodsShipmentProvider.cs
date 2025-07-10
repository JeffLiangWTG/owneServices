using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class IM432GoodsShipmentProvider : IIM432GoodsShipment
	{
		readonly AsycudaBill bill;

		public IM432GoodsShipmentProvider(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}

		public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocumentsCached
			?? (previousDocumentsCached = bill.PreviousDocuments
										.Select(document => new DocumentProvider(document))
										.ToArray());
		IReadOnlyCollection<IDocument> previousDocumentsCached;

		public IMConsignment02 Consignment => CachedValueHelper.GetValue(ref consignmentCached, () => new MConsignment02Provider(bill));
		CachedValue<IMConsignment02> consignmentCached;
	}
}
