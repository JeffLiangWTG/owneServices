using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This interface will be used in the future.")]
	public interface IIE813EMCSMessage : IEMCSMessage
	{
		IIE813Attributes Attributes { get; set; }

		ITransportTrader NewTransportArrangerTrader { get; set; }

		IUpdateEad UpdateEad { get; set; }

		IIE813DestinationChanged DestinationChanged { get; set; }

		ITransportTrader NewTransporterTrader { get; set; }

		ITransportDetails TransportDetails { get; set; }
	}

	public interface IIE813Attributes
	{
		ZDateTime DateAndTimeOfValidationOfChangeOfDestination { get; set; }
	}

	public interface IUpdateEad
	{
		ZString AdministrativeReferenceCode { get; set; }

		ZString JourneyTime { get; set; }

		ZString ChangedTransportArrangement { get; set; }

		ZString SequenceNumber { get; set; }

		ZDate InvoiceDate { get; set; }

		ZString InvoiceNumber { get; set; }

		ZString TransportModeCode { get; set; }

		ZString ComplementaryInformation { get; set; }
	}

	public interface IIE813DestinationChanged
	{
		ZString DestinationTypeCode { get; set; }

		IConsigneeTrader NewConsigneeTrader { get; set; }

		IDeliveryPlaceTrader DeliveryPlaceTrader { get; set; }

		IOffice DeliveryPlaceCustomsOffice { get; set; }
	}
}
