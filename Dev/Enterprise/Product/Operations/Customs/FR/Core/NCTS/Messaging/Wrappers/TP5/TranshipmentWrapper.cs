using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class TranshipmentWrapper : ITranshipment
	{
		TranshipmentWrapper(EnRouteIncident enRouteIncident)
		{
			this.enRouteIncident = Argument.NotNull(enRouteIncident, nameof(enRouteIncident));
		}

		readonly EnRouteIncident enRouteIncident;

		public static TranshipmentWrapper New(EnRouteIncident enRouteIncident) => enRouteIncident == null ? null : new TranshipmentWrapper(enRouteIncident);

		public bool ContainerIndicator => enRouteIncident.IncidentContainers.Where(x => !x.BC_ContainerNum.IsEmpty).IsCountMoreThan(0);

		public ITransportMeans TransportMeans => transportMeans ?? (transportMeans = EnRouteIncidentTransportMeansWrapper.New(enRouteIncident));
		ITransportMeans transportMeans;
	}
}
