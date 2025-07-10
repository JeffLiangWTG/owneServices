using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface IEnRouteEvent
	{
		ZString EventPlace { get; }

		ZString EventPlaceLanguage { get; }

		ZString EventCountryCode { get; }

		ZBool ControlAlreadyInNcts { get; }

		IEnRouteIncident EnRouteIncident { get; }

		IEnRouteEventSeal EnRouteEventSeal { get; }

		IEnRouteTranshipment EnRouteTranshipment { get; }
	}
}
