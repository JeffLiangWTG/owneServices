using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalRouteIncidentEventWrapper : IArrivalRouteEvent
	{
		public ArrivalRouteIncidentEventWrapper(EnRouteIncident incident)
		{
			this.incident = Argument.NotNull(incident, nameof(incident));
		}
		readonly EnRouteIncident incident;

		public ZString EventPlace => incident.BN_EventPlace;

		public ZString EventPlaceLanguage => ZString.Empty;

		public ZString EventCountry => incident.BN_EventCountryCode;

		public ZBool IncidentInEvent => true;

		public IArrivalFormData IncidentFormData => incidentFromData ?? (incidentFromData = new ArrivalFormDataIncidentWrapper(incident));
		ArrivalFormDataIncidentWrapper incidentFromData;

		public ZString NewSealsInEventNum => ZString.Empty;

		public IReadOnlyCollection<IArrivalNewSealsInformation> NewSealsInformation => newSealsInformation ?? (newSealsInformation = Array.Empty<IArrivalNewSealsInformation>());
		IReadOnlyCollection<IArrivalNewSealsInformation> newSealsInformation;

		public ZString NewTransportNationality => ZString.Empty;

		public IArrivalFormData TransferFormData => null;

		public IReadOnlyCollection<ZString> NewContainerIDs => newContainerIDs ?? (newContainerIDs = Array.Empty<ZString>());
		IReadOnlyCollection<ZString> newContainerIDs;
	}
}
