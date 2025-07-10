using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.GPM;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business
{
	public class MessageGatePassMovementWrapper : IMessageGatePassMovement
	{
		public MessageGatePassMovementWrapper(GatePassMovementDocDataObject gatePassMovement, bool isCancelActionTypeCode)
		{
			this.gatePassMovement = gatePassMovement;
			this.isCancelActionTypeCode = isCancelActionTypeCode;
		}

		internal static MessageGatePassMovementWrapper NewOrNull(GatePassMovementDocDataObject gatePassMovement, bool isCancelActionTypeCode)
			=> gatePassMovement != null ? new MessageGatePassMovementWrapper(gatePassMovement, isCancelActionTypeCode) : null;

		ICollection<IMessageGatePassMovementDetails> IMessageGatePassMovement.GatepassRequestMessageDetailsList => new Collection<IMessageGatePassMovementDetails>() { MessageGatePassMovementDetailsWrapper.NewOrNull(gatePassMovement, isCancelActionTypeCode) };

		IRequestContentHeader IMessageGatePassMovement.RequestContentHeader => RequestContentHeaderWrapper.New();

		readonly GatePassMovementDocDataObject gatePassMovement;
		readonly bool isCancelActionTypeCode;
	}
}
