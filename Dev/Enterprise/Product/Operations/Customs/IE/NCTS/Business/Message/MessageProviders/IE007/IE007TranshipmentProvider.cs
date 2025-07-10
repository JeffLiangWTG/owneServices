using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE007TranshipmentProvider : IIE007Transhipment
	{
		public IE007TranshipmentProvider(EnRouteIncident incident, bool hasContainer)
		{
			this.incident = incident;
			this.hasContainer = hasContainer;
		}
		readonly EnRouteIncident incident;
		readonly bool hasContainer;

		public bool HasContainer => hasContainer;

		public ITransportMeans TransportMeans => transportMeans ?? (transportMeans = DepartureTransportMeansProvider.GetTransportMean(incident));
		ITransportMeans transportMeans;
	}
}
