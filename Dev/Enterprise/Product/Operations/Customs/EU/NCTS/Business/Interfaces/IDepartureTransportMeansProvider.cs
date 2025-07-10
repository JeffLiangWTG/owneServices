using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Interfaces
{
	public interface IDepartureTransportMeansProvider : IBusiness
	{
		ZString InlandTransportModeAtDeparture { get; }

		ZPropertyInfo InlandTransportModeAtDepartureInfo { get; }

		ZString VesselNameAtDeparture { get; }

		ZString VesselCountryAtDeparture { get; }

		IBusinessObjectCollection<IAdditionalWagonProvider> AdditionalWagons { get; }

		ZString TransportTypeAtDeparture { get; }

		ZPropertyInfo TransportTypeAtDepartureInfo { get; }

		ZString TransportAtDeparture { get; }

		ZPropertyInfo TransportAtDepartureInfo { get; }

		ZString TransportCountryAtDeparture { get; }

		ZString Trailer1IDAtDeparture { get; }

		ZString Trailer1NationalityAtDeparture { get; }

		ZString Trailer2IDAtDeparture { get; }

		ZString Trailer2NationalityAtDeparture { get; }

		ZString AircraftIDAtDeparture { get; }

		ZBool IsInPhase5TransitionPeriod { get; }
	}
}
