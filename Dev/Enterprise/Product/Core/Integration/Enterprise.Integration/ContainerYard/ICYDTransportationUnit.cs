using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ICYDTransportationUnit
	{
		ZGuid PK { get; }
		ZString YTU_TransportationReference { get; }

		ZDateTimeOffset YTU_EstimatedGateInTime { get; }

		IActiveBusinessObjectCollection<ICYDDelivery> GetDeliveries { get; }

		IActiveBusinessObjectCollection<ICYDPickup> GetPickups { get; }
	}
}
