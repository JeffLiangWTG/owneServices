using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE007ConsignmentProvider : IIE007Consignment
	{
		public IE007ConsignmentProvider(NctsHeader nctsHeader)
		{
			header = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}
		readonly NctsHeader header;

		public ILocationOfGoods LocationOfGoods => locationOfGoods ?? (locationOfGoods = new LocationOfGoodsProvider(header.ArrivalMovementHeader));
		ILocationOfGoods locationOfGoods;

		public IReadOnlyCollection<IIE007Incident> Incidents => incidents ?? (incidents = header.EnRouteIncidents.Select(i => new IE007IncidentProvider(i)).ToArray());
		IReadOnlyCollection<IIE007Incident> incidents;
	}
}
