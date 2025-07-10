using CargoWise.Common;
using CargoWise.Customs.IL.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business.Message.MessageBuilder
{
	sealed class ILGPM130MessageBuilder : ILMessageBuilderBase, Integration.Customs.IL.IILGPMMessageBuilder
	{
		public ILGPM130MessageBuilder(IGatePassMovementProvider messageProvider, GatePassMovementDocDataSendingObject gatePassMovementDocDataSendingObject)
			: base(messageProvider?.BusinessObject)
		{
			this.messageProvider = Argument.NotNull(messageProvider, nameof(messageProvider));
			this.messageSendingObject = Argument.NotNull(gatePassMovementDocDataSendingObject, nameof(gatePassMovementDocDataSendingObject));
			this.gatePassMovementDocDataObject = Argument.NotNull(messageSendingObject.GatePassMovementDocDataObject, nameof(messageSendingObject.GatePassMovementDocDataObject));
		}

		protected override string GetMessageText()
		{
			var gatePassMovementMessageBuilder = new GatePassMovementMessageBuilder(MessageGatePassMovementWrapper.NewOrNull(gatePassMovementDocDataObject, messageSendingObject.IsCancelActionTypeCode)) as CargoWise.Customs.Shared.MessageContracts.IXmlMessageBuilder;
			return gatePassMovementMessageBuilder?.GenerateXmlMessage().GetSerializedString() ?? ZString.Empty;
		}

		protected override ILEDIMessage GetMessage() => messageProvider.Factory.New<ILGPM130RequestMessage>();

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => messageProvider.Messages;

		readonly GatePassMovementDocDataSendingObject messageSendingObject;
		readonly GatePassMovementDocDataObject gatePassMovementDocDataObject;
		readonly IGatePassMovementProvider messageProvider;
	}
}
