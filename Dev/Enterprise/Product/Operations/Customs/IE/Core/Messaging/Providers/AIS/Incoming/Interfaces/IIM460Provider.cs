using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging;

public interface IIM460Provider
{
	ZString MovementReferenceNumber { get; }
}
