using CargoWise.Common;
using CargoWise.Customs.IL.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Customs.IL.Business.Message.MessageBuilder
{
	sealed class ILDLO120MessageBuilder : ILMessageBuilderBase, Integration.Customs.IL.IILDLOMessageBuilder
	{
		public ILDLO120MessageBuilder(IDeliveryOrderProvider messageProvider, DeliveryOrderDocDataSendingObject deliveryOrderDocDataSendingObject)
			: base(messageProvider?.BusinessObject)
		{
			this.messageProvider = Argument.NotNull(messageProvider, nameof(messageProvider));
			this.messageSendingObject = Argument.NotNull(deliveryOrderDocDataSendingObject, nameof(deliveryOrderDocDataSendingObject));
			this.deliveryOrderDocDataObject = Argument.NotNull(messageSendingObject.DeliveryOrderDocDataObject, nameof(messageSendingObject.DeliveryOrderDocDataObject));
		}

		protected override string GetMessageText()
		{
			var deliveryOrderMessageBuilder = new DeliveryOrderMessageBuilder(MessageDeliveryOrderWrapper.NewOrNull(deliveryOrderDocDataObject, messageSendingObject.IsCancelActionTypeCode)) as CargoWise.Customs.Shared.MessageContracts.IXmlMessageBuilder;
			return deliveryOrderMessageBuilder?.GenerateXmlMessage().GetSerializedString() ?? ZString.Empty;
		}

		protected override ILEDIMessage GetMessage() => messageProvider.Factory.New<ILDLO120RequestMessage>();

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => messageProvider.Messages;

		readonly DeliveryOrderDocDataSendingObject messageSendingObject;
		readonly DeliveryOrderDocDataObject deliveryOrderDocDataObject;
		readonly IDeliveryOrderProvider messageProvider;
	}
}
