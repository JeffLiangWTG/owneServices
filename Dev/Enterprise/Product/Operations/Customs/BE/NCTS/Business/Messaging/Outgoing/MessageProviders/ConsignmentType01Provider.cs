using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class ConsignmentType01Provider : IConsignmentType01
	{
		readonly NctsHeader nctsHeader;

		public ConsignmentType01Provider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		public IReadOnlyCollection<INCTSIncident> Incidents => incidents ?? (incidents = nctsHeader.EnRouteIncidents.Select((inc, index) => new IncidentsProvider(inc, index + 1)).ToArray());
		IReadOnlyCollection<INCTSIncident> incidents;

		public ILocationOfGoods LocationOfGoods => locationOfGoods ?? (locationOfGoods = new LocationOfGoodsProvider(nctsHeader));
		ILocationOfGoods locationOfGoods;
	}
}
