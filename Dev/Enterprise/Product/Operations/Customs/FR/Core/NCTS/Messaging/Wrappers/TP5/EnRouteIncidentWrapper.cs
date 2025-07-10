using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class EnRouteIncidentWrapper : IIncident
	{
		EnRouteIncidentWrapper(EnRouteIncident incident)
		{
			this.incident = Argument.NotNull(incident, nameof(incident));
		}
		readonly EnRouteIncident incident;

		public static EnRouteIncidentWrapper New(EnRouteIncident incident) => incident == null ? null : new EnRouteIncidentWrapper(incident);

		public string Code => incident.BN_IncidentCode;

		public string Text => incident.BN_Information;

		public IEndorsement Endorsement => endorsment ?? (endorsment = EndorsementWrapper.New(incident));
		IEndorsement endorsment;

		public ILocation Location => location ?? (location = IncidentLocationWrapper.New(incident));
		ILocation location;

		public ITranshipment Transhipment => transhipment ?? (transhipment = TranshipmentWrapper.New(incident));
		ITranshipment transhipment;

		public ICollection<ITransportEquipment> TransportEquipment => transportEquipment ?? (transportEquipment = GetTransportEquipment());
		ICollection<ITransportEquipment> transportEquipment;

		ICollection<ITransportEquipment> GetTransportEquipment()
		{
			var result = new Collection<ITransportEquipment>();
			incident.IncidentContainers.Cast<NctsContainer>().ForEach(x => result.Add(EnRouteTransportEquipmentWrapper.New(x)));
			return result;
		}
	}
}
