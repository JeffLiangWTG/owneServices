using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public abstract class TemporaryStorageMessageBuilder : EU.Business.CusTempStorage.TemporaryStorageMessageBuilder
	{
		public TemporaryStorageMessageBuilder(TemporaryStorageMessageSendingObject messageSendingObject, EU.Business.CusTempStorage.TemporaryStorageMessageFunction messageFunction)
			: base(messageSendingObject, messageFunction)
		{
		}

		public new TemporaryStorageMessageSendingObject MessageSendingObject => (TemporaryStorageMessageSendingObject)base.MessageSendingObject;

		protected override ZString GetMessageType() => MessageFunction.MessageType;
		protected override ZString GetMessageSubType() => ZString.Empty;
		protected override ZGuid GetGlbExternalPassword() => Header.Branch.Company.GetCredentialPK();
		protected override IMessageNumberStrategy GetMessageNumberStrategy(Common.MessageBuilders.BuilderResult builderResult) => null;
		protected override void Message_Saving(Enterprise.Messaging.Business.EDIMessage message) { }
		protected override Enterprise.Messaging.Business.EDIMessage GetNewMessage() => Header.Messages.AddNew(typeof(AISOutboundEDIMessage));

		protected override ZString GetMessageText(EU.Business.ErrorCollector errorCollector)
		{
			var result = ZString.Empty;
			var messageBuilder = GetMessageBuilder(errorCollector);
			if (messageBuilder != null)
			{
				result = messageBuilder.GenerateXmlMessage().GetSerializedString();
			}
			return result;
		}

		protected abstract IXmlMessageBuilder GetMessageBuilder(EU.Business.ErrorCollector errorCollector);
	}
}
