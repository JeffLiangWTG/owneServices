using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE044HouseConsignmentProvider : IIE044HouseConsignment
	{
		public IE044HouseConsignmentProvider(NctsBill bill)
		{
			this.nctsBill = Argument.NotNull(bill, nameof(bill));
		}
		readonly NctsBill nctsBill;

		public decimal GrossMass => nctsBill.B0_GrossWeightUnloaded;

		public IReadOnlyCollection<ITransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans =
				nctsBill.DepartureTransportInfos
				.Select(d => new IE044DepartureTransportMeansProvider(d))
				.ToArray());
		IReadOnlyCollection<ITransportMeans> departureTransportMeans;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments =
				nctsBill.SupportingDocuments
				.Select(s => new SupportingDocumentProvider(s))
				.ToArray());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ?? (transportDocuments =
				MessageProviderHelper.FindSupportingDocuments(nctsBill, AdditionalInfoSubTypeList.Codes.TransportDocument)
				.Select(x => new DocumentProvider(x))
				.ToArray());
		IReadOnlyCollection<IDocument> transportDocuments;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences =
				MessageProviderHelper.FindSupportingDocuments(nctsBill, AdditionalInfoSubTypeList.Codes.AdditionalReference)
				.Select(x => new DocumentProvider(x))
				.ToArray());
		IReadOnlyCollection<IDocument> additionalReferences;

		public IReadOnlyCollection<IIE044ConsignmentItem> ConsignmentItems => consignmentItems ?? (consignmentItems =
				nctsBill.ArrivalGoodsItems
				.Where(x => !x.BY_UnloadedState.EqualsIgnoringCase(NctsUnloadedStateList.Codes.DEC))
				.Select(x => new IE044ConsignmentItemProvider(x))
				.ToArray());
		IReadOnlyCollection<IIE044ConsignmentItem> consignmentItems;
	}
}
