using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteEventWrapper : IEnRouteEvent
	{
		public EnRouteEventWrapper(EnRouteTransshipment enRouteEvent, EnRouteIncident enRouteIncident, EnRouteSeal enRouteSeal)
		{
			enRouteEventItem = enRouteEvent;
			enRouteIncidentItem = enRouteIncident;
			enRouteSealItem = enRouteSeal;

			if (enRouteIncidentItem != null)
			{
				EventPlace = enRouteIncidentItem.BN_EventPlace.IsEmpty ? enRouteIncidentItem.BN_EndorsementPlace : enRouteIncidentItem.BN_EventPlace;
				EventCountryCode = enRouteIncidentItem.BN_EventCountryCode;
			}
			else if (enRouteEventItem != null)
			{
				EventPlace = enRouteEventItem.BN_EventPlace.IsEmpty ? enRouteEventItem.BN_EndorsementPlace : enRouteEventItem.BN_EventPlace;
				EventCountryCode = enRouteEventItem.BN_EventCountryCode;
			}
			else if (enRouteSealItem != null)
			{
				EventPlace = enRouteSealItem.BN_EventPlace.IsEmpty ? enRouteSealItem.BN_EndorsementPlace : enRouteSealItem.BN_EventPlace;
				EventCountryCode = enRouteSealItem.BN_EventPlace;
			}
		}

		#region Members

		readonly EnRouteTransshipment enRouteEventItem;
		readonly EnRouteIncident enRouteIncidentItem;
		readonly EnRouteSeal enRouteSealItem;

		public ZString EventPlace { get; }

		public ZString EventPlaceLanguage => ZString.Empty;

		public ZString EventCountryCode { get; }

		public ZBool ControlAlreadyInNcts => true;

		public IEnRouteIncident EnRouteIncident => CachedValueHelper.GetValue(ref enRouteIncident, () => enRouteIncidentItem != null ? new EnRouteIncidentWrapper(enRouteIncidentItem) : null);
		CachedValue<IEnRouteIncident> enRouteIncident;

		public IEnRouteEventSeal EnRouteEventSeal => CachedValueHelper.GetValue(ref enRouteEventSeal, () => enRouteSealItem != null ? new EnRouteEventSealWrapper(enRouteSealItem) : null);
		CachedValue<IEnRouteEventSeal> enRouteEventSeal;

		public IEnRouteTranshipment EnRouteTranshipment => CachedValueHelper.GetValue(ref enRouteTranshipment, () => enRouteEventItem != null ? new EnRouteTransshipmentWrapper(enRouteEventItem) : null);
		CachedValue<IEnRouteTranshipment> enRouteTranshipment;

		#endregion
	}
}
