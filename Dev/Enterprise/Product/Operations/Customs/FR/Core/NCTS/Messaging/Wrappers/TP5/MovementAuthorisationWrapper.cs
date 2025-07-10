using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class MovementAuthorisationWrapper : IAuthorisation
	{
		MovementAuthorisationWrapper(NctsArrivalMovementHeader movementHeader)
		{
			this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		}
		readonly NctsArrivalMovementHeader movementHeader;

		public static MovementAuthorisationWrapper New(NctsArrivalMovementHeader movementHeader) => movementHeader == null ? null : new MovementAuthorisationWrapper(movementHeader);

		public string ReferenceNumber => referenceNumber ?? (referenceNumber = movementHeader.AuthorizationNumber);
		string referenceNumber;

		public string Type => type ?? (type = EURefCusMapper.MapCW1AuthorisationCodeToCustomsCode(movementHeader.Factory, movementHeader.AuthorizationCode));
		string type;
	}
}
