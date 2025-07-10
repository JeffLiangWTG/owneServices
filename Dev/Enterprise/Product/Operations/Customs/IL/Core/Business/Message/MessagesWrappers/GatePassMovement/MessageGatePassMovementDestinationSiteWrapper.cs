using CargoWise.Customs.IL.MessageDefinitions.GPM;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business
{
	sealed class MessageGatePassMovementDestinationSiteWrapper : IMessageGatePassMovementDestinationSite
	{
		MessageGatePassMovementDestinationSiteWrapper(GatePassMovementDocDataObject gatePassMovement)
		{
			this.gatePassMovement = gatePassMovement;
		}

		internal static IMessageGatePassMovementDestinationSite NewOrNull(GatePassMovementDocDataObject gatePassMovement)
			=> gatePassMovement != null ? new MessageGatePassMovementDestinationSiteWrapper(gatePassMovement) : null;

		public string DesignateSiteCode => gatePassMovement.DestinationSite.Code;

		public int TransportationTypeCode => int.TryParse(gatePassMovement.TransportMethod.Code, out var result) ? result : ZInt.Zero;

		public bool IsFinalDestination => true;

		public string ExportManifestNo => null;

		readonly GatePassMovementDocDataObject gatePassMovement;
	}
}
