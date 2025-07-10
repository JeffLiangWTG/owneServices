using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface IHeaderEad
{
	ZString DurationTransportUnitMeasure { get; }

	ZBool SendFlagDeferred { get; }

	ZInt DestinationTypeCode { get; }

	ZInt JourneyTime { get; }

	ZInt TransportArrangement { get; }
}
