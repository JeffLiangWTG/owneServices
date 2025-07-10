using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class ConsignmentType01Provider : IConsignmentType01
	{
		public ConsignmentType01Provider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		public IReadOnlyCollection<IIncident> Incidents => incidents ?? (incidents = nctsHeader.EnRouteIncidents.Select((inc, index) => new IncidentsProvider(inc, index + 1)).ToArray());
		IReadOnlyCollection<IIncident> incidents;

		public ILocationOfGoods LocationOfGoods => locationOfGoods ?? (locationOfGoods = new LocationOfGoodsProvider(nctsHeader));
		ILocationOfGoods locationOfGoods;

		readonly NctsHeader nctsHeader;
	}
}
