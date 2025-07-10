using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NCTSEventProvider : INCTSEvent
	{
		public static NCTSEventProvider New(EnRouteIncident incident)
		{
			Argument.NotNull(incident, nameof(incident));
			return new NCTSEventProvider(incident.BN_EventPlace, incident.BN_EventCountryCode, incident, null, null);
		}

		public static NCTSEventProvider New(EnRouteTransshipment transshipment)
		{
			Argument.NotNull(transshipment, nameof(transshipment));
			return new NCTSEventProvider(transshipment.BN_EventPlace, transshipment.BN_EventCountryCode, null, transshipment, null);
		}

		public static NCTSEventProvider New(EnRouteSeal seal)
		{
			Argument.NotNull(seal, nameof(seal));
			return new NCTSEventProvider(seal.BN_EventPlace, seal.BN_EventCountryCode, null, null, seal);
		}

		NCTSEventProvider(ZString place, ZString country, EnRouteIncident incident, EnRouteTransshipment transshipment, EnRouteSeal seal)
		{
			Place = place.ValueOrNullIfEmpty();
			Country = country.ValueOrNullIfEmpty();
			Incident = NCTSIncidentBaseProvider.NewOrNull(incident);
			Transhipment = NCTSTranshipmentProvider.NewOrNull(transshipment);
			Seals = NCTSEventSealsProvider.NewOrNull(seal);
		}

		public string Place { get; }

		public string Country { get; }

		public INCTSIncidentBase Incident { get; }

		public INCTSTranshipment Transhipment { get; }

		public INCTSSeals Seals { get; }
	}
}
