using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalNCTS5IncidentWrapper : IArrivalNCTSIncident
	{
		public ArrivalNCTS5IncidentWrapper(EnRouteIncident enRouteIncident, ZShort seqNum)
		{
			this.enRouteIncident = Argument.NotNull(enRouteIncident, nameof(enRouteIncident));
			SequenceNumber = seqNum.ToString();
		}
		protected readonly EnRouteIncident enRouteIncident;

		public ZString SequenceNumber { get; }

		public ZString Code => enRouteIncident.BN_IncidentCode;

		public ZString Text => enRouteIncident.BN_Information;

		public IArrivalNCTSEndorsement Endorsement => endorsement ?? (endorsement = new ArrivalNCTS5EndorsementWrapper(enRouteIncident));
		IArrivalNCTSEndorsement endorsement;

		public IArrivalNCTSLocation Location => location ?? (location = new ArrivalNCTS5LocationWrapper(enRouteIncident));
		IArrivalNCTSLocation location;

		public IArrivalNCTSTranshipment Transhipment => transhipment ?? (transhipment = GetTranshipmentWrapper());

		ArrivalNCTS5TranshipmentWrapper GetTranshipmentWrapper() => enRouteIncident.BN_TransportAtDepartureType.IsEmpty ? null : new ArrivalNCTS5TranshipmentWrapper(enRouteIncident);

		IArrivalNCTSTranshipment transhipment;

		public IReadOnlyCollection<INCTSCommonTransportEquipment> TransportEquipment
		{
			get
			{
				if (transportEquipment == null)
				{
					var transportEquipmentList = new List<ArrivalNCTS5TransportEquipmentWrapper>();

					ZShort seqNum = 1;
					IEnumerable<NctsContainer> containerizedContainers;

					containerizedContainers = enRouteIncident.IncidentContainers.Cast<NctsContainer>().Where(x => x.BC_Mode == Core.Constants.ContainerModes.Containerised);
					foreach (var container in containerizedContainers)
					{
						transportEquipmentList.Add(new ArrivalNCTS5TransportEquipmentWrapper(container, seqNum));
						seqNum++;
					}

					var nonContainerizedContainers = enRouteIncident.IncidentContainers.Cast<NctsContainer>().Where(x => x.BC_Mode == Core.Constants.ContainerModes.NonContainerised);
					foreach (var container in nonContainerizedContainers)
					{
						transportEquipmentList.Add(new ArrivalNCTS5TransportEquipmentWrapper(container, seqNum));
						seqNum++;
					}

					transportEquipment = transportEquipmentList.AsReadOnly();
				}
				return transportEquipment;
			}
		}
		IReadOnlyCollection<ArrivalNCTS5TransportEquipmentWrapper> transportEquipment;
	}
}
