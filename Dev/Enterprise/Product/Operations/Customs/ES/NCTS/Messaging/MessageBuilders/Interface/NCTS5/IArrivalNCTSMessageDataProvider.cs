using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface IArrivalNCTSMessageDataProvider : INCTSCommonDataProvider
	{
		IArrivalNCTSTransitOperation TransitOperation { get; }
		IReadOnlyCollection<INCTSCommonAuthorisation> Authorisations { get; }
		ZString CustomsOfficeOfDestinationActual { get; }
		IPartyIdProvider TraderAtDestination { get; }
		IPartyIdProvider RepresentativeAtDestination { get; }
		IArrivalNCTSIndicators Indicators { get; }
		IArrivalNCTSConsigment Consignment { get; }
	}

	public interface IArrivalNCTSTransitOperation : INCTSCommonTransitOperationMRN
	{
		ZBool SimplifiedProcedure { get; }
		ZBool IncidentFlag { get; }
	}

	public interface IArrivalNCTSIndicators
	{
		ZString GoodsDirectlyShipped { get; }
		ZString AutomaticCompletion { get; }
		ZString TIRPageCompletion { get; }
		ZString TIRParcialTotalUnloading { get; }
		ZString ReceptionSummary { get; }
		ZString SummaryTypeIndicator { get; }
		IReadOnlyCollection<IArrivalNCTS5PreviousG4> PreviousG4 { get; }
	}

	public interface IArrivalNCTS5PreviousG4
	{
		ZString SequenceNumber { get; }
		ZString PreviousG4MRN { get; }
	}

	public interface IArrivalNCTSConsigment
	{
		INCTSCommonLocationOfGoods LocationOfGoods { get; }
		IReadOnlyCollection<IArrivalNCTSIncident> Incident { get; }
	}

	public interface IArrivalNCTSIncident
	{
		ZString SequenceNumber { get; }
		ZString Code { get; }
		ZString Text { get; }
		IArrivalNCTSEndorsement Endorsement { get; }
		IArrivalNCTSLocation Location { get; }
		IReadOnlyCollection<INCTSCommonTransportEquipment> TransportEquipment { get; }
		IArrivalNCTSTranshipment Transhipment { get; }
	}

	public interface IArrivalNCTSEndorsement
	{
		ZDateTime Date { get; }
		ZString Authority { get; }
		ZString Place { get; }
		ZString Country { get; }
	}

	public interface IArrivalNCTSLocation
	{
		ZString Qualifier { get; }
		ZString UNLocode { get; }
		ZString Country { get; }
		IArrivalNCTSGNSS GNSS { get; }
		INCTSCommonAddress Address { get; }
	}

	public interface IArrivalNCTSGNSS
	{
		ZString Latitude { get; }
		ZString Longitude { get; }
	}

	public interface IArrivalNCTSTranshipment
	{
		ZBool ContainerIndicator { get; }
		ITransportMediumInfoCommon TransportMeans { get; }
	}
}
