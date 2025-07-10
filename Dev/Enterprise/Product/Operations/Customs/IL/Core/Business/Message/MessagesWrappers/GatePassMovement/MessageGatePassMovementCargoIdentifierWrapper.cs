using CargoWise.Customs.IL.MessageDefinitions.GPM;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business
{
	sealed class MessageGatePassMovementCargoIdentifierWrapper : IMessageGatePassMovementCargoIdentifier
	{
		MessageGatePassMovementCargoIdentifierWrapper(GatePassMovementDocDataObject gatePassMovement)
		{
			this.gatePassMovement = gatePassMovement;
		}

		internal static IMessageGatePassMovementCargoIdentifier NewOrNull(GatePassMovementDocDataObject gatePassMovement)
			=> gatePassMovement != null ? new MessageGatePassMovementCargoIdentifierWrapper(gatePassMovement) : null;

		public string CargoIdentifierKey1 => gatePassMovement.CargoIdentifierKey1;

		public string CargoIdentifierKey2 => gatePassMovement.CargoIdentifierKey2;

		public int CargoIdentifierType => int.TryParse(gatePassMovement.CargoIdentifierType.Code, out var number) ? number : ZInt.Zero;

		public string CargoIdentifierKey3 => null;

		readonly GatePassMovementDocDataObject gatePassMovement;
	}
}
