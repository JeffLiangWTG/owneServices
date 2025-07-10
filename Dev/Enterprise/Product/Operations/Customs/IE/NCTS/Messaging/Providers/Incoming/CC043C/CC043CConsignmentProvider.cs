using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CConsignmentProvider
	{
		readonly ConsignmentType05 consignment;

		public CC043CConsignmentProvider(ConsignmentType05 consignment)
		{
			this.consignment = Argument.NotNull(consignment, nameof(consignment));
		}

		public ZDecimal GrossMass => consignment.GrossMass ?? ZDecimal.Zero;

		public ZBool ContainerIndicator => consignment.ContainerIndicator == Flag.Item1;

		public CC043CConsignorProvider Consignor => CachedValueHelper.GetValue(ref consignor, () => consignment.Consignor == null ? null : new CC043CConsignorProvider(consignment.Consignor));
		CachedValue<CC043CConsignorProvider> consignor;

		public CC043CConsigneeProvider Consignee => CachedValueHelper.GetValue(ref consignee, () => consignment.Consignee == null ? null : new CC043CConsigneeProvider(consignment.Consignee));
		CachedValue<CC043CConsigneeProvider> consignee;

		public IReadOnlyCollection<CC043CTransportEquipmentProvider> TransportEquipment => transportEquipmentCached ?? (transportEquipmentCached = consignment.TransportEquipment?.Select(x => new CC043CTransportEquipmentProvider(x)).ToArray() ?? Array.Empty<CC043CTransportEquipmentProvider>());
		IReadOnlyCollection<CC043CTransportEquipmentProvider> transportEquipmentCached;

		public IReadOnlyCollection<CC043CDepartureTransportMeansProvider> DepartureTransportMeans => departureTransportMeansCached ?? (departureTransportMeansCached = consignment.DepartureTransportMeans?.Select(x => new CC043CDepartureTransportMeansProvider(x)).ToArray() ?? Array.Empty<CC043CDepartureTransportMeansProvider>());
		IReadOnlyCollection<CC043CDepartureTransportMeansProvider> departureTransportMeansCached;

		public IReadOnlyCollection<CC043CSupportingDocumentProvider> SupportingDocuments => supportingDocumentsCached ?? (supportingDocumentsCached = consignment.SupportingDocument?.Select(x => new CC043CSupportingDocumentProvider(x)).ToArray() ?? Array.Empty<CC043CSupportingDocumentProvider>());
		IReadOnlyCollection<CC043CSupportingDocumentProvider> supportingDocumentsCached;

		public IReadOnlyCollection<CC043CDocumentProvider> TransportDocuments => transportDocumentsCached ?? (transportDocumentsCached = consignment.TransportDocument?.Select(x => new CC043CDocumentProvider(x)).ToArray() ?? Array.Empty<CC043CDocumentProvider>());
		IReadOnlyCollection<CC043CDocumentProvider> transportDocumentsCached;

		public IReadOnlyCollection<CC043CDocumentProvider> AdditionalReferences => additionalReferencesCached ?? (additionalReferencesCached = consignment.AdditionalReference?.Select(x => new CC043CDocumentProvider(x)).ToArray() ?? Array.Empty<CC043CDocumentProvider>());
		IReadOnlyCollection<CC043CDocumentProvider> additionalReferencesCached;

		public IReadOnlyCollection<CC043CIncidentProvider> Incidents => incidentsCached ?? (incidentsCached = consignment.Incident?.Select(x => new CC043CIncidentProvider(x)).ToArray() ?? Array.Empty<CC043CIncidentProvider>());
		IReadOnlyCollection<CC043CIncidentProvider> incidentsCached;

		public ZString InlandModeOfTransport => CachedValueHelper.GetValue(ref inlandModeOfTransport, () => consignment.InlandModeOfTransport);
		CachedValue<ZString> inlandModeOfTransport;

		public IReadOnlyCollection<CC043AdditionalInformationProvider> AdditionalInformation => additionalInformation ?? (additionalInformation = consignment.AdditionalInformation.Select(x => new CC043AdditionalInformationProvider(x)).ToArray());
		IReadOnlyCollection<CC043AdditionalInformationProvider> additionalInformation;

		public IReadOnlyCollection<CC043CPreviousDocumentProvider> PreviousDocument => previousDocument ?? (previousDocument = consignment.PreviousDocument.Select(x => new CC043CPreviousDocumentProvider(x)).ToArray());
		IReadOnlyCollection<CC043CPreviousDocumentProvider> previousDocument;

		public IReadOnlyCollection<CC043CHouseConsignmentProvider> HouseConsignment => houseConsignment ?? (houseConsignment = consignment.HouseConsignment.Select(c => new CC043CHouseConsignmentProvider(c)).ToArray());
		IReadOnlyCollection<CC043CHouseConsignmentProvider> houseConsignment;
	}
}
