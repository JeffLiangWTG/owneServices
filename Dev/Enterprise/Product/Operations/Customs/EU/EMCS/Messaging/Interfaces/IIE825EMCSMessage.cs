using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This interface will be used in the future.")]
	public interface IIE825EMCSMessage : IEMCSMessage
	{
		ISplittingEad SplittingEad { get; set; }

		ISplitDetailsEad SplitDetailsEad { get; set; }

		IMsaOfSplitting MsaOfSplitting { get; set; }
	}

	public interface ISplittingEad
	{
		ZString UpstreamArc { get; set; }
	}

	public interface ISplitDetailsEad
	{
		ZString LocalReferenceNumber { get; set; }

		ZString JourneyTime { get; set; }

		ZString ChangedTransportArrangement { get; set; }

		IIE825DestinationChanged DestinationChanged { get; set; }

		IConsigneeTrader NewConsigneeTrader { get; set; }

		IDeliveryPlaceTrader DeliveryPlaceTrader { get; set; }

		IOffice DeliveryPlaceCustomsOffice { get; set; }

		ITransportTrader NewTransportArrangerTrader { get; set; }

		ITransportTrader NewTransporterTrader { get; set; }

		ITransportDetails TransportDetails { get; set; }

		IBodyEad BodyEad { get; set; }
	}

	public interface IIE825DestinationChanged
	{
		ZString DestinationTypeCode { get; set; }
	}

	public interface IMsaOfSplitting
	{
		ZString MemberStateCode { get; set; }
	}
}
