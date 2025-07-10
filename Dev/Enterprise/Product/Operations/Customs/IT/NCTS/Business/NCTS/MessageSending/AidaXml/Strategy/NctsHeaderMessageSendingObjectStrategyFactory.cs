using System;
using CargoWise.Common;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

static class NctsHeaderMessageSendingObjectStrategyFactory
{
	public static INctsHeaderMessageSendingObjectStrategy CreateStrategy(NctsHeaderMessageSendingObject messageSendingObject)
	{
		_ = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		var messageType = messageSendingObject.MessageType;
		return messageType.ToString() switch
		{
			EDIMessageTypeList.Codes.Amendment => new NctsHeaderAmendmentMessageSendingObjectStrategy(messageSendingObject),
			EDIMessageTypeList.Codes.Cancellation => new NctsHeaderCancellationMessageSendingObjectStrategy(messageSendingObject),
			EDIMessageTypeList.Codes.NewDeclaration => new NctsHeaderNewDeclarationMessageSendingObjectStrategy(messageSendingObject),
			_ => throw new InvalidOperationException($"Invalid value for {nameof(messageType)}: {messageType.ToString()}"),
		};
	}
}
