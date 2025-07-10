using System.Collections.Generic;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public interface IHouseConsignmentCustomsMessageWrapper
{
	int SequenceNumber { get; }

	IReadOnlyCollection<IPreviousDocument> PreviousDocuments { get; }

	IReadOnlyCollection<ISupportingDocument> SupportingDocuments { get; }

	IReadOnlyCollection<IAdditionalReference> AdditionalReferences { get; }

	IReadOnlyCollection<ITransportDocument> TransportDocuments { get; }

	string Ucr { get; }

	IReadOnlyCollection<IAdditionalInformation> AdditionalInformation { get; }

	ITrader Consignor { get; }

	ITrader Consignee { get; }

	IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors { get; }

	string TransportChargesMethodOfPayment { get; }

	string CountryOfDispatch { get; }

	decimal GrossMass { get; }

	IReadOnlyCollection<IMeansOfTransport> DepartureMeansOfTransports { get; }
}
