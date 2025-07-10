using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IETHeaderTransitCustomsOffice
{
	ZString ReferenceNumber { get; }
	ZDateTime EstimatedArrivalTime { get; }
}
