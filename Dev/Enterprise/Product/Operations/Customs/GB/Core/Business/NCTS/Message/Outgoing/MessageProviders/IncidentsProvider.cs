using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class IncidentsProvider : IIncident
	{
		public IncidentsProvider(EnRouteIncident incident, int sequenceNumber)
		{
			this.incident = CargoWise.Common.Argument.NotNull(incident, nameof(incident));
			SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; }

		public string Code => incident.BN_IncidentCode;

		public string Text => incident.BN_Information;

		public IEndorsement Endorsement => endorsment ?? (endorsment = new EndorsmentProvider(incident));
		IEndorsement endorsment;

		public ILocationType Location => location ?? (location = new LocationProvider(incident));
		ILocationType location;

		public IReadOnlyCollection<ITransportEquipment> TransportEquipments => Code.In(IncidentCodeList.Codes._2, IncidentCodeList.Codes._3, IncidentCodeList.Codes._4, IncidentCodeList.Codes._6)
			? transportEquipments ?? (transportEquipments = incident.IncidentContainers.Cast<NctsContainer>().Select((ctr, index) => new TransportEquipmentsForNCTSContainerProvider(ctr, index + 1)).ToArray())
			: new Collection<ITransportEquipment>();
		IReadOnlyCollection<ITransportEquipment> transportEquipments;

		public ITranshipment Transhipment => Code.In(IncidentCodeList.Codes._3, IncidentCodeList.Codes._4)
			? transhipment ?? (transhipment = new TranshipmentProvider(incident))
			: null;
		ITranshipment transhipment;

		readonly EnRouteIncident incident;
	}
}
