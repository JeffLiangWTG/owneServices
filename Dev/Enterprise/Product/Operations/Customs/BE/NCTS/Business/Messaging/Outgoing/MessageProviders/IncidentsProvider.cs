using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class IncidentsProvider : INCTSIncident
	{
		readonly EnRouteIncident incident;

		public IncidentsProvider(EnRouteIncident incident, ZInt sequenceNumber)
		{
			this.incident = Argument.NotNull(incident, nameof(incident));
			SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; }

		public string Code => incident.BN_IncidentCode;

		public string Text => incident.BN_Information;

		public IEndorsement Endorsement => endorsment ?? (endorsment = new EndorsmentProvider(incident));
		IEndorsement endorsment;

		public ILocationType Location => location ?? (location = new LocationProvider(incident));
		ILocationType location;

		public IReadOnlyCollection<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment> TransportEquipments => Code.In(IncidentCodeList.Codes._2, IncidentCodeList.Codes._3, IncidentCodeList.Codes._4, IncidentCodeList.Codes._6)
			? transportEquipments ?? (transportEquipments = incident.IncidentContainers.Cast<NctsContainer>().Select((ctr, index) => new TransportEquipmentsForNCTSContainerProvider(ctr, index + 1)).ToArray())
			: null;
		IReadOnlyCollection<CargoWise.Customs.BE.MessageContracts.Interfaces.INCTSTransportEquipment> transportEquipments;

		public ITranshipment Transhipment => Code.In(IncidentCodeList.Codes._3, IncidentCodeList.Codes._4)
			? transhipment ?? (transhipment = new TranshipmentProvider(incident))
			: null;
		ITranshipment transhipment;
	}
}
