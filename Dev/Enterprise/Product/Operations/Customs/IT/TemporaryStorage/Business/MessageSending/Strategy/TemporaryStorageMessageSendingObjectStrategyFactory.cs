using CargoWise.Common;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

static class TemporaryStorageMessageSendingObjectStrategyFactory
{
	public static ITemporaryStorageMessageSendingObjectStrategy CreateStrategy(TemporaryStorageMessageSendingObject messageSendingObject)
	{
		_ = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		var messageType = messageSendingObject.MessageType;
		return messageType.ToString() switch
		{
			EDIMessageTypeList.Codes.Amendment => new TemporaryStorageAmendmentMessageSendingObjectStrategy(messageSendingObject),
			EDIMessageTypeList.Codes.NewDeclaration => new TemporaryStorageNewDeclarationMessageSendingObjectStrategy(messageSendingObject),
			_ => new TemporaryStorageDefaultMessageSendingObjectStrategy(messageSendingObject),
		};
	}
}
