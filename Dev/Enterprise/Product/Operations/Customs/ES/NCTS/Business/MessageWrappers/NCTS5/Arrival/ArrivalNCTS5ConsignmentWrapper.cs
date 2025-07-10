using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalNCTS5ConsignmentWrapper : IArrivalNCTSConsigment
	{
		public ArrivalNCTS5ConsignmentWrapper(NctsHeader header)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
			arrivalMovementHeader = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(nctsHeader.ArrivalMovementHeader));
		}
		protected readonly NctsHeader nctsHeader;
		protected readonly NctsArrivalMovementHeader arrivalMovementHeader;

		public INCTSCommonLocationOfGoods LocationOfGoods => locationOfGoods ?? (locationOfGoods = new NCTS5CommonLocationOfGoodsWrapper(arrivalMovementHeader.GoodsLocation));
		INCTSCommonLocationOfGoods locationOfGoods;

		public IReadOnlyCollection<IArrivalNCTSIncident> Incident => incident ?? (incident = nctsHeader.IsInPhase5TransitionPeriod ? GetIncidentList(nctsHeader) : new List<ArrivalNCTS5IncidentWrapper>());
		IReadOnlyCollection<ArrivalNCTS5IncidentWrapper> incident;

		public IReadOnlyCollection<ArrivalNCTS5IncidentWrapper> GetIncidentList(NctsHeader header)
		{
			var allIncidents = header.EnRouteIncidents;

			var incidents = new List<ArrivalNCTS5IncidentWrapper>();
			ZShort seqNum = 1;
			foreach (var incident in allIncidents)
			{
				incidents.Add(new ArrivalNCTS5IncidentWrapper(incident, seqNum));
				seqNum++;
			}
			return incidents.AsReadOnly();
		}
	}
}
