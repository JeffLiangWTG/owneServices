using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class IE170HouseConsignmentProvider : IIE170HouseConsignment
	{
		public IE170HouseConsignmentProvider(NctsDepartureMovementHeader movementHeader)
		{
			MovementHeader = Argument.NotNull(movementHeader, nameof(MovementHeader));
		}
		public readonly NctsDepartureMovementHeader MovementHeader;

		public IReadOnlyCollection<ITransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = DepartureTransportMeansProvider.GetTransportMeans(MovementHeader));
		IReadOnlyCollection<ITransportMeans> departureTransportMeans;

		public IReadOnlyCollection<IIE170HouseConsignment> AsReadOnlyCollection() => new List<IIE170HouseConsignment> { this }.AsReadOnly();
	}
}
