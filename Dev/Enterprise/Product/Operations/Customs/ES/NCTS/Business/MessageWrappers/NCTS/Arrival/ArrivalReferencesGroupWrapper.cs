using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalReferencesGroupWrapper : IArrivalReferencesGroup
	{
		public ArrivalReferencesGroupWrapper(NctsHeader header)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
		}
		readonly NctsHeader nctsHeader;

		public ZString TransitNumber => nctsHeader.ArrivalMrnFromUser;

		public ZString PreviousSummaryDeclarationNumber => nctsHeader.ESNctsHeader.CEN_PreviousSummaryDeclaration;

		public IReadOnlyCollection<IArrivalRouteEvent> RouteEvents
		{
			get
			{
				if (routeEvents == null)
				{
					var routeEventsList = new List<IArrivalRouteEvent>();

					routeEventsList.AddRange(nctsHeader.EnRouteIncidents.Select(incident => new ArrivalRouteIncidentEventWrapper(incident)));

					routeEventsList.AddRange(nctsHeader.EnRouteSeals.Select(seal => new ArrivalRouteSealEventWrapper(seal)));

					routeEventsList.AddRange(nctsHeader.EnRouteTransshipments.Select(transshipment => new ArrivalRouteTransshipmentEventWrapper(transshipment)));

					routeEvents = routeEventsList.AsReadOnly();
				}
				return routeEvents;
			}
		}
		IReadOnlyCollection<IArrivalRouteEvent> routeEvents;
	}
}
