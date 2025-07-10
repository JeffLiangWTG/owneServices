using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IMovementReferenceNumberProvider
{
	ZString MovementReferenceNumber { get; }
}
