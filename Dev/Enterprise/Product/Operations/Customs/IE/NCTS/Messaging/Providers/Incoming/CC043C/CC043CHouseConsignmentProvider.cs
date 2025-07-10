using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CHouseConsignmentProvider
	{
		public CC043CHouseConsignmentProvider(HouseConsignmentType04 houseConsignment)
		{
			this.houseConsignment = Argument.NotNull(houseConsignment, nameof(houseConsignment));
		}
		readonly HouseConsignmentType04 houseConsignment;

		public string SequenceNumber => houseConsignment.SequenceNumber;
		public decimal GrossMass => houseConsignment.GrossMass;
		public string SecurityIndicatorFromExportDeclaration => houseConsignment.SecurityIndicatorFromExportDeclaration;

		public IReadOnlyCollection<CC043CDocumentProvider> TransportDocument => transportDocument ?? (transportDocument = houseConsignment.TransportDocument?.Select(x => new CC043CDocumentProvider(x)).ToArray() ?? Array.Empty<CC043CDocumentProvider>());
		IReadOnlyCollection<CC043CDocumentProvider> transportDocument;

		public IReadOnlyCollection<CC043CDocumentProvider> AdditionalReference => additionalReference ?? (additionalReference = houseConsignment.AdditionalReference?.Select(x => new CC043CDocumentProvider(x)).ToArray() ?? Array.Empty<CC043CPreviousDocumentWithGoodsItemNumberProvider>());
		IReadOnlyCollection<CC043CDocumentProvider> additionalReference;

		public IReadOnlyCollection<CC043CHouseConsignmentItemProvider> ConsignmentItem => consignmentItem ?? (consignmentItem = houseConsignment.ConsignmentItem?.Select(x => new CC043CHouseConsignmentItemProvider(x)).ToArray() ?? Array.Empty<CC043CHouseConsignmentItemProvider>());
		IReadOnlyCollection<CC043CHouseConsignmentItemProvider> consignmentItem;

		public IReadOnlyCollection<CC043CSupportingDocumentProvider> SupportingDocument => supportingDocument ?? (supportingDocument = houseConsignment.SupportingDocument?.Select(x => new CC043CSupportingDocumentProvider(x)).ToArray() ?? Array.Empty<CC043CSupportingDocumentProvider>());
		IReadOnlyCollection<CC043CSupportingDocumentProvider> supportingDocument;

		public IReadOnlyCollection<CC043CDepartureTransportMeansProvider> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = houseConsignment.DepartureTransportMeans?.Select(x => new CC043CDepartureTransportMeansProvider(x)).ToArray() ?? Array.Empty<CC043CDepartureTransportMeansProvider>());
		IReadOnlyCollection<CC043CDepartureTransportMeansProvider> departureTransportMeans;
	}
}
