using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalNCTS5TranshipmentWrapper : IArrivalNCTSTranshipment
	{
		public ArrivalNCTS5TranshipmentWrapper(EnRouteIncident enRouteIncident)
		{
			this.enRouteIncident = Argument.NotNull(enRouteIncident, nameof(enRouteIncident));
		}
		readonly EnRouteIncident enRouteIncident;

		public ZBool ContainerIndicator
		{
			get
			{
				var containers = enRouteIncident.IncidentContainers;
				return !containers.IsNullOrEmpty() && containers.All(x => x.BC_Mode == Core.Constants.ContainerModes.Containerised);
			}
		}

		public ITransportMediumInfoCommon TransportMeans => transportMeans ?? (transportMeans = new TransportMediumInfoCommonWrapper(enRouteIncident.BN_TransportAtDepartureType, enRouteIncident.BN_TransportAtDepartureID, enRouteIncident.BN_RN_NKTransportAtDepartureIDNationality));
		ITransportMediumInfoCommon transportMeans;
	}
}
