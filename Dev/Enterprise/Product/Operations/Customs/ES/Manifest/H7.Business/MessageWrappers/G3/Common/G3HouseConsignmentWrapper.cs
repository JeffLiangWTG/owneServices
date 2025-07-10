using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business.MessageWrappers.G3.Common
{
	public class G3HouseConsignmentWrapper : IG3HouseConsignment
	{
		public G3HouseConsignmentWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}

		readonly AsycudaBill bill;

		public IReadOnlyCollection<ICommonDocumentGoodsItemId> PreviousDocument => previousDocument ??= bill.PreviousDocuments.Select(d => new G3CommonPreviousDocumentWrapper(d)).ToList().AsReadOnly();
		IReadOnlyCollection<ICommonDocumentGoodsItemId> previousDocument;

		public IDocumentsCommon TransportDocument => CachedValueHelper.GetValue(ref transportDocument, () => new DocumentCommonWrapper(TransportDocumentCode, bill.ABL_BillNumber));
		CachedValue<IDocumentsCommon> transportDocument;

		const string TransportDocumentCode = "5025";
	}
}
