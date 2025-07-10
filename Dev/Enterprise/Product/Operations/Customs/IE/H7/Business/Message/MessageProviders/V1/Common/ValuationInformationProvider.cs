using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
using IDocument = CargoWise.Customs.IE.MessageContracts.Interfaces.IDocument;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class ValuationInformationProvider : IValuationInformation
	{
		public ValuationInformationProvider(AsycudaBill bill)
		{
			this.bill = bill;
		}
		readonly AsycudaBill bill;

		public IMoney TransportCosts => CachedValueHelper.GetValue(ref transportCostsCached, () => TransportCostsProvider.NewOrNull(bill));
		CachedValue<IMoney> transportCostsCached;

		public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ??=
			bill.AdditionalDocuments.Where(x => x.IsATransportDocument).Select(x => new DocumentProvider(x)).ToArray<IDocument>();
		IReadOnlyCollection<IDocument> transportDocuments;
	}
}
