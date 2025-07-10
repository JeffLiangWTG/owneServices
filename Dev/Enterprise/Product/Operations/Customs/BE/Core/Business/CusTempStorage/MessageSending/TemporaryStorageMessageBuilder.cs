using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.Business.CusTempStorage;

public class TemporaryStorageMessageBuilder : EU.Business.CusTempStorage.TemporaryStorageMessageBuilder
{
	public TemporaryStorageMessageBuilder(TemporaryStorageMessageSendingObject messageSendingObject, TemporaryStorageMessageFunction messageFunction) : base(messageSendingObject, messageFunction)
	{
	}

	protected override ZString GetApplicationCode() => EDIInterchange.ApplicationCodes.BECustoms;

	protected override IMessageNumberStrategy GetMessageNumberStrategy(Common.MessageBuilders.BuilderResult builderResult) => new BEPNTSMessageNumberStrategy(builderResult.Message.Factory, EDIInterchange.ApplicationCodes.BECustoms);

	protected override ZString GetMessageText(ErrorCollector errorCollector)
	{
		var messageBuilder = GetMessageBuilder(MessageFunction.MessageType);
		if (messageBuilder != null)
		{
			return messageBuilder.GenerateXmlMessage().GetSerializedString();
		}
		else
		{
			return ZString.Empty;
		}
	}

	IXmlMessageBuilder GetMessageBuilder(ZString messageType)
	{
		switch (messageType)
		{
			case PNTSEntryTypeList.Codes.CombinedTemporaryStorage:
				var provider = new IETS115DataProvider((TemporaryStorageMessageSendingObject)MessageSendingObject);
				return new IETS115MessageBuilder(provider);
			case PNTSEntryTypeList.Codes.DeconsolidationNotification:
				var provider215 = new IETS215DataProvider((TemporaryStorageMessageSendingObject)MessageSendingObject);
				return new IETS215MessageBuilder(provider215);
			default:
				return null;
		}
	}
}
